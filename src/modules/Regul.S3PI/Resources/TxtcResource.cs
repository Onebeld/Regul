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
    public class TxtcResource : AResource
    {
        public override List<string> ContentFields => GetContentFields(GetType());

        #region Attributes
        ContentType _root;
        #endregion

        public TxtcResource(Stream s) : base(s) { if (stream == null) { stream = UnParse(); OnResourceChanged(this, EventArgs.Empty); } stream.Position = 0; Parse(stream); }

        #region Data I/O
        void Parse(Stream s) { _root = new ContentType(OnResourceChanged, s); }

        protected override Stream UnParse()
        {
            MemoryStream ms = new MemoryStream();
            if (_root == null) _root = new ContentType(OnResourceChanged);
            _root.UnParse(ms);
            return ms;
        }
        #endregion

        #region Sub-classes
        public enum PatternSizeType : uint
        {
            Default = 0x00,
            Large = 0x01,
        }
        //Copied from CASPartFlags - TODO: resolve!
        [Flags]
        public enum DataTypeFlags : uint
        {
            Hair = 0x00000001,
            Scalp = 0x00000002,
            FaceOverlay = 0x00000004,
            Body = 0x00000008,
            Accessory = 0x00000010,
        }

        public class ContentType : AHandlerElement, IEquatable<ContentType>
        {
            DependentList<TGIBlock> _parentTgiBlocks;
            public DependentList<TGIBlock> ParentTGIBlocks
            {
                get => _parentTgiBlocks;
                set { if (_parentTgiBlocks != value) { _parentTgiBlocks = value; if (_superBlocks != null) _superBlocks.ParentTGIBlocks = _parentTgiBlocks; if (_entries != null) _entries.ParentTGIBlocks = _parentTgiBlocks; } }
            }

            void SetTgiBlocks() { if (_superBlocks != null) _superBlocks.ParentTGIBlocks = _tgiBlocks; if (_entries != null) _entries.ParentTGIBlocks = _tgiBlocks; }

            #region Attributes
            uint _version;
            SuperBlockList _superBlocks;//if version >= 7
            PatternSizeType _patternSize;
            DataTypeFlags _dataType;
            byte _unknown3;
            byte _unknown4;//if version >= 8
            EntryBlockList _entries;
            CountedTGIBlockList _tgiBlocks;
            #endregion

            #region Constructors
            public ContentType(EventHandler handler) : base(handler)
            {
                _tgiBlocks = new CountedTGIBlockList(handler, byte.MaxValue);
                _superBlocks = new SuperBlockList(handler, _tgiBlocks);
                _entries = new EntryBlockList(handler, _tgiBlocks);
            }
            public ContentType(EventHandler handler, Stream s) : base(handler) { Parse(s); }
            public ContentType(EventHandler handler, ContentType basis)
                : this(handler,
                basis._version, basis._superBlocks, basis._patternSize, basis._dataType, basis._unknown3, basis._unknown4, basis._entries, basis._tgiBlocks
                ) { }
            public ContentType(EventHandler handler,
                uint version, SuperBlockList superBlocks, PatternSizeType patternSize, DataTypeFlags dataType,
                byte unknown3, byte unknown4, EntryBlockList entries, CountedTGIBlockList tgiBlocks)
                : base(handler)
            {
                _tgiBlocks = tgiBlocks == null ? null : new CountedTGIBlockList(handler, tgiBlocks);
                _version = version;
                _superBlocks = superBlocks == null ? null : new SuperBlockList(handler, superBlocks, tgiBlocks);
                _patternSize = patternSize;
                _dataType = dataType;
                _unknown3 = unknown3;
                _unknown4 = unknown4;
                _entries = entries == null ? null : new EntryBlockList(handler, entries, tgiBlocks);
            }
            #endregion

            #region Data I/O
            void Parse(Stream s)
            {
                BinaryReader r = new BinaryReader(s);

                _version = r.ReadUInt32();
                long tgiPos = r.ReadUInt32() + s.Position;
                if (_version >= 7)
                    _superBlocks = new SuperBlockList(handler, s);
                _patternSize = (PatternSizeType)r.ReadUInt32();
                _dataType = (DataTypeFlags)r.ReadUInt32();
                _unknown3 = r.ReadByte();
                int count = r.ReadInt32();
                if (_version >= 8)
                    _unknown4 = r.ReadByte();
                _entries = new EntryBlockList(handler, count, s);
                if (tgiPos != s.Position)
                    throw new InvalidDataException($"TGI Block found at 0x{s.Position:X8}; expected position 0x{tgiPos:X8}");
                _tgiBlocks = new CountedTGIBlockList(handler, "IGT", r.ReadByte(), s, Byte.MaxValue);

                SetTgiBlocks();
            }

            internal void UnParse(Stream s)
            {
                BinaryWriter w = new BinaryWriter(s);

                w.Write(_version);
                long osetPos = s.Position;
                w.Write((uint)0);
                if (_tgiBlocks == null) _tgiBlocks = new CountedTGIBlockList(handler, "IGT", Byte.MaxValue);
                if (_version >= 7)
                {
                    if (_superBlocks == null) _superBlocks = new SuperBlockList(handler, _tgiBlocks);
                    _superBlocks.UnParse(s);
                }
                w.Write((uint)_patternSize);
                w.Write((uint)_dataType);
                w.Write(_unknown3);
                if (_entries == null) _entries = new EntryBlockList(handler);
                w.Write(_entries.Count);
                if (_version >= 8)
                    w.Write(_unknown4);
                _entries.UnParse(s);
                long tgiPosn = s.Position;
                w.Write((byte)_tgiBlocks.Count);
                _tgiBlocks.UnParse(s);

                long posn = s.Position;
                s.Position = osetPos;
                w.Write((uint)(tgiPosn - osetPos - sizeof(uint)));
                s.Position = posn;
            }
            #endregion

            #region AHandlerElement Members
            public override List<string> ContentFields
            {
                get
                {
                    List<string> res = GetContentFields(GetType());
                    res.Remove("ParentTGIBlocks");
                    if (_version < 8)
                    {
                        res.Remove("Unknown4");
                        if (_version < 7)
                        {
                            res.Remove("SuperBlocks");
                        }
                    }
                    return res;
                }
            }
            #endregion

            #region IEquatable<ContentType> Members

            public bool Equals(ContentType other)
            {
                return _version == other._version
                    && _superBlocks.Equals(other._superBlocks)
                    && _patternSize.Equals(other._patternSize)
                    && _dataType.Equals(other._dataType)
                    && _unknown3.Equals(other._unknown3)
                    && _unknown4.Equals(other._unknown4)
                    && _entries.Equals(other._entries)
                    && _tgiBlocks.Equals(other._tgiBlocks)
                    ;
            }

            public override bool Equals(object obj)
            {
                return obj as ContentType != null ? Equals(obj as ContentType) : false;
            }

            public override int GetHashCode()
            {
                return _version.GetHashCode()
                    ^ _superBlocks.GetHashCode()
                    ^ _patternSize.GetHashCode()
                    ^ _dataType.GetHashCode()
                    ^ _unknown3.GetHashCode()
                    ^ _unknown4.GetHashCode()
                    ^ _entries.GetHashCode()
                    ^ _tgiBlocks.GetHashCode()
                    ;
            }

            #endregion

            #region Content Fields
            [ElementPriority(1)]
            public uint Version { get => _version;
                set { if (_version != value) { _version = value; OnElementChanged(); } } }
            [ElementPriority(2)]
            public SuperBlockList SuperBlocks
            {
                get { if (_version < 0x00000007) throw new InvalidOperationException(); return _superBlocks; }
                set { if (_version < 0x00000007) throw new InvalidOperationException(); if (_superBlocks != value) { _superBlocks = value == null ? null : new SuperBlockList(handler, value) { ParentTGIBlocks = _tgiBlocks }; OnElementChanged(); } }
            }
            [ElementPriority(3)]
            public PatternSizeType PatternSize { get => _patternSize;
                set { if (_patternSize != value) { _patternSize = value; OnElementChanged(); } } }
            [ElementPriority(4)]
            public DataTypeFlags DataType { get => _dataType;
                set { if (_dataType != value) { _dataType = value; OnElementChanged(); } } }
            [ElementPriority(5)]
            public byte Unknown3 { get => _unknown3;
                set { if (_unknown3 != value) { _unknown3 = value; OnElementChanged(); } } }
            [ElementPriority(6)]
            public byte Unknown4
            {
                get { if (_version < 0x00000008) throw new InvalidOperationException(); return _unknown4; }
                set { if (_version < 0x00000008) throw new InvalidOperationException(); if (_unknown4 != value) { _unknown4 = value; OnElementChanged(); } }
            }
            [ElementPriority(7)]
            public EntryBlockList Entries { get => _entries;
                set { if (_entries != value) { _entries = value == null ? null : new EntryBlockList(handler, value) { ParentTGIBlocks = _tgiBlocks }; OnElementChanged(); } } }
            [ElementPriority(8)]
            public CountedTGIBlockList TgiBlocks { get => _tgiBlocks;
                set { if (_tgiBlocks != value) { _tgiBlocks = value == null ? null : new CountedTGIBlockList(handler, "IGT", value); SetTgiBlocks(); OnElementChanged(); } } }

            public string Value => ValueBuilder;

            #endregion
        }

        public class SuperBlock : AHandlerElement, IEquatable<SuperBlock>
        {
            DependentList<TGIBlock> _parentTgiBlocks;
            public DependentList<TGIBlock> ParentTGIBlocks
            {
                get => _parentTgiBlocks;
                set { if (_parentTgiBlocks != value) { _parentTgiBlocks = value; _fabric.ParentTGIBlocks = value; } }
            }
            public override List<string> ContentFields { get { List<string> res = GetContentFields(GetType()); res.Remove("ParentTGIBlocks"); return res; } }

            #region Attributes
            byte _tgiIndex;
            ContentType _fabric;
            byte _unknown1;
            byte _unknown2;
            byte _unknown3;
            #endregion

            #region Constructors
            public SuperBlock(EventHandler handler, DependentList<TGIBlock> parentTgiBlocks = null)
                : base(handler) { _parentTgiBlocks = parentTgiBlocks; _fabric = new ContentType(handler); }
            public SuperBlock(EventHandler handler, Stream s, DependentList<TGIBlock> parentTgiBlocks = null)
                : base(handler) { _parentTgiBlocks = parentTgiBlocks; Parse(s); }
            public SuperBlock(EventHandler handler, SuperBlock basis, DependentList<TGIBlock> parentTgiBlocks = null)
                : this(handler, basis._tgiIndex, basis._fabric, parentTgiBlocks ?? basis._parentTgiBlocks) { }
            public SuperBlock(EventHandler handler, byte tgiIndex, ContentType fabric,
                DependentList<TGIBlock> parentTgiBlocks = null)
                : base(handler) { _parentTgiBlocks = parentTgiBlocks; _tgiIndex = tgiIndex; _fabric = new ContentType(handler, fabric); }
            #endregion

            #region Data I/O
            // http://simswiki.info/wiki.php?title=Sims_3:0x033A1435
            void Parse(Stream s)
            {
                BinaryReader r = new BinaryReader(s);

                _tgiIndex = r.ReadByte();
                uint offset = r.ReadUInt32();
                long posn2 = s.Position;

                _fabric = new ContentType(handler, s);
                _unknown1 = r.ReadByte();
                _unknown2 = r.ReadByte();
                _unknown3 = r.ReadByte();

                if (posn2 + offset != s.Position)
                    throw new InvalidDataException(
                        "Unexpected data around position 0x" + posn2.ToString("X8") +
                        "; read 0x" + (s.Position - posn2).ToString("X8") + " bytes" +
                        "; expected 0x" + offset.ToString("X8") + " bytes."
                        );
            }

            internal void UnParse(Stream s)
            {
                BinaryWriter w = new BinaryWriter(s);

                w.Write(_tgiIndex);

                long before = s.Position;
                w.Write((uint)0);

                long posn = s.Position;
                _fabric.UnParse(s);
                w.Write(_unknown1);
                w.Write(_unknown2);
                w.Write(_unknown3);
                long after = s.Position;

                uint offset = (uint)(after - posn);

                s.Position = before;
                w.Write(offset);
                s.Position = after;
            }
            #endregion

            #region IEquatable<SuperBlock> Members

            public bool Equals(SuperBlock other)
            {
                return _tgiIndex == other._tgiIndex
                    && _fabric.Equals(other._fabric)
                    && _unknown1.Equals(other._unknown1)
                    && _unknown2.Equals(other._unknown2)
                    && _unknown3.Equals(other._unknown3)
                    ;
            }

            public override bool Equals(object obj)
            {
                return obj as SuperBlock != null ? Equals(obj as SuperBlock) : false;
            }

            public override int GetHashCode()
            {
                return _tgiIndex.GetHashCode()
                    ^ _fabric.GetHashCode()
                    ^ _unknown1.GetHashCode()
                    ^ _unknown2.GetHashCode()
                    ^ _unknown3.GetHashCode()
                    ;
            }

            #endregion

            #region Content Fields
            [ElementPriority(1), TGIBlockListContentField("ParentTGIBlocks")]
            public byte TgiIndex { get => _tgiIndex;
                set { if (_tgiIndex != value) { _tgiIndex = value; OnElementChanged(); } } }
            [ElementPriority(2)]
            public ContentType Fabric { get => _fabric;
                set { if (!_fabric.Equals(value)) { _fabric = new ContentType(handler, value); OnElementChanged(); } } }
            [ElementPriority(3)]
            public byte Unknown1 { get => _unknown1;
                set { if (_unknown1 != value) { _unknown1 = value; OnElementChanged(); } } }
            [ElementPriority(4)]
            public byte Unknown2 { get => _unknown2;
                set { if (_unknown2 != value) { _unknown2 = value; OnElementChanged(); } } }
            [ElementPriority(5)]
            public byte Unknown3 { get => _unknown3;
                set { if (_unknown3 != value) { _unknown3 = value; OnElementChanged(); } } }

            public string Value => ValueBuilder;

            #endregion
        }

        public class SuperBlockList : DependentList<SuperBlock>
        {
            private DependentList<TGIBlock> _parentTgiBlocks;
            public DependentList<TGIBlock> ParentTGIBlocks
            {
                get => _parentTgiBlocks;
                set { if (_parentTgiBlocks != value) { _parentTgiBlocks = value; foreach (SuperBlock i in this) i.ParentTGIBlocks = _parentTgiBlocks; } }
            }

            public SuperBlockList(EventHandler handler, DependentList<TGIBlock> parentTgiBlocks = null)
                : base(handler, Byte.MaxValue) { _parentTgiBlocks = parentTgiBlocks; }
            public SuperBlockList(EventHandler handler, Stream s, DependentList<TGIBlock> parentTgiBlocks = null)
                : this(null, parentTgiBlocks) { elementHandler = handler; Parse(s); this.handler = handler; }
            public SuperBlockList(EventHandler handler, IEnumerable<SuperBlock> lsb, DependentList<TGIBlock> parentTgiBlocks = null)
                : this(null, parentTgiBlocks) { elementHandler = handler; foreach (SuperBlock t in lsb) Add((SuperBlock)t.Clone(null)); this.handler = handler; }

            protected override int ReadCount(Stream s) { return new BinaryReader(s).ReadByte(); }
            protected override SuperBlock CreateElement(Stream s) { return new SuperBlock(elementHandler, s, _parentTgiBlocks); }

            protected override void WriteCount(Stream s, int count) { new BinaryWriter(s).Write((byte)count); }
            protected override void WriteElement(Stream s, SuperBlock element) { element.UnParse(s); }

            public override void Add() { Add(new SuperBlock(handler, _parentTgiBlocks)); }
            public override void Add(SuperBlock item) { item.ParentTGIBlocks = _parentTgiBlocks; base.Add(item); }
        }

        #region Entry
        public abstract class Entry : AHandlerElement, IEquatable<Entry>
        {
            #region Attributes
            protected uint property;
            private Type _enumType;
            protected byte unknown;
            private byte _dataType;
            #endregion

            #region Constructors
            public Entry(EventHandler handler, uint property, Type enumType, byte unknown, byte dataType)
                : base(handler)
            {
                if (enumType != null && !Enum.IsDefined(enumType, property))
                    throw new InvalidDataException($"Unexpected property ID 0x{property:X8} for enumType {enumType.Name}");
                this.property = property; _enumType = enumType; this.unknown = unknown; _dataType = dataType;
            }

            public static Entry CreateEntry(EventHandler handler, Stream s, DependentList<TGIBlock> parentTgiBlocks)
            {
                BinaryReader r = new BinaryReader(s);
                uint property = r.ReadUInt32();
                if (property == 0)
                    return new EntryNull(handler);

                byte unknown = r.ReadByte();
                byte dataType = r.ReadByte();
                switch (dataType)
                {
                    // bytes
                    case 0x00: return new EntryBoolean(handler, (EntryBoolean.BooleanProperties)property, unknown, r.ReadByte());
                    case 0x01: return new EntrySByte(handler, property, unknown, r.ReadSByte());
                    case 0x05: return new EntryByte(handler, property, unknown, r.ReadByte());
                    case 0x0C: return new EntryTgiIndex(handler, (EntryTgiIndex.TgiIndexProperties)property, unknown, r.ReadByte(), parentTgiBlocks);
                    // words
                    case 0x02: return new EntryInt16(handler, property, unknown, r.ReadInt16());
                    case 0x06: return new EntryUInt16(handler, property, unknown, r.ReadUInt16());
                    // dwords
                    case 0x03: return new EntryInt32(handler, (EntryInt32.Int32Properties)property, unknown, r.ReadInt32());
                    case 0x07: return new EntryUInt32(handler, (EntryUInt32.UInt32Properties)property, unknown, r.ReadUInt32());
                    // qwords
                    case 0x04: return new EntryInt64(handler, property, unknown, r.ReadInt64());
                    case 0x08: return new EntryUInt64(handler, property, unknown, r.ReadUInt64());
                    // float
                    case 0x09: return new EntrySingle(handler, (EntrySingle.SingleProperties)property, unknown, r.ReadSingle());
                    // rectangle
                    case 0x0A: return new EntryRectangle(handler, (EntryRectangle.RectangleProperties)property, unknown, r);
                    // vector
                    case 0x0B: return new EntryVector(handler, (EntryVector.VectorProperties)property, unknown, r);
                    // String
                    case 0x0D: return new EntryString(handler, (EntryString.StringProperties)property, unknown, new String(r.ReadChars(r.ReadUInt16())));
                    default:
                        throw new InvalidDataException($"Unsupported data type 0x{dataType:X2} at 0x{s.Position:X8}");
                }
            }
            #endregion

            #region Data I/O
            internal virtual void UnParse(Stream s)
            {
                BinaryWriter w = new BinaryWriter(s);
                w.Write(property);
                if (property == 0)
                    return;
                w.Write(unknown);
                w.Write(_dataType);
            }
            #endregion

            #region AHandlerElement Members
            
            public override List<string> ContentFields => GetContentFields(GetType());

            #endregion

            #region IEquatable<Entry> Members

            public bool Equals(Entry other)
            {
                MemoryStream thisMs = new MemoryStream();
                UnParse(thisMs);
                MemoryStream otherMs = new MemoryStream();
                other.UnParse(otherMs);
                return thisMs.ToArray().Equals(otherMs.ToArray());
            }

            public override bool Equals(object obj) => obj is Entry entry && Equals(entry);

            public override int GetHashCode()
            {
                MemoryStream thisMs = new MemoryStream();
                UnParse(thisMs);
                return thisMs.ToArray().GetHashCode();
            }

            #endregion

            #region Content Fields
            [ElementPriority(2)]
            public byte Unknown { get => unknown;
                set { if (unknown != value) { unknown = value; OnElementChanged(); } } }
            //-this is linked to the subclass of Entry, so should not be editable
            //[ElementPriority(3)]
            //public byte DataType { get { return dataType; } set { if (dataType != value) { dataType = value; OnElementChanged(); } } }

            protected abstract string EntryValue { get; }
            public virtual string Value =>
                (_enumType == null
                    ? new TypedValue(typeof(uint), property, "X")
                    : new TypedValue(_enumType, Enum.ToObject(_enumType, property), "X"))
                + "; " + EntryValue;

            #endregion
        }

        public class EntryNull : Entry
        {
            public EntryNull(EventHandler handler, EntryNull basis)
                : base(handler, 0, null, 0, 0) { throw new NotImplementedException(); }
            public EntryNull(EventHandler handler)
                : base(handler, 0, null, 0, 0) { }
            internal override void UnParse(Stream s) { throw new NotImplementedException(); }
            public override AHandlerElement Clone(EventHandler handler) { throw new NotImplementedException(); }
            protected override string EntryValue => null;
            public override string Value => throw new NotImplementedException();
        }
        public class EntryBoolean : Entry
        {
            public enum BooleanProperties : uint
            {
                //EntryBoolean
                Unknown0X655Fd973 = 0x655FD973,
                Unknown0XD49E8879 = 0xD49E8879,
                UiVisible = 0xD92A4C8B,
                EnableFiltering = 0xE27FE962,
                EnableBlending = 0xFBF310C7,
            }

            byte _data;
            public EntryBoolean(EventHandler handler) : this(handler, BooleanProperties.UiVisible, 0, 0) { }
            public EntryBoolean(EventHandler handler, EntryBoolean basis) : this(handler, (BooleanProperties)basis.property, basis.unknown, basis._data) { }
            public EntryBoolean(EventHandler handler, BooleanProperties property, byte unknown, byte data) : base(handler, (uint)property, typeof(BooleanProperties), unknown, 0x00) { _data = data; }
            internal override void UnParse(Stream s) { base.UnParse(s); new BinaryWriter(s).Write(_data); }
            [ElementPriority(1)]
            public BooleanProperties Property
            {
                get => (BooleanProperties)property;
                set
                {
                    if (property != (uint)value)
                    {
                        if (!Enum.IsDefined(typeof(BooleanProperties), value))
                            throw new ArgumentException($"Unexpected property ID 0x{(uint) value:X8}");
                        property = (uint)value;
                        OnElementChanged();
                    }
                }
            }
            [ElementPriority(4)]
            public byte Data { get => _data;
                set { if (_data != value) { _data = value; OnElementChanged(); } } }
            protected override string EntryValue => "" + (_data != 0);
        }
        public class EntrySByte : Entry
        {
            sbyte _data;
            public EntrySByte(EventHandler handler) : this(handler, 1, 0, 0) { }
            public EntrySByte(EventHandler handler, EntrySByte basis) : this(handler, basis.property, basis.unknown, basis._data) { }
            public EntrySByte(EventHandler handler, uint property, byte unknown, sbyte data) : base(handler, property, null, unknown, 0x01) { _data = data; }
            internal override void UnParse(Stream s) { base.UnParse(s); new BinaryWriter(s).Write(_data); }
            [ElementPriority(1)]
            public uint Property { get => property;
                set { if (property != value) { property = value; OnElementChanged(); } } }
            [ElementPriority(4)]
            public sbyte Data { get => _data;
                set { if (_data != value) { _data = value; OnElementChanged(); } } }
            protected override string EntryValue => "0x" + _data.ToString("X2");
        }
        public class EntryByte : Entry
        {
            byte _data;
            public EntryByte(EventHandler handler) : this(handler, 5, 0, 0) { }
            public EntryByte(EventHandler handler, EntryByte basis) : this(handler, basis.property, basis.unknown, basis._data) { }
            public EntryByte(EventHandler handler, uint property, byte unknown, byte data) : base(handler, property, null, unknown, 0x05) { _data = data; }
            internal override void UnParse(Stream s) { base.UnParse(s); new BinaryWriter(s).Write(_data); }
            [ElementPriority(1)]
            public uint Property { get => property;
                set { if (property != value) { property = value; OnElementChanged(); } } }
            [ElementPriority(4)]
            public byte Data { get => _data;
                set { if (_data != value) { _data = value; OnElementChanged(); } } }
            protected override string EntryValue => "0x" + _data.ToString("X2");
        }
        public class EntryTgiIndex : Entry
        {
            public enum TgiIndexProperties : uint
            {
                //EntryTGIIndex
                MaskKey = 0x49DE3B16,
                DefaultFabric = 0xDCFF6D7B,
                ImageKey = 0xF6CC8471,
            }

            public DependentList<TGIBlock> ParentTGIBlocks { get; set; }
            public override List<string> ContentFields { get { List<string> res = base.ContentFields; res.Remove("ParentTGIBlocks"); return res; } }

            byte _data;

            public EntryTgiIndex(EventHandler handler, DependentList<TGIBlock> parentTgiBlocks = null)
                : this(handler, TgiIndexProperties.MaskKey, 0, 0, parentTgiBlocks) { }
            public EntryTgiIndex(EventHandler handler, EntryTgiIndex basis, DependentList<TGIBlock> parentTgiBlocks = null)
                : this(handler, (TgiIndexProperties)basis.property, basis.unknown, basis._data, parentTgiBlocks ?? basis.ParentTGIBlocks) { }
            public EntryTgiIndex(EventHandler handler, TgiIndexProperties property, byte unknown, byte data, DependentList<TGIBlock> parentTgiBlocks = null)
                : base(handler, (uint)property, typeof(TgiIndexProperties), unknown, 0x0C) { ParentTGIBlocks = parentTgiBlocks; _data = data; }

            internal override void UnParse(Stream s) { base.UnParse(s); new BinaryWriter(s).Write(_data); }
            [ElementPriority(1)]
            public TgiIndexProperties Property
            {
                get => (TgiIndexProperties)property;
                set
                {
                    if (property != (uint)value)
                    {
                        if (!Enum.IsDefined(typeof(TgiIndexProperties), value))
                            throw new ArgumentException($"Unexpected property ID 0x{(uint) value:X8}");
                        property = (uint)value;
                        OnElementChanged();
                    }
                }
            }
            [ElementPriority(4), TGIBlockListContentField("ParentTGIBlocks")]
            public byte Data { get => _data;
                set { if (_data != value) { _data = value; OnElementChanged(); } } }
            protected override string EntryValue =>
                $"0x{_data:X2} ({(ParentTGIBlocks == null || _data >= ParentTGIBlocks.Count ? "unknown" : ParentTGIBlocks[_data])})";
        }
        public class EntryInt16 : Entry
        {
            Int16 _data;
            public EntryInt16(EventHandler handler) : this(handler, 2, 0, 0) { }
            public EntryInt16(EventHandler handler, EntryInt16 basis) : this(handler, basis.property, basis.unknown, basis._data) { }
            public EntryInt16(EventHandler handler, uint property, byte unknown, Int16 data) : base(handler, property, null, unknown, 0x02) { _data = data; }
            internal override void UnParse(Stream s) { base.UnParse(s); new BinaryWriter(s).Write(_data); }
            [ElementPriority(1)]
            public uint Property { get => property;
                set { if (property != value) { property = value; OnElementChanged(); } } }
            [ElementPriority(4)]
            public Int16 Data { get => _data;
                set { if (_data != value) { _data = value; OnElementChanged(); } } }
            protected override string EntryValue => "0x" + _data.ToString("X4");
        }
        public class EntryUInt16 : Entry
        {
            UInt16 _data;
            public EntryUInt16(EventHandler handler) : this(handler, 6, 0, 0) { }
            public EntryUInt16(EventHandler handler, EntryUInt16 basis) : this(handler, basis.property, basis.unknown, basis._data) { }
            public EntryUInt16(EventHandler handler, uint property, byte unknown, UInt16 data) : base(handler, property, null, unknown, 0x06) { _data = data; }
            internal override void UnParse(Stream s) { base.UnParse(s); new BinaryWriter(s).Write(_data); }
            [ElementPriority(1)]
            public uint Property { get => property;
                set { if (property != value) { property = value; OnElementChanged(); } } }
            [ElementPriority(4)]
            public UInt16 Data { get => _data;
                set { if (_data != value) { _data = value; OnElementChanged(); } } }
            protected override string EntryValue => "0x" + _data.ToString("X4");
        }
        public class EntryInt32 : Entry
        {
            public enum Int32Properties : uint
            {
                DestinationBlend = 0x048F7567,
                SkipShaderModel = 0x06A775CE,
                MinShaderModel = 0x2EDF5F53,
                ColorWrite = 0xB07B3B93,
                SourceBlend = 0xE055EE36,
            }

            // see http://msdn.microsoft.com/en-us/library/microsoft.xna.framework.graphics.blend.aspx
            public enum Blend : int
            {
                Zero,
                One,
                SourceColor,
                InverseSourceColor,
                SourceAlpha,
                InverseSourceAlpha,
                DestinationAlpha,
                InverseDestinationAlpha,
                DestinationColor,
                InverseDestinationColor,
                SourceAlphaSaturation,
                BlendFactor,
                InverseBlendFactor,
            }

            public enum ShaderModel : int
            {
                Sm10 = 0x00000000,
                Sm11 = 0x00000001,
                Sm20 = 0x00000002,
                Sm21 = 0x00000003,
                SmHighest = 0x7FFFFFFF,
            }

            [Flags]
            public enum ColorWriteChannels : int
            {
                Red = 0x01,
                Green = 0x02,
                Blue = 0x04,
                Alpha = 0x08,
                // -
                //None = 0x00,
                //Color = 0x07,
                //All = 0x0F,
            }

            Int32 _data;
            public EntryInt32(EventHandler handler) : this(handler, Int32Properties.DestinationBlend, 0, 0) { }
            public EntryInt32(EventHandler handler, EntryInt32 basis) : this(handler, (Int32Properties)basis.property, basis.unknown, basis._data) { }
            public EntryInt32(EventHandler handler, Int32Properties property, byte unknown, Int32 data) : base(handler, (uint)property, typeof(Int32Properties), unknown, 0x03) { _data = data; }
            internal override void UnParse(Stream s) { base.UnParse(s); new BinaryWriter(s).Write(_data); }
            [ElementPriority(1)]
            public Int32Properties Property
            {
                get => (Int32Properties)property;
                set
                {
                    if (property != (uint)value)
                    {
                        if (!Enum.IsDefined(typeof(Int32Properties), value))
                            throw new ArgumentException($"Unexpected property ID 0x{(uint) value:X8}");
                        property = (uint)value;
                        OnElementChanged();
                    }
                }
            }
            [ElementPriority(4)]
            public Int32 Data { get => _data;
                set { if (_data != value) { _data = value; OnElementChanged(); } } }
            protected override string EntryValue
            {
                get
                {
                    switch ((Int32Properties)property)
                    {
                        case Int32Properties.DestinationBlend:
                        case Int32Properties.SourceBlend:
                            return new TypedValue(typeof(Blend), Enum.ToObject(typeof(Blend), _data), "X") + "";
                        case Int32Properties.SkipShaderModel:
                            return new TypedValue(typeof(ShaderModel), Enum.ToObject(typeof(ShaderModel), _data), "X") + "";
                        case Int32Properties.ColorWrite:
                            return new TypedValue(typeof(ColorWriteChannels), Enum.ToObject(typeof(ColorWriteChannels), _data), "X") + "";
                        default:
                            return "0x" + _data.ToString("X8");
                    }
                }
            }
        }
        public class EntryUInt32 : Entry
        {
            public enum UInt32Properties : uint
            {
                MaskSource = 0x10DA0B6A,
                Width = 0x182E64EB,
                SkipDetailLevel = 0x331178DF,
                Height = 0x4C47D5C0,
                DefaultColor = 0x64399EC5,
                Id = 0x687720A6,
                ImageSource = 0x8A7006DB,
                RenderTarget = 0xA2C91332,
                MinDetailLevel = 0xAE5FE82A,
                Color = 0xB01748DA,
            }

            public enum DetailLevel : uint
            {
                Lowest = 0x00000000,
                Highest = 0x00000003,
            }

            public enum StepType : uint
            {
                DrawFabric = 0x034210A5,
                ChannelSelect = 0x1E363B9B,
                SkinTone = 0x43B554E3,
                HairTone = 0x5D7C85D4,
                RemappedChannelSelect = 0x890805DB,
                ColorFill = 0x9CD1269D,
                DrawImage = 0xA15200B1,
                CasPickData = 0xC6B6AC1F,
                SetTarget = 0xD6BD8695,
                HsVtoRgb = 0xDC0984B9,
            }

            public enum RenderTarget : uint
            {
                RenderTargetA = 0x21E9CD2,
                RenderTargetB = 0x21E9CD4,
            }


            UInt32 _data;
            public EntryUInt32(EventHandler handler) : this(handler, UInt32Properties.Width, 0, 0) { }
            public EntryUInt32(EventHandler handler, EntryUInt32 basis) : this(handler, (UInt32Properties)basis.property, basis.unknown, basis._data) { }
            public EntryUInt32(EventHandler handler, UInt32Properties property, byte unknown, UInt32 data) : base(handler, (uint)property, typeof(UInt32Properties), unknown, 0x07) { _data = data; }
            internal override void UnParse(Stream s) { base.UnParse(s); new BinaryWriter(s).Write(_data); }
            [ElementPriority(1)]
            public UInt32Properties Property
            {
                get => (UInt32Properties)property;
                set
                {
                    if (property != (uint)value)
                    {
                        if (!Enum.IsDefined(typeof(UInt32Properties), value))
                            throw new ArgumentException($"Unexpected property ID 0x{(uint) value:X8}");
                        property = (uint)value;
                        OnElementChanged();
                    }
                }
            }
            [ElementPriority(4)]
            public UInt32 Data { get => _data;
                set { if (_data != value) { _data = value; OnElementChanged(); } } }
            protected override string EntryValue
            {
                get
                {
                    switch ((UInt32Properties)property)
                    {
                        case UInt32Properties.SkipDetailLevel:
                        case UInt32Properties.MinDetailLevel:
                            return new TypedValue(typeof(DetailLevel), Enum.ToObject(typeof(DetailLevel), _data), "X") + "";
                        case UInt32Properties.Id:
                            return new TypedValue(typeof(StepType), Enum.ToObject(typeof(StepType), _data), "X") + "";
                        case UInt32Properties.ImageSource:
                        case UInt32Properties.RenderTarget:
                            return new TypedValue(typeof(RenderTarget), Enum.ToObject(typeof(RenderTarget), _data), "X") + "";
                        default:
                            return "0x" + _data.ToString("X8");
                    }
                }
            }
        }
        public class EntryInt64 : Entry
        {
            Int64 _data;
            public EntryInt64(EventHandler handler) : this(handler, 4, 0, 0) { }
            public EntryInt64(EventHandler handler, EntryInt64 basis) : this(handler, basis.property, basis.unknown, basis._data) { }
            public EntryInt64(EventHandler handler, uint property, byte unknown, Int64 data) : base(handler, property, null, unknown, 0x04) { _data = data; }
            internal override void UnParse(Stream s) { base.UnParse(s); BinaryWriter w = new BinaryWriter(s); w.Write(property); w.Write(_data); }
            [ElementPriority(1)]
            public uint Property { get => property;
                set { if (property != value) { property = value; OnElementChanged(); } } }
            [ElementPriority(4)]
            public Int64 Data { get => _data;
                set { if (_data != value) { _data = value; OnElementChanged(); } } }
            protected override string EntryValue => "0x" + _data.ToString("X16");
        }
        public class EntryUInt64 : Entry
        {
            UInt64 _data;
            public EntryUInt64(EventHandler handler) : this(handler, 8, 0, 0) { }
            public EntryUInt64(EventHandler handler, EntryUInt64 basis) : this(handler, basis.property, basis.unknown, basis._data) { }
            public EntryUInt64(EventHandler handler, uint property, byte unknown, ulong data) : base(handler, property, null, unknown, 0x08) { _data = data; }
            internal override void UnParse(Stream s) { base.UnParse(s); BinaryWriter w = new BinaryWriter(s); w.Write(property); w.Write(_data); }
            [ElementPriority(1)]
            public uint Property { get => property;
                set { if (property != value) { property = value; OnElementChanged(); } } }
            [ElementPriority(4)]
            public UInt64 Data { get => _data;
                set { if (_data != value) { _data = value; OnElementChanged(); } } }
            protected override string EntryValue => "0x" + _data.ToString("X16");
        }
        public class EntrySingle : Entry
        {
            public enum SingleProperties : uint
            {
                MaskBias = 0x3A3260E6,
                Rotation = 0x49F996DB,
            }

            Single _data;
            public EntrySingle(EventHandler handler) : this(handler, SingleProperties.MaskBias, 0, 0) { }
            public EntrySingle(EventHandler handler, EntrySingle basis) : this(handler, (SingleProperties)basis.property, basis.unknown, basis._data) { }
            public EntrySingle(EventHandler handler, SingleProperties property, byte unknown, Single data) : base(handler, (uint)property, typeof(SingleProperties), unknown, 0x09) { _data = data; }
            internal override void UnParse(Stream s) { base.UnParse(s); new BinaryWriter(s).Write(_data); }
            [ElementPriority(1)]
            public SingleProperties Property
            {
                get => (SingleProperties)property;
                set
                {
                    if (property != (uint)value)
                    {
                        if (!Enum.IsDefined(typeof(SingleProperties), value))
                            throw new ArgumentException($"Unexpected property ID 0x{(uint) value:X8}");
                        property = (uint)value;
                        OnElementChanged();
                    }
                }
            }
            [ElementPriority(4)]
            public Single Data { get => _data;
                set { if (_data != value) { _data = value; OnElementChanged(); } } }
            protected override string EntryValue => _data.ToString("F4");
        }
        public class EntryRectangle : Entry
        {
            public enum RectangleProperties : uint
            {
                SourceRectangle = 0xA3AAFC98,
                DestinationRectangle = 0xE1D6D01F,
            }

            Single[] _data = new Single[4];
            public EntryRectangle(EventHandler handler) : this(handler, RectangleProperties.SourceRectangle, 0, new Single[] { 0, 0, 0, 0 }) { }
            public EntryRectangle(EventHandler handler, RectangleProperties property, byte unknown, BinaryReader r) : this(handler, property, unknown, new Single[4]) { for (int i = 0; i < _data.Length; i++) _data[i] = r.ReadSingle(); }
            public EntryRectangle(EventHandler handler, EntryRectangle basis) : this(handler, (RectangleProperties)basis.property, basis.unknown, basis._data) { }
            public EntryRectangle(EventHandler handler, RectangleProperties property, byte unknown, Single[] data) : base(handler, (uint)property, typeof(RectangleProperties), unknown, 0x0A) { Array.Copy(data, _data, Math.Max(data.Length, _data.Length)); }
            internal override void UnParse(Stream s) { base.UnParse(s); BinaryWriter w = new BinaryWriter(s); for (int i = 0; i < _data.Length; i++) w.Write(_data[i]); }
            [ElementPriority(1)]
            public RectangleProperties Property
            {
                get => (RectangleProperties)property;
                set
                {
                    if (property != (uint)value)
                    {
                        if (!Enum.IsDefined(typeof(RectangleProperties), value))
                            throw new ArgumentException($"Unexpected property ID 0x{(uint) value:X8}");
                        property = (uint)value;
                        OnElementChanged();
                    }
                }
            }
            [ElementPriority(4)]
            public Single[] Data { get => (Single[])_data.Clone();
                set { if (value.Length != _data.Length) throw new ArgumentLengthException(); if (!_data.Equals(value)) { _data = (Single[])value.Clone(); OnElementChanged(); } } }
            protected override string EntryValue => $"{_data[0]:F4}, {_data[1]:F4}; {_data[2]:F4}, {_data[3]:F4}";
        }
        public class EntryVector : Entry
        {
            public enum VectorProperties : uint
            {
                MaskSelect = 0x1F091259,
                HsvShift = 0xB67C2EF8,
                ChannelSelect = 0xD0E69002,
            }

            Single[] _data = new Single[4];
            public EntryVector(EventHandler handler) : this(handler, VectorProperties.MaskSelect, 0, new Single[] { 0, 0, 0, 0 }) { }
            public EntryVector(EventHandler handler, VectorProperties property, byte unknown, BinaryReader r) : this(handler, property, unknown, new Single[4]) { for (int i = 0; i < _data.Length; i++) _data[i] = r.ReadSingle(); }
            public EntryVector(EventHandler handler, EntryVector basis) : this(handler, (VectorProperties)basis.property, basis.unknown, basis._data) { }
            public EntryVector(EventHandler handler, VectorProperties property, byte unknown, Single[] data) : base(handler, (uint)property, typeof(VectorProperties), unknown, 0x0B) { Array.Copy(data, _data, Math.Max(data.Length, _data.Length)); }
            internal override void UnParse(Stream s) { base.UnParse(s); BinaryWriter w = new BinaryWriter(s); for (int i = 0; i < _data.Length; i++) w.Write(_data[i]); }
            [ElementPriority(1)]
            public VectorProperties Property
            {
                get => (VectorProperties)property;
                set
                {
                    if (property != (uint)value)
                    {
                        if (!Enum.IsDefined(typeof(VectorProperties), value))
                            throw new ArgumentException($"Unexpected property ID 0x{(uint) value:X8}");
                        property = (uint)value;
                        OnElementChanged();
                    }
                }
            }
            [ElementPriority(4)]
            public Single[] Data { get => (Single[])_data.Clone();
                set { if (value.Length != _data.Length) throw new ArgumentLengthException(); if (!_data.Equals(value)) { _data = (Single[])value.Clone(); OnElementChanged(); } } }
            protected override string EntryValue => $"{_data[0]:F4}, {_data[1]:F4}, {_data[2]:F4}, {_data[3]:F4}";
        }
        public class EntryString : Entry
        {
            public enum StringProperties : uint
            {
                Description = 0x6B7119C1,
            }
            String _data;
            public EntryString(EventHandler handler) : this(handler, StringProperties.Description, 0, "") { }
            public EntryString(EventHandler handler, EntryString basis) : this(handler, (StringProperties)basis.property, basis.unknown, basis._data) { }
            public EntryString(EventHandler handler, StringProperties property, byte unknown, String data) : base(handler, (uint)property, typeof(StringProperties), unknown, 0x0D) { _data = data; }
            internal override void UnParse(Stream s) { base.UnParse(s); BinaryWriter w = new BinaryWriter(s); w.Write((UInt16)_data.Length); w.Write(_data.ToCharArray()); }
            [ElementPriority(1)]
            public StringProperties Property
            {
                get => (StringProperties)property;
                set
                {
                    if (property != (uint)value)
                    {
                        if (!Enum.IsDefined(typeof(StringProperties), value))
                            throw new ArgumentException($"Unexpected property ID 0x{(uint) value:X8}");
                        property = (uint)value;
                        OnElementChanged();
                    }
                }
            }
            [ElementPriority(4)]
            public String Data { get => _data;
                set { if (_data != value) { _data = value; OnElementChanged(); } } }
            protected override string EntryValue => _data;
        }

        public class EntryList : DependentList<Entry>
        {
            private DependentList<TGIBlock> _parentTgiBlocks;
            public DependentList<TGIBlock> ParentTGIBlocks
            {
                get => _parentTgiBlocks;
                set { if (_parentTgiBlocks != value) { _parentTgiBlocks = value; foreach (EntryTgiIndex i in FindAll(e => e is EntryTgiIndex)) i.ParentTGIBlocks = _parentTgiBlocks; } }
            }

            public EntryList(EventHandler handler, DependentList<TGIBlock> parentTgiBlocks = null)
                : base(handler) { _parentTgiBlocks = parentTgiBlocks; }
            public EntryList(EventHandler handler, Stream s, DependentList<TGIBlock> parentTgiBlocks = null)
                : this(null, parentTgiBlocks) { elementHandler = handler; Parse(s); this.handler = handler; }
            public EntryList(EventHandler handler, IEnumerable<Entry> le, DependentList<TGIBlock> parentTgiBlocks = null)
                : this(null, parentTgiBlocks) { elementHandler = handler; foreach (Entry t in le) Add((Entry)t.Clone(null)); this.handler = handler; }

            protected void Parse(Stream s)
            {
                for (Entry e = Entry.CreateEntry(elementHandler, s, _parentTgiBlocks); !(e is EntryNull); e = Entry.CreateEntry(elementHandler, s, _parentTgiBlocks))
                    Add(e);
            }

            protected override int ReadCount(Stream s) { throw new InvalidOperationException(); }
            protected override Entry CreateElement(Stream s) { throw new InvalidOperationException(); }

            protected override void WriteCount(Stream s, int count) { } // List owner must do this, if required
            protected override void WriteElement(Stream s, Entry element) { element.UnParse(s); }

            public override void Add(Entry item) { if (item is EntryTgiIndex) (item as EntryTgiIndex).ParentTGIBlocks = _parentTgiBlocks; base.Add(item); }
            public override void Add(Type elementType)
            {
                if (elementType.IsAbstract)
                    throw new ArgumentException("Must pass a concrete element type.", "elementType");

                if (!typeof(Entry).IsAssignableFrom(elementType))
                    throw new ArgumentException("The element type must belong to the generic type of the list.", "elementType");

                Entry newElement;
                if (elementType == typeof(EntryTgiIndex))
                    newElement = new EntryTgiIndex(elementHandler, _parentTgiBlocks);
                else
                    newElement = Activator.CreateInstance(elementType, new object[] { elementHandler }) as Entry;
                base.Add(newElement);
            }
        }
        #endregion

        public class EntryBlock : AHandlerElement, IEquatable<EntryBlock>
        {
            DependentList<TGIBlock> _parentTgiBlocks;
            public DependentList<TGIBlock> ParentTGIBlocks
            {
                get => _parentTgiBlocks;
                set { if (_parentTgiBlocks != value) { _parentTgiBlocks = value; if (_theList != null) _theList.ParentTGIBlocks = ParentTGIBlocks; } }
            }
            public override List<string> ContentFields { get { List<string> res = GetContentFields(GetType()); res.Remove("ParentTGIBlocks"); return res; } }

            EntryList _theList;

            public EntryBlock(EventHandler handler, DependentList<TGIBlock> parentTgiBlocks = null)
                : base(handler) { _parentTgiBlocks = parentTgiBlocks; _theList = new EntryList(handler, _parentTgiBlocks); }
            public EntryBlock(EventHandler handler, Stream s, DependentList<TGIBlock> parentTgiBlocks = null)
                : base(handler) { _parentTgiBlocks = parentTgiBlocks; _theList = new EntryList(handler, s, _parentTgiBlocks); }
            public EntryBlock(EventHandler handler, EntryBlock basis, DependentList<TGIBlock> parentTgiBlocks = null)
                : base(handler) { _parentTgiBlocks = parentTgiBlocks ?? basis._parentTgiBlocks; _theList = new EntryList(handler, basis._theList, _parentTgiBlocks); }

            internal void UnParse(Stream s) { _theList.UnParse(s); new BinaryWriter(s).Write((uint)0); }

            #region IEquatable<EntryBlock> Members

            public bool Equals(EntryBlock other) { return _theList.Equals(other._theList); }
            public override bool Equals(object obj)
            {
                return obj as EntryBlock != null ? Equals(obj as EntryBlock) : false;
            }
            public override int GetHashCode()
            {
                return _theList.GetHashCode();
            }

            #endregion

            #region Content Fields
            public EntryList Entries { get => _theList;
                set { if (_theList != value) { _theList = new EntryList(handler, value, _parentTgiBlocks); OnElementChanged(); } } }

            public string Value => ValueBuilder;

            #endregion
        }

        public class EntryBlockList : DependentList<EntryBlock>
        {
            private DependentList<TGIBlock> _parentTgiBlocks;
            public DependentList<TGIBlock> ParentTGIBlocks
            {
                get => _parentTgiBlocks;
                set { if (_parentTgiBlocks != value) { _parentTgiBlocks = value; foreach (EntryBlock i in this) i.ParentTGIBlocks = _parentTgiBlocks; } }
            }

            int _blockCount;
            public EntryBlockList(EventHandler handler, DependentList<TGIBlock> parentTgiBlocks = null)
                : base(handler) { _parentTgiBlocks = parentTgiBlocks; }
            public EntryBlockList(EventHandler handler, int blockCount, Stream s, DependentList<TGIBlock> parentTgiBlocks = null)
                : this(null, parentTgiBlocks) { elementHandler = handler; _blockCount = blockCount; Parse(s); this.handler = handler; }
            public EntryBlockList(EventHandler handler, IEnumerable<EntryBlock> leb, DependentList<TGIBlock> parentTgiBlocks = null)
                : this(null, parentTgiBlocks) { elementHandler = handler; foreach (EntryBlock t in leb) Add((EntryBlock)t.Clone(null)); this.handler = handler; }

            protected override int ReadCount(Stream s) { return _blockCount; }
            protected override EntryBlock CreateElement(Stream s) { return new EntryBlock(elementHandler, s, _parentTgiBlocks); }

            protected override void WriteCount(Stream s, int count) { } // List owner must do this
            protected override void WriteElement(Stream s, EntryBlock element) { element.UnParse(s); }

            public override void Add() { Add(new EntryBlock(handler, _parentTgiBlocks)); }
            public override void Add(EntryBlock item) { item.ParentTGIBlocks = _parentTgiBlocks; base.Add(item); }
        }
        #endregion

        #region Content Fields
        [ElementPriority(1)]
        public ContentType Root { get => _root;
            set { if (!_root.Equals(value)) { _root = new ContentType(OnResourceChanged, value); OnResourceChanged(this, EventArgs.Empty); } } }

        public string Value => ValueBuilder;

        #endregion
    }
    
    /// <summary>
    /// ResourceHandler for TxtcResource wrapper
    /// </summary>
    public class TxtcResourceHandler : AResourceHandler
    {
        public TxtcResourceHandler()
        {
            Add(typeof(TxtcResource), new List<string>(new[] { "0x033A1435", "0x0341ACC9", }));
        }
    }
}