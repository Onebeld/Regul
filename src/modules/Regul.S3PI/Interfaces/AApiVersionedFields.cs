/***************************************************************************
 *  Copyright (C) 2009 by Peter L Jones                                    *
 *  pljones@users.sf.net                                                   *
 *                                                                         *
 *  This file is part of the Sims 3 Package Interface (s3pi)               *
 *                                                                         *
 *  s3pi is free software: you can redistribute it and/or modify           *
 *  it under the terms of the GNU General Public License as published by   *
 *  the Free Software Foundation, either version 3 of the License, or      *
 *  (at your option) any later version.                                    *
 *                                                                         *
 *  s3pi is distributed in the hope that it will be useful,                *
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of         *
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the          *
 *  GNU General Public License for more details.                           *
 *                                                                         *
 *  You should have received a copy of the GNU General Public License      *
 *  along with s3pi.  If not, see <http://www.gnu.org/licenses/>.          *
 ***************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Regul.S3PI.Interfaces
{
    /// <summary>
    /// API Objects should all descend from this Abstract class.
    /// It will provide versioning support -- when implemented.
    /// It provides ContentFields support
    /// </summary>
    public abstract class AApiVersionedFields : IContentFields
    {
        #region IContentFields Members
        /// <summary>
        /// The list of available field names on this API object
        /// </summary>
        public abstract List<string> ContentFields { get; } // This should be implemented with a call to GetContentFields(requestedApiVersion, this.GetType())
        /// <summary>
        /// A typed value on this object
        /// </summary>
        /// <param name="index">The name of the field (i.e. one of the values from ContentFields)</param>
        /// <returns>The typed value of the named field</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when an unknown index name is requested</exception>
        public virtual TypedValue this[string index]
        {
            get
            {
                string[] fields = index.Split('.');
                object result = this;
                Type t = GetType();
                for (int i = 0; i < fields.Length; i++)
                {
                    PropertyInfo p = t.GetProperty(fields[i]);
                    if (p == null)
                        throw new ArgumentOutOfRangeException(nameof(index),
                            "Unexpected value received in index: " + index);
                    t = p.PropertyType;
                    result = p.GetValue(result, null);
                }

                return new TypedValue(t, result, "X");
            }
            set
            {
                string[] fields = index.Split('.');
                object result = this;
                Type t = GetType();
                PropertyInfo p = null;
                for (int i = 0; i < fields.Length; i++)
                {
                    p = t.GetProperty(fields[i]);
                    if (p == null)
                        throw new ArgumentOutOfRangeException(nameof(index), "Unexpected value received in index: " + index);
                    if (i < fields.Length - 1)
                    {
                        t = p.PropertyType;
                        result = p.GetValue(result, null);
                    }
                }
                p.SetValue(result, value.Value, null);
            }
        }

        #endregion

        static readonly List<string> Banlist;
        static AApiVersionedFields()
        {
            Banlist = new List<string>();
            PropertyInfo[] array = typeof(AApiVersionedFields).GetProperties();
            for (int i = 0; i < array.Length; i++)
                Banlist.Add(array[i].Name);
        }

        /// <summary>
        /// Versioning is not currently implemented
        /// Return the list of fields for a given API Class
        /// </summary>
        /// <param name="t">The class type for which to get the fields</param>
        /// <returns>List of field names for the given API version</returns>
        public static List<string> GetContentFields(Type t)
        {
            List<string> fields = new List<string>();

            PropertyInfo[] ap = t.GetProperties();
            for (int i = 0; i < ap.Length; i++)
            {
                PropertyInfo m = ap[i];
                if (Banlist.Contains(m.Name)) continue;

                fields.Add(m.Name);
            }
            fields.Sort(new PriorityComparer(t));

            return fields;
        }

        /// <summary>
        /// Get the TGIBlock list for a Content Field.
        /// </summary>
        /// <param name="o">The object to query.</param>
        /// <param name="f">The property name under inspection.</param>
        /// <returns>The TGIBlock list for a Content Field, if present; otherwise <c>null</c>.</returns>
        public static DependentList<TGIBlock> GetTgiBlocks(AApiVersionedFields o, string f)
        {
            string tgiBlockListCf = TGIBlockListContentFieldAttribute.GetTgiBlockListContentField(o.GetType(), f);
            if (tgiBlockListCf != null)
                try
                {
                    return o[tgiBlockListCf].Value as DependentList<TGIBlock>;
                }
                catch { }
            return null;
        }

        /// <summary>
        /// Get the TGIBlock list for a Content Field.
        /// </summary>
        /// <param name="f">The property name under inspection.</param>
        /// <returns>The TGIBlock list for a Content Field, if present; otherwise <c>null</c>.</returns>
        public DependentList<TGIBlock> GetTgiBlocks(string f) { return GetTgiBlocks(this, f); }

        class PriorityComparer : IComparer<string>
        {
            readonly Type _t;
            public PriorityComparer(Type t) { _t = t; }
            public int Compare(string x, string y)
            {
                int res = ElementPriorityAttribute.GetPriority(_t, x).CompareTo(ElementPriorityAttribute.GetPriority(_t, y));
                if (res == 0) res = string.Compare(x, y, StringComparison.Ordinal);
                return res;
            }
        }

        static readonly List<string> ValueBuilderBanlist = new List<string>(new[] {
            "Value", "Stream", "AsBytes",
        });
        static readonly List<string> IDictionaryBanlist = new List<string>(new[] {
            "Keys", "Values", "Count", "IsReadOnly", "IsFixedSize", "IsSynchronized", "SyncRoot",
        });

        /// <summary>
        /// The fields ValueBuilder will return; used to eliminate those that should not be used.
        /// </summary>
        protected virtual List<string> ValueBuilderFields
        {
            get
            {
                List<string> fields = ContentFields;
                fields.RemoveAll(Banlist.Contains);
                fields.RemoveAll(ValueBuilderBanlist.Contains);
                if (typeof(System.Collections.IDictionary).IsAssignableFrom(GetType())) fields.RemoveAll(IDictionaryBanlist.Contains);
                return fields;
            }
        }

        /// <summary>
        /// Returns a string representing the value of the field (and any contained sub-fields)
        /// </summary>
        protected string ValueBuilder
        {
            get
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();

                List<string> fields = ValueBuilderFields;

                string headerFmt = "\n--- {0}: {1} (0x{2:X}) ---";

                for (int i1 = 0; i1 < fields.Count; i1++)
                {
                    string f = fields[i1];
                    TypedValue tv = this[f];

                    if (typeof(AApiVersionedFields).IsAssignableFrom(tv.Type))
                    {
                        AApiVersionedFields apiObj = (AApiVersionedFields)tv.Value;
                        if (apiObj.ContentFields.Contains("Value") &&
                            typeof(string).IsAssignableFrom(GetContentFieldTypes(tv.Type)["Value"]))
                        {
                            string elem = (string)apiObj["Value"].Value;
                            if (elem.Contains("\n"))
                                sb.Append("\n--- " + tv.Type.Name + ": " + f + " ---\n   " + elem.Replace("\n", "\n   ").TrimEnd() + "\n---");
                            else
                                sb.Append("\n" + f + ": " + elem);
                        }
                    }
                    else if (tv.Type.BaseType != null && tv.Type.BaseType.Name.Contains("IndexList`"))
                    {
                        System.Collections.IList l = (System.Collections.IList)tv.Value;
                        string fmt = "\n   [{0:X" + l.Count.ToString("X").Length + "}]: {1}";
                        int i = 0;

                        sb.Append(string.Format(headerFmt, tv.Type.Name, f, l.Count));
                        for (int i2 = 0; i2 < l.Count; i2++)
                            sb.Append(string.Format(fmt, i++, (string)((AHandlerElement)l[i2])["Value"].Value));

                        sb.Append("\n---");
                    }
                    else if (tv.Type.BaseType != null && tv.Type.BaseType.Name.Contains("SimpleList`"))
                    {
                        System.Collections.IList l = (System.Collections.IList)tv.Value;
                        string fmt = "\n   [{0:X" + l.Count.ToString("X").Length + "}]: {1}";
                        int i = 0;

                        sb.Append(string.Format(headerFmt, tv.Type.Name, f, l.Count));
                        for (int i2 = 0; i2 < l.Count; i2++)
                            sb.Append(string.Format(fmt, i++, ((AHandlerElement)l[i2])["Val"]));

                        sb.Append("\n---");
                    }
                    else if (typeof(DependentList<TGIBlock>).IsAssignableFrom(tv.Type))
                    {
                        DependentList<TGIBlock> l = (DependentList<TGIBlock>)tv.Value;
                        string fmt = "\n   [{0:X" + l.Count.ToString("X").Length + "}]: {1}";
                        int i = 0;

                        sb.Append(string.Format(headerFmt, tv.Type.Name, f, l.Count));
                        for (int i2 = 0; i2 < l.Count; i2++)
                            sb.Append(string.Format(fmt, i++, l[i2]));

                        sb.Append("\n---");
                    }
                    else if (tv.Type.BaseType != null && tv.Type.BaseType.Name.Contains("DependentList`"))
                    {
                        System.Collections.IList l = (System.Collections.IList)tv.Value;
                        string fmtLong = "\n--- {0}[{1:X" + l.Count.ToString("X").Length + "}] ---\n   ";
                        string fmtShort = "\n   [{0:X" + l.Count.ToString("X").Length + "}]: {1}";
                        int i = 0;

                        sb.Append(string.Format(headerFmt, tv.Type.Name, f, l.Count));
                        for (int i2 = 0; i2 < l.Count; i2++)
                        {
                            AHandlerElement v = (AHandlerElement)l[i2];
                            if (v.ContentFields.Contains("Value") &&
                                typeof(string).IsAssignableFrom(GetContentFieldTypes(v.GetType())["Value"]))
                            {
                                string elem = (string)v["Value"].Value;
                                if (elem.Contains("\n"))
                                    sb.Append(string.Format(fmtLong, f, i++) + elem.Replace("\n", "\n   ").TrimEnd());
                                else
                                    sb.Append(string.Format(fmtShort, i++, elem));
                            }
                        }
                        sb.Append("\n---");
                    }
                    else if (tv.Type.HasElementType && typeof(AApiVersionedFields).IsAssignableFrom(tv.Type.GetElementType())) // it's an AApiVersionedFields array, slightly glossy...
                    {
                        sb.Append(string.Format(headerFmt, tv.Type.Name, f, ((Array)tv.Value).Length));
                        sb.Append("\n   " + tv.ToString().Replace("\n", "\n   ").TrimEnd() + "\n---");
                    }
                    else
                    {
                        string suffix = "";
                        DependentList<TGIBlock> tgis = GetTgiBlocks(f);
                        if (tgis != null && tgis.Count > 0)
                            try
                            {
                                int i = Convert.ToInt32(tv.Value);
                                if (i >= 0 && i < tgis.Count)
                                    suffix = " (" + tgis[i] + ")";
                            }
                            catch (Exception e)
                            {
                                sb.Append(" (Exception: " + e.Message + ")");
                            }
                        sb.Append("\n" + f + ": " + tv + suffix);
                    }
                }

                if (typeof(System.Collections.IDictionary).IsAssignableFrom(GetType()))
                {
                    System.Collections.IDictionary l = (System.Collections.IDictionary) this;
                    string fmt = "\n   [{0:X" + l.Count.ToString("X").Length + "}] {1}: {2}";
                    int i = 0;
                    sb.Append("\n--- (0x" + l.Count.ToString("X") + ") ---");
                    foreach (object key in l.Keys)
                        sb.Append(string.Format(fmt, i++,
                            new TypedValue(key.GetType(), key, "X"),
                            new TypedValue(l[key].GetType(), l[key], "X")));
                    sb.Append("\n---");
                }

                return sb.ToString().Trim('\n');
            }
        }

        /// <summary>
        /// Returns a <see cref="string"/> that represents the current <see cref="AApiVersionedFields"/> object.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents the current <see cref="AApiVersionedFields"/> object.</returns>
        public override string ToString() => ValueBuilder;

        /// <summary>
        /// Sorts Content Field names by their <see cref="ElementPriorityAttribute"/> (if set)
        /// </summary>
        /// <param name="x">First content field name</param>
        /// <param name="y">Second content field name</param>
        /// <returns>A signed number indicating the relative values of this instance and value.</returns>
        public int CompareByPriority(string x, string y) => new PriorityComparer(GetType()).Compare(x, y);

        /// <summary>
        /// Gets a lookup table from fieldname to type.
        /// </summary>
        /// <param name="t">API data type to query</param>
        /// <returns></returns>
        public static Dictionary<string, Type> GetContentFieldTypes(Type t)
        {
            Dictionary<string, Type> types = new Dictionary<string, Type>();

            PropertyInfo[] ap = t.GetProperties();
            for (int i = 0; i < ap.Length; i++)
            {
                PropertyInfo m = ap[i];
                if (Banlist.Contains(m.Name)) continue;

                types.Add(m.Name, m.PropertyType);
            }

            return types;
        }

#if UNDEF
        protected static List<string> getMethods(Int32 APIversion, Type t)
        {
            List<string> methods = null;

            Int32 recommendedApiVersion = getRecommendedApiVersion(t);//Could be zero if no "recommendedApiVersion" const field
            methods = new List<string>();
            MethodInfo[] am = t.GetMethods();
            foreach (MethodInfo m in am)
            {
                if (!m.IsPublic || banlist.Contains(m.Name)) continue;
                if ((m.Name.StartsWith("get_") && m.GetParameters().Length == 0)) continue;
                if (!checkVersion(t, m.Name, APIversion == 0 ? recommendedApiVersion : APIversion)) continue;

                methods.Add(m.Name);
            }

            return methods;
        }

        public List<string> Methods { get; }
        
        public TypedValue Invoke(string method, params TypedValue[] parms)
        {
            Type[] at = new Type[parms.Length];
            object[] ao = new object[parms.Length];
            for (int i = 0; i < parms.Length; i++) { at[i] = parms[i].Type; ao[i] = parms[i].Value; }//Array.Copy, please...

            MethodInfo m = this.GetType().GetMethod(method, at);
            if (m == null)
                throw new ArgumentOutOfRangeException("Unexpected method received: " + method + "(...)");

            return new TypedValue(m.ReturnType, m.Invoke(this, ao), "X");
        }
#endif

        /// <summary>
        /// A class enabling sorting API objects by a ContentFields name
        /// </summary>
        /// <typeparam name="T">API object type</typeparam>
        public class Comparer<T> : IComparer<T>
            where T : IContentFields
        {
            readonly string _field;
            /// <summary>
            /// Sort API Objects by <paramref name="field"/>
            /// </summary>
            /// <param name="field">ContentField name to sort by</param>
            public Comparer(string field) { _field = field; }

            #region IComparer<T> Members

            /// <summary>
            /// Compares two objects of type T and returns a value indicating whether one is less than,
            /// equal to, or greater than the other.
            /// </summary>
            /// <param name="x">The first IContentFields object to compare.</param>
            /// <param name="y">The second IContentFields object to compare.</param>
            /// <returns>Value Condition Less than zero -- x is less than y.
            /// Zero -- x equals y.
            /// Greater than zero -- x is greater than y.</returns>
            public int Compare(T x, T y) => x[_field].CompareTo(y[_field]);

            #endregion

        }

        // Random helper functions that should live somewhere...

        /// <summary>
        /// Convert a string (up to 8 characters) to a UInt64
        /// </summary>
        /// <param name="s">String to convert</param>
        /// <returns>UInt64 packed representation of <paramref name="s"/></returns>
        public static ulong FOURCC(string s)
        {
            if (s.Length > 8) throw new ArgumentLengthException("String", 8);
            ulong i = 0;
            for (int j = s.Length - 1; j >= 0; j--) i += ((uint)s[j]) << (j * 8);
            return i;
        }

        /// <summary>
        /// Convert a UInt64 to a string (up to 8 characters, high-order zeros omitted)
        /// </summary>
        /// <param name="i">Bytes to convert</param>
        /// <returns>String representation of <paramref name="i"/></returns>
        public static string FOURCC(ulong i)
        {
            string s = "";
            for (int j = 7; j >= 0; j--) { char c = (char)((i >> (j * 8)) & 0xff); if (s.Length > 0 || c != 0) s = c + s; }
            return s;
        }

        /// <summary>
        /// Return a space-separated string containing valid enumeration names for the given type
        /// </summary>
        /// <param name="t">Enum type</param>
        /// <returns>Valid enum names</returns>
        public static string FlagNames(Type t) { string p = "";
            for (int index = 0; index < Enum.GetNames(t).Length; index++) p += " " + Enum.GetNames(t)[index];

            return p.Trim(); }
    }

    /// <summary>
    /// A useful extension to <see cref="AApiVersionedFields"/> where a change handler is required
    /// </summary>
    public abstract class AHandlerElement : AApiVersionedFields
    {
        /// <summary>
        /// Holds the <see cref="EventHandler"/> delegate to invoke if the <see cref="AHandlerElement"/> changes.
        /// </summary>
        protected EventHandler handler;

        /// <summary>
        /// Indicates if the <see cref="AHandlerElement"/> has been changed by OnElementChanged()
        /// </summary>
        protected bool dirty;

        /// <summary>
        /// Initialize a new instance
        /// </summary>
        /// <param name="handler">The <see cref="EventHandler"/> delegate to invoke if the <see cref="AHandlerElement"/> changes.</param>
        public AHandlerElement(EventHandler handler) { this.handler = handler; }
        /// <summary>
        /// Get a copy of the <see cref="AHandlerElement"/> but with a new change <see cref="EventHandler"/>.
        /// </summary>
        /// <param name="handler">The replacement <see cref="EventHandler"/> delegate.</param>
        /// <returns>Return a copy of the <see cref="AHandlerElement"/> but with a new change <see cref="EventHandler"/>.</returns>
        public virtual AHandlerElement Clone(EventHandler handler)
        {
            List<object> args = new List<object>(new object[] { handler, this, });

            // Default values for parameters are resolved by the compiler.
            // Activator.CreateInstance does not simulate this, so we have to do it.
            // Avoid writing a Binder class just for this...
            ConstructorInfo ci = GetType().GetConstructors()
                .Where(c =>
                {
                    ParameterInfo[] pi = c.GetParameters();

                    // Our required arguments followed by one or more optional ones
                    if (pi.Length <= args.Count) return false;
                    if (pi[args.Count - 1].IsOptional) return false;
                    if (!pi[args.Count].IsOptional) return false;

                    // Do the required args match?
                    for (int i = 0; i < args.Count; i++)
                    {
                        // null matches anything except a value type
                        if (args[i] == null)
                        {
                            if (pi[i].ParameterType.IsValueType) return false;
                        }
                        else
                            // Otherwise check the target parameter is assignable from the provided argument
                            if (!pi[i].ParameterType.IsInstanceOfType(args[i])) return false;
                    }

                    // OK, we have a match

                    // Pad the args with Type.Missing to save repeating the reflection
                    for (int i = args.Count; i < pi.Length; i++)
                        args.Add(Type.Missing);

                    // Say we've found "the" match
                    return true;
                })
                // Use the first one (or none...)
                .FirstOrDefault();

            if (ci != null)
                return ci.Invoke(args.ToArray()) as AHandlerElement;

            return Activator.CreateInstance(GetType(), args.ToArray(), null) as AHandlerElement;
        }
        //public abstract AHandlerElement Clone(EventHandler handler);

        /// <summary>
        /// Flag the <see cref="AHandlerElement"/> as dirty and invoke the <see cref="EventHandler"/> delegate.
        /// </summary>
        protected virtual void OnElementChanged()
        {
            dirty = true;
            //Console.WriteLine(this.GetType().Name + " dirtied.");
            handler?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Change the element change handler to <paramref name="handler"/>.
        /// </summary>
        /// <param name="handler">The new element change handler.</param>
        internal void SetHandler(EventHandler handler) { if (this.handler != handler) this.handler = handler; }
    }

    /// <summary>
    /// An extension to <see cref="AHandlerElement"/>, for simple data types (such as <see cref="uint"/>).
    /// </summary>
    /// <typeparam name="T">A simple data type (such as <see cref="uint"/>).</typeparam>
    /// <remarks>For an example of use, see <see cref="SimpleList{T}"/>.</remarks>
    /// <seealso cref="SimpleList{T}"/>
    public class HandlerElement<T> : AHandlerElement, IEquatable<HandlerElement<T>>
        where T : struct, IComparable, IConvertible, IEquatable<T>, IComparable<T>
    {
        T _val;

        /// <summary>
        /// Initialize a new instance with a default value.
        /// </summary>
        /// <param name="handler">The <see cref="EventHandler"/> delegate to invoke if the <see cref="AHandlerElement"/> changes.</param>
        public HandlerElement(EventHandler handler) : this(handler, default(T)) { }

        /// <summary>
        /// Initialize a new instance with an initial value of <paramref name="basis"/>.
        /// </summary>
        /// <param name="handler">The <see cref="EventHandler"/> delegate to invoke if the <see cref="AHandlerElement"/> changes.</param>
        /// <param name="basis">Initial value for instance.</param>
        public HandlerElement(EventHandler handler, T basis) : base(handler) { _val = basis; }

        /// <summary>
        /// Initialize a new instance with an initial value from <paramref name="basis"/>.
        /// </summary>
        /// <param name="handler">The <see cref="EventHandler"/> delegate to invoke if the <see cref="AHandlerElement"/> changes.</param>
        /// <param name="basis">Element containing the initial value for instance.</param>
        public HandlerElement(EventHandler handler, HandlerElement<T> basis) : base(handler) { _val = basis._val; }

        #region AHandlerElement
        /// <summary>
        /// Get a copy of the HandlerElement but with a new change <see cref="EventHandler"/>.
        /// </summary>
        /// <param name="handler">The replacement HandlerElement delegate.</param>
        /// <returns>Return a copy of the HandlerElement but with a new change <see cref="EventHandler"/>.</returns>
        public override AHandlerElement Clone(EventHandler handler) => new HandlerElement<T>(handler, _val);

        /// <summary>
        /// The list of available field names on this API object.
        /// </summary>
        public override List<string> ContentFields => GetContentFields(GetType());
        #endregion

        #region IEquatable<HandlerElement<T>>
        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>true if the current object is equal to the other parameter; otherwise, false.</returns>
        public bool Equals(HandlerElement<T> other) => _val.Equals(other._val);

        /// <summary>
        /// Determines whether the specified <see cref="object"/> is equal to the current <see cref="HandlerElement{T}"/>.
        /// </summary>
        /// <param name="obj">The <see cref="object"/> to compare with the current <see cref="HandlerElement{T}"/>.</param>
        /// <returns>true if the specified <see cref="object"/> is equal to the current <see cref="HandlerElement{T}"/>; otherwise, false.</returns>
        /// <exception cref="System.NullReferenceException">The obj parameter is null.</exception>
        public override bool Equals(object obj)
        {
            if (obj is T t) return _val.Equals(t);
            else if (obj is HandlerElement<T> element) return Equals(element);
            return false;
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override int GetHashCode() => _val.GetHashCode();

        #endregion

        /// <summary>
        /// The value of the object.
        /// </summary>
        public T Val { get => _val; set { if (!_val.Equals(value)) { _val = value; OnElementChanged(); } } }

        /// <summary>
        /// Implicit cast from <see cref="HandlerElement{T}"/> to <typeparamref name="T"/>.
        /// </summary>
        /// <param name="value">Value to cast.</param>
        /// <returns>Cast value.</returns>
        public static implicit operator T(HandlerElement<T> value) => value._val;
        //// <summary>
        //// Implicit cast from <typeparamref name="T"/> to <see cref="HandlerElement{T}"/>.
        //// </summary>
        //// <param name="value">Value to cast.</param>
        //// <returns>Cast value.</returns>
        //--do not want to accidentally disrupt the content of lists through this cast!
        //public static implicit operator HandlerElement<T>(T value) { return new HandlerElement<T>(0, null, value); }

        /// <summary>
        /// Get displayable value.
        /// </summary>
        public string Value => new TypedValue(typeof(T), _val, "X").ToString();
    }

    /// <summary>
    /// An extension to <see cref="AHandlerElement"/>, for lists of TGIBlockList indices.
    /// </summary>
    /// <typeparam name="T">A simple data type (such as <see cref="int"/>).</typeparam>
    /// <remarks>For an example of use, see <see cref="IndexList{T}"/>.</remarks>
    /// <seealso cref="IndexList{T}"/>
    public class TGIBlockListIndex<T> : AHandlerElement, IEquatable<TGIBlockListIndex<T>>
        where T : struct, IComparable, IConvertible, IEquatable<T>, IComparable<T>
    {
        /// <summary>
        /// Reference to list into which this is an index.
        /// </summary>
        public DependentList<TGIBlock> ParentTGIBlocks { get; set; }

        T _data;

        /// <summary>
        /// Initialize a new instance with a default value.
        /// </summary>
        /// <param name="handler">The <see cref="EventHandler"/> delegate to invoke if the <see cref="AHandlerElement"/> changes.</param>
        /// <param name="parentTgiBlocks">Reference to list into which this is an index.</param>
        public TGIBlockListIndex(EventHandler handler, DependentList<TGIBlock> parentTgiBlocks = null)
            : this(handler, default(T), parentTgiBlocks) { }

        /// <summary>
        /// Initialize a new instance with an initial value from <paramref name="basis"/>.
        /// </summary>
        /// <param name="handler">The <see cref="EventHandler"/> delegate to invoke if the <see cref="AHandlerElement"/> changes.</param>
        /// <param name="basis">Element containing the initial value for instance.</param>
        /// <param name="parentTgiBlocks">Reference to list into which this is an index, or null to use that in <paramref name="basis"/>.</param>
        public TGIBlockListIndex(EventHandler handler, TGIBlockListIndex<T> basis, DependentList<TGIBlock> parentTgiBlocks = null)
            : this(handler, basis._data, parentTgiBlocks ?? basis.ParentTGIBlocks) { }

        /// <summary>
        /// Initialize a new instance with an initial value of <paramref name="value"/>.
        /// </summary>
        /// <param name="handler">The <see cref="EventHandler"/> delegate to invoke if the <see cref="AHandlerElement"/> changes.</param>
        /// <param name="value">Initial value for instance.</param>
        /// <param name="parentTgiBlocks">Reference to list into which this is an index.</param>
        public TGIBlockListIndex(EventHandler handler, T value, DependentList<TGIBlock> parentTgiBlocks = null)
            : base(handler) { ParentTGIBlocks = parentTgiBlocks; _data = value; }

        #region AHandlerElement
        // /// <summary>
        // /// Get a copy of the HandlerElement but with a new change <see cref="EventHandler"/>.
        // /// </summary>
        // /// <param name="handler">The replacement HandlerElement delegate.</param>
        // /// <returns>Return a copy of the HandlerElement but with a new change <see cref="EventHandler"/>.</returns>
        // public override AHandlerElement Clone(EventHandler handler) { return new TGIBlockListIndex<T>(requestedApiVersion, handler, data) { ParentTGIBlocks = ParentTGIBlocks }; }

        /// <summary>
        /// The list of available field names on this API object.
        /// </summary>
        public override List<string> ContentFields { get { List<string> res = GetContentFields(GetType()); res.Remove("ParentTGIBlocks"); return res; } }
        #endregion

        #region IEquatable<TGIBlockListIndex<T>>
        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>true if the current object is equal to the other parameter; otherwise, false.</returns>
        public bool Equals(TGIBlockListIndex<T> other) { return _data.Equals(other._data); }

        /// <summary>
        /// Determines whether the specified <see cref="object"/> is equal to the current <see cref="HandlerElement{T}"/>.
        /// </summary>
        /// <param name="obj">The <see cref="object"/> to compare with the current <see cref="HandlerElement{T}"/>.</param>
        /// <returns>true if the specified <see cref="object"/> is equal to the current <see cref="HandlerElement{T}"/>; otherwise, false.</returns>
        /// <exception cref="System.NullReferenceException">The obj parameter is null.</exception>
        public override bool Equals(object obj)
        {
            if (obj is T) return _data.Equals((T)obj);
            else if (obj is TGIBlockListIndex<T>) return Equals(obj as TGIBlockListIndex<T>);
            return false;
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override int GetHashCode() { return _data.GetHashCode(); }

        #endregion

        /// <summary>
        /// The value of the object.
        /// </summary>
        [TGIBlockListContentField("ParentTGIBlocks")]
        public T Data { get => _data;
            set { if (!_data.Equals(value)) { _data = value; OnElementChanged(); } } }

        /// <summary>
        /// Implicit cast from <see cref="HandlerElement{T}"/> to <typeparamref name="T"/>.
        /// </summary>
        /// <param name="value">Value to cast.</param>
        /// <returns>Cast value.</returns>
        public static implicit operator T(TGIBlockListIndex<T> value) => value._data;
        //// <summary>
        //// Implicit cast from <typeparamref name="T"/> to <see cref="HandlerElement{T}"/>.
        //// </summary>
        //// <param name="value">Value to cast.</param>
        //// <returns>Cast value.</returns>
        //--do not want to accidentally disrupt the content of lists through this cast!
        //public static implicit operator HandlerElement<T>(T value) { return new HandlerElement<T>(0, null, value); }
        /// <summary>
        /// Displayable value
        /// </summary>
        public string Value => ValueBuilder.Replace("Data: ", "");
    }
}
