﻿using System;
using System.Collections.Generic;
using System.IO;
using Regul.S3PI.Interfaces;

namespace Regul.S3PI.Resources
{
    public class RigResource : AResource
    {
        #region Attributes
        RigFormat rigFormat = 0;
        //RawGranny
        byte[] granny2Data = null;
        //WrappedGranny - not done
        //Clear
        uint major = 0x00000004;
        uint minor = 0x00000002;
        BoneList bones = null;
        string skeletonName = null;
        IKChainList ikChains = null;//major >= 4
        #endregion

        public RigResource(Stream s) : base(s) { if (stream == null) { rigFormat = RigFormat.Clear; stream = UnParse(); OnResourceChanged(this, EventArgs.Empty); } stream.Position = 0; Parse(stream); }

        #region Data I/O
        private void Parse(Stream s)
        {
            BinaryReader r = new BinaryReader(s);
            uint dw1 = r.ReadUInt32();
            uint dw2 = r.ReadUInt32();
            s.Position = 0;

            if (dw1 == 0x8EAF13DE && dw2 == 0x00000000)
            {
                rigFormat = RigFormat.WrappedGranny;
                //ParseWrappedGranny(s);
                ParseRawGranny(s);
            }
            else if ((dw1 == 0x00000003 || dw1 == 0x00000004) && ((dw2 == 0x00000001 || dw2 == 0x00000002)))
            {
                rigFormat = RigFormat.Clear;
                ParseClear(s);
            }
            else
            {
                rigFormat = RigFormat.RawGranny;
                ParseRawGranny(s);
            }
        }

        private void ParseRawGranny(Stream s)
        {
            granny2Data = new byte[s.Length];
            s.Read(granny2Data, 0, granny2Data.Length);
        }

        private void ParseWrappedGranny(Stream s)
        {
            throw new NotImplementedException();
        }

        private void ParseClear(Stream s)
        {
            BinaryReader r = new BinaryReader(s);
            major = r.ReadUInt32();
            minor = r.ReadUInt32();
            bones = new BoneList(OnResourceChanged, s);
            skeletonName = new String(r.ReadChars(r.ReadInt32()));
            if (major >= 4)
                ikChains = new IKChainList(OnResourceChanged, s);
        }

        protected override Stream UnParse()
        {
            switch (rigFormat)
            {
                case RigFormat.WrappedGranny:
                    return UnParseRawGranny();
                case RigFormat.RawGranny:
                    //return UnParseWrappedGranny();
                    return UnParseRawGranny();
                case RigFormat.Clear:
                    return UnParseClear();
            }
            throw new InvalidOperationException("Unknown RIG format: " + rigFormat);
        }

        private Stream UnParseRawGranny()
        {
            MemoryStream s = new MemoryStream();
            if (granny2Data == null) granny2Data = new byte[0];
            s.Write(granny2Data, 0, granny2Data.Length);
            s.Flush();
            return s;
        }

        private Stream UnParseWrappedGranny()
        {
            throw new NotImplementedException();
        }

        private Stream UnParseClear()
        {
            MemoryStream s = new MemoryStream();
            BinaryWriter w = new BinaryWriter(s);

            w.Write(major);
            w.Write(minor);

            if (bones == null) bones = new BoneList(OnResourceChanged);
            bones.UnParse(s);

            if (skeletonName == null) skeletonName = "";
            w.Write(skeletonName.Length);
            w.Write(skeletonName.ToCharArray());

            if (major >= 4)
            {
                if (ikChains == null) ikChains = new IKChainList(OnResourceChanged);
                ikChains.UnParse(s);
            }

            return s;
        }
        #endregion

        #region Sub-Types
        enum RigFormat
        {
            RawGranny,
            WrappedGranny,
            Clear,
        }

        public class Bone : AHandlerElement, IEquatable<Bone>
        {
            #region Attributes
            Vertex position;
            Quaternion orientation;
            Vertex scaling;
            string name;
            int opposingBoneIndex;
            int parentBoneIndex;
            uint hash;
            uint unknown2;
            #endregion

