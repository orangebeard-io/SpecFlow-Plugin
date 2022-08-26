using Orangebeard.Client.Abstractions;
using Orangebeard.Client.Abstractions.Requests;
using Orangebeard.Shared.Reporter;
using System;
using TechTalk.SpecFlow;

namespace Orangebeard.SpecFlowPlugin.EventArguments
{
    public class StepStartedEventArgs : EventArgs
    {
        public StepStartedEventArgs(IClientService service, StartTestItemRequest request)
        {
            Service = service;
            StarTestItemRequest = request;
        }

        public StepStartedEventArgs(IClientService service, StartTestItemRequest request, ITestReporter testReporter)
            : this(service, request)
        {
            TestReporter = testReporter;
        }

        public StepStartedEventArgs(IClientService service, StartTestItemRequest request, ITestReporter testReporter, FeatureContext featureContext, ScenarioContext scenarioContext, ScenarioStepContext stepContext)
            : this(service, request, testReporter)
        {
            FeatureContext = featureContext;
            ScenarioContext = scenarioContext;
            StepContext = stepContext;
        }

        public IClientService Service { get; }

        public StartTestItemRequest StarTestItemRequest { get; }

        public ITestReporter TestReporter { get; }

        public FeatureContext FeatureContext { get; }

        public ScenarioContext ScenarioContext { get; }

        public ScenarioStepContext StepContext { get; }

        public bool Canceled { get; set;}
    }
}
