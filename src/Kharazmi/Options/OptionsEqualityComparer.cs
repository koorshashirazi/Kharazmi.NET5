using System.Collections.Generic;

namespace Kharazmi.Options
{
    public class OptionsEqualityComparer : IEqualityComparer<IChildOption>
    {
        public bool Equals(IChildOption? x, IChildOption? y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;
            return x.OptionKey == y.OptionKey;
        }

        public int GetHashCode(IChildOption obj)
        {
            return obj.OptionKey.GetHashCode();
        }
    }
}