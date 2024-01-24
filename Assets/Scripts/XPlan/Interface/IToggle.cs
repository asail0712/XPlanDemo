namespace XPlan.Interface
{
    public interface IToggle
    {
        void Toggle();

        void Switch(bool bEnable);

        void TurnOn();

        void TurnOff();
    }
}