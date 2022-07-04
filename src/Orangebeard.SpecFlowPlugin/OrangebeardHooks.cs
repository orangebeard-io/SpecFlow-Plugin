using Orangebeard.Client;
using Orangebeard.Client.Entities;
using Orangebeard.Client.OrangebeardProperties;
using Orangebeard.Shared.Configuration;
using Orangebeard.Shared.Internal.Logging;
using Orangebeard.SpecFlowPlugin.EventArguments;
using Orangebeard.SpecFlowPlugin.Extensions;
using Orangebeard.SpecFlowPlugin.LogHandler;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using TechTalk.SpecFlow;
using Attribute = Orangebeard.Client.Entities.Attribute;

namespace Orangebeard.SpecFlowPlugin
{
    [Binding]
    internal class OrangebeardHooks : Steps
    {
        private static readonly ITraceLogger _traceLogger = TraceLogManager.Instance.GetLogger<OrangebeardHooks>();

        private static OrangebeardV2Client _client;
        private static Guid? _testRunUuid;

        private static IConfiguration Initialize()
        {
            var args = new InitializingEventArgs(Plugin.Config);

            OrangebeardAddIn.OnInitializing(typeof(OrangebeardHooks), args);

            if (args.Client != null)
            {
                _client = args.Client as OrangebeardV2Client;
            }
            else
            {
                var orangebeardConfig = new OrangebeardConfiguration(Plugin.Config).WithListenerIdentification(
                            "SpecFlow Plugin/" +
                            typeof(OrangebeardHooks).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion
                            );
                _client = new OrangebeardV2Client(orangebeardConfig, true);
            }

            OrangebeardAddIn.Client = _client;

            return args.Config;
        }

        [BeforeTestRun(Order = -20000)]
        public static void BeforeTestRun()
        {
            try
            {
                var config = Initialize();

                string name = config.GetValue(ConfigurationPath.TestSetName, "SpecFlow Launch");
                var attributes = config.GetKeyValues("TestSet:Attributes", new List<KeyValuePair<string, string>>()).Select(a => new Attribute(a.Key, a.Value));
                string description = config.GetValue(ConfigurationPath.TestSetDescription, string.Empty);
                var startRequest = new StartTestRun(name, description, new HashSet<Attribute>(attributes));

                var eventArg = new RunStartedEventArgs(_client, startRequest);
                OrangebeardAddIn.OnBeforeRunStarted(null, eventArg);

                if (eventArg.TestRunUuid != null && eventArg.TestRunUuid != Guid.Empty)
                {
                    _testRunUuid = eventArg.TestRunUuid;
                }

                if (!eventArg.Canceled)
                {
                    if (_testRunUuid == null || _testRunUuid == Guid.Empty)
                    {
                        _testRunUuid = _client.StartTestRun(startRequest);
                    }

                    OrangebeardAddIn.TestrunUuid = _testRunUuid;
                    if (_testRunUuid == null)
                    {
                        _traceLogger.Error("Test run failed to start!");
                    }
                    else
                    {
                        Context.Current = new NewTestContext(null, _testRunUuid.Value);

                        OrangebeardAddIn.OnAfterRunStarted(null, new RunStartedEventArgs(_client, startRequest, _testRunUuid.Value));
                    }

                }

                OrangebeardAddIn.TestrunUuid = _testRunUuid;
            }
            catch (Exception exp)
            {
                _traceLogger.Error(exp.ToString());
            }
        }
       
        [AfterTestRun(Order = 20000)]
        public static void AfterTestRun()
        {
            try
            {
                if (_testRunUuid != null)
                {
                    var finishTestRun = new FinishTestRun();

                    var eventArg = new RunFinishedEventArgs(_client, finishTestRun, _testRunUuid.Value);
                    OrangebeardAddIn.OnBeforeRunFinished(null, eventArg);

                    if (!eventArg.Canceled)
                    {
                        _client.FinishTestRun(_testRunUuid.Value, finishTestRun);
                        Context.Current = Context.Current.Parent;

                        var sw = Stopwatch.StartNew();

                        _traceLogger.Info($"Finishing Orangebeard Run...");
                        _traceLogger.Info($"Elapsed: {sw.Elapsed}");

                        OrangebeardAddIn.OnAfterRunFinished(null, new RunFinishedEventArgs(_client, finishTestRun, _testRunUuid.Value));
                    }
                }
            }
            catch (Exception exp)
            {
                _traceLogger.Error(exp.ToString());
            }
        }

