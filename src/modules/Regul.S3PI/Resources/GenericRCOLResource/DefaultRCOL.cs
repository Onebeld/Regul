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
using Regul.S3PI.Interfaces;
using System;
using System.IO;

namespace Regul.S3PI.Resources.GenericRCOLResource
{
    /// <summary>
    /// An RCOL block handler that "does nothing", providing the minimal support required for any RCOL block in a <see cref="GenericRCOLResource"/>.
    /// </summary>
    public sealed class DefaultRCOL : ARCOLBlock
    {
        private byte[] _data = Array.Empty<byte>();

        /// <summary>
        /// <see cref="DefaultRCOL"/> does not provide a constructor that <see cref="GenericRCOLResourceHandler.CreateRcolBlock"/> can call.
        /// <para>Calling this constructor throws a <see cref="NotImplementedException"/>.</para>
        /// </summary>
        /// <param name="handler">Unused.</param>
        /// <exception cref="NotImplementedException">Thrown if this constructor is called.</exception>
        public DefaultRCOL(EventHandler handler) : base(handler, null) { throw new NotImplementedException(); }
        /// <summary>
        /// Read the block data from the <see cref="Stream"/> provided.
        /// </summary>
        /// <param name="handler">Unused; change <see cref="EventHandler"/>.</param>
        /// <param name="s"><see cref="Stream"/> containing the data to read.</param>
        public DefaultRCOL(EventHandler handler, Stream s) : base(handler, s) { }
        /// <summary>
        /// Create a new instance from an existing instance.
        /// </summary>
        /// <param name="handler">Unused; change <see cref="EventHandler"/>.</param>
        /// <param name="basis">An existing <see cref="DefaultRCOL"/> to use as a basis.</param>
        public DefaultRCOL(EventHandler handler, DefaultRCOL basis) : base(null, null) { this.handler = handler; _data = (byte[])basis._data.Clone(); }

        /// <summary>
        /// Read the data.
        /// </summary>
        /// <param name="s">The <see cref="Stream"/> containing the data.</param>
        protected override void Parse(Stream s) { _data = new byte[s.Length]; s.Read(_data, 0, (int)s.Length); }

        // /// <summary>
        // /// Creating a copy of this instance with a new change <see cref="EventHandler"/>.
        // /// </summary>
        // /// <param name="handler">The <see cref="EventHandler"/> for the new instance.</param>
        // /// <returns>A new instance with a copy of the data and the given change <see cref="EventHandler"/>.</returns>
        // public override AHandlerElement Clone(EventHandler handler) { return new DefaultRCOL(requestedApiVersion, handler, this); }

        /// <summary>
        /// DefaultRCOL only supplies &quot;Tag&quot; for <see cref="GenericRCOLResourceHandler"/>.
        /// <para>It returns &quot;*&quot; to indicate it is the default RCOL block handler.</para>
        /// </summary>
        [ElementPriority(2)]
        public override string Tag => "*"; // For RCOLDealer

        /// <summary>
        /// DefaultRCOL only supplies &quot;ResourceType&quot; for <see cref="GenericRCOLResourceHandler"/>.
        /// <para>It returns <c>(uint)-1</c>.</para>
        /// </summary>
        [ElementPriority(3)]
        public override uint ResourceType => 0xFFFFFFFF;

        /// <summary>
        /// Return the data in a <see cref="Stream"/>.
        /// </summary>
        /// <returns>The data in a <see cref="Stream"/>.</returns>
        public override Stream UnParse() { MemoryStream ms = new MemoryStream(); ms.Write(_data, 0, _data.Length); return ms; }

        /// <summary>
        /// A default string to display, identifying any tag within the data and the length of the data block.
        /// </summary>
        public string Value => "Tag: " + FOURCC(BitConverter.ToUInt32(_data, 0)) + "\nLength: " + _data.Length;
    }
}
