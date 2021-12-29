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
using System.Reflection;
using Regul.S3PI.Interfaces;

namespace Regul.S3PI.Resources
{
    /// <summary>
    /// A resource wrapper that understands Catalog Entry resources
    /// </summary>
    public class ObjKeyResource : AResource
    {
        public override List<string> ContentFields => GetContentFields(GetType());

        #region Attributes
        uint _format = 7;
        ComponentList _components;
        ComponentDataList _componentData;
        byte _unknown1;
        TGIBlockList _tgiBlocks;
        #endregion

        public ObjKeyResource(Stream s) : base(s) { if (stream == null) { stream = UnParse(); OnResourceChanged(this, EventArgs.Empty); } stream.Position = 0; Parse(stream); }

        #region Data I/O
        void Parse(Stream s)
        {
            long tgiPosn, tgiSize;
            BinaryReader r = new BinaryReader(s);

            _format = r.ReadUInt32();
            tgiPosn = r.ReadUInt32() + s.Position;
            tgiSize = r.ReadUInt32();

            _components = new ComponentList(OnResourceChanged, s);
            _componentData = new ComponentDataList(OnResourceChanged, s);
            _unknown1 = r.ReadByte();

            _tgiBlocks = new TGIBlockList(OnResourceChanged, s, tgiPosn, tgiSize);

            _componentData.ParentTGIBlocks = _tgiBlocks;
        }

        protected override Stream UnParse()
        {
            long posn;
            MemoryStream s = new MemoryStream();
            BinaryWriter w = new BinaryWriter(s);

            w.Write(_format);

            posn = s.Position;
            w.Write((uint)0);
            w.Write((uint)0);

            if (_components == null) _components = new ComponentList(OnResourceChanged);
            _components.UnParse(s);

            if (_tgiBlocks == null) _tgiBlocks = new TGIBlockList(OnResourceChanged);
            if (_componentData == null) _componentData = new ComponentDataList(OnResourceChanged, _tgiBlocks);
            _componentData.UnParse(s);

            w.Write(_unknown1);

            _tgiBlocks.UnParse(s, posn);

            s.Flush();

            return s;
        }
        #endregion

        #region Sub-classes
        public enum Component : uint
        {
            Animation = 0xee17c6ad,
            Effect = 0x80d91e9e,
            Footprint = 0xc807312a,
            Lighting = 0xda6c50fd,
            Location = 0x461922c8,
            LotObject = 0x6693c8b3,
            Model = 0x2954e734,
            Physics = 0x1a8feb14,
            Sacs = 0x3ae9a8e7,
            Script = 0x23177498,
            Sim = 0x22706efa,
            Slot = 0x2ef1e401,
            Steering = 0x61bd317c,
            Transform = 0x54cb7ebb,
            Tree = 0xc602cd31,
            VisualState = 0x50b3d17c,
        }

        static Dictionary<Component, string> _componentDataMap;

        public class ComponentElement : AHandlerElement, IEquatable<ComponentElement>
        {
            private Component _element;
            
            public ComponentElement(EventHandler handler) : base(handler) { }
            public ComponentElement(EventHandler handler, ComponentElement basis) : this(handler, basis._element) { }
            public ComponentElement(EventHandler handler, uint value) : base(handler) { _element = (Component)value; }
            public ComponentElement(EventHandler handler, Component element) : base(handler) { _element = element; }

            static ComponentElement()
            {
                _componentDataMap = new Dictionary<Component, string>
                {
                    { Component.Sim, "simOutfitKey" },
                    { Component.Script, "scriptClass" },
                    { Component.Model, "modelKey" },
                    { Component.Steering, "steeringInstance" },
                    { Component.Tree, "modelKey" },
                    { Component.Footprint, "footprintKey" }
                };
            }

            #region AHandlerElement Members
            
            public override List<string> ContentFields => GetContentFields(GetType());

            #endregion

            #region IEquatable<ComponentElement> Members

            public bool Equals(ComponentElement other) { return ((uint)_element).Equals((uint)other._element); }

            public override bool Equals(object obj) => obj is ComponentElement componentElement && Equals(componentElement);

