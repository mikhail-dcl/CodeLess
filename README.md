# CodeLess
Auto-generation of common simple patterns that can be applied in the high-scale Unity projects

## Singleton

The main disadvantage of the Singleton pattern is inability to control its lifecycle, the usage scope and use it in the unit tests.

However, using the Singleton pattern still can be beneficial to simplify dependency management and open up static-like access to the classes that by nature must exist in a single instance and don't contain any logic that can alter the state of the application. 

Despite you may achieve it by using a base generic class it has obvious drawbacks:
- It requires the class to inherit from the base class, thus limiting the usage of the inheritance;
- The usage of the `Instance` property will lead to the base generic class
- It may harm the visual simplicity of the code

**`SingletonAttribute`**

Annotating a class with  the `SingletonAttribute` will generate the code to make it an advanced Singleton that resolves the main drawbacks of the classic Singleton pattern:
- The singleton lifecycle can be controlled explicitly based on the provided `SingletonGenerationBehavior` enum value
   - By default, if `ALLOW_IMPLICIT_CONSTRUCTION` is not specified, the singleton won't be created on demand
      - It gives the full initialization control to the developer. The singleton then must be set explicitly by calling the generated `Initialize` method;
      - If the singleton accessed prematurely, it will throw an `ArgumentNullException` exception;
      - The singleton will require a default constructor to be present;
   - If `ALLOW_IMPLICIT_CONSTRUCTION` is specified, the singleton will be created on demand when accessed for the first time
      - If the instance is created implicitly it can't be initialized explicitly, i.e. the `Initialize` method will not be generated;
   - For all singletons `Reset` method will be generated;
      - The `Reset` method includes an optional parameter "dispose" ("true" by default) that allows the developer to dispose the singleton instance if it implements `IDisposable` before resetting it;
   - `SingletonRegistry` class will be generated to include calls to all referenced Singletons
       - It provides a way to reset all singletons at once without writing the code manually;
       - It is expected to be used when the game/application session is over: e.g. when the player disconnects, starts another session, logs out, etc.
- Additionally by specifying `GENERATE_STATIC_ACCESSORS` the static accessors will be generated to access the singleton instance without the need to call `Singleton.Instance` every time
   - Thus, the developer may explicitly specify that the class will be used in a static-like manner only

### Examples

**Default Singleton Behavior**

```csharp
using CodeLess.Attributes;

namespace CodeLess.Tests.Module1
{
    [Singleton]
    public partial class SingletonType1
    {
        private readonly string param;

        public SingletonType1(string param)
        {
            this.param = param;
        }
    }
}
```

The generated code:

```csharp
#nullable enable
using System;
using CodeLess.Attributes;

namespace CodeLess.Tests.Module1
{
    public partial class SingletonType1
    {
        private static SingletonType1? instance = null;
        private static readonly object syncObj = new();
        public static SingletonType1 Instance
        {
            get
            {
                lock (syncObj)
                {
                    if (instance == null)
                        throw new ArgumentNullException(nameof(instance), $"{nameof(SingletonType1)} is not initialized. Call {nameof(Initialize)} before accessing the instance");
                    return instance;
                }
            }
        }

        public static void Initialize(SingletonType1 instance)
        {
            lock (syncObj)
            {
                if (SingletonType1.instance != null)
                    throw new InvalidOperationException($"{nameof(SingletonType1)} is already initialized.");
                SingletonType1.instance = instance;
            }
        }

        public static void Reset(bool dispose = true)
        {
            lock (syncObj)
            {
                instance = null;
            }
        }
    }
}
```

**Implicit Construction Allowed**

```csharp
    public partial class ChatCommandsBus
    {
        public event Action<bool> ConnectionStatusPanelVisibilityChanged;

        public void SendConnectionStatusPanelChangedNotification(bool isVisible)
        {
            ConnectionStatusPanelVisibilityChanged?.Invoke(isVisible);
        }
    }
```

The generated code:
```csharp
    [Singleton(SingletonGenerationBehavior.ALLOW_IMPLICIT_CONSTRUCTION)]
    public partial class ChatCommandsBus
    {
        private static ChatCommandsBus? instance = null;
        private static readonly object syncObj = new();
        public static ChatCommandsBus Instance
        {
            get
            {
                lock (syncObj)
                {
                    if (instance == null)
                        instance = new ChatCommandsBus();
                    return instance;
                }
            }
        }

        public static void Reset(bool dispose = true)
        {
            lock (syncObj)
            {
                instance = null;
            }
        }
    }
```

**Static Accessors**

```csharp
    [Singleton(SingletonGenerationBehavior.ALLOW_IMPLICIT_CONSTRUCTION | SingletonGenerationBehavior.GENERATE_STATIC_ACCESSORS)]
    public partial class PhysicsTickProvider
    {
        internal int tick { get; set; }
    }
```

The generated code:
```csharp
    public partial class PhysicsTickProvider
    {
        private static PhysicsTickProvider? instance = null;
        private static readonly object syncObj = new();
        public static PhysicsTickProvider Instance
        {
            get
            {
                lock (syncObj)
                {
                    if (instance == null)
                        instance = new PhysicsTickProvider();
                    return instance;
                }
            }
        }

        public static void Reset(bool dispose = true)
        {
            lock (syncObj)
            {
                instance = null;
            }
        }

        /// <inheritdoc cref = "DCL.Time.PhysicsTickProvider.tick"/>
        public static int Tick { get => Instance.tick; set => Instance.tick = value; }
    }
```

**Disposable Singleton**

```csharp
[Singleton]
public partial class DisposableSingleton : IDisposable
{
    public void Dispose()
    {
        // Dispose logic here
    }
}
```

