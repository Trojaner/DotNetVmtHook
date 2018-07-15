# DotNetVmtHook [![DotNetVmtHook NuGet](https://img.shields.io/nuget/v/DotNetVmtHook.svg)](https://www.nuget.org/packages/DotNetVmtHook)
Allows you to hook a VMT of a native application from .NET Framework and .NET Core.


Performance appears to be good; but I didn't run any benchmarks yet and of course hooking natively will be much faster.

Note: this uses shadow VMT and does not modify the original VMT function addresses. Instead, it replaces the object's VMT pointer.
If you plan to use this in any game for "cheating": Don't. Most anti-cheats will detect this anyway.

ONLY WORKS IF LOADED AS MODULE INTO TARGET PROCESS.

# Example
```cs
    [SuppressUnmanagedCodeSecurity] //to make calls faster
    [UnmanagedFunctionPointer(CallingConvention.StdCall)] //depends on the native method
    [return: MarshalAs(UnmanagedType.I1)] //depends on the native method
    public unsafe delegate bool SomeMethodDelegate(void* data, int data2); //Adjust your signature to the native function
    
    public class SomeFunctionHook : VmtFunctionHook<SomeMethodDelegate>
    {
        public SomeFunctionHook(VmtTable vmt) : base(vmt, NATIVE_FUNCTION_INDEX_HERE) { }
    
        private unsafe bool SomeFunction_Hooked(data, data2)
        {
             //ANYTHING USED FROM OUTSIDE HERE NEEDS Gc.KeepAlive()!!!
             //Otherwise GC might loose track of it and collect it; resulting in native crash
             //...
             //Do Something
             return OriginalFunction(data, data2);
        }
 
        protected override unsafe SomeMethodDelegate GetCallback()
        {
            return SomeFunction_Hooked;
        }
    }    
    
    //Usage
    VmtTable vmt = new Vmt32Table(YOUR_NATIVE_INSTANCE_POINTER_HERE);
    var functionHook = new SomeFunctionHook(vmt);
    functionHook.Hook();
    
    // Somewhen later; when you want to clean up (restores original functions):
    functionHook.Dispose();
    vmt.Dispose();
```

# Compatibility
Works with .NET 4.5 / .NET Standard 1.3 (Depends on Buffer.MemoryCopy)

Should work on Linux; afaik you have to +1 the function indexes there (e.g. if your function index is 0x03 on Windows, its 0x04 on Linux).


# License
Everything is licensed MIT. Please provide credit if you use this.