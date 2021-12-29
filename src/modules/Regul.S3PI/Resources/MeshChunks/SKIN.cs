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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Regul.S3PI.Interfaces;
using Regul.S3PI.Resources.MeshChunks.Common;

namespace Regul.S3PI.Resources.MeshChunks
{
    public class SKIN : ARCOLBlock
    {
        public class BoneList : DependentList<Bone>
        {
            public BoneList(EventHandler handler) : base(handler) { }
            public BoneList(EventHandler handler, IEnumerable<Bone> ilt) : base(handler, ilt) { }
            protected override Bone CreateElement(Stream s)
            {
                throw new NotSupportedException();
            }

            protected override void WriteElement(Stream s, Bone element)
            {
                throw new NotSupportedException();
            }
            public Bone this[UInt32 nameHash]
            {
                get { return this.FirstOrDefault(x=>x.NameHash.Equals(nameHash)); }
            }

        }
        public class Bone : AHandlerElement, IEquatable<Bone>
        {
            private UInt32 mNameHash;
            private Matrix43 mInverseBindPose;
            public Bone(EventHandler handler)
                : base(handler)
            {
                mInverseBindPose = new Matrix43(handler);
            }
            public Bone(EventHandler handler, uint Bone, Matrix43 matrix43)
                : base(handler)
            {
                mNameHash = Bone;
                mInverseBindPose = new Matrix43(handler, matrix43);
            }
            public Bone(EventHandler handler, Bone j)
                : this(handler, j.mNameHash, j.mInverseBindPose)
            {
            }
            public string Value { get { return ValueBuilder; } }
            [ElementPriority(1)]
            public uint NameHash
            {
                get { return mNameHash; }
                set { if(mNameHash!=value){mNameHash = value; OnElementChanged();} }
            }

            [ElementPriority(2)]
            public Matrix43 InverseBindPose
            {
                get { return mInverseBindPose; }
                set { if(mInverseBindPose!=value){mInverseBindPose = value; OnElementChanged();} }
            }

            public override List<string> ContentFields
            {
                get { return GetContentFields(GetType()); }
            }

            public bool Equals(Bone other)
            {
                return mNameHash.Equals(other.mNameHash)
                    && mInverseBindPose.Equals(other.mInverseBindPose);
            }
            public override bool Equals(object obj)
            {
                return obj as Bone != null ? this.Equals(obj as Bone) : false;
            }
            public override int GetHashCode()
            {
                return mNameHash.GetHashCode()
                    ^ mInverseBindPose.GetHashCode();
            }

        }

        private UInt32 mVersion;
        private BoneList mBones;

        public SKIN(EventHandler handler): this(handler, new BoneList(handler),0x00000001){}
        public SKIN(EventHandler handler, SKIN basis) : this(handler, new BoneList(handler, basis.Bones), basis.Version) { }
        public SKIN(EventHandler handler, Stream s): base(handler, s){}
        public SKIN(EventHandler handler, BoneList bones, uint version) : base(handler, null)
        {
            mBones = bones == null ? null : new BoneList(handler, bones);
            mVersion = version;
        }

        [ElementPriority(1)]
        public uint Version
        {
            get { return mVersion; }
            set { if(mVersion!=value){mVersion = value; OnRCOLChanged(this, EventArgs.Empty);} }
        }

        [ElementPriority(2)]
        public BoneList Bones
        {
            get { return mBones; }
            set { if (mBones != value) { mBones = value == null ? null : new BoneList(handler, value); OnRCOLChanged(this, EventArgs.Empty); } }
        }

        public string Value { get { return ValueBuilder; } }

        protected override void Parse(Stream s)
        {
            BinaryReader br = new BinaryReader(s);
            string tag = FOURCC(br.ReadUInt32());
            if (tag != Tag)
            {
                throw new InvalidDataException(string.Format("Invalid Tag read: '{0}'; expected: '{1}'; at 0x{1:X8}", tag, Tag, s.Position));
            }
            mVersion = br.ReadUInt32();
            mBones = new BoneList(handler);
            int count = br.ReadInt32();
            uint[] names = new uint[count];
            for (int i = 0; i < count; i++)names[i] = br.ReadUInt32();
            for (int i = 0; i < count; i++)mBones.Add(new Bone(handler, names[i], new Matrix43(handler, s)));
        }

        public override Stream UnParse()
        {
            MemoryStream s = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(s);
            bw.Write((uint)FOURCC(Tag));
            bw.Write(mVersion);
            if (mBones == null) mBones = new BoneList(handler);
            bw.Write(mBones.Count);
            foreach (Bone j in mBones) bw.Write(j.NameHash);
            foreach (Bone j in mBones) j.InverseBindPose.UnParse(s);
            return s;
        }

        public override string Tag
        {
            get { return "SKIN"; }
        }

        public override uint ResourceType
        {
            get { return 0x01D0E76B; }
        }
    }
}