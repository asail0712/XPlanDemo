using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using XPlan.Interface;
using XPlan.Utility;

namespace XPlan.UI.Component
{
    public class VerticalTableManager<T> where T : TableItemInfo
    {
        /**********************************
         * 注入資料
         * *******************************/
        private List<T> itemInfoList;

        /**********************************
         * Unity元件
         * *******************************/
        private VerticalLayoutGroup vLayoutGroup;
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

        public bool InitTable(GameObject anchor, int row, int col, GameObject item, IPageChange page = null)
        {
            if (null == anchor || null == item)
            {
                Debug.LogError($"{anchor} 或是 {item} 為 null");
                return false;
            }

            if (!item.TryGetComponent<TableItem>(out var dummyItem))
            {
                Debug.LogError($"{item} 沒有包含 TableItem");
                return false;
            }

            /**********************************
             * 初始化
             * *******************************/
            this.row        = row;
            this.col        = col;
            itemNumPerPage  = row * col; // 分頁數量仍沿用
            itemPrefab      = item;
            this.anchor     = anchor;
            pageChange      = page;
            itemList        = new List<TableItem>();

            /**********************************
             * 設定 VerticalLayoutGroup
             * *******************************/
            vLayoutGroup                = anchor.AddComponent<VerticalLayoutGroup>();
            vLayoutGroup.spacing        = 10f;
            vLayoutGroup.childAlignment = TextAnchor.UpperLeft;

            // 預設值（可由外部覆蓋）
            vLayoutGroup.childControlWidth      = false;
            vLayoutGroup.childControlHeight     = false;
            vLayoutGroup.childForceExpandWidth  = false;
            vLayoutGroup.childForceExpandHeight = false;

            // 建議搭配 ContentSizeFitter
            var fitter              = anchor.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit    = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit      = ContentSizeFitter.FitMode.PreferredSize;

            /**********************************
             * 生成 item
             * *******************************/
            for (int i = 0; i < itemNumPerPage; ++i)
            {
                GameObject itemGO = GameObject.Instantiate(itemPrefab);

                itemGO.transform.SetParent(anchor.transform);
                itemGO.transform.localPosition      = Vector3.zero;
                itemGO.transform.localEulerAngles   = Vector3.zero;
                itemGO.transform.localScale         = Vector3.one;
                itemGO.SetActive(true);

                var tableItem = itemGO.GetComponent<TableItem>();
                itemList.Add(tableItem);
            }

            return true;
        }

        public void SetInfoList(List<T> infoList)
        {
            itemInfoList    = infoList;
            currPageIdx     = 0;

            if (pageChange != null)
            {
                pageChange.SetPageCallback((currIdx) =>
                {
                    currPageIdx = currIdx;
                    Refresh();
                });
            }
        }

        // 兼容舊介面：colSpace 當作垂直 spacing；rowSpace 在 VLG 中無效
        public void SetGridSpacing(int rowSpace, int colSpace)
        {
            if (vLayoutGroup != null)
            {
                vLayoutGroup.spacing = colSpace;
            }
        }

        public void SetChildAlignment(TextAnchor anchor)
        {
            if (vLayoutGroup != null)
            {
                vLayoutGroup.childAlignment = anchor;
            }
        }

        public void SetChildControlOptions(
                                    bool childControlWidth,
                                    bool childControlHeight,
                                    bool childForceExpandWidth,
                                    bool childForceExpandHeight)
        {
            if (vLayoutGroup == null) return;

            vLayoutGroup.childControlWidth      = childControlWidth;
            vLayoutGroup.childControlHeight     = childControlHeight;
            vLayoutGroup.childForceExpandWidth  = childForceExpandWidth;
            vLayoutGroup.childForceExpandHeight = childForceExpandHeight;
        }

        public void Refresh(bool bRefreshAnchorSize = true, bool bRefreshScrollPosition = true)
        {
            if (itemInfoList == null)
            {
                Debug.LogWarning("itemInfoList 為 null，請先呼叫 SetInfoList");
                return;
            }

            /**********************************
             * 分頁計算
             * *******************************/
            totalPage = (itemInfoList.Count / itemNumPerPage) + 1;

            if (currPageIdx < 0 || currPageIdx >= totalPage)
            {
                Debug.LogError($"{currPageIdx} 當前 Page 不正確");
                return;
            }

            int startIdx = itemNumPerPage * currPageIdx;
            int infoCountInPage = 0;

            if (totalPage == 1)
            {
                infoCountInPage = itemInfoList.Count;
            }
            else
            {
                if (currPageIdx < (totalPage - 1))
                {
                    infoCountInPage = itemNumPerPage;
                }
                else
                {
                    infoCountInPage = itemInfoList.Count % itemNumPerPage;
                }
            }

            for (int i = 0; i < itemList.Count; ++i)
            {
                bool bEnabled   = i < infoCountInPage;
                TableItem item  = itemList[i];
                item.gameObject.SetActive(bEnabled);

                if (bEnabled)
                {
                    item.SetInfo(itemInfoList[startIdx + i]);
                    item.Refresh();
                }
            }

            if (pageChange != null)
            {
                pageChange.SetTotalPageNum(totalPage);
                pageChange.RefershPageInfo();
            }

            if (bRefreshAnchorSize)
            {
                var fitter              = anchor.AddOrFindComponent<ContentSizeFitter>();
                fitter.verticalFit      = ContentSizeFitter.FitMode.PreferredSize;
                fitter.horizontalFit    = ContentSizeFitter.FitMode.PreferredSize;
            }

            if (!bRefreshScrollPosition) return;

            RefreshScroll();
        }

        private void RefreshScroll()
        {
            if (anchor == null) return;

            Transform parent = anchor.transform.parent;
            if (parent == null) return;

            Transform grandParent = parent.parent;
            if (grandParent == null) return;

            ScrollRect scrollRect = grandParent.gameObject.GetComponent<ScrollRect>();
            if (scrollRect == null) return;

            // VerticalLayoutGroup → 使用垂直滾動
            scrollRect.verticalNormalizedPosition = 1f; // 移到最上方（0f 則是最底部）
        }
    }
}
