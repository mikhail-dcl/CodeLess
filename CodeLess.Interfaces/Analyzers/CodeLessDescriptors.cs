using Microsoft.CodeAnalysis;

namespace CodeLess.Interfaces.Analyzers
{
    public static class CodeLessDescriptors
    {
        public static readonly DiagnosticDescriptor CL011_INTERFACE_TYPE_CANNOT_BE_INFERRED =
            new (id: "CODELESS_INTERFACES_0001",
                title: "Class must implement an undefined or partial interface",
                messageFormat: "The class '{0}' must inherit from an interface that has the same generic signature, is not defined in the project or is partial. That interface will be automatically generated.",
                category: "Usage",
                defaultSeverity: DiagnosticSeverity.Error,
                isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor GENERATOR_ERROR = new(
            id:    "CODELESS_INTERFACES_0500",
            title: "Interface Source Generator Error",
            messageFormat: "An exception occurred while generating the interface member '{0}' for '{1}': {2}",
            category: "InterfaceGenerator",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true
        );
    }
}
