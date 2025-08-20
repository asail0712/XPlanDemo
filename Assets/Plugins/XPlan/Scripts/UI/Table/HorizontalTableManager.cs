using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using XPlan.Interface;
using XPlan.Utility;

namespace XPlan.UI.Component
{
    public class HorizontalTableManager<T> where T : TableItemInfo
    {
        /**********************************
         * 注入資料
         * *******************************/
        private List<T> itemInfoList;

        /**********************************
         * Unity元件
         * *******************************/
        private HorizontalLayoutGroup hLayoutGroup;
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
            // bHorizontal 參數對 HorizontalLayoutGroup 沒意義，但保留介面相容
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
            itemNumPerPage  = row * col;        // 視覺上不再是網格，但仍用來做分頁數量控制
            itemPrefab      = item;
            this.anchor     = anchor;
            pageChange      = page;
            itemList        = new List<TableItem>();

            /**********************************
             * 取得 prefab 尺寸（非必要，僅保留）
             * *******************************/
            // 注意：在 HorizontalLayoutGroup 下，我們不會強制指定 cellSize

            /**********************************
             * 設定 HorizontalLayoutGroup
             * *******************************/
            hLayoutGroup                = anchor.AddComponent<HorizontalLayoutGroup>();
            hLayoutGroup.spacing        = 10f;
            hLayoutGroup.childAlignment = TextAnchor.UpperLeft;

            // 讓子物件保有自己的尺寸（不被統一）
            hLayoutGroup.childControlWidth      = false;
            hLayoutGroup.childControlHeight     = false;
            hLayoutGroup.childForceExpandWidth  = false;
            hLayoutGroup.childForceExpandHeight = false;

            // 建議搭配 ContentSizeFitter 讓 anchor 依子物件自動撐開（也會在 Refresh 中保險設定一次）
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

                // 子物件若要依內容自適應，請在 prefab 上加 ContentSizeFitter(PreferredSize)
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

        // 兼容舊介面：rowSpace 當作水平 spacing 使用；colSpace 在 HLG 中無效
        public void SetGridSpacing(int rowSpace, int colSpace)
        {
            if (hLayoutGroup != null)
            {
                hLayoutGroup.spacing = rowSpace;
            }
        }

        public void SetChildAlignment(TextAnchor anchor)
        {
            if (hLayoutGroup != null)
            {
                hLayoutGroup.childAlignment = anchor;
            }
        }
        public void SetChildControlOptions(
                                    bool childControlWidth,
                                    bool childControlHeight,
                                    bool childForceExpandWidth,
                                    bool childForceExpandHeight)
        {
            if (hLayoutGroup == null)
            {
                return;
            }

            hLayoutGroup.childControlWidth      = childControlWidth;
            hLayoutGroup.childControlHeight     = childControlHeight;
            hLayoutGroup.childForceExpandWidth  = childForceExpandWidth;
            hLayoutGroup.childForceExpandHeight = childForceExpandHeight;
        }

        public void Refresh(bool bRefreshAnchorSize = true, bool bRefreshScrollPosition = true)
        {
            if (itemInfoList == null)
            {
                Debug.LogWarning("itemInfoList 為 null，請先呼叫 SetInfoList");
                return;
            }

            /**********************************
             * 分頁計算（仍沿用 row*col 的邏輯）
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

            if (!bRefreshScrollPosition)
            {
                return;
            }
                
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

            // HorizontalLayoutGroup → 主要使用水平滾動
            scrollRect.horizontalNormalizedPosition = 0f; // 依需求可改為 1f 移到最右
        }
    }
}
