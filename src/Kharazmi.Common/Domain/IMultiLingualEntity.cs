using System;
using System.Collections.Generic;
using Kharazmi.Common.Domain;

namespace Kharazmi.AspNetCore.Core.Domain.Entities
{
    public interface IMultiLingualEntity<TTranslation, TKey>
        where TTranslation : Entity<TKey>, IEntityTranslation where TKey : IEquatable<TKey>, IComparable
    {
        ICollection<TTranslation> Translations { get; }
    }
}