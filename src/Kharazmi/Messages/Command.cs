#region

using Kharazmi.Common.Events;

#endregion

namespace Kharazmi.Messages
{
    public abstract class Command : Message, ICommand
    {
        protected Command()
        {
        }
    }
}