using Orangebeard.Client.Abstractions.Models;
using Orangebeard.Client.Abstractions.Requests;
using Orangebeard.Client.Entities;
using Orangebeard.Shared.Execution.Logging;
using Orangebeard.Shared.Extensibility;
using Orangebeard.Shared.Extensibility.Commands;
using Orangebeard.Shared.Internal.Logging;
using Orangebeard.Shared.Reporter;
using System;
using System.Collections.Generic;
using System.Threading;
using TechTalk.SpecFlow;

namespace Orangebeard.SpecFlowPlugin.LogHandler
{
    public class ContextAwareLogHandler : ICommandsListener
    {
        private readonly ITraceLogger _traceLogger = TraceLogManager.Instance.GetLogger<ContextAwareLogHandler>();

        public void Initialize(ICommandsSource commandsSource)
        {
            commandsSource.OnBeginLogScopeCommand += CommandsSource_OnBeginLogScopeCommand;
            commandsSource.OnEndLogScopeCommand += CommandsSource_OnEndLogScopeCommand;
            commandsSource.OnLogMessageCommand += CommandsSource_OnLogMessageCommand;
        }

        private void CommandsSource_OnLogMessageCommand(Orangebeard.Shared.Execution.ILogContext logContext, Orangebeard.Shared.Extensibility.Commands.CommandArgs.LogMessageCommandArgs args)
        {
            var logScope = args.LogScope;

            Guid? testReporter;

            if (logScope != null && OrangebeardAddIn.LogScopes.ContainsKey(logScope.Id))
            {
                testReporter = OrangebeardAddIn.LogScopes[logScope.Id];
            }
            else
            {
                // TODO: investigate SpecFlow how to understand current scenario context
                testReporter = GetCurrentTestReporter();
            }

            if (testReporter != null)
            {
                testReporter.Log(args.LogMessage.ConvertToRequest());
            }
            else
            {
                _traceLogger.Warn("Unknown current context to log message.");
            }
        }

        private void CommandsSource_OnBeginLogScopeCommand(Orangebeard.Shared.Execution.ILogContext logContext, Orangebeard.Shared.Extensibility.Commands.CommandArgs.LogScopeCommandArgs args)
        {
            var logScope = args.LogScope;

            var startRequest = new StartTestItemRequest
            {
                Name = logScope.Name,
                StartTime = logScope.BeginTime,
                HasStats = false
            };



            Guid? testReporter = null;

            if (logScope.Parent != null)
            {
                if (OrangebeardAddIn.LogScopes.ContainsKey(logScope.Parent.Id))
                {
                    testReporter = OrangebeardAddIn.LogScopes[logScope.Parent.Id];
                }
            }
            else
            {
                testReporter = GetCurrentTestReporter();
            }

            if (testReporter != null)
            {
                var nestedStep = testReporter.StartChildTestReporter(startRequest);
                OrangebeardAddIn.LogScopes[logScope.Id] = nestedStep;
            }
            else
            {
                _traceLogger.Warn("Unknown current step context to begin new log scope.");
            }
        }

        private void CommandsSource_OnEndLogScopeCommand(Orangebeard.Shared.Execution.ILogContext logContext, Orangebeard.Shared.Extensibility.Commands.CommandArgs.LogScopeCommandArgs args)
        {
            var logScope = args.LogScope;

            var finishRequest = new FinishTestItemRequest
            {
                EndTime = logScope.EndTime.Value,
                Status = _nestedStepStatusMap[logScope.Status]
            };

            if (OrangebeardAddIn.LogScopes.ContainsKey(logScope.Id))
            {
                OrangebeardAddIn.LogScopes[logScope.Id].Finish(finishRequest);
                OrangebeardAddIn.LogScopes.TryRemove(logScope.Id, out ITestReporter _);
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
