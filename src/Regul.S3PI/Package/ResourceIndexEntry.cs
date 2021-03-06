using System;
using System.IO;
using Regul.S3PI.Interfaces;

namespace Regul.S3PI.Package
{
    /// <summary>
    /// Implementation of an index entry
    /// </summary>
    public class ResourceIndexEntry : AResourceIndexEntry
    {
        #region AApiVersionedFields

        //No ContentFields override as we don't want to make anything more public than AResourceIndexEntry provides
        #endregion

        #region AResourceIndexEntry
        /// <summary>
        /// The "type" of the resource
        /// </summary>
        public override uint ResourceType
        {
            get { return BitConverter.ToUInt32(indexEntry, 4); }
            set { byte[] src = BitConverter.GetBytes(value); Array.Copy(src, 0, indexEntry, 4, src.Length); OnElementChanged(); }
        }
        /// <summary>
        /// The "group" the resource is part of
        /// </summary>
        public override uint ResourceGroup
        {
            get { return BitConverter.ToUInt32(indexEntry, 8); }
            set { byte[] src = BitConverter.GetBytes(value); Array.Copy(src, 0, indexEntry, 8, src.Length); OnElementChanged(); }
        }
        /// <summary>
        /// The "instance" number of the resource
        /// </summary>
        public override ulong Instance
        {
            get { return ((ulong)BitConverter.ToUInt32(indexEntry, 12) << 32) | (ulong)BitConverter.ToUInt32(indexEntry, 16); }
            set
            {
                byte[] src = BitConverter.GetBytes((uint)(value >> 32)); Array.Copy(src, 0, indexEntry, 12, src.Length);
                src = BitConverter.GetBytes((uint)(value & 0xffffffff)); Array.Copy(src, 0, indexEntry, 16, src.Length);
                OnElementChanged();
            }
        }
        /// <summary>
        /// If the resource was read from a package, the location in the package the resource was read from
        /// </summary>
        public override uint Chunkoffset
        {
            get { return BitConverter.ToUInt32(indexEntry, 20); }
            set { byte[] src = BitConverter.GetBytes(value); Array.Copy(src, 0, indexEntry, 20, src.Length); OnElementChanged(); }
        }
        /// <summary>
        /// The number of bytes the resource uses within the package
        /// </summary>
        public override uint Filesize
        {
            get { return BitConverter.ToUInt32(indexEntry, 24) & 0x7fffffff; }
            set { byte[] src = BitConverter.GetBytes(value | 0x80000000); Array.Copy(src, 0, indexEntry, 24, src.Length); OnElementChanged(); OnElementChanged(); }
        }
        /// <summary>
        /// The number of bytes the resource uses in memory
        /// </summary>
        public override uint Memsize
        {
            get { return BitConverter.ToUInt32(indexEntry, 28); }
            set { byte[] src = BitConverter.GetBytes(value); Array.Copy(src, 0, indexEntry, 28, src.Length); OnElementChanged(); }
        }
        /// <summary>
        /// 0xFFFF if Filesize != Memsize, else 0x0000
        /// </summary>
        public override ushort Compressed
        {
            get { return BitConverter.ToUInt16(indexEntry, 32); }
            set { byte[] src = BitConverter.GetBytes(value); Array.Copy(src, 0, indexEntry, 32, src.Length); OnElementChanged(); }
        }
        /// <summary>
        /// Always 0x0001
        /// </summary>
        public override ushort Unknown2
        {
            get { return BitConverter.ToUInt16(indexEntry, 34); }
            set { byte[] src = BitConverter.GetBytes(value); Array.Copy(src, 0, indexEntry, 34, src.Length); OnElementChanged(); }
        }

        /// <summary>
        /// A MemoryStream covering the index entry bytes
        /// </summary>
        public override Stream Stream { get { return new MemoryStream(indexEntry); } }

        /// <summary>
        /// True if the index entry has been deleted from the package index
        /// </summary>
        public override bool IsDeleted { get { return isDeleted; } set { if (isDeleted != value) { isDeleted = value; OnElementChanged(); } } }

