using Orangebeard.Client.Entities;
using System;

namespace Orangebeard.SpecFlowPlugin.ClientExecution.Logging
{
    public static class LogMessageExtensions
    {
        /// <summary>
        /// Given an ILogMessage, turn it into an Orangebeard "Log" instance.
        /// If the message contains an Attachment, create an Orangebeard "Attachment" instance as well.
        /// </summary>
        /// <param name="logMessage">The log message.</param>
        /// <param name="testRunUuid">Unique identifier for the test run, that the ILogMessage belongs to.</param>
        /// <param name="testItemUuid">Unique identifier for the test, that the ILogMessage belongs to.</param>
        /// <returns>A Log instance and an Attachment instance. If the ILogMessage did not contain an attachment, the Attachment instance returned will be <code>null</code>.</returns>
        public static Tuple<Log, Attachment> ConvertToLogAndAttachment(this ILogMessage logMessage, Guid testRunUuid, Guid testItemUuid)
        {
            if (logMessage == null) throw new ArgumentNullException("Cannot convert nullable log message object.", nameof(logMessage));

            LogLevel logLevel;

            switch (logMessage.Level)
            {
                case LogMessageLevel.Debug:
                    logLevel = LogLevel.debug;
                    break;
                case LogMessageLevel.Error:
                    logLevel = LogLevel.error;
                    break;
                //TODO?- V2 client doesn't support fatal anymore.
                //case LogMessageLevel.Fatal:
                //    logLevel = LogLevel.fatal;
                //    break;
                case LogMessageLevel.Info:
                    logLevel = LogLevel.info;
                    break;
                //TODO?- V2 client doesn't support trace anmymore.
                //case LogMessageLevel.Trace:
                //    logLevel = LogLevel.trace;
                //    break;
                case LogMessageLevel.Warning:
                    logLevel = LogLevel.warn;
                    break;
                default:
                    throw new Exception(string.Format("Unknown {0} level of log message.", logMessage.Level));
            }

            //TODO?+ Original code set the timestamp to logMessage.Time .
            var log = new Log(testRunUuid, testItemUuid, logLevel, logMessage.Message);

            Attachment attachment = null;
            string fileName = ""; //TODO!~ Find the proper name for the attachment file!!
            if (logMessage.Attachment != null)
            {
                Attachment.AttachmentFile attachmentFile = new Attachment.AttachmentFile(fileName, logMessage.Attachment.MimeType, logMessage.Attachment.Data);
                attachment = new Attachment(testRunUuid, testItemUuid, logLevel, fileName, attachmentFile);
            }

            return new Tuple<Log, Attachment>(log, attachment);
        }
    }
}