            #region Constructors
            public Bone(EventHandler handler) : base(handler)
            {
                position = new Vertex(handler);
                orientation = new Quaternion(handler);
                scaling = new Vertex(handler);
                name = "";
            }
            public Bone(EventHandler handler, Stream s) : base(handler) { Parse(s); }
            public Bone(EventHandler handler, Bone basis)
                : this(handler, basis.position, basis.orientation, basis.scaling,
                basis.name, basis.opposingBoneIndex, basis.parentBoneIndex, basis.hash, basis.unknown2) { }
            public Bone(EventHandler handler,
                Vertex position, Quaternion quaternion, Vertex scaling,
                string name, int opposingBoneIndex, int parentBoneIndex, uint hash, uint unknown2)
                : base(handler)
            {
                this.position = new Vertex(handler, position);
                this.orientation = new Quaternion(handler, quaternion);
                this.scaling = new Vertex(handler, scaling);
                this.name = name;
                this.opposingBoneIndex = opposingBoneIndex;
                this.parentBoneIndex = parentBoneIndex;
                this.hash = hash;
                this.unknown2 = unknown2;
            }
            #endregion

            #region Data I/O
            private void Parse(Stream s)
            {
                BinaryReader r = new BinaryReader(s);
                position = new Vertex(handler, s);
                orientation = new Quaternion(handler, s);
                scaling = new Vertex(handler, s);
                name = new String(r.ReadChars(r.ReadInt32()));
                opposingBoneIndex = r.ReadInt32();
                parentBoneIndex = r.ReadInt32();
                hash = r.ReadUInt32();
                unknown2 = r.ReadUInt32();
            }

            internal void UnParse(Stream s)
            {
                BinaryWriter w = new BinaryWriter(s);
                if (position == null) position = new Vertex(handler);
                position.UnParse(s);
                if (orientation == null) orientation = new Quaternion(handler);
                orientation.UnParse(s);
                if (scaling == null) scaling = new Vertex(handler);
                scaling.UnParse(s);
                if (name == null) name = "";
                w.Write(name.Length);
                w.Write(name.ToCharArray());
                w.Write(opposingBoneIndex);
                w.Write(parentBoneIndex);
                w.Write(hash);
                w.Write(unknown2);
            }
            #endregion

            #region AHandlerElement Members
            public override List<string> ContentFields { get { return GetContentFields(GetType()); } }
            #endregion

            #region IEquatable<Bone>
            public bool Equals(Bone other)
            {
                return position.Equals(other.position) && orientation.Equals(other.orientation) && scaling.Equals(other.scaling) && name.Equals(other.name)
                    && opposingBoneIndex.Equals(other.opposingBoneIndex) && parentBoneIndex.Equals(other.parentBoneIndex) && hash.Equals(other.hash) && unknown2.Equals(other.unknown2);
            }

            public override bool Equals(object obj)
            {
                return obj as Bone != null && this.Equals(obj as Bone);
            }

            public override int GetHashCode()
            {
                return position.GetHashCode() ^ orientation.GetHashCode() ^ scaling.GetHashCode() ^ name.GetHashCode()
                    ^ opposingBoneIndex.GetHashCode() ^ parentBoneIndex.GetHashCode() ^ hash.GetHashCode() ^ unknown2.GetHashCode();
            }
            #endregion

            #region Content Fields
            [ElementPriority(1)]
            public Vertex Position { get { return position; } set { if (!position.Equals(value)) { position = new Vertex(handler, value); OnElementChanged(); } } }
            [ElementPriority(2)]
            public Quaternion Orientation { get { return orientation; } set { if (!orientation.Equals(value)) { orientation = new Quaternion(handler, value); OnElementChanged(); } } }
            [ElementPriority(3)]
            public Vertex Scaling { get { return scaling; } set { if (!scaling.Equals(value)) { scaling = new Vertex(handler, value); OnElementChanged(); } } }
            [ElementPriority(4)]
            public String Name { get { return name; } set { if (name != value) { name = value; OnElementChanged(); } } }
            [ElementPriority(5)]
            public int OpposingBoneIndex { get { return opposingBoneIndex; } set { if (opposingBoneIndex != value) { opposingBoneIndex = value; OnElementChanged(); } } }
            [ElementPriority(6)]
            public int ParentBoneIndex { get { return parentBoneIndex; } set { if (parentBoneIndex != value) { parentBoneIndex = value; OnElementChanged(); } } }
            [ElementPriority(7)]
            public uint Hash { get { return hash; } set { if (hash != value) { hash = value; OnElementChanged(); } } }
            [ElementPriority(8)]
            public uint Unknown2 { get { return unknown2; } set { if (unknown2 != value) { unknown2 = value; OnElementChanged(); } } }

            public string Value { get { return ValueBuilder; } }
            #endregion
        }

