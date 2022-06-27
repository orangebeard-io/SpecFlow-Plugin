using Orangebeard.Client;
using Orangebeard.Client.Entities;
using Orangebeard.Shared.Extensibility.LogFormatter;
using Orangebeard.SpecFlowPlugin.ClientExtensibility;
using Orangebeard.SpecFlowPlugin.ClientExtensibility.Commands;
using System;
using System.Collections.Generic;
using Attribute = Orangebeard.Client.Entities.Attribute;

namespace Orangebeard.SpecFlowPlugin.ClientExecution.Logging
{
    abstract public class BaseLogScope : ILogScope
    {
        protected ICommandsSource _commandsSource;

        public BaseLogScope(ILogContext logContext)
        {
            Context = logContext;
            BeginTime = DateTime.UtcNow;
        }

        public virtual string Id { get; } = Guid.NewGuid().ToString();

        public virtual ILogScope Parent { get; }

        public virtual ILogScope Root { get; protected set; }

        //TODO!+ Make sure that this thing is initialized....
        public virtual ILogContext Context { get; }

        public virtual string Name { get; }

        public virtual DateTime BeginTime { get; }

        public virtual DateTime? EndTime { get; private set; }

        public virtual LogScopeStatus Status { get; set; } = LogScopeStatus.InProgress;

        public static ILogFormatter fileFormatter = new FileLogFormatter();

        public virtual ILogScope BeginScope(string name)
        {
            // NOTE: In the current implementation, the TestRunUuid and TestUuid are NULL when BeginScope is first called!
            // Need to fix this. Also, maybe keep TestUuid in OrangebeardAddIn ?
            // Note that we can use `OrangebeardAddIn.GetScenarioTestReporter` and its brothers to get a Suite/Test/Step UUID given a SpecFlow Context.

            var testUuid = SpecFlowPlugin.Context.Current.TestUuid;
            Guid testRunUuid = OrangebeardAddIn.TestrunUuid.Value; //TODO?+ Check if OrangebeardAddIn.TestrunUuid != null
            
            StartTestItem startTestItem = new StartTestItem(
                testRunUUID: testRunUuid,
                name: name,
                type: TestItemType.STEP,
                description: null,
                attributes: new HashSet<Attribute>()
                );
            OrangebeardV2Client client = OrangebeardAddIn.Client;
            var childTestUuid = client.StartTestItem(testUuid, startTestItem);
            NewTestContext newTestContext = new NewTestContext(SpecFlowPlugin.Context.Current, childTestUuid);
            SpecFlowPlugin.Context.Current = newTestContext;

            var logScope = new LogScope(Context, Root, this, name);
            OrangebeardAddIn.LogScopes[logScope.Id] = childTestUuid.Value;
            Context.Log = logScope;
            return logScope;
        }

        public void Debug(string message)
        {
            var logMessage = GetDefaultLogRequest(message);
            logMessage.Level = LogLevel.debug;
            Message(logMessage);
        }

        public void Debug(string message, string mimeType, byte[] content)
        {
            var logMessage = GetDefaultLogRequest(message);
            logMessage.Level = LogLevel.debug;
            logMessage.Attachment = GetAttachFromContent(mimeType, content, logMessage.Attachment.FileName); //TODO?- Isn't this ALREADY in logMessage.Attachment?
            Message(logMessage);
        }

        public void Error(string message)
        {
            var logMessage = GetDefaultLogRequest(message);
            logMessage.Level = LogLevel.error;
            Message(logMessage);
        }

        public void Error(string message, string mimeType, byte[] content)
        {
            var logMessage = GetDefaultLogRequest(message);
            logMessage.Level = LogLevel.error;
            logMessage.Attachment = GetAttachFromContent(mimeType, content, logMessage.Attachment.FileName); //TODO?- Isn't this ALREADY in logMessage.Attachment?
            Message(logMessage);
        }

        public void Fatal(string message)
        {
            var logMessage = GetDefaultLogRequest(message);
            logMessage.Level = LogLevel.error; // WAS: LogMessageLevel.Fatal
            Message(logMessage);
        }

