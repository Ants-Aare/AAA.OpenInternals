using System;
using Unity.Plastic.Newtonsoft.Json.Serialization;

namespace AAA.UnityOpenInternals
{
    [AttributeUsage(validOn: AttributeTargets.Class | AttributeTargets.Interface, Inherited = false)]
    public class OpenInternalClassAttribute : Attribute
    {
        public string FullTypeName;

        public OpenInternalClassAttribute(string fullTypeName)
        {
            FullTypeName = fullTypeName;
        }
    }
}