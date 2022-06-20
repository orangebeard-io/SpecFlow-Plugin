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
            //ContextAwareLogHandler.CommandsSource_OnEndLogScopeCommand(Context, new ClientExtensibility.Commands.CommandArgs.LogScopeCommandArgs(this));


            //TODO!+ The status is not processed properly.
            // (1) In here, we have the right value for Status - "SKIPPED".
            // (2) But for some reason, in the end result, we get a "STOPPED" instead.
            // (3) When we DO execute the "EndLogScope" hook, we get the SKIPPED as expected.. but ALSO two extra tests!
            // (4) When we DON'T execute the "EndLogScope" hook, we get the right tests... but "STOPPED" instead of "SKIPPED".
            // (5) In the "ContextAwareLogHandler.EndLogScope" hook, the value for the status is determined as follows:
            //                      var finishTestItem = new FinishTestItem(testRunUuid.Value, _nestedStepStatusMap[logScope.Status]);

            // So in other words, we ARE getting the right status. But somehow it only picks up when EndLogScope is called.
            
            OrangebeardV2Client client = OrangebeardAddIn.Client;
            Guid testRunUuid = OrangebeardAddIn.TestrunUuid.Value;
            Status status = ContextAwareLogHandler._nestedStepStatusMap[this.Status];
            FinishTestItem finishTestItem = new FinishTestItem(testRunUuid, status);
            Guid itemUuid = new Guid(this.Id);
            client.FinishTestItem(itemUuid, finishTestItem);

            Context.Log = Parent;
        }
    }
}