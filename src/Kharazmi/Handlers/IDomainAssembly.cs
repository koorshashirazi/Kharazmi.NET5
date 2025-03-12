using System.Reflection;

namespace Kharazmi.Handlers
{
    public interface IDomainAssembly
    {
        Assembly[] GetAssemblies();
    }
}