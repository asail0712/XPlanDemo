using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using XPlan.Interface;

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
			OnRefresh(itemInfo);
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
			tableItem.SetChoose(b);
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

		public void Refresh(bool bRefreshAnchorSize = true, bool bRefreshAnchorPos = false)
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
			 * 刷新content大小
			 * *******************************/
			if(bRefreshAnchorSize)
			{
				int currCol		= 1;
				int currRow		= 1;
				int infoCount	= itemInfoList.Count;

				if (gridLayoutGroup.startAxis == GridLayoutGroup.Axis.Horizontal)
				{
					currCol = Mathf.Min(infoCount, col);
					currRow = Mathf.CeilToInt((float)infoCount / (float)col);
					currRow = Mathf.Min(infoCount, row);
				}
				else
				{
					currRow = Mathf.Min(infoCount, row);
					currCol = Mathf.CeilToInt((float)infoCount / (float)row);
					currCol = Mathf.Min(infoCount, col);
				}

				float spaceX = gridLayoutGroup.spacing.x;
				float spaceY = gridLayoutGroup.spacing.y;

				RectTransform rectTF	= (RectTransform)anchor.transform;
				rectTF.sizeDelta		= new Vector2(currCol * gridLayoutGroup.cellSize.x + (currCol - 1) * spaceX,
														currRow * gridLayoutGroup.cellSize.y + (currRow - 1) * spaceY);
			}

			if(bRefreshAnchorPos)
			{
				RectTransform rectTF = (RectTransform)anchor.transform;

				if (gridLayoutGroup.startAxis == GridLayoutGroup.Axis.Horizontal)
				{
					rectTF.localPosition = new Vector3(rectTF.localPosition.x, 0f, rectTF.localPosition.z);
				}
				else
				{
					rectTF.localPosition = new Vector3(0f, rectTF.localPosition.y, rectTF.localPosition.z);
				}
			}
		}
	}
}
