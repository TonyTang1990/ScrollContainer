/*
 * Description:             HorizontalScrollContainer.cs
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
    /// 横向滚动容器抽象
    /// </summary>
    public class HorizontalScrollContainer : BaseScrollContainer
    {
        /// <summary>
        /// Container Layout Direction Enum
        /// 排版方向
        /// </summary>
        public enum EHorizontalLayoutDirection
        {
            LeftToRight = 1,              // 从左往右
            RightToLeft,                  // 从右往左
        }

        /// <summary>
        /// Horizontal Container Layout Direction
        /// 排版方向
        /// </summary>
        [Header("排版方向(不允许动态修改)")]
        public EHorizontalLayoutDirection LayoutDirection = EHorizontalLayoutDirection.LeftToRight;

        /// <summary>
        /// 是否是逆向
        /// </summary>
        protected bool mIsReverse;

        public override void Awake()
        {
            base.Awake();
            mAvalibleDragDirection = EDragDirection.Horizontal;

            mScrollAnchorPosition = LayoutDirection == EHorizontalLayoutDirection.LeftToRight ? new Vector2(0.0f, 1.0f) : new Vector2(1.0f, 1.0f);
            // 如果方向是反向的话，当做逆向来处理计算单元格位置和大小
            mIsReverse = LayoutDirection == EHorizontalLayoutDirection.LeftToRight ? false : true;

            RectContentTrasform.pivot = mScrollAnchorPosition;
            RectContentTrasform.anchorMax = mScrollAnchorPosition;
            RectContentTrasform.anchorMin = mScrollAnchorPosition;
            RectContentTrasform.anchoredPosition = Vector2.zero;
        }

        /// <summary>
        /// 改变可滚动状态
        /// </summary>
        /// <param name="isEnable"></param>
        public override void ChangeScrollable(bool isEnable)
        {
            IsAllowedScroll = isEnable;
            ScrollRect.horizontal = IsAllowedScroll ? true : false;
        }

        /// <summary>
        /// 移动到特定Cell单元格位置
        /// </summary>
        /// <param name="index">Cell Index</param>
        /// <param name="moveTime">移动时长</param>
        public override bool MoveToIndex(int index = 0, float moveTime = 1.0f)
        {
            if(base.MoveToIndex(index, moveTime))
            {
                //Debug.Log($"滚动到索引位置:{index}");
                CurrentMoveToCellIndex = CorrectMoveToIndex(index);
                //Debug.Log($"最终滚动到索引位置:{CurrentMoveToCellIndex}");
                Vector2 cellPos = mCellDatas[CurrentMoveToCellIndex].GetPos();
                var maxScrollOffset = RectContentTrasform.rect.width - mRootRectContentTrasform.rect.width;
                var scrollOffset = 0f;
                if (mIsReverse == false)
                {
                    scrollOffset = Mathf.Clamp(-cellPos.x + BeginOffset.x, -maxScrollOffset, 0f);
                }
                else
                {
                    scrollOffset = Mathf.Clamp(-cellPos.x - BeginOffset.x, 0f, maxScrollOffset);
                }
                //Debug.Log($"单元格容器:{gameObject.name}移动到位置索引:{index}目标位置偏移:{scrolloffset.ToString()}");
                mCellMoveToTweener = RectContentTrasform.DOAnchorPosX(scrollOffset, moveTime);
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
            mCenterPositionOffset.x = RectContentTrasform.rect.size.x / 2 - BeginOffset.x;
            mCenterPositionOffset.y = 0;
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
        /// <param name="forceRefreshCellSize">是否强制刷新单元格大小</param>
        /// <param name="forceRefreshShow">是否强制刷新显示</param>
        protected override void Display(bool forceRefreshCellSize = false, bool forceRefreshShow = false)
        {
            if (mCellDatas != null)
            {
                mMaskRect.x = mAvalibleScrollDistance * ScrollRect.horizontalNormalizedPosition;
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
            ScrollRect.vertical = false;
            if (IsAllowedScroll)
            {
                var hasAvalibleScrollDistance = !Mathf.Approximately(mAvalibleScrollDistance, Mathf.Epsilon);
                var newScrollerable = (hasAvalibleScrollDistance || (hasAvalibleScrollDistance == false && IsAllowedInsideScroll)) ? true : false;
                if (ScrollRect.horizontal == true && newScrollerable == false)
                {
                    //从可滚动到不可滚动可能是动态Size导致的，为了确保不可滚动状态下所有单元格可见，必须滚到最顶或最底部
                    ScrollRect.horizontalNormalizedPosition = mIsReverse ? 1 : 0;
                }
                ScrollRect.horizontal = newScrollerable;
            }
            else
            {
                ScrollRect.horizontal = false;
            }
        }

        /// <summary>
        /// 获取指定单元格数据列表的容器大小
        /// </summary>
        /// <param name="cellDatas"></param>
        /// <returns></returns>
        protected override Vector2 GetContentSizeByDatas(List<CellData> cellDatas = null)
        {
            Vector2 contentRectSize = mRootRectContentTrasform.rect.size;
            Vector2 contentNewSize = Vector2.zero;
            contentNewSize.x = BeginOffset.x * 2;
            contentNewSize.y = mRootRectContentTrasform.rect.size.y;

            for (int i = 0, length = cellDatas != null ? cellDatas.Count : 0; i < length; i++)
            {
                if (i != 0)
                {
                    contentNewSize.x += CellSpace;
                }
                contentNewSize.x += Mathf.Abs(cellDatas[i].GetSize().x);
            }
            //根据所有Cell的Size设置RectContent Rect大小
            return contentRectSize.x >= contentNewSize.x ? contentRectSize : contentNewSize;
        }

        /// <summary>
        /// 内容尺寸变化时，为保持绝对滚动位置而计算新的滚动归一化位置
        /// </summary>
        protected override Vector2 CalculateNewNormalizedPos(Vector2 preScrollNormalizedPos, Vector2 preContentSize, Vector2 newContentSize)
        {
            var viewportWidth = mRootRectContentTrasform.rect.width;
            var preScrollableDistance = Mathf.Max(0f, preContentSize.x - viewportWidth);
            var newScrollableDistance = Mathf.Max(0f, newContentSize.x - viewportWidth);
            if (newScrollableDistance <= 0f)
            {
                return preScrollNormalizedPos;
            }

            var scale = Mathf.Clamp01(preScrollableDistance / newScrollableDistance);
            var newX = mIsReverse == false ? preScrollNormalizedPos.x * scale : 1f - (1f - preScrollNormalizedPos.x) * scale;
            return new Vector2(Mathf.Clamp01(newX), preScrollNormalizedPos.y);
        }

        /// <summary>
        /// 更新容器数据
        /// </summary>
        /// <param name="scrollNormalizedPos">单元格初始滚动位置</param>
        /// <param name="keepRectContentPos">是否保持rect content的相对位置(优先于scrollnormalizaedposition)</param>
        protected override void UpdateContainerData(Vector2? scrollNormalizedPos = null, bool keepRectContentPos = false)
        {
            var newContentSize = GetContentSizeByDatas(mCellDatas);
            RectContentTrasform.sizeDelta = newContentSize;

            //Debug.Log($"当前横向单元格滚动Size:{mRectContentTrasform.sizeDelta.ToString()}");
            mAvalibleScrollDistance = RectContentTrasform.rect.width - mRootRectContentTrasform.rect.width;
            //Debug.Log($"当前横向单元格可滚动距离:{mAvalibleScrollDistance}");
            if (keepRectContentPos == false)
            {
                // 考虑到还原单元格滚动位置会传自定义的位置且嵌套单元格会主动调用clearCellDatas()并修正Content位置
                // 所以这里每次都要根据当前最新的滚动位置计算最新位置确保Content位置正确
                var newHorizontalNormalizedPos = scrollNormalizedPos != null ? ((Vector2)scrollNormalizedPos).x : (mIsReverse == false ? 0.0f : 1.0f);
                newHorizontalNormalizedPos = Mathf.Clamp01(newHorizontalNormalizedPos);
                var newAnchorePos = RectContentTrasform.anchoredPosition;
                newAnchorePos.x = mAvalibleScrollDistance * (mIsReverse == false ? -newHorizontalNormalizedPos : (1 - newHorizontalNormalizedPos));
                RectContentTrasform.anchoredPosition = newAnchorePos;
                mMaskRect.x = mAvalibleScrollDistance * newHorizontalNormalizedPos;
            }
            //Debug.Log($"当前横向新滚动位置:{newhorizontalnormalizedposition}");
            //Debug.Log($"当前横向单元格Mask信息:{mMaskRect.ToString()}");
            //Debug.Log($"当前横向滚动位置:{mScrollRect.horizontalNormalizedPosition}");

            // 逆向滚动容器的位置要反向计算
            Vector2 cellRectPos = BeginOffset;
            cellRectPos.y = -cellRectPos.y;
            Vector2 cellMaskBeginOffset = BeginOffset;
            if (mIsReverse == true)
            {
                cellRectPos.x = -cellRectPos.x;
                cellMaskBeginOffset.x = RectContentTrasform.rect.width - BeginOffset.x - (mCellDatas != null ? mCellDatas[mCellDatas.Count - 1].GetSize().x : 0);
            }
            for (int i = 0, length = mCellDatas != null ? mCellDatas.Count : 0; i < length; i++)
            {
                mCellDatas[i].SetAnchor(mScrollAnchorPosition, mScrollAnchorPosition, mScrollAnchorPosition);
                mCellDatas[i].SetRect(cellRectPos, cellMaskBeginOffset);
                mCellDatas[i].CellIndex = i;
                cellRectPos.x += mIsReverse == false ? CellSpace : -CellSpace;
                cellRectPos.x += mIsReverse == false ? mCellDatas[i].GetSize().x : -mCellDatas[i].GetSize().x;
                cellMaskBeginOffset.x += mIsReverse == false ? CellSpace : -CellSpace;
                cellMaskBeginOffset.x += mIsReverse == false ? mCellDatas[i].GetSize().x : -mCellDatas[i].GetSize().x;
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
                var maxScrollOffset = RectContentTrasform.rect.width - mRootRectContentTrasform.rect.width;
                for (int i = 0, length = mCellDatas != null ? mCellDatas.Count : 0; i < length; i++)
                {
                    var cellAbsPos = mCellDatas[i].GetAbsPos();
                    if (cellAbsPos.x - BeginOffset.x >= maxScrollOffset)
                    {
                        mMaxCorrectToCellIndex = i;
                        break;
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
                mMaskRect.x = mAvalibleScrollDistance * ScrollRect.horizontalNormalizedPosition;
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
                //如果evenData.delta.x == 0，那么就沿用前一刻的滚动方向
                if(Mathf.Abs(eventData.delta.x) >= Mathf.Abs(eventData.delta.y))
                {
                    if (eventData.delta.x < 0)
                    {
                        mCurrentScrollDir = EScrollDir.ScrollLeft;
                    }
                    else if (eventData.delta.x > 0)
                    {
                        mCurrentScrollDir = EScrollDir.ScrollRight;
                    }
                    //Debug.Log($"滚动偏移x:{eventData.delta.x}更新单元格容器:{gameObject.name}的滚动方向为:{mCurrentScrollDir.ToString()}");
                }
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
        /// <param name="scrollAbsPos"></param>
        /// <param name="cellIndex"></param>
        /// <returns></returns>
        protected override float GetCellCenterPosOffset(float scrollAbsPos, int cellIndex)
        {
            var cellData = GetCellData(cellIndex);
            // 单元格离中心点的偏移 = 当前滚动到的位置 + 中心点位置偏移 - 单元格位置 - 单元格大小 / 2
            var cellPos = cellData.GetAbsPos();
            var cellSize = cellData.GetSize();
            return scrollAbsPos + mCenterPositionOffset.x - cellPos.x - cellSize.x / 2;
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
                return mAvalibleScrollDistance * ScrollRect.horizontalNormalizedPosition + BeginOffset.x;
            }
            else
            {
                return -mAvalibleScrollDistance * (1 - ScrollRect.horizontalNormalizedPosition) - BeginOffset.x;
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
                for (int i = 0, length = mCellDatas != null ? mCellDatas.Count : 0; i < length; i++)
                {
                    if (i + 1 < length)
                    {
                        if (scrollPos < mCellDatas[0].GetAbsPos().x)
                        {
                            scrollIndex = 0;
                            scrollIndex += ((scrollPos - mCellDatas[0].GetAbsPos().x) / mCellDatas[0].GetSize().x);
                            break;
                        }
                        else if (scrollPos >= mCellDatas[i].GetAbsPos().x && scrollPos <= mCellDatas[i + 1].GetAbsPos().x)
                        {
                            scrollIndex = i;
                            var celloffset = mCellDatas[i + 1].GetAbsPos().x - mCellDatas[i].GetAbsPos().x;
                            scrollIndex += ((scrollPos - mCellDatas[i].GetAbsPos().x) / celloffset);
                            break;
                        }
                    }
                    else
                    {
                        scrollIndex = length - 1;
                        scrollIndex += ((scrollPos - mCellDatas[length - 1].GetAbsPos().x) / mCellDatas[length - 1].GetSize().x);
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
                if (igorePosCheckThredhold == true || (ScrollRect.velocity.x >= -CorrectVelocityThredHold && ScrollRect.velocity.x <= CorrectVelocityThredHold))
                {
                    if (mIsCorrectScrolling == false)
                    {
                        var destinationIndex = -1;
                        if (scrollDir == EScrollDir.ScrollLeft)
                        {
                            var nearestIndex = mIsReverse == false ? Mathf.CeilToInt(CurrentScrollIndexValue) : Mathf.FloorToInt(CurrentScrollIndexValue);
                            destinationIndex = Mathf.Clamp(nearestIndex, 0, mCellDatas.Count - 1);
                        }
                        else if (scrollDir == EScrollDir.ScrollRight)
                        {
                            var nearestIndex = mIsReverse == false ? Mathf.FloorToInt(CurrentScrollIndexValue) : Mathf.CeilToInt(CurrentScrollIndexValue);
                            destinationIndex = Mathf.Clamp(nearestIndex, 0, mCellDatas.Count - 1);
                        }
                        else
                        {
                            if(requireScrollDir == false)
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
                        //Debug.Log("已经在矫正中，不支持连续矫正!");
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
            if (mIsCorrectScrolling)
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
