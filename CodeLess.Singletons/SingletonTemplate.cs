using CodeLess.Common;
using Microsoft.CodeAnalysis;
using System.Linq;
using System.Text;

namespace CodeLess.Singletons
{
    public class SingletonTemplate : TypeBuilder
    {
        private string BuildExplicitInstance() =>
            $$"""
              public static {{ClassName}} Instance
              {
                  get
                  {
                      lock (syncObj)
                      {
                          if (instance == null)
                              throw new ArgumentNullException(nameof(instance), $"{nameof({{ClassName}})} is not initialized. Call {nameof(Initialize)} before accessing the instance");

                          return instance;
                      }
                  }
              }
              """;

        private string BuildImplicitInstance() =>
            $$"""
              public static {{ClassName}} Instance
              {
                  get
                  {
                      lock (syncObj)
                      {
                          if (instance == null)
                              instance = new {{ClassName}}();

                          return instance;
                      }
                  }
              }
              """;

        private string BuildInitializeMethod() =>
            $$"""
               public static void Initialize({{ClassName}} instance)
               {
                  lock (syncObj)
                  {
                      if ({{ClassName}}.instance != null)
                          throw new InvalidOperationException($"{nameof({{ClassName}})} is already initialized.");

                      {{ClassName}}.instance = instance;
                  }
               }
               """;

        private bool buildImplicitInstance;

        private const string DISPOSE_CALL =
            """
            if (dispose)
                instance?.Dispose();
            """;

        private readonly StringBuilder staticAccessors = new (500);

        private readonly StringBuilder disposeCall = new (DISPOSE_CALL.Length);

        internal override string BuildSource() =>
            $$"""
               #nullable enable
               using System;
               {{Usings}}
               {{NamespaceLeading}}
                 {{AccessModifier}} partial class {{ClassName}}{{TypeGenericArguments}}
                 {
                     private static {{ClassName}}? instance = null;
                     private static readonly object syncObj = new();

                     {{(buildImplicitInstance ? BuildImplicitInstance() : BuildExplicitInstance())}}

                     {{(!buildImplicitInstance ? BuildInitializeMethod() : string.Empty)}}

                     public static void Reset(bool dispose = true)
                     {
                        lock (syncObj)
                        {
                            {{disposeCall}}
                            instance = null;
                        }
                     }

                     {{staticAccessors}}
                 }
               {{NamespaceTrailing}}
               """;

        internal void SetImplicitInstance()
        {
            buildImplicitInstance = true;
        }

        internal void SetDisposeCall(in GeneratorTypeInfo typeInfo)
        {
            disposeCall.Clear();

            // Generate a dispose call only if the type implements IDisposable
            if (typeInfo.Symbol.AllInterfaces.Any(static i => i.GetFullyQualifiedName() == Consts.DISPOSE_FULLY_QUALIFIED_NAME))
                disposeCall.Append(DISPOSE_CALL);
        }

        internal void SetStaticAccessors(in GeneratorTypeInfo typeInfo)
        {
            staticAccessors.Clear();

            foreach (ISymbol member in typeInfo.Symbol.GetMembers())
            {
                if (member.DeclaredAccessibility != Accessibility.Internal || member.IsStatic)
                    continue;

                var memberName = member.Name;
                var staticName = char.ToUpper(memberName[0]) + memberName.Substring(1);

                switch (member)
                {
                    case IPropertySymbol prop:
                        // Only non-indexer, readable or writable
                        if (prop.IsIndexer) continue;
                        var type = prop.Type.ToDisplayString();

                        if (prop.GetMethod != null && prop.SetMethod != null)
                        {
                            staticAccessors.AppendLine($$"""
                                                         {{prop.InheritDoc()}}
                                                         public static {{type}} {{staticName}}
                                                         {
                                                            get => Instance.{{memberName}};
                                                            set => Instance.{{memberName}} = value;
                                                         }
                                                         """);
                        }
                        else if (prop.GetMethod != null)
                        {
                            staticAccessors.AppendLine($$"""
                                                         {{prop.InheritDoc()}}
                                                         public static {{type}} {{staticName}} => Instance.{{memberName}};
                                                         """);
                        }
                        else if (prop.SetMethod != null)
                        {
                            staticAccessors.AppendLine($$"""
                                                         {{prop.InheritDoc()}}
                                                         public static {{type}} {{staticName}} { set => Instance.{{memberName}} = value; }
                                                         """);
                        }

                        break;

                    case IMethodSymbol method:
                        // Exclude constructors, property/event accessors, operators, and static methods
                        if (method.MethodKind != MethodKind.Ordinary)
                            continue;

                        var returnType = method.ReturnType.ToDisplayString();
                        var parameters = string.Join(", ", method.Parameters.Select(p => $"{(p.RefKind != RefKind.None ? p.RefKind.ToString().ToLower() + " " : "")}{p.Type.ToDisplayString()} {p.Name}"));
                        var args = string.Join(", ", method.Parameters.Select(p => $"{(p.RefKind != RefKind.None ? p.RefKind.ToString().ToLower() + " " : "")}{p.Name}"));

                        staticAccessors.AppendLine($"""
                                                    {method.InheritDoc()}
                                                    public static {returnType} {staticName}({parameters}) => Instance.{memberName}({args});
                                                    """);

                        break;

                    case IEventSymbol evt:
                        var evtType = evt.Type.ToDisplayString();

                        staticAccessors.AppendLine($$"""
                                                     {{evt.InheritDoc()}}
                                                     public static event {{evtType}} {{staticName}}
                                                     {
                                                        add { Instance.{{memberName}} += value; }
                                                        remove { Instance.{{memberName}} -= value; }
                                                     }
                                                     """);

                        break;
                }
            }
        }

        internal override void Clear()
        {
            base.Clear();
            buildImplicitInstance = false;
            staticAccessors.Clear();
            disposeCall.Clear();
        }
    }
}