        public class BoneList : DependentList<Bone>
        {
            public BoneList(EventHandler handler) : base(handler) { }
            public BoneList(EventHandler handler, Stream s) : base(handler, s) { }
            public BoneList(EventHandler handler, IEnumerable<Bone> lb) : base(handler, lb) { }

            protected override Bone CreateElement(Stream s) { return new Bone(elementHandler, s); }
            protected override void WriteElement(Stream s, Bone element) { element.UnParse(s); }
        }

        public class IKElement : AHandlerElement, IEquatable<IKElement>
        {
            #region Attributes
            IntList bones;
            int infoNode0Index;
            int infoNode1Index;
            int infoNode2Index;
            int infoNode3Index;
            int infoNode4Index;
            int infoNode5Index;
            int infoNode6Index;
            int infoNode7Index;
            int infoNode8Index;
            int infoNode9Index;
            int infoNodeAIndex;
            int poleVectorIndex;
            int slotInfoIndex;
            int slotOffsetIndex;
            int rootIndex;
            #endregion

            #region Constructors
            public IKElement(EventHandler handler) : base(handler)
            {
                bones = new IntList(handler);
            }
            public IKElement(EventHandler handler, Stream s) : base(handler) { Parse(s); }
            public IKElement(EventHandler handler, IKElement basis)
                : this(handler, basis.bones,
                basis.infoNode0Index, basis.infoNode1Index,
                basis.infoNode2Index, basis.infoNode3Index,
                basis.infoNode4Index, basis.infoNode5Index, basis.infoNode6Index,
                basis.infoNode7Index, basis.infoNode8Index,
                basis.infoNode9Index, basis.infoNodeAIndex,
                basis.poleVectorIndex, basis.slotInfoIndex, basis.slotOffsetIndex, basis.rootIndex) { }
            public IKElement(EventHandler handler,
                IntList bones,
                int infoNode0Index, int infoNode1Index,
                int infoNode2Index, int infoNode3Index,
                int infoNode4Index, int infoNode5Index, int infoNode6Index,
                int infoNode7Index, int infoNode8Index,
                int infoNode9Index, int infoNodeAIndex,
                int poleVectorIndex, int slotInfoIndex, int slotOffsetIndex, int rootIndex)
                : base(handler)
            {
                this.bones = new IntList(handler, bones);
                this.infoNode0Index = infoNode0Index;
                this.infoNode1Index = infoNode1Index;
                this.infoNode2Index = infoNode2Index;
                this.infoNode3Index = infoNode3Index;
                this.infoNode4Index = infoNode4Index;
                this.infoNode5Index = infoNode5Index;
                this.infoNode6Index = infoNode6Index;
                this.infoNode7Index = infoNode7Index;
                this.infoNode8Index = infoNode8Index;
                this.infoNode9Index = infoNode9Index;
                this.infoNodeAIndex = infoNodeAIndex;
                this.poleVectorIndex = poleVectorIndex;
                this.slotInfoIndex = slotInfoIndex;
                this.slotOffsetIndex = slotOffsetIndex;
                this.rootIndex = rootIndex;
            }
            #endregion

            #region Data I/O
            private void Parse(Stream s)
            {
                BinaryReader r = new BinaryReader(s);
                this.bones = new IntList(handler, s);
                this.infoNode0Index = r.ReadInt32();
                this.infoNode1Index = r.ReadInt32();
                this.infoNode2Index = r.ReadInt32();
                this.infoNode3Index = r.ReadInt32();
                this.infoNode4Index = r.ReadInt32();
                this.infoNode5Index = r.ReadInt32();
                this.infoNode6Index = r.ReadInt32();
                this.infoNode7Index = r.ReadInt32();
                this.infoNode8Index = r.ReadInt32();
                this.infoNode9Index = r.ReadInt32();
                this.infoNodeAIndex = r.ReadInt32();
                this.poleVectorIndex = r.ReadInt32();
                this.slotInfoIndex = r.ReadInt32();
                this.slotOffsetIndex = r.ReadInt32();
                this.rootIndex = r.ReadInt32();
            }

