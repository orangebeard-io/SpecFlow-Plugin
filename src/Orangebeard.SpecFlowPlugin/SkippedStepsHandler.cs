﻿using System;
using Orangebeard.Client.V3.Entity;
using Orangebeard.Client.V3.Entity.Step;
using Orangebeard.SpecFlowPlugin.Extensions;
using Orangebeard.SpecFlowPlugin.Util;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Infrastructure;

namespace Orangebeard.SpecFlowPlugin
{
    public class SkippedStepsHandler : ISkippedStepHandler
    {
        public void Handle(ScenarioContext scenarioContext)
        {
            var context = OrangebeardAddIn.GetCurrentContext();

            var skippedStep = new StartStep
            {
                TestRunUUID = context.testrun,
                TestUUID = context.test,
                StepName = scenarioContext.StepContext.StepInfo.GetCaption(),
                StartTime = PreciseUtcTime.UtcNow
            };

            if (context.step.HasValue)
            {
                skippedStep.ParentStepUUID = context.step.Value;
            }

            var skippedStepGuid = OrangebeardHooks.GetClient().StartStep(skippedStep);
            
            OrangebeardHooks.GetClient().FinishStep(skippedStepGuid, new FinishStep
            {
                TestRunUUID = context.testrun,
                Status = TestStatus.SKIPPED,
                EndTime = PreciseUtcTime.UtcNow
            });
        }
    }
}