        [BeforeFeature(Order = -20000)]
        public static void BeforeFeature(FeatureContext featureContext)
        {
            try
            {
                if (_testRunUuid != null)
                {
                    ContextAwareLogHandler.ActiveFeatureContext = featureContext;

                    lock (LockHelper.GetLock(FeatureInfoEqualityComparer.GetFeatureInfoHashCode(featureContext.FeatureInfo)))
                    {
                        var currentFeature = OrangebeardAddIn.GetFeatureTestReporter(featureContext);

                        if (currentFeature == null)
                        {
                            var startTestItem = new StartTestItem(
                                testRunUUID: _testRunUuid.Value,
                                name: featureContext.FeatureInfo.Title,
                                type: TestItemType.SUITE,
                                description: featureContext.FeatureInfo.Description,
                                attributes: new HashSet<Attribute>(featureContext.FeatureInfo.Tags?.Select(t => new Attribute( value: t )))
                            );

                            var eventArg = new TestItemStartedEventArgs(_client, startTestItem, null, featureContext, null);
                            OrangebeardAddIn.OnBeforeFeatureStarted(null, eventArg);

                            if (!eventArg.Canceled)
                            {
                                currentFeature = _client.StartTestItem(null, startTestItem);
                                Context.Current = new NewTestContext(Context.Current, currentFeature.Value);
                                OrangebeardAddIn.SetFeatureTestReporter(featureContext, currentFeature.Value);
                                OrangebeardAddIn.OnAfterFeatureStarted(null, new TestItemStartedEventArgs(_client, startTestItem, currentFeature, featureContext, null));
                            }
                        }
                        else
                        {
                            OrangebeardAddIn.IncrementFeatureThreadCount(featureContext);
                        }
                    }
                }
            }
            catch (Exception exp)
            {
                _traceLogger.Error(exp.ToString());
            }
        }

        [AfterFeature(Order = 20000)]
        public static void AfterFeature(FeatureContext featureContext)
        {
            try
            {
                lock (LockHelper.GetLock(FeatureInfoEqualityComparer.GetFeatureInfoHashCode(featureContext.FeatureInfo)))
                {
                    var currentFeature = OrangebeardAddIn.GetFeatureTestReporter(featureContext);
                    var remainingThreadCount = OrangebeardAddIn.DecrementFeatureThreadCount(featureContext);

                    if (currentFeature != null && remainingThreadCount == 0)
                    {
                        var finishTestItem = new FinishTestItem(_testRunUuid.Value, Status.SKIPPED);

                        var eventArg = new TestItemFinishedEventArgs(_client, finishTestItem, currentFeature.Value, featureContext, null);
                        OrangebeardAddIn.OnBeforeFeatureFinished(null, eventArg);

                        if (!eventArg.Canceled)
                        {
                            Context.Current = Context.Current.Parent;
                            _client.FinishTestItem(_testRunUuid.Value, finishTestItem);

                            OrangebeardAddIn.OnAfterFeatureFinished(null, new TestItemFinishedEventArgs(_client, finishTestItem, currentFeature.Value, featureContext, null));
                        }

                        OrangebeardAddIn.RemoveFeatureTestReporter(featureContext, currentFeature.Value);
                    }
                }
            }
            catch (Exception exp)
            {
                _traceLogger.Error(exp.ToString());
            }
            finally
            {
                ContextAwareLogHandler.ActiveFeatureContext = null;
            }
        }

