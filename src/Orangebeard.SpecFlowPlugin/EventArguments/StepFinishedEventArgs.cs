using Orangebeard.Client;
using Orangebeard.Client.Entities;
using System;
using TechTalk.SpecFlow;

namespace Orangebeard.SpecFlowPlugin.EventArguments
{
    public class StepFinishedEventArgs : EventArgs
    {
        public StepFinishedEventArgs(OrangebeardV2Client client, FinishTestItem finishTestItem, Guid testUuid)
        {
            Client = client;
            FinishTestItemObject = finishTestItem;
            TestUuid = testUuid;
        }

        public StepFinishedEventArgs(OrangebeardV2Client client, FinishTestItem finishTestItem, Guid testUuid, FeatureContext featureContext, ScenarioContext scenarioContext, ScenarioStepContext stepContext)
            : this(client, finishTestItem, testUuid)
        {
            FeatureContext = featureContext;
            ScenarioContext = scenarioContext;
            StepContext = stepContext;
        }

        public OrangebeardV2Client Client { get; }

        public FinishTestItem FinishTestItemObject { get; }

        public Guid TestUuid { get; }

        public FeatureContext FeatureContext { get; }

        public ScenarioContext ScenarioContext { get; }

        public ScenarioStepContext StepContext { get; }

        public bool Canceled { get; set; }
    }
}
