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
using Regul.S3PI.Interfaces;
using Regul.S3PI.Resources.MeshChunks.Common;

namespace Regul.S3PI.Resources.MeshChunks
{
    public class MLOD : ARCOLBlock
    {
        #region Attributes
        private UInt32 mVersion = 0x00000202;
        private MeshList mMeshes;
        #endregion

        #region Constructors
        public MLOD(EventHandler handler, MLOD basis) : this(handler, basis.Version, new MeshList(handler,basis, basis.mMeshes)) { }
        public MLOD(EventHandler handler) : base(handler, null) { }
        public MLOD(EventHandler handler, Stream s) : base(handler, s) { }
        public MLOD(EventHandler handler, uint version, MeshList meshes)
            : base(handler, null)
        {
            mVersion = version;
            mMeshes = meshes;
        }
        #endregion

        #region ARCOLBlock
        [ElementPriority(2)]
        public override string Tag { get { return "MLOD"; } }

        [ElementPriority(3)]
        public override uint ResourceType { get { return 0x01D10F34; } }

        protected override void Parse(Stream s)
        {
            BinaryReader br = new BinaryReader(s);
            string tag = FOURCC(br.ReadUInt32());
            if (tag != Tag)
                throw new InvalidDataException(string.Format("Invalid Tag read: '{0}'; expected: '{1}'; at 0x{1:X8}", tag, Tag, s.Position));
            mVersion = br.ReadUInt32();
            mMeshes = new MeshList(handler, this, s);
        }

        public override Stream UnParse()
        {
            MemoryStream s = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(s);
            bw.Write((uint)FOURCC(Tag));
            bw.Write(mVersion);
            if (mMeshes == null) mMeshes = new MeshList(handler, this);
            mMeshes.UnParse(s);
            return s;
        }
        #endregion

        #region Sub-types
        public class GeometryState : AHandlerElement, IEquatable<GeometryState>
        {
            private const int kRecommendedApiVersion = 1;

            private UInt32 mName;
            private Int32 mStartIndex;
            private Int32 mMinVertexIndex;
            private Int32 mVertexCount;
            private Int32 mPrimitiveCount;

            public GeometryState(EventHandler handler) : base(handler) { }
            public GeometryState(EventHandler handler, GeometryState basis) : this(handler, basis.Name, basis.StartIndex, basis.MinVertexIndex, basis.VertexCount, basis.PrimitiveCount) { }
            public GeometryState(EventHandler handler, Stream s) : base(handler) { Parse(s); }
            public GeometryState(EventHandler handler, uint name, int startIndex, int minVertexIndex, int vertexCount,
                int primitiveCount)
                : base(handler)
            {
                mName = name;
                mStartIndex = startIndex;
                mMinVertexIndex = minVertexIndex;
                mVertexCount = vertexCount;
                mPrimitiveCount = primitiveCount;
            }

            [ElementPriority(1)]
            public UInt32 Name
            {
                get { return mName; }
                set { if (mName != value) { mName = value; OnElementChanged(); } }
            }
            [ElementPriority(2)]
            public Int32 StartIndex
            {
                get { return mStartIndex; }
                set { if (mStartIndex != value) { mStartIndex = value; OnElementChanged(); } }
            }
            [ElementPriority(3)]
            public Int32 MinVertexIndex
            {
                get { return mMinVertexIndex; }
                set { if (mMinVertexIndex != value) { mMinVertexIndex = value; OnElementChanged(); } }
            }
            [ElementPriority(4)]
            public Int32 VertexCount
            {
                get { return mVertexCount; }
                set { if (mVertexCount != value) { mVertexCount = value; OnElementChanged(); } }
            }
            [ElementPriority(5)]
            public Int32 PrimitiveCount
            {
                get { return mPrimitiveCount; }
                set { if (mPrimitiveCount != value) { mPrimitiveCount = value; OnElementChanged(); } }
            }

