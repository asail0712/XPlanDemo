using System;

namespace XPlan.Interface
{
    public interface IToggleButton
    {
        event Action onClick;

		void Toggle();

		void Switch(bool bEnable);

		void PressBtn();

		void ReleaseBtn();
	}
}