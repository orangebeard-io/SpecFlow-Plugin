using Orangebeard.SpecFlowPlugin.ClientExtensibility.Commands;

namespace Orangebeard.SpecFlowPlugin.ClientExtensibility
{
    public interface ICommandsListener
    {
        void Initialize(ICommandsSource commandsSource);
    }
}
