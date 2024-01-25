﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using XPlan.Interface;

namespace XPlan.UI
{
	public class TableItem : MonoBehaviour
	{
		private TableItemInfo itemInfo;

		public void SetInfo(TableItemInfo info)
		{
			itemInfo = info;

			itemInfo.SetItem(this);
		}

		public string GetID()
		{
			return itemInfo.uniqueID;
		}

		public void Refresh()
		{
			OnRefresh(itemInfo);
		}

		protected void DirectTrigger<T>(string uniqueID, T param, Action<T> onPress = null)
		{
			UIParam p = param.GetUIParam();

			if (p == null)
			{
				Debug.LogError("UISystem not support this type !!");
				return;
			}

			UISystem.TriggerCallback<T>(uniqueID, p, onPress);
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
	}

	public class TableManager
    {
		/**********************************
		 * 注入資料
		 * *******************************/
		private List<TableItemInfo> itemInfoList;

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
		private int totalItemNum;
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
			totalItemNum	= row * col;
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
			gridLayoutGroup				= anchor.AddComponent<GridLayoutGroup>();
			gridLayoutGroup.cellSize	= new Vector2(cellSizeX, cellSizeY);
			gridLayoutGroup.spacing		= new Vector2(10, 10);
			gridLayoutGroup.startAxis	= bHorizontal ? GridLayoutGroup.Axis.Horizontal : GridLayoutGroup.Axis.Vertical;

			/**********************************
			 * 設定itemPrefab
			 * *******************************/
			for (int i = 0; i < totalItemNum; ++i)
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

		public void SetInfoList(List<TableItemInfo> infoList)
		{
			/**********************************
			 * 初始化
			 * *******************************/
			itemInfoList	= infoList;
			currPageIdx		= 0;
			totalPage		= (itemInfoList.Count / totalItemNum) + 1;

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
				pageChange.SetTotalPageNum(totalPage);
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

		public void Refresh(bool bRefreshAnchorSize = false)
		{
			/**********************************
			 * 依照Page來決定設定進Item的資料
			 * *******************************/			
			if (currPageIdx < 0 || currPageIdx >= totalPage)
			{
				Debug.LogError($"{currPageIdx} 當前Page不正確");
				return;
			}

			/**********************************
			 * 將ItemInfo資料放進TableItem裡面
			 * *******************************/
			int startIdx		= totalItemNum * currPageIdx;
			int infoCountInPage	= currPageIdx < (totalPage - 1) ? totalItemNum : (itemInfoList.Count % totalItemNum);

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
				}
				else
				{
					currRow = Mathf.Min(infoCount, row);
					currCol = Mathf.CeilToInt((float)infoCount / (float)row);
				}

				float spaceX = gridLayoutGroup.spacing.x;
				float spaceY = gridLayoutGroup.spacing.y;

				RectTransform rectTF	= (RectTransform)anchor.transform;
				rectTF.sizeDelta		= new Vector2(currCol * gridLayoutGroup.cellSize.x + (currCol - 1) * spaceX,
														currRow * gridLayoutGroup.cellSize.y + (currRow - 1) * spaceY);
			}
		}
	}
}
