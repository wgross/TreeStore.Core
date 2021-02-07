using System.Collections.Generic;

namespace PowerShellFilesystemProviderBase.Test
{
    public class TypeExtensionsTest
    {
        public class DerivedDictionary : Dictionary<string, object>
        {
        }

        public class PropertyBag
        {
            private string Data { get; set; }
        }

        public class ContainerParent : IContainerItem
        {
            public Dictionary<string, object> Dictionary { get; set; }

            public IDictionary<string, object> IDictionary { get; set; }

            public DerivedDictionary DerivedDictionary { get; set; }

            public Dictionary<int, object> WrongKey { get; set; }

            public PropertyBag PropertyBag { get; set; }

            public int ScalarValue { get; set; }
        }
    }
}