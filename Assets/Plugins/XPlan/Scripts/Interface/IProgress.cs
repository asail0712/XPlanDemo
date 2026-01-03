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

namespace XPlan.Interface
{
	public class ProgressLazyAdapter : Lazy<IProgress>, IProgress
	{
        public ProgressLazyAdapter(Func<IProgress> valueFactory) : base(valueFactory)
        {
        }

        public void Start()
        {
            Value.Start();
        }

        public void InProgress(string s, float f)
        {
            Value.InProgress(s, f);
        }

        public void Finish(bool b)
        {
            Value.Finish(b);
        }
    }


	public interface IProgress
    {
        void Start();
        void InProgress(string s, float f);
        void Finish(bool b);
    }
}

