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
using System.IO;

namespace System.Text
{
    /// <summary>
    /// Read and write a seven-bit encoded length-prefixed string in a given <see cref="Encoding"/> from or to a <see cref="Stream"/>.
    /// </summary>
    public static class SevenBitString
    {
        /// <summary>
        /// Read a string from <see cref="Stream"/> <paramref name="s"/> using <see cref="Encoding"/> <paramref name="enc"/>.
        /// </summary>
        /// <param name="s"><see cref="Stream"/> from which to read string.</param>
        /// <param name="enc"><see cref="Encoding"/> to use when reading.</param>
        /// <returns>A <see cref="string"/> value.</returns>
        public static string Read(Stream s, Encoding enc) { return new BinaryReader(s, enc).ReadString(); }

        /// <summary>
        /// Write a string to <see cref="Stream"/> <paramref name="s"/> using <see cref="Encoding"/> <paramref name="enc"/>.
        /// </summary>
        /// <param name="s"><see cref="Stream"/> to which to write string.</param>
        /// <param name="enc"><see cref="Encoding"/> to use when writing.</param>
        /// <param name="value">The <see cref="string"/> to write.</param>
        public static void Write(Stream s, Encoding enc, string value)
        {
            byte[] bytes = enc.GetBytes(value ?? "");
            BinaryWriter w = new BinaryWriter(s, enc);
            for (int i = bytes.Length; ; ) { w.Write((byte)((i & 0x7F) | (i > 0x7F ? 0x80 : 0))); i >>= 7; if (i == 0) break; }//zero length? write a zero
            w.Write(bytes);
        }
    }

    /// <summary>
    /// Read and write a seven-bit encoded length-prefixed string in <see cref="Encoding.BigEndianUnicode"/> from or to a <see cref="Stream"/>.
    /// </summary>
    public static class BigEndianUnicodeString
    {
        /// <summary>
        /// Read a string from <see cref="Stream"/> <paramref name="s"/> using <see cref="Encoding.BigEndianUnicode"/>.
        /// </summary>
        /// <param name="s"><see cref="Stream"/> from which to read string.</param>
        /// <returns>A <see cref="string"/> value.</returns>
        public static string Read(Stream s) { return SevenBitString.Read(s, Encoding.BigEndianUnicode); }

        /// <summary>
        /// Write a string to <see cref="Stream"/> <paramref name="s"/> using <see cref="Encoding.BigEndianUnicode"/>.
        /// </summary>
        /// <param name="s"><see cref="Stream"/> to which to write string.</param>
        /// <param name="value">The <see cref="string"/> to write.</param>
        public static void Write(Stream s, string value) { SevenBitString.Write(s, Encoding.BigEndianUnicode, value); }
    }
}
