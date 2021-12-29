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
using System.Collections.Generic;
using System.IO;
using Regul.S3PI.Interfaces;

namespace Regul.S3PI.Package
{
    /// <summary>
    /// Internal -- used by Package to manage the package index
    /// </summary>
    internal class PackageIndex : List<IResourceIndexEntry>
    {
        const int NumFields = 9;

        uint _indextype;
        public uint Indextype => _indextype;

        int Hdrsize
        {
            get
            {
                int hc = 1;
                for (int i = 0; i < sizeof(uint); i++) if ((_indextype & (1 << i)) != 0) hc++;
                return hc;
            }
        }

        public PackageIndex() { }
        public PackageIndex(uint type) { _indextype = type; }
        public PackageIndex(Stream s, int indexposition, int indexsize, int indexcount)
        {
            if (s == null) return;
            if (indexposition == 0) return;

            BinaryReader r = new BinaryReader(s);
            s.Position = indexposition;
            _indextype = r.ReadUInt32();

            int[] hdr = new int[Hdrsize];
            int[] entry = new int[NumFields - Hdrsize];

            hdr[0] = (int)_indextype;
            for (int i = 1; i < hdr.Length; i++)
                hdr[i] = r.ReadInt32();

            for (int i = 0; i < indexcount; i++)
            {
                for (int j = 0; j < entry.Length; j++)
                    entry[j] = r.ReadInt32();
                base.Add(new ResourceIndexEntry(hdr, entry));
            }
        }

        public IResourceIndexEntry Add(IResourceKey rk)
        {
            ResourceIndexEntry rc = new ResourceIndexEntry(new int[Hdrsize], new int[NumFields - Hdrsize])
            {
                ResourceType = rk.ResourceType,
                ResourceGroup = rk.ResourceGroup,
                Instance = rk.Instance,
                Chunkoffset = 0xffffffff,
                Unknown2 = 1,
                ResourceStream = null
            };


            base.Add(rc);
            return rc;
        }

        public int Size => (Count * (NumFields - Hdrsize) + Hdrsize) * 4;

        public void Save(BinaryWriter w)
        {
            BinaryReader r = null;
            r = Count == 0 ? new BinaryReader(new MemoryStream(new byte[NumFields * 4])) : new BinaryReader(this[0].Stream);
            
            r.BaseStream.Position = 4;
            w.Write(_indextype);
            if ((_indextype & 0x01) != 0) w.Write(r.ReadUInt32()); else r.BaseStream.Position += 4;
            if ((_indextype & 0x02) != 0) w.Write(r.ReadUInt32()); else r.BaseStream.Position += 4;
            if ((_indextype & 0x04) != 0) w.Write(r.ReadUInt32()); else r.BaseStream.Position += 4;

            for (int index = 0; index < Count; index++)
            {
                IResourceIndexEntry ie = this[index];
                
                r = new BinaryReader(ie.Stream);
                r.BaseStream.Position = 4;
                if ((_indextype & 0x01) == 0) w.Write(r.ReadUInt32());
                else r.BaseStream.Position += 4;
                if ((_indextype & 0x02) == 0) w.Write(r.ReadUInt32());
                else r.BaseStream.Position += 4;
                if ((_indextype & 0x04) == 0) w.Write(r.ReadUInt32());
                else r.BaseStream.Position += 4;
                w.Write(r.ReadBytes((int) (ie.Stream.Length - ie.Stream.Position)));
            }
        }

        /// <summary>
        /// Sort the index by the given field
        /// </summary>
        /// <param name="index">Field to sort by</param>
        public void Sort(string index) { base.Sort(new AApiVersionedFields.Comparer<IResourceIndexEntry>(index)); }

        /// <summary>
        /// Return the index entry with the matching TGI
        /// </summary>
        /// <param name="type">Entry type</param>
        /// <param name="group">Entry group</param>
        /// <param name="instance">Entry instance</param>
        /// <returns>Matching entry</returns>
        public IResourceIndexEntry this[uint type, uint group, ulong instance]
        {
            get
            {
                for (int index = 0; index < Count; index++)
                {
                    ResourceIndexEntry rie = (ResourceIndexEntry) this[index];
                    if (rie.ResourceType != type) continue;
                    if (rie.ResourceGroup != @group) continue;
                    if (rie.Instance == instance) return rie;
                }

                return null;
            }
        }

        /// <summary>
        /// Returns requested resource, ignoring EPFlags
        /// </summary>
        /// <param name="rk">Resource key to find</param>
        /// <returns>Matching entry</returns>
        public IResourceIndexEntry this[IResourceKey rk] => this[rk.ResourceType, rk.ResourceGroup, rk.Instance];
    }
}
