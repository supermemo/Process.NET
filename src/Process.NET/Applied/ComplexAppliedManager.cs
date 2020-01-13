namespace Process.NET.Applied
{
    public class ComplexAppliedManager<T> : AppliedManager<T>, IComplexAppliedManager<T> where T : IComplexApplied
    {
        public void Disable(T item, bool dueToRules)
        {
            Disable(item.Identifier, dueToRules);
        }

        public void Disable(string name, bool dueToRules)
        {
            _internalItems[name].Disable(dueToRules);
        }

        public void Enable(T item, bool dueToRules)
        {
            Enable(item.Identifier, dueToRules);
        }

        public void Enable(string name, bool dueToRules)
        {
            _internalItems[name].Enable(dueToRules);
        }

        public void DisableAll(bool dueToRules)
        {
            foreach (var value in _internalItems.Values)
                value.Disable(dueToRules);
        }

        public void EnableAll(bool dueToRules)
        {
            foreach (var value in _internalItems.Values)
                value.Enable(dueToRules);
        }
    }
}