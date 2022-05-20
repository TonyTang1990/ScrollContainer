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
            TopToBottom,              // 从上往下
            BottomToTop,              // 从下往上
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

            // LayoutDirection == EVerticalLayoutDirection.TopToBottom
            // new Vector2(0.0f, 1.0f) : new Vector2(0.0f, 0.0f);
            var axis = (int) LayoutDirection;
            mScrollAnchorPosition = new Vector2(0, axis ^ 1);
            // 如果方向是反向的话，当做逆向来处理计算单元格位置和大小
            mIsReverse = LayoutDirection != EVerticalLayoutDirection.TopToBottom;

            RectContentTrasform.pivot = mScrollAnchorPosition;
            RectContentTrasform.anchorMax = mScrollAnchorPosition;
            RectContentTrasform.anchorMin = mScrollAnchorPosition;
            RectContentTrasform.anchoredPosition = Vector2.zero;
        }

        /// <summary>
        /// 改变可滚动状态
        /// </summary>
        /// <param name="isenable"></param>
        public override void changeScrollable(bool isenable)
        {
            IsAllowedScroll = isenable;
            ScrollRect.vertical = IsAllowedScroll;
        }

        /// <summary>
        /// 移动到特定Cell单元格位置
        /// </summary>
        /// <param name="index">Cell Index</param>
        /// <param name="movetime">移动时长</param>
        public override bool moveToIndex(int index = 0, float movetime = 1.0f)
        {
            if (base.moveToIndex(index, movetime))
            {
                //Debug.Log($"滚动到索引位置:{index}");
                CurrentMoveToCellIndex = correctMoveToIndex(index);
                //Debug.Log($"最终滚动到索引位置:{CurrentMoveToCellIndex}");
                Vector2 cellpos = mCellDatas[CurrentMoveToCellIndex].getPos();
                var maxscrolloffset = RectContentTrasform.rect.height - mRootRectContentTrasform.rect.height;
                var scrolloffset = 0f;
                if(mIsReverse == false)
                {
                    scrolloffset = Mathf.Clamp(-(cellpos.y + BeginOffset.y), 0f, maxscrolloffset);
                }
                else
                {
                    scrolloffset = Mathf.Clamp(-(cellpos.y - BeginOffset.y), -maxscrolloffset, 0f);
                }
                //Debug.Log($"单元格容器:{gameObject.name}移动到位置索引:{index}目标位置偏移:{scrolloffset.ToString()}");
                mCellMoveToTweener = RectContentTrasform.DOAnchorPosY(scrolloffset, movetime);
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
        protected override void initCenterPositionOffset()
        {
            mCenterPositionOffset.x = 0;
            mCenterPositionOffset.y = RectContentTrasform.rect.size.y / 2 - BeginOffset.y;
        }

        /// <summary>
        /// 矫正MoveToIndex
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        protected virtual int correctMoveToIndex(int index)
        {
            var finalmovetoindex = 0;
            if (!CorrectCellPostionSwitch)
            {
                finalmovetoindex = index;
            }
            else
            {
                finalmovetoindex = Mathf.Clamp(index, 0, mMaxCorrectToCellIndex);
            }
            return finalmovetoindex;
        }

        /// <summary>
        /// 显示所有Cell
        /// </summary>
        /// <param name="forcerefreshcellsize">是否强制刷新单元格大小</param>s
        protected override void display(bool forcerefreshcellsize = false)
        {
            if (mCellDatas != null)
            {
                mMaskRect.y = mAvalibleScrollDistance * (1 - ScrollRect.verticalNormalizedPosition);
                for (int i = 0; i < mCellDatas.Count; i++)
                {
                    onCellDisplay(i, forcerefreshcellsize);
                }
            }
        }

        /// <summary>
        /// 更新可滚动状态
        /// </summary>
        public override void updateScrollable()
        {
            ScrollRect.horizontal = false;
            if (IsAllowedScroll)
            {
                var hasavaliblescrolldistance = !Mathf.Approximately(mAvalibleScrollDistance, Mathf.Epsilon);
                var newscrollerable = (hasavaliblescrolldistance || (hasavaliblescrolldistance == false && IsAllowedInsideScroll)) ? true : false;
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
        /// 更新容器数据
        /// </summary>
        /// <param name="scrollnormalizaedposition">单元格初始滚动位置</param>
        /// <param name="keeprectcontentpos">是否保持rect content的相对位置(优先于scrollnormalizaedposition)</param>
        protected override void updateContainerData(Vector2? scrollnormalizaedposition = null, bool keeprectcontentpos = false)
        {
            //调整RectContent Rect大小
            Vector2 contentrectsize = mRootRectContentTrasform.rect.size;
            Vector2 contentnewsize = Vector2.zero;
            contentnewsize.y = BeginOffset.y * 2;
            contentnewsize.x = mRootRectContentTrasform.rect.size.x;

            for (int i = 0, length = mCellDatas != null ? mCellDatas.Count : 0; i < length; i++)
            {
                if (i != 0)
                {
                    contentnewsize.y += CellSpace;
                }
                contentnewsize.y += Mathf.Abs(mCellDatas[i].getSize().y);
            }
            //根据所有Cell数据设置RectContent Rect大小
            RectContentTrasform.sizeDelta = contentrectsize.y >= contentnewsize.y ? contentrectsize : contentnewsize;
            //Debug.Log($"当前纵向单元格滚动Size:{mRectContentTrasform.sizeDelta.ToString()}");
            mAvalibleScrollDistance = RectContentTrasform.rect.height - mRootRectContentTrasform.rect.height;
            //Debug.Log($"当前纵向单元格可滚动距离:{mAvalibleScrollDistance}");
            if (keeprectcontentpos == false)
            {
                // 考虑到还原单元格滚动位置会传自定义的位置且嵌套单元格会主动调用clearCellDatas()并修正Content位置
                // 所以这里每次都要根据当前最新的滚动位置计算最新位置确保Content位置正确
                var newverticalnormalizedposition = scrollnormalizaedposition != null ? ((Vector2)scrollnormalizaedposition).y : (mIsReverse == false ? 1.0f : 0.0f);
                newverticalnormalizedposition = Mathf.Clamp01(newverticalnormalizedposition);
                var newanchoreposition = RectContentTrasform.anchoredPosition;
                newanchoreposition.y = mAvalibleScrollDistance * (mIsReverse == false ? (1 - newverticalnormalizedposition) : -newverticalnormalizedposition);
                RectContentTrasform.anchoredPosition = newanchoreposition;
                mMaskRect.y = mAvalibleScrollDistance * (1 - newverticalnormalizedposition);
            }
            //Debug.Log($"当前纵向新滚动位置:{newverticalnormalizedposition}");
            //Debug.Log($"当前纵向单元格Mask信息:{mMaskRect.ToString()}");
            //Debug.Log($"当前纵向滚动位置:{mScrollRect.verticalNormalizedPosition}");

            // 逆向滚动容器的位置要反向计算
            var cellrectpos = BeginOffset;
            var cellmaskbenginoffset = BeginOffset;
            if (mIsReverse == true)
            {
                cellmaskbenginoffset.y = RectContentTrasform.rect.height - BeginOffset.y - (mCellDatas != null ? mCellDatas[mCellDatas.Count - 1].getSize().y : 0);
            }
            else
            {
                cellrectpos.y = -cellrectpos.y;
            }
            for (int i = 0, length = mCellDatas != null ? mCellDatas.Count : 0; i < length; i++)
            {
                mCellDatas[i].setAnchor(mScrollAnchorPosition, mScrollAnchorPosition, mScrollAnchorPosition);
                mCellDatas[i].setRect(cellrectpos, cellmaskbenginoffset);
                mCellDatas[i].CellIndex = i;
                cellrectpos.y += mIsReverse == false ? -CellSpace : CellSpace;
                cellrectpos.y += mIsReverse == false ? -mCellDatas[i].getSize().y : mCellDatas[i].getSize().y;
                cellmaskbenginoffset.y += mIsReverse == false ? CellSpace : -CellSpace;
                cellmaskbenginoffset.y += mIsReverse == false ? mCellDatas[i].getSize().y : -mCellDatas[i].getSize().y;
            }
            // 强制更新最新的滚动索引位置
            updateScrollValue();
            updateScrollable();
        }

        /// <summary>
        /// 更新最大矫正到单元格索引值
        /// </summary>
        protected override void updateMaxCorrectToCellIndex()
        {
            if (CorrectCellPostionSwitch)
            {
                var maxscrolloffset = RectContentTrasform.rect.height - mRootRectContentTrasform.rect.height;
                for (int i = 0, length = mCellDatas != null ? mCellDatas.Count : 0; i < length; i++)
                {
                    var cellabspos = mCellDatas[i].getAbsPos();
                    if(cellabspos.y - BeginOffset.y <= maxscrolloffset)
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
        /// <param name="scrollpos"></param>
        protected override void onScrollChanged(Vector2 scrollpos)
        {
            if (mCellDatas != null)
            {
                updateScrollValue();
                mMaskRect.y = mAvalibleScrollDistance * (1 - ScrollRect.verticalNormalizedPosition);
                for (int i = 0; i < mCellDatas.Count; i++)
                {
                    onCellDisplay(i);
                }
                checkCellPostionCorrect(mCurrentScrollDir);
            }
        }

        /// <summary>
        /// 更新滚动方向数据
        /// </summary>
        /// <param name="eventData"></param>
        public override void updateScrollDir(PointerEventData eventData)
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
        /// <param name="currentscrollabspos"></param>
        /// <param name="celldata"></param>
        /// <returns></returns>
        protected override float getCellCenterPositionOffset(float currentscrollabspos, int cellindex)
        {
            var celldata = getCellDataWithIndex(cellindex);
            // 单元格离中心点的偏移 = 当前滚动到的位置 + 中心点位置偏移 - 单元格位置 - 单元格大小 / 2
            var cellposition = celldata.getAbsPos();
            var cellsize = celldata.getSize();
            return currentscrollabspos + mCenterPositionOffset.y - cellposition.y - cellsize.y / 2;
        }

        /// <summary>
        /// Get Current Scroll Position
        /// 获取当前滚动到的位置
        /// </summary>
        /// <returns></returns>
        protected override float getCurrentScrollPosition()
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
        protected virtual void updateScrollValue()
        {
            if(mCellDatas != null)
            {
                var currentscrollpos = getCurrentScrollPosition();
                //统一换算成正的偏移位置，方便统一正向和逆向的滚动计算
                currentscrollpos = Mathf.Abs(currentscrollpos);
                CurrentScrollIndexValue = getSpecificScrollPositonScrollIndex(currentscrollpos);
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
        /// <param name="scrollposition"></param>
        /// <returns></returns>
        protected virtual float getSpecificScrollPositonScrollIndex(float scrollposition)
        {
            if (mCellDatas != null)
            {
                var scrollindex = 0f;
                for (int i = 0, length = mCellDatas.Count; i < length; i++)
                {
                    if (i + 1 < length)
                    {
                        if (scrollposition < mCellDatas[0].getAbsPos().y)
                        {
                            scrollindex = 0;
                            scrollindex += ((scrollposition - mCellDatas[0].getAbsPos().y) / mCellDatas[0].getSize().y);
                            break;
                        }
                        else if (scrollposition >= mCellDatas[i].getAbsPos().y && scrollposition <= mCellDatas[i + 1].getAbsPos().y)
                        {
                            scrollindex = i;
                            var celloffset = mCellDatas[i + 1].getAbsPos().y - mCellDatas[i].getAbsPos().y;
                            scrollindex += ((scrollposition - mCellDatas[i].getAbsPos().y) / celloffset);
                            break;
                        }
                    }
                    else
                    {
                        scrollindex = length - 1;
                        scrollindex += ((scrollposition - mCellDatas[length - 1].getAbsPos().y) / mCellDatas[length - 1].getSize().y);
                        break;
                    }
                }
                return scrollindex;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// 检查单元格位置矫正
        /// </summary>
        /// <param name="scrolldir"></param>
        /// <param name="igorepositioncheckthredhold">是否忽略单元格矫正速度判定</param>
        /// <param name="requirescrolldir">是否要求有效滚动方向才矫正</param>
        protected override bool checkCellPostionCorrect(EScrollDir scrolldir, bool igorepositioncheckthredhold = false, bool requirescrolldir = true)
        {
            if (CorrectCellPostionSwitch && mIsEndDrag)
            {
                if (igorepositioncheckthredhold == true || (ScrollRect.velocity.y >= -CorrectVelocityThredHold && ScrollRect.velocity.y <= CorrectVelocityThredHold))
                {
                    if (mIsCorrectScrolling == false)
                    {
                        //Debug.Log($"单元格容器:{gameObject.name}当前滚动索引位置:{mCurrentScrollIndexValue}滚动方向:{scrolldir}");
                        var destinationindex = -1;
                        if (scrolldir == EScrollDir.ScrollUp)
                        {
                            var nearestindex = mIsReverse == false ? Mathf.CeilToInt(CurrentScrollIndexValue) : Mathf.FloorToInt(CurrentScrollIndexValue);
                            destinationindex = Mathf.Clamp(nearestindex, 0, mCellDatas.Count - 1);
                        }
                        else if (scrolldir == EScrollDir.ScrollDown)
                        {
                            var nearestindex = mIsReverse == false ? Mathf.FloorToInt(CurrentScrollIndexValue) : Mathf.CeilToInt(CurrentScrollIndexValue);
                            destinationindex = Mathf.Clamp(nearestindex, 0, mCellDatas.Count - 1);
                        }
                        else
                        {
                            if (requirescrolldir == false)
                            {
                                // 如果移动方向都没有那么应该是初始化创建或者直接指定初始化滚动位置的情况
                                // 这里强制将滚动位置选择到最近的整数单元格索引值来确保目标滚动位置的正确性
                                var nearestindex = Mathf.RoundToInt(CurrentScrollIndexValue);
                                destinationindex = Mathf.Clamp(nearestindex, 0, mCellDatas.Count - 1);
                            }
                        }
                        if (destinationindex != -1)
                        {
                            mIsCorrectScrolling = true;
                            ScrollRect.StopMovement();
                            moveToIndex(destinationindex, CorrectCellTime);
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
