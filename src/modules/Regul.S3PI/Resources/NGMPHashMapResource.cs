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
    /// A resource wrapper that understands 0xF3A38370 resources
    /// </summary>
    public class NGMPHashMapResource : AResource
    {
        uint _version = 1;

        NGMPPairList _data;

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
        public NGMPHashMapResource(Stream s)
            : base(s)
        {
            if (stream == null) { stream = UnParse(); OnResourceChanged(this, EventArgs.Empty); }
            stream.Position = 0;
            Parse(stream);
        }

        void Parse(Stream s)
        {
            BinaryReader br = new BinaryReader(s);

            _version = br.ReadUInt32();
            if (_version != 1)
                throw new InvalidDataException(
                    $"{GetType().Name}: unsupported 'version'.  Read '0x{_version:X8}', supported: '0x00000001'");

            _data = new NGMPPairList(OnResourceChanged, s);

            if (s.Position != s.Length)
                throw new InvalidDataException(
                    $"{GetType().Name}: Length {s.Length} bytes, parsed {s.Position} bytes");
        }

        protected override Stream UnParse()
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter w = new BinaryWriter(ms);
            w.Write(_version);
            if (_data == null) _data = new NGMPPairList(OnResourceChanged);
            _data.UnParse(ms);
            w.Flush();
            return ms;
        }

        public class NgmpPair : AHandlerElement, IEquatable<NgmpPair>
        {
            ulong _nameHash;
            ulong _instance;

            public NgmpPair(EventHandler handler) : base(handler) { }
            public NgmpPair(EventHandler handler, NgmpPair basis) : this(handler, basis._nameHash, basis._instance) { }
            public NgmpPair(EventHandler handler, ulong nameHash, ulong instance) : base(handler) { _nameHash = nameHash; _instance = instance; }
            public NgmpPair(EventHandler handler, Stream s) : base(handler) { Parse(s); }

            void Parse(Stream s)
            {
                BinaryReader r = new BinaryReader(s);
                _nameHash = r.ReadUInt64();
                _instance = r.ReadUInt64();
            }
            internal void UnParse(Stream s)
            {
                BinaryWriter w = new BinaryWriter(s);
                w.Write(_nameHash);
                w.Write(_instance);
            }

            #region AHandlerElement
            public override List<string> ContentFields => GetContentFields(GetType());

            #endregion

            #region IEquatable<NgmpPair>
            public bool Equals(NgmpPair other) { return _nameHash.Equals(other._nameHash) && _instance.Equals(other._instance); }
            #endregion

            #region Content Fields
            [ElementPriority(1)]
            public ulong NameHash { get => _nameHash;
                set { if (_nameHash != value) { _nameHash = value; OnElementChanged(); } } }
            [ElementPriority(2)]
            public ulong Instance { get => _instance;
                set { if (_instance != value) { _instance = value; OnElementChanged(); } } }

            public string Value => ValueBuilder.Replace("\n", "; ");

            #endregion
        }

        public class NGMPPairList : DependentList<NgmpPair>
        {
            public NGMPPairList(EventHandler handler) : base(handler) { }
            public NGMPPairList(EventHandler handler, Stream s) : base(handler, s) { }
            public NGMPPairList(EventHandler handler, IEnumerable<NgmpPair> ln) : base(handler, ln) { }

            #region DependentList<NgmpPair>
            protected override NgmpPair CreateElement(Stream s) { return new NgmpPair(elementHandler, s); }
            protected override void WriteElement(Stream s, NgmpPair element) { element.UnParse(s); }
            #endregion
        }
        
        [ElementPriority(1)]
        public NGMPPairList Data { get => _data;
            set { if (!_data.Equals(value)) { _data = value == null ? null : new NGMPPairList(OnResourceChanged, value); OnResourceChanged(this, EventArgs.Empty); } } }

        public String Value => ValueBuilder;
    }
    
    /// <summary>
    /// ResourceHandler for NameMapResource wrapper
    /// </summary>
    public class NGMPHashMapResourceHandler : AResourceHandler
    {
        /// <summary>
        /// Create the content of the Dictionary
        /// </summary>
        public NGMPHashMapResourceHandler()
        {
            Add(typeof(NGMPHashMapResource), new List<string>(new[] { "0xF3A38370" }));
        }
    }
}