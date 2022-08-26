using Orangebeard.Client.Abstractions;
using Orangebeard.Client.Abstractions.Requests;
using Orangebeard.Shared.Reporter;
using System;
using TechTalk.SpecFlow;

namespace Orangebeard.SpecFlowPlugin.EventArguments
{
    public class TestItemFinishedEventArgs: EventArgs
    {
        public TestItemFinishedEventArgs(IClientService service, FinishTestItemRequest request, ITestReporter testReporter)
        {
            Service = service;
            FinishTestItemRequest = request;
            TestReporter = testReporter;
        }

        public TestItemFinishedEventArgs(IClientService service, FinishTestItemRequest request, ITestReporter testReporter, FeatureContext featureContext, ScenarioContext scenarioContext)
            : this(service, request, testReporter)
        {
            FeatureContext = featureContext;
            ScenarioContext = scenarioContext;
        }

        public IClientService Service { get; }

        public FinishTestItemRequest FinishTestItemRequest { get; }

        public ITestReporter TestReporter { get; }

        public FeatureContext FeatureContext { get; }

        public ScenarioContext ScenarioContext { get; }

        public bool Canceled { get; set; }
    }
}
