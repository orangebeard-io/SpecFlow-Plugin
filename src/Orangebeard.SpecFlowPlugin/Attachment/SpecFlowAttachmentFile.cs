using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orangebeard.SpecFlowPlugin.Attachment
{
    internal class SpecFlowAttachmentFile
    {
        public string MimeType { get; }
        public string FileName { get; }
        public byte[] Data { get; }
        public SpecFlowAttachmentFile(string mimeType, string fileName, byte[] data)
        {
            this.MimeType = mimeType;
            this.FileName = fileName;
            this.Data = data;
        }   
    }
}
