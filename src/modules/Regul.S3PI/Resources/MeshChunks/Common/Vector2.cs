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
    public class Vector2 : AHandlerElement
    {
        protected float mX, mY;
        public Vector2(EventHandler handler, float x, float y): base(handler){mX = x;mY = y;}
        public Vector2(EventHandler handler) : base(handler) { }
        public Vector2(EventHandler handler, Stream s) : base(handler) { Parse(s); }
        public Vector2(EventHandler handler, Vector2 basis) : this(handler, basis.X, basis.Y){ }

        [ElementPriority(1)]
        public float X
        {
            get { return mX; }
            set { if (mX != value) { mX = value; OnElementChanged(); } }
        }
        [ElementPriority(2)]
        public float Y
        {
            get { return mY; }
            set { if (mY != value) { mY = value; OnElementChanged(); } }
        }
        public virtual void Parse(Stream s)
        {
            BinaryReader br = new BinaryReader(s);
            mX = br.ReadSingle();
            mY = br.ReadSingle();
        }
        public virtual void UnParse(Stream s)
        {
            BinaryWriter bw = new BinaryWriter(s);
            bw.Write(mX);
            bw.Write(mY);
        }
        public override string ToString()
        {
            return String.Format("[{0,8:0.00000},{1,8:0.00000}]", X, Y);
        }

        public override List<string> ContentFields
        {
            get { return GetContentFields(GetType()); }
        }

        public string Value { get { return ValueBuilder.Replace("\n", "; "); } }
    }
}