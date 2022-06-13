using Orangebeard.Client.Entities;
using System;

namespace Orangebeard.SpecFlowPlugin.ClientExecution.Logging
{
    public class LogMessage
    {
        /// <summary>
        /// Textual log event message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Time representation when log event occurs.
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        /// Level of log event.
        /// </summary>
        public LogLevel Level { get; set; }

        /// <summary>
        /// Binary data attached to log event.
        /// Null if log event is without attachment.
        /// </summary>
        public LogMessageAttachment Attachment { get; set; }

    }
}
