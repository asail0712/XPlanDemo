using XPlan.Utility;
using XPlan.Interface;


namespace XPlan.UI
{
    public class SingleClickStateValueAdapter : CreateSingleton<SingleClickStateValueAdapter>, IStateValue<bool>
    {
        private IStateValue<bool> stateValue;

        public bool State { get => stateValue.State; set => stateValue.State = value; }

        protected override void InitSingleton()
        {
            stateValue = GlobalState.IsClicked;
        }
    }
}