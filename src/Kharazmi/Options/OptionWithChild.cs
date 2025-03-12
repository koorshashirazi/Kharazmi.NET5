using System;
using System.Collections.Generic;
using System.Linq;
using Kharazmi.Extensions;

namespace Kharazmi.Options
{
    public abstract class OptionWithChild<TOption> : Options, IChildOptions<TOption>
        where TOption : class, IChildOption
    {
        protected OptionWithChild()
        {
            ChildOptions = new HashSet<TOption>(new ChildOptionOptionsEqualityComparer());
        }


        public HashSet<TOption> ChildOptions { get; set; }

        public void AddChildOption(TOption value)
            => ChildOptions.Add(value);

        public void UpdateChildOption(TOption value)
        {
            value.MakeDirty(true);

            ChildOptions.Remove(value);
            ChildOptions.Add(value);
        }


        public TOption? FindOrNone(Func<TOption, bool>? when = null)
        {
            return when.IsNull() ? ChildOptions.FirstOrDefault() : ChildOptions.FirstOrDefault(when);
        }

        public Type GetChildType() => typeof(TOption);

        public IEnumerable<IChildOption> GetChildOptions(Type childTpe)
        {
            return ChildOptions.Where(x => x.GetType() == childTpe).Select(x => (IChildOption) x).ToList();
        }
    }
}