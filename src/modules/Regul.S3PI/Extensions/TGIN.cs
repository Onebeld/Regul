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
using Regul.S3PI.Interfaces;
using System;
using System.IO;

namespace Regul.S3PI.Extensions
{
    /// <summary>
    /// A structure to manage conversion from <see cref="AResourceKey"/> to
    /// the standardised Sims3 resource export file name format.
    /// </summary>
    /// <seealso cref="ExtList"/>
    [Serializable]
    public struct TGIN
    {
        /// <summary>
        /// The Resource Type represented by this instance.
        /// </summary>
        public uint ResType;
        /// <summary>
        /// The Resource Group represented by this instance.
        /// </summary>
        public uint ResGroup;
        /// <summary>
        /// The Resource Instance ID represented by this instance.
        /// </summary>
        public ulong ResInstance;
        /// <summary>
        /// The Resource Name (from the package name map, based on the IID) represented by this instance.
        /// </summary>
        public string ResName;

        /// <summary>
        /// Instantiate a new <see cref="TGIN"/> based on the <see cref="IResourceKey"/> and <paramref name="name"/>.
        /// </summary>
        /// <param name="rk">An <see cref="IResourceKey"/>.</param>
        /// <param name="name">A <see cref="string"/>, the name of the resource.</param>
        public TGIN(IResourceKey rk, string name) { ResType = rk.ResourceType; ResGroup = rk.ResourceGroup; ResInstance = rk.Instance; ResName = name; }

        /// <summary>
        /// Cast an <see cref="AResourceKey"/> value to a <see cref="TGIN"/>.
        /// </summary>
        /// <param name="value">An <see cref="AResourceKey"/> value.</param>
        /// <returns>The equivalent <see cref="TGIN"/> (with no <see cref="ResName"/>).</returns>
        public static implicit operator TGIN(AResourceKey value)
        {
            return new TGIN()
            {
                ResType = value.ResourceType,
                ResGroup = value.ResourceGroup,
                ResInstance = value.Instance
            };
        }
        /// <summary>
        /// Cast a <see cref="TGIN"/> to an <see cref="AResourceKey"/> value.
        /// </summary>
        /// <param name="value">A <see cref="TGIN"/>.</param>
        /// <returns>The equivalent <see cref="AResourceKey"/> value.</returns>
        public static implicit operator AResourceKey(TGIN value) { return new TGIBlock(null, value.ResType, value.ResGroup, value.ResInstance); }

        /// <summary>
        /// Casts a <see cref="string"/> to a <see cref="TGIN"/>.
        /// <para>The string value is presumed to be in the standardised
        /// Sims3 resource export file name format.</para>
        /// </summary>
        /// <param name="value">A string value is presumed to be in the standardised
        /// Sims3 resource export file name format.</param>
        /// <returns>The equivalent <see cref="TGIN"/> value.</returns>
        public static implicit operator TGIN(string value)
        {
            TGIN res = new TGIN();

            value = Path.GetFileNameWithoutExtension(value);

            int i = value.ToLower().IndexOf("s3_");
            if (i == 0) value = value.Substring(3);
            i = value.IndexOf("%%+");
            if (i >= 0) value = value.Substring(0, i);

            string[] fnParts = value.Split(new[] { '_', '-' }, 4);
            if (fnParts.Length >= 3)
            {
                try
                {
                    res.ResType = Convert.ToUInt32(fnParts[0], 16);
                    res.ResGroup = Convert.ToUInt32(fnParts[1], 16);
                    res.ResInstance = Convert.ToUInt64(fnParts[2], 16);
                }
                catch { }
                if (fnParts.Length == 4)
                    res.ResName = UnescapeString(fnParts[3]);
            }

            return res;
        }
        /// <summary>
        /// Casts a <see cref="TGIN"/> to a <see cref="string"/> 
        /// in the standardised Sims3 resource export file name format.
        /// </summary>
        /// <param name="value">A <see cref="TGIN"/>.</param>
        /// <returns>A <see cref="string"/> in the standardised Sims3 resource export file name format.</returns>
        public static implicit operator string(TGIN value)
        {
            string extn = ".dat";
            if (ExtList.Ext.ContainsKey("0x" + value.ResType.ToString("X8")))
                extn = string.Join("", ExtList.Ext["0x" + value.ResType.ToString("X8")].ToArray());
            else if (ExtList.Ext.ContainsKey("*"))
                extn = string.Join("", ExtList.Ext["*"].ToArray());

            return string.Format((value.ResName != null && value.ResName.Length > 0)
                    ? "S3_{0:X8}_{1:X8}_{2:X16}_{3}%%+{4}"
                    : "S3_{0:X8}_{1:X8}_{2:X16}%%+{4}"
                    , value.ResType, value.ResGroup, value.ResInstance, value.ResName == null ? "" : EscapeString(value.ResName), extn);
        }

        /// <summary>
        /// Returns a <see cref="string"/> in the standardised Sims3 resource export file name format
        /// equivalent to this instance.
        /// </summary>
        /// <returns>A <see cref="string"/> in the standardised Sims3 resource export file name format.</returns>
        public override string ToString() { return this; }

        private static string UnescapeString(string value)
        {
            for (int i = value.IndexOf("%"); i >= 0 && i + 2 < value.Length; i = value.IndexOf("%"))
            {
                try
                {
                    string bad = value.Substring(i + 1, 2);
                    string rep = new string(new[] { (char)Convert.ToByte(bad, 16) });
                    value = value.Replace("%" + bad, rep);
                }
                catch { break; }
            }
            return value;
        }

        private static string EscapeString(string value)
        {
            char[][] array = new[] { Path.GetInvalidFileNameChars(), Path.GetInvalidPathChars(), new[] { '-' } };
            for (int i1 = 0; i1 < array.Length; i1++)
            {
                char[] ary = array[i1];
                for (int i = 0; i < ary.Length; i++)
                {
                    char c = ary[i];
                    string bad = new string(new[] { c });
                    string rep = $"%{(uint) c:x2}";
                    value = value.Replace(bad, rep);
                }
            }
            return value;
        }
    }
}
