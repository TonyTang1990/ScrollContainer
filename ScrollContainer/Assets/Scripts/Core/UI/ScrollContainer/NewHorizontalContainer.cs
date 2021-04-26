/*
 * Description:             NewHorizontalContainer.cs
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
    public class NewHorizontalContainer : NewBaseContainer
    {
        /// <summary>
        /// 排版方向
        /// </summary>
        public enum EHorizontalLayoutDirection
        {
            LeftToRight = 1,              // 从左往右
            RightToLeft,                  // 从右往左
        }

        /// <summary>
        /// 排版方向
        /// </summary>
        [Header("排版方向(不允许动态修改)")]
        public EHorizontalLayoutDirection LayoutDirection = EHorizontalLayoutDirection.LeftToRight;
        
        /// <summary>
        /// 起始位置偏移(含水平和垂直)
        /// </summary>
        public Vector2 BeginOffset = Vector2.one * 10;

        /// <summary>
        /// Cell单元格之间的间距
        /// </summary>
        public float CellSpace = 10.0f;

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
        /// <param name="isenable"></param>
        public override void changeScrollable(bool isenable)
        {
            IsAllowedScroll = isenable;
            ScrollRect.horizontal = IsAllowedScroll ? true : false;
        }

        /// <summary>
        /// 移动到特定Cell单元格位置
        /// </summary>
        /// <param name="index">Cell Index</param>
        /// <param name="movetime">移动时长</param>
        public override bool moveToIndex(int index = 0, float movetime = 1.0f)
        {
            if(base.moveToIndex(index, movetime))
            {
                //Debug.Log($"滚动到索引位置:{index}");
                CurrentMoveToCellIndex = correctMoveToIndex(index);
                //Debug.Log($"最终滚动到索引位置:{CurrentMoveToCellIndex}");
                Vector2 cellpos = mCellDatas[CurrentMoveToCellIndex].getPos();
                var maxscrolloffset = RectContentTrasform.rect.width - mRootRectContentTrasform.rect.width;
                var scrolloffset = 0f;
                if (mIsReverse == false)
                {
                    scrolloffset = Mathf.Clamp(-cellpos.x + BeginOffset.x, -maxscrolloffset, 0f);
                }
                else
                {
                    scrolloffset = Mathf.Clamp(-cellpos.x - BeginOffset.x, 0f, maxscrolloffset);
                }
                //Debug.Log($"单元格容器:{gameObject.name}移动到位置索引:{index}目标位置偏移:{scrolloffset.ToString()}");
                mCellMoveToTweener = RectContentTrasform.DOAnchorPosX(scrolloffset, movetime);
                mCellMoveToTweener.OnComplete(OnMoveToComlete);
                return true;
            }
            else
            {
                return false;
            }
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
        /// <param name="forcerefreshcellsize">是否强制刷新单元格大小</param>
        protected override void display(bool forcerefreshcellsize = false)
        {
            if (mCellDatas != null)
            {
                mMaskRect.x = mAvalibleScrollDistance * ScrollRect.horizontalNormalizedPosition;
                for (int i = 0; i < mCellDatas.Count; i++)
                {
                    onCellDisplay(i, CurrentScrollIndexValue, forcerefreshcellsize);
                }
            }
        }

        /// <summary>
        /// 更新可滚动状态
        /// </summary>
        public override void updateScrollable()
        {
            ScrollRect.vertical = false;
            if (IsAllowedScroll)
            {
                var hasavaliblescrolldistance = !Mathf.Approximately(mAvalibleScrollDistance, Mathf.Epsilon);
                var newscrollerable = (hasavaliblescrolldistance || (hasavaliblescrolldistance == false && IsAllowedInsideScroll)) ? true : false;
                if (ScrollRect.horizontal == true && newscrollerable == false)
                {
                    //从可滚动到不可滚动可能是动态Size导致的，为了确保不可滚动状态下所有单元格可见，必须滚到最顶或最底部
                    ScrollRect.horizontalNormalizedPosition = mIsReverse ? 1 : 0;
                }
                ScrollRect.horizontal = newscrollerable;
            }
            else
            {
                ScrollRect.horizontal = false;
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
            contentnewsize.x = BeginOffset.x * 2;
            contentnewsize.y = mRootRectContentTrasform.rect.size.y;

            for (int i = 0, length = mCellDatas != null ? mCellDatas.Count : 0; i < length; i++)
            {
                if (i != 0)
                {
                    contentnewsize.x += CellSpace;
                }
                contentnewsize.x += Mathf.Abs(mCellDatas[i].getSize().x);
            }
            //根据所有Cell的Size设置RectContent Rect大小
            RectContentTrasform.sizeDelta = contentrectsize.x >= contentnewsize.x ? contentrectsize : contentnewsize;
            //Debug.Log($"当前横向单元格滚动Size:{mRectContentTrasform.sizeDelta.ToString()}");
            mAvalibleScrollDistance = RectContentTrasform.rect.width - mRootRectContentTrasform.rect.width;
            //Debug.Log($"当前横向单元格可滚动距离:{mAvalibleScrollDistance}");
            if (keeprectcontentpos == false)
            {
                // 考虑到还原单元格滚动位置会传自定义的位置且嵌套单元格会主动调用clearCellDatas()并修正Content位置
                // 所以这里每次都要根据当前最新的滚动位置计算最新位置确保Content位置正确
                var newhorizontalnormalizedposition = scrollnormalizaedposition != null ? ((Vector2)scrollnormalizaedposition).x : (mIsReverse == false ? 0.0f : 1.0f);
                newhorizontalnormalizedposition = Mathf.Clamp01(newhorizontalnormalizedposition);
                var newanchoreposition = RectContentTrasform.anchoredPosition;
                newanchoreposition.x = mAvalibleScrollDistance * (mIsReverse == false ? -newhorizontalnormalizedposition : (1 - newhorizontalnormalizedposition));
                RectContentTrasform.anchoredPosition = newanchoreposition;
                mMaskRect.x = mAvalibleScrollDistance * newhorizontalnormalizedposition;
            }
            //Debug.Log($"当前横向新滚动位置:{newhorizontalnormalizedposition}");
            //Debug.Log($"当前横向单元格Mask信息:{mMaskRect.ToString()}");
            //Debug.Log($"当前横向滚动位置:{mScrollRect.horizontalNormalizedPosition}");

            // 逆向滚动容器的位置要反向计算
            Vector2 cellrectpos = BeginOffset;
            cellrectpos.y = -cellrectpos.y;
            Vector2 cellmaskbenginoffset = BeginOffset;
            if (mIsReverse == true)
            {
                cellrectpos.x = -cellrectpos.x;
                cellmaskbenginoffset.x = RectContentTrasform.rect.width - BeginOffset.x - (mCellDatas != null ? mCellDatas[mCellDatas.Count - 1].getSize().x : 0);
            }
            for (int i = 0, length = mCellDatas != null ? mCellDatas.Count : 0; i < length; i++)
            {
                mCellDatas[i].setAnchor(mScrollAnchorPosition, mScrollAnchorPosition, mScrollAnchorPosition);
                mCellDatas[i].setRect(cellrectpos, cellmaskbenginoffset);
                mCellDatas[i].CellIndex = i;
                cellrectpos.x += mIsReverse == false ? CellSpace : -CellSpace;
                cellrectpos.x += mIsReverse == false ? mCellDatas[i].getSize().x : -mCellDatas[i].getSize().x;
                cellmaskbenginoffset.x += mIsReverse == false ? CellSpace : -CellSpace;
                cellmaskbenginoffset.x += mIsReverse == false ? mCellDatas[i].getSize().x : -mCellDatas[i].getSize().x;
            }
            // 强制更新最新的滚动索引位置
            CurrentScrollIndexValue = getCurrentScrollIndexValue();
            updateScrollable();
        }

        /// <summary>
        /// 更新最大矫正到单元格索引值
        /// </summary>
        protected override void updateMaxCorrectToCellIndex()
        {
            if (CorrectCellPostionSwitch)
            {
                var maxscrolloffset = RectContentTrasform.rect.width - mRootRectContentTrasform.rect.width;
                for (int i = 0, length = mCellDatas != null ? mCellDatas.Count : 0; i < length; i++)
                {
                    var cellabspos = mCellDatas[i].getAbsPos();
                    if (cellabspos.x - BeginOffset.x > maxscrolloffset)
                    {
                        mMaxCorrectToCellIndex = i - 1;
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
        /// <param name="scrollpos"></param>
        protected override void onScrollChanged(Vector2 scrollpos)
        {
            if (mCellDatas != null)
            {
                CurrentScrollIndexValue = getCurrentScrollIndexValue();
                mMaskRect.x = mAvalibleScrollDistance * ScrollRect.horizontalNormalizedPosition;
                for (int i = 0; i < mCellDatas.Count; i++)
                {
                    onCellDisplay(i, CurrentScrollIndexValue);
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
        /// 获取当前滚动单元格索引值
        /// </summary>
        /// <returns></returns>
        protected virtual float getCurrentScrollIndexValue()
        {
            var scrollindex = 0f;
            if(mCellDatas != null)
            {
                var currentscrollpos = 0f;
                if (mIsReverse == false)
                {
                    currentscrollpos = mAvalibleScrollDistance * ScrollRect.horizontalNormalizedPosition + BeginOffset.x;
                }
                else
                {
                    currentscrollpos = -mAvalibleScrollDistance * (1 - ScrollRect.horizontalNormalizedPosition) - BeginOffset.x;
                }
                //统一换算成正的偏移位置，方便统一正向和逆向的滚动计算
                currentscrollpos = Mathf.Abs(currentscrollpos);
                for (int i = 0, length = mCellDatas != null ? mCellDatas.Count : 0; i < length; i++)
                {
                    if (i + 1 < length)
                    {
                        if (currentscrollpos < mCellDatas[0].getAbsPos().x)
                        {
                            scrollindex = 0;
                            scrollindex += ((currentscrollpos - mCellDatas[0].getAbsPos().x) / mCellDatas[0].getSize().x);
                            break;
                        }
                        else if (currentscrollpos >= mCellDatas[i].getAbsPos().x && currentscrollpos <= mCellDatas[i + 1].getAbsPos().x)
                        {
                            scrollindex = i;
                            var celloffset = mCellDatas[i + 1].getAbsPos().x - mCellDatas[i].getAbsPos().x;
                            scrollindex += ((currentscrollpos - mCellDatas[i].getAbsPos().x) / celloffset);
                            break;
                        }
                    }
                    else
                    {
                        scrollindex = length - 1;
                        scrollindex += ((currentscrollpos - mCellDatas[length - 1].getAbsPos().x) / mCellDatas[length - 1].getSize().x);
                        break;
                    }
                }
            }
            return scrollindex;
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
                if (igorepositioncheckthredhold == true || (ScrollRect.velocity.x >= -CorrectVelocityThredHold && ScrollRect.velocity.x <= CorrectVelocityThredHold))
                {
                    if (mIsCorrectScrolling == false)
                    {
                        var destinationindex = -1;
                        if (scrolldir == EScrollDir.ScrollLeft)
                        {
                            var nearestindex = mIsReverse == false ? Mathf.CeilToInt(CurrentScrollIndexValue) : Mathf.FloorToInt(CurrentScrollIndexValue);
                            destinationindex = Mathf.Clamp(nearestindex, 0, mCellDatas.Count - 1);
                        }
                        else if (scrolldir == EScrollDir.ScrollRight)
                        {
                            var nearestindex = mIsReverse == false ? Mathf.FloorToInt(CurrentScrollIndexValue) : Mathf.CeilToInt(CurrentScrollIndexValue);
                            destinationindex = Mathf.Clamp(nearestindex, 0, mCellDatas.Count - 1);
                        }
                        else
                        {
                            if(requirescrolldir == false)
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
