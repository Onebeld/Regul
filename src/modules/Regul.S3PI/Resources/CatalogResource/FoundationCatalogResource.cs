﻿/***************************************************************************
 *  Copyright (C) 2013 by Peter L Jones                                    *
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

namespace Regul.S3PI.Resources.CatalogResource
{
    public class FoundationCatalogResource : CatalogResourceTGIBlockList
    {
        #region Attributes
        //--Version
        //--TGIBlockList offset and size
        FoundationType foundation;//Version>=0x04; appears after CommonBlock in earlier versions!
        MaterialList materials = null;//FoundationType.StiltedConcrete and above
        //--CommonBlock
        uint wallCatalogIndex;
        uint floorCatalogIndex;
        uint vpxyIndex;//Version>=0x04
        uint index3;
        uint index4;
        uint unknown5;
        uint unknown6;
        ShapeType shape;
        #endregion

        #region Constructors
        public FoundationCatalogResource(Stream s) : base(s) { }
        public FoundationCatalogResource(Stream unused, FoundationCatalogResource basis)
            : this(basis.version,
            basis.foundation,
            basis.common,
            basis.wallCatalogIndex,
            basis.floorCatalogIndex,
            basis.index3,
            basis.index4,
            basis.unknown5,
            basis.unknown6,
            basis.shape,
            basis.list)
        {
            materials = new MaterialList(OnResourceChanged, basis.materials);
            vpxyIndex = basis.vpxyIndex;
        }

        //Current
        public FoundationCatalogResource(uint version,
            FoundationType foundation,
            MaterialList materials,
            Common common,
            uint wallCatalogIndex,
            uint floorCatalogIndex,
            uint vpxyIndex,
            uint index3,
            uint index4,
            uint unknown5,
            uint unknown6,
            ShapeType shape,
            TGIBlockList ltgib)
            : this(version,
            foundation,
            common,
            wallCatalogIndex,
            floorCatalogIndex,
            index3,
            index4,
            unknown5,
            unknown6,
            shape,
            ltgib)
        {
            if (version < 0x00000004)
                throw new InvalidOperationException(String.Format("Version {0} invalid with these arguments", version));
            this.materials = new MaterialList(OnResourceChanged, materials);
            this.vpxyIndex = vpxyIndex;
        }

        //Version<0x04
        public FoundationCatalogResource(uint version,
            Common common,
            FoundationType foundation,
            uint wallCatalogIndex,
            uint floorCatalogIndex,
            uint index3,
            uint index4,
            uint unknown5,
            uint unknown6,
            ShapeType shape,
            TGIBlockList ltgib)
            : this(version,
            foundation,
            common,
            wallCatalogIndex,
            floorCatalogIndex,
            index3,
            index4,
            unknown5,
            unknown6,
            shape,
            ltgib)
        {
            if (version >= 0x00000004)
                throw new InvalidOperationException(String.Format("Version {0} invalid with these arguments", version));
        }

        //Common
        private FoundationCatalogResource(
            uint version,
            FoundationType foundation, //Here to avoid same method signature as Version<0x04
            Common common,
            uint wallCatalogIndex,
            uint floorCatalogIndex,
            uint index3,
            uint index4,
            uint unknown5,
            uint unknown6,
            ShapeType shape,
            TGIBlockList ltgib)
            : base(version, common, ltgib)
        {
            this.foundation = foundation;
            this.wallCatalogIndex = wallCatalogIndex;
            this.floorCatalogIndex = floorCatalogIndex;
            this.index3 = index3;
            this.index4 = index4;
            this.unknown5 = unknown5;
            this.unknown6 = unknown6;
            this.shape = shape;
        }

        #endregion

        #region Data I/O
        protected override void Parse(Stream s)
        {
            BinaryReader r = new BinaryReader(s);
            base.Parse(s);
            if (version >= 0x00000004)
            {
                this.foundation = (FoundationType)r.ReadUInt32();
                if ((uint)foundation >= (uint)FoundationType.StiltedConcrete)
                {
                    this.materials = new MaterialList(OnResourceChanged, s);
                }
            }
            this.common = new Common(OnResourceChanged, s);
            if (version < 0x00000004)
                this.foundation = (FoundationType)r.ReadUInt32();
            this.wallCatalogIndex = r.ReadUInt32();
            this.floorCatalogIndex = r.ReadUInt32();
            if (version >= 0x00000004)
                this.vpxyIndex = r.ReadUInt32();
            this.index3 = r.ReadUInt32();
            this.index4 = r.ReadUInt32();
            this.unknown5 = r.ReadUInt32();
            this.unknown6 = r.ReadUInt32();
            this.shape = (ShapeType)r.ReadUInt32();

            list = new TGIBlockList(OnResourceChanged, s, tgiPosn, tgiSize);

            if (GetType().Equals(typeof(FoundationCatalogResource)) && s.Position != s.Length)
                throw new InvalidDataException(String.Format("Data stream length 0x{0:X8} is {1:X8} bytes longer than expected at {2:X8}",
                        s.Length, s.Length - s.Position, s.Position));
        }

        protected override Stream UnParse()
        {
            Stream s = base.UnParse();
            BinaryWriter w = new BinaryWriter(s);
            if (version >= 0x00000004)
            {
                w.Write((uint)foundation);
                if ((uint)foundation >= (uint)FoundationType.StiltedConcrete)
                {
                    if (materials == null) materials = new MaterialList(OnResourceChanged);
                    materials.UnParse(s);
                }
            }
            if (common == null) common = new Common(OnResourceChanged);
            common.UnParse(s);
            if (version < 0x00000004)
                w.Write((uint)foundation);
            w.Write(wallCatalogIndex);
            w.Write(floorCatalogIndex);
            if (version >= 0x00000004)
                w.Write(vpxyIndex);
            w.Write(index3);
            w.Write(index4);
            w.Write(unknown5);
            w.Write(unknown6);
            w.Write((uint)shape);

            base.UnParse(s);

            w.Flush();

            return s;
        }
        #endregion

        #region AApiVersionedFields
        /// <summary>
        /// The list of available field names on this API object
        /// </summary>
        public override List<string> ContentFields
        {
            get
            {
                List<string> res = base.ContentFields;
                if (this.version < 0x00000004)
                {
                    res.Remove("Materials");
                    res.Remove("VPXYIndex");
                }
                else
                {
                    if ((uint)foundation < (uint)FoundationType.StiltedConcrete)
                    {
                        res.Remove("Materials");
                    }
                }
                return res;
            }
        }
        #endregion

        #region Sub-classes
        public enum FoundationType : uint
        {
            None = 0,
            Deck = 0x00000001,
            Foundation,
            Pool,
            Frieze,
            Platform,
            Fountain,
            StiltedConcrete,
            StiltedSteel,
            StiltedWood,
        }
        public enum ShapeType : uint
        {
            Rectangle = 0x00000000,
            Diamond = 0x00000001,
        }
        #endregion

        #region Content Fields
        //--insert Version: ElementPriority(1)
        [ElementPriority(3)]
        public MaterialList Materials
        {
            get { if (version < 0x00000004 || (uint)foundation < (uint)FoundationType.StiltedConcrete) throw new InvalidOperationException(); return materials; }
            set { if (version < 0x00000004 || (uint)foundation < (uint)FoundationType.StiltedConcrete) throw new InvalidOperationException(); if (!materials.Equals(value)) { materials = new MaterialList(OnResourceChanged, value); OnResourceChanged(this, EventArgs.Empty); } }
        }
        //--insert CommonBlock: ElementPriority(11)
        [ElementPriority(21)]
        public FoundationType Foundation { get { return foundation; } set { if (foundation != value) { foundation = value; OnResourceChanged(this, EventArgs.Empty); } } }
        [ElementPriority(22), TGIBlockListContentField("TGIBlocks")]
        public uint WallCatalogIndex { get { return wallCatalogIndex; } set { if (wallCatalogIndex != value) { wallCatalogIndex = value; OnResourceChanged(this, EventArgs.Empty); } } }
        [ElementPriority(23), TGIBlockListContentField("TGIBlocks")]
        public uint FloorCatalogIndex { get { return floorCatalogIndex; } set { if (floorCatalogIndex != value) { floorCatalogIndex = value; OnResourceChanged(this, EventArgs.Empty); } } }
        [ElementPriority(24), TGIBlockListContentField("TGIBlocks")]
        public uint VPXYIndex
        {
            get { if (version < 0x00000004) throw new InvalidOperationException(); return vpxyIndex; }
            set { if (version < 0x00000004) throw new InvalidOperationException(); if (vpxyIndex != value) { vpxyIndex = value; OnResourceChanged(this, EventArgs.Empty); } }
        }
        [ElementPriority(25), TGIBlockListContentField("TGIBlocks")]
        public uint Index3 { get { return index3; } set { if (index3 != value) { index3 = value; OnResourceChanged(this, EventArgs.Empty); } } }
        [ElementPriority(26), TGIBlockListContentField("TGIBlocks")]
        public uint Index4 { get { return index4; } set { if (index4 != value) { index4 = value; OnResourceChanged(this, EventArgs.Empty); } } }
        [ElementPriority(27)]
        public uint Unknown5 { get { return unknown5; } set { if (unknown5 != value) { unknown5 = value; OnResourceChanged(this, EventArgs.Empty); } } }
        [ElementPriority(28)]
        public uint Unknown6 { get { return unknown6; } set { if (unknown6 != value) { unknown6 = value; OnResourceChanged(this, EventArgs.Empty); } } }
        [ElementPriority(29)]
        public ShapeType Shape { get { return shape; } set { if (shape != value) { shape = value; OnResourceChanged(this, EventArgs.Empty); } } }
        //--insert TGIBlockList: no ElementPriority
        #endregion
    }
}