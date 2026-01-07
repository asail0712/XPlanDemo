// ==============================================================================
// XPlan Framework
//
// Copyright (c) 2026 Asail
// All rights reserved.
//
// Author  : Asail0712
// Project : XPlan
// Description:
//     A modular framework for Unity projects, focusing on MVVM architecture,
//     runtime tooling, event-driven design, and extensibility.
//
// Contact : asail0712@gmail.com
// GitHub  : https://github.com/asail0712/XPlanDemo
//
// Unauthorized copying, modification, or distribution of this file,
// via any medium, is strictly prohibited without prior permission.
// ==============================================================================

using System;
using System.Collections.Generic;
using System.Linq;

namespace XPlan.Activity
{
    public class TrackerInfo
    {
        public string FeatureName { get; set; }
        public DateTime LateTouched { get; set; }
        public bool NeedFlush { get; set; }
    }

    public class ActivityTracker : IActivityTracker
    {
        public event Action<string, DateTime> OnFeatureTouched;

        private readonly List<TrackerInfo> trackerInfos;
        private readonly float flushDelaySeconds = 60;
        private DateTime lastFlushTime;

        public ActivityTracker(float flushDelaySeconds)
        {
            this.trackerInfos       = new List<TrackerInfo>();
            this.flushDelaySeconds  = flushDelaySeconds;
            this.lastFlushTime      = DateTime.Now;
        }

        public void Touch(string feature)
        {
            TrackerInfo info = trackerInfos.FirstOrDefault(t => t.FeatureName == feature);

            if (info != null)
            {
                info.LateTouched = DateTime.Now;
                info.NeedFlush   = true;
            }
            else
            {
                trackerInfos.Add(new TrackerInfo()
                {
                    FeatureName = feature,
                    LateTouched = DateTime.Now,
                    NeedFlush   = true
                });
            }
        }

        public void Tick()
        {
            var now = DateTime.Now;

            if((now - lastFlushTime).TotalSeconds >= flushDelaySeconds)
            {
                lastFlushTime = now;
                Flush();
            }
        }

        public void Flush(bool bForce = false)
        {
            foreach (var item in trackerInfos)
            {
                if(!item.NeedFlush && !bForce)
                    continue;

                OnFeatureTouched?.Invoke(item.FeatureName, item.LateTouched);

                item.NeedFlush = false;
            }
        }
    }
}