            private void Parse(Stream s)
            {
                BinaryReader br = new BinaryReader(s);
                mName = br.ReadUInt32();
                mStartIndex = br.ReadInt32();
                mMinVertexIndex = br.ReadInt32();
                mVertexCount = br.ReadInt32();
                mPrimitiveCount = br.ReadInt32();
            }
            public void UnParse(Stream s)
            {
                BinaryWriter bw = new BinaryWriter(s);
                bw.Write(mName);
                bw.Write(mStartIndex);
                bw.Write(mMinVertexIndex);
                bw.Write(mVertexCount);
                bw.Write(mPrimitiveCount);
            }

            public override List<string> ContentFields
            {
                get { return GetContentFields(GetType()); }
            }

            public bool Equals(GeometryState other)
            {
                return
                    mName.Equals(other.mName)
                    && mStartIndex.Equals(other.mStartIndex)
                    && mMinVertexIndex.Equals(other.mMinVertexIndex)
                    && mVertexCount.Equals(other.mVertexCount)
                    && mPrimitiveCount.Equals(other.mPrimitiveCount)
                    ;
            }
            public override bool Equals(object obj)
            {
                return obj as GeometryState != null ? this.Equals(obj as GeometryState) : false;
            }
            public override int GetHashCode()
            {
                return
                    mName.GetHashCode()
                    ^ mStartIndex.GetHashCode()
                    ^ mMinVertexIndex.GetHashCode()
                    ^ mVertexCount.GetHashCode()
                    ^ mPrimitiveCount.GetHashCode()
                    ;
            }

            public string Value
            {
                get
                {
                    return ValueBuilder;
                    /*
                    StringBuilder sb = new StringBuilder();
                    sb.AppendFormat("Name: 0x{0:X8}\n", mName);
                    sb.AppendFormat("Start Index:\t{0}\n", mStartIndex);
                    sb.AppendFormat("Min Vertex Index:\t{0}\n", mMinVertexIndex);
                    sb.AppendFormat("Vertex Count:\t{0}\n", mVertexCount);
                    sb.AppendFormat("Primitive Count:\t{0}\n", mPrimitiveCount);
                    return sb.ToString();
                    /**/
                }
            }
        }

        public class GeometryStateList : DependentList<GeometryState>
        {
            public GeometryStateList(EventHandler handler) : base(handler) { }
            public GeometryStateList(EventHandler handler, Stream s) : base(handler, s) { }
            public GeometryStateList(EventHandler handler, IEnumerable<GeometryState> basis) : base(handler, basis) { }

            protected override GeometryState CreateElement(Stream s) { return new GeometryState(handler, s); }
            protected override void WriteElement(Stream s, GeometryState element) { element.UnParse(s); }

            //public override void Add() { Add(new GeometryState(0, handler)); }
        }

        public class Mesh : AHandlerElement, IEquatable<Mesh>
        {
            private const int kRecommendedApiVersion = 1;
            private MLOD mOwner;

            #region Attributes
            private UInt32 mName;
            private GenericRCOLResource.GenericRCOLResource.ChunkReference mMaterialIndex;
            private GenericRCOLResource.GenericRCOLResource.ChunkReference mVertexFormatIndex;
            private GenericRCOLResource.GenericRCOLResource.ChunkReference mVertexBufferIndex;
            private GenericRCOLResource.GenericRCOLResource.ChunkReference mIndexBufferIndex;
            private ModelPrimitiveType mPrimitiveType;
            private MeshFlags mFlags;
            private UInt32 mStreamOffset;
            private Int32 mStartVertex;
            private Int32 mStartIndex;
            private Int32 mMinVertexIndex;
            private Int32 mVertexCount;
            private Int32 mPrimitiveCount;
            private GenericRCOLResource.GenericRCOLResource.ChunkReference mSkinControllerIndex;
            private GenericRCOLResource.GenericRCOLResource.ChunkReference mScaleOffsetIndex;
            private UIntList mJointReferences;
            private BoundingBox mBounds;
            private GeometryStateList mGeometryStates;
            private UInt32 mParentName;
            private Vector4 mMirrorPlane;
            #endregion

