using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using XPlan.Interface;
using XPlan.Utility;

namespace XPlan.UI.Component
{
    public enum ViewMode
	{
        Horizontal,
        Vertical
    }

    public struct SlotContentParam
	{
        // 初始位置
        public int defaultSlotIdx;
    }

    public struct SlotViewParam
    {
        // 垂直移動還是水平移動
        public ViewMode viewMode;

        // Slot的大小
        public int slotSize;
        // 前方空隙
        public int startGap;
        // 中間空隙
        public int betweenGap;
        // 後方空隙
        public int endGap;

        // 對準的螢幕位置
        public Vector2 screenAnchor;
        // 滑行時間
        public float slidingTime;
        // 觸發滑動的拖曳距離
        public float dragDis;

        public SlotViewParam(GridLayoutGroup layoutGroup, CanvasScaler canvasScaler, ViewMode viewMode, float slidingTime, float dragDis)
		{
            this.viewMode       = viewMode;

            if(viewMode == ViewMode.Horizontal)
			{
                // 前方空隙
                this.startGap   = layoutGroup.padding.left;
                // 中間空隙
                this.betweenGap = (int)layoutGroup.spacing.x;
                // 後方空隙
                this.endGap     = layoutGroup.padding.right;
                // Slot的大小
                this.slotSize   = (int)layoutGroup.cellSize.x;
            }
			else
			{
                // 前方空隙
                this.startGap   = layoutGroup.padding.top;
                // 中間空隙
                this.betweenGap = (int)layoutGroup.spacing.y;
                // 後方空隙
                this.endGap     = layoutGroup.padding.bottom;
                // Slot的大小
                this.slotSize   = (int)layoutGroup.cellSize.y;
            }

            // 對準的螢幕位置
            this.screenAnchor	= canvasScaler.referenceResolution;
            // 滑行時間
            this.slidingTime    = slidingTime;
            // 觸發滑動的拖曳距離
            this.dragDis        = dragDis;
        }
    }

	public class SlotView
    {
        GdScrollView scrollRect;
        SlotViewParam slotViewParam;
        SlotContentParam slotContentParam;

        /***********************************
        * 基本參數
        ***********************************/
        // 每個slot在中間時的content位置
        //List<int> slotCenterList;
        IScrollItem[] itemList;
        // content的RectTransform
        RectTransform contentRect;
        // 當前的slot idx
        int currSlotIdx;
        // 要前往的slot idx
        public int targetSlotIdx;

        /***********************************
        * 滑行參數
        ***********************************/
        // 當前的滑行時間
        float currSlidingTime;
        // 預計無總共滑動時間
        public float totalSlidingTime;
        // 是否開始滑動
        bool bNeedToSliding = false;
        // 滑動開始時的content起始位置
        private int startSlidingPos;

        public int slotIndex { get => currSlotIdx; }
        public int nextIndex { get => targetSlotIdx; set => targetSlotIdx = value; }

        /***********************************
        * 拖曳參數
        ***********************************/
        // 滑動開始時的content起始位置
        private Vector2 startDrapPos;
        // 滑動開始時Content的位置
        private int startContentPos;
        // 拖曳完成時的delegate
        Action<int, int> dropFinishDelegate;
        // 滑動完成時的delegate
        Action<int> sliderFinishDelegate;

        // Start is called before the first frame update
        public SlotView(GdScrollView scroll, SlotViewParam param, Action<int, int> dropFinish = null, Action<int> sliderFinish = null)
        {            
            scrollRect              = scroll;
            slotViewParam           = param;
            dropFinishDelegate      = dropFinish;
            sliderFinishDelegate    = sliderFinish;
        }

        public void RefreshUI(bool bNotify = false)
        {
            InitialDragAndDrop();
            InitialSlider();

            if(bNotify)
			{
                dropFinishDelegate?.Invoke(slotContentParam.defaultSlotIdx, 0);
            }
        }

        public void SetContentParam(SlotContentParam param)
        {
            slotContentParam = param;
        }

        public void SliderSlots(float slidingTime = 0)
        {
            // 設定滑動時間
            if (slidingTime != 0)
            {
                totalSlidingTime = slidingTime;
            }
            else
            {
                totalSlidingTime = slotViewParam.slidingTime;
            }

            startSlidingPos = GetContentPos();
            bNeedToSliding  = true;
            currSlidingTime = 0f;
        }

        public void GotoSlot(int idx, float slidingTime = 0)
		{
            if(slidingTime != 0)
			{
                totalSlidingTime = slidingTime;
            }
            else
			{
                totalSlidingTime = slotViewParam.slidingTime;
            }
            
            targetSlotIdx       = idx;
            startSlidingPos     = GetContentPos();
            bNeedToSliding      = true;
            currSlidingTime     = 0f;
        }

        public void UpdateView(float deltaT)
		{
            if(bNeedToSliding && itemList.Length > targetSlotIdx)
			{
                int targetPos   = itemList[targetSlotIdx].CenterPos;

                if (currSlidingTime >= totalSlidingTime)
				{
                    Debug.Log($"Finish Slider :{currSlotIdx} ");

                    // 到達目標
                    SetContentPos(targetPos);

                    currSlotIdx     = targetSlotIdx;
                    // reset資料
                    currSlidingTime = 0f;
                    bNeedToSliding  = false;

                    sliderFinishDelegate?.Invoke(currSlotIdx);
                    return;
                }

                currSlidingTime += Time.deltaTime;
                float timeRatio = currSlidingTime / totalSlidingTime;
                int newPos      = startSlidingPos + (int)(timeRatio.EaseOutBack() * (targetPos - startSlidingPos));

                //Debug.Log($"Time Ratio : {timeRatio} Pos : {newPos} ");

                SetContentPos(newPos);
            }
		}