        /// <summary>
        /// Get a copy of this element but with a new change event handler
        /// </summary>
        /// <param name="handler">Element change event handler</param>
        /// <returns>Return a copy of this element but with a new change event handler</returns>
        public override AHandlerElement Clone(EventHandler handler) { return new ResourceIndexEntry(indexEntry); }
        #endregion

        #region IEquatable<IResourceIndexEntry>
        /// <summary>
        /// Indicates whether the current <see cref="ResourceIndexEntry"/> instance is equal to another <see cref="IResourceIndexEntry"/> instance.
        /// </summary>
        /// <param name="other">An <see cref="IResourceIndexEntry"/> instance to compare with this instance.</param>
        /// <returns>true if the current instance is equal to the <paramref name="other"/> parameter; otherwise, false.</returns>
        public override bool Equals(IResourceIndexEntry other) => other is ResourceIndexEntry entry && indexEntry == entry.indexEntry;

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override int GetHashCode() => indexEntry.GetHashCode();

        #endregion


        #region Implementation
        /// <summary>
        /// The index entry data
        /// </summary>
        byte[] indexEntry;

        /// <summary>
        /// True if the index entry should be treated as deleted
        /// </summary>
        bool isDeleted;

        /// <summary>
        /// The uncompressed resource data associated with this index entry
        /// (used to save having to uncompress the same entry again if it's requested more than once)
        /// </summary>
        Stream resourceStream;

        /// <summary>
        /// Create a new index entry as a byte-for-byte copy of <paramref name="indexEntry"/>
        /// </summary>
        /// <param name="indexEntry">The source index entry</param>
        private ResourceIndexEntry(byte[] indexEntry) { this.indexEntry = (byte[])indexEntry.Clone(); }

        /// <summary>
        /// Create a new expanded index entry from the header and entry data passed
        /// </summary>
        /// <param name="header">header ints (same for each index entry); [0] is the index type</param>
        /// <param name="entry">entry ints (specific to this entry)</param>
        internal ResourceIndexEntry(int[] header, int[] entry)
        {
            indexEntry = new byte[(header.Length + entry.Length) * 4];
            MemoryStream ms = new MemoryStream(indexEntry);
            BinaryWriter w = new BinaryWriter(ms);

            w.Write(header[0]);

            int hc = 1;// header[0] is indexType, already written!
            int ec = 0;
            uint IhGT = (uint)header[0];
            w.Write((IhGT & 0x01) != 0 ? header[hc++] : entry[ec++]);
            w.Write((IhGT & 0x02) != 0 ? header[hc++] : entry[ec++]);
            w.Write((IhGT & 0x04) != 0 ? header[hc++] : entry[ec++]);

            for (; hc < header.Length - 1; hc++)
                w.Write(header[hc]);

            for (; ec < entry.Length; ec++)
                w.Write(entry[ec]);
        }

        /// <summary>
        /// Return a new index entry as a copy of this one
        /// </summary>
        /// <returns>A copy of this index entry</returns>
        internal ResourceIndexEntry Clone() => (ResourceIndexEntry)Clone(null);

        /// <summary>
        /// Flag this index entry as deleted
        /// </summary>
        /// <remarks>Use APackage.RemoveResource() from user code</remarks>
        internal void Delete()
        {
            if (Settings.Settings.Checking && isDeleted) throw new InvalidOperationException("Index entry already deleted!");

            isDeleted = true;
            OnElementChanged();
        }

        /// <summary>
        /// The uncompressed resource data associated with this index entry
        /// (used to save having to uncompress the same entry again if it's requested more than once)
        /// Setting the stream updates the Memsize
        /// </summary>
        /// <remarks>Use Package.ReplaceResource() from user code</remarks>
        internal Stream ResourceStream
        {
            get { return resourceStream; }
            set { if (resourceStream != value) { resourceStream = value; if (Memsize != (uint)resourceStream.Length) Memsize = (uint)resourceStream.Length; else OnElementChanged(); } }
        }

        /// <summary>
        /// True if the index entry should be treated as dirty - e.g. the ResourceStream has been replaced
        /// </summary>
        internal bool IsDirty { get { return dirty; } }
        #endregion
    }
}
