/***************************************************************************
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Regul.S3PI.Interfaces;

namespace Regul.S3PI.Resources.RCOLChunks
{
    public class LITE : ARCOLBlock
    {
        const string TAG = "LITE";

        #region Attributes
        uint tag = (uint)FOURCC(TAG);
        uint version = 4;
        uint unknown1 = 0x84;
        ushort unknown2 = 0;
        LightSourceList lightSources = null;
        OccluderList occluders = null;
        #endregion

        #region Constructors
        public LITE(EventHandler handler) : base(handler, null) { }
        public LITE(EventHandler handler, Stream s) : base(handler, s) { }
        public LITE(EventHandler handler, LITE basis)
            : base(handler, null)
        {
            this.version = basis.version;
            this.unknown1 = basis.unknown1;
            this.lightSources = basis.lightSources == null ? null : new LightSourceList(handler, basis.lightSources);
            this.occluders = basis.occluders == null ? null : new OccluderList(handler, basis.occluders);
            this.unknown2 = basis.unknown2;
        }
        #endregion

        #region ARCOLBlock
        [ElementPriority(2)]
        public override string Tag { get { return TAG; } }

        [ElementPriority(3)]
        public override uint ResourceType { get { return 0x03B4C61D; } }

        protected override void Parse(Stream s)
        {
            BinaryReader r = new BinaryReader(s);
            tag = r.ReadUInt32();
            if (tag != (uint)FOURCC(TAG))
                throw new InvalidDataException(String.Format("Invalid Tag read: '{0}'; expected: '{1}'; at 0x{2:X8}", FOURCC(tag), TAG, s.Position));
            version = r.ReadUInt32();
            unknown1 = r.ReadUInt32();
            byte lsCount = r.ReadByte();
            byte ssCount = r.ReadByte();
            unknown2 = r.ReadUInt16();
            lightSources = new LightSourceList(handler, lsCount, s);
            occluders = new OccluderList(handler, ssCount, s);
        }

        public override Stream UnParse()
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter w = new BinaryWriter(ms);

            w.Write(tag);
            w.Write(version);
            w.Write(unknown1);
            if (lightSources == null) lightSources = new LightSourceList(handler);
            w.Write((byte)lightSources.Count);
            if (occluders == null) occluders = new OccluderList(handler);
            w.Write((byte)occluders.Count);
            w.Write(unknown2);
            lightSources.UnParse(ms);
            occluders.UnParse(ms);

            return ms;
        }
        #endregion

        #region Sub-types
        public class LightSource : AHandlerElement, IEquatable<LightSource>
        {
            #region Attributes
            LightSourceType lightSourceType = LightSourceType.Unknown;
            Vertex transform;
            RGB color;
            float intensity;
            AbstractLightSourceType lightSourceData;
            #endregion

            #region Constructors
            public LightSource(EventHandler handler)
                : this(handler
                , LightSourceType.Unknown
                , new Vertex(handler, 0f, 0f, 0f)
                , new RGB(handler, 0f, 0f, 0f)
                , 0f
                , LightSourceTypeFactory.create(handler,
                    LightSourceType.Unknown,
                    new GeneralLightSourceType(null, new float[] { 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, }))
                ) { }
            public LightSource(EventHandler handler, LightSource basis)
                : this(handler
                , basis.lightSourceType
                , basis.transform
                , basis.color
                , basis.intensity
                , basis.lightSourceData
                ) { }
            public LightSource(EventHandler handler
                , LightSourceType lightSourceType
                , Vertex transform
                , RGB color
                , float intensity
                , AbstractLightSourceType lightSourceData)
                : base(handler)
            {
                this.lightSourceType = lightSourceType;
                this.transform = new Vertex(handler, transform);
                this.color = new RGB(handler, color);
                this.intensity = intensity;
                this.lightSourceData = LightSourceTypeFactory.create(handler, lightSourceType, lightSourceData);
            }
            public LightSource(EventHandler handler, Stream s) : base(handler) { Parse(s); }
            #endregion

            #region Data I/O
            void Parse(Stream s)
            {
                BinaryReader r = new BinaryReader(s);
                lightSourceType = (LightSourceType)r.ReadUInt32();
                transform = new Vertex(handler, s);
                color = new RGB(handler, s);
                intensity = r.ReadSingle();
                lightSourceData = LightSourceTypeFactory.create(handler, lightSourceType, s);
            }

            internal void UnParse(Stream s)
            {
                BinaryWriter w = new BinaryWriter(s);
                w.Write((uint)lightSourceType);
                transform.UnParse(s);
                color.UnParse(s);
                w.Write(intensity);
                lightSourceData.UnParse(s);
            }
            #endregion

            #region AHandlerElement Members
            /// <summary>
            /// The list of available field names on this API object
            /// </summary>
            public override List<string> ContentFields
            {
                get
                {
                    List<string> res = GetContentFields(GetType());
                    /* Try without this lot for now
                    List<string> removals = new List<string>(new string[]{
                        "LightSourceData",
                        "SpotLightLightSourceData",
                        "LampShadeLightSourceData",
                        "TubeLightLightSourceData",
                        "SquareWindowLightSourceData",
                        "CircularWindowLightSourceData",
                        "SquareAreaLightSourceData",
                        "DiscAreaLightSourceData",
                    });
                    switch (lightSourceType)
                    {
                        case LightSourceType.Spot: removals.Remove("SpotLightLightSourceData"); break;
                        case LightSourceType.LampShade: removals.Remove("LampShadeLightSourceData"); break;
                        case LightSourceType.TubeLight: removals.Remove("TubeLightLightSourceData"); break;
                        case LightSourceType.SquareWindow: removals.Remove("SquareWindowLightSourceData"); break;
                        case LightSourceType.CircularWindow: removals.Remove("CircularWindowLightSourceData"); break;
                        case LightSourceType.SquareAreaLight: removals.Remove("SquareAreaLightSourceData"); break;
                        case LightSourceType.DiscAreaLight: removals.Remove("DiscAreaLightSourceData"); break;
                        default: removals.Remove("LightSourceData"); break;
                    }
                    foreach (var rem in removals) res.Remove(rem);
                     */
                    return res;
                }
            }
            #endregion

            #region IEquatable<LightSource> Members

            public bool Equals(LightSource other)
            {
                return lightSourceType.Equals(other.lightSourceType)
                    && transform.Equals(other.transform)
                    && color.Equals(other.color)
                    && intensity.Equals(other.intensity)
                    && lightSourceData.Equals(other.lightSourceData)
                    ;
            }

            public override bool Equals(object obj)
            {
                return obj as LightSource != null ? this.Equals(obj as LightSource) : false;
            }

            public override int GetHashCode()
            {
                return lightSourceType.GetHashCode()
                    ^ transform.GetHashCode()
                    ^ color.GetHashCode()
                    ^ intensity.GetHashCode()
                    ^ lightSourceData.GetHashCode()
                    ;
            }

            #endregion

            #region Sub-types
            public class RGB : Vertex
            {
                #region Constructors
                public RGB(EventHandler handler) : base(handler) { }
                public RGB(EventHandler handler, Stream s) : base(handler, s) { }
                public RGB(EventHandler handler, RGB basis) : base(handler, basis.X, basis.Y, basis.Z) { }
                public RGB(EventHandler handler, float x, float y, float z) : base(handler, x, y, z) { }
                #endregion

                public override List<string> ContentFields
                {
                    get
                    {
                        List<string> res = GetContentFields(GetType());
                        res.Remove("X");
                        res.Remove("Y");
                        res.Remove("Z");
                        return res;
                    }
                }

                [ElementPriority(1)]
                public float R { get { return X; } set { X = value; } }
                [ElementPriority(2)]
                public float G { get { return Y; } set { Y = value; } }
                [ElementPriority(3)]
                public float B { get { return Z; } set { Z = value; } }
            }

            public enum LightSourceType : uint
            {
                Unknown = 0x00,//unused
                Ambient = 0x01,//unused
                Directional = 0x02,//unused
                Point = 0x03,
                Spot = 0x04,
                LampShade = 0x05,
                TubeLight = 0x06,
                SquareWindow = 0x07,
                CircularWindow = 0x08,
                SquareAreaLight = 0x09,
                DiscAreaLight = 0x0A,
                WorldLight = 0x0B,
            }

            private static class LightSourceTypeFactory
            {
                public static AbstractLightSourceType create(EventHandler handler, LightSourceType lightSourceType,
                    Stream s)
                {
                    switch (lightSourceType)
                    {
                        case LightSourceType.Unknown: break;
                        case LightSourceType.Ambient: break;
                        case LightSourceType.Directional: break;
                        case LightSourceType.Point: break;
                        case LightSourceType.Spot:
                            return new SpotLightSourceType(handler, s);
                        case LightSourceType.LampShade:
                            return new LampShadeLightSourceType(handler, s);
                        case LightSourceType.TubeLight:
                            return new TubeLightSourceType(handler, s);
                        case LightSourceType.SquareWindow:
                            return new SquareWindowLightSourceType(handler, s);
                        case LightSourceType.CircularWindow:
                            return new CircularWindowLightSourceType(handler, s);
                        case LightSourceType.SquareAreaLight: break;
                        case LightSourceType.DiscAreaLight: break;
                        case LightSourceType.WorldLight: break;
                    }
                    return new GeneralLightSourceType(handler, s);
                }
                public static AbstractLightSourceType create(EventHandler handler, LightSourceType lightSourceType,
                    AbstractLightSourceType lightSourceData)
                {
                    switch (lightSourceType)
                    {
                        case LightSourceType.Unknown: break;
                        case LightSourceType.Ambient: break;
                        case LightSourceType.Directional: break;
                        case LightSourceType.Point: break;
                        case LightSourceType.Spot:
                            return new SpotLightSourceType(handler, (SpotLightSourceType)lightSourceData);
                        case LightSourceType.LampShade:
                            return new LampShadeLightSourceType(handler, (LampShadeLightSourceType)lightSourceData);
                        case LightSourceType.TubeLight:
                            return new TubeLightSourceType(handler, (TubeLightSourceType)lightSourceData);
                        case LightSourceType.SquareWindow:
                            return new SquareWindowLightSourceType(handler, (SquareWindowLightSourceType)lightSourceData);
                        case LightSourceType.CircularWindow:
                            return new CircularWindowLightSourceType(handler, (CircularWindowLightSourceType)lightSourceData);
                        case LightSourceType.SquareAreaLight: break;
                        case LightSourceType.DiscAreaLight: break;
                        case LightSourceType.WorldLight: break;
                    }
                    return new GeneralLightSourceType(handler, (GeneralLightSourceType)lightSourceData);
                }
            }

            public abstract class AbstractLightSourceType : AHandlerElement
            {
                public AbstractLightSourceType(EventHandler handler) : base(handler) { }
                internal abstract void UnParse(Stream s);

                #region AHandlerElement
                public override List<string> ContentFields { get { return GetContentFields(GetType()); } }
                #endregion

                public string Value { get { return ValueBuilder; } }
            }

            public class GeneralLightSourceType : AbstractLightSourceType
            {
                #region Attributes
                Single[] lightSourceData;// 24
                #endregion

                #region Constructors
                public GeneralLightSourceType(EventHandler handler) : this(handler, new float[] { 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, }) { }
                public GeneralLightSourceType(EventHandler handler, GeneralLightSourceType basis) : this(handler, basis.lightSourceData) { }
                public GeneralLightSourceType(EventHandler handler, IEnumerable<float> lightSourceData)
                    : base(handler)
                {
                    this.lightSourceData = lightSourceData.ToArray();
                    if (this.lightSourceData.Length != 24)
                        throw new ArgumentException("Must provide 24 values", "lightSourceData");
                }
                public GeneralLightSourceType(EventHandler handler, Stream s) : base(handler) { Parse(s); }
                #endregion

                #region Data I/O
                void Parse(Stream s)
                {
                    BinaryReader r = new BinaryReader(s);
                    this.lightSourceData = new Single[24];
                    for (int i = 0; i < lightSourceData.Length; lightSourceData[i++] = r.ReadSingle()) { }
                }

                internal override void UnParse(Stream s)
                {
                    BinaryWriter w = new BinaryWriter(s);
                    lightSourceData.ToList().ForEach(item => w.Write(item));
                }
                #endregion

                #region AHandlerElement
                public override List<string> ContentFields { get { return GetContentFields(GetType()); } }
                #endregion

                #region Content Fields
                [ElementPriority(1)]
                public float[] LightSourceData
                {
                    get { return (float[])lightSourceData.Clone(); ; }
                    set
                    {
                        if (value.Length != this.lightSourceData.Length) throw new ArgumentLengthException("LightSourceData", this.lightSourceData.Length);
                        if (!lightSourceData.Equals(value)) { lightSourceData = value == null ? null : (float[])value.Clone(); this.OnElementChanged(); }
                    }
                }
                #endregion

                public string Value { get { return ValueBuilder; } }
            }

            public class SpotLightSourceType : AbstractLightSourceType
            {
                #region Attributes
                Vertex location;// 24 - 3
                Single falloffAngle; // 21 - 1
                Single blurScale;// 20 - 1
                Single[] unusedLightSourceData;// 19
                #endregion

                #region Constructors
                public SpotLightSourceType(EventHandler handler, GeneralLightSourceType basis) : this(handler, basis.LightSourceData) { }
                public SpotLightSourceType(EventHandler handler, IEnumerable<float> lightSourceData)
                    : this(handler
                    , new Vertex(handler, lightSourceData.ElementAt(0), lightSourceData.ElementAt(1), lightSourceData.ElementAt(2))
                    , lightSourceData.ElementAt(3)
                    , lightSourceData.ElementAt(4)
                    , lightSourceData.Skip(5)
                    ) { }
                public SpotLightSourceType(EventHandler handler)
                    : this(handler
                    , new Vertex(handler, 0f, 0f, 0f)
                    , 0f
                    , 0f
                    , new float[] { 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, }
                    ) { }
                public SpotLightSourceType(EventHandler handler, SpotLightSourceType basis)
                    : this(handler
                    , basis.location
                    , basis.falloffAngle
                    , basis.blurScale
                    , basis.unusedLightSourceData
                    ) { }
                public SpotLightSourceType(EventHandler handler
                    , Vertex location
                    , float falloffAngle
                    , float blurScale
                    , IEnumerable<float> unusedLightSourceData)
                    : base(handler)
                {
                    this.location = new Vertex(handler, location);
                    this.falloffAngle = falloffAngle;
                    this.blurScale = blurScale;
                    this.unusedLightSourceData = unusedLightSourceData.ToArray();
                    if (this.unusedLightSourceData.Length != 19)
                        throw new ArgumentException("Must provide 19 values", "unusedLightSourceData");
                }
                public SpotLightSourceType(EventHandler handler, Stream s) : base(handler) { Parse(s); }
                #endregion

                #region Data I/O
                void Parse(Stream s)
                {
                    BinaryReader r = new BinaryReader(s);
                    this.location = new Vertex(handler, s);
                    this.falloffAngle = r.ReadSingle();
                    this.blurScale = r.ReadSingle();
                    this.unusedLightSourceData = new Single[19];
                    for (int i = 0; i < unusedLightSourceData.Length; unusedLightSourceData[i++] = r.ReadSingle()) { }
                }

                internal override void UnParse(Stream s)
                {
                    BinaryWriter w = new BinaryWriter(s);
                    location.UnParse(s);
                    w.Write(falloffAngle);
                    w.Write(blurScale);
                    unusedLightSourceData.ToList().ForEach(item => w.Write(item));
                }
                #endregion

                #region AHandlerElement
                public override List<string> ContentFields { get { return GetContentFields(GetType()); } }
                #endregion

                #region Content Fields
                [ElementPriority(1)]
                public Vertex At { get { return location; } set { if (!location.Equals(value)) { location = new Vertex(handler, value); OnElementChanged(); } } }
                [ElementPriority(2)]
                public Single FalloffAngle { get { return falloffAngle; } set { if (!falloffAngle.Equals(value)) { falloffAngle = value; OnElementChanged(); } } }
                [ElementPriority(3)]
                public Single BlurScale { get { return blurScale; } set { if (!blurScale.Equals(value)) { blurScale = value; OnElementChanged(); } } }
                [ElementPriority(4)]
                public float[] UnusedLightSourceData
                {
                    get { return (float[])unusedLightSourceData.Clone(); ; }
                    set
                    {
                        if (value.Length != this.unusedLightSourceData.Length) throw new ArgumentLengthException("UnusedLightSourceData", this.unusedLightSourceData.Length);
                        if (!unusedLightSourceData.Equals(value)) { unusedLightSourceData = value == null ? null : (float[])value.Clone(); this.OnElementChanged(); }
                    }
                }
                #endregion

                public string Value { get { return ValueBuilder; } }
            }

            public class LampShadeLightSourceType : AbstractLightSourceType
            {
                #region Attributes
                Vertex location;// 24 - 3
                Single falloffAngle;// 21 - 1
                Single shadeLightRigMultiplier;// 20 - 1
                Single bottomAngle;// 19 - 1
                RGB shadeColor;// 18 - 3
                Single[] unusedLightSourceData;// 15
                #endregion

                #region Constructors
                public LampShadeLightSourceType(EventHandler handler, GeneralLightSourceType basis) : this(handler, basis.LightSourceData) { }
                public LampShadeLightSourceType(EventHandler handler, IEnumerable<float> lightSourceData)
                    : this(handler
                    , new Vertex(handler, lightSourceData.ElementAt(0), lightSourceData.ElementAt(1), lightSourceData.ElementAt(2))
                    , lightSourceData.ElementAt(3)
                    , lightSourceData.ElementAt(4)
                    , lightSourceData.ElementAt(5)
                    , new RGB(handler, lightSourceData.ElementAt(6), lightSourceData.ElementAt(7), lightSourceData.ElementAt(8))
                    , lightSourceData.Skip(9)
                    ) { }
                public LampShadeLightSourceType(EventHandler handler)
                    : this(handler
                    , new Vertex(handler, 0f, 0f, 0f)
                    , 0f
                    , 0f
                    , 0f
                    , new RGB(handler, 0f, 0f, 0f)
                    , new float[] { 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, }
                    ) { }
                public LampShadeLightSourceType(EventHandler handler, LampShadeLightSourceType basis)
                    : this(handler
                    , basis.location
                    , basis.falloffAngle
                    , basis.shadeLightRigMultiplier
                    , basis.bottomAngle
                    , basis.shadeColor
                    , basis.unusedLightSourceData
                    ) { }
                public LampShadeLightSourceType(EventHandler handler
                    , Vertex location
                    , float falloffAngle
                    , float shadeLightRigMultiplier
                    , float bottomAngle
                    , RGB shadeColor
                    , IEnumerable<float> unusedLightSourceData)
                    : base(handler)
                {
                    this.location = new Vertex(handler, location);
                    this.falloffAngle = falloffAngle;
                    this.shadeLightRigMultiplier = shadeLightRigMultiplier;
                    this.bottomAngle = bottomAngle;
                    this.shadeColor = new RGB(handler, shadeColor);
                    this.unusedLightSourceData = unusedLightSourceData.ToArray();
                    if (this.unusedLightSourceData.Length != 15)
                        throw new ArgumentException("Must provide 15 values", "unusedLightSourceData");
                }
                public LampShadeLightSourceType(EventHandler handler, Stream s) : base(handler) { Parse(s); }
                #endregion

                #region Data I/O
                void Parse(Stream s)
                {
                    BinaryReader r = new BinaryReader(s);
                    this.location = new Vertex(handler, s);
                    this.falloffAngle = r.ReadSingle();
                    this.shadeLightRigMultiplier = r.ReadSingle();
                    this.bottomAngle = r.ReadSingle();
                    this.shadeColor = new RGB(handler, s);
                    this.unusedLightSourceData = new Single[15];
                    for (int i = 0; i < unusedLightSourceData.Length; unusedLightSourceData[i++] = r.ReadSingle()) { }
                }

                internal override void UnParse(Stream s)
                {
                    BinaryWriter w = new BinaryWriter(s);
                    location.UnParse(s);
                    w.Write(falloffAngle);
                    w.Write(shadeLightRigMultiplier);
                    w.Write(bottomAngle);
                    shadeColor.UnParse(s);
                    unusedLightSourceData.ToList().ForEach(item => w.Write(item));
                }
                #endregion

                #region AHandlerElement
                public override List<string> ContentFields { get { return GetContentFields(GetType()); } }
                #endregion

                #region Content Fields
                [ElementPriority(1)]
                public Vertex At { get { return location; } set { if (!location.Equals(value)) { location = new Vertex(handler, value); OnElementChanged(); } } }
                [ElementPriority(2)]
                public Single FalloffAngle { get { return falloffAngle; } set { if (!falloffAngle.Equals(value)) { falloffAngle = value; OnElementChanged(); } } }
                [ElementPriority(3)]
                public Single ShadeLightRigMultiplier { get { return shadeLightRigMultiplier; } set { if (!shadeLightRigMultiplier.Equals(value)) { shadeLightRigMultiplier = value; OnElementChanged(); } } }
                [ElementPriority(4)]
                public Single BottomAngle { get { return bottomAngle; } set { if (!bottomAngle.Equals(value)) { bottomAngle = value; OnElementChanged(); } } }
                [ElementPriority(5)]
                public RGB ShadeColor { get { return shadeColor; } set { if (!shadeColor.Equals(value)) { shadeColor = new RGB(handler, value); OnElementChanged(); } } }
                [ElementPriority(6)]
                public float[] UnusedLightSourceData
                {
                    get { return (float[])unusedLightSourceData.Clone(); ; }
                    set
                    {
                        if (value.Length != this.unusedLightSourceData.Length) throw new ArgumentLengthException("UnusedLightSourceData", this.unusedLightSourceData.Length);
                        if (!unusedLightSourceData.Equals(value)) { unusedLightSourceData = value == null ? null : (float[])value.Clone(); this.OnElementChanged(); }
                    }
                }
                #endregion

                public string Value { get { return ValueBuilder; } }
            }

            public class TubeLightSourceType : AbstractLightSourceType
            {
                #region Attributes
                Vertex location;// 24 - 3
                Single tubeLength;// 21 - 1
                Single blurScale;// 20 - 1
                Single[] unusedLightSourceData;// 19
                #endregion

                #region Constructors
                public TubeLightSourceType(EventHandler handler, GeneralLightSourceType basis) : this(handler, basis.LightSourceData) { }
                public TubeLightSourceType(EventHandler handler, IEnumerable<float> lightSourceData)
                    : this(handler
                    , new Vertex(handler, lightSourceData.ElementAt(0), lightSourceData.ElementAt(1), lightSourceData.ElementAt(2))
                    , lightSourceData.ElementAt(3)
                    , lightSourceData.ElementAt(4)
                    , lightSourceData.Skip(5)
                    ) { }
                public TubeLightSourceType(EventHandler handler)
                    : this(handler
                    , new Vertex(handler, 0f, 0f, 0f)
                    , 0f
                    , 0f
                    , new float[] { 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, }
                    ) { }
                public TubeLightSourceType(EventHandler handler, TubeLightSourceType basis)
                    : this(handler
                    , basis.location
                    , basis.tubeLength
                    , basis.blurScale
                    , basis.unusedLightSourceData
                    ) { }
                public TubeLightSourceType(EventHandler handler
                    , Vertex location
                    , float tubeLength
                    , float blurScale
                    , IEnumerable<float> unusedLightSourceData)
                    : base(handler)
                {
                    this.location = new Vertex(handler, location);
                    this.tubeLength = tubeLength;
                    this.blurScale = blurScale;
                    this.unusedLightSourceData = unusedLightSourceData.ToArray();
                    if (this.unusedLightSourceData.Length != 19)
                        throw new ArgumentException("Must provide 19 values", "unusedLightSourceData");
                }
                public TubeLightSourceType(EventHandler handler, Stream s) : base(handler) { Parse(s); }
                #endregion

                #region Data I/O
                void Parse(Stream s)
                {
                    BinaryReader r = new BinaryReader(s);
                    this.location = new Vertex(handler, s);
                    this.tubeLength = r.ReadSingle();
                    this.blurScale = r.ReadSingle();
                    this.unusedLightSourceData = new Single[19];
                    for (int i = 0; i < unusedLightSourceData.Length; unusedLightSourceData[i++] = r.ReadSingle()) { }
                }

                internal override void UnParse(Stream s)
                {
                    BinaryWriter w = new BinaryWriter(s);
                    location.UnParse(s);
                    w.Write(tubeLength);
                    w.Write(blurScale);
                    unusedLightSourceData.ToList().ForEach(item => w.Write(item));
                }
                #endregion

                #region AHandlerElement
                public override List<string> ContentFields { get { return GetContentFields(GetType()); } }
                #endregion

                #region Content Fields
                [ElementPriority(1)]
                public Vertex At { get { return location; } set { if (!location.Equals(value)) { location = new Vertex(handler, value); OnElementChanged(); } } }
                [ElementPriority(2)]
                public Single TubeLength { get { return tubeLength; } set { if (!tubeLength.Equals(value)) { tubeLength = value; OnElementChanged(); } } }
                [ElementPriority(3)]
                public Single BlurScale { get { return blurScale; } set { if (!blurScale.Equals(value)) { blurScale = value; OnElementChanged(); } } }
                [ElementPriority(4)]
                public float[] UnusedLightSourceData
                {
                    get { return (float[])unusedLightSourceData.Clone(); ; }
                    set
                    {
                        if (value.Length != this.unusedLightSourceData.Length) throw new ArgumentLengthException("UnusedLightSourceData", this.unusedLightSourceData.Length);
                        if (!unusedLightSourceData.Equals(value)) { unusedLightSourceData = value == null ? null : (float[])value.Clone(); this.OnElementChanged(); }
                    }
                }
                #endregion

                public string Value { get { return ValueBuilder; } }
            }

            public class SquareWindowLightSourceType : AbstractLightSourceType
            {
                #region Attributes
                Vertex location;// 24 - 3
                Vertex right;// 21 - 3
                Single width;// 18 - 1
                Single height;// 17 - 1
                Single falloffAngle;// 16 - 1
                Single windowTopBottomAngle;// 15 - 1
                Single[] unusedLightSourceData;// 14
                #endregion

                #region Constructors
                public SquareWindowLightSourceType(EventHandler handler, GeneralLightSourceType basis) : this(handler, basis.LightSourceData) { }
                public SquareWindowLightSourceType(EventHandler handler, IEnumerable<float> lightSourceData)
                    : this(handler
                    , new Vertex(handler, lightSourceData.ElementAt(0), lightSourceData.ElementAt(1), lightSourceData.ElementAt(2))
                    , new Vertex(handler, lightSourceData.ElementAt(3), lightSourceData.ElementAt(4), lightSourceData.ElementAt(5))
                    , lightSourceData.ElementAt(6)
                    , lightSourceData.ElementAt(7)
                    , lightSourceData.ElementAt(8)
                    , lightSourceData.ElementAt(9)
                    , lightSourceData.Skip(10)
                    ) { }
                public SquareWindowLightSourceType(EventHandler handler)
                    : this(handler
                    , new Vertex(handler, 0f, 0f, 0f)
                    , new Vertex(handler, 0f, 0f, 0f)
                    , 0f
                    , 0f
                    , 0f
                    , 0f
                    , new float[] { 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, }
                    ) { }
                public SquareWindowLightSourceType(EventHandler handler, SquareWindowLightSourceType basis)
                    : this(handler
                    , basis.location
                    , basis.right
                    , basis.width
                    , basis.height
                    , basis.falloffAngle
                    , basis.windowTopBottomAngle
                    , basis.unusedLightSourceData
                    ) { }
                public SquareWindowLightSourceType(EventHandler handler
                    , Vertex location
                    , Vertex right
                    , float width
                    , float height
                    , float falloffAngle
                    , float windowTopBottomAngle
                    , IEnumerable<float> unusedLightSourceData)
                    : base(handler)
                {
                    this.location = new Vertex(handler, location);
                    this.right = new Vertex(handler, right);
                    this.width = width;
                    this.height = height;
                    this.falloffAngle = falloffAngle;
                    this.windowTopBottomAngle = windowTopBottomAngle;
                    this.unusedLightSourceData = unusedLightSourceData.ToArray();
                    if (this.unusedLightSourceData.Length != 14)
                        throw new ArgumentException("Must provide 14 values", "unusedLightSourceData");
                }
                public SquareWindowLightSourceType(EventHandler handler, Stream s) : base(handler) { Parse(s); }
                #endregion

                #region Data I/O
                void Parse(Stream s)
                {
                    BinaryReader r = new BinaryReader(s);
                    this.location = new Vertex(handler, s);
                    this.right = new Vertex(handler, s);
                    this.width = r.ReadSingle();
                    this.height = r.ReadSingle();
                    this.falloffAngle = r.ReadSingle();
                    this.windowTopBottomAngle = r.ReadSingle();
                    this.unusedLightSourceData = new Single[14];
                    for (int i = 0; i < unusedLightSourceData.Length; unusedLightSourceData[i++] = r.ReadSingle()) { }
                }

                internal override void UnParse(Stream s)
                {
                    BinaryWriter w = new BinaryWriter(s);
                    location.UnParse(s);
                    right.UnParse(s);
                    w.Write(width);
                    w.Write(height);
                    w.Write(falloffAngle);
                    w.Write(windowTopBottomAngle);
                    unusedLightSourceData.ToList().ForEach(item => w.Write(item));
                }
                #endregion

                #region AHandlerElement
                public override List<string> ContentFields { get { return GetContentFields(GetType()); } }
                #endregion

                #region Content Fields
                [ElementPriority(1)]
                public Vertex At { get { return location; } set { if (!location.Equals(value)) { location = new Vertex(handler, value); OnElementChanged(); } } }
                [ElementPriority(2)]
                public Vertex Right { get { return right; } set { if (!right.Equals(value)) { right = new Vertex(handler, value); OnElementChanged(); } } }
                [ElementPriority(3)]
                public Single Width { get { return width; } set { if (!width.Equals(value)) { width = value; OnElementChanged(); } } }
                [ElementPriority(4)]
                public Single Height { get { return height; } set { if (!height.Equals(value)) { height = value; OnElementChanged(); } } }
                [ElementPriority(5)]
                public Single FalloffAngle { get { return falloffAngle; } set { if (!falloffAngle.Equals(value)) { falloffAngle = value; OnElementChanged(); } } }
                [ElementPriority(6)]
                public Single WindowTopBottomAngle { get { return windowTopBottomAngle; } set { if (!windowTopBottomAngle.Equals(value)) { windowTopBottomAngle = value; OnElementChanged(); } } }
                [ElementPriority(7)]
                public float[] UnusedLightSourceData
                {
                    get { return (float[])unusedLightSourceData.Clone(); ; }
                    set
                    {
                        if (value.Length != this.unusedLightSourceData.Length) throw new ArgumentLengthException("UnusedLightSourceData", this.unusedLightSourceData.Length);
                        if (!unusedLightSourceData.Equals(value)) { unusedLightSourceData = value == null ? null : (float[])value.Clone(); this.OnElementChanged(); }
                    }
                }
                #endregion

                public string Value { get { return ValueBuilder; } }
            }

            public class CircularWindowLightSourceType : AbstractLightSourceType
            {
                #region Attributes
                Vertex location;// 24 - 3
                Vertex right;// 21 - 3
                Single radius;// 18 - 1
                Single[] unusedLightSourceData;// 17
                #endregion

                #region Constructors
                public CircularWindowLightSourceType(EventHandler handler, GeneralLightSourceType basis) : this(handler, basis.LightSourceData) { }
                public CircularWindowLightSourceType(EventHandler handler, IEnumerable<float> lightSourceData)
                    : this(handler
                    , new Vertex(handler, lightSourceData.ElementAt(0), lightSourceData.ElementAt(1), lightSourceData.ElementAt(2))
                    , new Vertex(handler, lightSourceData.ElementAt(3), lightSourceData.ElementAt(4), lightSourceData.ElementAt(5))
                    , lightSourceData.ElementAt(6)
                    , lightSourceData.Skip(7)
                    ) { }
                public CircularWindowLightSourceType(EventHandler handler)
                    : this(handler
                    , new Vertex(handler, 0f, 0f, 0f)
                    , new Vertex(handler, 0f, 0f, 0f)
                    , 0f
                    , new float[] { 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, }
                    ) { }
                public CircularWindowLightSourceType(EventHandler handler, CircularWindowLightSourceType basis)
                    : this(handler
                    , basis.location
                    , basis.right
                    , basis.radius
                    , basis.unusedLightSourceData
                    ) { }
                public CircularWindowLightSourceType(EventHandler handler
                    , Vertex location
                    , Vertex right
                    , float radius
                    , IEnumerable<float> unusedLightSourceData)
                    : base(handler)
                {
                    this.location = new Vertex(handler, location);
                    this.right = new Vertex(handler, right);
                    this.radius = radius;
                    this.unusedLightSourceData = unusedLightSourceData.ToArray();
                    if (this.unusedLightSourceData.Length != 17)
                        throw new ArgumentException("Must provide 17 values", "unusedLightSourceData");
                }
                public CircularWindowLightSourceType(EventHandler handler, Stream s) : base(handler) { Parse(s); }
                #endregion

                #region Data I/O
                void Parse(Stream s)
                {
                    BinaryReader r = new BinaryReader(s);
                    this.location = new Vertex(handler, s);
                    this.right = new Vertex(handler, s);
                    this.radius = r.ReadSingle();
                    this.unusedLightSourceData = new Single[17];
                    for (int i = 0; i < unusedLightSourceData.Length; unusedLightSourceData[i++] = r.ReadSingle()) { }
                }

                internal override void UnParse(Stream s)
                {
                    BinaryWriter w = new BinaryWriter(s);
                    location.UnParse(s);
                    right.UnParse(s);
                    w.Write(radius);
                    unusedLightSourceData.ToList().ForEach(item => w.Write(item));
                }
                #endregion

                #region AHandlerElement
                public override List<string> ContentFields { get { return GetContentFields(GetType()); } }
                #endregion

                #region Content Fields
                [ElementPriority(1)]
                public Vertex At { get { return location; } set { if (!location.Equals(value)) { location = new Vertex(handler, value); OnElementChanged(); } } }
                [ElementPriority(2)]
                public Vertex Right { get { return right; } set { if (!right.Equals(value)) { right = new Vertex(handler, value); OnElementChanged(); } } }
                [ElementPriority(3)]
                public Single Radius { get { return radius; } set { if (!radius.Equals(value)) { radius = value; OnElementChanged(); } } }
                [ElementPriority(4)]
                public float[] UnusedLightSourceData
                {
                    get { return (float[])unusedLightSourceData.Clone(); ; }
                    set
                    {
                        if (value.Length != this.unusedLightSourceData.Length) throw new ArgumentLengthException("UnusedLightSourceData", this.unusedLightSourceData.Length);
                        if (!unusedLightSourceData.Equals(value)) { unusedLightSourceData = value == null ? null : (float[])value.Clone(); this.OnElementChanged(); }
                    }
                }
                #endregion

                public string Value { get { return ValueBuilder; } }
            }
            #endregion

            #region Content Fields
            [ElementPriority(1)]
            public LightSourceType LightType
            {
                get { return lightSourceType; }
                set
                {
                    if (lightSourceType != value)
                    {
                        lightSourceType = value;
                        MemoryStream s = new MemoryStream();
                        lightSourceData.UnParse(s);
                        s.Flush();
                        s.Seek(0, 0);
                        lightSourceData = LightSourceTypeFactory.create(handler, lightSourceType, s);
                        s.Close();
                        OnElementChanged();
                    }
                }
            }
            [ElementPriority(2)]
            public Vertex Transform { get { return transform; } set { if (!transform.Equals(value)) { transform = new Vertex(handler, value); OnElementChanged(); } } }
            [ElementPriority(3)]
            public RGB Color { get { return color; } set { if (!color.Equals(value)) { color = new RGB(handler, value); OnElementChanged(); } } }
            [ElementPriority(4)]
            public float Intensity { get { return intensity; } set { if (intensity != value) { intensity = value; OnElementChanged(); } } }
            [ElementPriority(5)]
            public AbstractLightSourceType LightSourceData
            {
                get { return lightSourceData; }
                set
                {
                    if (value.GetType() != lightSourceData.GetType())
                        throw new InvalidCastException();

                    if (!lightSourceData.Equals(value))
                    {
                        MemoryStream s = new MemoryStream();
                        value.UnParse(s);
                        lightSourceData = LightSourceTypeFactory.create(handler, lightSourceType, s);
                        s.Close();
                        OnElementChanged();
                    }
                }
            }

            public string Value { get { return ValueBuilder; } }
            #endregion
        }

        public class LightSourceList : DependentList<LightSource>
        {
            int count;

            #region Constructors
            public LightSourceList(EventHandler handler) : base(handler, Byte.MaxValue) { }
            public LightSourceList(EventHandler handler, int count, Stream s) : base(null, Byte.MaxValue) { this.count = count; elementHandler = handler; Parse(s); this.handler = handler; }
            public LightSourceList(EventHandler handler, IEnumerable<LightSource> llp) : base(handler, llp, Byte.MaxValue) { }
            #endregion

            #region Data I/O
            protected override int ReadCount(Stream s) { return count; }
            protected override void WriteCount(Stream s, int count) { }

            protected override LightSource CreateElement(Stream s) { return new LightSource(elementHandler, s); }

            protected override void WriteElement(Stream s, LightSource element) { element.UnParse(s); }
            #endregion
        }

        public class Occluder : AHandlerElement, IEquatable<Occluder>
        {
            #region Attributes
            OccluderType occluderType = 0;
            Vertex origin;
            Vertex normal;
            Vertex xAxis;
            Vertex yAxis;
            float pairOffset;
            #endregion

            #region Constructors
            public Occluder(EventHandler handler)
                : this(handler, Occluder.OccluderType.Disc,
                    new Vertex(null, 0f, 0f, 0f), new Vertex(null, 0f, 0f, 0f), new Vertex(null, 0f, 0f, 0f), new Vertex(null, 0f, 0f, 0f), 0f) { }
            public Occluder(EventHandler handler, Stream s) : base(handler) { Parse(s); }
            public Occluder(EventHandler handler, Occluder basis)
                : this(handler, basis.occluderType, basis.origin, basis.normal, basis.xAxis, basis.yAxis, basis.pairOffset) { }
            public Occluder(EventHandler handler, OccluderType occluderType, Vertex origin, Vertex normal, Vertex xAxis,
                Vertex yAxis, float pairOffset)
                : base(handler)
            {
                this.occluderType = occluderType;
                this.origin = new Vertex(handler, origin);
                this.normal = new Vertex(handler, normal);
                this.xAxis = new Vertex(handler, xAxis);
                this.yAxis = new Vertex(handler, yAxis);
                this.pairOffset = pairOffset;
            }
            #endregion

            #region Data I/O
            void Parse(Stream s)
            {
                BinaryReader r = new BinaryReader(s);
                occluderType = (OccluderType)r.ReadUInt32();
                origin = new Vertex(handler, s);
                normal = new Vertex(handler, s);
                xAxis = new Vertex(handler, s);
                yAxis = new Vertex(handler, s);
                pairOffset = r.ReadSingle();
            }

            internal void UnParse(Stream s)
            {
                BinaryWriter w = new BinaryWriter(s);
                w.Write((uint)occluderType);
                origin.UnParse(s);
                normal.UnParse(s);
                xAxis.UnParse(s);
                yAxis.UnParse(s);
                w.Write(pairOffset);
            }
            #endregion

            #region AHandlerElement Members
            /// <summary>
            /// The list of available field names on this API object
            /// </summary>
            public override List<string> ContentFields { get { return GetContentFields(GetType()); } }
            #endregion

            #region IEquatable<Occluder> Members

            public bool Equals(Occluder other)
            {
                return occluderType.Equals(other.occluderType)
                    && origin.Equals(other.origin)
                    && normal.Equals(other.normal)
                    && xAxis.Equals(other.xAxis)
                    && yAxis.Equals(other.yAxis)
                    && pairOffset.Equals(other.pairOffset)
                    ;
            }

            public override bool Equals(object obj)
            {
                return obj as Occluder != null ? this.Equals(obj as Occluder) : false;
            }

            public override int GetHashCode()
            {
                return occluderType.GetHashCode()
                    ^ origin.GetHashCode()
                    ^ normal.GetHashCode()
                    ^ xAxis.GetHashCode()
                    ^ yAxis.GetHashCode()
                    ^ pairOffset.GetHashCode()
                    ;
            }

            #endregion

            #region Sub-types
            public enum OccluderType : uint
            {
                Disc = 0x00,
                Rectangle = 0x01,
            }
            #endregion

            #region Content Fields
            [ElementPriority(0)]
            public OccluderType Occluder_Type { get { return occluderType; } set { if (occluderType != value) { occluderType = value; OnElementChanged(); } } }
            [ElementPriority(1)]
            public Vertex Origin { get { return origin; } set { if (!origin.Equals(value)) { origin = new Vertex(handler, value); OnElementChanged(); } } }
            [ElementPriority(2)]
            public Vertex Normal { get { return normal; } set { if (!normal.Equals(value)) { normal = new Vertex(handler, value); OnElementChanged(); } } }
            [ElementPriority(3)]
            public Vertex XAxis { get { return xAxis; } set { if (!xAxis.Equals(value)) { xAxis = new Vertex(handler, value); OnElementChanged(); } } }
            [ElementPriority(4)]
            public Vertex YAxis { get { return yAxis; } set { if (!yAxis.Equals(value)) { yAxis = new Vertex(handler, value); OnElementChanged(); } } }
            [ElementPriority(5)]
            public float PairOffset { get { return pairOffset; } set { pairOffset = value; OnElementChanged(); } }

            public string Value { get { return ValueBuilder; } }
            #endregion
        }

        public class OccluderList : DependentList<Occluder>
        {
            int count;

            #region Constructors
            public OccluderList(EventHandler handler) : base(handler, Byte.MaxValue) { }
            public OccluderList(EventHandler handler, int count, Stream s) : base(null, Byte.MaxValue) { this.count = count; elementHandler = handler; Parse(s); this.handler = handler; }
            public OccluderList(EventHandler handler, IEnumerable<Occluder> lss) : base(handler, lss, Byte.MaxValue) { }
            #endregion

            #region Data I/O
            protected override int ReadCount(Stream s) { return count; }
            protected override void WriteCount(Stream s, int count) { }

            protected override Occluder CreateElement(Stream s) { return new Occluder(elementHandler, s); }

            protected override void WriteElement(Stream s, Occluder element) { element.UnParse(s); }
            #endregion
        }
        #endregion

        #region Content Fields
        [ElementPriority(11)]
        public uint Version { get { return version; } set { if (version != value) { version = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(12)]
        public uint Unknown1 { get { return unknown1; } set { if (unknown1 != value) { unknown1 = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(13)]
        public ushort Unknown2 { get { return unknown2; } set { if (unknown2 != value) { unknown2 = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(14)]
        public LightSourceList LightSources { get { return lightSources; } set { if (lightSources != value) { lightSources = value == null ? null : new LightSourceList(handler, value); OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(15)]
        public OccluderList Occluders { get { return occluders; } set { if (occluders != value) { occluders = value == null ? null : new OccluderList(handler, value); OnRCOLChanged(this, EventArgs.Empty); } } }

        public string Value { get { return ValueBuilder; } }
        #endregion
    }
}