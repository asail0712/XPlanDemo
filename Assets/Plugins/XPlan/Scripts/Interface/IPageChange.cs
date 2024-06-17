using System;

namespace XPlan.Interface
{
    public interface IPageChange
    {
        void SetPageCallback(Action<int> callback);

        void SetTotalPageNum(int pageNum);

        void SetAllowCycle(bool bAllowCycle);

        void RefershPageInfo();
    }
}