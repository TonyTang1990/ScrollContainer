/*
 * Description:             VerticalScrollContainer.cs
 * Author:                  TONYTANG
 * Create Date:             2019/07/15
 */

using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.EventSystems;

namespace TH.Modules.UI
{
    /// <summary>
    /// 纵向容器滚动抽象
    /// </summary>
    public class VerticalScrollContainer : BaseScrollContainer
    {
        /// <summary>
        /// Vertical Container Layout Direction Enum
        /// 排版方向
        /// </summary>
        public enum EVerticalLayoutDirection
        {
            TopToBottom = 1,              // 从上往下
            BottomToTop,                  // 从下往上
        }

        /// <summary>
        /// Vertical Container Layout Direction
        /// 排版方向
        /// </summary>
        [Header("排版方向(不允许动态修改)")]
        public EVerticalLayoutDirection LayoutDirection = EVerticalLayoutDirection.TopToBottom;

        /// <summary>
        /// 是否是逆向
        /// </summary>
        protected bool mIsReverse;

        public override void Awake()
        {
            base.Awake();
            mAvalibleDragDirection = EDragDirection.Vertical;

            mScrollAnchorPosition = LayoutDirection == EVerticalLayoutDirection.TopToBottom ? new Vector2(0.0f, 1.0f) : new Vector2(0.0f, 0.0f);
            // 如果方向是反向的话，当做逆向来处理计算单元格位置和大小
            mIsReverse = LayoutDirection == EVerticalLayoutDirection.TopToBottom ? false : true;

            RectContentTrasform.pivot = mScrollAnchorPosition;
            RectContentTrasform.anchorMax = mScrollAnchorPosition;
            RectContentTrasform.anchorMin = mScrollAnchorPosition;
            RectContentTrasform.anchoredPosition = Vector2.zero;
        }

        /// <summary>
        /// 改变可滚动状态
        /// </summary>
        /// <param name="isenable"></param>
        public override void ChangeScrollable(bool isenable)
        {
            IsAllowedScroll = isenable;
            ScrollRect.vertical = IsAllowedScroll ? true : false;
        }

