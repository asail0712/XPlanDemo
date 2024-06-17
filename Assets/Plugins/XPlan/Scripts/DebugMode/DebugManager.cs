using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XPlan;

namespace XPlan.DebugMode
{ 
    public class DebugManager : InstallerBase
    {
        [SerializeField]
        private GameObject debugConsole;

		// Start is called before the first frame update
		protected override void OnInitialHandler()
		{
            RegisterHandler(new DebugHandler(debugConsole));
        }
    }
}
