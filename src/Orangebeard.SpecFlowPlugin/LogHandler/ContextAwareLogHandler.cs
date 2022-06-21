using Orangebeard.Client;
using Orangebeard.Client.Entities;
using Orangebeard.Shared.Internal.Logging;
using Orangebeard.SpecFlowPlugin.ClientExecution.Logging;
using Orangebeard.SpecFlowPlugin.ClientExtensibility;
using Orangebeard.SpecFlowPlugin.ClientExtensibility.Commands;
using System;
using System.Collections.Generic;
using System.Threading;
using TechTalk.SpecFlow;

namespace Orangebeard.SpecFlowPlugin.LogHandler
{
    //TODO?- The  On...Command handlers aren't initialized, or called. Their code has been copied elsewhere.
    public class ContextAwareLogHandler //: ICommandsListener
    {
        private readonly ITraceLogger _traceLogger = TraceLogManager.Instance.GetLogger<ContextAwareLogHandler>();

        //TODO?~ Was a "private" instance method, and supposed to be called on a hook. Except that didn't happen.
        // Made static for the moment, see if we fix the hook or not.
        public static void CommandsSource_OnEndLogScopeCommand(ClientExecution.ILogContext logContext, ClientExtensibility.Commands.CommandArgs.LogScopeCommandArgs args)
        {
            var logScope = args.LogScope;

            if (OrangebeardAddIn.LogScopes.ContainsKey(logScope.Id))
            {
                var testRunUuid = OrangebeardAddIn.TestrunUuid;
                var client = OrangebeardAddIn.Client;
                //TODO?~ In the original code, EndTime is set to logScope.EndTime.Value
                var status = _nestedStepStatusMap[logScope.Status];
                var finishTestItem = new FinishTestItem(testRunUuid.Value, status);
                Guid testItem = OrangebeardAddIn.LogScopes[logScope.Id];
                client.FinishTestItem(testItem, finishTestItem);

                OrangebeardAddIn.LogScopes.TryRemove(logScope.Id, out Guid _);
            }
            else
            {
                //TODO?+ _traceLogger.Warn($"Unknown current step context to end log scope with `{logScope.Id}` ID.");
            }
        }

        private static readonly AsyncLocal<ScenarioStepContext> _activeStepContext = new AsyncLocal<ScenarioStepContext>();

        public static ScenarioStepContext ActiveStepContext
        {
            get
            {
                return _activeStepContext.Value;
            }
            set
            {
                _activeStepContext.Value = value;
            }
        }

        private static readonly AsyncLocal<ScenarioContext> _activeScenarioContext = new AsyncLocal<ScenarioContext>();

        public static ScenarioContext ActiveScenarioContext
        {
            get
            {
                return _activeScenarioContext.Value;
            }
            set
            {
                _activeScenarioContext.Value = value;
            }
        }

        private static readonly AsyncLocal<FeatureContext> _activeFeatureContext = new AsyncLocal<FeatureContext>();

        public static FeatureContext ActiveFeatureContext
        {
            get
            {
                return _activeFeatureContext.Value;
            }
            set
            {
                _activeFeatureContext.Value = value;
            }
        }

        //TODO?~ Was a private instance method. Should be a general translate function... not something every class can modify, but still something everyone could use for lookup.
        public static Dictionary<LogScopeStatus, Status> _nestedStepStatusMap = new Dictionary<LogScopeStatus, Status> {
            { LogScopeStatus.InProgress, Status.IN_PROGRESS },
            { LogScopeStatus.Passed, Status.PASSED },
            { LogScopeStatus.Failed, Status.FAILED },
            { LogScopeStatus.Skipped,Status.SKIPPED }
        };
    }
}
