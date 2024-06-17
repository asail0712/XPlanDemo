namespace XPlan.Interface
{
    public interface IStateValue<T>
    {
        T State { get; set; }
    }
}