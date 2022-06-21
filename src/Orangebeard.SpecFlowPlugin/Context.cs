using Orangebeard.SpecFlowPlugin.ClientExecution;
using Orangebeard.SpecFlowPlugin.ClientExecution.Logging;
using System;

namespace Orangebeard.SpecFlowPlugin
{
    public class NewTestContext
    {
        public NewTestContext(NewTestContext parent, Guid? testUuid)
        {
            Parent = parent;
            TestUuid = testUuid;
            if (parent == null)
            {
                //TODO?~ Also find the values for the other parameters (logContext, root, and parent)? Or remove them?
                //TODO?+ Since we apparently need a logContext... SHOULD we allow its "Log" to be null? Probably not!)
                ILogContext launchContext = new LaunchContext { Log = null };
                Log = new LogScope(logContext: launchContext, root: null, parent: null, name: "Dummy scope name."); //TODO!~ Better scope name! Note that it is not allowed to be null or empty. Maybe remove scope names altogether.
                launchContext.Log = Log; //TODO?~ Is this circularity OK or will it give us an endless loop?
                
            }
            else
            {
                //TODO?~ Also find the values for the other parameters? Or remove them?
                Log = new LogScope(parent.Log.Context, root: null, parent: null, name: "Dummy scope name."); //TODO!~ Better scope name! Note that it is not allowed to be null or empty. Maybe remove scope names altogether.
            }
        }

        public NewTestContext Parent { get; private set; }
        public Guid? TestUuid { get; private set; }

        //TODO!~ Make sure this thing isn't null....
        public ILogScope Log { get; private set; }
        //TODO?+ Log function?
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
