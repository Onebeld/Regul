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

namespace Regul.S3PI.Resources.MeshChunks
{
    public class IBUF2 : IBUF
    {
        public IBUF2(EventHandler handler, IBUF basis)
            : base(handler, basis)
        {
        }

        public IBUF2(EventHandler handler)
            : base(handler)
        {
        }

        public IBUF2(EventHandler handler, Stream s)
            : base(handler, s)
        {
        }

        public override uint ResourceType
        {
            get { return 0x0229684F; }
        }
    }
    public class IBUF : ARCOLBlock
    {
        public int AddIndices(Int32[] indices)
        {
            int starts = mBuffer.Length;
            Int32[] newBuffer = new int[indices.Length+mBuffer.Length];
            Array.Copy(mBuffer, 0, newBuffer, 0, mBuffer.Length);
            Array.Copy(indices, 0, newBuffer, 0, indices.Length);
            mBuffer = newBuffer;
            return starts;
        }
        public Int32[] GetIndices(MLOD.Mesh mesh)
        {
            return GetIndices(mesh.PrimitiveType, mesh.StartIndex, mesh.PrimitiveCount);
        }

        public Int32[] GetIndices(MLOD.Mesh mesh, int geoStateIndex)
        {
            return GetIndices(mesh, mesh.GeometryStates[geoStateIndex]);
        }
        public Int32[] GetIndices(MLOD.Mesh mesh, MLOD.GeometryState geometryState)
        {
            return GetIndices(mesh.PrimitiveType, geometryState.StartIndex, geometryState.PrimitiveCount)
                .Select(x => x - geometryState.MinVertexIndex).ToArray();
        }

        public Int32[] GetIndices(ModelPrimitiveType type, Int32 startIndex, Int32 count)
        {
            return GetIndices(IndexCountFromPrimitiveType(type), startIndex, count);
        }
        public Int32[] GetIndices(int sizePerPrimitive, Int32 startIndex, Int32 count)
        {
            Int32[] output = new int[count * sizePerPrimitive];
            Array.Copy(mBuffer, startIndex, output, 0, output.Length);
            return output;
        }

        public void SetIndices(MLOD mlod, MLOD.Mesh mesh, Int32[] indices)
        {
            SetIndices(mlod, mesh.IndexBufferIndex, mesh.PrimitiveType, mesh.StartIndex, mesh.PrimitiveCount, indices);
            mesh.PrimitiveCount = indices.Length / IndexCountFromPrimitiveType(mesh.PrimitiveType);
        }

        public void SetIndices(MLOD mlod, MLOD.Mesh mesh, int geoStateIndex, Int32[] indices)
        {
            MLOD.GeometryState geometryState = mesh.GeometryStates[geoStateIndex];
            SetIndices(mlod, mesh, geometryState, indices);
        }
        public void SetIndices(MLOD mlod, MLOD.Mesh mesh, MLOD.GeometryState geometryState, Int32[] indices)
        {
            SetIndices(mlod, mesh.IndexBufferIndex, mesh.PrimitiveType, geometryState.StartIndex, geometryState.PrimitiveCount,
                indices.Select(x => x + geometryState.MinVertexIndex).ToArray());
            geometryState.PrimitiveCount = indices.Length / IndexCountFromPrimitiveType(mesh.PrimitiveType);
        }
        void SetIndices(MLOD mlod, GenericRCOLResource.GenericRCOLResource.ChunkReference myIBI, ModelPrimitiveType type, Int32 startIndex, Int32 primCount, Int32[] indices)
        {
            SetIndices(mlod, myIBI, startIndex, startIndex + primCount * IndexCountFromPrimitiveType(type), indices);
        }
        void SetIndices(MLOD mlod, GenericRCOLResource.GenericRCOLResource.ChunkReference myIBI, Int32 beforeLength, Int32 afterPos, Int32[] indices)
        {
            Int32[] before = new Int32[beforeLength];
            Array.Copy(mBuffer, 0, before, 0, before.Length);

            Int32[] after = new Int32[mBuffer.Length - afterPos];
            Array.Copy(mBuffer, afterPos, after, 0, after.Length);

            mBuffer = new Int32[before.Length + indices.Length + after.Length];
            Array.Copy(before, 0, mBuffer, 0, before.Length);
            Array.Copy(indices, 0, mBuffer, before.Length, indices.Length);
            Array.Copy(after, 0, mBuffer, before.Length + indices.Length, after.Length);

            int offset = beforeLength + indices.Length - afterPos;
            if (offset != 0)
                foreach (MLOD.Mesh m in mlod.Meshes.FindAll(m => m.IndexBufferIndex.Equals(myIBI)))
                    if (m.StartIndex > beforeLength)
                    {
                        m.StartIndex += offset;
                        foreach (MLOD.GeometryState g in m.GeometryStates)
                            if (g.StartIndex > beforeLength)
                                g.StartIndex += offset;
                    }
        }