        public void Fatal(string message, string mimeType, byte[] content)
        {
            var logMessage = GetDefaultLogRequest(message);
            logMessage.Level = LogLevel.error; // WAS: LogMessageLevel.Fatal
            logMessage.Attachment = GetAttachFromContent(mimeType, content, logMessage.Attachment.FileName); //TODO?- Isn't this ALREADY in logMessage.Attachment?
            Message(logMessage);
        }

        public void Info(string message)
        {
            var logMessage = GetDefaultLogRequest(message);
            logMessage.Level = LogLevel.info;
            Message(logMessage);
        }

        public void Info(string message, string mimeType, byte[] content)
        {
            var logMessage = GetDefaultLogRequest(message);
            logMessage.Level = LogLevel.info;
            logMessage.Attachment = GetAttachFromContent(mimeType, content, logMessage.Attachment.FileName); //TODO?- Isn't this ALREADY in logMessage.Attachment?
            Message(logMessage);
        }

        public void Trace(string message)
        {
            var logMessage = GetDefaultLogRequest(message);
            logMessage.Level = LogLevel.info; // WAS: LogMessageLevel.Trace
            Message(logMessage);
        }

        public void Trace(string message, string mimeType, byte[] content)
        {
            var logMessage = GetDefaultLogRequest(message);
            logMessage.Level = LogLevel.info; // WAS: LogMessageLevel.Trace
            logMessage.Attachment = GetAttachFromContent(mimeType, content, logMessage.Attachment.FileName); //TODO?- Isn't this ALREADY in logMessage.Attachment?
            Message(logMessage);
        }

        public void Warn(string message)
        {
            var logMessage = GetDefaultLogRequest(message);
            logMessage.Level = LogLevel.warn;
            Message(logMessage);
        }

        public void Warn(string message, string mimeType, byte[] content)
        {
            var logMessage = GetDefaultLogRequest(message);
            logMessage.Level = LogLevel.warn;
            logMessage.Attachment = GetAttachFromContent(mimeType, content, logMessage.Attachment.FileName); //TODO?- Isn't this ALREADY in logMessage.Attachment?
            Message(logMessage);
        }

        //TODO?~ The only reason this thing isn't a static method, is that BaseLogScope needs to implement ILogScope.
        public virtual void Message(LogMessage log)
        {
            OrangebeardV2Client client = OrangebeardAddIn.Client;
            Guid? testRunUuid = OrangebeardAddIn.TestrunUuid; //TODO?~ Test if it isn't null... or make it not a nullable thing in the first place, in OrangebeardAddIn.
            Guid? testUuid = SpecFlowPlugin.Context.Current.TestUuid;
            if (testUuid != null)
            {
                // Workaround: error messages are usually stack traces, which don't display nicely in markdown.
                //  For this reason, when the log is at the level of an error, we display in plain text instead of markdown.
                var logFormat = log.Level == LogLevel.error ? LogFormat.PLAIN_TEXT : LogFormat.MARKDOWN;
                Log logItem = new Log(testRunUuid.Value, testUuid.Value, log.Level, log.Message, logFormat);
                if (log.Attachment == null)
                {
                    client.Log(logItem);
                }
                else 
                {
                    string fileName = log.Attachment.FileName;
                    Attachment.AttachmentFile attachmentFile = new Attachment.AttachmentFile(fileName, log.Attachment.MimeType, log.Attachment.Data);
                    Attachment attachment = new Attachment(testRunUuid.Value, testUuid.Value, log.Level, log.Message, attachmentFile);
                    client.SendAttachment(attachment);
                }
            }
        }

        protected LogMessage GetDefaultLogRequest(string text)
        {
            fileFormatter.FormatLog(text, out string newLogMessage, out LogMessageAttachment attachment);

            var logMessage = new LogMessage { Message = newLogMessage, Attachment = attachment, Level = LogLevel.info /* ? */, Time = DateTime.Now };

            return logMessage;
        }

        protected LogMessageAttachment GetAttachFromContent(string mimeType, byte[] content, string fileName)
        {
            return new LogMessageAttachment(mimeType, content, fileName);
        }

        public virtual void Dispose()
        {
            EndTime = DateTime.UtcNow;

            if (Status == LogScopeStatus.InProgress)
            {
                Status = LogScopeStatus.Passed;
            }
        }
    }
}