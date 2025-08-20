using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using XPlan.Interface;
using XPlan.Utility;

namespace XPlan.UI.Component
{
	public class TableItem : MonoBehaviour
	{
		private TableItemInfo itemInfo;
		private bool bBeChoosed = false;

		/*****************************************
		 * 設定Info
		 * **************************************/
		public void SetInfo(TableItemInfo info)
		{
			itemInfo = info;

			itemInfo.SetItem(this);
		}

		/*****************************************
		 * 取得唯一ID
		 * **************************************/
		public string GetID()
		{
			return itemInfo.uniqueID;
		}

		/*****************************************
		 * 選中相關資訊
		 * **************************************/
		public void SetChoose(bool b)
		{
			bBeChoosed = b;
		}

		public bool IsChoosed()
		{
			return bBeChoosed;
		}

		/*****************************************
		 * 資訊刷新
		 * **************************************/
		public void Refresh()
		{
			bBeChoosed = itemInfo.IsChoose();

			OnRefresh(itemInfo);
		}

		/********************************
		 * 其他
		 * *****************************/
		protected string GetStr(string keyStr)
		{
			return UIController.Instance.GetStr(keyStr);
		}

		/*****************************************
		 * 功能複寫
		 * **************************************/
		protected void DirectTrigger<T>(string uniqueID, T param, Action<T> onPress = null)
		{
			UISystem.TriggerCallback<T>(uniqueID, param, onPress);
		}

		protected void DirectTrigger(string uniqueID, Action onPress = null)
		{
			UISystem.TriggerCallback(uniqueID, onPress);
		}

		protected virtual void OnRefresh(TableItemInfo info)
		{
			// nothing to do here
		}
	}

	public class TableItemInfo
	{
		public string uniqueID;

		private TableItem tableItem;

		private bool bBeChoose = false;

		public TableItemInfo()
        {

        }

		public TableItemInfo(string uniqueID)
        {
			this.uniqueID = uniqueID;
        }

		public void SetItem(TableItem item)
		{
			tableItem = item;
		}

		public void FlushInfo()
		{
			tableItem.Refresh();
		}

		public void SetChoose(bool b)
		{
			bBeChoose = b;
		}

		public bool IsChoose()
        {
			return bBeChoose;

		}
	}

	public class TableManager<T> where T : TableItemInfo
    {
		/**********************************
		 * 注入資料
		 * *******************************/
		private List<T> itemInfoList;

		/**********************************
		 * Unity元件
		 * *******************************/
		private GridLayoutGroup gridLayoutGroup;
		private List<TableItem> itemList;

		/**********************************
		 * 內部參數
		 * *******************************/
		private int currPageIdx;
		private int totalPage;
		private int itemNumPerPage;
		private int row;
		private int col;
		private GameObject itemPrefab;
		private GameObject anchor;
		private IPageChange pageChange;

		public bool InitTable(GameObject anchor, int row, int col, GameObject item, IPageChange page = null, bool bHorizontal = true)
		{
			if (null == anchor || null == item)
			{
				Debug.LogError($" {anchor} 或是 {item} 為null");
				return false;
			}

			TableItem dummyItem;

			if (!item.TryGetComponent<TableItem>(out dummyItem))
			{
				Debug.LogError($"{item} 沒有包含 TableItem");
				return false;
			}

			/**********************************
			 * 初始化
			 * *******************************/
			this.row		= row;
			this.col		= col;
			itemNumPerPage	= row * col;
			itemPrefab		= item;
			this.anchor		= anchor;
			pageChange		= page;
			itemList		= new List<TableItem>();

			/**********************************
			 * 計算cell 大小
			 * *******************************/
			RectTransform rectTF	= (RectTransform)item.transform;
			float cellSizeX			= rectTF.rect.width;
			float cellSizeY			= rectTF.rect.height;

			/**********************************
			 * grid設定
			 * *******************************/
			gridLayoutGroup					= anchor.AddComponent<GridLayoutGroup>();
			gridLayoutGroup.cellSize		= new Vector2(cellSizeX, cellSizeY);
			gridLayoutGroup.spacing			= new Vector2(10, 10);
			gridLayoutGroup.startAxis		= bHorizontal ? GridLayoutGroup.Axis.Horizontal : GridLayoutGroup.Axis.Vertical;
			gridLayoutGroup.constraint		= bHorizontal ? GridLayoutGroup.Constraint.FixedColumnCount : GridLayoutGroup.Constraint.FixedColumnCount;
			gridLayoutGroup.constraintCount = bHorizontal ? col : row;

			/**********************************
			 * 設定itemPrefab
			 * *******************************/
			for (int i = 0; i < itemNumPerPage; ++i)
			{
				GameObject itemGO = GameObject.Instantiate(itemPrefab);

				// 設定位置
				itemGO.transform.SetParent(anchor.transform);
				itemGO.transform.localPosition		= Vector3.zero;
				itemGO.transform.localEulerAngles	= Vector3.zero;
				itemGO.transform.localScale			= Vector3.one;

				itemGO.SetActive(true);

				// 取出component
				TableItem tableItem = itemGO.GetComponent<TableItem>();
				itemList.Add(tableItem);
			}

			return true;
		}

