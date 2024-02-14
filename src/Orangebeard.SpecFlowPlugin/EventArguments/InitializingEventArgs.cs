using Orangebeard.Client.V3;
using Orangebeard.Client.V3.OrangebeardConfig;

namespace Orangebeard.SpecFlowPlugin.EventArguments
{
    public class InitializingEventArgs
    {
        public InitializingEventArgs(IConfiguration config)
        {
            Config = config;
        }

        public IConfiguration Config { get; set; }

        public OrangebeardAsyncV3Client OrangebeardClient { get; set; }
    }
}
