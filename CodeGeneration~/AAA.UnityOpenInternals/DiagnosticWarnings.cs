using Microsoft.CodeAnalysis;

namespace AAA.UnityOpenInternals
{
    public class DiagnosticWarnings
    {
        public static readonly DiagnosticDescriptor InvalidTypeNameWarning = new(id: "OPENINTERNALGEN001",
            title: "Couldn't find Type",
            messageFormat: "Couldn't find Full Type Name based on 'OpenInternalClassAttribute'  with parameter '{0}'.",
            category: "OpenInternalsGenerator",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);
        
        public static readonly DiagnosticDescriptor TypeNotInternalWarning = new(id: "OPENINTERNALGEN002",
            title: "Specified Type is not internal",
            messageFormat: "Accessibility: {1}. The specified Target Type {0} does not have the 'internal' or 'private' accessor. OpenInternals opens up internal classes and is redundant for already open classes.",
            category: "OpenInternalsGenerator",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);
    }
}