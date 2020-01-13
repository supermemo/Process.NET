using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Process.NET.Applied
{
    [SuppressMessage("ReSharper", "LoopCanBePartlyConvertedToQuery")]
    public class AppliedManager<T> : IAppliedManager<T> where T : IApplied
    {
        protected readonly Dictionary<string, T> _internalItems = new Dictionary<string, T>();

        public IReadOnlyDictionary<string, T> Items => _internalItems;

        public void Disable(T item)
        {
            throw new NotImplementedException();
        }

        public void Disable(string name)
        {
            throw new NotImplementedException();
        }

        public T this[string key] => _internalItems[key];

        public void EnableAll()
        {
            foreach (var item in _internalItems)
                if (!item.Value.IsEnabled)
                    item.Value.Disable();
        }

        public void DisableAll()
        {
            foreach (var item in _internalItems)
                if (item.Value.IsEnabled)
                    item.Value.Disable();
        }

        public void Remove(string name)
        {
            if (!_internalItems.ContainsKey(name))
                return;

            try
            {
                _internalItems[name].Dispose();
            }

            finally
            {
                _internalItems.Remove(name);
            }
        }

        public void Remove(T item)
        {
            Remove(item.Identifier);
        }

        public void RemoveAll()
        {
            foreach (var item in _internalItems)
                item.Value.Dispose();
            _internalItems.Clear();
        }

        public void Add(T applicable)
        {
            _internalItems.Add(applicable.Identifier, applicable);
        }

        public void Add(IEnumerable<T> applicableRange)
        {
            foreach (var applicable in applicableRange)
                Add(applicable);
        }
    }
}