        [Flags]
        public enum FormatFlags : uint
        {
            DifferencedIndices = 0x1,
            Uses32BitIndices = 0x2,
            IsDisplayList = 0x4
        }
        private UInt32 mVersion;
        private FormatFlags mFlags;
        private UInt32 mDisplayListUsage;
        private Int32[] mBuffer;
        public IBUF(EventHandler handler) : base(handler, null) { }
        public IBUF(EventHandler handler, Stream s) : base(handler, s) { }
        public IBUF(EventHandler handler, IBUF basis) : this(handler, basis.Version, basis.Flags, basis.DisplayListUsage, basis.Buffer) { }
        public IBUF(EventHandler handler, uint version, FormatFlags flags, uint displayListUsage, int[] buffer)
            : base(handler, null)
        {
            mVersion = version;
            mFlags = flags;
            mDisplayListUsage = displayListUsage;
            mBuffer = buffer;
        }

        [ElementPriority(1)]
        public UInt32 Version { get { return mVersion; } set { if (mVersion != value) { mVersion = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(2)]
        public FormatFlags Flags { get { return mFlags; } set { if (mFlags != value) { mFlags = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(3)]
        public UInt32 DisplayListUsage { get { return mDisplayListUsage; } set { if (mDisplayListUsage != value) { mDisplayListUsage = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(4)]
        public Int32[] Buffer { get { return mBuffer; } set { if (mBuffer != value) { mBuffer = value; OnRCOLChanged(this, EventArgs.Empty); } } }

        public string Value { get { return ValueBuilder; } }
        protected override List<string> ValueBuilderFields
        {
            get
            {
                List<string> fields = base.ValueBuilderFields;
                fields.Remove("Buffer");
                return fields;
            }
        }

        protected override void Parse(Stream s)
        {
            BinaryReader br = new BinaryReader(s);
            string tag = FOURCC(br.ReadUInt32());
            if (tag != Tag)
            {
                throw new InvalidDataException(string.Format("Invalid Tag read: '{0}'; expected: '{1}'; at 0x{2:X8}", tag, Tag, s.Position));
            }
            mVersion = br.ReadUInt32();
            mFlags = (FormatFlags)br.ReadUInt32();
            mDisplayListUsage = br.ReadUInt32();

            bool is32Bit = (mFlags & FormatFlags.Uses32BitIndices) != 0;
            mBuffer = new Int32[(s.Length-s.Position) / (is32Bit ? 4 : 2)];
            Int32 last = 0;
            for (int i = 0; i < mBuffer.Length; i++)
            {
                Int32 cur = is32Bit ? br.ReadInt32() : br.ReadInt16();
                if ((mFlags & FormatFlags.DifferencedIndices) != 0)
                {
                    cur += last;
                }
                mBuffer[i] = cur;
                last = cur;
            }


        }

        public override Stream UnParse()
        {
            MemoryStream s = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(s);
            if (mBuffer == null) mBuffer = new Int32[0];
            bool is32Bit = mBuffer.Length > UInt16.MaxValue;
            if (is32Bit)
            {
                mFlags |= FormatFlags.Uses32BitIndices;
            }
            else
            {
                mFlags &= (FormatFlags)UInt32.MaxValue ^ FormatFlags.Uses32BitIndices;
            }

            bw.Write((UInt32)FOURCC(Tag));
            bw.Write(mVersion);
            bw.Write((UInt32)mFlags);
            bw.Write(mDisplayListUsage);
            
            bool isDifferenced = (mFlags & FormatFlags.DifferencedIndices) != 0;
            Int32 last = 0;
            for (int i = 0; i < mBuffer.Length; i++)
            {
                Int32 cur = mBuffer[i];
                if (isDifferenced)
                {
                    cur -= last;
                    last = mBuffer[i];
                }
                if (is32Bit) bw.Write(cur); else bw.Write((UInt16)cur);
            }
            return s;
        }

        public override uint ResourceType
        {
            get { return 0x01D0E70F; }
        }

        public override string Tag
        {
            get { return "IBUF"; }
        }

        public static int IndexCountFromPrimitiveType(ModelPrimitiveType t)
        {
            switch (t)
            {
                case ModelPrimitiveType.TriangleList:
                    return 3;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}