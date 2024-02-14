using System;
using System.IO;
using Orangebeard.Client.V3.ClientUtils.Logging;
using Orangebeard.Client.V3.OrangebeardConfig;
using Orangebeard.SpecFlowPlugin;
using TechTalk.SpecFlow.Bindings;
using TechTalk.SpecFlow.Infrastructure;
using TechTalk.SpecFlow.Plugins;
using TechTalk.SpecFlow.UnitTestProvider;

[assembly: RuntimePlugin(typeof(Plugin))]
namespace Orangebeard.SpecFlowPlugin
{
    /// <summary>
    /// Registered SpecFlow plugin from configuration file.
    /// </summary>
    internal class Plugin : IRuntimePlugin
    {
        private ILogger _logger;

        public static IConfiguration Config { get; set; }

        public void Initialize(RuntimePluginEvents runtimePluginEvents, RuntimePluginParameters runtimePluginParameters, UnitTestProviderConfiguration unitTestProviderConfiguration)
        {
            var currentDirectory = Path.GetDirectoryName(new Uri(typeof(Plugin).Assembly.CodeBase).LocalPath);

            _logger = LogManager.Instance.WithBaseDir(currentDirectory).GetLogger<Plugin>();

            Config = new ConfigurationBuilder().AddDefaults(currentDirectory).Build();

            var isEnabled = Config.GetValue("Enabled", true);

            if (!isEnabled) return;
            
            runtimePluginEvents.CustomizeGlobalDependencies += (sender, e) =>
            {
                e.SpecFlowConfiguration.AdditionalStepAssemblies.Add("Orangebeard.SpecFlowPlugin");
                e.ObjectContainer.RegisterTypeAs<SafeBindingInvoker, IBindingInvoker>();
                e.ObjectContainer.RegisterTypeAs<OrangebeardOutputHelper, ISpecFlowOutputHelper>();
            };

            runtimePluginEvents.CustomizeScenarioDependencies += (sender, e) =>
            {
                e.ObjectContainer.RegisterTypeAs<SkippedStepsHandler, ISkippedStepHandler>();
                e.ObjectContainer.RegisterTypeAs<OrangebeardOutputHelper, ISpecFlowOutputHelper>();
            };
        }
    }
}
