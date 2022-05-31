using Orangebeard.Client.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orangebeard.SpecFlowPlugin.ClientExecution.Logging
{
    public static class LogMessageExtensions
    {
        public static CreateLogItemRequest ConvertToRequest(this ILogMessage logMessage, Guid testRunUuid, Guid testItemUuid)
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
                case LogMessageLevel.Fatal:
                    logLevel = LogLevel.fatal;
                    break;
                case LogMessageLevel.Info:
                    logLevel = LogLevel.info;
                    break;
                case LogMessageLevel.Trace:
                    logLevel = LogLevel.trace;
                    break;
                case LogMessageLevel.Warning:
                    logLevel = LogLevel.warn;
                    break;
                default:
                    throw new Exception(string.Format("Unknown {0} level of log message.", logMessage.Level));
            }

            /*
            var logRequest = new CreateLogItemRequest
            {
                Text = logMessage.Message,
                Time = logMessage.Time,
                Level = logLevel
            };
            */
            //TODO?+ Original code set the timestamp to logMessage.Time .
            var log = new Log(testRunUuid, testItemUuid, logLevel, logMessage.Message);

            if (logMessage.Attachment != null)
            {
                logRequest.Attach = new LogItemAttach
                {
                    MimeType = logMessage.Attachment.MimeType,
                    Data = logMessage.Attachment.Data
                };
            }

            return logRequest;
        }
    }
}
