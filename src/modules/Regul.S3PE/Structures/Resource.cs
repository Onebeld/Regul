using Regul.S3PI.Interfaces;

namespace Regul.S3PE.Structures
{
    public class Resource
    {
        public string Name { get; set; }
        public string Tag { get; set; }
        
        public IResourceIndexEntry IndexEntry { get; set; }
    }
}