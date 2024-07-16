using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using XPlan;

namespace DemoXplan
{ 
	public class TestObj
	{

	}

    public class DemoInstaller : SystemBase
	{
		protected override void OnPreInitial()
		{
			
		}

		protected override void OnInitialGameObject()
		{
			object[] objList = new object[10];

			objList[0] = 1;
			objList[1] = "2";
			objList[2] = 3f;
			objList[3] = false;
			objList[4] = (Action)(()=> { });
			objList[5] = (Texture)(null);
			objList[6] = new TestObj();

			TestParam(objList);
		}

		protected override void OnInitialHandler()
		{

		}

		protected override void OnPostInitial()
		{

		}

		public void TestParam(params object[] value)
		{
			for(int i = 0; i < value.Length; ++i)
			{
				if(value[i] == null)
				{
					continue;
				}

				Debug.Log(value[i].ToString());
			}
		}
	}
}