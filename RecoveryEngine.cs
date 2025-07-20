using System;
using System.Collections.Generic;
using System.IO;

namespace NullBack
{
    public class RecoveryEngine
    {
        private readonly NTFSHelper _ntfsHelper;

        public RecoveryEngine()
        {
            _ntfsHelper = new NTFSHelper();
        }

        public List<DeletedFile> ScanForDeletedItems(string directoryPath)
        {
            // Verify NTFS file system
            DriveInfo drive = new DriveInfo(Path.GetPathRoot(directoryPath));
            if (drive.DriveFormat != "NTFS")
            {
                throw new NotSupportedException("Only NTFS file systems are supported.");
            }

            return _ntfsHelper.ScanDeletedItems(directoryPath);
        }

        public void RecoverFile(DeletedFile file, string destinationPath)
        {
            if (!_ntfsHelper.RecoverFile(file, destinationPath))
            {
                throw new IOException("File recovery failed.");
            }
        }

        public void PurgeMftEntry(DeletedFile file)
        {
            Console.WriteLine("\nWARNING: This will permanently remove the file entry from the MFT.");
            Console.WriteLine("The file will become unrecoverable by any means.");
            Console.Write("Are you sure? (Y/N): ");
            
            if (Console.ReadLine().Trim().Equals("Y", StringComparison.OrdinalIgnoreCase))
            {
                if (!_ntfsHelper.PurgeMftEntry(file))
                {
                    throw new IOException("MFT entry purge failed.");
                }
            }
        }
    }
}