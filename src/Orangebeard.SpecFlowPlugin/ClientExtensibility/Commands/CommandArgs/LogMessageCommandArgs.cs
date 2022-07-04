using Orangebeard.SpecFlowPlugin.ClientExecution.Logging;

namespace Orangebeard.SpecFlowPlugin.ClientExtensibility.Commands.CommandArgs
{
    public class LogMessageCommandArgs
    {
        public LogMessageCommandArgs(ILogScope logScope, LogMessage logMessage)
        {
            LogScope = logScope;
            LogMessage = logMessage;
        }

        public ILogScope LogScope { get; }

        public LogMessage LogMessage { get; }
    }
}
