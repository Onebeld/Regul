﻿/***************************************************************************
 *  Copyright (C) 2010 by Peter L Jones                                    *
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
using System.IO;
using Regul.S3PI.Interfaces;

namespace Regul.S3PI.Resources.CatalogResource
{
    public class RoofStyleCatalogResource : CatalogResourceTGIBlockList
    {
        #region Attributes
        Roof roofType;
        uint catalogRoofPattern;
        uint catalogWallStyle;
        float defaultSlope;
        RoofStyle roofStyleFlags;//roofStyleFlags
        #endregion

        #region Constructors
        public RoofStyleCatalogResource(Stream s) : base(s) { }
        public RoofStyleCatalogResource(Stream unused, RoofStyleCatalogResource basis)
            : base(basis.version, basis.common, basis.list)
        {
            this.common = new Common(OnResourceChanged, basis.common);
            this.roofType = basis.roofType;
            this.catalogRoofPattern = basis.catalogRoofPattern;
            this.catalogWallStyle = basis.catalogWallStyle;
            this.defaultSlope = basis.defaultSlope;
            this.roofStyleFlags = basis.roofStyleFlags;
        }
        public RoofStyleCatalogResource(uint version,
            Common common, Roof roofType, uint catalogRoofStyle, uint catalogWallStyle, float defaultSlope,
            RoofStyle roofStyleFlags,
            TGIBlockList ltgib)
            : base(version, common, ltgib)
        {
            this.common = new Common(OnResourceChanged, common);
            this.roofType = roofType;
            this.catalogRoofPattern = catalogRoofStyle;
            this.catalogWallStyle = catalogWallStyle;
            this.defaultSlope = defaultSlope;
            this.roofStyleFlags = roofStyleFlags;
        }
        #endregion

        #region Data I/O
        protected override void Parse(Stream s)
        {
            BinaryReader r = new BinaryReader(s);
            base.Parse(s);
            this.common = new Common(OnResourceChanged, s);
            this.roofType = (Roof)r.ReadUInt32();
            this.catalogRoofPattern = r.ReadUInt32();
            this.catalogWallStyle = r.ReadUInt32();
            this.defaultSlope = r.ReadSingle();
            this.roofStyleFlags = (RoofStyle)r.ReadUInt32();

            list = new TGIBlockList(OnResourceChanged, s, tgiPosn, tgiSize);

            if (this.GetType().Equals(typeof(RoofStyleCatalogResource)) && s.Position != s.Length)
                throw new InvalidDataException(String.Format("Data stream length 0x{0:X8} is {1:X8} bytes longer than expected at {2:X8}",
                        s.Length, s.Length - s.Position, s.Position));
        }

        protected override Stream UnParse()
        {
            Stream s = base.UnParse();
            BinaryWriter w = new BinaryWriter(s);

            if (common == null) common = new Common(OnResourceChanged);
            common.UnParse(s);

            w.Write((uint)roofType);
            w.Write(catalogRoofPattern);
            w.Write(catalogWallStyle);
            w.Write(defaultSlope);
            w.Write((uint)roofStyleFlags);

            base.UnParse(s);

            w.Flush();

            return s;
        }
        #endregion

        #region Sub-Types
        public enum Roof : uint
        {
            Gable = 0x00000001,
            TallGable = 0x00000002,
            HalfGable = 0x00000003,
            Mansard = 0x00000004,
            Hipped = 0x00000005,
            HalfHipped = 0x00000006,
            Conical = 0x00000007,
            Dome = 0x00000008,
            Octagonal = 0x00000009,
            DiagonalGable = 0x0000000A,
            DiagonalTallGable = 0x0000000B,
            DiagonalHalfGable = 0x0000000C,
            DiagonalHipped = 0x0000000D,
            DiagonalHalfHipped = 0x0000000E,
            Dormer = 0x0000000F,
            Flat = 0x00000010,
            DiagonalMansard = 0x00000011,
            PagodaGable = 0x00000012,
            PagodaTallGable = 0x00000013,
            PagodaHalfGable = 0x00000014,
            PagodaHipped = 0x00000015,
            PagodaOctagonal = 0x00000016,
            DiagonalPagodaGable = 0x00000017,
            DiagonalPagodaTallGable = 0x00000018,
            DiagonalPagodaHalfGable = 0x00000019,
            DiagonalPagodaHipped = 0x0000001A,
            PagodaMansard = 0x0000001B,
            DiagonalPagodaMansard = 0x0000001C,
            PagodaOctagonalMansard = 0x0000001D,
            OctagonalMansard = 0x0000001E,
        }

        public enum RoofStyle : uint
        {
            Dormer = 0x0000001,
            Diagonal = 0x00000002,
        }
        #endregion

        #region Content Fields
        //--insert Version: ElementPriority(1)
        //--insert CommonBlock: ElementPriority(11)
        [ElementPriority(27)]
        public Roof RoofType { get { return roofType; } set { if (roofType != value) { roofType = value; OnResourceChanged(this, EventArgs.Empty); } } }
        [ElementPriority(28), TGIBlockListContentField("TGIBlocks")]
        public uint CatalogRoofPattern { get { return catalogRoofPattern; } set { if (catalogRoofPattern != value) { catalogRoofPattern = value; OnResourceChanged(this, EventArgs.Empty); } } }
        [ElementPriority(29), TGIBlockListContentField("TGIBlocks")]
        public uint CatalogWallStyle { get { return catalogWallStyle; } set { if (catalogWallStyle != value) { catalogWallStyle = value; OnResourceChanged(this, EventArgs.Empty); } } }
        [ElementPriority(30)]
        public float DefaultSlope { get { return defaultSlope; } set { if (defaultSlope != value) { defaultSlope = value; OnResourceChanged(this, EventArgs.Empty); } } }
        [ElementPriority(31)]
        public RoofStyle RoofStyleFlags { get { return roofStyleFlags; } set { if (roofStyleFlags != value) { roofStyleFlags = value; OnResourceChanged(this, EventArgs.Empty); } } }
        //--insert TGIBlockList: no ElementPriority
        #endregion
    }
}