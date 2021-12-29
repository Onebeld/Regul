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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Regul.S3PI.Interfaces;

namespace Regul.S3PI.Resources
{
    public sealed class StblResource : AResource, IDictionary<ulong, string>, IDictionary
    {
        public override List<string> ContentFields => GetContentFields(GetType());

        #region Attributes

        private ushort _unknown1;
        private ushort _unknown2;
        private uint _unknown3;
        private Dictionary<ulong, string> _entries;
        #endregion

        public StblResource(Stream s) : base(s) { if (stream == null) { stream = UnParse(); OnResourceChanged(this, EventArgs.Empty); } stream.Position = 0; Parse(stream); }

        #region Data I/O

        private void Parse(Stream s)
        {
            BinaryReader r = new BinaryReader(s);

            uint magic = r.ReadUInt32();
            if (magic != FOURCC("STBL"))
                throw new InvalidDataException(
                    $"Expected magic tag 0x{FOURCC("STBL"):X8}; read 0x{magic:X8}; position 0x{s.Position:X8}");
            byte version = r.ReadByte();
            if (version != 0x02)
                throw new InvalidDataException(
                    $"Expected version 0x02; read 0x{version:X2}; position 0x{s.Position:X8}");
            
            _unknown1 = r.ReadUInt16();

            uint count = r.ReadUInt32();

            _unknown2 = r.ReadUInt16();
            _unknown3 = r.ReadUInt32();

            _entries = new Dictionary<ulong, string>(); 
            for (int i = 0; i < count; i++)
            {
                ulong key = r.ReadUInt64();
                string value = Encoding.Unicode.GetString(r.ReadBytes(r.ReadInt32() * 2));
                if (_entries.ContainsKey(key)) continue; // Patch 1.6 has problems in the STBLs (World Adventures sneaked into the DeltaBuild0 file)
                _entries.Add(key, value);
            }
        }

        protected override Stream UnParse()
        {
            MemoryStream ms = new MemoryStream();

            BinaryWriter w = new BinaryWriter(ms);

            w.Write((uint)FOURCC("STBL"));
            w.Write((byte)0x02);

            w.Write(_unknown1);

            if (_entries == null) _entries = new Dictionary<ulong, string>();
            w.Write(_entries.Count);

            w.Write(_unknown2);
            w.Write(_unknown3);

            foreach (KeyValuePair<ulong, string> kvp in _entries)
            {
                w.Write(kvp.Key);
                w.Write(kvp.Value.Length);
                w.Write(Encoding.Unicode.GetBytes(kvp.Value));
            }

            return ms;
        }
        #endregion

        #region IDictionary<ulong,string> Members

        public void Add(ulong key, string value) { _entries.Add(key, value); OnResourceChanged(this, EventArgs.Empty); }

        public bool ContainsKey(ulong key) { return _entries.ContainsKey(key); }

        public ICollection<ulong> Keys => _entries.Keys;

        public bool Remove(ulong key) { try { return _entries.Remove(key); } finally { OnResourceChanged(this, EventArgs.Empty); } }

        public bool TryGetValue(ulong key, out string value) { return _entries.TryGetValue(key, out value); }

        public ICollection<string> Values => _entries.Values;

        public string this[ulong key]
        {
            get => _entries[key];
            set { if (_entries[key] != value) { _entries[key] = value; OnResourceChanged(this, EventArgs.Empty); } }
        }

        #endregion

        #region ICollection<KeyValuePair<ulong,string>> Members

        public void Add(KeyValuePair<ulong, string> item) { _entries.Add(item.Key, item.Value); }

        public void Clear() { _entries.Clear(); OnResourceChanged(this, EventArgs.Empty); }

        public bool Contains(KeyValuePair<ulong, string> item) { return _entries.ContainsKey(item.Key) && _entries[item.Key].Equals(item.Value); }

        public void CopyTo(KeyValuePair<ulong, string>[] array, int arrayIndex) { foreach (KeyValuePair<ulong, string> kvp in _entries) array[arrayIndex++] = kvp; }

        public int Count => _entries.Count;

        public bool IsReadOnly => false;

        public bool Remove(KeyValuePair<ulong, string> item) { try { return Contains(item) && _entries.Remove(item.Key); } finally { OnResourceChanged(this, EventArgs.Empty); } }

        #endregion

        #region IEnumerable<KeyValuePair<ulong,string>> Members

        public IEnumerator<KeyValuePair<ulong, string>> GetEnumerator() { return _entries.GetEnumerator(); }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator() { return _entries.GetEnumerator(); }

        #endregion

        #region IDictionary Members

        public void Add(object key, object value) { Add((ulong)key, (string)value); }

        public bool Contains(object key) { return ContainsKey((ulong)key); }

        IDictionaryEnumerator IDictionary.GetEnumerator() { return _entries.GetEnumerator(); }

        public bool IsFixedSize => false;

        ICollection IDictionary.Keys => _entries.Keys;

        public void Remove(object key) { Remove((ulong)key); }

        ICollection IDictionary.Values => _entries.Values;

        public object this[object key] { get => this[(ulong)key];
            set => this[(ulong)key] = (string)value;
        }

        #endregion

        #region ICollection Members

        public void CopyTo(Array array, int index) { CopyTo((KeyValuePair<ulong, string>[])array, index); }

        public bool IsSynchronized => false;

        public object SyncRoot => null;

        #endregion

        /// <summary>
        /// Return the default dictionary entry for this <c>IDictionary{TKey, TValue}</c>.
        /// </summary>
        /// <returns>The default dictionary entry for this <c>IDictionary{TKey, TValue}</c>.</returns>
        public static DictionaryEntry GetDefault() { return new DictionaryEntry((ulong)0, ""); }

        #region Content Fields
        public ushort Unknown1 { get => _unknown1;
            set { if (_unknown1 != value) { _unknown1 = value; OnResourceChanged(this, EventArgs.Empty); } } }
        public ushort Unknown2 { get => _unknown2;
            set { if (_unknown2 != value) { _unknown2 = value; OnResourceChanged(this, EventArgs.Empty); } } }
        public uint Unknown3 { get => _unknown3;
            set { if (_unknown3 != value) { _unknown3 = value; OnResourceChanged(this, EventArgs.Empty); } } }

        public String Value => ValueBuilder;

        #endregion
    }
    
    /// <summary>
    /// ResourceHandler for StblResource wrapper
    /// </summary>
    public class StblResourceHandler : AResourceHandler
    {
        public StblResourceHandler()
        {
            Add(typeof(StblResource), new List<string>(new[] { "0x220557DA", }));
        }
    }
}