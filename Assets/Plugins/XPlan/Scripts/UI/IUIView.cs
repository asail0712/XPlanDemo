namespace XPlan.UI
{
    public interface IUIView
    {
        int SortIdx { get; set; }

        void RefreshLanguage();
    }
}