            public override int GetHashCode() => _element.GetHashCode();

            #endregion

            public TypedValue Data(ComponentDataList list, TGIBlockList tgiBlocks)
            {
                if (!_componentDataMap.ContainsKey(_element)) return null;
                if (!list.ContainsKey(_componentDataMap[_element])) return null;
                ComponentDataType cd = list[_componentDataMap[_element]];
                PropertyInfo pi = cd.GetType().GetProperty("Data");
                if (pi == null || !pi.CanRead) return null;
                if (_element == Component.Footprint || _element == Component.Model || _element == Component.Tree)
                    return new TypedValue(typeof(TGIBlock), tgiBlocks[(int)pi.GetValue(cd, null)], "X");
                return new TypedValue(pi.PropertyType, pi.GetValue(cd, null), "X");
            }

            public Component Element { get => _element;
                set { if (_element != value) { _element = value; OnElementChanged(); } } }
            public string Value => "0x" + ((uint)_element).ToString("X8") + " (" + (Enum.IsDefined(typeof(Component), _element) ? _element + "" : "undefined") + ")";
        }

        public class ComponentList : DependentList<ComponentElement>
        {
            #region Constructors
            public ComponentList(EventHandler handler) : base(handler, Byte.MaxValue) { }
            public ComponentList(EventHandler handler, IEnumerable<ComponentElement> luint) : base(handler, luint, Byte.MaxValue) { }
            internal ComponentList(EventHandler handler, Stream s) : base(handler, s, Byte.MaxValue) { }
            #endregion

            #region Data I/O
            protected override int ReadCount(Stream s) => new BinaryReader(s).ReadByte();
            protected override ComponentElement CreateElement(Stream s) => new ComponentElement(elementHandler, new BinaryReader(s).ReadUInt32());

            protected override void WriteCount(Stream s, int count) { new BinaryWriter(s).Write((byte)count); }
            protected override void WriteElement(Stream s, ComponentElement element) { new BinaryWriter(s).Write((uint)element.Element); }
            #endregion

            public bool HasComponent(Component component) { return Find(component) != null; }

            public ComponentElement Find(Component component)
            {
                foreach (ComponentElement ce in this)
                    if (ce.Element == component) return ce;
                return null;
            }
        }

        public abstract class ComponentDataType : AHandlerElement, IComparable<ComponentDataType>, IEqualityComparer<ComponentDataType>, IEquatable<ComponentDataType>
        {
            #region Attributes
            protected string key;
            protected byte controlCode;
            #endregion

            #region Constructors
            protected ComponentDataType(EventHandler handler, string key, byte controlCode)
                : base(handler) { this.key = key; this.controlCode = controlCode; }

            public static ComponentDataType CreateComponentData(EventHandler handler, Stream s,
                DependentList<TGIBlock> parentTgiBlocks)
            {
                BinaryReader r = new BinaryReader(s);
                string key = new string(r.ReadChars(r.ReadInt32()));
                byte controlCode = r.ReadByte();
                switch (controlCode)
                {
                    case 0x00: return new CdtString(handler, key, controlCode, new string(r.ReadChars(r.ReadInt32())));
                    case 0x01: return new CdtResourceKey(handler, key, controlCode, r.ReadInt32(), parentTgiBlocks);
                    case 0x02: return new CdtAssetResourceName(handler, key, controlCode, r.ReadInt32(), parentTgiBlocks);
                    case 0x03: return new CdtSteeringInstance(handler, key, controlCode, new string(r.ReadChars(r.ReadInt32())));
                    case 0x04: return new CdtuInt32(handler, key, controlCode, r.ReadUInt32());
                    default:
                        throw new InvalidDataException(
                            $"Unknown control code 0x{controlCode:X2} at position 0x{s.Position:X8}");
                }
            }
            #endregion

            #region Data I/O
            internal virtual void UnParse(Stream s)
            {
                BinaryWriter w = new BinaryWriter(s);
                w.Write(key.Length);
                w.Write(key.ToCharArray());
                w.Write(controlCode);
            }
            #endregion

            #region AHandlerElement Members
            
            public override List<string> ContentFields => GetContentFields(GetType());

