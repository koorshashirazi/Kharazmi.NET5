using System;
using System.Collections.Generic;
using System.IO;

namespace Kharazmi.Configuration
{
    internal class BackupFileInfoComparer : IEqualityComparer<FileInfo>
    {
        public bool Equals(FileInfo? x, FileInfo? y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;
            return x.Name == y.Name;
        }

        public int GetHashCode(FileInfo obj)
        {
            return HashCode.Combine(obj.Name);
        }
    }
}