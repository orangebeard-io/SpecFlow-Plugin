using Orangebeard.Client.Abstractions;
using Orangebeard.Shared.Configuration;

namespace Orangebeard.SpecFlowPlugin.EventArguments
{
    public class InitializingEventArgs
    {
        public InitializingEventArgs(IConfiguration config)
        {
            Config = config;
        }

        public IConfiguration Config { get; set; }

        public IClientService Service { get; set; }
    }
}
