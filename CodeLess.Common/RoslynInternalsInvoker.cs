using System;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Linq;

namespace CodeLess.Common
{
    /// <summary>
    /// Helper to call the internal CSharpAssemblySymbol.GetTypeByMetadataName(...)
    /// and return an INamedTypeSymbol? directly, dropping the internal 'conflicts' out parameter.
    /// Unwraps only the public‐model SourceAssemblySymbol wrapper; otherwise returns null.
    /// </summary>
    public static class RoslynInternalsInvoker
    {
        // Delegate that matches the internal method (boxes everything as object)
        private delegate object? RawGetTypeByMetadataNameDelegate(
            object assemblySymbol,
            string metadataName,
            bool includeReferences,
            bool isWellKnownType
        );

        // Build the expression‐tree once, on demand
        private static readonly Lazy<RawGetTypeByMetadataNameDelegate> s_rawInvoker
            = new Lazy<RawGetTypeByMetadataNameDelegate>(CreateRawInvoker);

        /// <summary>
        /// Look up a type by metadata name in the given assembly.
        /// Returns an INamedTypeSymbol? (null if not found,
        /// or if the assembly symbol isn't the internal type or SourceAssemblySymbol).
        /// </summary>
        public static INamedTypeSymbol? GetTypeByMetadataName(
            IAssemblySymbol assembly,
            string metadataName,
            bool includeReferences,
            bool isWellKnownType
        )
        {
            const string internalAsmFullName = "Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol";
            const string sourceWrapperFullName = "Microsoft.CodeAnalysis.CSharp.Symbols.PublicModel.SourceAssemblySymbol";

            object asmObj = assembly;
            var asmTypeName = asmObj.GetType().FullName;

            object internalAsm;
            if (asmTypeName == internalAsmFullName)
            {
                // already the internal symbol
                internalAsm = asmObj;
            }
            else if (asmTypeName == sourceWrapperFullName)
            {
                // unwrap the SourceAssemblySymbol wrapper
                internalAsm = UnwrapSourceAssemblySymbol(asmObj);
            }
            else
            {
                // not supported
                return null;
            }

            // invoke the compiled delegate
            var boxed = s_rawInvoker.Value(
                internalAsm,
                metadataName,
                includeReferences,
                isWellKnownType
            );

            return boxed as INamedTypeSymbol;
        }

        // Unwraps only the SourceAssemblySymbol public‐model wrapper to the internal AssemblySymbol.
        private static object UnwrapSourceAssemblySymbol(object sourceWrapper)
        {
            // We know this is SourceAssemblySymbol, so find its non-public field pointing at the internal symbol:
            var wrapperType = sourceWrapper.GetType();
            var field = wrapperType
                .GetProperties(BindingFlags.Instance | BindingFlags.NonPublic)
                .FirstOrDefault(f => f.PropertyType.FullName == "Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol");

            if (field is null)
                throw new InvalidOperationException($"Cannot unwrap {wrapperType.FullName}");

            var internalAsm = field.GetValue(sourceWrapper);
            if (internalAsm is null)
                throw new InvalidOperationException($"Internal AssemblySymbol was null in {wrapperType.FullName}");

            return internalAsm;
        }

        private static RawGetTypeByMetadataNameDelegate CreateRawInvoker()
        {
            // 1) Load the Roslyn assemblies
            var coreAsm   = typeof(Compilation).Assembly;
            var csharpAsm = typeof(CSharpCompilation).Assembly;

            // 2) Find the internal types
            var asmSymType = csharpAsm.GetType(
                "Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol", throwOnError: true
            )!;
            var diagBagType = csharpAsm.GetType(
                "Microsoft.CodeAnalysis.DiagnosticBag", throwOnError: false
            ) ?? coreAsm.GetType("Microsoft.CodeAnalysis.DiagnosticBag", throwOnError: true)!;

            // 3) Prepare the out‐param tuple type
            var vtDef = typeof(ValueTuple<,>).GetGenericTypeDefinition();
            var conflictsVT    = vtDef.MakeGenericType(asmSymType, asmSymType);
            var conflictsByRef = conflictsVT.MakeByRefType();

            // 4) Bind the internal method
            var mi = asmSymType.GetMethod(
                "GetTypeByMetadataName",
                BindingFlags.Instance | BindingFlags.NonPublic,
                binder: null,
                types: new[]
                {
                    typeof(string),       // metadataName
                    typeof(bool),         // includeReferences
                    typeof(bool),         // isWellKnownType
                    conflictsByRef,       // out (AssemblySymbol,AssemblySymbol)
                    typeof(bool),         // useCLSCompliantNameArityEncoding
                    diagBagType,          // DiagnosticBag warnings
                    typeof(bool)          // ignoreCorLibraryDuplicatedTypes
                },
                modifiers: null
            )!;

            // 5) Build the parameters for our expression
            var pAsm       = Expression.Parameter(typeof(object), "assemblySymbol");
            var pName      = Expression.Parameter(typeof(string), "metadataName");
            var pInclude   = Expression.Parameter(typeof(bool),   "includeReferences");
            var pWellKnown = Expression.Parameter(typeof(bool),   "isWellKnownType");

            // 6) Cast and default constants
            var asmCast      = Expression.Convert(pAsm, asmSymType);
            var warningsConst= Expression.Constant(null, diagBagType);
            var useClsConst  = Expression.Constant(false, typeof(bool));
            var ignoreConst  = Expression.Constant(false, typeof(bool));

            // 7) Declare the out‐tuple local
            var conflictsVar = Expression.Variable(conflictsVT, "conflicts");

            // 8) Call the internal method
            var call = Expression.Call(
                asmCast, mi,
                pName, pInclude, pWellKnown,
                conflictsVar,
                useClsConst, warningsConst, ignoreConst
            );

            // 9) Box the result
            var boxedResult = Expression.Convert(call, typeof(object));

            // 10) Return only the boxed result
            var body = Expression.Block(new[] { conflictsVar }, boxedResult);

            // 11) Compile to delegate
            var lambda = Expression.Lambda<RawGetTypeByMetadataNameDelegate>(
                body, pAsm, pName, pInclude, pWellKnown
            );
            return lambda.Compile();
        }
    }
}
