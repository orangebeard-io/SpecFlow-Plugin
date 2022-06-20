//using Orangebeard.Shared.Extensibility;
//using Orangebeard.Shared.Extensibility.Commands.CommandArgs;
using Orangebeard.Client;
using Orangebeard.Client.Entities;
using Orangebeard.SpecFlowPlugin.LogHandler;
using System;

namespace Orangebeard.SpecFlowPlugin.ClientExecution.Logging
{
    public class LogScope : BaseLogScope
    {
        //TODO?+ Add IExtensionManager and CommandsSource?
        public LogScope(ILogContext logContext /*, IExtensionManager extensionManager*/ /*, CommandsSource commandsSource*/, ILogScope root, ILogScope parent, string name) 
            : base(logContext /*, extensionManager*/ /*, commandsSource */)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Log scope name cannot be null of empty.", nameof(name));
            }

            Root = root;
            Parent = parent;
            Name = name;

            //TODO?+ CommandsSource.RaiseOnBeginScopeCommand(commandsSource, logContext, new LogScopeCommandArgs(this));
        }

        public override ILogScope Parent { get; }

        public override string Name { get; }

        public override void Dispose()
        {
            base.Dispose();

            //TODO?+ CommandsSource.RaiseOnEndScopeCommand(_commandsSource, Context, new LogScopeCommandArgs(this));
            ContextAwareLogHandler.CommandsSource_OnEndLogScopeCommand(Context, new ClientExtensibility.Commands.CommandArgs.LogScopeCommandArgs(this));


            //TODO!+ The status is not processed properly.
            // (1) In here, we have the right value for Status - "SKIPPED".
            // (2) But for some reason, in the end result, we get a "STOPPED" instead.
            
            
            // (3) When we DO execute the "EndLogScope" hook, we get the SKIPPED as expected.. but ALSO two extra tests!
            //                  ---> These extra tests no longer appear, looks like we're good!


            // (4) When we DON'T execute the "EndLogScope" hook, we get the right tests... but "STOPPED" instead of "SKIPPED".
            // (5) In the "ContextAwareLogHandler.EndLogScope" hook, the value for the status is determined as follows:
            //                      var finishTestItem = new FinishTestItem(testRunUuid.Value, _nestedStepStatusMap[logScope.Status]);
            // (6) If, in this Dispose() method, I set the status to "FAILED", we get the same results. 
            // (7) Still: - What should be "PASSED"  (enum 1) goes to "IN_PROGRESS" (enum 0)
            //            - What sould be "SKIPPED" (enum 4) goes to "STOPPED" (enum 3).
            //            ... so mabye it IS being set to the wrong value after all....???
            //            But if I simply add "4" to the status, we get the same results. Although the Google Cloud Console seems to  log a few NullPointerExceptions that MIGHT be related to this.
            // (8) When I don't call client.FinishTestItem at all, I  STILL get the same results....
            //
            //
            //    All of which suggests, what happens here somehow NEVER gets executed, even though the debugger says it does!


            // So in other words, we ARE getting the right status. But somehow it only picks up when EndLogScope is called.
            
            OrangebeardV2Client client = OrangebeardAddIn.Client;
            Guid testRunUuid = OrangebeardAddIn.TestrunUuid.Value;
            
            Status status = ContextAwareLogHandler._nestedStepStatusMap[this.Status];

            FinishTestItem finishTestItem = new FinishTestItem(testRunUuid, status);
            Guid itemUuid = new Guid(this.Id);
            //client.FinishTestItem(itemUuid, finishTestItem); //TODO!+ Not calling the FinishTestItem at all and see what happens then...

            //OrangebeardAddIn.LogScopes.TryRemove(Id, out Guid _); //TODO?+ Adding this didn't change the problem with the statuses.

            Context.Log = Parent;


        }
    }
}