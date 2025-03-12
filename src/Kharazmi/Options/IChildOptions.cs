using System;
using System.Collections.Generic;

namespace Kharazmi.Options
{
    public interface IChildOptions
    {
        Type GetChildType();
        IEnumerable<IChildOption> GetChildOptions(Type childTpe);
    }

    public interface IChildOptions<TOption> : IChildOptions
        where TOption : class, IChildOption
    {
        HashSet<TOption> ChildOptions { get; set; }
        void AddChildOption(TOption value);
        void UpdateChildOption(TOption value);
        TOption? FindOrNone(Func<TOption, bool>? when = null);
    }
}