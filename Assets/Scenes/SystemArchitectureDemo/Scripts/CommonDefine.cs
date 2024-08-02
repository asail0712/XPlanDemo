using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XPlan.Demo.Architecture
{
	public enum OperatorType
	{
		Addition,
		Subtraction,
		Multiplication,
		Division
	}

	public static class UIRequest
	{
		public const string ToCalcular = "ToCalcular";
	}

	public static class UICommand
	{
		public const string CalcularResult = "CalcularResult";
	}
}