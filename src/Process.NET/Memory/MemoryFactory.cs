using System;
using System.Collections.Generic;
using System.Linq;
using Process.NET.Native.Types;
using Process.NET.Utilities;

namespace Process.NET.Memory
{
    /// <summary>
    ///     Class providing tools for manipulating memory space.
    /// </summary>
    public class MemoryFactory : IMemoryFactory
    {
        /// <summary>
        ///     The list containing all allocated memory.
        /// </summary>
        protected readonly List<IAllocatedMemory> _internalRemoteAllocations;

        /// <summary>
        ///     The reference of the <see cref="_process" /> object.
        /// </summary>
        protected readonly IProcess _process;

        /// <summary>
        ///     Initializes a new instance of the <see cref="MemoryFactory" /> class.
        /// </summary>
        /// <param name="process">The reference of the <see cref="_process" /> object.</param>
        public MemoryFactory(IProcess process)
        {
            // Save the parameter
            _process = process;
            // Create a list containing all allocated memory
            _internalRemoteAllocations = new List<IAllocatedMemory>();
        }

        /// <summary>
        ///     A collection containing all allocated memory in the remote process.
        /// </summary>
        public IEnumerable<IAllocatedMemory> Allocations => _internalRemoteAllocations.AsReadOnly();

        /// <summary>
        ///     Gets the <see cref="IAllocatedMemory" /> with the specified name.
        /// </summary>
        /// <value>
        ///     The <see cref="IAllocatedMemory" />.
        /// </value>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public IAllocatedMemory this[string name]
        {
            get { return _internalRemoteAllocations.FirstOrDefault(am => am.Identifier == name); }
        }

        /// <summary>
        ///     Gets all blocks of memory allocated in the remote process.
        /// </summary>
        public IEnumerable<MemoryRegion> Regions
        {
            get
            {
                var size = IntPtr.Size;
                var adresseTo = size == 8 ? new IntPtr(0x7fffffffffffffff) : new IntPtr(0x7fffffff);
                return
                    MemoryHelper.Query(_process.Handle, IntPtr.Zero, adresseTo)
                        .Select(page => new MemoryRegion(_process, page.BaseAddress));
            }
        }

        /// <summary>
        ///     Allocates a region of memory within the virtual address space of the remote process.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="size">The size of the memory to allocate.</param>
        /// <param name="protection">The protection of the memory to allocate.</param>
        /// <param name="mustBeDisposed">The allocated memory will be released when the finalizer collects the object.</param>
        public IAllocatedMemory Allocate(string name, int size,
            MemoryProtectionFlags protection = MemoryProtectionFlags.ExecuteReadWrite, bool mustBeDisposed = true)
        {
            // Allocate a memory space
            var memory = new AllocatedMemory(_process, name, size, protection, mustBeDisposed);
            // Add the memory in the list
            _internalRemoteAllocations.Add(memory);
            return memory;
        }

        /// <summary>
        ///     Deallocates a region of memory previously allocated within the virtual address space of the remote process.
        /// </summary>
        /// <param name="allocation">The allocated memory to release.</param>
        public void Deallocate(IAllocatedMemory allocation)
        {
            // Dispose the element
            if (!allocation.IsDisposed)
                allocation.Dispose();
            // Remove the element from the allocated memory list
            if (_internalRemoteAllocations.Contains(allocation))
                _internalRemoteAllocations.Remove(allocation);
        }

        /// <summary>
        ///     Releases all resources used by the <see cref="MemoryFactory" /> object.
        /// </summary>
        public virtual void Dispose()
        {
            // Release all allocated memories which must be disposed
            foreach (var allocatedMemory in _internalRemoteAllocations.Where(m => m.MustBeDisposed).ToArray())
                allocatedMemory.Dispose();
            // Avoid the finalizer
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Frees resources and perform other cleanup operations before it is reclaimed by garbage collection.
        /// </summary>
        ~MemoryFactory()
        {
            Dispose();
        }
    }
}