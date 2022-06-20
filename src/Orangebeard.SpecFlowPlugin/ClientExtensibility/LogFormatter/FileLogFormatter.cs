using Orangebeard.SpecFlowPlugin.ClientExecution.Logging;
using Orangebeard.SpecFlowPlugin.ClientExtensibility;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Orangebeard.Shared.Extensibility.LogFormatter
{
    /// <inheritdoc/>
    public class FileLogFormatter : ILogFormatter
    {
        /// <inheritdoc/>
        public int Order => 10;

        /// <inheritdoc/>
        //public bool FormatLog(CreateLogItemRequest logRequest)
        public bool FormatLog(string logMessage, out string newLogMessage, out LogMessageAttachment logMessageAttachment)
        {
            newLogMessage = logMessage;
            logMessageAttachment = null;

            if (logMessage != null)
            {
                var regex = new Regex("{rp#file#(.*)}");
                var match = regex.Match(logMessage);
                if (match.Success)
                {
                    newLogMessage = logMessage.Replace(match.Value, "");

                    var filePath = match.Groups[1].Value;

                    try
                    {
                        var mimeType = MimeTypes.MimeTypeMap.GetMimeType(Path.GetExtension(filePath));

                        //logRequest.Attach = new LogItemAttach(mimeType, File.ReadAllBytes(filePath));
                        logMessageAttachment = new LogMessageAttachment(mimeType, File.ReadAllBytes(filePath));

                        return true;
                    }
                    catch (Exception exp)
                    {
                        newLogMessage += $"{Environment.NewLine}{Environment.NewLine}Cannot fetch data by `{filePath}` path.{Environment.NewLine}{exp}";
                    }
                }
            }
            return false;
        }
    }
}