            #endregion

            #region IComparable<Key> Members

            public abstract int CompareTo(ComponentDataType other);

            #endregion

            #region IEqualityComparer<Key> Members

            public bool Equals(ComponentDataType x, ComponentDataType y) { return x.Equals(y); }

            public abstract int GetHashCode(ComponentDataType obj);

            #endregion

            #region IEquatable<Key> Members

            public bool Equals(ComponentDataType other) { return CompareTo(other) == 0; }

            public override bool Equals(object obj) => obj is ComponentDataType type && Equals(type);

            public override int GetHashCode() => key.GetHashCode() ^ controlCode.GetHashCode();

            #endregion

            #region Content Fields
            [ElementPriority(1)]
            public string Key { get => key;
                set { if (key != value) { key = value; OnElementChanged(); } } }

            public virtual string Value => GetType().Name + " -- Key: \"" + key + "\"; Control code: 0x" + controlCode.ToString("X2");

            #endregion
        }
        public class CdtString : ComponentDataType
        {
            #region Attributes
            protected string data;
            #endregion

            #region Constructors
            public CdtString(EventHandler handler) : this(handler, "CDTString-Key", 0x00, "Value") { }
            public CdtString(EventHandler handler, CdtString basis) : this(handler, basis.key, basis.controlCode, basis.data) { }
            public CdtString(EventHandler handler, string key, byte controlCode, string data) : base(handler, key, controlCode) { this.data = data; }
            #endregion

            #region Data I/O
            internal override void UnParse(Stream s)
            {
                base.UnParse(s);
                BinaryWriter w = new BinaryWriter(s);
                w.Write(data.Length);
                w.Write(data.ToCharArray());
            }
            #endregion

            public override int CompareTo(ComponentDataType other)
            {
                if (GetType() != other.GetType()) return -1;
                CdtString oth = (CdtString)other;
                int res = string.Compare(key, oth.key, StringComparison.Ordinal); if (res != 0) return res;
                res = controlCode.CompareTo(oth.controlCode); if (res != 0) return res;
                return string.Compare(data, oth.data, StringComparison.Ordinal);
            }

            public override int GetHashCode(ComponentDataType obj) { return key.GetHashCode() ^ controlCode ^ data.GetHashCode(); }

            [ElementPriority(2)]
            public string Data { get => data;
                set { if (data != value) { data = value; OnElementChanged(); } } }

            public override string Value => base.Value + "; Data: " + "\"" + data + "\"";
        }
        public class CdtResourceKey : ComponentDataType
        {
            public DependentList<TGIBlock> ParentTGIBlocks { get; set; }
            public override List<string> ContentFields { get { List<string> res = base.ContentFields; res.Remove("ParentTGIBlocks"); return res; } }

            #region Attributes
            protected int data;
            #endregion

            #region Constructors
            public CdtResourceKey(EventHandler handler, DependentList<TGIBlock> parentTgiBlocks = null)
                : this(handler, "CDTResourceKey-Key", 0x01, 0) { ParentTGIBlocks = parentTgiBlocks; }
            public CdtResourceKey(EventHandler handler, CdtResourceKey basis,
                DependentList<TGIBlock> parentTgiBlocks = null)
                : this(handler, basis.key, basis.controlCode, basis.data, parentTgiBlocks ?? basis.ParentTGIBlocks) { }
            public CdtResourceKey(EventHandler handler, string key, byte controlCode, int data,
                DependentList<TGIBlock> parentTgiBlocks = null)
                : base(handler, key, controlCode) { ParentTGIBlocks = parentTgiBlocks; this.data = data; }
            #endregion

            #region Data I/O
            internal override void UnParse(Stream s)
            {
                base.UnParse(s);
                new BinaryWriter(s).Write(data);
            }
            #endregion

            public override int CompareTo(ComponentDataType other)
            {
                if (GetType() != other.GetType()) return -1;
                CdtResourceKey oth = (CdtResourceKey)other;
                int res = string.Compare(key, oth.key, StringComparison.Ordinal); if (res != 0) return res;
                res = controlCode.CompareTo(oth.controlCode); if (res != 0) return res;
                return data.CompareTo(oth.data);
            }

