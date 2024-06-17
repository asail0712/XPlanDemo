using XPlan.Interface;

namespace XPlan.Value
{
    public class StateValue<T> : IStateValue<T>
    {
        public StateValue(T state)
        {
            State = state;
        }

        public T State { get; set; }
    }
}