using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using Orangebeard.Client;
using Orangebeard.Client.Abstractions;
using Orangebeard.Client.Abstractions.Models;
using Orangebeard.Client.Abstractions.Requests;
using Orangebeard.Client.OrangebeardProperties;
using Orangebeard.Shared.Configuration;
using Orangebeard.Shared.Internal.Logging;
using Orangebeard.Shared.Reporter;
using Orangebeard.SpecFlowPlugin.EventArguments;
using Orangebeard.SpecFlowPlugin.Extensions;
using Orangebeard.SpecFlowPlugin.LogHandler;
using TechTalk.SpecFlow;

namespace Orangebeard.SpecFlowPlugin
{
    [Binding]
    internal class OrangebeardHooks : Steps
    {
        private static readonly ITraceLogger _traceLogger = TraceLogManager.Instance.GetLogger<OrangebeardHooks>();

        private static IClientService _service;
        private static ILaunchReporter _launchReporter;

        [BeforeTestRun(Order = -20000)]
        public static void BeforeTestRun()
        {
            try
            {
                var config = Initialize();

                var request = new StartLaunchRequest
                {
                    Name = config.GetValue(ConfigurationPath.TestSetName, "SpecFlow Launch"),
                    StartTime = DateTime.UtcNow
                };
               

                request.Attributes = config.GetKeyValues("TestSet:Attributes", new List<KeyValuePair<string, string>>()).Select(a => new ItemAttribute { Key = a.Key, Value = a.Value }).ToList();
                request.Description = config.GetValue(ConfigurationPath.TestSetDescription, string.Empty);

                var eventArg = new RunStartedEventArgs(_service, request);
                OrangebeardAddIn.OnBeforeRunStarted(null, eventArg);

                if (eventArg.LaunchReporter != null)
                {
                    _launchReporter = eventArg.LaunchReporter;
                }

                if (!eventArg.Canceled)
                {
                    _launchReporter = _launchReporter ?? new LaunchReporter(_service, config, null, Orangebeard.Shared.Extensibility.ExtensionManager.Instance);
                    
                    _launchReporter.Start(request);

                    OrangebeardAddIn.OnAfterRunStarted(null, new RunStartedEventArgs(_service, request, _launchReporter));

                }
            }
            catch (Exception exp)
            {
                _traceLogger.Error(exp.ToString());
            }
        }

        private static IConfiguration Initialize()
        {
           // IConfigurationBuilder _configBuilder new ConfigurationBuilder().AddJsonFile(jsonPath).AddEnvironmentVariables();
           // IConfiguration _configuration;
           // OrangebeardConfiguration _config;
           // OrangebeardClient _orangebeard;

            var args = new InitializingEventArgs(Plugin.Config);

            OrangebeardAddIn.OnInitializing(typeof(OrangebeardHooks), args);

            var uri = Plugin.Config.GetValue<string>(ConfigurationPath.ServerUrl);
            var project = Plugin.Config.GetValue<string>(ConfigurationPath.ServerProject); ;
            var uuid = Plugin.Config.GetValue<string>(ConfigurationPath.ServerAuthenticationUuid); ;

            if (args.Service != null)
            {
                _service = args.Service as OrangebeardClient;
            }
            else
            {
                var orangebeardConfig = new OrangebeardConfiguration(Plugin.Config).WithListenerIdentification(
                            "SpecFlow Plugin/" +
                            typeof(OrangebeardHooks).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion
                            );
                _service = new OrangebeardClient(orangebeardConfig);
            }

            return args.Config;
        }

