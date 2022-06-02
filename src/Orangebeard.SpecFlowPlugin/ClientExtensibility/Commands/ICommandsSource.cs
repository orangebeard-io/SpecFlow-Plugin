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
        event LogCommandHandler<LogScopeCommandArgs> OnBeginLogScopeCommand;

        event LogCommandHandler<LogScopeCommandArgs> OnEndLogScopeCommand;

        event LogCommandHandler<LogMessageCommandArgs> OnLogMessageCommand;

        ITestCommandsSource TestCommandsSource { get; }
    }

    public delegate void LogCommandHandler<TCommandArgs>(ILogContext logContext, TCommandArgs args);
}