        [BeforeScenario(Order = -20000)]
        public void BeforeScenario()
        {
            try
            {
                ContextAwareLogHandler.ActiveScenarioContext = this.ScenarioContext;

                var currentFeature = OrangebeardAddIn.GetFeatureTestReporter(this.FeatureContext);

                if (currentFeature != null)
                {
                    string description = DetermineDescription();

                    // In the original code, "BeforeScenario" starts a Step. But in our system, it is called for "Add two numbers",  which is a TestItemType.TEST .
                    // However, "Add two numbers" IS a Scenario.
                    var startTestItem = new StartTestItem(
                        testRunUUID: _testRunUuid.Value,
                        name: this.ScenarioContext.ScenarioInfo.Title,
                        type: TestItemType.TEST, // WAS: TestItemType.STEP 
                        description: description,
                        attributes: new HashSet<Attribute>(this.ScenarioContext.ScenarioInfo.Tags?.Select(tag => new Attribute(value: tag)))
                    );

                    var eventArg = new TestItemStartedEventArgs(_client, startTestItem, currentFeature, this.FeatureContext, this.ScenarioContext);
                    OrangebeardAddIn.OnBeforeScenarioStarted(this, eventArg);

                    if (!eventArg.Canceled)
                    {
                        var currentScenario = _client.StartTestItem(currentFeature, startTestItem);
                        Context.Current = new NewTestContext(Context.Current, currentScenario.Value);
                        OrangebeardAddIn.SetScenarioTestReporter(this.ScenarioContext, currentScenario.Value);
                        OrangebeardAddIn.OnAfterScenarioStarted(this, new TestItemStartedEventArgs(_client, startTestItem, currentFeature, this.FeatureContext, this.ScenarioContext));
                    }
                }
            }
            catch (Exception exp)
            {
                _traceLogger.Error(exp.ToString());
            }
        }

        private string DetermineDescription()
        {
            string description = this.ScenarioContext.ScenarioInfo.Description;

            // fetch scenario parameters (from Examples block)
            var arguments = this.ScenarioContext.ScenarioInfo.Arguments;
            if (arguments != null && arguments.Count > 0)
            {
                var parameters = new List<KeyValuePair<string, string>>();

                foreach (DictionaryEntry argument in arguments)
                {
                    parameters.Add(new KeyValuePair<string, string>
                    (
                        argument.Key.ToString(),
                        argument.Value.ToString()
                    ));
                }

                // append scenario outline parameters to description
                var parametersInfo = new StringBuilder();
                parametersInfo.Append("|");
                foreach (var p in parameters)
                {
                    parametersInfo.Append(p.Key);

                    parametersInfo.Append("|");
                }

                parametersInfo.AppendLine();
                parametersInfo.Append("|");
                foreach (var p in parameters)
                {
                    parametersInfo.Append("---");
                    parametersInfo.Append("|");
                }

                parametersInfo.AppendLine();
                parametersInfo.Append("|");
                foreach (var p in parameters)
                {
                    parametersInfo.Append("**");
                    parametersInfo.Append(p.Value);
                    parametersInfo.Append("**");

                    parametersInfo.Append("|");
                }

                if (string.IsNullOrEmpty(description))
                {
                    description = parametersInfo.ToString();
                }
                else
                {
                    description = parametersInfo.ToString() + Environment.NewLine + Environment.NewLine + description;
                }

            }

            return description;
        }

