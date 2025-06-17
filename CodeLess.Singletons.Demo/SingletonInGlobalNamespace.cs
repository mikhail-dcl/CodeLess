using CodeLess.Attributes;

[Singleton]
// ReSharper disable once CheckNamespace
public partial class SingletonInGlobalNamespace(string name)
{
    public string Name { get; } = name;
}
