# DotNetVmtHook
Allows you to hook a VMT of a native application from .NET Framework and .NET Core.


Performance appears to be good; but I didn't run any benchmarks yet and of course hooking natively will be much faster.

ONLY WORKS IF LOADED AS MODULE INTO TARGET PROCESS.

Example usage:
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
             return OriginalMethod(data, data2);
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
