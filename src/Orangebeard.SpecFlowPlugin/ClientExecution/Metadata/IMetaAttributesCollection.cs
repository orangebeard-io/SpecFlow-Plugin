using System.Collections.Generic;

namespace Orangebeard.SpecFlowPlugin.ClientExecution.Metadata
{
    public interface IMetaAttributesCollection : ICollection<MetaAttribute>
    {
        void Add(string key, string value);
    }
}