        /***********************************
         * 初始化
         ***********************************/

        private void InitialDragAndDrop()
		{
            scrollRect.beginDragDelegate    = StartDrap;
            scrollRect.endDragDelegate      = StopDrap;
            scrollRect.dragingDelgate       = Draging;
        }

        private void InitialSlider()
        {
            itemList        = scrollRect.content.GetComponentsInChildren<IScrollItem>();
            currSlotIdx     = slotContentParam.defaultSlotIdx;
            targetSlotIdx   = currSlotIdx;
            contentRect     = scrollRect.content;

            /**************************************
             * 改變定位點
             **************************************/
            contentRect.pivot = new Vector2(0f, 1f);

            /**************************************
             * 計算每個Slow的中心點 
             **************************************/
            int anchorPos       = 0;
            int screenAnchor    = 0;

            if (slotViewParam.viewMode == ViewMode.Horizontal)
			{
                screenAnchor = (int)slotViewParam.screenAnchor.x / 2;
            }
			else
			{
                screenAnchor = (int)slotViewParam.screenAnchor.y / 2;
            }    

            for (int i = 0; i < itemList.Length; ++i)
            {
                anchorPos               = (slotViewParam.startGap + slotViewParam.slotSize / 2) + i * (slotViewParam.slotSize + slotViewParam.betweenGap);
                itemList[i].CenterPos   = screenAnchor - anchorPos;
            }

            if(itemList.Length <= currSlotIdx)
			{
                Debug.LogError("Content item number is not enough In SlotView");
                return;
			}

            // 設定初始位置
            SetContentPos(itemList[currSlotIdx].CenterPos);
        }

        /***********************************
         * Drag處理
         ***********************************/
        private void StartDrap(PointerEventData val)
		{
            Debug.Log($"Start Drap :{val.pressPosition} ");
            
            bNeedToSliding  = false;
            startDrapPos    = val.pressPosition;
            startContentPos = GetContentPos();
        }

        private void StopDrap(PointerEventData val)
        {
            Debug.Log($"Stop Drap :{val.position} ");

            //int nearestIdx  = -1;
            //int shortestDis = int.MaxValue;
            //int currPos     = GetContentPos();

            //// 找最近的slot滑過去
            //for (int i = 0; i < itemList.Length; ++i)
            //{
            //    int offset = Math.Abs(itemList[i].CenterPos - currPos);
            //    if (shortestDis > offset)
            //    {
            //        shortestDis = offset;
            //        nearestIdx  = i;
            //    }
            //}

            //// 判斷是否要滑動
            //if (shortestDis != 0)
            //{
            //    targetSlotIdx = nearestIdx;

            //    Debug.Log($"Need To Slider :{targetSlotIdx} ");

            //    dropFinishDelegate?.Invoke(targetSlotIdx, shortestDis);
            //}

            Vector2 offset2D    = val.position - startDrapPos;
            float offsetDis     = 0;

            if (slotViewParam.viewMode == ViewMode.Horizontal)
            {
                offsetDis = offset2D.x;
            }
            else
            {
                offsetDis = offset2D.y;
            }

            if(Mathf.Abs(offsetDis) >= slotViewParam.dragDis)
			{
                if (offsetDis > 0)
                {
                    targetSlotIdx = Mathf.Max(0, --targetSlotIdx);
                }
                else
                {
                    targetSlotIdx = Mathf.Min(itemList.Length - 1, ++targetSlotIdx);
                }
            }

            int shortestDis = Mathf.Abs(itemList[targetSlotIdx].CenterPos - GetContentPos());

            dropFinishDelegate?.Invoke(targetSlotIdx, shortestDis);
        }

        private void Draging(PointerEventData val)
		{
            if (Application.isMobilePlatform)
            {
                if (Input.touchCount > 0)
                {
                    DragToMove(Input.GetTouch(0).position);
                }
            }
            else
            {
                if (Input.GetMouseButton(0))
                {
                    DragToMove(Input.mousePosition);
                }
            }
        }

        void DragToMove(Vector2 pos)
		{
            Vector2 offset2D    = pos - startDrapPos;
            float offsetDis     = 0;

            if (slotViewParam.viewMode == ViewMode.Horizontal)
			{
                offsetDis = offset2D.x;
            }
			else
			{
                offsetDis = offset2D.y;
            }

            SetContentPos(startContentPos + (int)offsetDis);
        }

        /***********************************
         * Content位置資訊
         ***********************************/
        private void SetContentPos(int contextPos)
		{
            if (slotViewParam.viewMode == ViewMode.Horizontal)
            {
                contentRect.anchoredPosition = new Vector2(contextPos, contentRect.anchoredPosition.y);
            }
            else
            {
                contentRect.anchoredPosition = new Vector2(contentRect.anchoredPosition.x, contextPos);
            }
        }

        private int GetContentPos()
        {
            if (slotViewParam.viewMode == ViewMode.Horizontal)
            {
                return (int)contentRect.anchoredPosition.x;
            }
            else
            {
                return (int)contentRect.anchoredPosition.y;
            }
        }
    }
}

