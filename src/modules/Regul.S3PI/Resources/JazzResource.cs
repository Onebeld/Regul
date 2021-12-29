using System;
using System.Collections.Generic;
using System.IO;
using Regul.S3PI.Interfaces;

namespace Regul.S3PI.Resources
{
    public abstract class JazzChunk : ARCOLBlock
    {
        protected static class Deadbeef
        {
            public static void Parse(Stream s)
            {
                uint filler = new BinaryReader(s).ReadUInt32();
                if (filler != 0xDEADBEEF)
                    throw new InvalidDataException($"Invalid filler read: 0x{filler:X8}; expected: 0x{0xDEADBEEF:X8}; at 0x{s.Position:X8}");
            }
            public static void UnParse(Stream s) { new BinaryWriter(s).Write(0xDEADBEEF); }
        }

        protected static class CloseDgn
        {
            static UInt32 _closeDgn = (uint)FOURCC("/DGN");
            public static void Parse(Stream s)
            {
                uint filler = new BinaryReader(s).ReadUInt32();
                if (filler != _closeDgn)
                    throw new InvalidDataException($"Invalid filler read: 0x{filler:X8}; expected: 0x{_closeDgn:X8}; at 0x{s.Position:X8}");
            }
            public static void UnParse(Stream s) { new BinaryWriter(s).Write(_closeDgn); }
        }

        protected static void ExpectZero(Stream s, int size = sizeof(uint))
        {
            while (s.Position % size != 0)
            {
                int b = s.ReadByte();
                if (b == -1) break;
                if (b != 0)
                    throw new InvalidDataException($"Invalid padding read: 0x{b:X2}; expected: 0x00; at 0x{s.Position:X8}");
            }
        }
        protected static void PadZero(Stream s, int size = sizeof(uint)) { while (s.Position % size != 0) s.WriteByte(0); }

        public enum AnimationPriority : uint
        {
            Default = 0xfffffffe,//-2,
            Broadcast = 0xffffffff,//-1,
            Unset = 0,
            Low = 6000,
            LowPlus = 8000,
            Normal = 10000,
            NormalPlus = 15000,
            FacialIdle = 17500,
            High = 20000,
            HighPlus = 25000,
            CarryRight = 30000,
            CarryRightPlus = 35000,
            CarryLeft = 40000,
            CarryLeftPlus = 45000,
            Ultra = 50000,
            UltraPlus = 55000,
            LookAt = 60000,
        }

        public enum AwarenessLevel
        {
            ThoughtBubble,
            OverlayFace,
            OverlayHead,
            OverlayBothArms,
            OverlayUpperbody,
            OverlayNone,
            Unset
        }

        public class ChunkReferenceList : DependentList<GenericRCOLResource.GenericRCOLResource.ChunkReference>
        {
            #region Constructors
            public ChunkReferenceList(EventHandler handler) : base(handler) { }
            public ChunkReferenceList(EventHandler handler, IEnumerable<GenericRCOLResource.GenericRCOLResource.ChunkReference> ilt) : base(handler, ilt) { }
            public ChunkReferenceList(EventHandler handler, Stream s) : base(handler, s) { }
            #endregion

            #region DependentList<Animation>
            protected override GenericRCOLResource.GenericRCOLResource.ChunkReference CreateElement(Stream s) { return new GenericRCOLResource.GenericRCOLResource.ChunkReference(handler, s); }
            protected override void WriteElement(Stream s, GenericRCOLResource.GenericRCOLResource.ChunkReference element) { element.UnParse(s); }
            #endregion
        }

        #region Constructors
        public JazzChunk(EventHandler handler) : base(handler, null) { }
        public JazzChunk(EventHandler handler, Stream s) : base(handler, s) { }
        #endregion

        protected override void Parse(Stream s)
        {
            uint tag = new BinaryReader(s).ReadUInt32();
            if (tag != (uint)FOURCC(Tag))
                throw new InvalidDataException($"Invalid Tag read: '{FOURCC(tag)}'; expected: '{Tag}'; at 0x{s.Position:X8}");
        }

        public override Stream UnParse()
        {
            MemoryStream ms = new MemoryStream();
            new BinaryWriter(ms).Write((uint)FOURCC(Tag));
            return ms;
        }
    }

    #region Definition Chunks
    public class JazzStateMachine : JazzChunk
    {
        const string _tag = "S_SM";

        [Flags]
        public enum Flags : uint
        {
            Default = 0x01,
            UnilateralActor = 0x01,
            PinAllResources = 0x02,
            BlendMotionAccumulation = 0x04,
            HoldAllPoses = 0x08
        }

        #region Attributes
        uint _version = 0x0202;
        uint _nameHash;
        ChunkReferenceList _actorDefinitionIndexes;
        ChunkReferenceList _propertyDefinitionIndexes;
        ChunkReferenceList _stateIndexes;
        AnimationList _animations;
        //0xDEADBEEF
        Flags _properties;
        AnimationPriority _automationPriority;
        AwarenessLevel _awarenessOverlayLevel;
        uint _unknown2;
        uint _unknown3;
        uint _unknown4;
        uint _unknown5;
        #endregion

        #region Constructors
        public JazzStateMachine(EventHandler handler) : base(handler, null) { }
        public JazzStateMachine(EventHandler handler, Stream s) : base(handler, s) { }
        public JazzStateMachine(EventHandler handler, JazzStateMachine basis)
            : this(handler
            , basis._version, basis._nameHash, basis._actorDefinitionIndexes, basis._propertyDefinitionIndexes, basis._stateIndexes, basis._animations
            , basis._properties, basis._automationPriority, basis._awarenessOverlayLevel, basis._unknown2, basis._unknown3, basis._unknown4, basis._unknown5
            )
        { }
        public JazzStateMachine(EventHandler handler
            , uint version
            , uint nameHash
            , IEnumerable<GenericRCOLResource.GenericRCOLResource.ChunkReference> actorDefinitionIndexes
            , IEnumerable<GenericRCOLResource.GenericRCOLResource.ChunkReference> propertyDefinitionIndexes
            , IEnumerable<GenericRCOLResource.GenericRCOLResource.ChunkReference> stateIndexes
            , IEnumerable<Animation> animations
            , Flags properties
            , AnimationPriority automationPriority
            , AwarenessLevel awarenessOverlayLevel
            , uint unknown2
            , uint unknown3
            , uint unknown4
            , uint unknown5)
            : base(handler, null)
        {
            _version = version;
            _nameHash = nameHash;
            _actorDefinitionIndexes = new ChunkReferenceList(handler, actorDefinitionIndexes);
            _propertyDefinitionIndexes = new ChunkReferenceList(handler, propertyDefinitionIndexes);
            _stateIndexes = new ChunkReferenceList(handler, stateIndexes);
            _animations = new AnimationList(handler, animations);
            _properties = properties;
            _automationPriority = automationPriority;
            _awarenessOverlayLevel = awarenessOverlayLevel;
            _unknown2 = unknown2;
            _unknown3 = unknown3;
            _unknown4 = unknown4;
            _unknown5 = unknown5;
        }
        #endregion

        #region ARCOLBlock
        [ElementPriority(2)]
        public override string Tag => _tag;

        [ElementPriority(3)]
        public override uint ResourceType => 0x02D5DF13;

        protected override void Parse(Stream s)
        {
            base.Parse(s);

            BinaryReader r = new BinaryReader(s);
            _version = r.ReadUInt32();
            _nameHash = r.ReadUInt32();
            _actorDefinitionIndexes = new ChunkReferenceList(handler, s);
            _propertyDefinitionIndexes = new ChunkReferenceList(handler, s);
            _stateIndexes = new ChunkReferenceList(handler, s);
            _animations = new AnimationList(handler, s);
            Deadbeef.Parse(s);
            _properties = (Flags)r.ReadUInt32();
            _automationPriority = (AnimationPriority)r.ReadUInt32();
            _awarenessOverlayLevel = (AwarenessLevel)r.ReadUInt32();
            _unknown2 = r.ReadUInt32();
            _unknown3 = r.ReadUInt32();
            _unknown4 = r.ReadUInt32();
            _unknown5 = r.ReadUInt32();
        }

        public override Stream UnParse()
        {
            Stream ms = base.UnParse();
            BinaryWriter w = new BinaryWriter(ms);

            w.Write(_version);
            w.Write(_nameHash);
            if (_actorDefinitionIndexes == null) _actorDefinitionIndexes = new ChunkReferenceList(handler);
            _actorDefinitionIndexes.UnParse(ms);
            if (_propertyDefinitionIndexes == null) _propertyDefinitionIndexes = new ChunkReferenceList(handler);
            _propertyDefinitionIndexes.UnParse(ms);
            if (_stateIndexes == null) _stateIndexes = new ChunkReferenceList(handler);
            _stateIndexes.UnParse(ms);
            if (_animations == null) _animations = new AnimationList(handler);
            _animations.UnParse(ms);
            Deadbeef.UnParse(ms);
            w.Write((uint)_properties);
            w.Write((uint)_automationPriority);
            w.Write((uint)_awarenessOverlayLevel);
            w.Write(_unknown2);
            w.Write(_unknown3);
            w.Write(_unknown4);
            w.Write(_unknown5);

            return ms;
        }

        // public override AHandlerElement Clone(EventHandler handler) { return new JazzStateMachine(requestedApiVersion, handler, this); }
        #endregion

        #region Sub-types
        public class Animation : AHandlerElement, IEquatable<Animation>
        {
            #region Attributes
            uint _nameHash;
            uint _actor1Hash;
            uint _actor2Hash;
            #endregion

            #region Constructors
            public Animation(EventHandler handler) : base(handler) { }
            public Animation(EventHandler handler, Stream s) : base(handler) { Parse(s); }
            public Animation(EventHandler handler, Animation basis)
                : this(handler
                , basis._nameHash
                , basis._actor1Hash
                , basis._actor2Hash
                ) { }
            public Animation(EventHandler handler
                , uint nameHash
                , uint actor1Hash
                , uint actor2Hash)
                : base(handler)
            {
                _nameHash = nameHash;
                _actor1Hash = actor1Hash;
                _actor2Hash = actor2Hash;
            }
            #endregion

            #region Data I/O
            void Parse(Stream s)
            {
                BinaryReader r = new BinaryReader(s);
                _nameHash = r.ReadUInt32();
                _actor1Hash = r.ReadUInt32();
                _actor2Hash = r.ReadUInt32();
            }

            internal void UnParse(Stream s)
            {
                BinaryWriter w = new BinaryWriter(s);
                w.Write(_nameHash);
                w.Write(_actor1Hash);
                w.Write(_actor2Hash);
            }
            #endregion

            #region AHandlerElement
            // public override AHandlerElement Clone(EventHandler handler) { return new Animation(requestedApiVersion, handler, this); }
            public override List<string> ContentFields => GetContentFields(GetType());

            #endregion