		public void SetInfoList(List<T> infoList)// where T : TableItemInfo
		{
			/**********************************
			 * 初始化
			 * *******************************/
			itemInfoList	= infoList;
			currPageIdx		= 0;
			
			/**********************************
			 * 設定pageChange
			 * *******************************/
			if (pageChange != null)
			{
				pageChange.SetPageCallback((currIdx)=> 
				{
					currPageIdx = currIdx;

					Refresh();
				});
			}
		}

		public void SetGridSpacing(int rowSpace, int colSpace)
		{
			gridLayoutGroup.spacing = new Vector2(rowSpace, colSpace);
		}

		public void SetChildAlignment(TextAnchor anchor)
		{
			gridLayoutGroup.childAlignment = anchor;
		}

		public void Refresh(bool bRefreshAnchorSize = true, bool bRefreshScrollPosition = true)//, bool bRefreshAnchorPos = true)
		{
			/**********************************
			 * 依照Page來決定設定進Item的資料
			 * *******************************/
			totalPage = (itemInfoList.Count / itemNumPerPage) + 1;

			if (currPageIdx < 0 || currPageIdx >= totalPage)
			{
				Debug.LogError($"{currPageIdx} 當前Page不正確");
				return;
			}

			/**********************************
			 * 將ItemInfo資料放進TableItem裡面
			 * *******************************/
			int startIdx		= itemNumPerPage * currPageIdx;
			int infoCountInPage = 0;

			if(totalPage == 1)
			{
				infoCountInPage = itemInfoList.Count;
			}
			else
			{
				if(currPageIdx < (totalPage - 1))
				{
					infoCountInPage = itemNumPerPage;
				}
				else
				{
					infoCountInPage = itemInfoList.Count % itemNumPerPage;
				}
			}
			
			for(int i = 0; i < itemList.Count; ++i)
			{
				bool bEnabled	= i < infoCountInPage;
				TableItem item	= itemList[i];
				item.gameObject.SetActive(bEnabled);

				if (bEnabled)
				{
					item.SetInfo(itemInfoList[startIdx + i]);
					item.Refresh();
				}				
			}

			/**********************************
			 * 刷新page change
			 * *******************************/
			if (pageChange != null)
			{
				pageChange.SetTotalPageNum(totalPage);
				pageChange.RefershPageInfo();
			}

			/**********************************
			 * 刷新content
			 * *******************************/
			if (bRefreshAnchorSize)
            {
                ContentSizeFitter fitter	= anchor.AddOrFindComponent<ContentSizeFitter>();
                fitter.verticalFit			= ContentSizeFitter.FitMode.PreferredSize;
				fitter.horizontalFit		= ContentSizeFitter.FitMode.PreferredSize;
            }


			/**********************************
			 * 刷新Scroll
			 * *******************************/
			if(!bRefreshScrollPosition)
            {
				return;
            }

			RefreshScroll();

			//         if (bRefreshAnchorPos)
			//{
			//	RectTransform rectTF = (RectTransform)anchor.transform;

			//	if (gridLayoutGroup.startAxis == GridLayoutGroup.Axis.Horizontal)
			//	{
			//		rectTF.localPosition = new Vector3(rectTF.localPosition.x, 0f, rectTF.localPosition.z);
			//	}
			//	else
			//	{
			//		rectTF.localPosition = new Vector3(0f, rectTF.localPosition.y, rectTF.localPosition.z);
			//	}
			//}
		}

		private void RefreshScroll()
        {
			if (anchor != null)
			{
				Transform parent = anchor.transform.parent;

				if (parent == null)
				{
					return;
				}

				Transform grandParent = parent.parent;

				if (grandParent == null)
				{
					return;
				}

				ScrollRect scrollRect = grandParent.gameObject.GetComponent<ScrollRect>();

				if (scrollRect == null)
				{
					return;
				}

				if (gridLayoutGroup.startAxis == GridLayoutGroup.Axis.Horizontal)
				{
					// scroll移到最上面
					scrollRect.verticalNormalizedPosition = 1f;
				}
				else
                {
					// scroll移到最左邊
					scrollRect.horizontalNormalizedPosition = 1f;
				}
			}
		}
	}
}
