using System;
using System.Collections.Generic;
using System.IO;

namespace Regul.S3PI.Interfaces
{
    /// <summary>
    /// Defines a quaternion - the quotient of two vectors.
    /// </summary>
    public class Quaternion : AHandlerElement, IEquatable<Quaternion>
    {
        #region Attributes
        float a = 0f;
        float b = 0f;
        float c = 0f;
        float d = 0f;
        #endregion

        #region Constructors
        /// <summary>
        /// Create a Quaternion { 0, 0, 0, 0 }.
        /// </summary>
        /// <param name="APIversion">The requested API version.</param>
        /// <param name="handler">The <see cref="EventHandler"/> delegate to invoke if the <see cref="AHandlerElement"/> changes.</param>
        public Quaternion(EventHandler handler) : base(handler) { }
        /// <summary>
        /// Create a Quaternion from a <see cref="Stream"/>.
        /// </summary>
        /// <param name="APIversion">The requested API version.</param>
        /// <param name="handler">The <see cref="EventHandler"/> delegate to invoke if the <see cref="AHandlerElement"/> changes.</param>
        /// <param name="s"><see cref="Stream"/> containing coordinates.</param>
        public Quaternion(EventHandler handler, Stream s) : base(handler) { Parse(s); }
        /// <summary>
        /// Create a Quaternion from a given value.
        /// </summary>
        /// <param name="APIversion">The requested API version.</param>
        /// <param name="handler">The <see cref="EventHandler"/> delegate to invoke if the <see cref="AHandlerElement"/> changes.</param>
        /// <param name="basis"><see cref="Quaternion"/> to copy.</param>
        public Quaternion(EventHandler handler, Quaternion basis)
            : this(handler, basis.a, basis.b, basis.c, basis.d) { }
        /// <summary>
        /// Create a Quaternion { a, b, c, d }.
        /// </summary>
        /// <param name="APIversion">The requested API version.</param>
        /// <param name="handler">The <see cref="EventHandler"/> delegate to invoke if the <see cref="AHandlerElement"/> changes.</param>
        /// <param name="a">Q 'a' value.</param>
        /// <param name="b">Q 'b' value.</param>
        /// <param name="c">Q 'c' value.</param>
        /// <param name="d">Q 'd' value.</param>
        public Quaternion(EventHandler handler, float a, float b, float c, float d)
            : base(handler)
        {
            this.a = a;
            this.b = b;
            this.c = c;
            this.d = d;
        }
        #endregion

        #region Data I/O
        void Parse(Stream s)
        {
            BinaryReader r = new BinaryReader(s);
            this.a = r.ReadSingle();
            this.b = r.ReadSingle();
            this.c = r.ReadSingle();
            this.d = r.ReadSingle();
        }

        /// <summary>
        /// Write the Quaternion to the given <see cref="Stream"/>.
        /// </summary>
        /// <param name="s"><see cref="Stream"/> to contain coordinates.</param>
        public void UnParse(Stream s)
        {
            BinaryWriter w = new BinaryWriter(s);
            w.Write(a);
            w.Write(b);
            w.Write(c);
            w.Write(d);
        }
        #endregion

        #region AHandlerElement Members
        /// <summary>
        /// The list of available field names on this API object.
        /// </summary>
        public override List<string> ContentFields { get { return GetContentFields(GetType()); } }

        // /// <summary>
        // /// Get a copy of the <see cref="Quaternion"/> but with a new change <see cref="EventHandler"/>.
        // /// </summary>
        // /// <param name="handler">The replacement <see cref="EventHandler"/> delegate.</param>
        // /// <returns>Return a copy of the <see cref="Quaternion"/> but with a new change <see cref="EventHandler"/>.</returns>
        // public override AHandlerElement Clone(EventHandler handler) { return new Quaternion(requestedApiVersion, handler, this); }
        #endregion

        #region IEquatable<Quaternion> Members

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>true if the current object is equal to the other parameter; otherwise, false.</returns>
        public bool Equals(Quaternion other)
        {
            return this.a == other.a && this.b == other.b && this.c == other.c && this.d == other.d;
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to the current <see cref="Quaternion"/>.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with the current <see cref="Quaternion"/>.</param>
        /// <returns>true if the specified <see cref="System.Object"/> is equal to the current <see cref="Quaternion"/>; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return obj as Quaternion != null ? this.Equals(obj as Quaternion) : false;
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override int GetHashCode()
        {
            return a.GetHashCode() ^ b.GetHashCode() ^ c.GetHashCode() ^ d.GetHashCode();
        }

        #endregion

        #region Content Fields
        /// <summary>
        /// 'a' value
        /// </summary>
        [ElementPriority(1)]
        public float A { get { return a; } set { if (a != value) { a = value; OnElementChanged(); } } }
        /// <summary>
        /// 'b' value
        /// </summary>
        [ElementPriority(2)]
        public float B { get { return b; } set { if (b != value) { b = value; OnElementChanged(); } } }
        /// <summary>
        /// 'c' value
        /// </summary>
        [ElementPriority(3)]
        public float C { get { return c; } set { if (c != value) { c = value; OnElementChanged(); } } }
        /// <summary>
        /// 'd' value
        /// </summary>
        [ElementPriority(3)]
        public float D { get { return d; } set { if (d != value) { d = value; OnElementChanged(); } } }

        /// <summary>
        /// A displayable representation of the object
        /// </summary>
        public string Value { get { return "{ " + string.Format("{0:F4}; {1:F4}; {2:F4}; {3:F4}", a, b, c, d) + " }"; } }
        #endregion
    }
}