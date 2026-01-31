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

    public class ActivityTracker : IActivityTracker<List<TrackerInfo>>
    {
        public event Action<List<TrackerInfo>> OnFeatureTouched;

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
            bool bFirstTouch = trackerInfos.Count == 0;
            TrackerInfo info = trackerInfos.FirstOrDefault(t => t.FeatureName == feature);

            if (info != null)
            {
                info.LateTouched = DateTime.UtcNow;
                info.NeedFlush   = true;
            }
            else
            {
                trackerInfos.Add(new TrackerInfo()
                {
                    FeatureName = feature,
                    LateTouched = DateTime.UtcNow,
                    NeedFlush   = true
                });
            }

            // 第一次touch 強制送出
            if(bFirstTouch)
            {
                Flush();
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
            List<TrackerInfo> trackerList = trackerInfos.Where(t => t.NeedFlush || bForce).ToList();

            OnFeatureTouched?.Invoke(trackerList);
        }
    }
}