            public override int GetHashCode(ComponentDataType obj) { return key.GetHashCode() ^ controlCode ^ data; }

            [ElementPriority(2), TGIBlockListContentField("ParentTGIBlocks")]
            public int Data { get => data;
                set { if (data != value) { data = value; OnElementChanged(); } } }

            public override string Value => base.Value + "; Data: " + "0x" + data.ToString("X8") + " (" + (ParentTGIBlocks == null ? "unknown" : ParentTGIBlocks[data]) + ")";
        }
        public class CdtAssetResourceName : CdtResourceKey
        {
            public CdtAssetResourceName(EventHandler handler, DependentList<TGIBlock> parentTgiBlocks = null)
                : base(handler, "CDTAssetResourceName-Key", 0x02, 0, parentTgiBlocks) { }
            public CdtAssetResourceName(EventHandler handler, CdtAssetResourceName basis,
                DependentList<TGIBlock> parentTgiBlocks = null)
                : base(handler, basis, parentTgiBlocks ?? basis.ParentTGIBlocks) { }
            public CdtAssetResourceName(EventHandler handler, string key, byte controlCode, int data,
                DependentList<TGIBlock> parentTgiBlocks = null)
                : base(handler, key, controlCode, data, parentTgiBlocks) { }
        }
        public class CdtSteeringInstance : CdtString
        {
            #region Constructors
            public CdtSteeringInstance(EventHandler handler) : base(handler, "CDTSteeringInstance-Key", 0x03, "Value") { }
            public CdtSteeringInstance(EventHandler handler, CdtSteeringInstance basis) : base(handler, basis) { }
            public CdtSteeringInstance(EventHandler handler, string key, byte controlCode, string data) : base(handler, key, controlCode, data) { }
            #endregion
        }
        public class CdtuInt32 : ComponentDataType
        {
            #region Attributes
            uint _data;
            #endregion

            #region Constructors
            public CdtuInt32(EventHandler handler) : this(handler, "CDTUInt32-Key", 0x04, 0) { }
            public CdtuInt32(EventHandler handler, CdtuInt32 basis) : this(handler, basis.key, basis.controlCode, basis._data) { }
            public CdtuInt32(EventHandler handler, string key, byte controlCode, uint data) : base(handler, key, controlCode) { _data = data; }
            #endregion

            #region Data I/O
            internal override void UnParse(Stream s)
            {
                base.UnParse(s);
                new BinaryWriter(s).Write(_data);
            }
            #endregion

            public override int CompareTo(ComponentDataType other)
            {
                if (GetType() != other.GetType()) return -1;
                CdtuInt32 oth = (CdtuInt32)other;
                int res = key.CompareTo(oth.key); if (res != 0) return res;
                res = controlCode.CompareTo(oth.controlCode); if (res != 0) return res;
                return _data.CompareTo(oth._data);
            }

            public override int GetHashCode(ComponentDataType obj) { return (int)(key.GetHashCode() ^ controlCode ^ _data); }

            [ElementPriority(2)]
            public uint Data { get => _data;
                set { if (_data != value) { _data = value; OnElementChanged(); } } }

            public override string Value => base.Value + "; Data: " + "0x" + _data.ToString("X8");
        }

        public class ComponentDataList : DependentList<ComponentDataType>
        {
            private DependentList<TGIBlock> _parentTgiBlocks;
            public DependentList<TGIBlock> ParentTGIBlocks
            {
                get => _parentTgiBlocks;
                set { if (_parentTgiBlocks != value) { _parentTgiBlocks = value; foreach (ComponentDataType i in FindAll(e => e is CdtResourceKey)) (i as CdtResourceKey).ParentTGIBlocks = _parentTgiBlocks; } }
            }