            #region IEquatable<Animation>
            public bool Equals(Animation other)
            {
                return _nameHash.Equals(other._nameHash) && _actor1Hash.Equals(other._actor1Hash) && _actor2Hash.Equals(other._actor2Hash);
            }
            #endregion

            #region ContentFields
            [ElementPriority(1)]
            public uint NameHash { get => _nameHash;
                set { if (_nameHash != value) { _nameHash = value; OnElementChanged(); } } }
            [ElementPriority(2)]
            public uint Actor1Hash { get => _actor1Hash;
                set { if (_actor1Hash != value) { _actor1Hash = value; OnElementChanged(); } } }
            [ElementPriority(3)]
            public uint Actor2Hash { get => _actor2Hash;
                set { if (_actor2Hash != value) { _actor2Hash = value; OnElementChanged(); } } }

            public string Value => ValueBuilder.Replace("\n", "; ");

            #endregion
        }
        public class AnimationList : DependentList<Animation>
        {
            #region Constructors
            public AnimationList(EventHandler handler) : base(handler) { }
            public AnimationList(EventHandler handler, IEnumerable<Animation> ilt) : base(handler, ilt) { }
            public AnimationList(EventHandler handler, Stream s) : base(handler, s) { }
            #endregion

            #region DependentList<Animation>
            protected override Animation CreateElement(Stream s) { return new Animation(handler, s); }
            protected override void WriteElement(Stream s, Animation element) { element.UnParse(s); }
            #endregion
        }
        #endregion

