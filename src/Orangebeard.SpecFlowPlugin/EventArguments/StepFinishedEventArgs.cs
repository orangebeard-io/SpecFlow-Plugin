using System;
using Orangebeard.Client.Abstractions;
using Orangebeard.Client.Abstractions.Requests;
using Orangebeard.Shared.Reporter;
using TechTalk.SpecFlow;

namespace Orangebeard.SpecFlowPlugin.EventArguments
{
    public class StepFinishedEventArgs : EventArgs
    {
        public StepFinishedEventArgs(IClientService service, FinishTestItemRequest request, ITestReporter testReporter)
        {
            Service = service;
            FinishTestItemRequest = request;
            TestReporter = testReporter;
        }

        public StepFinishedEventArgs(IClientService service, FinishTestItemRequest request, ITestReporter testReporter, FeatureContext featureContext, ScenarioContext scenarioContext, ScenarioStepContext stepContext)
            : this(service, request, testReporter)
        {
            FeatureContext = featureContext;
            ScenarioContext = scenarioContext;
            StepContext = stepContext;
        }

        public IClientService Service { get; }

        public FinishTestItemRequest FinishTestItemRequest { get; }

        public ITestReporter TestReporter { get; }

        public FeatureContext FeatureContext { get; }

        public ScenarioContext ScenarioContext { get; }

        public ScenarioStepContext StepContext { get; }

        public bool Canceled { get; set; }
    }
}
