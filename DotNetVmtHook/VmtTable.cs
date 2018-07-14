using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace DotNetVmtHook
{
    public abstract class VmtTable : IDisposable, IEnumerable<IntPtr>
    {
        protected IntPtr InstanceAddress { get; set; }
        private bool _inited;

        protected VmtTable(IntPtr instanceAddress)
        {
            InstanceAddress = instanceAddress;
        }

        public IntPtr this[int index]
        {
            get => GetVmtFunctionAddress(index);
            set => SetVmtFunctionAddress(index, value);
        }

        public int Length { get; protected set; }

        protected int CountFuncs(IntPtr vmt)
        {
            int i = 0;
            do ++i; while (ReadIndex(vmt, i) != IntPtr.Zero);
            return i;
        }

        public IntPtr[] ToArray()
        {
            var list = new IntPtr[Length];
            for (int i = 0; i < Length; i++)
                list[i] = GetVmtFunctionAddress(i);
            return list;
        }

        public List<IntPtr> ToList()
        {
            var list = new List<IntPtr>(Length);
            for (int i = 0; i < Length; i++)
                list[i] = GetVmtFunctionAddress(i);
            return list;
        }

        public IEnumerator<IntPtr> GetEnumerator()
        {
            return ToList().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IntPtr GetVmtFunctionAddress(int index)
        {
            return ReadIndex(VmtAddress, index);
        }

        public void SetVmtFunctionAddress(int index, IntPtr value)
        {
            WriteIndex(VmtAddress, index, value);
        }

        protected void GuardIndex(long index)
        {
            if (index >= Length || index < 0)
                throw new IndexOutOfRangeException($"Index \"{index}\" must be greater than 0 and less than {Length}.");
        }

        public virtual void EnsureInitialized()
        {
            if (_inited)
                return;

            _inited = true;
        }

        public virtual void Dispose()
        {
            if (!_inited)
                return;

            Marshal.FreeHGlobal(VmtAddress);
            OriginalVmtAddress = IntPtr.Zero;

            //restore original vmt
            SetVmt(OriginalVmtAddress);

            InstanceAddress = IntPtr.Zero;
            _inited = true;
        }

        public IntPtr VmtAddress { get; protected set; }

        public IntPtr OriginalVmtAddress { get; protected set; }

        public TDelegate GetFunction<TDelegate>(int index)
        {
            return Marshal.GetDelegateForFunctionPointer<TDelegate>(GetVmtFunctionAddress(index));
        }

        public IntPtr GetOriginalFunctionAddress(int index)
        {
            return ReadIndex(OriginalVmtAddress, index);
        }

        protected abstract IntPtr ReadIndex(IntPtr ptr, int index);
        protected abstract void WriteIndex(IntPtr ptr, int index, IntPtr value);

        public TDelegate GetOriginalFunction<TDelegate>(int index)
        {
            return Marshal.GetDelegateForFunctionPointer<TDelegate>(GetOriginalFunctionAddress(index));
        }

        public void RestoreFunction(int index)
        {
            GuardIndex(index);
            SetVmtFunctionAddress(index, GetOriginalFunctionAddress(index));
        }

        public TDelegate ReplaceFunction<TDelegate>(int index, TDelegate target)
        {
            GuardIndex(index);
            var ptr = Marshal.GetFunctionPointerForDelegate(target);
            SetVmtFunctionAddress(index, ptr);
            return GetOriginalFunction<TDelegate>(index);
        }

        protected void SetVmt(IntPtr target)
        {
            WriteIndex(InstanceAddress, 0, target);
        }
    }
}