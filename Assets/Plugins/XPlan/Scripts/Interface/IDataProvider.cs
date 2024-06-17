using System.Threading.Tasks;

namespace XPlan.Interface
{
    public interface IDataProvider<T>
    {
        Task<T> GetAsync();
    }
}