The generated code:

```csharp
public partial class DisposableSingleton
{
    private static DisposableSingleton? instance = null;
    private static readonly object syncObj = new();
    public static DisposableSingleton Instance
    {
        get
        {
            lock (syncObj)
            {
                if (instance == null)
                    throw new ArgumentNullException(nameof(instance), $"{nameof(DisposableSingleton)} is not initialized. Call {nameof(Initialize)} before accessing the instance");
                return instance;
            }
        }
    }

    public static void Initialize(DisposableSingleton instance)
    {
        lock (syncObj)
        {
            if (DisposableSingleton.instance != null)
                throw new InvalidOperationException($"{nameof(DisposableSingleton)} is already initialized.");
            DisposableSingleton.instance = instance;
        }
    }

    public static void Reset(bool dispose = true)
    {
        lock (syncObj)
        {
            if (dispose)
                instance?.Dispose();
            instance = null;
        }
    }
}
```

**Singleton Registry Auto-Generated**

```csharp
       #nullable enable
       using CodeLess.Tests.Module1;
       using CodeLess.Tests;
       using CodeLess.Singletons.Tests;

       namespace CodeLess.Singletons
       {
           public class SingletonRegistry
           {
                public static void Reset(bool dispose = true)
                {
                    SingletonType1.Reset(dispose);
                    SingletonType2.Reset(dispose);
                    FeatureFlagsGenerated.Reset(dispose);
                }
           }
       }
```

### Prevent State Leakage in Unit Tests

If a regular Singleton is used its state will leak between unit tests, thus leading to the false positives and negatives in the test results.

To prevent it the following scheme should be applied to every test assembly:

```csharp
using CodeLess.Singletons;

    [assembly: ResetSingletonsInTests]
        
    public class ResetSingletonsInTestsAttribute : TestActionAttribute
    {
        public override void AfterTest(ITest test)
        {
            SingletonRegistry.Reset();
        }
    }
```

By doing this the `SingletonRegistry.Reset` will be called after each `TestFixture` preventing the state leakage between tests.
The `SingletonRegistry` is a no-brainer as it is auto-generated from the current and all referenced projects.

## Interfaces

The main idea behind auto-generating interfaces is to reduce the boilerplate code needed.

There several cases where this approaches will shine:
- An interface is symmetric to the class, i.e. it has the same [open] methods and properties;
- An interface exists only for possibility to mock the class in Unit tests;
- An interface doesn't contain any additional logic and is not segregated into multiple ones;

**[AutoInterface] attribute**

Annotating a class with the `AutoInterface` attribute will generate an interface according to the following rules:
- The interface will be named as the symbol that a developer specified in place of the interface
- The specified symbol should have the same generic signature as the class
- There should be no other non-partial interface with the same name
- The interface will be generated in the same namespace as the class
- The interface will contain all public and internal methods, properties and events of the class
- If the member is annotated with `IgnoreAutoInterfaceMember` attribute it will be ignored and not included in the generated interface


### Examples

```csharp
using System;
using System.Threading.Tasks;

namespace CodeLess.Interfaces.Tests
{
    [AutoInterface]
    public class AllFunctionalities : IAllFunctionalities
    {
        [Obsolete]
        public event Action SimpleEvent;
        public event EventHandler<int> GenericEvent;
        public event Action InitEvent = () => { };
        public event EventHandler ExplicitEvent
        {
            add { }
            remove { }
        }
        [Obsolete("test")]
        public event EventHandler AttributedEvent = delegate { };
        /// <summary>Xml doc event</summary>
        public event EventHandler XmlDocEvent;
        
        /// <summary>XML doc property</summary>
        public string DocProp { get; set; }
        public int ReadOnlyProp { get; }
        public string WriteOnlyProp { set { } }
        public string InitOnlyProp { get; init; }
        public string ExprBodiedProp => "val";
        public int this[int idx] { get => 0; set { } }

        public void Method() { }
        public Task<int> AsyncMethod() => Task.FromResult(1);
        public TResult GenericMethod<T, TResult>(T input) => default!;
        public void MethodWithParams(int a, string b, out bool ok, in double pi, ref long x, Action callback)
        {
            ok = true;
        }
        public (string, int) MethodOutInRefReturn(in string s, ref int count, out bool success)
        {
            success = true;
            return (s, count);
        }
    }
}
```

The generated interface:

```csharp
#nullable enable
using System;
using System.Threading.Tasks;

namespace CodeLess.Interfaces.Tests
{
    public partial interface IAllFunctionalities
    {
        [Obsolete]
        event Action SimpleEvent;
        
        event EventHandler<int> GenericEvent;
        
        event Action InitEvent;
        
        event EventHandler ExplicitEvent;
        
        [Obsolete("test")]
        event EventHandler AttributedEvent;
        
        /// <summary>Xml doc event</summary>
        event EventHandler XmlDocEvent;
        
        /// <summary>XML doc property</summary>
        string DocProp { get; set; }

        int ReadOnlyProp { get; }

        string WriteOnlyProp { set; }

        string InitOnlyProp { get; init; }

        string ExprBodiedProp { get; }

        int this[int idx] { get; set; }

        void Method();
        
        Task<int> AsyncMethod();
        
        TResult GenericMethod<T, TResult>(T input);
        
        void MethodWithParams(int a, string b, out bool ok, in double pi, ref long x, Action callback);
        
        (string, int) MethodOutInRefReturn(in string s, ref int count, out bool success);
    }
}
```

### Roadmap/Currently not supported

- [x] Generic Types are not supported yet (Generic members are supported)
- [ ] Ignore members of other interfaces
- [ ] Internal accessors are not properly supported