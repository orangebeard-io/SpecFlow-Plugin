using Orangebeard.SpecFlowPlugin.ClientExecution;
using Orangebeard.SpecFlowPlugin.ClientExtensibility.Commands.CommandArgs;

namespace Orangebeard.SpecFlowPlugin.ClientExtensibility.Commands
{
    public interface ITestCommandsSource
    {
        event TestCommandHandler<TestAttributesCommandArgs> OnGetTestAttributes;

        event TestCommandHandler<TestAttributesCommandArgs> OnAddTestAttributes;

        event TestCommandHandler<TestAttributesCommandArgs> OnRemoveTestAttributes;
    }

    public delegate void TestCommandHandler<TCommandArgs>(ITestContext testContext, TCommandArgs args);
}
