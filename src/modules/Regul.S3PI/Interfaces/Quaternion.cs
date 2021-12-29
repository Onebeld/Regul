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
    /// Defines a quaternion - the quotient of two vectors.
    /// </summary>
    public class Quaternion : AHandlerElement, IEquatable<Quaternion>
    {
        #region Attributes
        float _a = 0f;
        float _b = 0f;
        float _c = 0f;
        float _d = 0f;
        #endregion

        #region Constructors
        /// <summary>
        /// Create a Quaternion { 0, 0, 0, 0 }.
        /// </summary>
        /// <param name="handler">The <see cref="EventHandler"/> delegate to invoke if the <see cref="AHandlerElement"/> changes.</param>
        public Quaternion(EventHandler handler) : base(handler) { }
        /// <summary>
        /// Create a Quaternion from a <see cref="Stream"/>.
        /// </summary>
        /// <param name="handler">The <see cref="EventHandler"/> delegate to invoke if the <see cref="AHandlerElement"/> changes.</param>
        /// <param name="s"><see cref="Stream"/> containing coordinates.</param>
        public Quaternion(EventHandler handler, Stream s) : base(handler) { Parse(s); }
        /// <summary>
        /// Create a Quaternion from a given value.
        /// </summary>
        /// <param name="handler">The <see cref="EventHandler"/> delegate to invoke if the <see cref="AHandlerElement"/> changes.</param>
        /// <param name="basis"><see cref="Quaternion"/> to copy.</param>
        public Quaternion(EventHandler handler, Quaternion basis)
            : this(handler, basis._a, basis._b, basis._c, basis._d) { }
        /// <summary>
        /// Create a Quaternion { a, b, c, d }.
        /// </summary>
        /// <param name="handler">The <see cref="EventHandler"/> delegate to invoke if the <see cref="AHandlerElement"/> changes.</param>
        /// <param name="a">Q 'a' value.</param>
        /// <param name="b">Q 'b' value.</param>
        /// <param name="c">Q 'c' value.</param>
        /// <param name="d">Q 'd' value.</param>
        public Quaternion(EventHandler handler, float a, float b, float c, float d)
            : base(handler)
        {
            _a = a;
            _b = b;
            _c = c;
            _d = d;
        }
        #endregion

        #region Data I/O
        void Parse(Stream s)
        {
            BinaryReader r = new BinaryReader(s);
            _a = r.ReadSingle();
            _b = r.ReadSingle();
            _c = r.ReadSingle();
            _d = r.ReadSingle();
        }

        /// <summary>
        /// Write the Quaternion to the given <see cref="Stream"/>.
        /// </summary>
        /// <param name="s"><see cref="Stream"/> to contain coordinates.</param>
        public void UnParse(Stream s)
        {
            BinaryWriter w = new BinaryWriter(s);
            w.Write(_a);
            w.Write(_b);
            w.Write(_c);
            w.Write(_d);
        }
        #endregion

        #region AHandlerElement Members
        /// <summary>
        /// The list of available field names on this API object.
        /// </summary>
        public override List<string> ContentFields => GetContentFields(GetType());

        // /// <summary>
        // /// Get a copy of the <see cref="Quaternion"/> but with a new change <see cref="EventHandler"/>.
        // /// </summary>
        // /// <param name="handler">The replacement <see cref="EventHandler"/> delegate.</param>
        // /// <returns>Return a copy of the <see cref="Quaternion"/> but with a new change <see cref="EventHandler"/>.</returns>
        // public override AHandlerElement Clone(EventHandler handler) { return new Quaternion(requestedApiVersion, handler, this); }
        #endregion

        #region IEquatable<Quaternion> Members

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>true if the current object is equal to the other parameter; otherwise, false.</returns>
        public bool Equals(Quaternion other)
        {
            return _a == other._a && _b == other._b && _c == other._c && _d == other._d;
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to the current <see cref="Quaternion"/>.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with the current <see cref="Quaternion"/>.</param>
        /// <returns>true if the specified <see cref="System.Object"/> is equal to the current <see cref="Quaternion"/>; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return obj as Quaternion != null ? Equals(obj as Quaternion) : false;
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override int GetHashCode()
        {
            return _a.GetHashCode() ^ _b.GetHashCode() ^ _c.GetHashCode() ^ _d.GetHashCode();
        }

        #endregion

        #region Content Fields
        /// <summary>
        /// 'a' value
        /// </summary>
        [ElementPriority(1)]
        public float A { get => _a;
            set { if (_a != value) { _a = value; OnElementChanged(); } } }
        /// <summary>
        /// 'b' value
        /// </summary>
        [ElementPriority(2)]
        public float B { get => _b;
            set { if (_b != value) { _b = value; OnElementChanged(); } } }
        /// <summary>
        /// 'c' value
        /// </summary>
        [ElementPriority(3)]
        public float C { get => _c;
            set { if (_c != value) { _c = value; OnElementChanged(); } } }
        /// <summary>
        /// 'd' value
        /// </summary>
        [ElementPriority(3)]
        public float D { get => _d;
            set { if (_d != value) { _d = value; OnElementChanged(); } } }

        /// <summary>
        /// A displayable representation of the object
        /// </summary>
        public string Value => "{ " + $"{_a:F4}; {_b:F4}; {_c:F4}; {_d:F4}" + " }";

        #endregion
    }
}