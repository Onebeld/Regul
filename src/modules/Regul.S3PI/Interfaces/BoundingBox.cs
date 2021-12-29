/***************************************************************************
 *  Copyright (C) 2009 by Peter L Jones                                    *
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

namespace Regul.S3PI.Interfaces
{
    /// <summary>
    /// Defines a vertex - a point in 3d space defined by three coordinates.
    /// </summary>
    public class Vertex : AHandlerElement, IEquatable<Vertex>
    {
        #region Attributes
        float _x = 0f;
        float _y = 0f;
        float _z = 0f;
        #endregion

        #region Constructors
        /// <summary>
        /// Create a vertex at { 0, 0, 0 }.
        /// </summary>
        /// <param name="handler">The <see cref="EventHandler"/> delegate to invoke if the <see cref="AHandlerElement"/> changes.</param>
        public Vertex(EventHandler handler) : base(handler) { }
        /// <summary>
        /// Create a vertex from a <see cref="Stream"/>.
        /// </summary>
        /// <param name="handler">The <see cref="EventHandler"/> delegate to invoke if the <see cref="AHandlerElement"/> changes.</param>
        /// <param name="s"><see cref="Stream"/> containing coordinates.</param>
        public Vertex(EventHandler handler, Stream s) : base(handler) { Parse(s); }
        /// <summary>
        /// Create a vertex from a given value.
        /// </summary>
        /// <param name="handler">The <see cref="EventHandler"/> delegate to invoke if the <see cref="AHandlerElement"/> changes.</param>
        /// <param name="basis"><see cref="Vertex"/> to copy.</param>
        public Vertex(EventHandler handler, Vertex basis)
            : this(handler, basis._x, basis._y, basis._z) { }
        /// <summary>
        /// Create a vertex at { x, y, z }.
        /// </summary>
        /// <param name="handler">The <see cref="EventHandler"/> delegate to invoke if the <see cref="AHandlerElement"/> changes.</param>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <param name="z">Z coordinate.</param>
        public Vertex(EventHandler handler, float x, float y, float z)
            : base(handler)
        {
            _x = x;
            _y = y;
            _z = z;
        }
        #endregion

        #region Data I/O
        void Parse(Stream s)
        {
            BinaryReader r = new BinaryReader(s);
            _x = r.ReadSingle();
            _y = r.ReadSingle();
            _z = r.ReadSingle();
        }

        /// <summary>
        /// Write the vertex to the given <see cref="Stream"/>.
        /// </summary>
        /// <param name="s"><see cref="Stream"/> to contain coordinates.</param>
        public void UnParse(Stream s)
        {
            BinaryWriter w = new BinaryWriter(s);
            w.Write(_x);
            w.Write(_y);
            w.Write(_z);
        }
        #endregion

        #region AHandlerElement Members
        /// <summary>
        /// The list of available field names on this API object.
        /// </summary>
        public override List<string> ContentFields => GetContentFields(GetType());

        // /// <summary>
        // /// Get a copy of the <see cref="Vertex"/> but with a new change <see cref="EventHandler"/>.
        // /// </summary>
        // /// <param name="handler">The replacement <see cref="EventHandler"/> delegate.</param>
        // /// <returns>Return a copy of the <see cref="Vertex"/> but with a new change <see cref="EventHandler"/>.</returns>
        // public override AHandlerElement Clone(EventHandler handler) { return new Vertex(requestedApiVersion, handler, this); }
        #endregion

        #region IEquatable<BoxPoint> Members

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>true if the current object is equal to the other parameter; otherwise, false.</returns>
        public bool Equals(Vertex other)
        {
            return _x == other._x && _y == other._y && _z == other._z;
        }

        /// <summary>
        /// Determines whether the specified <see cref="object"/> is equal to the current <see cref="Vertex"/>.
        /// </summary>
        /// <param name="obj">The <see cref="object"/> to compare with the current <see cref="Vertex"/>.</param>
        /// <returns>true if the specified <see cref="object"/> is equal to the current <see cref="Vertex"/>; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return obj as Vertex != null ? Equals(obj as Vertex) : false;
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override int GetHashCode()
        {
            return _x.GetHashCode() ^ _y.GetHashCode() ^ _z.GetHashCode();
        }

        #endregion

        #region Content Fields
        /// <summary>
        /// X coordinate
        /// </summary>
        [ElementPriority(1)]
        public float X { get => _x;
            set { if (_x != value) { _x = value; OnElementChanged(); } } }
        /// <summary>
        /// Y coordinate
        /// </summary>
        [ElementPriority(2)]
        public float Y { get => _y;
            set { if (_y != value) { _y = value; OnElementChanged(); } } }
        /// <summary>
        /// Z coordinate
        /// </summary>
        [ElementPriority(3)]
        public float Z { get => _z;
            set { if (_z != value) { _z = value; OnElementChanged(); } } }

        /// <summary>
        /// A displayable representation of the object
        /// </summary>
        public string Value => "{ " + $"{_x:F4}; {_y:F4}; {_z:F4}" + " }";

        #endregion
    }

    /// <summary>
    /// Defines a bounding box - a imaginary box large enough to completely contain an object
    /// - by its minimum and maximum vertices.
    /// </summary>
    public class BoundingBox : AHandlerElement, IEquatable<BoundingBox>
    {
        #region Attributes
        Vertex _min;
        Vertex _max;
        #endregion

        #region Constructors
        /// <summary>
        /// Create an zero-sized bounding box.
        /// </summary>
        /// <param name="handler">The <see cref="EventHandler"/> delegate to invoke if the <see cref="AHandlerElement"/> changes.</param>
        public BoundingBox(EventHandler handler) : base(handler)
        {
            _min = new Vertex(handler);
            _max = new Vertex(handler);
        }
        /// <summary>
        /// Create a bounding box from a <see cref="Stream"/>.
        /// </summary>
        /// <param name="handler">The <see cref="EventHandler"/> delegate to invoke if the <see cref="AHandlerElement"/> changes.</param>
        /// <param name="s"><see cref="Stream"/> containing vertices.</param>
        public BoundingBox(EventHandler handler, Stream s) : base(handler) { Parse(s); }
        /// <summary>
        /// Create a bounding box from a given value.
        /// </summary>
        /// <param name="handler">The <see cref="EventHandler"/> delegate to invoke if the <see cref="AHandlerElement"/> changes.</param>
        /// <param name="basis"><see cref="BoundingBox"/> to copy.</param>
        public BoundingBox(EventHandler handler, BoundingBox basis)
            : this(handler, basis._min, basis._max) { }
        /// <summary>
        /// Create a bounding box with the specified minimum and maximum vertices.
        /// </summary>
        /// <param name="handler">The <see cref="EventHandler"/> delegate to invoke if the <see cref="AHandlerElement"/> changes.</param>
        /// <param name="min">Minimum vertex.</param>
        /// <param name="max">Maximum vertex.</param>
        public BoundingBox(EventHandler handler, Vertex min, Vertex max)
            : base(handler)
        {
            _min = new Vertex(handler, min);
            _max = new Vertex(handler, max);
        }
        #endregion

        #region Data I/O
        void Parse(Stream s)
        {
            _min = new Vertex(handler, s);
            _max = new Vertex(handler, s);
        }

        /// <summary>
        /// Write the bounding box to the given <see cref="Stream"/>.
        /// </summary>
        /// <param name="s"><see cref="Stream"/> to contain vertices.</param>
        public void UnParse(Stream s)
        {
            if (_min == null) _min = new Vertex(handler);
            _min.UnParse(s);

            if (_max == null) _max = new Vertex(handler);
            _max.UnParse(s);
        }
        #endregion

        #region AHandlerElement Members

        /// <summary>
        /// The list of available field names on this API object.
        /// </summary>
        public override List<string> ContentFields => GetContentFields(GetType());

        // /// <summary>
        // /// Get a copy of the <see cref="BoundingBox"/> but with a new change <see cref="EventHandler"/>.
        // /// </summary>
        // /// <param name="handler">The replacement <see cref="EventHandler"/> delegate.</param>
        // /// <returns>Return a copy of the <see cref="BoundingBox"/> but with a new change <see cref="EventHandler"/>.</returns>
        // public override AHandlerElement Clone(EventHandler handler) { return new BoundingBox(requestedApiVersion, handler, this); }
        #endregion

        #region IEquatable<BoundingBox> Members

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>true if the current object is equal to the other parameter; otherwise, false.</returns>
        public bool Equals(BoundingBox other)
        {
            return _min.Equals(other._min) && _max.Equals(other._max);
        }

        /// <summary>
        /// Determines whether the specified <see cref="object"/> is equal to the current <see cref="BoundingBox"/>.
        /// </summary>
        /// <param name="obj">The <see cref="object"/> to compare with the current <see cref="BoundingBox"/>.</param>
        /// <returns>true if the specified <see cref="object"/> is equal to the current <see cref="BoundingBox"/>; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return obj is BoundingBox box && Equals(box);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override int GetHashCode()
        {
            return _min.GetHashCode() ^ _max.GetHashCode();
        }

        #endregion

        #region Content Fields
        /// <summary>
        /// Minimum vertex
        /// </summary>
        [ElementPriority(1)]
        public Vertex Min { get => _min;
            set { if (_min != value) { _min = new Vertex(handler, value); OnElementChanged(); } } }
        /// <summary>
        /// Maximum vertex
        /// </summary>
        [ElementPriority(2)]
        public Vertex Max { get => _max;
            set { if (_max != value) { _max = new Vertex(handler, value); OnElementChanged(); } } }

        /// <summary>
        /// A displayable representation of the object
        /// </summary>
        public string Value => $"[ Min: {_min.Value} | Max: {_max.Value} ]";

        #endregion
    }
}