using System;
using UnityEngine;
using XPlan.UI.Fade;

namespace XPlan.UI
{
    internal static class ViewVisibilityHelper
    {
        public static void ToggleUI(GameObject ui, bool enabled)
        {
            if (ui.activeSelf == enabled)
                return;

            var fadeList = ui.GetComponents<FadeBase>();

            if (fadeList == null || fadeList.Length == 0)
            {
                ui.SetActive(enabled);
                return;
            }

            if (enabled)
            {
                ui.SetActive(true);

                Array.ForEach(fadeList, fadeComp =>
                {
                    if (fadeComp == null) return;
                    fadeComp.PleaseStartYourPerformance(true, null);
                });
            }
            else
            {
                int finishCounter = 0;

                Array.ForEach(fadeList, fadeComp =>
                {
                    if (fadeComp == null) return;

                    fadeComp.PleaseStartYourPerformance(false, () =>
                    {
                        if (++finishCounter == fadeList.Length)
                        {
                            ui.SetActive(false);
                        }
                    });
                });
            }
        }
    }
}