        /// <summary>
        /// 移动到特定Cell单元格位置
        /// </summary>
        /// <param name="index">Cell Index</param>
        /// <param name="moveTime">移动时长</param>
        public override bool MoveToIndex(int index = 0, float moveTime = 1.0f)
        {
            if (base.MoveToIndex(index, moveTime))
            {
                //Debug.Log($"滚动到索引位置:{index}");
                CurrentMoveToCellIndex = CorrectMoveToIndex(index);
                //Debug.Log($"最终滚动到索引位置:{CurrentMoveToCellIndex}");
                Vector2 cellpos = mCellDatas[CurrentMoveToCellIndex].GetPos();
                var maxScrollOffset = RectContentTrasform.rect.height - mRootRectContentTrasform.rect.height;
                var scrollOffset = 0f;
                if(mIsReverse == false)
                {
                    scrollOffset = Mathf.Clamp(-(cellpos.y + BeginOffset.y), 0f, maxScrollOffset);
                }
                else
                {
                    scrollOffset = Mathf.Clamp(-(cellpos.y - BeginOffset.y), -maxScrollOffset, 0f);
                }
                //Debug.Log($"单元格容器:{gameObject.name}移动到位置索引:{index}目标位置偏移:{scrollOffset.ToString()}");
                mCellMoveToTweener = RectContentTrasform.DOAnchorPosY(scrollOffset, moveTime);
                mCellMoveToTweener.OnComplete(OnMoveToComlete);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Initialization for Center Position Offset
        /// 初始化中心位置偏移
        /// </summary>
        protected override void InitCenterPositionOffset()
        {
            mCenterPositionOffset.x = 0;
            mCenterPositionOffset.y = RectContentTrasform.rect.size.y / 2 - BeginOffset.y;
        }

        /// <summary>
        /// 矫正MoveToIndex
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        protected virtual int CorrectMoveToIndex(int index)
        {
            var finalMoveToIndex = 0;
            if (!CorrectCellPostionSwitch)
            {
                finalMoveToIndex = index;
            }
            else
            {
                finalMoveToIndex = Mathf.Clamp(index, 0, mMaxCorrectToCellIndex);
            }
            return finalMoveToIndex;
        }

        /// <summary>
        /// 显示所有Cell
        /// </summary>
        /// <param name="forceRefreshCellSize">是否强制刷新单元格大小</param>s
        /// <param name="forceRefreshShow">是否强制刷新显示</param>
        protected override void Display(bool forceRefreshCellSize = false, bool forceRefreshShow = false)
        {
            if (mCellDatas != null)
            {
                mMaskRect.y = mAvalibleScrollDistance * (1 - ScrollRect.verticalNormalizedPosition);
                for (int i = 0; i < mCellDatas.Count; i++)
                {
                    OnCellDisplay(i, forceRefreshCellSize, forceRefreshShow);
                }
            }
        }

        /// <summary>
        /// 更新可滚动状态
        /// </summary>
        public override void UpdateScrollable()
        {
            ScrollRect.horizontal = false;
            if (IsAllowedScroll)
            {
                var hasAvalibleScrollDistance = !Mathf.Approximately(mAvalibleScrollDistance, Mathf.Epsilon);
                var newscrollerable = (hasAvalibleScrollDistance || (hasAvalibleScrollDistance == false && IsAllowedInsideScroll)) ? true : false;
                if (ScrollRect.vertical == true && newscrollerable == false)
                {
                    //从可滚动到不可滚动可能是动态Size导致的，为了确保不可滚动状态下所有单元格可见，必须滚到最顶或最底部
                    ScrollRect.verticalNormalizedPosition = mIsReverse ? 0 : 1;
                }
                ScrollRect.vertical = newscrollerable;
            }
            else
            {
                ScrollRect.vertical = false;
            }
        }
        
        /// <summary>
        /// 获取指定单元格数据列表的容器大小
        /// </summary>
        /// <param name="cellDatas"></param>
        /// <returns></returns>
        protected override Vector2 GetContentSizeByDatas(List<CellData> cellDatas = null)
        {
            Vector2 contentrectsize = mRootRectContentTrasform.rect.size;
            Vector2 contentnewsize = Vector2.zero;
            contentnewsize.y = BeginOffset.y * 2;
            contentnewsize.x = mRootRectContentTrasform.rect.size.x;

            for (int i = 0, length = cellDatas != null ? cellDatas.Count : 0; i < length; i++)
            {
                if (i != 0)
                {
                    contentnewsize.y += CellSpace;
                }
                contentnewsize.y += Mathf.Abs(cellDatas[i].GetSize().y);
            }
            //根据所有Cell数据设置RectContent Rect大小
            return contentrectsize.y >= contentnewsize.y ? contentrectsize : contentnewsize;
        }

        /// <summary>
        /// 内容尺寸变化时，为保持绝对滚动位置而计算新的滚动归一化位置
        /// </summary>
        protected override Vector2 CalculateNewNormalizedPos(Vector2 preScrollNormalizedPos, Vector2 preContentSize, Vector2 newContentSize)
        {
            var viewportHeight = mRootRectContentTrasform.rect.height;
            var preScrollableDistance = Mathf.Max(0f, preContentSize.y - viewportHeight);
            var newScrollableDistance = Mathf.Max(0f, newContentSize.y - viewportHeight);
            if (newScrollableDistance <= 0f)
            {
                return preScrollNormalizedPos;
            }

            var scale = Mathf.Clamp01(preScrollableDistance / newScrollableDistance);
            var newY = mIsReverse == false ? 1f - (1f - preScrollNormalizedPos.y) * scale : preScrollNormalizedPos.y * scale;
            return new Vector2(preScrollNormalizedPos.x, Mathf.Clamp01(newY));
        }

        /// <summary>
        /// 更新容器数据
        /// </summary>
        /// <param name="scrollNormalizedPos">单元格初始滚动位置</param>
        /// <param name="keepRectcontentPos">是否保持rect content的相对位置(优先于scrollnormalizaedposition)</param>
        protected override void UpdateContainerData(Vector2? scrollNormalizedPos = null, bool keepRectcontentPos = false)
        {
            var newContentSize = GetContentSizeByDatas(mCellDatas);
            RectContentTrasform.sizeDelta = newContentSize;

            //Debug.Log($"当前纵向单元格滚动Size:{mRectContentTrasform.sizeDelta.ToString()}");
            mAvalibleScrollDistance = RectContentTrasform.rect.height - mRootRectContentTrasform.rect.height;
            //Debug.Log($"当前纵向单元格可滚动距离:{mAvalibleScrollDistance}");
            if (keepRectcontentPos == false)
            {
                // 考虑到还原单元格滚动位置会传自定义的位置且嵌套单元格会主动调用clearCellDatas()并修正Content位置
                // 所以这里每次都要根据当前最新的滚动位置计算最新位置确保Content位置正确
                var newVerticalNormalizedPos = scrollNormalizedPos != null ? ((Vector2)scrollNormalizedPos).y : (mIsReverse == false ? 1.0f : 0.0f);
                newVerticalNormalizedPos = Mathf.Clamp01(newVerticalNormalizedPos);
                var newAnchorePos = RectContentTrasform.anchoredPosition;
                newAnchorePos.y = mAvalibleScrollDistance * (mIsReverse == false ? (1 - newVerticalNormalizedPos) : -newVerticalNormalizedPos);
                RectContentTrasform.anchoredPosition = newAnchorePos;
                mMaskRect.y = mAvalibleScrollDistance * (1 - newVerticalNormalizedPos);
            }
            //Debug.Log($"当前纵向新滚动位置:{newverticalnormalizedposition}");
            //Debug.Log($"当前纵向单元格Mask信息:{mMaskRect.ToString()}");
            //Debug.Log($"当前纵向滚动位置:{mScrollRect.verticalNormalizedPosition}");

            // 逆向滚动容器的位置要反向计算
            var cellRectPos = BeginOffset;
            var cellMaskBeginOffset = BeginOffset;
            if (mIsReverse == true)
            {
                cellMaskBeginOffset.y = RectContentTrasform.rect.height - BeginOffset.y - (mCellDatas != null ? mCellDatas[mCellDatas.Count - 1].GetSize().y : 0);
            }
            else
            {
                cellRectPos.y = -cellRectPos.y;
            }
            for (int i = 0, length = mCellDatas != null ? mCellDatas.Count : 0; i < length; i++)
            {
                mCellDatas[i].SetAnchor(mScrollAnchorPosition, mScrollAnchorPosition, mScrollAnchorPosition);
                mCellDatas[i].SetRect(cellRectPos, cellMaskBeginOffset);
                mCellDatas[i].CellIndex = i;
                cellRectPos.y += mIsReverse == false ? -CellSpace : CellSpace;
                cellRectPos.y += mIsReverse == false ? -mCellDatas[i].GetSize().y : mCellDatas[i].GetSize().y;
                cellMaskBeginOffset.y += mIsReverse == false ? CellSpace : -CellSpace;
                cellMaskBeginOffset.y += mIsReverse == false ? mCellDatas[i].GetSize().y : -mCellDatas[i].GetSize().y;
            }
            // 强制更新最新的滚动索引位置
            UpdateScrollValue();
            UpdateScrollable();
        }

        /// <summary>
        /// 更新最大矫正到单元格索引值
        /// </summary>
        protected override void UpdateMaxCorrectToIndex()
        {
            if (CorrectCellPostionSwitch)
            {
                var maxScrollOffset = RectContentTrasform.rect.height - mRootRectContentTrasform.rect.height;
                for (int i = 0, length = mCellDatas != null ? mCellDatas.Count : 0; i < length; i++)
                {
                    var cellAbsPos = mCellDatas[i].GetAbsPos();
                    if(cellAbsPos.y - BeginOffset.y <= maxScrollOffset)
                    {
                        mMaxCorrectToCellIndex = i;
                    }
                }
            }
            else
            {
                mMaxCorrectToCellIndex = -1;
            }
            //Debug.Log($"单元格:{gameObject.name}最大可矫正到单元格索引:{mMaxCorrectToCellIndex}");
        }

        /// <summary>
        /// 滚动回调刷新Cell显示
        /// </summary>
        /// <param name="scrollPos"></param>
        protected override void OnScrollChanged(Vector2 scrollPos)
        {
            if (mCellDatas != null)
            {
                UpdateScrollValue();
                mMaskRect.y = mAvalibleScrollDistance * (1 - ScrollRect.verticalNormalizedPosition);
                for (int i = 0; i < mCellDatas.Count; i++)
                {
                    OnCellDisplay(i);
                }
                CheckCellPosCorrect(mCurrentScrollDir);
            }
        }

        /// <summary>
        /// 更新滚动方向数据
        /// </summary>
        /// <param name="eventData"></param>
        public override void UpdateScrollDir(PointerEventData eventData)
        {
            if (eventData != null)
            {
                //如果evenData.delta.y == 0，那么就沿用前一刻的滚动方向
                if (eventData.delta.y > 0)
                {
                    mCurrentScrollDir = EScrollDir.ScrollUp;
                }
                else if (eventData.delta.y < 0)
                {
                    mCurrentScrollDir = EScrollDir.ScrollDown;
                }
                //Debug.Log($"滚动偏移y:{eventData.delta.y}更新单元格容器:{gameObject.name}的滚动方向为:{mCurrentScrollDir.ToString()}");
            }
            //else
            //{
            //    Debug.Log($"单元格容器:{gameObject.name}无有效滚动数据!");
            //}
        }

        /// <summary>
        /// Get specific cell index center position offset
        /// 获取指定单元格离中心点位置的偏移
        /// </summary>
        /// <param name="currentScrollabspos"></param>
        /// <param name="celldata"></param>
        /// <returns></returns>
        protected override float GetCellCenterPosOffset(float currentScrollabspos, int cellIndex)
        {
            var cellData = GetCellData(cellIndex);
            // 单元格离中心点的偏移 = 当前滚动到的位置 + 中心点位置偏移 - 单元格位置 - 单元格大小 / 2
            var cellPos = cellData.GetAbsPos();
            var cellSize = cellData.GetSize();
            return currentScrollabspos + mCenterPositionOffset.y - cellPos.y - cellSize.y / 2;
        }

        /// <summary>
        /// Get Current Scroll Position
        /// 获取当前滚动到的位置
        /// </summary>
        /// <returns></returns>
        protected override float GetCurrentScrollPos()
        {
            if (mIsReverse == false)
            {
                return -mAvalibleScrollDistance * (1 - ScrollRect.verticalNormalizedPosition) - BeginOffset.y;
            }
            else
            {
                return mAvalibleScrollDistance * ScrollRect.verticalNormalizedPosition + BeginOffset.y;
            }
        }

        /// <summary>
        /// 更新滚动相关值
        /// </summary>
        /// <returns></returns>
        protected virtual void UpdateScrollValue()
        {
            if(mCellDatas != null)
            {
                var currentScrollPos = GetCurrentScrollPos();
                //统一换算成正的偏移位置，方便统一正向和逆向的滚动计算
                currentScrollPos = Mathf.Abs(currentScrollPos);
                CurrentScrollIndexValue = GetScrollPosIndex(currentScrollPos);
            }
            else
            {
                CurrentScrollIndexValue = 0f;
            }
        }

        /// <summary>
        /// Get scroll index with specific scroll position
        /// 获取指定滚动位置的单元格滚动索引值
        /// </summary>
        /// <param name="scrollPos"></param>
        /// <returns></returns>
        protected virtual float GetScrollPosIndex(float scrollPos)
        {
            if (mCellDatas != null)
            {
                var scrollIndex = 0f;
                for (int i = 0, length = mCellDatas.Count; i < length; i++)
                {
                    if (i + 1 < length)
                    {
                        if (scrollPos < mCellDatas[0].GetAbsPos().y)
                        {
                            scrollIndex = 0;
                            scrollIndex += ((scrollPos - mCellDatas[0].GetAbsPos().y) / mCellDatas[0].GetSize().y);
                            break;
                        }
                        else if (scrollPos >= mCellDatas[i].GetAbsPos().y && scrollPos <= mCellDatas[i + 1].GetAbsPos().y)
                        {
                            scrollIndex = i;
                            var celloffset = mCellDatas[i + 1].GetAbsPos().y - mCellDatas[i].GetAbsPos().y;
                            scrollIndex += ((scrollPos - mCellDatas[i].GetAbsPos().y) / celloffset);
                            break;
                        }
                    }
                    else
                    {
                        scrollIndex = length - 1;
                        scrollIndex += ((scrollPos - mCellDatas[length - 1].GetAbsPos().y) / mCellDatas[length - 1].GetSize().y);
                        break;
                    }
                }
                return scrollIndex;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// 检查单元格位置矫正
        /// </summary>
        /// <param name="scrollDir"></param>
        /// <param name="igorePosCheckThredhold">是否忽略单元格矫正速度判定</param>
        /// <param name="requireScrollDir">是否要求有效滚动方向才矫正</param>
        protected override bool CheckCellPosCorrect(EScrollDir scrollDir, bool igorePosCheckThredhold = false, bool requireScrollDir = true)
        {
            if (CorrectCellPostionSwitch && mIsEndDrag)
            {
                if (igorePosCheckThredhold == true || (ScrollRect.velocity.y >= -CorrectVelocityThredHold && ScrollRect.velocity.y <= CorrectVelocityThredHold))
                {
                    if (mIsCorrectScrolling == false)
                    {
                        //Debug.Log($"单元格容器:{gameObject.name}当前滚动索引位置:{mCurrentScrollIndexValue}滚动方向:{scrolldir}");
                        var destinationIndex = -1;
                        if (scrollDir == EScrollDir.ScrollUp)
                        {
                            var nearestIndex = mIsReverse == false ? Mathf.CeilToInt(CurrentScrollIndexValue) : Mathf.FloorToInt(CurrentScrollIndexValue);
                            destinationIndex = Mathf.Clamp(nearestIndex, 0, mCellDatas.Count - 1);
                        }
                        else if (scrollDir == EScrollDir.ScrollDown)
                        {
                            var nearestIndex = mIsReverse == false ? Mathf.FloorToInt(CurrentScrollIndexValue) : Mathf.CeilToInt(CurrentScrollIndexValue);
                            destinationIndex = Mathf.Clamp(nearestIndex, 0, mCellDatas.Count - 1);
                        }
                        else
                        {
                            if (requireScrollDir == false)
                            {
                                // 如果移动方向都没有那么应该是初始化创建或者直接指定初始化滚动位置的情况
                                // 这里强制将滚动位置选择到最近的整数单元格索引值来确保目标滚动位置的正确性
                                var nearestIndex = Mathf.RoundToInt(CurrentScrollIndexValue);
                                destinationIndex = Mathf.Clamp(nearestIndex, 0, mCellDatas.Count - 1);
                            }
                        }
                        if (destinationIndex != -1)
                        {
                            mIsCorrectScrolling = true;
                            ScrollRect.StopMovement();
                            MoveToIndex(destinationIndex, CorrectCellTime);
                            mCurrentScrollDir = EScrollDir.None;
                            return true;
                        }
                    }
                    else
                    {
                        //Log.Info("已经在矫正中，不支持连续矫正!");
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 移动到指定索引结束
        /// </summary>
        protected virtual void OnMoveToComlete()
        {
            if(mIsCorrectScrolling)
            {
                mIsCorrectScrolling = false;
            }
            mCurrentScrollDir = EScrollDir.None;
            ScrollRect.StopMovement();
            mMoveToIndexDelegate?.Invoke(CurrentMoveToCellIndex, mCellDatas[CurrentMoveToCellIndex].CellGO);
            mCellMoveToTweener = null;
            CurrentMoveToCellIndex = -1;
            //Debug.Log($"单元格容器:{gameObject.name}矫正完成!");
        }
    }
}
