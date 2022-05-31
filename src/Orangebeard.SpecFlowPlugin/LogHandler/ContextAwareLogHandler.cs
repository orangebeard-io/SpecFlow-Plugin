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
    public class ContextAwareLogHandler : ICommandsListener
    {
        private readonly ITraceLogger _traceLogger = TraceLogManager.Instance.GetLogger<ContextAwareLogHandler>();

        //private OrangebeardV2Client _client; //TODO!+ Find out where to intialize this one! Maybe make it a public Property.
        //private Guid _testRunUuid; //TODO!+ Find out where to initialize this one! Maybe make it a public Property.

        public void Initialize(ICommandsSource commandsSource)
        {
            //TODO?~ It's unclear if the client and testRunUuid will ever be instantiated properly....
            commandsSource.OnBeginLogScopeCommand += CommandsSource_OnBeginLogScopeCommand;
            commandsSource.OnEndLogScopeCommand += CommandsSource_OnEndLogScopeCommand;
            commandsSource.OnLogMessageCommand += CommandsSource_OnLogMessageCommand;
        }

        private void CommandsSource_OnLogMessageCommand(ClientExecution.ILogContext logContext, ClientExtensibility.Commands.CommandArgs.LogMessageCommandArgs args, OrangebeardV2Client client, Guid? testRunUuid)
        {
            var logScope = args.LogScope;

            Guid? testItemUuid;

            if (logScope != null && OrangebeardAddIn.LogScopes.ContainsKey(logScope.Id))
            {
                testItemUuid = OrangebeardAddIn.LogScopes[logScope.Id];
            }
            else
            {
                // TODO: investigate SpecFlow how to understand current scenario context
                testItemUuid = GetCurrentTestReporter();
            }

            if (testItemUuid != null)
            {
                client.Log(args.LogMessage.ConvertToRequest(testRunUuid.Value, testItemUuid.Value));
            }
            else
            {
                _traceLogger.Warn("Unknown current context to log message.");
            }
        }

        private void CommandsSource_OnBeginLogScopeCommand(ClientExecution.ILogContext logContext, ClientExtensibility.Commands.CommandArgs.LogScopeCommandArgs args, OrangebeardV2Client client, Guid? testRunUuid)
        {
            var logScope = args.LogScope;

            //TODO?~ Original code set the start time to logScope.BeginTime
            var startTestItem = new StartTestItem(
                testRunUUID: testRunUuid.Value,
                name: logScope.Name,
                type: TestItemType.STEP,
                description: null,
                attributes: null
            );

            Guid? testItemUuid = null;

            if (logScope.Parent != null)
            {
                if (OrangebeardAddIn.LogScopes.ContainsKey(logScope.Parent.Id))
                {
                    testItemUuid = OrangebeardAddIn.LogScopes[logScope.Parent.Id];
                }
            }
            else
            {
                testItemUuid = GetCurrentTestReporter();
            }

            if (testItemUuid != null)
            {
                var nestedStep = client.StartTestItem(testItemUuid, startTestItem);
                OrangebeardAddIn.LogScopes[logScope.Id] = nestedStep.Value; //TODO?~ Check if nestedStep == null ?
            }
            else
            {
                _traceLogger.Warn("Unknown current step context to begin new log scope.");
            }
        }

        private void CommandsSource_OnEndLogScopeCommand(ClientExecution.ILogContext logContext, ClientExtensibility.Commands.CommandArgs.LogScopeCommandArgs args, OrangebeardV2Client client, Guid? testRunUuid)
        {
            var logScope = args.LogScope;

            //TODO?~ In the original code, EndTime is set to logScope.EndTime.Value
            var finishTestItem = new FinishTestItem(testRunUuid.Value, _nestedStepStatusMap[logScope.Status]);

            if (OrangebeardAddIn.LogScopes.ContainsKey(logScope.Id))
            {
                Guid testItem = OrangebeardAddIn.LogScopes[logScope.Id];
                client.FinishTestItem(testItem, finishTestItem);

                OrangebeardAddIn.LogScopes.TryRemove(logScope.Id, out Guid _);
            }
            else
            {
                _traceLogger.Warn($"Unknown current step context to end log scope with `{logScope.Id}` ID.");
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

        public Guid? GetCurrentTestReporter()
        {
            var testReporter = OrangebeardAddIn.GetStepTestReporter(ActiveStepContext);

            if (testReporter == null)
            {
                testReporter = OrangebeardAddIn.GetScenarioTestReporter(ActiveScenarioContext);
            }

            if (testReporter == null)
            {
                testReporter = OrangebeardAddIn.GetFeatureTestReporter(ActiveFeatureContext);
            }

            return testReporter;
        }

        private Dictionary<LogScopeStatus, Status> _nestedStepStatusMap = new Dictionary<LogScopeStatus, Status> {
            { LogScopeStatus.InProgress, Status.IN_PROGRESS },
            { LogScopeStatus.Passed, Status.PASSED },
            { LogScopeStatus.Failed, Status.FAILED },
            { LogScopeStatus.Skipped,Status.SKIPPED }
        };
    }
}
