using System;
using System.Collections.Generic;
using Kharazmi.Options;

namespace Kharazmi.Configuration
{
    public interface ISettings : IReadOnlyDictionary<string, object>
    {
        bool IsDirty();
        void MakeDirty(bool value);
        object GetOption(Type optionType);
        object? TryGetOption(Type optionType);
        T GetOption<T>() where T : IOptions;
        T? TryGetOption<T>() where T : IOptions;
        void AddOption(string key, object option);
        void AddOption<T>(T option) where T : class, IOptions;
        void UpdateOption<T>(T option) where T : class, IOptions;
    }
}