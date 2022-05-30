using Orangebeard.Client;
using Orangebeard.Client.Entities;
using System;
using TechTalk.SpecFlow;

namespace Orangebeard.SpecFlowPlugin.EventArguments
{
    public class StepStartedEventArgs : EventArgs
    {
        public StepStartedEventArgs(OrangebeardV2Client client, StartTestItem startTestItem)
        {
            Client = client;
            StartTestItemObject = startTestItem;
        }

        public StepStartedEventArgs(OrangebeardV2Client client, StartTestItem startTestItem, Guid testUuid)
            : this(client, startTestItem)
        {
            TestUuid = testUuid;
        }

        public StepStartedEventArgs(OrangebeardV2Client service, StartTestItem request, Guid testReporter, FeatureContext featureContext, ScenarioContext scenarioContext, ScenarioStepContext stepContext)
            : this(service, request, testReporter)
        {
            FeatureContext = featureContext;
            ScenarioContext = scenarioContext;
            StepContext = stepContext;
        }

        public OrangebeardV2Client Client { get; }

        public StartTestItem StartTestItemObject { get; }

        public Guid TestUuid { get; }

        public FeatureContext FeatureContext { get; }

        public ScenarioContext ScenarioContext { get; }

        public ScenarioStepContext StepContext { get; }

        public bool Canceled { get; set;}
    }
}
