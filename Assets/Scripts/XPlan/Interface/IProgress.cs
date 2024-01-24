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

