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
    public class ModularResource : AResource
    {
        public override List<string> ContentFields => GetContentFields(GetType());

        #region Attributes
        ushort _unknown1;
        ushort _unknown2;
        Int32IndexList _tgiIndexes;
        TGIBlockList _tgiBlocks;
        #endregion

        public ModularResource(Stream s) : base(s) { if (stream == null) { stream = UnParse(); OnResourceChanged(this, EventArgs.Empty); } stream.Position = 0; Parse(stream); }

        #region Data I/O
        void Parse(Stream s)
        {
            long tgiPosn, tgiSize;
            BinaryReader r = new BinaryReader(s);

            _unknown1 = r.ReadUInt16();
            tgiPosn = r.ReadUInt32() + s.Position;
            tgiSize = r.ReadUInt32();
            _unknown2 = r.ReadUInt16();
            _tgiIndexes = new Int32IndexList(OnResourceChanged, s, Int16.MaxValue, ReadInt16, WriteInt16);
            _tgiBlocks = new TGIBlockList(OnResourceChanged, s, tgiPosn, tgiSize);

            _tgiIndexes.ParentTGIBlocks = _tgiBlocks;
        }

        protected override Stream UnParse()
        {
            long pos;
            MemoryStream ms = new MemoryStream();
            BinaryWriter w = new BinaryWriter(ms);

            w.Write(_unknown1);
            pos = ms.Position;
            w.Write((uint)0);//tgiOffset
            w.Write((uint)0);//tgiSize
            w.Write(_unknown2);
            if (_tgiBlocks == null) _tgiBlocks = new TGIBlockList(OnResourceChanged);
            if (_tgiIndexes == null) _tgiIndexes = new Int32IndexList(OnResourceChanged, Int16.MaxValue, ReadInt16, WriteInt16, _tgiBlocks);
            _tgiIndexes.UnParse(ms);
            _tgiBlocks.UnParse(ms, pos);

            _tgiIndexes.ParentTGIBlocks = _tgiBlocks;

            return ms;
        }
        private static int ReadInt16(Stream s) { return new BinaryReader(s).ReadInt16(); }
        private static void WriteInt16(Stream s, int count) { new BinaryWriter(s).Write((Int16)count); }
        #endregion

        #region Content Fields
        [ElementPriority(1)]
        public ushort Unknown1 { get => _unknown1;
            set { if (_unknown1 != value) { _unknown1 = value; OnResourceChanged(this, EventArgs.Empty); } } }
        [ElementPriority(2)]
        public ushort Unknown2 { get => _unknown2;
            set { if (_unknown2 != value) { _unknown2 = value; OnResourceChanged(this, EventArgs.Empty); } } }
        [ElementPriority(3)]
        public Int32IndexList TgiIndexes { get => _tgiIndexes;
            set { if (_tgiIndexes != value) { _tgiIndexes = new Int32IndexList(OnResourceChanged, value, Int16.MaxValue, ReadInt16, WriteInt16, _tgiBlocks); OnResourceChanged(this, EventArgs.Empty); } } }
        [ElementPriority(4)]
        public TGIBlockList TgiBlocks { get => _tgiBlocks;
            set { if (_tgiBlocks != value) { _tgiBlocks = value == null ? null : new TGIBlockList(OnResourceChanged, value); _tgiIndexes.ParentTGIBlocks = _tgiBlocks; OnResourceChanged(this, EventArgs.Empty); } } }

        public String Value => ValueBuilder;

        #endregion
    }
    
    /// <summary>
    /// ResourceHandler for ModularResource wrapper
    /// </summary>
    public class ModularResourceHandler : AResourceHandler
    {
        public ModularResourceHandler()
        {
            Add(typeof(ModularResource), new List<string>(new[] { "0xCF9A4ACE", }));
        }
    }
}