            internal void UnParse(Stream s)
            {
                BinaryWriter w = new BinaryWriter(s);
                if (bones == null) bones = new IntList(handler);
                bones.UnParse(s);
                w.Write(infoNode0Index);
                w.Write(infoNode1Index);
                w.Write(infoNode2Index);
                w.Write(infoNode3Index);
                w.Write(infoNode4Index);
                w.Write(infoNode5Index);
                w.Write(infoNode6Index);
                w.Write(infoNode7Index);
                w.Write(infoNode8Index);
                w.Write(infoNode9Index);
                w.Write(infoNodeAIndex);
                w.Write(poleVectorIndex);
                w.Write(slotInfoIndex);
                w.Write(slotOffsetIndex);
                w.Write(rootIndex);
            }
            #endregion

            #region AHandlerElement Members
            public override List<string> ContentFields { get { return GetContentFields(GetType()); } }
            #endregion

            #region IEquatable<IKElement>
            public bool Equals(IKElement other)
            {
                return bones.Equals(other.bones)
                    && infoNode0Index.Equals(other.infoNode0Index) && infoNode1Index.Equals(other.infoNode1Index)
                    && infoNode2Index.Equals(other.infoNode2Index) && infoNode3Index.Equals(other.infoNode3Index)
                    && infoNode4Index.Equals(other.infoNode4Index) && infoNode5Index.Equals(other.infoNode5Index) && infoNode6Index.Equals(other.infoNode6Index)
                    && infoNode7Index.Equals(other.infoNode7Index) && infoNode8Index.Equals(other.infoNode8Index)
                    && infoNode9Index.Equals(other.infoNode9Index) && infoNodeAIndex.Equals(other.infoNodeAIndex)
                    && poleVectorIndex.Equals(other.poleVectorIndex)
                    && slotInfoIndex.Equals(other.slotInfoIndex)
                    && slotOffsetIndex.Equals(other.slotOffsetIndex)
                    && rootIndex.Equals(other.rootIndex)
                    ;
            }

            public override bool Equals(object obj)
            {
                return obj as IKElement != null && this.Equals(obj as IKElement);
            }

            public override int GetHashCode()
            {
                return bones.GetHashCode()
                    ^ infoNode0Index ^ infoNode1Index
                    ^ infoNode2Index ^ infoNode3Index
                    ^ infoNode4Index ^ infoNode5Index ^ infoNode6Index
                    ^ infoNode7Index ^ infoNode8Index
                    ^ infoNode9Index ^ infoNodeAIndex
                    ^ poleVectorIndex
                    ^ slotInfoIndex
                    ^ slotOffsetIndex
                    ^ rootIndex
                    ;
            }
            #endregion

            #region Content Fields
            [ElementPriority(1)]
            public IntList Bones { get { return bones; } set { if (!bones.Equals(value)) { bones = new IntList(handler, value); OnElementChanged(); } } }
            [ElementPriority(2)]
            public int InfoNode0Index { get { return infoNode0Index; } set { if (infoNode0Index != value) { infoNode0Index = value; OnElementChanged(); } } }
            [ElementPriority(3)]
            public int InfoNode1Index { get { return infoNode1Index; } set { if (infoNode1Index != value) { infoNode1Index = value; OnElementChanged(); } } }
            [ElementPriority(4)]
            public int InfoNode2Index { get { return infoNode2Index; } set { if (infoNode2Index != value) { infoNode2Index = value; OnElementChanged(); } } }
            [ElementPriority(5)]
            public int InfoNode3Index { get { return infoNode3Index; } set { if (infoNode3Index != value) { infoNode3Index = value; OnElementChanged(); } } }
            [ElementPriority(6)]
            public int InfoNode4Index { get { return infoNode4Index; } set { if (infoNode4Index != value) { infoNode4Index = value; OnElementChanged(); } } }
            [ElementPriority(7)]
            public int InfoNode5Index { get { return infoNode5Index; } set { if (infoNode5Index != value) { infoNode5Index = value; OnElementChanged(); } } }
            [ElementPriority(8)]
            public int InfoNode6Index { get { return infoNode6Index; } set { if (infoNode6Index != value) { infoNode6Index = value; OnElementChanged(); } } }
            [ElementPriority(9)]
            public int InfoNode7Index { get { return infoNode7Index; } set { if (infoNode7Index != value) { infoNode7Index = value; OnElementChanged(); } } }
            [ElementPriority(10)]
            public int InfoNode8Index { get { return infoNode8Index; } set { if (infoNode8Index != value) { infoNode8Index = value; OnElementChanged(); } } }
            [ElementPriority(11)]
            public int InfoNode9Index { get { return infoNode9Index; } set { if (infoNode9Index != value) { infoNode9Index = value; OnElementChanged(); } } }
            [ElementPriority(12)]
            public int InfoNodeAIndex { get { return infoNodeAIndex; } set { if (infoNodeAIndex != value) { infoNodeAIndex = value; OnElementChanged(); } } }
            [ElementPriority(13)]
            public int PoleVectorIndex { get { return poleVectorIndex; } set { if (poleVectorIndex != value) { poleVectorIndex = value; OnElementChanged(); } } }
            [ElementPriority(14)]
            public int SlotInfoIndex { get { return slotInfoIndex; } set { if (slotInfoIndex != value) { slotInfoIndex = value; OnElementChanged(); } } }
            [ElementPriority(15)]
            public int SlotOffsetIndex { get { return slotOffsetIndex; } set { if (slotOffsetIndex != value) { slotOffsetIndex = value; OnElementChanged(); } } }
            [ElementPriority(16)]
            public int RootIndex { get { return rootIndex; } set { if (rootIndex != value) { rootIndex = value; OnElementChanged(); } } }

