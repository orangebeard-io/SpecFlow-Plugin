using Orangebeard.SpecFlowPlugin.ClientExecution;
using Orangebeard.SpecFlowPlugin.ClientExecution.Logging;
using System;

namespace Orangebeard.SpecFlowPlugin
{
    public class NewTestContext
    {
        public NewTestContext(NewTestContext parent, Guid? testUuid)
        {
            TestUuid = testUuid;
            Parent = parent;

            string scopeName = " ";
            if (parent == null)
            {
                ILogContext launchContext = new LaunchContext { Log = null };
                Log = new LogScope(logContext: launchContext, root: null, parent: null, name: scopeName);
                launchContext.Log = Log;
            }
            else
            {
                Log = new LogScope(parent.Log.Context, root: null, parent: null, name: scopeName);
            }
        }

        public Guid? TestUuid { get; private set; }

        public ILogScope Log { get; private set; }

        public NewTestContext Parent { get; private set; }
    }

    /// <summary>
    /// Provides an access to work with reporting context.
    /// Using it you are able to add log messages, amend curent test metainfo.
    /// </summary>
    public static class Context
    {
        public static NewTestContext Current { get; set; } = new NewTestContext(null, null);
    }
}
