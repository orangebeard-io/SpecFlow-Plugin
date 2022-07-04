using Orangebeard.Client;
using Orangebeard.Client.Entities;
using System;
using TechTalk.SpecFlow;

namespace Orangebeard.SpecFlowPlugin.EventArguments
{
    public class TestItemStartedEventArgs : EventArgs
    {
        public TestItemStartedEventArgs(OrangebeardV2Client client, StartTestItem startTestItem)
        {
            Client = client;
            StartTestItemRequest = startTestItem;
        }

        public TestItemStartedEventArgs(OrangebeardV2Client client, StartTestItem startTestItem, Guid? testUuid)
            : this(client, startTestItem)
        {
            TestUuid = testUuid;
        }

        public TestItemStartedEventArgs(OrangebeardV2Client client, StartTestItem startTestItem, Guid? testUuid, FeatureContext featureContext, ScenarioContext scenarioContext)
            : this(client, startTestItem, testUuid)
        {
            this.FeatureContext = featureContext;
            this.ScenarioContext = scenarioContext;
        }

        public OrangebeardV2Client Client { get; }

        public StartTestItem StartTestItemRequest { get; }

        public Guid? TestUuid { get; }

        public FeatureContext FeatureContext { get; }

        public ScenarioContext ScenarioContext { get; }

        public bool Canceled { get; set;}
    }
}
