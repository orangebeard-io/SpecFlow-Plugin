using Orangebeard.Client;
using Orangebeard.Client.Entities;
using System;

namespace Orangebeard.SpecFlowPlugin.EventArguments
{
    public class RunStartedEventArgs : EventArgs
    {
        RunStartedEventArgs(OrangebeardV2Client client, StartTestRun startTestRun)
        {
            Client = client;
            StartTestRun = startTestRun;
        }

        public RunStartedEventArgs(OrangebeardV2Client client, StartTestRun request, Guid testRunUuid)
            : this(client, request)
        {
            TestRunUuid = testRunUuid;
        }

        public OrangebeardV2Client Client { get; }

        public StartTestRun StartTestRun { get; }

        public Guid TestRunUuid { get; set; }

        public bool Canceled { get; set; }
    }
}
