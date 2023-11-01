using Orangebeard.Client.Abstractions.Models;
using Orangebeard.Client.Abstractions.Requests;
using Orangebeard.Shared.Reporter;
using Orangebeard.SpecFlowPlugin.Attachment;
using Orangebeard.SpecFlowPlugin.LogHandler;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using TechTalk.SpecFlow.Events;
using TechTalk.SpecFlow.Infrastructure;
using TechTalk.SpecFlow.Tracing;

namespace Orangebeard.SpecFlowPlugin
{
    public class OrangebeardOutputHelper : ISpecFlowOutputHelper
    {
        private readonly SpecFlowOutputHelper _baseHelper;

        private const string FILE_PATH_PATTERN = @"((((?<!\w)[A-Z,a-z]:)|(\.{0,2}\\))([^\b%\/\|:\n<>""']*))";

        public OrangebeardOutputHelper(ITestThreadExecutionEventPublisher testThreadExecutionEventPublisher, ITraceListener traceListener, ISpecFlowAttachmentHandler specFlowAttachmentHandler)
        {
            _baseHelper = new SpecFlowOutputHelper(testThreadExecutionEventPublisher, traceListener, specFlowAttachmentHandler);   
        }

        public void AddAttachment(string filePath)
        {
            SendAttachment(GetAttachmentFileFromPath(filePath), null);
            _baseHelper.AddAttachment(filePath);
        }

        public void WriteLine(string text)
        {
            SendLog(text);
            _baseHelper.WriteLine(text);
        }

        public void WriteLine(string format, params object[] args)
        {
            SendLog(string.Format(format, args));
            _baseHelper.WriteLine(format, args);
        }

        private void SendLog(string message)
        {
            Match match = Regex.Match(message, FILE_PATH_PATTERN);
            if (match.Success) //Look only at first match, as we support max 1 attachment per log entry
            {
                string filePath = match.Value;
                if (!Path.IsPathRooted(filePath))
                {
                    filePath = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + filePath;
                }
                SendAttachment(GetAttachmentFileFromPath(filePath), message);               
            }
            else
            {
                SendLog(message, LogLevel.Info);
            }             
        }

        private void SendLog(string message, LogLevel level)
        {
            GetCurrentReporter().Log(new CreateLogItemRequest
            {
                Level = level,
                Text = message,
                Format = LogFormat.MARKDOWN,
                Time = DateTime.UtcNow
            });
        }

        private void SendAttachment(SpecFlowAttachmentFile attachment, string logMessage)
        {
            if (attachment != null)
            {
                GetCurrentReporter().Log(new CreateLogItemRequest
                {
                    Time = DateTime.UtcNow,
                    Level = LogLevel.Info,
                    Text = logMessage ?? "[Attachment]: " + attachment.FileName,
                    Format = LogFormat.PLAIN_TEXT,
                    Attach = new LogItemAttach(attachment.MimeType, attachment.Data) { Name = attachment.FileName }
                });
            }
        }

        private SpecFlowAttachmentFile GetAttachmentFileFromPath(string filePath)
        {
            try
            {
                return new SpecFlowAttachmentFile(
                    Orangebeard.Shared.MimeTypes.MimeTypeMap.GetMimeType(Path.GetExtension(filePath)),
                    Path.GetFileName(filePath),
                    File.ReadAllBytes(filePath));            
            } catch (Exception e)
            {
                SendLog($"\r\nFailed to attach {filePath} ({e.Message})", LogLevel.Warning);
                return null;
            }
        }

        private ITestReporter GetCurrentReporter()
        {
            var reporter = OrangebeardAddIn.GetStepTestReporter(ContextAwareLogHandler.ActiveStepContext);

            if (reporter == null)
            {
                reporter = OrangebeardAddIn.GetScenarioTestReporter(ContextAwareLogHandler.ActiveScenarioContext);
            }

            if (reporter == null)
            {
                reporter = OrangebeardAddIn.GetFeatureTestReporter(ContextAwareLogHandler.ActiveFeatureContext);
            }
          
            return reporter;
        }

    }
}