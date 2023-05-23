using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace AAA.UnityOpenInternals
{
    public static class ClassGenerator
    {
        private const string ClassTemplate = @"
using System;
using System.Linq;
using System.Reflection;
using AAA.UnityOpenInternals.Runtime.Extensions;
using static AAA.UnityOpenInternals.Runtime.Extensions.OpenInternalsExtensions;

namespace AAA.OpenInternals.Generated
{
    public partial class $[CLASSNAME]
    {
        public static Type OpenInternalType => _openInternalType 
            ?? GetTargetAssembly(""$[SOURCEASSEMBLY]"")
                .GetType(""$[SOURCETYPE]"");

        public static MethodInfo[] Methods => _methods ?? OpenInternalType.GetMethods();

        private static Type _openInternalType;
        private static MethodInfo[] _methods;

        public readonly object OpenInternalObjectInstance;

        public $[CLASSNAME](object instance)
        {
            OpenInternalObjectInstance = instance ?? throw new NullReferenceException(""Instance must be specified."");
        }
$[BODY]
    }
}
";

        public static string GenerateClass(
            GeneratorExecutionContext context,
            TypeData typeData,
            Func<FieldData, string> generateFieldFunc = null,
            Func<PropertyData, string> generatePropertyFunc = null,
            Func<EventData, string> generateEventFunc = null,
            Func<MethodData, string> generateMethodFunc = null
        )
        {
            var classBody = new StringBuilder();
            
            AddMembers(classBody, typeData.FieldDatas, generateFieldFunc);
            AddMembers(classBody, typeData.PropertyDatas, generatePropertyFunc);
            AddMembers(classBody, typeData.EventDatas, generateEventFunc);
            AddMembers(classBody, typeData.MethodDatas, generateMethodFunc);
                
            var classText = new StringBuilder(typeData.SourceTypeName)
                    .Replace("$[SOURCEASEMBLY]", typeData.TypeSymbol.ContainingAssembly.ToDisplayString())
                    .Replace("$[SOURCETYPE]", typeData.FullSourceTypeName)
                    .Replace("$[CLASSNAME]", typeData.SourceTypeName)
                    .Replace("$[BODY]", classBody.ToString())
                ;

            return classText.ToString();
            // context.AddSource(typeData.SourceTypeName, SourceText.From(classText.ToString(), Encoding.UTF8));
        }

        private static void AddMembers<T>(StringBuilder stringBuilder, IEnumerable<T> dataEnumerable, Func<T, string> generateMethodFunc)
        {
            if (generateMethodFunc == null)
                return;
            
            foreach (var data in dataEnumerable)
            {
                stringBuilder.Append(generateMethodFunc.Invoke(data));
            }
        }
    }
}