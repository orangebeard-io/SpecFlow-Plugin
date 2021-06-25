using Orangebeard.Client.Abstractions;
using Orangebeard.Client.Abstractions.Requests;
using Orangebeard.Shared.Reporter;
using System;
using TechTalk.SpecFlow;

namespace Orangebeard.SpecFlowPlugin.EventArguments
{
    public class TestItemStartedEventArgs : EventArgs
    {
        public TestItemStartedEventArgs(IClientService service, StartTestItemRequest request)
        {
            Service = service;
            StartTestItemRequest = request;
        }

        public TestItemStartedEventArgs(IClientService service, StartTestItemRequest request, ITestReporter testReporter)
            : this(service, request)
        {
            TestReporter = testReporter;
        }

        public TestItemStartedEventArgs(IClientService service, StartTestItemRequest request, ITestReporter testReporter, FeatureContext featureContext, ScenarioContext scenarioContext)
            : this(service, request, testReporter)
        {
            this.FeatureContext = featureContext;
            this.ScenarioContext = scenarioContext;
        }

        public IClientService Service { get; }

        public StartTestItemRequest StartTestItemRequest { get; }

        public ITestReporter TestReporter { get; }

        public FeatureContext FeatureContext { get; }

        public ScenarioContext ScenarioContext { get; }

        public bool Canceled { get; set;}
    }
}
