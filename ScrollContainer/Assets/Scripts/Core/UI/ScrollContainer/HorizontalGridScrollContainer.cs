/*
 * Description:             HorizontalGridScrollContainer.cs
 * Author:                  TONYTANG
 * Create Date:             2019/07/15
 */

using UnityEngine;
using System.Collections.Generic;

namespace TH.Modules.UI
{
    /// <summary>
    /// 横向格子容器抽象
    /// </summary>
    public class HorizontalGridScrollContainer : HorizontalScrollContainer
    {
        /// <summary>
        /// 每行多少个Cell
        /// </summary>
        public int mNumCellPerRow = 1;

        /// <summary>
        /// 总共多少行
        /// </summary>
        private int mTotalNumRow;

        /// <summary>
        /// 总共多少列
        /// </summary>
        private int mTotalNumColume;

        /// <summary>
        /// 单元格模板Size
        /// Note:
        /// 网格单元格不支持不同单元格大小，默认以第一个有效单元格设置的Size为基准Size
        /// </summary>
        private Vector2 mTemplateCellSize;

        /// <summary>
        /// 最大矫正到单元格列索引(仅当开启矫正时有用)
        /// </summary>
        protected int mMaxCorrectToCellColumeIndex;

        public override void Awake()
        {
            base.Awake();
            mTotalNumRow = 0;
            mTotalNumColume = 0;
            mTemplateCellSize = Vector2.zero;
            mMaxCorrectToCellColumeIndex = -1;
        }

        /// <summary>
        /// 获取指定单元格数据列表的容器大小
        /// </summary>
        /// <param name="cellDatas"></param>
        /// <returns></returns>
        protected override Vector2 GetContentSizeByDatas(List<CellData> cellDatas = null)
        {
            int totalcellnumber = cellDatas != null ? cellDatas.Count : 0;
            mTemplateCellSize = (totalcellnumber > 0) ? cellDatas[0].GetSize() : Vector2.zero;
            mTotalNumRow = Mathf.CeilToInt((totalcellnumber * 1.0f) / mNumCellPerRow);
            mTotalNumColume = Mathf.Min(totalcellnumber, mNumCellPerRow);
            
            //调整RectContent Rect大小
            Vector2 contentrectsize = mRootRectContentTrasform.rect.size;
            Vector2 contentnewsize = Vector2.zero;
            if (cellDatas != null && cellDatas.Count != 0)
            {
                contentnewsize.x = mTotalNumColume * mTemplateCellSize.x + (mTotalNumColume - 1) * CellSpace + BeginOffset.x * 2;
                contentnewsize.y = mRootRectContentTrasform.rect.size.y;
            }
            //根据所有Cell的Size设置RectContent Rect大小
            return contentrectsize.x >= contentnewsize.x ? contentrectsize : contentnewsize;
        }

        /// <summary>
        /// 更新容器数据
        /// </summary>
        /// <param name="scrollnormalizedPos">单元格初始滚动位置</param>
        /// <param name="keepRectcontentPos">是否保持rect content的相对位置(优先于scrollnormalizedPos)</param>
        protected override void UpdateContainerData(Vector2? scrollnormalizedPos = null, bool keepRectcontentPos = false)
        {
            var newContentSize = GetContentSizeByDatas(mCellDatas);
            RectContentTrasform.sizeDelta = newContentSize;

            //Debug.Log($"当前横向网格单元格滚动Size:{mRectContentTrasform.sizeDelta.ToString()}");
            mAvalibleScrollDistance = RectContentTrasform.rect.width - mRootRectContentTrasform.rect.width;
            //Debug.Log($"当前横向网格单元格可滚动距离:{mAvalibleScrollDistance}");
            //Debug.Log($"当前横向网格滚动位置:{mScrollRect.horizontalNormalizedPosition}");
            if (keepRectcontentPos == false)
            {
                // 考虑到还原单元格滚动位置会传自定义的位置且嵌套单元格会主动调用clearCellDatas()并修正Content位置
                // 所以这里每次都要根据当前最新的滚动位置计算最新位置确保Content位置正确
                var newHorizontalNormalizedPos = scrollnormalizedPos != null ? ((Vector2)scrollnormalizedPos).x : (mIsReverse == false ? 0.0f : 1.0f);
                newHorizontalNormalizedPos = Mathf.Clamp01(newHorizontalNormalizedPos);
                var newAnchorePos = RectContentTrasform.anchoredPosition;
                newAnchorePos.x = mAvalibleScrollDistance * (mIsReverse == false ? -newHorizontalNormalizedPos : (1 - newHorizontalNormalizedPos));
                RectContentTrasform.anchoredPosition = newAnchorePos;
                mMaskRect.x = mAvalibleScrollDistance * newHorizontalNormalizedPos;
            }
            //Debug.Log($"当前横向网格新滚动位置:{newhorizontalnormalizedposition}");
            //Debug.Log($"当前横向网格单元格Mask信息:{mMaskRect.ToString()}");
            //Debug.Log($"当前横向网格滚动位置:{mScrollRect.horizontalNormalizedPosition}");

            // 逆向滚动容器的位置要反向计算
            int rowNum = 0;
            int columnNum = 0;
            var benginOffset = Vector2.zero;
            Vector2 cellRectPos = Vector2.zero;
            Vector2 cellMaskBeginOffset = Vector2.zero;
            for (int i = 0, length = mCellDatas != null ? mCellDatas.Count : 0; i < length; i++)
            {
                rowNum = (i + mNumCellPerRow) / mNumCellPerRow;
                columnNum = Mathf.Min((i % mNumCellPerRow + 1), mNumCellPerRow);
                var positionoffsetx = (columnNum - 1) * (mTemplateCellSize.x + CellSpace) + BeginOffset.x;
                var positionoffsety = (rowNum - 1) * (mTemplateCellSize.y + CellSpace) + BeginOffset.y;
                cellRectPos.x = mIsReverse == false ? positionoffsetx : -positionoffsetx;
                cellRectPos.y = -positionoffsety;
                cellMaskBeginOffset.x = mIsReverse == false ? positionoffsetx : RectContentTrasform.rect.width - positionoffsetx - mTemplateCellSize.x;
                cellMaskBeginOffset.y = positionoffsety;
                mCellDatas[i].SetRect(cellRectPos, cellMaskBeginOffset);
                mCellDatas[i].SetAnchor(mScrollAnchorPosition, mScrollAnchorPosition, mScrollAnchorPosition);
                mCellDatas[i].CellIndex = i;
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
                    if (cellAbsPos.x - BeginOffset.x <= maxScrollOffset)
                    {
                        mMaxCorrectToCellIndex = i;
                        if(i < mTotalNumColume)
                        {
                            mMaxCorrectToCellColumeIndex = i;
                        }
                    }
                }
            }
            else
            {
                mMaxCorrectToCellIndex = -1;
                mMaxCorrectToCellColumeIndex = -1;
            }
            //Debug.Log($"单元格:{gameObject.name}最大可矫正到单元格索引:{mMaxCorrectToCellIndex}最大可矫正到单元格列索引:{mMaxCorrectToCellColumeIndex}");
        }

