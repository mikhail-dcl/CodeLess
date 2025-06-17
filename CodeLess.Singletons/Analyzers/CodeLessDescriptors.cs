using Microsoft.CodeAnalysis;

namespace CodeLess.Singletons
{
    internal static class CodeLessDescriptors
    {
        public static readonly DiagnosticDescriptor CL0001_CLASS_MUST_BE_PARTIAL =
            new (id: "CL0001",
                title: "Class must be partial",
                messageFormat: "The class '{0}' must be declared as partial to enable code generation.",
                category: "Usage",
                defaultSeverity: DiagnosticSeverity.Error,
                isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor CL0002_CLASS_MUST_HAVE_PARAMETERLESS_CONSTRUCTOR =
            new (id: "CL0002",
                title: "Class must have a parameterless constructor",
                messageFormat: "The class '{0}' must have a parameterless constructor to enable automatic instantiation.",
                category: "Usage",
                defaultSeverity: DiagnosticSeverity.Error,
                isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor CL0003_INTERNAL_MEMBER_MUST_BEGIN_WITH_LOWERCASE =
            new (id: "CL0003",
                title: "The member must start with a lowercase letter",
                messageFormat: "The member '{0}' must start with a lowercase letter so the static accessor can be generated.",
                category: "Usage",
                defaultSeverity: DiagnosticSeverity.Error,
                isEnabledByDefault: true);
    }
}
