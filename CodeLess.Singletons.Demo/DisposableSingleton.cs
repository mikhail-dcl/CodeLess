using CodeLess.Attributes;
using System;

[Singleton]
public partial class DisposableSingleton : IDisposable
{
    public void Dispose()
    {
        // Dispose logic here
    }
}
