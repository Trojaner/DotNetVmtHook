using System;
using System.Runtime.InteropServices;

namespace DotNetVmtHook
{
    public unsafe class Vmt32Table : VmtTable
    {
        public Vmt32Table(IntPtr instanceAddress) : base(instanceAddress)
        {
        }

        public override void EnsureInitialized()
        {
            base.EnsureInitialized();

            if (OriginalVmtAddress != IntPtr.Zero)
                return;

            OriginalVmtAddress = (IntPtr)((int**)InstanceAddress)[0];

            var vmt = (int**)OriginalVmtAddress;
            Length = CountFuncs((long**)vmt) * sizeof(int);

            int copySize = Length + sizeof(int);
            int** newVmt = (int**)Marshal.AllocHGlobal(copySize);
            Buffer.MemoryCopy((int*)vmt, (int*)newVmt, copySize, copySize);

            VmtAddress = (IntPtr)newVmt;
            SetVmt(VmtAddress);
        }

        protected override IntPtr ReadIndex(IntPtr ptr, int index)
        {
            return (IntPtr)((int**) ptr)[index];
        }

        protected override void WriteIndex(IntPtr ptr, int index, IntPtr value)
        {
            ((int**)ptr)[index] = (int*) value;
        }
    }
}