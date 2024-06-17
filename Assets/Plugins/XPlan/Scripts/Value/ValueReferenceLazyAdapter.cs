using System;

using XPlan.Interface;

namespace XPlan.Value
{
    public class ValueReferenceLazyAdapter<T> : Lazy<IValueReference<T>>, IValueReference<T>
    {
        public ValueReferenceLazyAdapter(Func<IValueReference<T>> valueFactory) : base(valueFactory)
        {
        }

        T IValueReference<T>.Value { get => Value.Value; set => Value.Value = value; }
    }
}