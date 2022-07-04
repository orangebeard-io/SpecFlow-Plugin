using Orangebeard.Client;
using Orangebeard.Client.Entities;
using System;

namespace Orangebeard.SpecFlowPlugin.EventArguments
{
    public class RunFinishedEventArgs : EventArgs
    {
        public RunFinishedEventArgs(OrangebeardV2Client client, FinishTestRun finishTestRun, Guid testRunUuid)
        {
            Client = client;
            FinishTestRunObject = finishTestRun;
            TestRunUuid = testRunUuid;
        }

        public OrangebeardV2Client Client { get; }

        public FinishTestRun FinishTestRunObject { get; }

        public Guid TestRunUuid { get; }

        public bool Canceled { get; set; }
    }
}
