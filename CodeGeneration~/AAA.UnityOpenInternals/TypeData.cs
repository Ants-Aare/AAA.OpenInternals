// using System.Collections.Generic;
// using Microsoft.CodeAnalysis;
//
// namespace AAA.UnityOpenInternals
// {
//     public class TypeData
//     {
//         public string FullSourceTypeName { get; }
//         public string SourceTypeName { get; }
//         public string SourceNamespaceName { get; }
//         public ITypeSymbol TypeSymbol { get; set; }
//
//         public readonly List<ITypeSymbol> DependencyTypes = new List<ITypeSymbol>();
//         
//         public readonly List<FieldData> FieldDatas = new List<FieldData>();
//         public readonly List<PropertyData> PropertyDatas = new List<PropertyData>();
//         public readonly List<EventData> EventDatas = new List<EventData>();
//         public readonly List<MethodData> MethodDatas = new List<MethodData>();
//
//         public TypeData(string fullSourceTypeName)
//         {
//             var lastDot = fullSourceTypeName.LastIndexOf('.');
//             SourceTypeName = fullSourceTypeName.Substring(lastDot + 1);
//             SourceNamespaceName = fullSourceTypeName.Substring(0, lastDot);
//             FullSourceTypeName = fullSourceTypeName;
//         }
//
//         public TypeData(ITypeSymbol typeSymbol) : this(typeSymbol.ToDisplayString())
//         {
//             TypeSymbol = typeSymbol;
//         }
//
//         public void TryAddDependencyType(ITypeSymbol typeSymbol)
//         {
//             if (typeSymbol.SpecialType != SpecialType.None)
//                 return;
//             // if(typeSymbol.null)
//             DependencyTypes.Add(typeSymbol);
//         }
//     }
//
//     public class EventData
//     {
//         public readonly IEventSymbol EventSymbol;
//
//         public EventData(IEventSymbol eventSymbol)
//         {
//             EventSymbol = eventSymbol;
//         }
//     }
//
//     public class MethodData
//     {
//         public readonly IMethodSymbol MethodSymbol;
//         public MethodData(IMethodSymbol methodSymbol)
//         {
//             MethodSymbol = methodSymbol;
//         }
//     }
//     public class PropertyData
//     {
//         public readonly IPropertySymbol PropertySymbol;
//
//         public PropertyData(IPropertySymbol propertySymbol)
//         {
//             PropertySymbol = propertySymbol;
//         }
//     }
//     
//     public class FieldData
//     {
//         public readonly IFieldSymbol FieldSymbol;
//
//         public FieldData(IFieldSymbol fieldSymbol)
//         {
//             FieldSymbol = fieldSymbol;
//         }
//     }
// }