        /// <summary>
        /// Initialization for Center Position Offset
        /// 初始化中心位置偏移
        /// </summary>
        protected override void InitCenterPositionOffset()
        {
            mCenterPositionOffset = RectContentTrasform.rect.size / 2 - BeginOffset;
        }

        /// <summary>
        /// 矫正MoveToIndex
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        protected override int CorrectMoveToIndex(int index)
        {
            var finalMoveToIndex = 0;
            if (!CorrectCellPostionSwitch)
            {
                finalMoveToIndex = index;
            }
            else
            {
                var indexcolumeindex = index % mTotalNumColume;
                var validecorrectindex = indexcolumeindex <= mMaxCorrectToCellColumeIndex ? indexcolumeindex : mMaxCorrectToCellColumeIndex;
                finalMoveToIndex = index - indexcolumeindex + validecorrectindex;
            }
            return finalMoveToIndex;
        }

        /// <summary>
        /// 获取当前滚动单元格索引值
        /// </summary>
        /// <returns></returns>
        protected override void UpdateScrollValue()
        {
            if (mCellDatas != null)
            {
                var currentScrollPos = 0f;
                if (mIsReverse == false)
                {
                    currentScrollPos = mAvalibleScrollDistance * ScrollRect.horizontalNormalizedPosition + BeginOffset.x;
                }
                else
                {
                    currentScrollPos = -mAvalibleScrollDistance * (1 - ScrollRect.horizontalNormalizedPosition) - BeginOffset.x;
                }
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
        protected override float GetScrollPosIndex(float scrollPos)
        {
            if (mCellDatas != null)
            {
                var scrollIndex = 0f;
                for (int i = 0, length = mTotalNumColume; i < length; i++)
                {
                    if (i + 1 < length)
                    {
                        if (scrollPos < mCellDatas[0].GetAbsPos().x)
                        {
                            scrollIndex = 0;
                            scrollIndex += ((scrollPos - mCellDatas[0].GetAbsPos().x) / mTemplateCellSize.x);
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
                        scrollIndex += ((scrollPos - mCellDatas[length - 1].GetAbsPos().x) / mTemplateCellSize.x);
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
        /// <param name="scrolldir"></param>
        /// <param name="igorepositioncheckthredhold">是否忽略单元格矫正速度判定</param>
        /// <param name="requirescrolldir">是否要求有效滚动方向才矫正</param>
        protected override bool CheckCellPosCorrect(EScrollDir scrolldir, bool igorepositioncheckthredhold = false, bool requirescrolldir = true)
        {
            if (CorrectCellPostionSwitch && mIsEndDrag)
            {
                if (igorepositioncheckthredhold == true || (ScrollRect.velocity.x >= -CorrectVelocityThredHold && ScrollRect.velocity.x <= CorrectVelocityThredHold))
                {
                    if (mIsCorrectScrolling == false)
                    {
                        var destinationIndex = -1;
                        if (scrolldir == EScrollDir.ScrollLeft)
                        {
                            var nearestIndex = mIsReverse == false ? Mathf.CeilToInt(CurrentScrollIndexValue) : Mathf.FloorToInt(CurrentScrollIndexValue);
                            destinationIndex = Mathf.Clamp(nearestIndex, 0, mTotalNumColume - 1);
                        }
                        else if (scrolldir == EScrollDir.ScrollRight)
                        {
                            var nearestIndex = mIsReverse == false ? Mathf.FloorToInt(CurrentScrollIndexValue) : Mathf.CeilToInt(CurrentScrollIndexValue);
                            destinationIndex = Mathf.Clamp(nearestIndex, 0, mTotalNumColume - 1);
                        }
                        else
                        {
                            if (requirescrolldir == false)
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
                            MoveToIndex(destinationIndex, CorrectCellTime);
                            mCurrentScrollDir = EScrollDir.None;
                            ScrollRect.StopMovement();
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
    }
}
