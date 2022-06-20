namespace Orangebeard.SpecFlowPlugin.ClientExecution.Logging
{
    public class LogMessageAttachment
    {
        public LogMessageAttachment(string mimeType, byte[] data, string fileName)
        {
            MimeType = mimeType;
            Data = data; //TODO?~ Explicit array copy? Safer but takes time.
            FileName = fileName;
        }

        public string MimeType { get; private set; }
        public byte[] Data { get; private set; }
        public string FileName { get; private set; }
    }
}
