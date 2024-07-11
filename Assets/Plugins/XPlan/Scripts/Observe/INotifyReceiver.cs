using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XPlan.Observe
{ 
    public interface INotifyReceiver
    {
        //void ReceiveNotify(MessageReceiver msgReceiver);

        Func<string> LazyGroupID { get; set; }
    }
}