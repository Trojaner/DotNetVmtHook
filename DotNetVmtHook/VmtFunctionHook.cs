using System;

namespace DotNetVmtHook
{
    public abstract class VmtFunctionHook<TDelegate> 
    {
        private readonly int _functionIndex;
        private readonly VmtTable _vmt;
        protected TDelegate OriginalFunction { get; private set; }
        private TDelegate _callback;

        protected VmtFunctionHook(VmtTable vmt, int functionIndex)
        {
            vmt.EnsureInitialized();

            _functionIndex = functionIndex;
            _vmt = vmt;
        }

        protected abstract TDelegate GetCallback();

        public void Dispose()
        {
            _vmt.RestoreFunction(_functionIndex);
        }

        public void Hook()
        {
            _callback = GetCallback();
            GC.KeepAlive(_callback);
            OriginalFunction = _vmt.ReplaceFunction(_functionIndex, _callback);
            GC.KeepAlive(OriginalFunction);
        }
    }
}