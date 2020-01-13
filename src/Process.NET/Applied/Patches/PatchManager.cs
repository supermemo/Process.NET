using System;
using Process.NET.Memory;

namespace Process.NET.Applied.Patches
{
    /// <summary>
    ///     A manager class to handle memory patches.
    ///     <remarks>All credits to Apoc.</remarks>
    /// </summary>
    public class PatchManager : ComplexAppliedManager<Patch>, IDisposable
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PatchManager" /> class.
        /// </summary>
        /// <param name="processMemory">The process memory.</param>
        public PatchManager(IMemory processMemory)
        {
            MemoryBase = processMemory;
        }

        /// <summary>
        ///     The reference of the <see cref="MemoryBase" /> object.
        ///     <remarks>This value is invalid if the manager was created for the <see cref="ProcessSharp" /> class.</remarks>
        /// </summary>
        protected IMemory MemoryBase { get; }

        /// <summary>
        ///     Releases unmanaged and - optionally - managed resources.
        /// </summary>
        public void Dispose()
        {
            RemoveAll();
        }

        /// <summary>
        ///     Creates a new <see cref="Patch" /> at the specified address.
        /// </summary>
        /// <param name="address">The address to begin the patch.</param>
        /// <param name="patchWith">The bytes to be written as the patch.</param>
        /// <param name="name">The name of the patch.</param>
        /// <returns>A patch object that exposes the required methods to apply and remove the patch.</returns>
        public Patch Create(IntPtr address, byte[] patchWith, string name)
        {
            if (_internalItems.ContainsKey(name))
                return _internalItems[name];
            _internalItems.Add(name, new Patch(address, patchWith, name, MemoryBase));
            return _internalItems[name];
        }

        /// <summary>
        ///     Creates a new <see cref="Patch" /> at the specified address, and applies it.
        /// </summary>
        /// <param name="address">The address to begin the patch.</param>
        /// <param name="patchWith">The bytes to be written as the patch.</param>
        /// <param name="name">The name of the patch.</param>
        /// <returns>A patch object that exposes the required methods to apply and remove the patch.</returns>
        public Patch CreateAndApply(IntPtr address, byte[] patchWith, string name)
        {
            Create(address, patchWith, name);
            _internalItems[name].Enable();
            return _internalItems[name];
        }
    }
}