        [AfterTestRun(Order = 20000)]
        public static void AfterTestRun()
        {
            try
            {
                if (_launchReporter != null)
                {
                    var request = new FinishLaunchRequest
                    {
                        EndTime = DateTime.UtcNow
                    };

                    var eventArg = new RunFinishedEventArgs(_service, request, _launchReporter);
                    OrangebeardAddIn.OnBeforeRunFinished(null, eventArg);

                    if (!eventArg.Canceled)
                    {
                        _launchReporter.Finish(request);

                        var sw = Stopwatch.StartNew();

                        _traceLogger.Info($"Finishing Oramngebeard Run...");
                        _launchReporter.Sync();
                        _traceLogger.Info($"Elapsed: {sw.Elapsed}");

                        OrangebeardAddIn.OnAfterRunFinished(null, new RunFinishedEventArgs(_service, request, _launchReporter));
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
                if (_launchReporter != null)
                {
                    ContextAwareLogHandler.ActiveFeatureContext = featureContext;

                    lock (LockHelper.GetLock(FeatureInfoEqualityComparer.GetFeatureInfoHashCode(featureContext.FeatureInfo)))
                    {
                        var currentFeature = OrangebeardAddIn.GetFeatureTestReporter(featureContext);

                        if (currentFeature == null || currentFeature.FinishTask != null)
                        {
                            var request = new StartTestItemRequest
                            {
                                Name = featureContext.FeatureInfo.Title,
                                Description = featureContext.FeatureInfo.Description,
                                StartTime = DateTime.UtcNow,
                                Type = TestItemType.Suite,
                                Attributes = featureContext.FeatureInfo.Tags?.Select(t => new ItemAttribute { Value = t }).ToList()
                            };

                            var eventArg = new TestItemStartedEventArgs(_service, request, null, featureContext, null);
                            OrangebeardAddIn.OnBeforeFeatureStarted(null, eventArg);

                            if (!eventArg.Canceled)
                            {
                                currentFeature = _launchReporter.StartChildTestReporter(request);
                                OrangebeardAddIn.SetFeatureTestReporter(featureContext, currentFeature);

                                OrangebeardAddIn.OnAfterFeatureStarted(null, new TestItemStartedEventArgs(_service, request, currentFeature, featureContext, null));
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

                    if (currentFeature != null && currentFeature.FinishTask == null && remainingThreadCount == 0)
                    {
                        var request = new FinishTestItemRequest
                        {
                            EndTime = DateTime.UtcNow,
                            Status = Status.Skipped
                        };

                        var eventArg = new TestItemFinishedEventArgs(_service, request, currentFeature, featureContext, null);
                        OrangebeardAddIn.OnBeforeFeatureFinished(null, eventArg);

                        if (!eventArg.Canceled)
                        {
                            currentFeature.Finish(request);

                            OrangebeardAddIn.OnAfterFeatureFinished(null, new TestItemFinishedEventArgs(_service, request, currentFeature, featureContext, null));
                        }

                        OrangebeardAddIn.RemoveFeatureTestReporter(featureContext, currentFeature);
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
                    var request = new StartTestItemRequest
                    {
                        Name = this.ScenarioContext.ScenarioInfo.Title,
                        Description = this.ScenarioContext.ScenarioInfo.Description,
                        StartTime = DateTime.UtcNow,
                        Type = TestItemType.Step,
                        Attributes = this.ScenarioContext.ScenarioInfo.Tags?.Select(tag => new ItemAttribute { Value = tag }).ToList()
                    };

                    // fetch scenario parameters (from Examples block)
                    var arguments = this.ScenarioContext.ScenarioInfo.Arguments;
                    if (arguments != null && arguments.Count > 0)
                    {
                        request.Parameters = new List<KeyValuePair<string, string>>();

                        foreach (DictionaryEntry argument in arguments)
                        {
                            request.Parameters.Add(new KeyValuePair<string, string>
                            (
                                argument.Key.ToString(),
                                argument.Value.ToString()
                            ));
                        }

                        // append scenario outline parameters to description
                        var parametersInfo = new StringBuilder();
                        parametersInfo.Append("|");
                        foreach (var p in request.Parameters)
                        {
                            parametersInfo.Append(p.Key);

                            parametersInfo.Append("|");
                        }

                        parametersInfo.AppendLine();
                        parametersInfo.Append("|");
                        foreach (var p in request.Parameters)
                        {
                            parametersInfo.Append("---");
                            parametersInfo.Append("|");
                        }

                        parametersInfo.AppendLine();
                        parametersInfo.Append("|");
                        foreach (var p in request.Parameters)
                        {
                            parametersInfo.Append("**");
                            parametersInfo.Append(p.Value);
                            parametersInfo.Append("**");

                            parametersInfo.Append("|");
                        }

                        if (string.IsNullOrEmpty(request.Description))
                        {
                            request.Description = parametersInfo.ToString();
                        }
                        else
                        {
                            request.Description = parametersInfo.ToString() + Environment.NewLine + Environment.NewLine + request.Description;
                        }
                    }

                    var eventArg = new TestItemStartedEventArgs(_service, request, currentFeature, this.FeatureContext, this.ScenarioContext);
                    OrangebeardAddIn.OnBeforeScenarioStarted(this, eventArg);

                    if (!eventArg.Canceled)
                    {
                        var currentScenario = currentFeature.StartChildTestReporter(request);
                        OrangebeardAddIn.SetScenarioTestReporter(this.ScenarioContext, currentScenario);

                        OrangebeardAddIn.OnAfterScenarioStarted(this, new TestItemStartedEventArgs(_service, request, currentFeature, this.FeatureContext, this.ScenarioContext));
                    }
                }
            }
            catch (Exception exp)
            {
                _traceLogger.Error(exp.ToString());
            }
        }

        [AfterScenario(Order = 20000)]
        public void AfterScenario()
        {
            try
            {
                var currentScenario = OrangebeardAddIn.GetScenarioTestReporter(this.ScenarioContext);

                if (currentScenario != null)
                {
                    if (this.ScenarioContext.ScenarioExecutionStatus == ScenarioExecutionStatus.TestError)
                    {
                        currentScenario.Log(new CreateLogItemRequest
                        {
                            Level = LogLevel.Error,
                            Time = DateTime.UtcNow,
                            Text = this.ScenarioContext.TestError?.ToString()
                        });
                    }
                    else if (this.ScenarioContext.ScenarioExecutionStatus == ScenarioExecutionStatus.BindingError)
                    {
                        currentScenario.Log(new CreateLogItemRequest
                        {
                            Level = LogLevel.Error,
                            Time = DateTime.UtcNow,
                            Text = this.ScenarioContext.TestError?.Message
                        });
                    }
                    else if (this.ScenarioContext.ScenarioExecutionStatus == ScenarioExecutionStatus.UndefinedStep)
                    {
                        currentScenario.Log(new CreateLogItemRequest
                        {
                            Level = LogLevel.Error,
                            Time = DateTime.UtcNow,
                            Text = new MissingStepDefinitionException().Message
                        });
                    }

                    var status = this.ScenarioContext.ScenarioExecutionStatus == ScenarioExecutionStatus.OK ? Status.Passed : Status.Failed;

                    var request = new FinishTestItemRequest
                    {
                        EndTime = DateTime.UtcNow,
                        Status = status
                    };

                    var eventArg = new TestItemFinishedEventArgs(_service, request, currentScenario, this.FeatureContext, this.ScenarioContext);
                    OrangebeardAddIn.OnBeforeScenarioFinished(this, eventArg);

                    if (!eventArg.Canceled)
                    {
                        currentScenario.Finish(request);

                        OrangebeardAddIn.OnAfterScenarioFinished(this, new TestItemFinishedEventArgs(_service, request, currentScenario, this.FeatureContext, this.ScenarioContext));

                        OrangebeardAddIn.RemoveScenarioTestReporter(this.ScenarioContext, currentScenario);
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

                var stepInfoRequest = new StartTestItemRequest
                {
                    Name = this.StepContext.StepInfo.GetCaption(),
                    StartTime = DateTime.UtcNow,
                    HasStats = false
                };

                var eventArg = new StepStartedEventArgs(_service, stepInfoRequest, currentScenario, this.FeatureContext, this.ScenarioContext, this.StepContext);
                OrangebeardAddIn.OnBeforeStepStarted(this, eventArg);

                if (!eventArg.Canceled)
                {
                    var stepReporter = currentScenario.StartChildTestReporter(stepInfoRequest);
                    OrangebeardAddIn.SetStepTestReporter(this.StepContext, stepReporter);

                    // step parameters
                    var formattedParameters = this.StepContext.StepInfo.GetFormattedParameters();
                    if (!string.IsNullOrEmpty(formattedParameters))
                    {
                        stepReporter.Log(new CreateLogItemRequest
                        {
                            Text = formattedParameters,
                            Level = LogLevel.Info,
                            Time = DateTime.UtcNow,
                            Format = LogFormat.MARKDOWN
                        });
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

                var stepFinishRequest = new FinishTestItemRequest
                {
                    EndTime = DateTime.UtcNow
                };

                if (this.ScenarioContext.ScenarioExecutionStatus == ScenarioExecutionStatus.TestError)
                {
                    stepFinishRequest.Status = Status.Failed;
                }

                var eventArg = new StepFinishedEventArgs(_service, stepFinishRequest, currentStep, this.FeatureContext, this.ScenarioContext, this.StepContext);
                OrangebeardAddIn.OnBeforeStepFinished(this, eventArg);

                if (!eventArg.Canceled)
                {
                    currentStep.Finish(stepFinishRequest);
                    OrangebeardAddIn.RemoveStepTestReporter(this.StepContext, currentStep);
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
