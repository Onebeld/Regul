/***************************************************************************
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
using System.Collections.Generic;
using System.IO;
using Regul.S3PI.Interfaces;

namespace Regul.S3PI.Resources.MeshChunks.Common
{
    public class Matrix43 : AHandlerElement
    {

        private Vector3 mRight, mUp, mBack, mTranslate;

        public Matrix43(EventHandler handler) : this(handler, new Vector3(handler, 1, 0, 0), new Vector3(handler, 0, 1, 0), new Vector3(handler, 0, 0, 1), new Vector3(handler, 0, 0, 0)) { }
        public Matrix43(EventHandler handler, Stream s) : base(handler) { Parse(s); }
        public Matrix43(EventHandler handler, Matrix43 basis) : this(handler, basis.mRight, basis.mUp, basis.mBack, basis.mTranslate) { }
        public Matrix43(EventHandler handler, Vector3 right, Vector3 up, Vector3 back, Vector3 translate)
            : base(handler)
        {
            mRight = new Vector3(handler, right);
            mUp = new Vector3(handler, up);
            mBack = new Vector3(handler, back);
            mTranslate = new Vector3(handler, translate);
        }
        [ElementPriority(1)]
        public Vector3 Right
        {
            get { return mRight; }
            set { if (mRight != value) { mRight = value; OnElementChanged(); } }
        }
        [ElementPriority(2)]
        public Vector3 Up
        {
            get { return mUp; }
            set { if (mUp != value) { mUp = value; OnElementChanged(); } }
        }
        [ElementPriority(3)]
        public Vector3 Back
        {
            get { return mBack; }
            set { if (mBack != value) { mBack = value; OnElementChanged(); } }
        }
        [ElementPriority(4)]
        public Vector3 Translate
        {
            get { return mTranslate; }
            set { if (mTranslate != value) { mTranslate = value; OnElementChanged(); } }
        }

        private void Parse(Stream s)
        {
            float m00, m01, m02, m03;
            float m10, m11, m12, m13;
            float m20, m21, m22, m23;
            BinaryReader br = new BinaryReader(s);
            m00 = br.ReadSingle();
            m01 = br.ReadSingle();
            m02 = br.ReadSingle();
            m03 = br.ReadSingle();

            m10 = br.ReadSingle();
            m11 = br.ReadSingle();
            m12 = br.ReadSingle();
            m13 = br.ReadSingle();

            m20 = br.ReadSingle();
            m21 = br.ReadSingle();
            m22 = br.ReadSingle();
            m23 = br.ReadSingle();

            mRight = new Vector3(handler, m00, m10, m20);
            mUp = new Vector3(handler, m01, m11, m21);
            mBack = new Vector3(handler, m02, m12, m22);
            mTranslate = new Vector3(handler, m03, m13, m23);
        }
        public void UnParse(Stream s)
        {
            BinaryWriter bw = new BinaryWriter(s);
            bw.Write(mRight.X);
            bw.Write(mUp.X);
            bw.Write(mBack.X);
            bw.Write(mTranslate.X);

            bw.Write(mRight.Y);
            bw.Write(mUp.Y);
            bw.Write(mBack.Y);
            bw.Write(mTranslate.Y);

            bw.Write(mRight.Z);
            bw.Write(mUp.Z);
            bw.Write(mBack.Z);
            bw.Write(mTranslate.Z);
        }
        public string Value
        {
            get { return String.Format("{0}\r\n{1}\r\n{2}\r\n{3}", Right, Up, Back, Translate); }
        }
        public override string ToString()
        {
            return String.Format("{0},{1},{2},{3}", Right, Up, Back, Translate);
        }

        // public override AHandlerElement Clone(EventHandler handler) { return new Matrix43(0, handler, this); }

        public override List<string> ContentFields
        {
            get { return GetContentFields(GetType()); }
        }
    }
}