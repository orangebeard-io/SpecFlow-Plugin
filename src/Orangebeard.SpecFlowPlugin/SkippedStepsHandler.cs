using Orangebeard.Client;
using Orangebeard.Client.Entities;
using Orangebeard.SpecFlowPlugin.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

            /*
            var skippedStepReporter = scenarioReporter.StartChildTestReporter(new Client.Abstractions.Requests.StartTestItemRequest
            {
                Name = scenarioContext.StepContext.StepInfo.GetCaption(),
                StartTime = DateTime.UtcNow,
                //Type = Client.Abstractions.Models.TestItemType.Step,
                Type = Client.Entities.TestItemType.STEP,
                HasStats = false
            });
            */
            StartTestItem startTestItem = new StartTestItem(
                testRunUUID: testRunUuid.Value,
                name: scenarioContext.StepContext.StepInfo.GetCaption(),
                type: TestItemType.STEP,
                description: null,
                attributes: null);
            var skippedStepUuid = client.StartTestItem(scenarioReporter, startTestItem);
            // No need to update the Context, since the step is going to be finished immediately with the status "SKIPPED".


            /*
            skippedStepReporter.Finish(new Client.Abstractions.Requests.FinishTestItemRequest
            {
                EndTime = DateTime.UtcNow,
                //Status = Client.Abstractions.Models.Status.Skipped
                Status = Client.Entities.Status.SKIPPED
            });
            */
            FinishTestItem finishTestItem = new FinishTestItem(testRunUuid.Value, Status.SKIPPED);
            client.FinishTestItem(skippedStepUuid.Value, finishTestItem);
        }
    }
}
