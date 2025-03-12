using System;
using Kharazmi.Exceptions;
using Microsoft.Extensions.Configuration;

namespace Kharazmi.Extensions
{
    public static class ConfigurationExtensions
    {
        public static T Get<T>(this IConfigurationSection section) where T : class
        {
            try
            {
                return ConfigurationBinder.Get<T>(section);
            }
            catch(Exception ex)
            {
                throw new OptionValidationException(ex.Message);
            }
        }
        
        public static T TryGet<T>(this IConfigurationSection section) where T : class
        {
            try
            {
                return section.Get<T>();
            }
            catch
            {
                return typeof(T).CreateInstance<T>();
            }
        }

        public static T TryGet<T>(this IConfigurationSection section, T value) where T : class
        {
            try
            {
                return ConfigurationBinder.Get<T>(section) ?? typeof(T).CreateInstance<T>();
            }
            catch
            {
                return value;
            }
        }
    }
}