            #region Constructors
            public Mesh(EventHandler handler, MLOD owner)
                : base(handler)
            {
                mOwner = owner;

                mMaterialIndex = new GenericRCOLResource.GenericRCOLResource.ChunkReference(handler, 0);
                mVertexFormatIndex = new GenericRCOLResource.GenericRCOLResource.ChunkReference(handler, 0);
                mVertexBufferIndex = new GenericRCOLResource.GenericRCOLResource.ChunkReference(handler, 0);
                mIndexBufferIndex = new GenericRCOLResource.GenericRCOLResource.ChunkReference(handler, 0);
                mBounds = new BoundingBox(handler);
                mSkinControllerIndex = new GenericRCOLResource.GenericRCOLResource.ChunkReference(handler, 0);
                mJointReferences = new UIntList(handler);
                mGeometryStates = new GeometryStateList(handler);
                mScaleOffsetIndex = new GenericRCOLResource.GenericRCOLResource.ChunkReference(handler, 0);
                mMirrorPlane = new Vector4(handler);//mOwner.Version > 0x00000201
            }
            public Mesh(EventHandler handler, Mesh basis)
                : this(handler, basis.mOwner,
                basis.mName,
                basis.mMaterialIndex, basis.mVertexFormatIndex, basis.mVertexBufferIndex, basis.mIndexBufferIndex,
                basis.mPrimitiveType, basis.mFlags,
                basis.mStreamOffset, basis.mStartVertex, basis.mStartIndex, basis.mMinVertexIndex, basis.mVertexCount, basis.mPrimitiveCount,
                basis.mBounds, basis.mSkinControllerIndex,
                basis.mJointReferences, basis.mGeometryStates, basis.mScaleOffsetIndex,
                basis.mParentName, basis.mMirrorPlane
                ) { }
            public Mesh(EventHandler handler, MLOD owner,
                uint name,
                GenericRCOLResource.GenericRCOLResource.ChunkReference materialIndex, GenericRCOLResource.GenericRCOLResource.ChunkReference vertexFormatIndex,
                GenericRCOLResource.GenericRCOLResource.ChunkReference vertexBufferIndex, GenericRCOLResource.GenericRCOLResource.ChunkReference indexBufferIndex,
                ModelPrimitiveType primitiveType, MeshFlags flags,
                uint streamOffset, int startVertex, int startIndex, int minVertexIndex, int vertexCount,
                int primitiveCount,
                BoundingBox bounds, GenericRCOLResource.GenericRCOLResource.ChunkReference skinControllerIndex,
                UIntList jointReferences, GeometryStateList geometryStates, GenericRCOLResource.GenericRCOLResource.ChunkReference scaleOffsetIndex,
                uint parentName, Vector4 mirrorPlane)
                : base(handler)
            {
                mOwner = owner;

                mName = name;
                mMaterialIndex = new GenericRCOLResource.GenericRCOLResource.ChunkReference(handler, materialIndex);
                mVertexFormatIndex = new GenericRCOLResource.GenericRCOLResource.ChunkReference(handler, vertexFormatIndex);
                mVertexBufferIndex = new GenericRCOLResource.GenericRCOLResource.ChunkReference(handler, vertexBufferIndex);
                mIndexBufferIndex = new GenericRCOLResource.GenericRCOLResource.ChunkReference(handler, indexBufferIndex);
                mPrimitiveType = primitiveType;
                mFlags = flags;
                mStreamOffset = streamOffset;
                mStartVertex = startVertex;
                mStartIndex = startIndex;
                mMinVertexIndex = minVertexIndex;
                mVertexCount = vertexCount;
                mPrimitiveCount = primitiveCount;
                mBounds = new BoundingBox(handler, bounds);
                mSkinControllerIndex = new GenericRCOLResource.GenericRCOLResource.ChunkReference(handler, skinControllerIndex);
                mJointReferences = jointReferences == null ? null : new UIntList(handler, jointReferences);
                mGeometryStates = geometryStates == null ? null : new GeometryStateList(handler, geometryStates);
                mScaleOffsetIndex = new GenericRCOLResource.GenericRCOLResource.ChunkReference(handler, scaleOffsetIndex);
                if (mOwner.Version > 0x00000201)
                {
                    mParentName = parentName;
                    mMirrorPlane = new Vector4(handler, mirrorPlane);
                }
            }
            public Mesh(EventHandler handler, MLOD owner, Stream s) : base(handler) { mOwner = owner; Parse(s); }
            #endregion

