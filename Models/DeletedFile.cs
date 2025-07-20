using System;

namespace NullBack
{
    public class DeletedFile
    {
        public string Name { get; set; }
        public string OriginalPath { get; set; }
        public long Size { get; set; }
        public DateTime DeletedDate { get; set; }
        public double RecoveryProbability { get; set; }
        public string FileType { get; set; }
        
        // NTFS-specific metadata
        public ulong MftReferenceNumber { get; set; }
        public bool IsDirectory { get; set; }
    }
}