        [AfterScenario(Order = 20000)]
        public void AfterScenario()
        {
            try
            {
                var currentScenario = OrangebeardAddIn.GetScenarioTestReporter(this.ScenarioContext);

                if (currentScenario != null)
                {
                    // Workaround: error messages are usually stack traces, which don't display nicely in markdown.
                    //  For this reason, when the log is at the level of an error, we display in plain text instead of markdown.

                    if (this.ScenarioContext.ScenarioExecutionStatus == ScenarioExecutionStatus.TestError)
                    {
                        var log = new Log(_testRunUuid.Value, currentScenario.Value, LogLevel.error, this.ScenarioContext.TestError?.ToString(), LogFormat.PLAIN_TEXT);
                        _client.Log(log);

                    }
                    else if (this.ScenarioContext.ScenarioExecutionStatus == ScenarioExecutionStatus.BindingError)
                    {
                        var log = new Log(_testRunUuid.Value, currentScenario.Value, LogLevel.error, this.ScenarioContext.TestError?.Message, LogFormat.PLAIN_TEXT);
                        _client.Log(log);
                    }
                    else if (this.ScenarioContext.ScenarioExecutionStatus == ScenarioExecutionStatus.UndefinedStep)
                    {
                        var log = new Log(_testRunUuid.Value, currentScenario.Value, LogLevel.error, new MissingStepDefinitionException().Message, LogFormat.PLAIN_TEXT);
                        _client.Log(log);
                    }

                    var status = this.ScenarioContext.ScenarioExecutionStatus == ScenarioExecutionStatus.OK ? Status.PASSED : Status.FAILED;

                    var finishTestItem = new FinishTestItem(_testRunUuid.Value, status);

                    var eventArg = new TestItemFinishedEventArgs(_client, finishTestItem, currentScenario.Value, this.FeatureContext, this.ScenarioContext);
                    OrangebeardAddIn.OnBeforeScenarioFinished(this, eventArg);

                    if (!eventArg.Canceled)
                    {
                        Context.Current = Context.Current.Parent;
                        _client.FinishTestItem(currentScenario.Value, finishTestItem);
                        OrangebeardAddIn.OnAfterScenarioFinished(this, new TestItemFinishedEventArgs(_client, finishTestItem, currentScenario.Value, this.FeatureContext, this.ScenarioContext));
                        OrangebeardAddIn.RemoveScenarioTestReporter(this.ScenarioContext, currentScenario.Value);
                    }
                }
            }
            catch (Exception exp)
            {
                _traceLogger.Error(exp.ToString());
            }
            finally
            {
                ContextAwareLogHandler.ActiveScenarioContext = null;
            }
        }

        [BeforeStep(Order = -20000)]
        public void BeforeStep()
        {
            try
            {
                ContextAwareLogHandler.ActiveStepContext = this.StepContext;

                var currentScenario = OrangebeardAddIn.GetScenarioTestReporter(this.ScenarioContext);

                var stepInfo = new StartTestItem(_testRunUuid.Value, this.StepContext.StepInfo.GetCaption(), TestItemType.STEP, description: null, attributes: null);

                var eventArg = new StepStartedEventArgs(_client, stepInfo, currentScenario.Value, this.FeatureContext, this.ScenarioContext, this.StepContext);
                OrangebeardAddIn.OnBeforeStepStarted(this, eventArg);

                if (!eventArg.Canceled)
                {
                    var stepUuid = _client.StartTestItem(currentScenario.Value, stepInfo);
                    Context.Current = new NewTestContext(Context.Current, stepUuid.Value);
                    OrangebeardAddIn.SetStepTestReporter(this.StepContext, stepUuid.Value);

                    // step parameters
                    var formattedParameters = this.StepContext.StepInfo.GetFormattedParameters();
                    if (!string.IsNullOrEmpty(formattedParameters))
                    {
                        var log = new Log(_testRunUuid.Value, stepUuid.Value, LogLevel.info, formattedParameters, LogFormat.MARKDOWN);
                        _client.Log(log);
                    }

                    OrangebeardAddIn.OnAfterStepStarted(this, eventArg);
                }
            }
            catch (Exception exp)
            {
                _traceLogger.Error(exp.ToString());
            }
        }

        [AfterStep(Order = 20000)]
        public void AfterStep()
        {
            try
            {
                var currentStep = OrangebeardAddIn.GetStepTestReporter(this.StepContext);

                var stepResult = Status.PASSED;
                if (this.ScenarioContext.ScenarioExecutionStatus == ScenarioExecutionStatus.TestError)
                {
                    stepResult = Status.FAILED;
                }
                var finishStepItem = new FinishTestItem(_testRunUuid.Value, stepResult);


                var eventArg = new StepFinishedEventArgs(_client, finishStepItem, currentStep.Value, this.FeatureContext, this.ScenarioContext, this.StepContext);
                OrangebeardAddIn.OnBeforeStepFinished(this, eventArg);

                if (!eventArg.Canceled)
                {
                    Context.Current = Context.Current.Parent;

                    _client.FinishTestItem(currentStep.Value, finishStepItem);
                    OrangebeardAddIn.RemoveStepTestReporter(this.StepContext, currentStep.Value);
                    OrangebeardAddIn.OnAfterStepFinished(this, eventArg);
                }
            }
            catch (Exception exp)
            {
                _traceLogger.Error(exp.ToString());
            }
            finally
            {
                ContextAwareLogHandler.ActiveStepContext = null;
            }
        }
    }
}
