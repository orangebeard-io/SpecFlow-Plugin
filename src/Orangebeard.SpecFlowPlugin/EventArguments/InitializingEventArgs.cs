using Orangebeard.Client;
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

        public OrangebeardV2Client Client { get; set; }
    }
}
