using System.Collections.Generic;
using Orangebeard.SpecFlowPlugin.ClientExecution.Metadata;

namespace Orangebeard.SpecFlowPlugin.ClientExtensibility.Commands.CommandArgs
{
    public class TestAttributesCommandArgs
    {
        public TestAttributesCommandArgs(ICollection<MetaAttribute> attributes)
        {
            Attributes = attributes ?? new List<MetaAttribute>();
        }

        public ICollection<MetaAttribute> Attributes { get; }
    }
}
