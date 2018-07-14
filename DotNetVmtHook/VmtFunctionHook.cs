using System;

namespace DotNetVmtHook
{
    public abstract class VmtFunctionHook<TVmt, TDelegate>  where TVmt : Vmt32Table
    {
        private readonly int _methodIndex;
        private readonly TVmt _vmt;
        protected TDelegate OriginalFunction { get; private set; }
        private TDelegate _callback;

        protected VmtFunctionHook(TVmt vmt, int methodIndex)
        {
            vmt.EnsureInitialized();

            _methodIndex = methodIndex;
            _vmt = vmt;
        }

        protected abstract TDelegate GetCallback();

        public void Dispose()
        {
            _vmt.RestoreFunction(_methodIndex);
        }

        public void HookMethods()
        {
            _callback = GetCallback();
            GC.KeepAlive(_callback);
            OriginalFunction = _vmt.ReplaceFunction(_methodIndex, _callback);
            GC.KeepAlive(OriginalFunction);
        }
    }
}