            #region Data I/O
            private void Parse(Stream s)
            {
                BinaryReader br = new BinaryReader(s);
                long expectedSize = br.ReadUInt32();
                long start = s.Position;

                mName = br.ReadUInt32();
                mMaterialIndex = new GenericRCOLResource.GenericRCOLResource.ChunkReference(handler, s);
                mVertexFormatIndex = new GenericRCOLResource.GenericRCOLResource.ChunkReference(handler, s);
                mVertexBufferIndex = new GenericRCOLResource.GenericRCOLResource.ChunkReference(handler, s);
                mIndexBufferIndex = new GenericRCOLResource.GenericRCOLResource.ChunkReference(handler, s);
                uint val = br.ReadUInt32();
                mPrimitiveType = (ModelPrimitiveType)(val & 0x000000FF);
                mFlags = (MeshFlags)(val >> 8);
                mStreamOffset = br.ReadUInt32();
                mStartVertex = br.ReadInt32();
                mStartIndex = br.ReadInt32();
                mMinVertexIndex = br.ReadInt32();
                mVertexCount = br.ReadInt32();
                mPrimitiveCount = br.ReadInt32();
                mBounds = new BoundingBox(handler, s);
                mSkinControllerIndex = new GenericRCOLResource.GenericRCOLResource.ChunkReference(handler, s);
                mJointReferences = new UIntList(handler, s);
                mScaleOffsetIndex = new GenericRCOLResource.GenericRCOLResource.ChunkReference(handler, s);
                mGeometryStates = new GeometryStateList(handler, s);
                if (mOwner.Version > 0x00000201)
                {
                    mParentName = br.ReadUInt32();
                    mMirrorPlane = new Vector4(handler, s);
                }
                long actualSize = s.Position - start;
                if (actualSize != expectedSize)
                    throw new Exception(String.Format("Expected end at {0}, actual end was {1}", expectedSize,
                                                      actualSize));
            }

            public void UnParse(Stream s)
            {
                BinaryWriter bw = new BinaryWriter(s);
                long sizeOffset = s.Position;
                bw.Write(0);
                long start = s.Position;
                bw.Write(mName);
                mMaterialIndex.UnParse(s);
                mVertexFormatIndex.UnParse(s);
                mVertexBufferIndex.UnParse(s);
                mIndexBufferIndex.UnParse(s);
                bw.Write((UInt32)mPrimitiveType | ((UInt32)mFlags << 8));
                bw.Write(mStreamOffset);
                bw.Write(mStartVertex);
                bw.Write(mStartIndex);
                bw.Write(mMinVertexIndex);
                bw.Write(mVertexCount);
                bw.Write(mPrimitiveCount);
                mBounds.UnParse(s);
                mSkinControllerIndex.UnParse(s);
                mJointReferences.UnParse(s);
                mScaleOffsetIndex.UnParse(s);
                mGeometryStates.UnParse(s);
                if (mOwner.Version > 0x00000201)
                {
                    bw.Write(mParentName);
                    mMirrorPlane.UnParse(s);
                }
                long end = s.Position;
                long size = end - start;
                s.Seek(sizeOffset, SeekOrigin.Begin);
                bw.Write((uint)size);
                s.Seek(end, SeekOrigin.Begin);
            }
            #endregion