        #region ContentFields
        [ElementPriority(11)]
        public uint Version { get => _version;
            set { if (_version != value) { _version = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(12)]
        public uint NameHash { get => _nameHash;
            set { if (_nameHash != value) { _nameHash = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(13)]
        public ChunkReferenceList ActorDefinitionIndexes { get => _actorDefinitionIndexes;
            set { if (_actorDefinitionIndexes != value) { _actorDefinitionIndexes = new ChunkReferenceList(OnRCOLChanged, value); OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(14)]
        public ChunkReferenceList PropertyDefinitionIndexes { get => _propertyDefinitionIndexes;
            set { if (_propertyDefinitionIndexes != value) { _propertyDefinitionIndexes = new ChunkReferenceList(OnRCOLChanged, value); OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(15)]
        public ChunkReferenceList StateIndexes { get => _stateIndexes;
            set { if (_stateIndexes != value) { _stateIndexes = new ChunkReferenceList(OnRCOLChanged, value); OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(16)]
        public AnimationList Animations { get => _animations;
            set { if (_animations != value) { _animations = new AnimationList(OnRCOLChanged, value); OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(17)]
        public Flags Properties { get => _properties;
            set { if (_properties != value) { _properties = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(18)]
        public AnimationPriority AutomationPriority { get => _automationPriority;
            set { if (_automationPriority != value) { _automationPriority = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(19)]
        public AwarenessLevel AwarenessOverlayLevel { get => _awarenessOverlayLevel;
            set { if (_awarenessOverlayLevel != value) { _awarenessOverlayLevel = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(20)]
        public uint Unknown2 { get => _unknown2;
            set { if (_unknown2 != value) { _unknown2 = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(21)]
        public uint Unknown3 { get => _unknown3;
            set { if (_unknown3 != value) { _unknown3 = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(22)]
        public uint Unknown4 { get => _unknown4;
            set { if (_unknown4 != value) { _unknown4 = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(23)]
        public uint Unknown5 { get => _unknown5;
            set { if (_unknown5 != value) { _unknown5 = value; OnRCOLChanged(this, EventArgs.Empty); } } }

        public string Value => ValueBuilder;

        #endregion
    }

    public class JazzState : JazzChunk
    {
        const string _tag = "S_St";

        [Flags]
        public enum Flags : uint
        {
            None = 0x0000,
            Public = 0x0001,
            Entry = 0x0002,
            Exit = 0x0004,
            Loop = 0x0008,
            OneShot = 0x0010,
            OneShotHold = 0x0020,
            Synchronized = 0x0040,
            Join = 0x0080,
            Explicit = 0x0100
        }

        #region Attributes
        uint _version = 0x0101;
        uint _nameHash;
        Flags _flags;
        GenericRCOLResource.GenericRCOLResource.ChunkReference _decisionGraphIndex;
        ChunkReferenceList _outboundStateIndexes;
        AwarenessLevel _awarenessOverlayLevel;
        #endregion

        #region Constructors
        public JazzState(EventHandler handler) : base(handler, null) { }
        public JazzState(EventHandler handler, Stream s) : base(handler, s) { }
        public JazzState(EventHandler handler, JazzState basis)
            : this(handler
            , basis._version
            , basis._nameHash
            , basis._flags
            , basis._decisionGraphIndex
            , basis._outboundStateIndexes
            , basis._awarenessOverlayLevel
            )
        { }
        public JazzState(EventHandler handler
            , uint version
            , uint nameHash
            , Flags flags
            , GenericRCOLResource.GenericRCOLResource.ChunkReference decisionGraphIndex
            , ChunkReferenceList outboundStateIndexes
            , AwarenessLevel awarenessOverlayLevel)
            : base(handler, null)
        {
            _version = version;
            _nameHash = nameHash;
            _flags = flags;
            _decisionGraphIndex = new GenericRCOLResource.GenericRCOLResource.ChunkReference(handler, decisionGraphIndex);
            _outboundStateIndexes = new ChunkReferenceList(handler, outboundStateIndexes);
            _awarenessOverlayLevel = awarenessOverlayLevel;
        }
        #endregion

        #region ARCOLBlock
        [ElementPriority(2)]
        public override string Tag => _tag;

        [ElementPriority(3)]
        public override uint ResourceType => 0x02EEDAFE;

        protected override void Parse(Stream s)
        {
            base.Parse(s);

            BinaryReader r = new BinaryReader(s);
            _version = r.ReadUInt32();
            _nameHash = r.ReadUInt32();
            _flags = (Flags)r.ReadUInt32();
            _decisionGraphIndex = new GenericRCOLResource.GenericRCOLResource.ChunkReference(handler, s);
            _outboundStateIndexes = new ChunkReferenceList(handler, s);
            _awarenessOverlayLevel = (AwarenessLevel)r.ReadUInt32();
        }

        public override Stream UnParse()
        {
            Stream ms = base.UnParse();
            BinaryWriter w = new BinaryWriter(ms);

            w.Write(_version);
            w.Write(_nameHash);
            w.Write((uint)_flags);
            if (_decisionGraphIndex == null) _decisionGraphIndex = new GenericRCOLResource.GenericRCOLResource.ChunkReference(handler, 0);
            _decisionGraphIndex.UnParse(ms);
            if (_outboundStateIndexes == null) _outboundStateIndexes = new ChunkReferenceList(handler);
            _outboundStateIndexes.UnParse(ms);
            w.Write((uint)_awarenessOverlayLevel);

            return ms;
        }

        // public override AHandlerElement Clone(EventHandler handler) { return new JazzState(requestedApiVersion, handler, this); }
        #endregion

        #region ContentFields
        [ElementPriority(11)]
        public uint Version { get => _version;
            set { if (_version != value) { _version = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(12)]
        public uint NameHash { get => _nameHash;
            set { if (_nameHash != value) { _nameHash = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(13)]
        public GenericRCOLResource.GenericRCOLResource.ChunkReference DecisionGraphIndex { get => _decisionGraphIndex;
            set { if (_decisionGraphIndex != value) { _decisionGraphIndex = new GenericRCOLResource.GenericRCOLResource.ChunkReference(handler, value); OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(14)]
        public Flags Properties { get => _flags;
            set { if (_flags != value) { _flags = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(15)]
        public ChunkReferenceList OutboundStateIndexes { get => _outboundStateIndexes;
            set { if (_outboundStateIndexes != value) { _outboundStateIndexes = new ChunkReferenceList(handler, value); OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(16)]
        public AwarenessLevel AwarenessOverlayLevel { get => _awarenessOverlayLevel;
            set { if (_awarenessOverlayLevel != value) { _awarenessOverlayLevel = value; OnRCOLChanged(this, EventArgs.Empty); } } }

        public string Value => ValueBuilder;

        #endregion
    }

    public class JazzDecisionGraph : JazzChunk
    {
        const string _tag = "S_DG";

        #region Attributes
        uint _version = 0x0101;
        uint _unknown1;
        ChunkReferenceList _outboundDecisionGraphIndexes;
        ChunkReferenceList _inboundDecisionGraphIndexes;
        //0xDEADBEEF
        #endregion

        #region Constructors
        public JazzDecisionGraph(EventHandler handler) : base(handler, null) { }
        public JazzDecisionGraph(EventHandler handler, Stream s) : base(handler, s) { }
        public JazzDecisionGraph(EventHandler handler, JazzDecisionGraph basis)
            : this(handler
            , basis._version
            , basis._unknown1
            , basis._outboundDecisionGraphIndexes
            , basis._inboundDecisionGraphIndexes
            )
        { }
        public JazzDecisionGraph(EventHandler handler
            , uint version
            , uint unknown1
            , IEnumerable<GenericRCOLResource.GenericRCOLResource.ChunkReference> outboundDecisionGraphIndexes
            , IEnumerable<GenericRCOLResource.GenericRCOLResource.ChunkReference> inboundDecisionGraphIndexes)
            : base(handler, null)
        {
            _version = version;
            _unknown1 = unknown1;
            _outboundDecisionGraphIndexes = new ChunkReferenceList(handler, outboundDecisionGraphIndexes);
            _inboundDecisionGraphIndexes = new ChunkReferenceList(handler, inboundDecisionGraphIndexes);
        }
        #endregion

        #region ARCOLBlock
        [ElementPriority(2)]
        public override string Tag => _tag;

        [ElementPriority(3)]
        public override uint ResourceType => 0x02EEDB18;

        protected override void Parse(Stream s)
        {
            base.Parse(s);

            BinaryReader r = new BinaryReader(s);
            _version = r.ReadUInt32();
            _unknown1 = r.ReadUInt32();
            _outboundDecisionGraphIndexes = new ChunkReferenceList(handler, s);
            _inboundDecisionGraphIndexes = new ChunkReferenceList(handler, s);
            Deadbeef.Parse(s);
        }

        public override Stream UnParse()
        {
            Stream ms = base.UnParse();
            BinaryWriter w = new BinaryWriter(ms);

            w.Write(_version);
            w.Write(_unknown1);
            if (_outboundDecisionGraphIndexes == null) _outboundDecisionGraphIndexes = new ChunkReferenceList(handler);
            _outboundDecisionGraphIndexes.UnParse(ms);
            if (_inboundDecisionGraphIndexes == null) _inboundDecisionGraphIndexes = new ChunkReferenceList(handler);
            _inboundDecisionGraphIndexes.UnParse(ms);
            Deadbeef.UnParse(ms);

            return ms;
        }

        // public override AHandlerElement Clone(EventHandler handler) { return new JazzDecisionGraph(requestedApiVersion, handler, this); }
        #endregion

        #region ContentFields
        [ElementPriority(11)]
        public uint Version { get => _version;
            set { if (_version != value) { _version = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(12)]
        public uint Unknown1 { get => _unknown1;
            set { if (_unknown1 != value) { _unknown1 = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(13)]
        public ChunkReferenceList OutboundDecisionGraphIndexes { get => _outboundDecisionGraphIndexes;
            set { if (_outboundDecisionGraphIndexes != value) { _outboundDecisionGraphIndexes = new ChunkReferenceList(OnRCOLChanged, value); OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(14)]
        public ChunkReferenceList InboundDecisionGraphIndexes { get => _inboundDecisionGraphIndexes;
            set { if (_inboundDecisionGraphIndexes != value) { _inboundDecisionGraphIndexes = new ChunkReferenceList(OnRCOLChanged, value); OnRCOLChanged(this, EventArgs.Empty); } } }

        public string Value => ValueBuilder;

        #endregion
    }

    public class JazzActorDefinition : JazzChunk
    {
        const string _tag = "S_AD";

        #region Attributes
        uint _version = 0x0100;
        uint _nameHash;
        uint _unknown1;
        #endregion

        #region Constructors
        public JazzActorDefinition(EventHandler handler) : base(handler, null) { }
        public JazzActorDefinition(EventHandler handler, Stream s) : base(handler, s) { }
        public JazzActorDefinition(EventHandler handler, JazzActorDefinition basis)
            : this(handler
            , basis._version
            , basis._nameHash
            , basis._unknown1
            )
        { }
        public JazzActorDefinition(EventHandler handler
            , uint version
            , uint nameHash
            , uint unknown1)
            : base(handler, null)
        {
            _version = version;
            _nameHash = nameHash;
            _unknown1 = unknown1;
        }
        #endregion

        #region ARCOLBlock
        [ElementPriority(2)]
        public override string Tag => _tag;

        [ElementPriority(3)]
        public override uint ResourceType => 0x02EEDB2F;

        protected override void Parse(Stream s)
        {
            base.Parse(s);

            BinaryReader r = new BinaryReader(s);
            _version = r.ReadUInt32();
            _nameHash = r.ReadUInt32();
            _unknown1 = r.ReadUInt32();
        }

        public override Stream UnParse()
        {
            Stream ms = base.UnParse();
            BinaryWriter w = new BinaryWriter(ms);

            w.Write(_version);
            w.Write(_nameHash);
            w.Write(_unknown1);

            return ms;
        }

        // public override AHandlerElement Clone(EventHandler handler) { return new JazzActorDefinition(requestedApiVersion, handler, this); }
        #endregion

        #region ContentFields
        [ElementPriority(11)]
        public uint Version { get => _version;
            set { if (_version != value) { _version = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(12)]
        public uint NameHash { get => _nameHash;
            set { if (_nameHash != value) { _nameHash = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(13)]
        public uint Unknown1 { get => _unknown1;
            set { if (_unknown1 != value) { _unknown1 = value; OnRCOLChanged(this, EventArgs.Empty); } } }

        public string Value => ValueBuilder;

        #endregion
    }

    public class JazzParameterDefinition : JazzChunk
    {
        const string _tag = "S_PD";

        #region Attributes
        uint _version = 0x0100;
        uint _nameHash;
        uint _defaultValue;
        #endregion

        #region Constructors
        public JazzParameterDefinition(EventHandler handler) : base(handler, null) { }
        public JazzParameterDefinition(EventHandler handler, Stream s) : base(handler, s) { }
        public JazzParameterDefinition(EventHandler handler, JazzParameterDefinition basis)
            : this(handler
            , basis._version
            , basis._nameHash
            , basis._defaultValue
            )
        { }
        public JazzParameterDefinition(EventHandler handler
            , uint version
            , uint nameHash
            , uint defaultValue)
            : base(handler, null)
        {
            _version = version;
            _nameHash = nameHash;
            _defaultValue = defaultValue;
        }
        #endregion

        #region ARCOLBlock
        [ElementPriority(2)]
        public override string Tag => _tag;

        [ElementPriority(3)]
        public override uint ResourceType => 0x02EEDB46;

        protected override void Parse(Stream s)
        {
            base.Parse(s);

            BinaryReader r = new BinaryReader(s);
            _version = r.ReadUInt32();
            _nameHash = r.ReadUInt32();
            _defaultValue = r.ReadUInt32();
        }

        public override Stream UnParse()
        {
            Stream ms = base.UnParse();
            BinaryWriter w = new BinaryWriter(ms);

            w.Write(_version);
            w.Write(_nameHash);
            w.Write(_defaultValue);

            return ms;
        }

        // public override AHandlerElement Clone(EventHandler handler) { return new JazzParameterDefinition(requestedApiVersion, handler, this); }
        #endregion

        #region ContentFields
        [ElementPriority(11)]
        public uint Version { get => _version;
            set { if (_version != value) { _version = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(12)]
        public uint NameHash { get => _nameHash;
            set { if (_nameHash != value) { _nameHash = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(13)]
        public uint DefaultValue { get => _defaultValue;
            set { if (_defaultValue != value) { _defaultValue = value; OnRCOLChanged(this, EventArgs.Empty); } } }

        public string Value => ValueBuilder;

        #endregion
    }
    #endregion

    #region Decision Graph Node Chunks
    [Flags]
    public enum JazzAnimationFlags : uint
    {
        TimingNormal = 0x00,
        Default = 0x01,
        AtEnd = 0x01,
        LoopAsNeeded = 0x02,
        OverridePriority = 0x04,
        Mirror = 0x08,
        OverrideMirror = 0x10,
        OverrideTiming0 = 0x20,
        OverrideTiming1 = 0x40,
        TimingMaster = 0x20,
        TimingSlave = 0x40,
        TimingIgnored = 0x60,
        TimingMask = 0x60,
        Interruptible = 0x80,
        ForceBlend = 0x100,
        UseTimingPriority = 0x200,
        UseTimingPriorityAsClockMaster = 0x400,
        BaseClipIsSocial = 0x800,
        AdditiveClipIsSocial = 0x1000,
        BaseClipIsObjectOnly = 0x2000,
        AdditiveClipIsObjectOnly = 0x4000,
        HoldPose = 0x8000,
        BlendMotionAccumulation = 0x10000
    }

    public class JazzPlayAnimationNode : JazzChunk
    {
        const string _tag = "Play";

        #region Attributes
        uint _version = 0x0105;
        TGIBlock _clipResource;
        TGIBlock _tkmkResource;
        //actorSlots count is here
        uint _unknown1;
        uint _unknown2;
        uint _unknown3;
        ActorSlotList _actorSlots;
        ActorSuffixList _actorSuffixes;
        //0xDEADBEEF
        TGIBlock _additiveClipResource;
        string _animation = "";
        //followed by padding to next DWORD
        string _additiveAnimation = "";
        //followed by padding to next DWORD
        //0xDEADBEEF
        JazzAnimationFlags _animationNodeFlags;
        AnimationPriority _animationPriority;
        float _unknown9;
        float _blendInTime;
        float _blendOutTime;
        float _unknown11;
        float _speed;
        GenericRCOLResource.GenericRCOLResource.ChunkReference _actorDefinitionIndex;
        AnimationPriority _timingPriority;
        uint _unknown13;
        uint _unknown14;
        uint _unknown15;
        uint _unknown16;
        uint _unknown17;
        uint _unknown18;
        //0xDEADBEEF
        ChunkReferenceList _decisionGraphIndexes;
        //'/DGN'
        #endregion

        #region Constructors
        public JazzPlayAnimationNode(EventHandler handler) : base(handler, null) { }
        public JazzPlayAnimationNode(EventHandler handler, Stream s) : base(handler, s) { }
        public JazzPlayAnimationNode(EventHandler handler, JazzPlayAnimationNode basis)
            : this(handler
            , basis._version
            , basis._clipResource
            , basis._tkmkResource
            , basis._unknown1
            , basis._unknown2
            , basis._unknown3
            , basis._actorSlots
            , basis._actorSuffixes
            , basis._additiveClipResource
            , basis._animation
            , basis._additiveAnimation
            , basis._animationNodeFlags
            , basis._animationPriority
            , basis._unknown9
            , basis._blendInTime
            , basis._blendOutTime
            , basis._unknown11
            , basis._speed
            , basis._actorDefinitionIndex
            , basis._timingPriority
            , basis._unknown13
            , basis._unknown14
            , basis._unknown15
            , basis._unknown16
            , basis._unknown17
            , basis._unknown18
            , basis._decisionGraphIndexes
            )
        { }
        public JazzPlayAnimationNode(EventHandler handler
            , uint version
            , IResourceKey clipResource
            , IResourceKey tkmkResource
            , uint unknown1
            , uint unknown2
            , uint unknown3
            , IEnumerable<ActorSlot> actorSlots
            , IEnumerable<ActorSuffix> actorSuffixes
            , IResourceKey additiveClipResource
            , string animation
            , string additiveAnimation
            , JazzAnimationFlags animationNodeFlags
            , AnimationPriority animationPriority
            , float unknown9
            , float blendInTime
            , float blendOutTime
            , float unknown11
            , float speed
            , GenericRCOLResource.GenericRCOLResource.ChunkReference actorDefinitionIndex
            , AnimationPriority timingPriority
            , uint unknown13
            , uint unknown14
            , uint unknown15
            , uint unknown16
            , uint unknown17
            , uint unknown18
            , IEnumerable<GenericRCOLResource.GenericRCOLResource.ChunkReference> decisionGraphIndexes)
            : base(handler, null)
        {
            _version = version;
            _clipResource = new TGIBlock(handler, "ITG", clipResource);
            _tkmkResource = new TGIBlock(handler, "ITG", tkmkResource);
            _unknown1 = unknown1;
            _unknown2 = unknown2;
            _unknown3 = unknown3;
            _actorSlots = actorSlots == null ? null : new ActorSlotList(handler, actorSlots);
            _actorSuffixes = actorSuffixes == null ? null : new ActorSuffixList(handler, actorSuffixes);
            _additiveClipResource = new TGIBlock(handler, "ITG", additiveClipResource);
            _animation = animation;
            _additiveAnimation = additiveAnimation;
            _animationNodeFlags = animationNodeFlags;
            _animationPriority = animationPriority;
            _unknown9 = unknown9;
            _blendInTime = blendInTime;
            _blendOutTime = blendOutTime;
            _unknown11 = unknown11;
            _speed = speed;
            _actorDefinitionIndex = actorDefinitionIndex;
            _timingPriority = timingPriority;
            _unknown13 = unknown13;
            _unknown14 = unknown14;
            _unknown15 = unknown15;
            _unknown16 = unknown16;
            _unknown17 = unknown17;
            _unknown18 = unknown18;
            _decisionGraphIndexes = decisionGraphIndexes == null ? null : new ChunkReferenceList(handler, decisionGraphIndexes);
        }
        #endregion

        #region ARCOLBlock
        [ElementPriority(2)]
        public override string Tag => _tag;

        [ElementPriority(3)]
        public override uint ResourceType => 0x02EEDB5F;

        protected override void Parse(Stream s)
        {
            base.Parse(s);

            int actorSlotCount;
            BinaryReader r = new BinaryReader(s);

            _version = r.ReadUInt32();
            _clipResource = new TGIBlock(handler, "ITG", s);
            _tkmkResource = new TGIBlock(handler, "ITG", s);
            actorSlotCount = r.ReadInt32();
            _unknown1 = r.ReadUInt32();
            _unknown2 = r.ReadUInt32();
            _unknown3 = r.ReadUInt32();
            _actorSlots = new ActorSlotList(handler, actorSlotCount, s);
            _actorSuffixes = new ActorSuffixList(handler, s);
            Deadbeef.Parse(s);
            _additiveClipResource = new TGIBlock(handler, "ITG", s);
            _animation = System.Text.Encoding.Unicode.GetString(r.ReadBytes(r.ReadInt32() * 2));
            if (_animation.Length > 0) r.ReadUInt16();
            ExpectZero(s);
            _additiveAnimation = System.Text.Encoding.Unicode.GetString(r.ReadBytes(r.ReadInt32() * 2));
            if (_additiveAnimation.Length > 0) r.ReadUInt16();
            ExpectZero(s);
            Deadbeef.Parse(s);
            _animationNodeFlags = (JazzAnimationFlags)r.ReadUInt32();
            _animationPriority = (AnimationPriority)r.ReadUInt32();
            _unknown9 = r.ReadSingle();
            _blendInTime = r.ReadSingle();
            _blendOutTime = r.ReadSingle();
            _unknown11 = r.ReadSingle();
            _speed = r.ReadSingle();
            _actorDefinitionIndex = new GenericRCOLResource.GenericRCOLResource.ChunkReference(handler, s);
            _timingPriority = (AnimationPriority)r.ReadUInt32();
            _unknown13 = r.ReadUInt32();
            _unknown14 = r.ReadUInt32();
            _unknown15 = r.ReadUInt32();
            _unknown16 = r.ReadUInt32();
            _unknown17 = r.ReadUInt32();
            _unknown18 = r.ReadUInt32();
            Deadbeef.Parse(s);
            _decisionGraphIndexes = new ChunkReferenceList(handler, s);
            CloseDgn.Parse(s);
        }

        public override Stream UnParse()
        {
            Stream ms = base.UnParse();
            BinaryWriter w = new BinaryWriter(ms);

            w.Write(_version);
            if (_clipResource == null) _clipResource = new TGIBlock(handler, "ITG");
            _clipResource.UnParse(ms);
            if (_tkmkResource == null) _tkmkResource = new TGIBlock(handler, "ITG");
            _tkmkResource.UnParse(ms);
            if (_actorSlots == null) _actorSlots = new ActorSlotList(handler);
            w.Write(_actorSlots.Count);
            w.Write(_unknown1);
            w.Write(_unknown2);
            w.Write(_unknown3);
            _actorSlots.UnParse(ms);
            if (_actorSuffixes == null) _actorSuffixes = new ActorSuffixList(handler);
            _actorSuffixes.UnParse(ms);
            Deadbeef.UnParse(ms);
            if (_additiveClipResource == null) _additiveClipResource = new TGIBlock(handler, "ITG");
            _additiveClipResource.UnParse(ms);
            w.Write(_animation.Length);
            w.Write(System.Text.Encoding.Unicode.GetBytes(_animation));
            if (_animation.Length > 0) w.Write((UInt16)0);
            PadZero(ms);
            w.Write(_additiveAnimation.Length);
            w.Write(System.Text.Encoding.Unicode.GetBytes(_additiveAnimation));
            if (_additiveAnimation.Length > 0) w.Write((UInt16)0);
            PadZero(ms);
            Deadbeef.UnParse(ms);
            w.Write((uint)_animationNodeFlags);
            w.Write((uint)_animationPriority);
            w.Write(_unknown9);
            w.Write(_blendInTime);
            w.Write(_blendOutTime);
            w.Write(_unknown11);
            w.Write(_speed);
            if (_actorDefinitionIndex == null) _actorDefinitionIndex = new GenericRCOLResource.GenericRCOLResource.ChunkReference(handler, 0);
            _actorDefinitionIndex.UnParse(ms);
            w.Write((uint)_timingPriority);
            w.Write(_unknown13);
            w.Write(_unknown14);
            w.Write(_unknown15);
            w.Write(_unknown16);
            w.Write(_unknown17);
            w.Write(_unknown18);
            Deadbeef.UnParse(ms);
            if (_decisionGraphIndexes == null) _decisionGraphIndexes = new ChunkReferenceList(handler);
            _decisionGraphIndexes.UnParse(ms);
            CloseDgn.UnParse(ms);

            return ms;
        }

        // public override AHandlerElement Clone(EventHandler handler) { return new JazzPlayAnimationNode(requestedApiVersion, handler, this); }
        #endregion

        #region Sub-types
        public class ActorSlot : AHandlerElement, IEquatable<ActorSlot>
        {
            #region Attributes
            uint _chainId;
            uint _slotId;
            uint _actorNameHash;
            uint _slotNameHash;
            #endregion

            #region Constructors
            public ActorSlot(EventHandler handler) : base(handler) { }
            public ActorSlot(EventHandler handler, Stream s) : base(handler) { Parse(s); }
            public ActorSlot(EventHandler handler, ActorSlot basis)
                : this(handler
                , basis._chainId
                , basis._slotId
                , basis._actorNameHash
                , basis._slotNameHash
                )
            {
            }
            public ActorSlot(EventHandler handler
                , uint chainId
                , uint slotId
                , uint actorNameHash
                , uint slotNameHash)
                : base(handler)
            {
                _chainId = chainId;
                _slotId = slotId;
                _actorNameHash = actorNameHash;
                _slotNameHash = slotNameHash;
            }
            #endregion

            #region Data I/O
            void Parse(Stream s)
            {
                BinaryReader r = new BinaryReader(s);
                _chainId = r.ReadUInt32();
                _slotId = r.ReadUInt32();
                _actorNameHash = r.ReadUInt32();
                _slotNameHash = r.ReadUInt32();
            }

            internal void UnParse(Stream s)
            {
                BinaryWriter w = new BinaryWriter(s);
                w.Write(_chainId);
                w.Write(_slotId);
                w.Write(_actorNameHash);
                w.Write(_slotNameHash);
            }
            #endregion

            #region AHandlerElement
            // public override AHandlerElement Clone(EventHandler handler) { return new ActorSlot(requestedApiVersion, handler, this); }
            public override List<string> ContentFields => GetContentFields(GetType());

            #endregion

            #region IEquatable<Animation>
            public bool Equals(ActorSlot other)
            {
                return _chainId.Equals(other._chainId) && _slotId.Equals(other._slotId) && _actorNameHash.Equals(other._actorNameHash) && _slotNameHash.Equals(other._slotNameHash);
            }
            #endregion

            #region ContentFields
            [ElementPriority(1)]
            public uint ChainId { get => _chainId;
                set { if (_chainId != value) { _chainId = value; OnElementChanged(); } } }
            [ElementPriority(2)]
            public uint SlotId { get => _slotId;
                set { if (_slotId != value) { _slotId = value; OnElementChanged(); } } }
            [ElementPriority(3)]
            public uint ActorNameHash { get => _actorNameHash;
                set { if (_actorNameHash != value) { _actorNameHash = value; OnElementChanged(); } } }
            [ElementPriority(4)]
            public uint SlotNameHash { get => _slotNameHash;
                set { if (_slotNameHash != value) { _slotNameHash = value; OnElementChanged(); } } }

            public string Value => ValueBuilder;

            #endregion
        }
        public class ActorSlotList : DependentList<ActorSlot>
        {
            int _count;

            #region Constructors
            public ActorSlotList(EventHandler handler) : base(handler) { }
            public ActorSlotList(EventHandler handler, IEnumerable<ActorSlot> basis) : base(handler, basis) { }
            public ActorSlotList(EventHandler handler, int count, Stream s) : base(handler) { elementHandler = handler; _count = count; Parse(s); this.handler = handler; }
            #endregion

            #region DependentList<ActorSlot>
            protected override int ReadCount(Stream s) { return _count; }
            protected override void WriteCount(Stream s, int count) { }

            protected override ActorSlot CreateElement(Stream s) { return new ActorSlot(handler, s); }

            protected override void WriteElement(Stream s, ActorSlot element) { element.UnParse(s); }
            #endregion
        }
        public class ActorSuffix : AHandlerElement, IEquatable<ActorSuffix>
        {
            #region Attributes
            uint _actorNameHash;
            uint _suffixHash;
            #endregion

            #region Constructors
            public ActorSuffix(EventHandler handler) : base(handler) { }
            public ActorSuffix(EventHandler handler, Stream s) : base(handler) { Parse(s); }
            public ActorSuffix(EventHandler handler, ActorSuffix basis)
                : this(handler
                , basis._actorNameHash
                , basis._suffixHash
                )
            {
            }
            public ActorSuffix(EventHandler handler
                , uint actorNameHash
                , uint suffixHash)
                : base(handler)
            {
                _actorNameHash = actorNameHash;
                _suffixHash = suffixHash;
            }
            #endregion

            #region Data I/O
            void Parse(Stream s)
            {
                BinaryReader r = new BinaryReader(s);
                _actorNameHash = r.ReadUInt32();
                _suffixHash = r.ReadUInt32();
            }

            internal void UnParse(Stream s)
            {
                BinaryWriter w = new BinaryWriter(s);
                w.Write(_actorNameHash);
                w.Write(_suffixHash);
            }
            #endregion

            #region AHandlerElement
            // public override AHandlerElement Clone(EventHandler handler) { return new ActorSuffix(requestedApiVersion, handler, this); }
            public override List<string> ContentFields => GetContentFields(GetType());

            #endregion

            #region IEquatable<Animation>
            public bool Equals(ActorSuffix other)
            {
                return _actorNameHash.Equals(other._actorNameHash) && _suffixHash.Equals(other._suffixHash);
            }
            #endregion

            #region ContentFields
            [ElementPriority(1)]
            public uint ActorNameHash { get => _actorNameHash;
                set { if (_actorNameHash != value) { _actorNameHash = value; OnElementChanged(); } } }
            [ElementPriority(2)]
            public uint SuffixHash { get => _suffixHash;
                set { if (_suffixHash != value) { _suffixHash = value; OnElementChanged(); } } }

            public string Value => ValueBuilder.Replace("\n", "; ");

            #endregion
        }
        public class ActorSuffixList : DependentList<ActorSuffix>
        {
            #region Constructors
            public ActorSuffixList(EventHandler handler) : base(handler) { }
            public ActorSuffixList(EventHandler handler, IEnumerable<ActorSuffix> basis) : base(handler, basis) { }
            public ActorSuffixList(EventHandler handler, Stream s) : base(handler, s) { }
            #endregion

            #region DependentList<ActorSuffix>
            protected override ActorSuffix CreateElement(Stream s) { return new ActorSuffix(handler, s); }

            protected override void WriteElement(Stream s, ActorSuffix element) { element.UnParse(s); }
            #endregion
        }
        #endregion

        #region ContentFields
        [ElementPriority(11)]
        public uint Version { get => _version;
            set { if (_version != value) { _version = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(12)]
        public IResourceKey ClipResource { get => _clipResource;
            set { if (_clipResource != value) { _clipResource = new TGIBlock(handler, "ITG", value); OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(13)]
        public IResourceKey TkmkResource { get => _tkmkResource;
            set { if (_tkmkResource != value) { _tkmkResource = new TGIBlock(handler, "ITG", value); OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(14)]
        public uint Unknown1 { get => _unknown1;
            set { if (_unknown1 != value) { _unknown1 = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(15)]
        public uint Unknown2 { get => _unknown2;
            set { if (_unknown2 != value) { _unknown2 = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(16)]
        public uint Unknown3 { get => _unknown3;
            set { if (_unknown3 != value) { _unknown3 = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(17)]
        public ActorSlotList ActorSlots { get => _actorSlots;
            set { if (_actorSlots != value) { _actorSlots = value == null ? null : new ActorSlotList(handler, value); OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(18)]
        public ActorSuffixList ActorSuffixes { get => _actorSuffixes;
            set { if (_actorSuffixes != value) { _actorSuffixes = value == null ? null : new ActorSuffixList(handler, value); OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(19)]
        public IResourceKey AdditiveClipResource { get => _additiveClipResource;
            set { if (_additiveClipResource != value) { _additiveClipResource = new TGIBlock(handler, "ITG", value); OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(23)]
        public string Animation { get => _animation;
            set { if (_animation != value) { _animation = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(24)]
        public string AdditiveAnimation { get => _additiveAnimation;
            set { if (_additiveAnimation != value) { _additiveAnimation = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(25)]
        public JazzAnimationFlags AnimationNodeFlags { get => _animationNodeFlags;
            set { if (_animationNodeFlags != value) { _animationNodeFlags = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(26)]
        public AnimationPriority AnimationPriority1 { get => _animationPriority;
            set { if (_animationPriority != value) { _animationPriority = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(27)]
        public float Unknown9 { get => _unknown9;
            set { if (_unknown9 != value) { _unknown9 = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(28)]
        public float BlendInTime { get => _blendInTime;
            set { if (_blendInTime != value) { _blendInTime = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(29)]
        public float BlendOutTime { get => _blendOutTime;
            set { if (_blendOutTime != value) { _blendOutTime = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(30)]
        public float Unknown11 { get => _unknown11;
            set { if (_unknown11 != value) { _unknown11 = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(31)]
        public float Speed { get => _speed;
            set { if (_speed != value) { _speed = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(32)]
        public GenericRCOLResource.GenericRCOLResource.ChunkReference ActorDefinitionIndex { get => _actorDefinitionIndex;
            set { if (_actorDefinitionIndex != value) { _actorDefinitionIndex = new GenericRCOLResource.GenericRCOLResource.ChunkReference(handler, value); OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(33)]
        public AnimationPriority TimingPriority { get => _timingPriority;
            set { if (_timingPriority != value) { _timingPriority = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(34)]
        public uint Unknown13 { get => _unknown13;
            set { if (_unknown13 != value) { _unknown13 = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(35)]
        public uint Unknown14 { get => _unknown14;
            set { if (_unknown14 != value) { _unknown14 = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(36)]
        public uint Unknown15 { get => _unknown15;
            set { if (_unknown15 != value) { _unknown15 = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(37)]
        public uint Unknown16 { get => _unknown16;
            set { if (_unknown16 != value) { _unknown16 = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(38)]
        public uint Unknown17 { get => _unknown17;
            set { if (_unknown17 != value) { _unknown17 = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(39)]
        public uint Unknown18 { get => _unknown18;
            set { if (_unknown18 != value) { _unknown18 = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(40)]
        public ChunkReferenceList DecisionGraphIndexes { get => _decisionGraphIndexes;
            set { if (_decisionGraphIndexes != value) { _decisionGraphIndexes = value == null ? null : new ChunkReferenceList(handler, value); OnRCOLChanged(this, EventArgs.Empty); } } }

        public string Value => ValueBuilder;

        #endregion
    }

    public class JazzRandomNode : JazzChunk
    {
        const string _tag = "Rand";

        [Flags]
        public enum Flags : uint
        {
            None = 0x00,
            AvoidRepeats = 0x01
        }

        #region Attributes
        uint _version = 0x0101;
        OutcomeList _outcomes;
        //0xDEADBEEF
        Flags _flags;
        //'/DGN'
        #endregion

        #region Constructors
        public JazzRandomNode(EventHandler handler) : base(handler, null) { }
        public JazzRandomNode(EventHandler handler, Stream s) : base(handler, s) { }
        public JazzRandomNode(EventHandler handler, JazzRandomNode basis)
            : this(handler
            , basis._version
            , basis._outcomes
            , basis._flags
            )
        { }
        public JazzRandomNode(EventHandler handler
            , uint version
            , IEnumerable<Outcome> outcomes
            , Flags flags)
            : base(handler, null)
        {
            _version = version;
            _outcomes = new OutcomeList(handler, outcomes);
            _flags = flags;
        }
        #endregion

        #region ARCOLBlock
        [ElementPriority(2)]
        public override string Tag => _tag;

        [ElementPriority(3)]
        public override uint ResourceType => 0x02EEDB70;

        protected override void Parse(Stream s)
        {
            base.Parse(s);

            BinaryReader r = new BinaryReader(s);
            _version = r.ReadUInt32();
            _outcomes = new OutcomeList(handler, s);
            Deadbeef.Parse(s);
            _flags = (Flags)r.ReadUInt32();
            CloseDgn.Parse(s);
        }

        public override Stream UnParse()
        {
            Stream ms = base.UnParse();
            BinaryWriter w = new BinaryWriter(ms);

            w.Write(_version);
            if (_outcomes == null) _outcomes = new OutcomeList(handler);
            _outcomes.UnParse(ms);
            Deadbeef.UnParse(ms);
            w.Write((uint)_flags);
            CloseDgn.UnParse(ms);

            return ms;
        }

        // public override AHandlerElement Clone(EventHandler handler) { return new JazzRandomNode(requestedApiVersion, handler, this); }
        #endregion

        #region Sub-types
        public class Outcome : AHandlerElement, IEquatable<Outcome>
        {
            #region Attributes
            float _weight;
            ChunkReferenceList _decisionGraphIndexes;
            #endregion

            #region Constructors
            public Outcome(EventHandler handler) : base(handler) { }
            public Outcome(EventHandler handler, Stream s) : base(handler) { Parse(s); }
            public Outcome(EventHandler handler, Outcome basis)
                : this(handler
                , basis._weight
                , basis._decisionGraphIndexes
                )
            {
            }
            public Outcome(EventHandler handler
                , float weight
                , IEnumerable<GenericRCOLResource.GenericRCOLResource.ChunkReference> decisionGraphIndexes)
                : base(handler)
            {
                _weight = weight;
                _decisionGraphIndexes = new ChunkReferenceList(handler, decisionGraphIndexes);
            }
            #endregion

            #region Data I/O
            void Parse(Stream s)
            {
                _weight = new BinaryReader(s).ReadSingle();
                _decisionGraphIndexes = new ChunkReferenceList(handler, s);
            }

            internal void UnParse(Stream s)
            {
                new BinaryWriter(s).Write(_weight);
                if (_decisionGraphIndexes == null) _decisionGraphIndexes = new ChunkReferenceList(handler);
                _decisionGraphIndexes.UnParse(s);
            }
            #endregion

            #region AHandlerElement
            // public override AHandlerElement Clone(EventHandler handler) { return new Outcome(requestedApiVersion, handler, this); }
            public override List<string> ContentFields => GetContentFields(GetType());

            #endregion

            #region IEquatable<Animation>
            public bool Equals(Outcome other)
            {
                return _weight.Equals(other._weight) && _decisionGraphIndexes.Equals(other._decisionGraphIndexes);
            }
            #endregion

            #region ContentFields
            [ElementPriority(1)]
            public float Weight { get => _weight;
                set { if (_weight != value) { _weight = value; OnElementChanged(); } } }
            [ElementPriority(2)]
            public ChunkReferenceList DecisionGraphIndexes { get => _decisionGraphIndexes;
                set { if (_decisionGraphIndexes != value) { _decisionGraphIndexes = new ChunkReferenceList(handler, value); OnElementChanged(); } } }

            public string Value => ValueBuilder;

            #endregion
        }
        public class OutcomeList : DependentList<Outcome>
        {
            #region Constructors
            public OutcomeList(EventHandler handler) : base(handler) { }
            public OutcomeList(EventHandler handler, IEnumerable<Outcome> basis) : base(handler, basis) { }
            public OutcomeList(EventHandler handler, Stream s) : base(handler, s) { }
            #endregion

            #region DependentList<Outcome>
            protected override Outcome CreateElement(Stream s) { return new Outcome(handler, s); }

            protected override void WriteElement(Stream s, Outcome element) { element.UnParse(s); }
            #endregion
        }
        #endregion

        #region ContentFields
        [ElementPriority(11)]
        public uint Version { get => _version;
            set { if (_version != value) { _version = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(12)]
        public OutcomeList Outcomes { get => _outcomes;
            set { if (_outcomes != value) { _outcomes = new OutcomeList(handler, value); OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(13)]
        public Flags Properties { get => _flags;
            set { if (_flags != value) { _flags = value; OnRCOLChanged(this, EventArgs.Empty); } } }

        public string Value => ValueBuilder;

        #endregion
    }

    public class JazzSelectOnParameterNode : JazzChunk
    {
        const string _tag = "SoPn";

        #region Attributes
        uint _version = 0x0101;
        GenericRCOLResource.GenericRCOLResource.ChunkReference _parameterDefinitionIndex;
        MatchList _matches;
        //0xDEADBEEF
        //'/DGN'
        #endregion

        #region Constructors
        public JazzSelectOnParameterNode(EventHandler handler) : base(handler, null) { }
        public JazzSelectOnParameterNode(EventHandler handler, Stream s) : base(handler, s) { }
        public JazzSelectOnParameterNode(EventHandler handler, JazzSelectOnParameterNode basis)
            : this(handler
            , basis._version
            , basis._parameterDefinitionIndex
            , basis._matches
            )
        { }
        public JazzSelectOnParameterNode(EventHandler handler
            , uint version
            , GenericRCOLResource.GenericRCOLResource.ChunkReference parameterDefinitionIndex
            , IEnumerable<Match> matches)
            : base(handler, null)
        {
            _version = version;
            _parameterDefinitionIndex = new GenericRCOLResource.GenericRCOLResource.ChunkReference(handler, parameterDefinitionIndex);
            _matches = new MatchList(handler, matches);
        }
        #endregion

        #region ARCOLBlock
        [ElementPriority(2)]
        public override string Tag => _tag;

        [ElementPriority(3)]
        public override uint ResourceType => 0x02EEDB92;

        protected override void Parse(Stream s)
        {
            base.Parse(s);

            BinaryReader r = new BinaryReader(s);
            _version = r.ReadUInt32();
            _parameterDefinitionIndex = new GenericRCOLResource.GenericRCOLResource.ChunkReference(handler, s);
            _matches = new MatchList(handler, s);
            Deadbeef.Parse(s);
            CloseDgn.Parse(s);
        }

        public override Stream UnParse()
        {
            Stream ms = base.UnParse();
            BinaryWriter w = new BinaryWriter(ms);

            w.Write(_version);
            if (_parameterDefinitionIndex == null) _parameterDefinitionIndex = new GenericRCOLResource.GenericRCOLResource.ChunkReference(handler, 0);
            _parameterDefinitionIndex.UnParse(ms);
            if (_matches == null) _matches = new MatchList(handler);
            _matches.UnParse(ms);
            Deadbeef.UnParse(ms);
            CloseDgn.UnParse(ms);

            return ms;
        }

        // public override AHandlerElement Clone(EventHandler handler) { return new JazzSelectOnParameterNode(requestedApiVersion, handler, this); }
        #endregion

        #region Sub-types
        public class Match : AHandlerElement, IEquatable<Match>
        {
            #region Attributes
            uint _testValue;
            ChunkReferenceList _decisionGraphIndexes;
            #endregion

            #region Constructors
            public Match(EventHandler handler) : base(handler) { }
            public Match(EventHandler handler, Stream s) : base(handler) { Parse(s); }
            public Match(EventHandler handler, Match basis)
                : this(handler
                , basis._testValue
                , basis._decisionGraphIndexes
                )
            {
            }
            public Match(EventHandler handler
                , uint testValue
                , IEnumerable<GenericRCOLResource.GenericRCOLResource.ChunkReference> decisionGraphIndexes)
                : base(handler)
            {
                _testValue = testValue;
                _decisionGraphIndexes = new ChunkReferenceList(handler, decisionGraphIndexes);
            }
            #endregion

            #region Data I/O
            void Parse(Stream s)
            {
                _testValue = new BinaryReader(s).ReadUInt32();
                _decisionGraphIndexes = new ChunkReferenceList(handler, s);
            }

            internal void UnParse(Stream s)
            {
                new BinaryWriter(s).Write(_testValue);
                if (_decisionGraphIndexes == null) _decisionGraphIndexes = new ChunkReferenceList(handler);
                _decisionGraphIndexes.UnParse(s);
            }
            #endregion

            #region AHandlerElement
            // public override AHandlerElement Clone(EventHandler handler) { return new Match(requestedApiVersion, handler, this); }
            public override List<string> ContentFields => GetContentFields(GetType());

            #endregion

            #region IEquatable<Animation>
            public bool Equals(Match other)
            {
                return _testValue.Equals(other._testValue) && _decisionGraphIndexes.Equals(other._decisionGraphIndexes);
            }
            #endregion

            #region ContentFields
            [ElementPriority(1)]
            public uint TestValue { get => _testValue;
                set { if (_testValue != value) { _testValue = value; OnElementChanged(); } } }
            [ElementPriority(2)]
            public ChunkReferenceList DecisionGraphIndexes { get => _decisionGraphIndexes;
                set { if (_decisionGraphIndexes != value) { _decisionGraphIndexes = new ChunkReferenceList(handler, value); OnElementChanged(); } } }

            public string Value => ValueBuilder;

            #endregion
        }
        public class MatchList : DependentList<Match>
        {
            #region Constructors
            public MatchList(EventHandler handler) : base(handler) { }
            public MatchList(EventHandler handler, IEnumerable<Match> basis) : base(handler, basis) { }
            public MatchList(EventHandler handler, Stream s) : base(handler, s) { }
            #endregion

            #region DependentList<Outcome>
            protected override Match CreateElement(Stream s) { return new Match(handler, s); }

            protected override void WriteElement(Stream s, Match element) { element.UnParse(s); }
            #endregion
        }
        #endregion

        #region ContentFields
        [ElementPriority(11)]
        public uint Version { get => _version;
            set { if (_version != value) { _version = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(12)]
        public GenericRCOLResource.GenericRCOLResource.ChunkReference ParameterDefinitionIndex { get => _parameterDefinitionIndex;
            set { if (_parameterDefinitionIndex != value) { _parameterDefinitionIndex = new GenericRCOLResource.GenericRCOLResource.ChunkReference(handler, value); OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(13)]
        public MatchList Matches { get => _matches;
            set { if (_matches != value) { _matches = new MatchList(handler, value); OnRCOLChanged(this, EventArgs.Empty); } } }

        public string Value => ValueBuilder;

        #endregion
    }

    public class JazzSelectOnDestinationNode : JazzChunk
    {
        const string _tag = "DG00";

        #region Attributes
        uint _version = 0x0101;
        MatchList _matches;
        //0xDEADBEEF
        //'/DGN'
        #endregion

        #region Constructors
        public JazzSelectOnDestinationNode(EventHandler handler) : base(handler, null) { }
        public JazzSelectOnDestinationNode(EventHandler handler, Stream s) : base(handler, s) { }
        public JazzSelectOnDestinationNode(EventHandler handler, JazzSelectOnDestinationNode basis)
            : this(handler
            , basis._version
            , basis._matches
            )
        { }
        public JazzSelectOnDestinationNode(EventHandler handler
            , uint version
            , IEnumerable<Match> matches)
            : base(handler, null)
        {
            _version = version;
            _matches = new MatchList(handler, matches);
        }
        #endregion

        #region ARCOLBlock
        [ElementPriority(2)]
        public override string Tag => _tag;

        [ElementPriority(3)]
        public override uint ResourceType => 0x02EEDBA5;

        protected override void Parse(Stream s)
        {
            base.Parse(s);

            BinaryReader r = new BinaryReader(s);
            _version = r.ReadUInt32();
            _matches = new MatchList(handler, s);
            Deadbeef.Parse(s);
            CloseDgn.Parse(s);
        }

        public override Stream UnParse()
        {
            Stream ms = base.UnParse();
            BinaryWriter w = new BinaryWriter(ms);

            w.Write(_version);
            if (_matches == null) _matches = new MatchList(handler);
            _matches.UnParse(ms);
            Deadbeef.UnParse(ms);
            CloseDgn.UnParse(ms);

            return ms;
        }

        // public override AHandlerElement Clone(EventHandler handler) { return new JazzSelectOnParameterNode(requestedApiVersion, handler, this); }
        #endregion

        #region Sub-types
        public class Match : AHandlerElement, IEquatable<Match>
        {
            #region Attributes
            GenericRCOLResource.GenericRCOLResource.ChunkReference _stateIndex;
            ChunkReferenceList _decisionGraphIndexes;
            #endregion

            #region Constructors
            public Match(EventHandler handler) : base(handler) { }
            public Match(EventHandler handler, Stream s) : base(handler) { Parse(s); }
            public Match(EventHandler handler, Match basis)
                : this(handler
                , basis._stateIndex
                , basis._decisionGraphIndexes
                )
            {
            }
            public Match(EventHandler handler
                , GenericRCOLResource.GenericRCOLResource.ChunkReference stateIndex
                , IEnumerable<GenericRCOLResource.GenericRCOLResource.ChunkReference> decisionGraphIndexes)
                : base(handler)
            {
                _stateIndex = stateIndex;
                _decisionGraphIndexes = new ChunkReferenceList(handler, decisionGraphIndexes);
            }
            #endregion

            #region Data I/O
            void Parse(Stream s)
            {
                _stateIndex = new GenericRCOLResource.GenericRCOLResource.ChunkReference(handler, s);
                _decisionGraphIndexes = new ChunkReferenceList(handler, s);
            }

            internal void UnParse(Stream s)
            {
                if (_stateIndex == null) _stateIndex = new GenericRCOLResource.GenericRCOLResource.ChunkReference(handler, 0);
                _stateIndex.UnParse(s);
                if (_decisionGraphIndexes == null) _decisionGraphIndexes = new ChunkReferenceList(handler);
                _decisionGraphIndexes.UnParse(s);
            }
            #endregion

            #region AHandlerElement
            // public override AHandlerElement Clone(EventHandler handler) { return new Match(requestedApiVersion, handler, this); }
            public override List<string> ContentFields => GetContentFields(GetType());

            #endregion

            #region IEquatable<Animation>
            public bool Equals(Match other)
            {
                return _stateIndex.Equals(other._stateIndex) && _decisionGraphIndexes.Equals(other._decisionGraphIndexes);
            }
            #endregion

            #region ContentFields
            [ElementPriority(1)]
            public GenericRCOLResource.GenericRCOLResource.ChunkReference StateIndex { get => _stateIndex;
                set { if (_stateIndex != value) { _stateIndex = new GenericRCOLResource.GenericRCOLResource.ChunkReference(handler, value); OnElementChanged(); } } }
            [ElementPriority(2)]
            public ChunkReferenceList DecisionGraphIndexes { get => _decisionGraphIndexes;
                set { if (_decisionGraphIndexes != value) { _decisionGraphIndexes = new ChunkReferenceList(handler, value); OnElementChanged(); } } }

            public string Value => ValueBuilder;

            #endregion
        }
        public class MatchList : DependentList<Match>
        {
            #region Constructors
            public MatchList(EventHandler handler) : base(handler) { }
            public MatchList(EventHandler handler, IEnumerable<Match> basis) : base(handler, basis) { }
            public MatchList(EventHandler handler, Stream s) : base(handler, s) { }
            #endregion

            #region DependentList<Outcome>
            protected override Match CreateElement(Stream s) { return new Match(handler, s); }

            protected override void WriteElement(Stream s, Match element) { element.UnParse(s); }
            #endregion
        }
        #endregion

        #region ContentFields
        [ElementPriority(11)]
        public uint Version { get => _version;
            set { if (_version != value) { _version = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(12)]
        public MatchList Matches { get => _matches;
            set { if (_matches != value) { _matches = new MatchList(handler, value); OnRCOLChanged(this, EventArgs.Empty); } } }

        public string Value => ValueBuilder;

        #endregion
    }

    public class JazzNextStateNode : JazzChunk
    {
        const string _tag = "SNSN";

        #region Attributes
        uint _version = 0x0101;
        GenericRCOLResource.GenericRCOLResource.ChunkReference _stateIndex;
        //'/DGN'
        #endregion

        #region Constructors
        public JazzNextStateNode(EventHandler handler) : base(handler, null) { }
        public JazzNextStateNode(EventHandler handler, Stream s) : base(handler, s) { }
        public JazzNextStateNode(EventHandler handler, JazzNextStateNode basis)
            : this(handler
            , basis._version
            , basis._stateIndex
            )
        { }
        public JazzNextStateNode(EventHandler handler
            , uint version
            , GenericRCOLResource.GenericRCOLResource.ChunkReference stateIndex)
            : base(handler, null)
        {
            _version = version;
            _stateIndex = stateIndex;
        }
        #endregion

        #region ARCOLBlock
        [ElementPriority(2)]
        public override string Tag => _tag;

        [ElementPriority(3)]
        public override uint ResourceType => 0x02EEEBDC;

        protected override void Parse(Stream s)
        {
            base.Parse(s);

            BinaryReader r = new BinaryReader(s);
            _version = r.ReadUInt32();
            _stateIndex = new GenericRCOLResource.GenericRCOLResource.ChunkReference(handler, s);
            CloseDgn.Parse(s);
        }

        public override Stream UnParse()
        {
            Stream ms = base.UnParse();
            BinaryWriter w = new BinaryWriter(ms);

            w.Write(_version);
            if (_stateIndex == null) _stateIndex = new GenericRCOLResource.GenericRCOLResource.ChunkReference(handler, 0);
            _stateIndex.UnParse(ms);
            CloseDgn.UnParse(ms);

            return ms;
        }

        // public override AHandlerElement Clone(EventHandler handler) { return new JazzNextStateNode(requestedApiVersion, handler, this); }
        #endregion

        #region ContentFields
        [ElementPriority(11)]
        public uint Version { get => _version;
            set { if (_version != value) { _version = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(12)]
        public GenericRCOLResource.GenericRCOLResource.ChunkReference StateIndex { get => _stateIndex;
            set { if (_stateIndex != value) { _stateIndex = new GenericRCOLResource.GenericRCOLResource.ChunkReference(handler, value); OnRCOLChanged(this, EventArgs.Empty); } } }

        public string Value => ValueBuilder;

        #endregion
    }

    public class JazzCreatePropNode : JazzChunk
    {
        const string _tag = "Prop";

        #region Attributes
        uint _version = 0x0100;
        GenericRCOLResource.GenericRCOLResource.ChunkReference _actorDefinitionIndex;
        GenericRCOLResource.GenericRCOLResource.ChunkReference _parameterDefinitionIndex;
        TGIBlock _propResource;
        uint _unknown2;
        uint _unknown3;
        uint _unknown4;
        uint _unknown5;
        //uint unknown6;
        ChunkReferenceList _decisionGraphIndexes;
        //'/DGN'
        #endregion

        #region Constructors
        public JazzCreatePropNode(EventHandler handler) : base(handler, null) { }
        public JazzCreatePropNode(EventHandler handler, Stream s) : base(handler, s) { }
        public JazzCreatePropNode(EventHandler handler, JazzCreatePropNode basis)
            : this(handler
            , basis._version
            , basis._actorDefinitionIndex
            , basis._parameterDefinitionIndex
            , basis._propResource
            , basis._unknown2
            , basis._unknown3
            , basis._unknown4
            , basis._unknown5
                //, basis.unknown6
            , basis._decisionGraphIndexes
            )
        { }
        public JazzCreatePropNode(EventHandler handler
            , uint version
            , GenericRCOLResource.GenericRCOLResource.ChunkReference actorDefinitionIndex
            , GenericRCOLResource.GenericRCOLResource.ChunkReference parameterDefinitionIndex
            , IResourceKey propResource
            , uint unknown2
            , uint unknown3
            , uint unknown4
            , uint unknown5
            //, uint unknown6
            , IEnumerable<GenericRCOLResource.GenericRCOLResource.ChunkReference> decisionGraphIndexes)
            : base(handler, null)
        {
            _version = version;
            _actorDefinitionIndex = new GenericRCOLResource.GenericRCOLResource.ChunkReference(handler, actorDefinitionIndex);
            _parameterDefinitionIndex = parameterDefinitionIndex;
            _propResource = new TGIBlock(handler, "ITG", propResource);
            _unknown2 = unknown2;
            _unknown3 = unknown3;
            _unknown4 = unknown4;
            _unknown5 = unknown5;
            //this.unknown6 = unknown6;
            _decisionGraphIndexes = new ChunkReferenceList(handler, decisionGraphIndexes);
        }
        #endregion

        #region ARCOLBlock
        [ElementPriority(2)]
        public override string Tag => _tag;

        [ElementPriority(3)]
        public override uint ResourceType => 0x02EEEBDD;

        protected override void Parse(Stream s)
        {
            base.Parse(s);

            BinaryReader r = new BinaryReader(s);

            _version = r.ReadUInt32();
            _actorDefinitionIndex = new GenericRCOLResource.GenericRCOLResource.ChunkReference(handler, s);
            _parameterDefinitionIndex = new GenericRCOLResource.GenericRCOLResource.ChunkReference(handler, s);
            _propResource = new TGIBlock(handler, "ITG", s);
            _unknown2 = r.ReadUInt32();
            _unknown3 = r.ReadUInt32();
            _unknown4 = r.ReadUInt32();
            _unknown5 = r.ReadUInt32();
            //this.unknown6 = r.ReadUInt32();
            _decisionGraphIndexes = new ChunkReferenceList(handler, s);
            CloseDgn.Parse(s);
        }

        public override Stream UnParse()
        {
            Stream ms = base.UnParse();
            BinaryWriter w = new BinaryWriter(ms);

            w.Write(_version);
            if (_actorDefinitionIndex == null) _actorDefinitionIndex = new GenericRCOLResource.GenericRCOLResource.ChunkReference(handler, 0);
            _actorDefinitionIndex.UnParse(ms);
            if (_parameterDefinitionIndex == null) _parameterDefinitionIndex = new GenericRCOLResource.GenericRCOLResource.ChunkReference(handler, 0);
            _parameterDefinitionIndex.UnParse(ms);
            if (_propResource == null) _propResource = new TGIBlock(handler, "ITG");
            _propResource.UnParse(ms);
            w.Write(_unknown2);
            w.Write(_unknown3);
            w.Write(_unknown4);
            w.Write(_unknown5);
            //w.Write(unknown6);
            if (_decisionGraphIndexes == null) _decisionGraphIndexes = new ChunkReferenceList(handler);
            _decisionGraphIndexes.UnParse(ms);
            CloseDgn.UnParse(ms);

            return ms;
        }

        // public override AHandlerElement Clone(EventHandler handler) { return new JazzCreatePropNode(requestedApiVersion, handler, this); }
        #endregion

        #region ContentFields
        [ElementPriority(11)]
        public uint Version { get => _version;
            set { if (_version != value) { _version = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(12)]
        public GenericRCOLResource.GenericRCOLResource.ChunkReference ActorDefinitionIndex { get => _actorDefinitionIndex;
            set { if (_actorDefinitionIndex != value) { _actorDefinitionIndex = new GenericRCOLResource.GenericRCOLResource.ChunkReference(handler, value); OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(13)]
        public GenericRCOLResource.GenericRCOLResource.ChunkReference ParameterDefinitionIndex { get => _parameterDefinitionIndex;
            set { if (_parameterDefinitionIndex != value) { _parameterDefinitionIndex = new GenericRCOLResource.GenericRCOLResource.ChunkReference(handler, value); OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(14)]
        public IResourceKey PropResource { get => _propResource;
            set { if (_propResource != value) { _propResource = new TGIBlock(handler, "ITG", value); OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(15)]
        public uint Unknown2 { get => _unknown2;
            set { if (_unknown2 != value) { _unknown2 = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(16)]
        public uint Unknown3 { get => _unknown3;
            set { if (_unknown3 != value) { _unknown3 = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(17)]
        public uint Unknown4 { get => _unknown4;
            set { if (_unknown4 != value) { _unknown4 = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(18)]
        public uint Unknown5 { get => _unknown5;
            set { if (_unknown5 != value) { _unknown5 = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        //[ElementPriority(19)]
        //public uint Unknown6 { get { return unknown6; } set { if (unknown6 != value) { unknown6 = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(20)]
        public ChunkReferenceList DecisionGraphIndexes { get => _decisionGraphIndexes;
            set { if (_decisionGraphIndexes != value) { _decisionGraphIndexes = new ChunkReferenceList(handler, value); OnRCOLChanged(this, EventArgs.Empty); } } }

        public string Value => ValueBuilder;

        #endregion
    }

    public class JazzActorOperationNode : JazzChunk
    {
        const string _tag = "AcOp";

        #region Attributes
        uint _version = 0x0100;
        GenericRCOLResource.GenericRCOLResource.ChunkReference _actorDefinitionIndex;
        ActorOperation _actorOp;
        uint _operand;
        uint _unknown1;
        uint _unknown2;
        uint _unknown3;
        ChunkReferenceList _decisionGraphIndexes;
        //'/DGN'
        #endregion

        #region Constructors
        public JazzActorOperationNode(EventHandler handler) : base(handler, null) { }
        public JazzActorOperationNode(EventHandler handler, Stream s) : base(handler, s) { }
        public JazzActorOperationNode(EventHandler handler, JazzActorOperationNode basis)
            : this(handler
            , basis._version
            , basis._actorDefinitionIndex
            , basis._actorOp
            , basis._operand
            , basis._unknown1
            , basis._unknown2
            , basis._unknown3
            , basis._decisionGraphIndexes
            )
        { }
        public JazzActorOperationNode(EventHandler handler
            , uint version
            , GenericRCOLResource.GenericRCOLResource.ChunkReference actorDefinitionIndex
            , ActorOperation actorOp
            , uint operand
            , uint unknown1
            , uint unknown2
            , uint unknown3
            , IEnumerable<GenericRCOLResource.GenericRCOLResource.ChunkReference> decisionGraphIndexes)
            : base(handler, null)
        {
            _version = version;
            _actorDefinitionIndex = actorDefinitionIndex;
            _actorOp = actorOp;
            _operand = operand;
            _unknown1 = unknown1;
            _unknown2 = unknown2;
            _unknown3 = unknown3;
            _decisionGraphIndexes = new ChunkReferenceList(handler, decisionGraphIndexes);
        }
        #endregion

        #region ARCOLBlock
        [ElementPriority(2)]
        public override string Tag => _tag;

        [ElementPriority(3)]
        public override uint ResourceType => 0x02EEEBDE;

        protected override void Parse(Stream s)
        {
            base.Parse(s);

            BinaryReader r = new BinaryReader(s);
            _version = r.ReadUInt32();
            _actorDefinitionIndex = new GenericRCOLResource.GenericRCOLResource.ChunkReference(handler, s);
            _actorOp = (ActorOperation)r.ReadUInt32();
            _operand = r.ReadUInt32();
            _unknown1 = r.ReadUInt32();
            _unknown2 = r.ReadUInt32();
            _unknown3 = r.ReadUInt32();
            _decisionGraphIndexes = new ChunkReferenceList(handler, s);
            CloseDgn.Parse(s);
        }

        public override Stream UnParse()
        {
            Stream ms = base.UnParse();
            BinaryWriter w = new BinaryWriter(ms);

            w.Write(_version);
            if (_actorDefinitionIndex == null) _actorDefinitionIndex = new GenericRCOLResource.GenericRCOLResource.ChunkReference(handler, 0);
            _actorDefinitionIndex.UnParse(ms);
            w.Write((uint)_actorOp);
            w.Write(_operand);
            w.Write(_unknown1);
            w.Write(_unknown2);
            w.Write(_unknown3);
            if (_decisionGraphIndexes == null) _decisionGraphIndexes = new ChunkReferenceList(handler);
            _decisionGraphIndexes.UnParse(ms);
            CloseDgn.UnParse(ms);

            return ms;
        }

        // public override AHandlerElement Clone(EventHandler handler) { return new JazzActorOperationNode(requestedApiVersion, handler, this); }
        #endregion

        #region Sub-types
        public enum ActorOperation : uint
        {
            None = 0,
            SetMirror = 1,
        }
        #endregion

        #region ContentFields
        [ElementPriority(11)]
        public uint Version { get => _version;
            set { if (_version != value) { _version = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(12)]
        public GenericRCOLResource.GenericRCOLResource.ChunkReference ActorDefinitionIndex { get => _actorDefinitionIndex;
            set { if (_actorDefinitionIndex != value) { _actorDefinitionIndex = new GenericRCOLResource.GenericRCOLResource.ChunkReference(handler, value); OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(13)]
        public ActorOperation ActorOp { get => _actorOp;
            set { if (_actorOp != value) { _actorOp = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(14)]
        public uint Operand { get => _operand;
            set { if (_operand != value) { _operand = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(15)]
        public uint Unknown1 { get => _unknown1;
            set { if (_unknown1 != value) { _unknown1 = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(16)]
        public uint Unknown2 { get => _unknown2;
            set { if (_unknown2 != value) { _unknown2 = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(17)]
        public uint Unknown3 { get => _unknown3;
            set { if (_unknown3 != value) { _unknown3 = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(18)]
        public ChunkReferenceList DecisionGraphIndexes { get => _decisionGraphIndexes;
            set { if (_decisionGraphIndexes != value) { _decisionGraphIndexes = new ChunkReferenceList(OnRCOLChanged, value); OnRCOLChanged(this, EventArgs.Empty); } } }

        public string Value => ValueBuilder;

        #endregion
    }

    public class JazzStopAnimationNode : JazzChunk
    {
        const string _tag = "Stop";

        #region Attributes
        uint _version = 0x0104;
        JazzAnimationFlags _animationFlags;
        AnimationPriority _animationPriority;
        float _unknown1;
        float _blendInTime;
        float _blendOutTime;
        float _unknown4;
        float _speed;
        GenericRCOLResource.GenericRCOLResource.ChunkReference _actorDefinitionIndex;
        AnimationPriority _timingPriority;
        uint _unknown6;
        uint _unknown7;
        uint _unknown8;
        uint _unknown9;
        uint _unknown10;
        uint _unknown11;
        //0xDEADBEEF
        ChunkReferenceList _decisionGraphIndexes;
        //'/DGN'
        #endregion

        #region Constructors
        public JazzStopAnimationNode(EventHandler handler) : base(handler, null) { }
        public JazzStopAnimationNode(EventHandler handler, Stream s) : base(handler, s) { }
        public JazzStopAnimationNode(EventHandler handler, JazzStopAnimationNode basis)
            : this(handler
            , basis._version
            , basis._animationFlags
            , basis._animationPriority
            , basis._unknown1
            , basis._blendInTime
            , basis._blendOutTime
            , basis._unknown4
            , basis._speed
            , basis._actorDefinitionIndex
            , basis._timingPriority
            , basis._unknown6
            , basis._unknown7
            , basis._unknown8
            , basis._unknown9
            , basis._unknown10
            , basis._unknown11
            , basis._decisionGraphIndexes
            )
        { }
        public JazzStopAnimationNode(EventHandler handler
            , uint version
            , JazzAnimationFlags animationFlags
            , AnimationPriority animationPriority
            , float unknown1
            , float blendInTime
            , float blendOutTime
            , float unknown4
            , float speed
            , GenericRCOLResource.GenericRCOLResource.ChunkReference actorDefinitionIndex
            , AnimationPriority timingPriority
            , uint unknown6
            , uint unknown7
            , uint unknown8
            , uint unknown9
            , uint unknown10
            , uint unknown11
            , IEnumerable<GenericRCOLResource.GenericRCOLResource.ChunkReference> decisionGraphIndexes)
            : base(handler, null)
        {
            _version = version;
            _animationFlags = animationFlags;
            _animationPriority = animationPriority;
            _unknown1 = unknown1;
            _blendInTime = blendInTime;
            _blendOutTime = blendOutTime;
            _unknown4 = unknown4;
            _speed = speed;
            _actorDefinitionIndex = new GenericRCOLResource.GenericRCOLResource.ChunkReference(handler, actorDefinitionIndex);
            _timingPriority = timingPriority;
            _unknown6 = unknown6;
            _unknown7 = unknown7;
            _unknown8 = unknown8;
            _unknown9 = unknown9;
            _unknown10 = unknown10;
            _unknown11 = unknown11;
            _decisionGraphIndexes = new ChunkReferenceList(handler, decisionGraphIndexes);
        }
        #endregion

        #region ARCOLBlock
        [ElementPriority(2)]
        public override string Tag => _tag;

        [ElementPriority(3)]
        public override uint ResourceType => 0x0344D438;

        protected override void Parse(Stream s)
        {
            base.Parse(s);

            BinaryReader r = new BinaryReader(s);

            _version = r.ReadUInt32();
            _animationFlags = (JazzAnimationFlags)r.ReadUInt32();
            _animationPriority = (AnimationPriority)r.ReadUInt32();
            _unknown1 = r.ReadSingle();
            _blendInTime = r.ReadSingle();
            _blendOutTime = r.ReadSingle();
            _unknown4 = r.ReadSingle();
            _speed = r.ReadSingle();
            _actorDefinitionIndex = new GenericRCOLResource.GenericRCOLResource.ChunkReference(handler, s);
            _timingPriority = (AnimationPriority)r.ReadUInt32();
            _unknown6 = r.ReadUInt32();
            _unknown7 = r.ReadUInt32();
            _unknown8 = r.ReadUInt32();
            _unknown9 = r.ReadUInt32();
            _unknown10 = r.ReadUInt32();
            _unknown11 = r.ReadUInt32();
            Deadbeef.Parse(s);
            _decisionGraphIndexes = new ChunkReferenceList(handler, s);
            CloseDgn.Parse(s);
        }

        public override Stream UnParse()
        {
            Stream ms = base.UnParse();
            BinaryWriter w = new BinaryWriter(ms);

            w.Write(_version);
            w.Write((uint)_animationFlags);
            w.Write((uint)_animationPriority);
            w.Write(_unknown1);
            w.Write(_blendInTime);
            w.Write(_blendOutTime);
            w.Write(_unknown4);
            w.Write(_speed);
            if (_actorDefinitionIndex == null) _actorDefinitionIndex = new GenericRCOLResource.GenericRCOLResource.ChunkReference(handler, 0);
            _actorDefinitionIndex.UnParse(ms);
            w.Write((uint)_timingPriority);
            w.Write(_unknown6);
            w.Write(_unknown7);
            w.Write(_unknown8);
            w.Write(_unknown9);
            w.Write(_unknown10);
            w.Write(_unknown11);
            Deadbeef.UnParse(ms);
            if (_decisionGraphIndexes == null) _decisionGraphIndexes = new ChunkReferenceList(handler);
            _decisionGraphIndexes.UnParse(ms);
            CloseDgn.UnParse(ms);

            return ms;
        }

        // public override AHandlerElement Clone(EventHandler handler) { return new JazzStopAnimationNode(requestedApiVersion, handler, this); }
        #endregion

        #region ContentFields
        [ElementPriority(11)]
        public uint Version { get => _version;
            set { if (_version != value) { _version = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(12)]
        public JazzAnimationFlags AnimationFlags { get => _animationFlags;
            set { if (_animationFlags != value) { _animationFlags = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(13)]
        public AnimationPriority AnimationPriority1 { get => _animationPriority;
            set { if (_animationPriority != value) { _animationPriority = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(14)]
        public float Unknown1 { get => _unknown1;
            set { if (_unknown1 != value) { _unknown1 = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(15)]
        public float BlendInTime { get => _blendInTime;
            set { if (_blendInTime != value) { _blendInTime = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(16)]
        public float BlendOutTime { get => _blendOutTime;
            set { if (_blendOutTime != value) { _blendOutTime = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(17)]
        public float Unknown4 { get => _unknown4;
            set { if (_unknown4 != value) { _unknown4 = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(18)]
        public float Speed { get => _speed;
            set { if (_speed != value) { _speed = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(19)]
        public GenericRCOLResource.GenericRCOLResource.ChunkReference ActorDefinitionIndex { get => _actorDefinitionIndex;
            set { if (_actorDefinitionIndex != value) { _actorDefinitionIndex = new GenericRCOLResource.GenericRCOLResource.ChunkReference(handler, value); OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(20)]
        public AnimationPriority TimingPriority { get => _timingPriority;
            set { if (_timingPriority != value) { _timingPriority = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(21)]
        public uint Unknown6 { get => _unknown6;
            set { if (_unknown6 != value) { _unknown6 = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(22)]
        public uint Unknown7 { get => _unknown7;
            set { if (_unknown7 != value) { _unknown7 = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(23)]
        public uint Unknown8 { get => _unknown8;
            set { if (_unknown8 != value) { _unknown8 = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(24)]
        public uint Unknown9 { get => _unknown9;
            set { if (_unknown9 != value) { _unknown9 = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(25)]
        public uint Unknown10 { get => _unknown10;
            set { if (_unknown10 != value) { _unknown10 = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(26)]
        public uint Unknown11 { get => _unknown11;
            set { if (_unknown11 != value) { _unknown11 = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(27)]
        public ChunkReferenceList DecisionGraphIndexes { get => _decisionGraphIndexes;
            set { if (_decisionGraphIndexes != value) { _decisionGraphIndexes = new ChunkReferenceList(handler, value); OnRCOLChanged(this, EventArgs.Empty); } } }

        public string Value => ValueBuilder;

        #endregion
    }
    #endregion
}