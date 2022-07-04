using Orangebeard.SpecFlowPlugin.ClientExecution.Logging;

namespace Orangebeard.SpecFlowPlugin.ClientExtensibility.Commands.CommandArgs
{ 
    public class LogScopeCommandArgs
    {
        public LogScopeCommandArgs(ILogScope logScope)
        {
            LogScope = logScope;
        }

        public ILogScope LogScope { get; }
    }
}