            #region AHandlerElement
            public override List<string> ContentFields
            {
                get
                {
                    List<string> fields = GetContentFields(GetType());

                    if (mOwner.Version < 0x00000202)
                    {
                        fields.Remove("ParentName");
                        fields.Remove("MirrorPlane");
                    }
                    return fields;
                }
            }
            #endregion

            #region IEquatable<Mesh>
            public bool Equals(Mesh other)
            {
                if (other == null) return false;

                return mName.Equals(other.mName)
                    && mMaterialIndex.Equals(other.mMaterialIndex)
                    && mVertexFormatIndex.Equals(other.mVertexFormatIndex)
                    && mVertexBufferIndex.Equals(other.mVertexBufferIndex)
                    && mIndexBufferIndex.Equals(other.mIndexBufferIndex)
                    && mPrimitiveType.Equals(other.mPrimitiveType)
                    && mFlags.Equals(other.mFlags)
                    && mStreamOffset.Equals(other.mStreamOffset)
                    && mStartVertex.Equals(other.mStartVertex)
                    && mStartIndex.Equals(other.mStartIndex)
                    && mMinVertexIndex.Equals(other.mMinVertexIndex)
                    && mVertexCount.Equals(other.mVertexCount)
                    && mPrimitiveCount.Equals(other.mPrimitiveCount)
                    && mSkinControllerIndex.Equals(other.mSkinControllerIndex)
                    && mScaleOffsetIndex.Equals(other.mScaleOffsetIndex)
                    && mJointReferences.Equals(other.mJointReferences)
                    && mBounds.Equals(other.mBounds)
                    && mGeometryStates.Equals(other.mGeometryStates)
                    && mParentName.Equals(other.mParentName)
                    && mMirrorPlane.Equals(other.mMirrorPlane)
                    && mOwner.Equals(other.mOwner)
                    ;
            }

            public override bool Equals(object obj) { return this.Equals(obj as Mesh); }

            public override int GetHashCode()
            {
                return mName.GetHashCode()
                    ^ mMaterialIndex.GetHashCode()
                    ^ mVertexFormatIndex.GetHashCode()
                    ^ mVertexBufferIndex.GetHashCode()
                    ^ mIndexBufferIndex.GetHashCode()
                    ^ mPrimitiveType.GetHashCode()
                    ^ mFlags.GetHashCode()
                    ^ mStreamOffset.GetHashCode()
                    ^ mStartVertex.GetHashCode()
                    ^ mStartIndex.GetHashCode()
                    ^ mMinVertexIndex.GetHashCode()
                    ^ mVertexCount.GetHashCode()
                    ^ mPrimitiveCount.GetHashCode()
                    ^ mSkinControllerIndex.GetHashCode()
                    ^ mScaleOffsetIndex.GetHashCode()
                    ^ mJointReferences.GetHashCode()
                    ^ mBounds.GetHashCode()
                    ^ mGeometryStates.GetHashCode()
                    ^ mParentName.GetHashCode()
                    ^ mMirrorPlane.GetHashCode()
                    ^ mOwner.GetHashCode()
                    ;
            }
            #endregion

            internal MLOD Owner
            {
                get { return mOwner; }
                set
                {
                    if (mOwner != value)
                    {
                        uint oldVersion = mOwner.Version;
                        mOwner = value;
                        if (mOwner.Version != oldVersion)
                            OnElementChanged();
                    }
                }
            }

            internal bool IsShadowCaster { get { return (mFlags & MeshFlags.ShadowCaster) != 0; } }

