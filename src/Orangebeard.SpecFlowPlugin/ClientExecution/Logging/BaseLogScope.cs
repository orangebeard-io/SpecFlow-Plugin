using Orangebeard.Client;
using Orangebeard.Client.Entities;
using Orangebeard.SpecFlowPlugin.ClientExtensibility.Commands;
using System;

namespace Orangebeard.SpecFlowPlugin.ClientExecution.Logging
{
    abstract public class BaseLogScope : ILogScope
    {
        //TODO?+ protected IExtensionManager _extensionManager;

        protected ICommandsSource _commandsSource;

        //TODO?+ Add IExtensionManager and [I]CommandsSource?
        public BaseLogScope(ILogContext logContext /*, IExtensionManager extensionManager*/ /*, ICommandsSource commandsSource */)
        {
            Context = logContext;
            //TODO?+ _extensionManager = extensionManager;
            //TODO?+ _commandsSource = commandsSource;

            BeginTime = DateTime.UtcNow;
        }

        public virtual string Id { get; } = Guid.NewGuid().ToString();

        public virtual ILogScope Parent { get; }

        public virtual ILogScope Root { get; protected set; }

        public virtual ILogContext Context { get; }

        public virtual string Name { get; }

        public virtual DateTime BeginTime { get; }

        public virtual DateTime? EndTime { get; private set; }

        public virtual LogScopeStatus Status { get; set; } = LogScopeStatus.InProgress;

        public virtual ILogScope BeginScope(string name)
        {
            /* ORIGINAL CODE:
             *   var logScope = new LogScope(Context, _extensionManager, _commandsSource, Root, this, name);
             *   Context.Log = logScope;
             *   return logScope;
             */
            //TODO?+ Include extensionManager and _commandsSource?
            var logScope = new LogScope(Context /*, _extensionManager */ /*, _commandsSource*/, Root, this, name);
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
            logMessage.Attachment = GetAttachFromContent(mimeType, content);
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
            logMessage.Attachment = GetAttachFromContent(mimeType, content);
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
            logMessage.Attachment = GetAttachFromContent(mimeType, content);
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
            logMessage.Attachment = GetAttachFromContent(mimeType, content);
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
            logMessage.Attachment = GetAttachFromContent(mimeType, content);
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
            logMessage.Attachment = GetAttachFromContent(mimeType, content);
            Message(logMessage);
        }

        public virtual void Message(LogMessage log)
        {
            //TODO?+ CommandsSource.RaiseOnLogMessageCommand(_commandsSource, Context, new Extensibility.Commands.CommandArgs.LogMessageCommandArgs(this, log));

            var context = Context;
            var currentScope = Context.Log;

            OrangebeardV2Client client = OrangebeardAddIn.Client;
            Guid? testRunUuid = OrangebeardAddIn.TestrunUuid;

            Guid? testUuid = Orangebeard.SpecFlowPlugin.Context.Current.TestUuid;
            if (testUuid != null) // And right now it ALWAYS is... :-(
            {
                Log logItem = new Log(testRunUuid.Value, testUuid.Value, log.Level, log.Message);
                client.Log(logItem);
            }
        }

        protected LogMessage GetDefaultLogRequest(string text)
        {
            //TODO?~
            var logMessage = new LogMessage { Message = text, Attachment = null, Level = LogLevel.info /* ? */, Time = DateTime.Now };
            /*
            var logMessage = new LogMessage(text);
             */
            return logMessage;
        }

        protected LogMessageAttachment GetAttachFromContent(string mimeType, byte[] content)
        {
            return new LogMessageAttachment(mimeType, content);
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