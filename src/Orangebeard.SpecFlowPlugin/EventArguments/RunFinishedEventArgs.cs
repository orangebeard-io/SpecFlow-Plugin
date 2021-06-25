using System;
using Orangebeard.Client.Abstractions;
using Orangebeard.Client.Abstractions.Requests;
using Orangebeard.Shared.Reporter;

namespace Orangebeard.SpecFlowPlugin.EventArguments
{
    public class RunFinishedEventArgs : EventArgs
    {
        public RunFinishedEventArgs(IClientService service, FinishLaunchRequest request, ILaunchReporter launchReporter)
        {
            Service = service;
            FinishLaunchRequest = request;
            LaunchReporter = launchReporter;
        }

        public IClientService Service { get; }

        public FinishLaunchRequest FinishLaunchRequest { get; }

        public ILaunchReporter LaunchReporter { get; }

        public bool Canceled { get; set; }
    }
}
