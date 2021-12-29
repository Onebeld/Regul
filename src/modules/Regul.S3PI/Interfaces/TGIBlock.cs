﻿/***************************************************************************
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
    /// An implementation of AResourceKey that supports storing the Type, Group and Instance in any order.
    /// </summary>
    /// <remarks>An explicit implementation of <see cref="IEquatable{TGIBlock}"/> is required by
    /// <see cref="CountedTGIBlockList"/> and <see cref="TGIBlockList"/>.</remarks>
    public class TGIBlock : AResourceKey, IEquatable<TGIBlock>
    {
        #region Attributes
        string _order = "TGI";
        #endregion

        #region Constructors
        /// <summary>
        /// Options for the order of the Type, Group and Instance elements of a TGIBlock
        /// </summary>
        public enum Order
        {
            /// <summary>
            /// Type, Group, Instance
            /// </summary>
            TGI,
            /// <summary>
            /// Type, Instance, Group
            /// </summary>
            TIG,
            /// <summary>
            /// Group, Type, Instance
            /// </summary>
            GTI,
            /// <summary>
            /// Group, Instance, Type
            /// </summary>
            GIT,
            /// <summary>
            /// Instance, Type, Group
            /// </summary>
            ITG,
            /// <summary>
            /// Instance, Group, Type
            /// </summary>
            IGT,
        }
        void Ok(string v) { Ok((Order)Enum.Parse(typeof(Order), v)); }
        void Ok(Order v) { if (!Enum.IsDefined(typeof(Order), v)) throw new ArgumentException("Invalid value " + v, "order"); }

        /// <summary>
        /// Initialize a new TGIBlock
        /// with the order and values
        /// based on <paramref name="basis"/>.
        /// </summary>
        /// <param name="handler">The <see cref="EventHandler"/> delegate to invoke if the <see cref="AHandlerElement"/> changes.</param>
        /// <param name="basis">The TGIBlock to use for the <see cref="Order"/> and <see cref="IResourceKey"/> values.</param>
        public TGIBlock(EventHandler handler, TGIBlock basis) : this(handler, basis._order, basis) { }

        /// <summary>
        /// Initialize a new TGIBlock
        /// with the default order ("TGI").
        /// </summary>
        /// <param name="handler">The <see cref="EventHandler"/> delegate to invoke if the <see cref="AHandlerElement"/> changes.</param>
        public TGIBlock(EventHandler handler) : base(handler, 0, 0, 0) { }
        /// <summary>
        /// Initialize a new TGIBlock
        /// with the specified order.
        /// </summary>
        /// <param name="handler">The <see cref="EventHandler"/> delegate to invoke if the <see cref="AHandlerElement"/> changes.</param>
        /// <param name="order">A <see cref="string"/> representing the <see cref="Order"/> to use to store the <see cref="IResourceKey"/> values.</param>
        public TGIBlock(EventHandler handler, string order) : this(handler) { Ok(order); _order = order; }
        /// <summary>
        /// Initialize a new TGIBlock
        /// with the specified order.
        /// </summary>
        /// <param name="handler">The <see cref="EventHandler"/> delegate to invoke if the <see cref="AHandlerElement"/> changes.</param>
        /// <param name="order">The <see cref="Order"/> to use to store the <see cref="IResourceKey"/> values.</param>
        public TGIBlock(EventHandler handler, Order order) : this(handler) { Ok(order); _order = "" + order; }

        /// <summary>
        /// Initialize a new TGIBlock
        /// with the default order ("TGI") and specified values.
        /// </summary>
        /// <param name="handler">The <see cref="EventHandler"/> delegate to invoke if the <see cref="AHandlerElement"/> changes.</param>
        /// <param name="resourceType">The resource type value.</param>
        /// <param name="resourceGroup">The resource group value.</param>
        /// <param name="instance">The resource instance value.</param>
        public TGIBlock(EventHandler handler, uint resourceType, uint resourceGroup, ulong instance)
            : base(handler, resourceType, resourceGroup, instance) { }
        /// <summary>
        /// Initialize a new TGIBlock
        /// with the specified order and values.
        /// </summary>
        /// <param name="handler">The <see cref="EventHandler"/> delegate to invoke if the <see cref="AHandlerElement"/> changes.</param>
        /// <param name="order">A <see cref="string"/> representing the <see cref="Order"/> to use to store the <see cref="IResourceKey"/> values.</param>
        /// <param name="resourceType">The resource type value.</param>
        /// <param name="resourceGroup">The resource group value.</param>
        /// <param name="instance">The resource instance value.</param>
        public TGIBlock(EventHandler handler, string order, uint resourceType, uint resourceGroup, ulong instance)
            : this(handler, resourceType, resourceGroup, instance) { Ok(order); _order = order; }
        /// <summary>
        /// Initialize a new TGIBlock
        /// with the specified order and values.
        /// </summary>
        /// <param name="handler">The <see cref="EventHandler"/> delegate to invoke if the <see cref="AHandlerElement"/> changes.</param>
        /// <param name="order">The <see cref="Order"/> to use to store the <see cref="IResourceKey"/> values.</param>
        /// <param name="resourceType">The resource type value.</param>
        /// <param name="resourceGroup">The resource group value.</param>
        /// <param name="instance">The resource instance value.</param>
        public TGIBlock(EventHandler handler, Order order, uint resourceType, uint resourceGroup, ulong instance)
            : this(handler, resourceType, resourceGroup, instance) { Ok(order); _order = "" + order; }

        /// <summary>
        /// Initialize a new TGIBlock
        /// with the default order ("TGI") and specified <see cref="IResourceKey"/> values.
        /// </summary>
        /// <param name="handler">The <see cref="EventHandler"/> delegate to invoke if the <see cref="AHandlerElement"/> changes.</param>
        /// <param name="rk">The <see cref="IResourceKey"/> values.</param>
        public TGIBlock(EventHandler handler, IResourceKey rk) : base(handler, rk) { }
        /// <summary>
        /// Initialize a new TGIBlock
        /// with the specified order and <see cref="IResourceKey"/> values.
        /// </summary>
        /// <param name="handler">The <see cref="EventHandler"/> delegate to invoke if the <see cref="AHandlerElement"/> changes.</param>
        /// <param name="order">A <see cref="string"/> representing the <see cref="Order"/> to use to store the <see cref="IResourceKey"/> values.</param>
        /// <param name="rk">The <see cref="IResourceKey"/> values.</param>
        public TGIBlock(EventHandler handler, string order, IResourceKey rk) : this(handler, rk) { Ok(order); _order = order; }
        /// <summary>
        /// Initialize a new TGIBlock
        /// with the specified order and <see cref="IResourceKey"/> values.
        /// </summary>
        /// <param name="handler">The <see cref="EventHandler"/> delegate to invoke if the <see cref="AHandlerElement"/> changes.</param>
        /// <param name="order">The <see cref="Order"/> to use to store the <see cref="IResourceKey"/> values.</param>
        /// <param name="rk">The <see cref="IResourceKey"/> values.</param>
        public TGIBlock(EventHandler handler, Order order, IResourceKey rk) : this(handler, rk) { Ok(order); _order = "" + order; }

        /// <summary>
        /// Initialize a new TGIBlock
        /// with the default order ("TGI") and values read from the specified <see cref="System.IO.Stream"/>.
        /// </summary>
        /// <param name="handler">The <see cref="EventHandler"/> delegate to invoke if the <see cref="AHandlerElement"/> changes.</param>
        /// <param name="s">The <see cref="System.IO.Stream"/> from which to read the <see cref="IResourceKey"/> values.</param>
        public TGIBlock(EventHandler handler, Stream s) : base(handler) { Parse(s); }
        /// <summary>
        /// Initialize a new TGIBlock
        /// with the specified order and values read from the specified <see cref="System.IO.Stream"/>.
        /// </summary>
        /// <param name="handler">The <see cref="EventHandler"/> delegate to invoke if the <see cref="AHandlerElement"/> changes.</param>
        /// <param name="order">A <see cref="string"/> representing the <see cref="Order"/> of the <see cref="IResourceKey"/> values.</param>
        /// <param name="s">The <see cref="System.IO.Stream"/> from which to read the <see cref="IResourceKey"/> values.</param>
        public TGIBlock(EventHandler handler, string order, Stream s) : base(handler) { Ok(order); _order = order; Parse(s); }
        /// <summary>
        /// Initialize a new TGIBlock
        /// with the specified order and values read from the specified <see cref="System.IO.Stream"/>.
        /// </summary>
        /// <param name="handler">The <see cref="EventHandler"/> delegate to invoke if the <see cref="AHandlerElement"/> changes.</param>
        /// <param name="order">The <see cref="Order"/> of the <see cref="IResourceKey"/> values.</param>
        /// <param name="s">The <see cref="System.IO.Stream"/> from which to read the <see cref="IResourceKey"/> values.</param>
        public TGIBlock(EventHandler handler, Order order, Stream s) : base(handler) { Ok(order); _order = "" + order; Parse(s); }
        #endregion

        #region Data I/O
        /// <summary>
        /// Used by the <see cref="TGIBlock"/> constructor to inialise a new <see cref="TGIBlock"/> from a <see cref="System.IO.Stream"/>.
        /// </summary>
        /// <param name="s"><see cref="System.IO.Stream"/> containing <see cref="TGIBlock"/> values in known order.</param>
        protected void Parse(Stream s)
        {
            BinaryReader r = new BinaryReader(s);
            foreach (char c in _order)
                switch (c)
                {
                    case 'T': resourceType = r.ReadUInt32(); break;
                    case 'G': resourceGroup = r.ReadUInt32(); break;
                    case 'I': instance = r.ReadUInt64(); break;
                }
        }

        /// <summary>
        /// Writes the <see cref="TGIBlock"/> to the specified <see cref="System.IO.Stream"/> in known order.
        /// </summary>
        /// <param name="s"><see cref="System.IO.Stream"/> to write <see cref="TGIBlock"/> values to.</param>
        public void UnParse(Stream s)
        {
            BinaryWriter w = new BinaryWriter(s);
            foreach (char c in _order)
                switch (c)
                {
                    case 'T': w.Write(resourceType); break;
                    case 'G': w.Write(resourceGroup); break;
                    case 'I': w.Write(instance); break;
                }
        }
        #endregion

        #region AHandlerElement
        /// <summary>
        /// The list of available field names on this API object
        /// </summary>
        public override List<string> ContentFields => GetContentFields(GetType());

        // /// <summary>
        // /// Get a copy of the <see cref="TGIBlock"/> but with a new change <see cref="EventHandler"/>.
        // /// </summary>
        // /// <param name="handler">The replacement <see cref="EventHandler"/> delegate.</param>
        // /// <returns>Return a copy of the <see cref="TGIBlock"/> but with a new change <see cref="EventHandler"/>.</returns>
        // public override AHandlerElement Clone(EventHandler handler) { return new TGIBlock(requestedApiVersion, handler, this); }
        #endregion

        #region IEquatable<TGIBlock> Members

        /// <summary>
        /// Indicates whether the current <see cref="TGIBlock"/> instance is equal to another <see cref="TGIBlock"/> instance.
        /// </summary>
        /// <param name="other">An <see cref="TGIBlock"/> instance to compare with this instance.</param>
        /// <returns>true if the current instance is equal to the <paramref name="other"/> parameter; otherwise, false.</returns>
        public bool Equals(TGIBlock other) { return base.Equals(other); }

        #endregion

        #region Content Fields
        /// <summary>
        /// A display-ready string representing the <see cref="TGIBlock"/>.
        /// </summary>
        public string Value => ToString();

        #endregion
    }
}
