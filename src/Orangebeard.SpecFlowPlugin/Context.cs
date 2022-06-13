using Orangebeard.SpecFlowPlugin.ClientExecution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orangebeard.SpecFlowPlugin
{
    public class NewTestContext
    {
        public NewTestContext Parent { get; private set; }
        public Guid? TestUuid { get; private set; }

        //TODO?+ Log function?
    }

    /// <summary>
    /// Provides an access to work with reporting context.
    /// Using it you are able to add log messages, amend curent test metainfo.
    /// </summary>
    public static class Context
    {
        //private static readonly Lazy<CommandsSource> _commandsSource = new Lazy<CommandsSource>(() => new CommandsSource(Extensibility.ExtensionManager.Instance.CommandsListeners));

        //private static readonly Lazy<ITestContext> _current = new Lazy<ITestContext>(() => new TestContext(Extensibility.ExtensionManager.Instance, _commandsSource.Value));

        //private static readonly Lazy<ILaunchContext> _launch = new Lazy<ILaunchContext>(() => new LaunchContext(Extensibility.ExtensionManager.Instance, _commandsSource.Value));

        public static NewTestContext Current { get; private set; }
        /*
        /// <summary>
        /// Returns context to amend current test metadata or add log messages.
        /// </summary>
        public static ITestContext Current
        {
            get
            {
                return _current.Value;
            }
        }
        */

        //TODO?~ Seems that the TestRunUuid is already tracked in OrangebeardAddIn.
        //  We MAY want to move that here.
        //  Same for the OrangebeardClientV2 instance being used.

        /*
        /// <summary>
        /// Returns context to amend current launch metadata or add log messages.
        /// </summary>
        public static ILaunchContext Launch
        {
            get
            {
                return _launch.Value;
            }
        }
        */
    }
}
