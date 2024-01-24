using System;
using UnityEngine;

namespace XPlan.UI
{
    public class UIDataContainer : UIParam
    {
        protected override object GetValue()
        {
            return this;
        }
    }

    public class UIParam
    {
        public T GetValue<T>()
		{
            return (T)GetValue();
		}

        protected virtual object GetValue()
        {
            return null;
        }
    }

    public class IntParam : UIParam
    {
        public int value;

        public IntParam(int v)
		{
            value = v;
		}
        protected override object GetValue()
        {
            return value;
        }
    }

    public class FloatParam : UIParam
    {
        public float value;
        public FloatParam(float v)
        {
            value = v;
        }
        protected override object GetValue()
        {
            return value;
        }
    }

    public class DoubleParam : UIParam
    {
        public double value;
        public DoubleParam(double v)
        {
            value = v;
        }
        protected override object GetValue()
        {
            return value;
        }
    }

    public class BoolParam : UIParam
    {
        public bool value;

        public BoolParam(bool v)
        {
            value = v;
        }
        protected override object GetValue()
        {
            return value;
        }
    }
    public class StringParam : UIParam
    {
        public string value;

        public StringParam(string v)
        {
            value = v;
        }
        protected override object GetValue()
        {
            return value;
        }
    }

    public class Vector2Param : UIParam
    {
        public Vector2 value;

        public Vector2Param(Vector2 v)
        {
            value = v;
        }
        protected override object GetValue()
        {
            return value;
        }
    }

    public class ActionParam : UIParam
    {
        public Action value;

        public ActionParam(Action v)
        {
            value = v;
        }
        protected override object GetValue()
        {
            return value;
        }
    }

    public class ByteArrParam : UIParam
    {
        public byte[] value;

        public ByteArrParam(byte[] v)
        {
            value = v;
        }
        protected override object GetValue()
        {
            return value;
        }
    }

    public class TextureParam : UIParam
    {
        public Texture value;

        public TextureParam(Texture v)
        {
            value = v;
        }
        protected override object GetValue()
        {
            return value;
        }
    }
}