            #region ContentFields
            [ElementPriority(1)]
            public UInt32 Name
            {
                get { return mName; }
                set { if (mName != value) { mName = value; OnElementChanged(); } }
            }
            [ElementPriority(2)]
            public GenericRCOLResource.GenericRCOLResource.ChunkReference MaterialIndex
            {
                get { return mMaterialIndex; }
                set { if (mMaterialIndex != value) { mMaterialIndex = new GenericRCOLResource.GenericRCOLResource.ChunkReference(handler, value); OnElementChanged(); } }
            }
            [ElementPriority(3)]
            public GenericRCOLResource.GenericRCOLResource.ChunkReference VertexFormatIndex
            {
                get { return mVertexFormatIndex; }
                set { if (mVertexFormatIndex != value) { mVertexFormatIndex = new GenericRCOLResource.GenericRCOLResource.ChunkReference(handler, value); OnElementChanged(); } }
            }
            [ElementPriority(4)]
            public GenericRCOLResource.GenericRCOLResource.ChunkReference VertexBufferIndex
            {
                get { return mVertexBufferIndex; }
                set { if (mVertexBufferIndex != value) { mVertexBufferIndex = new GenericRCOLResource.GenericRCOLResource.ChunkReference(handler, value); OnElementChanged(); } }
            }
            [ElementPriority(5)]
            public GenericRCOLResource.GenericRCOLResource.ChunkReference IndexBufferIndex
            {
                get { return mIndexBufferIndex; }
                set { if (mIndexBufferIndex != value) { mIndexBufferIndex = new GenericRCOLResource.GenericRCOLResource.ChunkReference(handler, value); OnElementChanged(); } }
            }
            [ElementPriority(6)]
            public ModelPrimitiveType PrimitiveType
            {
                get { return mPrimitiveType; }
                set { if (mPrimitiveType != value) { mPrimitiveType = value; OnElementChanged(); } }
            }
            [ElementPriority(7)]
            public MeshFlags Flags
            {
                get { return mFlags; }
                set { if (mFlags != value) { mFlags = value; OnElementChanged(); } }
            }
            [ElementPriority(8)]
            public UInt32 StreamOffset
            {
                get { return mStreamOffset; }
                set { if (mStreamOffset != value) { mStreamOffset = value; OnElementChanged(); } }
            }
            [ElementPriority(9)]
            public Int32 StartVertex
            {
                get { return mStartVertex; }
                set { if (mStartVertex != value) { mStartVertex = value; OnElementChanged(); } }
            }
            [ElementPriority(10)]
            public Int32 StartIndex
            {
                get { return mStartIndex; }
                set { if (mStartIndex != value) { mStartIndex = value; OnElementChanged(); } }
            }
            [ElementPriority(11)]
            public Int32 MinVertexIndex
            {
                get { return mMinVertexIndex; }
                set { if (mMinVertexIndex != value) { mMinVertexIndex = value; OnElementChanged(); } }
            }
            [ElementPriority(12)]
            public Int32 VertexCount
            {
                get { return mVertexCount; }
                set { if (mVertexCount != value) { mVertexCount = value; OnElementChanged(); } }
            }
            [ElementPriority(13)]
            public Int32 PrimitiveCount
            {
                get { return mPrimitiveCount; }
                set { if (mPrimitiveCount != value) { mPrimitiveCount = value; OnElementChanged(); } }
            }
            [ElementPriority(14)]
            public BoundingBox Bounds
            {
                get { return mBounds; }
                set { if (mBounds != value) { mBounds = value; OnElementChanged(); } }
            }
            [ElementPriority(15)]
            public GenericRCOLResource.GenericRCOLResource.ChunkReference SkinControllerIndex
            {
                get { return mSkinControllerIndex; }
                set { if (mSkinControllerIndex != value) { mSkinControllerIndex = new GenericRCOLResource.GenericRCOLResource.ChunkReference(handler, value); OnElementChanged(); } }
            }
            [ElementPriority(16)]
            public GenericRCOLResource.GenericRCOLResource.ChunkReference ScaleOffsetIndex
            {
                get { return mScaleOffsetIndex; }
                set { if (mScaleOffsetIndex != value) { mScaleOffsetIndex = new GenericRCOLResource.GenericRCOLResource.ChunkReference(handler, value); OnElementChanged(); } }
            }
            [ElementPriority(17)]
            public UIntList JointReferences
            {
                get { return mJointReferences; }
                set { if (mJointReferences != value) { mJointReferences = value == null ? null : new UIntList(handler, value); OnElementChanged(); } }
            }
            [ElementPriority(18)]
            public GeometryStateList GeometryStates
            {
                get { return mGeometryStates; }
                set { if (mGeometryStates != value) { mGeometryStates = value == null ? null : new GeometryStateList(handler, value); OnElementChanged(); } }
            }
            [ElementPriority(19)]
            public UInt32 ParentName
            {
                get { if (mOwner.Version < 0x0202) throw new InvalidOperationException(); return mParentName; }
                set { if (mOwner.Version < 0x0202) throw new InvalidOperationException(); if (mParentName != value) { mParentName = value; OnElementChanged(); } }
            }
            [ElementPriority(20)]
            public Vector4 MirrorPlane
            {
                get { if (mOwner.Version < 0x0202) throw new InvalidOperationException(); return mMirrorPlane; }
                set { if (mOwner.Version < 0x0202) throw new InvalidOperationException(); if (mMirrorPlane != value) { mMirrorPlane = value; OnElementChanged(); } }
            }