            #region Constructors
            public ComponentDataList(EventHandler handler, DependentList<TGIBlock> parentTgiBlocks = null)
                : base(handler, Byte.MaxValue) { _parentTgiBlocks = parentTgiBlocks; }
            internal ComponentDataList(EventHandler handler, Stream s, DependentList<TGIBlock> parentTgiBlocks = null)
                : this(null, parentTgiBlocks) { elementHandler = handler; Parse(s); this.handler = handler; }
            public ComponentDataList(EventHandler handler, IEnumerable<ComponentDataType> lcdt, DependentList<TGIBlock> parentTgiBlocks = null)
                : this(null, parentTgiBlocks) { elementHandler = handler; foreach (ComponentDataType t in lcdt) Add((ComponentDataType)t.Clone(null)); this.handler = handler; }
            #endregion

            #region Data I/O
            protected override int ReadCount(Stream s) { return new BinaryReader(s).ReadByte(); }
            protected override ComponentDataType CreateElement(Stream s) { return ComponentDataType.CreateComponentData(elementHandler, s, _parentTgiBlocks); }

            protected override void WriteCount(Stream s, int count) { new BinaryWriter(s).Write((byte)count); }
            protected override void WriteElement(Stream s, ComponentDataType element) { element.UnParse(s); }
            #endregion

            public bool ContainsKey(string key) { return Find(x => x.Key.Equals(key)) != null; }

            public ComponentDataType this[string key]
            {
                get
                {
                    ComponentDataType cd = Find(x => x.Key.Equals(key));
                    if (cd != null) return cd;
                    throw new KeyNotFoundException();
                }
                set => this[IndexOf(this[key])] = value;
            }

            public override void Add(ComponentDataType item)
            {
                if (item is CdtResourceKey) (item as CdtResourceKey).ParentTGIBlocks = _parentTgiBlocks;
                else if (item is CdtAssetResourceName) (item as CdtAssetResourceName).ParentTGIBlocks = _parentTgiBlocks;
                base.Add(item);
            }
            public override void Add(Type elementType)
            {
                if (elementType.IsAbstract)
                    throw new ArgumentException("Must pass a concrete element type.", "elementType");

                if (!typeof(ComponentDataType).IsAssignableFrom(elementType))
                    throw new ArgumentException("The element type must belong to the generic type of the list.", "elementType");

                ComponentDataType newElement;
                if (elementType == typeof(CdtResourceKey))
                    newElement = new CdtResourceKey(elementHandler, _parentTgiBlocks);
                else if (elementType == typeof(CdtAssetResourceName))
                    newElement = new CdtAssetResourceName(elementHandler, _parentTgiBlocks);
                else
                    newElement = Activator.CreateInstance(elementType, 0, elementHandler) as ComponentDataType;
                base.Add(newElement);
            }
        }

        #endregion

        #region Content Fields
        [ElementPriority(1)]
        public uint Format { get => _format;
            set { if (_format != value) { _format = value; OnResourceChanged(this, EventArgs.Empty); } } }
        [ElementPriority(2)]
        public ComponentList Components { get => _components;
            set { if (_components != value) { _components = value == null ? null : new ComponentList(OnResourceChanged, value); OnResourceChanged(this, EventArgs.Empty); } } }
        [ElementPriority(3)]
        public ComponentDataList ComponentData { get => _componentData;
            set { if (_componentData != value) { _componentData = value == null ? null : new ComponentDataList(OnResourceChanged, value, _tgiBlocks); OnResourceChanged(this, EventArgs.Empty); } } }
        [ElementPriority(4)]
        public byte Unknown1 { get => _unknown1;
            set { if (_unknown1 != value) { _unknown1 = value; OnResourceChanged(this, EventArgs.Empty); } } }
        [ElementPriority(5)]
        public TGIBlockList TgiBlocks { get => _tgiBlocks;
            set { if (_tgiBlocks != value) { _tgiBlocks = value == null ? null : new TGIBlockList(OnResourceChanged, value); _componentData.ParentTGIBlocks = _tgiBlocks; OnResourceChanged(this, EventArgs.Empty); } } }

        public string Value => ValueBuilder;

        #endregion
    }
    
    /// <summary>
    /// ResourceHandler for ObjKeyResource wrapper
    /// </summary>
    public class ObjKeyResourceHandler : AResourceHandler
    {
        public ObjKeyResourceHandler()
        {
            Add(typeof(ObjKeyResource), new List<string>(new[] { "0x02DC343F" }));
        }
    }
}