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
using System.IO;
using Regul.S3PI.Interfaces;

namespace Regul.S3PI.Resources.MeshChunks.Common
{
    public class Vector3 : Vector2
    {
        protected float mZ;
        public Vector3(EventHandler handler, float x, float y, float z)
            : base(handler,x,y)
        {
            mZ = z;
        }
        public Vector3(EventHandler handler, Stream s) : base(handler,s) { }

        public Vector3(EventHandler handler) : base(handler) { }
        public Vector3(EventHandler handler, Vector3 basis) : this(handler, basis.X, basis.Y, basis.Z) { }

        [ElementPriority(3)]
        public float Z
        {
            get { return mZ; }
            set { if (mZ != value) { mZ = value; OnElementChanged(); } }
        }

        public override void Parse(Stream s)
        {
            base.Parse(s);
            BinaryReader br = new BinaryReader(s);
            mZ = br.ReadSingle();
        }
        public override void UnParse(Stream s)
        {
            base.UnParse(s);
            BinaryWriter bw = new BinaryWriter(s);
            bw.Write(mZ);
        }
        public override string ToString()
        {
            return String.Format("[{0,8:0.00000},{1,8:0.00000},{2,8:0.00000}]", X, Y, Z);
        }

        // public override AHandlerElement Clone(EventHandler handler) { return new Vector3(0, handler, this); }

        public new string Value { get { return ValueBuilder.Replace("\n", "; "); } }
    }
}