            public string Value { get { return ValueBuilder; } }
            #endregion
        }

        public class IKChainList : DependentList<IKElement>
        {
            public IKChainList(EventHandler handler) : base(handler) { }
            public IKChainList(EventHandler handler, Stream s) : base(handler, s) { }
            public IKChainList(EventHandler handler, IEnumerable<IKElement> lb) : base(handler, lb) { }

            protected override IKElement CreateElement(Stream s) { return new IKElement(elementHandler, s); }
            protected override void WriteElement(Stream s, IKElement element) { element.UnParse(s); }
        }

        #endregion

        public override List<string> ContentFields
        {
            get
            {
                switch (rigFormat)
                {
                    case RigFormat.RawGranny:
                        return new List<string>(new string[] { "RawGranny", });
                    case RigFormat.WrappedGranny:
                        return new List<string>(new string[] { "RawGranny", });
                    case RigFormat.Clear:
                        List<string> res = GetContentFields(GetType());
                        res.Remove("RawGranny");
                        if (major < 4)
                        {
                            res.Remove("IKChains");
                        }
                        return res;
                }
                throw new InvalidOperationException("Unknown RIG format: " + rigFormat);
            }
        }

        #region Content Fields
        [ElementPriority(1)]
        public BinaryReader RawGranny
        {
            get { return new BinaryReader(UnParse()); }
            set
            {
                if (value.BaseStream.CanSeek) { value.BaseStream.Position = 0; Parse(value.BaseStream); }
                else
                {
                    MemoryStream ms = new MemoryStream();
                    byte[] buffer = new byte[1024 * 1024];
                    for (int read = value.BaseStream.Read(buffer, 0, buffer.Length); read > 0; read = value.BaseStream.Read(buffer, 0, buffer.Length))
                        ms.Write(buffer, 0, read);
                    Parse(ms);
                }
                OnResourceChanged(this, EventArgs.Empty);
            }
        }

        [ElementPriority(1)]
        public uint Major { get { return major; } set { if (major != value) { major = value; OnResourceChanged(this, EventArgs.Empty); } } }
        [ElementPriority(2)]
        public uint Minor { get { return minor; } set { if (minor != value) { minor = value; OnResourceChanged(this, EventArgs.Empty); } } }
        [ElementPriority(3)]
        public BoneList Bones { get { return bones; } set { if (!bones.Equals(value)) { bones = value == null ? null : new BoneList(OnResourceChanged, value); OnResourceChanged(this, EventArgs.Empty); } } }
        [ElementPriority(4)]
        public String SkeletonName { get { return skeletonName; } set { if (skeletonName != value) { skeletonName = value; OnResourceChanged(this, EventArgs.Empty); } } }
        [ElementPriority(5)]
        public IKChainList IKChains { get { if (major < 4) throw new InvalidOperationException(); return ikChains; } set { if (major < 4) throw new InvalidOperationException(); if (!ikChains.Equals(value)) { ikChains = value == null ? null : new IKChainList(OnResourceChanged, value); OnResourceChanged(this, EventArgs.Empty); } } }

        public string Value { get { return ValueBuilder; } }
        #endregion

    }

    /// <summary>
    /// ResourceHandler for RigResource wrapper
    /// </summary>
    public class RigResourceHandler : AResourceHandler
    {
        public RigResourceHandler()
        {
            this.Add(typeof(RigResource), new List<string>(new string[] { "0x8EAF13DE" }));
        }
    }
}