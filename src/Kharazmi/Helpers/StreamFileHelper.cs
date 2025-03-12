#region

using System.IO;
using System.Reflection;
using Kharazmi.Guard;

#endregion

namespace Kharazmi.Helpers
{
    public static class StreamFileHelper
    {
        public static TextReader? ReadManifestResource(string nameSpace, string filename)
        {
            var thisAssembly = Assembly.GetExecutingAssembly();
            return ReadManifestResource(thisAssembly, nameSpace, filename);
        }

        public static TextReader? ReadManifestResource(Assembly assembly, string nameSpace, string filename)
        {
            try
            {
                var stream = assembly.GetManifestResourceStream(nameSpace + "." + filename);
                stream = stream.NotNull(nameof(stream));
                return new StreamReader(stream);
            }
            catch
            {
                return default;
            }
        }

        public static byte[]? ReadByteManifestResource(Assembly assembly, string nameSpace, string filename)
        {
            try
            {
                var stream = assembly.GetManifestResourceStream(nameSpace + "." + filename);
                stream = stream.NotNull(nameof(stream));
                using var ms = new MemoryStream();
                stream.CopyTo(ms);
                return ms.ToArray();
            }
            catch
            {
                return default;
            }
        }
    }
}