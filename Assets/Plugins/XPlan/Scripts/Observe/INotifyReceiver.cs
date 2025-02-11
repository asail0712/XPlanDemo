using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XPlan.Observe
{ 
    public interface INotifyReceiver
    {
        Func<string> GetLazyZoneID { get; set; }
    }
}