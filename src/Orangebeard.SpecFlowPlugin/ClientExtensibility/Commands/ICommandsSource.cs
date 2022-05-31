using Orangebeard.Client;
using Orangebeard.SpecFlowPlugin.ClientExecution;
using Orangebeard.SpecFlowPlugin.ClientExtensibility.Commands.CommandArgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orangebeard.SpecFlowPlugin.ClientExtensibility.Commands
{
    public interface ICommandsSource
    {
        event LogCommandHandler<LogScopeCommandArgs, OrangebeardV2Client, Guid?> OnBeginLogScopeCommand;

        event LogCommandHandler<LogScopeCommandArgs, OrangebeardV2Client, Guid?> OnEndLogScopeCommand;

        event LogCommandHandler<LogMessageCommandArgs, OrangebeardV2Client, Guid?> OnLogMessageCommand;

        ITestCommandsSource TestCommandsSource { get; }
    }

    public delegate void LogCommandHandler<TCommandArgs, TClient, TGuid>(ILogContext logContext, TCommandArgs args, TClient client, TGuid nullableGuid);
}
