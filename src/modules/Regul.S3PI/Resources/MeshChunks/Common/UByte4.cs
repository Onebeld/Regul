﻿/***************************************************************************
 *  Based on earlier work.                                                 *
 *  Copyright (C) 2011 by Peter L Jones                                    *
 *  pljones@users.sf.net                                                   *
 *                                                                         *
 *  This is free software: you can redistribute it and/or modify           *
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
 *  along with this software.  If not, see <http://www.gnu.org/licenses/>. *
 ***************************************************************************/

using System;
using System.IO;
using Regul.S3PI.Interfaces;

namespace Regul.S3PI.Resources.MeshChunks.Common
{
    public class UByte4 : AHandlerElement
    {
        private byte mA;
        private byte mB;
        private byte mC;
        private byte mD;

        public UByte4(EventHandler handler) : base(handler) {}
        public UByte4(EventHandler handler, UByte4 basis): this(handler, basis.A,basis.B,basis.C,basis.D){}
        public UByte4(EventHandler handler, Stream s): base(handler){Parse(s);}
        public UByte4(EventHandler handler, byte a, byte b, byte c, byte d) : base(handler)
        {
            mA = a;
            mB = b;
            mC = c;
            mD = d;
        }

        public string Value { get { return ToString(); } }
        public override string ToString()
        {
            return String.Format("[{0:X2},{1:X2},{2:X2},{3:X2}]", mA, mB, mC, mD);
        }
        [ElementPriority(1)]
        public byte A
        {
            get { return mA; }
            set { if(mA!=value){mA = value; OnElementChanged();} }
        }
        [ElementPriority(2)]
        public byte B
        {
            get { return mB; }
            set { if(mB!=value){mB = value; OnElementChanged();} }
        }
        [ElementPriority(3)]
        public byte C
        {
            get { return mC; }
            set { if(mC!=value){mC = value; OnElementChanged();} }
        }
        [ElementPriority(4)]
        public byte D
        {
            get { return mD; }
            set { if(mD!=value){mD = value; OnElementChanged();} }
        }

        public virtual void Parse(Stream s)
        {
            BinaryReader br = new BinaryReader(s);
            mA = br.ReadByte();
            mB = br.ReadByte();
            mC = br.ReadByte();
            mD = br.ReadByte();
        }

        public virtual void UnParse(Stream s)
        {
            BinaryWriter bw = new BinaryWriter(s);
            bw.Write(mA);
            bw.Write(mB);
            bw.Write(mC);
            bw.Write(mD);
        }

        // public override AHandlerElement Clone(EventHandler handler) { return new UByte4(0, handler, this); }

        public override System.Collections.Generic.List<string> ContentFields
        {
            get { return GetContentFields(GetType()); }
        }
    }
}