using System.Runtime.Serialization;
using Regul.S3PI.Interfaces;

namespace Regul.S3PE.Structures
{
    public struct ClipboardDataFormat
    {
        public SerializableTGIN Tgin { get; set; }
        public string Data { get; set; }
    }
    
    [DataContract]
    public class SerializableTGIN
    {
        /// <summary>
        /// The Resource Type represented by this instance.
        /// </summary>
        [DataMember]
        public uint ResType { get; set; }
        /// <summary>
        /// The Resource Group represented by this instance.
        /// </summary>
        [DataMember]
        public uint ResGroup { get; set; }
        /// <summary>
        /// The Resource Instance ID represented by this instance.
        /// </summary>
        [DataMember]
        public ulong ResInstance { get; set; }
        /// <summary>
        /// The Resource Name (from the package name map, based on the IID) represented by this instance.
        /// </summary>
        [DataMember]
        public string ResName { get; set; }
        
        public SerializableTGIN() { }
        
        public SerializableTGIN(IResourceKey rk, string name) { ResType = rk.ResourceType; ResGroup = rk.ResourceGroup; ResInstance = rk.Instance; ResName = name; }
    }
}