            public string Value { get { return ValueBuilder; } }
            #endregion
        }

        public class MeshList : DependentList<Mesh>
        {
            private MLOD mOwner;

            public MeshList(EventHandler handler, MLOD owner) : base(handler) { mOwner = owner; }
            public MeshList(EventHandler handler, MLOD owner, Stream s) : this(handler, owner) { Parse(s); }
            public MeshList(EventHandler handler, MLOD owner, IEnumerable<Mesh> ilt)
                : base(null)
            {
                mOwner = owner;
                elementHandler = handler;
                foreach (Mesh t in ilt)
                {
                    base.Add(t);
                    t.Owner = mOwner;
                }
                this.handler = handler;
            }

            protected override Mesh CreateElement(Stream s) { return new Mesh(handler, mOwner, s); }
            protected override void WriteElement(Stream s, Mesh element) { element.UnParse(s); }

            public override void Add() { base.Add(new Mesh(handler, mOwner)); }
            public override void Add(Mesh item) { item.Owner = mOwner; base.Add(item); }
            //And, of course, all the other ways of getting an element into a list... bad, bad, bad...  But this covers s3pe's usage.
        }
        #endregion

        #region Content Fields
        [ElementPriority(11)]
        public uint Version
        {
            get { return mVersion; }
            set { if (mVersion != value) { mVersion = value; OnRCOLChanged(this, EventArgs.Empty); } }
        }

        [ElementPriority(12)]
        public MeshList Meshes
        {
            get { return mMeshes; }
            set { if (mMeshes != value) { mMeshes = value; OnRCOLChanged(this, EventArgs.Empty); } }
        }

        public string Value { get { return ValueBuilder; } }
        #endregion
    }

    #region Some flags here for some reason
    [Flags]
    public enum MeshFlags : uint
    {
        BasinInterior = 0x00000001,
        HDExteriorLit = 0x00000002,
        PortalSide = 0x00000004,
        DropShadow = 0x00000008,
        ShadowCaster = 0x00000010,
        Foundation = 0x00000020,
        Pickable = 0x00000040

    }
    public enum ModelPrimitiveType
    {
        PointList,
        LineList,
        LineStrip,
        TriangleList,
        TriangleFan,
        TriangleStrip,
        RectList,
        QuadList,
        DisplayList
    }
    #endregion
}