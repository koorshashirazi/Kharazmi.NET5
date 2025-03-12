#region

using Kharazmi.Messages;

#endregion

namespace Kharazmi.Collections
{
    public interface IPagedListQuery<T> : IQuery<T>, IPagedList<T> where T : class
    {
    }
}