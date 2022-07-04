using System;

namespace Orangebeard.SpecFlowPlugin.ClientExecution.Metadata
{
    public class MetaAttribute
    {
        public MetaAttribute(string key, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException("Attribute value cannot be null or empty.", nameof(value));
            }

            Key = key;
            Value = value;
        }

        public string Key { get; }

        public string Value { get; }

        public static MetaAttribute Parse(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException("Cannot parse empty value.");
            }

            string metaKey = null;
            string metaValue;

            var parts = value.Split(':');

            if (parts.Length == 1 || string.IsNullOrEmpty(parts[1]))
            {
                metaValue = value;
            }
            else
            {
                if (parts[0] != string.Empty)
                {
                    metaKey = parts[0];
                }

                metaValue = value.Substring(parts[0].Length + 1);
            }

            return new MetaAttribute(metaKey, metaValue);
        }

        public static implicit operator Client.Entities.Attribute(MetaAttribute a) => new Client.Entities.Attribute(a.Key, a.Value);
    }
}
