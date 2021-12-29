/***************************************************************************
 *  Based on earlier work.                                                 *
 *  Copyright (C) 2012 by Peter L Jones                                    *
 *  pljones@users.sf.net                                                   *
 *                                                                         *
 *  This is free software: you can redistribute it and/or modify           *
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
 *  along with this software.  If not, see <http://www.gnu.org/licenses/>. *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using Regul.S3PI.Interfaces;

namespace Regul.S3PI.Resources.MeshChunks
{
    public class GeometryResource : GenericRCOLResource.GenericRCOLResource
    {
        public GeometryResource(Stream s)
            : base(s)
        {
            if (ChunkEntries.Count != 1)
                throw new InvalidDataException(String.Format("Expected one chunk, found {0}.", ChunkEntries.Count));

            ChunkEntry chunk = ChunkEntries[0];
            ChunkEntries.Remove(chunk);
            using (MemoryStream ms = new MemoryStream())
            {
                new BinaryWriter(ms).Write(chunk.RCOLBlock.AsBytes);
                ms.Flush();
                ms.Position = 0;
                GEOM geom = new GEOM(OnResourceChanged, ms);
                ChunkEntries.Add(new ChunkEntry(OnResourceChanged, chunk.TGIBlock, geom));
            }
        }

        protected override Stream UnParse()
        {
            if (version == 0 && publicChunks == 0 && unused == 0 && blockList == null && resources == null)
            {
                // In that case, assume we're a newly created "stream == null" situation GenericRCOLResource that needs
                // some help to become a real life GeometryResource.

                resources = new CountedTGIBlockList(OnResourceChanged, "ITG");
                // Unfortunately, a Resource does not know its own ResourceKey.  This is the best we can manage.
                TGIBlock rk = new TGIBlock(null, 0x015A1849, 0, 0);
                GEOM geom = new GEOM(OnResourceChanged);
                blockList = new ChunkEntryList(OnResourceChanged, new[] { new ChunkEntry(OnResourceChanged, rk, geom), }) { ParentTGIBlocks = Resources, };
            }
            return base.UnParse();
        }
    }

    /// <summary>
    /// ResourceHandler for GeometryResource wrapper
    /// </summary>
    public class GeometryResourceHandler : AResourceHandler
    {
        public GeometryResourceHandler()
        {
            Add(typeof(GeometryResource), new List<string>(new[] { "0x015A1849", }));
        }
    }
}