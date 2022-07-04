using Orangebeard.Client;
using Orangebeard.Client.Entities;
using Orangebeard.SpecFlowPlugin.Extensions;
using System;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Infrastructure;

namespace Orangebeard.SpecFlowPlugin
{
    public class SkippedStepsHandler : ISkippedStepHandler
    {
        public void Handle(ScenarioContext scenarioContext)
        {
            var scenarioReporter = OrangebeardAddIn.GetScenarioTestReporter(scenarioContext);
            Guid? testRunUuid = OrangebeardAddIn.TestrunUuid;
            OrangebeardV2Client client = OrangebeardAddIn.Client;

            StartTestItem startTestItem = new StartTestItem(
                testRunUUID: testRunUuid.Value,
                name: scenarioContext.StepContext.StepInfo.GetCaption(),
                type: TestItemType.STEP,
                description: null,
                attributes: null);
            var skippedStepUuid = client.StartTestItem(scenarioReporter, startTestItem);
            // No need to update the Context, since the step is going to be finished immediately with the status "SKIPPED".

            FinishTestItem finishTestItem = new FinishTestItem(testRunUuid.Value, Status.SKIPPED);
            client.FinishTestItem(skippedStepUuid.Value, finishTestItem);
        }
    }
}
