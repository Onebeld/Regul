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
using System.IO;
using Regul.S3PI.Interfaces;

namespace Regul.S3PI.Resources
{
    /// <summary>
    /// A resource wrapper that understands 0x0166038C resources
    /// </summary>
    public class NameMapResource : AResource, IDictionary<ulong, string>, System.Collections.IDictionary
    {
        uint _version = 1;
        Dictionary<ulong, string> _data = new Dictionary<ulong, string>();

        #region AApiVersionedFields
        /// <summary>
        /// The list of available field names on this API object
        /// </summary>
        public override List<string> ContentFields => GetContentFields(GetType());

        #endregion

        /// <summary>
        /// Create a new instance of the resource
        /// </summary>
        /// <param name="s">Data stream to use, or null to create from scratch</param>
        public NameMapResource(Stream s) : base(s) { if (stream == null) { stream = UnParse(); OnResourceChanged(this, EventArgs.Empty); } stream.Position = 0; Parse(stream); }

        void Parse(Stream s)
        {
            BinaryReader br = new BinaryReader(s);

            _version = br.ReadUInt32();

            for (int i = br.ReadInt32(); i > 0; i--)
                _data.Add(br.ReadUInt64(), new String(br.ReadChars(br.ReadInt32())));

            if (s.Position != s.Length)
                throw new InvalidDataException(
                    $"{GetType().Name}: Length {s.Length} bytes, parsed {s.Position} bytes");
        }

        protected override Stream UnParse()
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter w = new BinaryWriter(ms);
            w.Write(_version);
            w.Write(Count);
            foreach (KeyValuePair<ulong, string> kvp in this)
            {
                w.Write(kvp.Key);
                w.Write(kvp.Value.Length);
                w.Write(kvp.Value.ToCharArray());
            }
            w.Flush();
            return ms;
        }


        /// <summary>
        /// Return the default dictionary entry for this <c>IDictionary{TKey, TValue}</c>.
        /// </summary>
        /// <returns>The default dictionary entry for this <c>IDictionary{TKey, TValue}</c>.</returns>
        public static System.Collections.DictionaryEntry GetDefault() { return new System.Collections.DictionaryEntry((ulong)0, ""); }
        
        public uint Version { get => _version;
            set { if (Version == value) return; _version = value; OnResourceChanged(this, EventArgs.Empty); } }

        public String Value => ValueBuilder;

        #region IDictionary<ulong,string> Members

        public void Add(ulong key, string value)
        {
            _data.Add(key, value);
            OnResourceChanged(this, EventArgs.Empty);
        }

        public bool ContainsKey(ulong key) { return _data.ContainsKey(key); }

        public ICollection<ulong> Keys => _data.Keys;

        public bool Remove(ulong key)
        {
            bool res = _data.Remove(key);
            if (res)
                OnResourceChanged(this, EventArgs.Empty);
            return res;
        }

        public bool TryGetValue(ulong key, out string value) { return _data.TryGetValue(key, out value); }

        public ICollection<string> Values => _data.Values;

        public string this[ulong key]
        {
            get => _data[key];
            set
            {
                if (_data[key] == value) return;
                _data[key] = value;
                OnResourceChanged(this, EventArgs.Empty);
            }
        }

        #endregion

        #region ICollection<KeyValuePair<ulong,string>> Members

        public void Add(KeyValuePair<ulong, string> item) { Add(item.Key, item.Value); }

        public void Clear()
        {
            _data.Clear();
            OnResourceChanged(this, EventArgs.Empty);
        }

        public bool Contains(KeyValuePair<ulong, string> item) { return _data.ContainsKey(item.Key) && _data[item.Key].Equals(item.Value); }

        public void CopyTo(KeyValuePair<ulong, string>[] array, int arrayIndex)
        {
            foreach (KeyValuePair<ulong, string> kvp in _data) array[arrayIndex++] = new KeyValuePair<ulong,string>(kvp.Key, kvp.Value);
        }

        public int Count => _data.Count;

        public bool IsReadOnly => false;

        public bool Remove(KeyValuePair<ulong, string> item) { return Contains(item) ? Remove(item.Key) : false; }

        #endregion

        #region IEnumerable<KeyValuePair<ulong,string>> Members

        public IEnumerator<KeyValuePair<ulong, string>> GetEnumerator() { return _data.GetEnumerator(); }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return _data.GetEnumerator(); }

        #endregion

        #region IDictionary Members

        public void Add(object key, object value) { Add((ulong)key, (string)value); }

        public bool Contains(object key) { return ContainsKey((ulong)key); }

        System.Collections.IDictionaryEnumerator System.Collections.IDictionary.GetEnumerator() { return _data.GetEnumerator(); }

        public bool IsFixedSize => false;

        System.Collections.ICollection System.Collections.IDictionary.Keys => _data.Keys;

        public void Remove(object key) { Remove((ulong)key); }

        System.Collections.ICollection System.Collections.IDictionary.Values => _data.Values;

        public object this[object key] { get => this[(ulong)key];
            set => this[(ulong)key] = (string)value;
        }

        #endregion

        #region ICollection Members

        public void CopyTo(Array array, int index) { CopyTo((KeyValuePair<ulong, string>[])array, index); }

        public bool IsSynchronized => false;

        public object SyncRoot => null;

        #endregion
    }

    /// <summary>
    /// ResourceHandler for NameMapResource wrapper
    /// </summary>
    public class NameMapResourceHandler : AResourceHandler
    {
        /// <summary>
        /// Create the content of the Dictionary
        /// </summary>
        public NameMapResourceHandler()
        {
            Add(typeof(NameMapResource), new List<string>(new[] { "0x0166038C" }));
        }
    }
}
