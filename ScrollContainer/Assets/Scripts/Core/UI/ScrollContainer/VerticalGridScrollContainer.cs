/*
 * Description:             VerticalGridScrollContainer.cs
 * Author:                  TONYTANG
 * Create Date:             2019/07/15
 */

using UnityEngine;
using System.Collections.Generic;
using System;

namespace TH.Modules.UI
{
    /// <summary>
    /// 纵向格子容器抽象
    /// </summary>
    public class VerticalGridScrollContainer : VerticalScrollContainer
    {
        /// <summary>
        /// Cell number per row
        /// 每行多少个Cell
        /// </summary>
        public int mNumCellPerRow = 1;

        /// <summary>
        /// Total row number
        /// 总共多少行
        /// </summary>
        private int mTotalNumRow;
        
        /// <summary>
        /// Total colume number
        /// 总共多少列
        /// </summary>
        private int mTotalNumColume;

        /// <summary>
        /// Prefab Size
        /// 单元格模板Size
        /// Note:
        /// 网格单元格不支持不同单元格大小，默认以第一个有效单元格设置的Size为基准Size
        /// </summary>
        private Vector2 mTemplateCellSize;

        /// <summary>
        /// Max correct cell index(Only working under using correct cell behaviour)
        /// 最大矫正到单元格行索引(仅当开启矫正时有用)
        /// </summary>
        protected int mMaxCorrectToCellRowIndex;

        public override void Awake()
        {
            base.Awake();
            mTotalNumRow = 0;
            mTotalNumColume = 0;
            mTemplateCellSize = Vector2.zero;
            mMaxCorrectToCellRowIndex = -1;
        }

        /// <summary>
        /// 更新容器数据
        /// </summary>
        /// <param name="scrollnormalizaedposition">单元格初始滚动位置</param>
        /// <param name="keeprectcontentpos">是否保持rect content的相对位置(优先于scrollnormalizaedposition)</param>
        protected override void updateContainerData(Vector2? scrollnormalizaedposition = null, bool keeprectcontentpos = false)
        {
            mTemplateCellSize = mCellDatas != null ? mCellDatas[0].getSize() : Vector2.zero;
            int totalcellnumber = mCellDatas != null ? mCellDatas.Count : 0;
            mTotalNumRow = Mathf.CeilToInt(((mCellDatas != null ? mCellDatas.Count : 0) * 1.0f) / mNumCellPerRow);
            mTotalNumColume = Mathf.Min(totalcellnumber, mNumCellPerRow);

            //调整RectContent Rect大小
            Vector2 contentrectsize = mRootRectContentTrasform.rect.size;
            Vector2 contentnewsize = Vector2.zero;
            if (mCellDatas != null && mCellDatas.Count != 0)
            {
                contentnewsize.x = mRootRectContentTrasform.rect.size.x;
                contentnewsize.y = mTotalNumRow * mTemplateCellSize.y + (mTotalNumRow - 1) * CellSpace + BeginOffset.y * 2;
            }
            //根据所有Cell的Size设置RectContent Rect大小
            RectContentTrasform.sizeDelta = contentrectsize.y >= contentnewsize.y ? contentrectsize : contentnewsize;
            //Debug.Log($"当前纵向网格单元格滚动Size:{mRectContentTrasform.sizeDelta.ToString()}");
            mAvalibleScrollDistance = RectContentTrasform.rect.height - mRootRectContentTrasform.rect.height;
            //Debug.Log($"当前纵向网格单元格可滚动距离:{mAvalibleScrollDistance}");
            //Debug.Log($"当前纵向网格滚动位置:{mScrollRect.verticalNormalizedPosition}");

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
            //Debug.Log($"当前纵向网格新滚动位置:{newverticalnormalizedposition}");
            //Debug.Log($"当前纵向网格单元格Mask信息:{mMaskRect.ToString()}");
            //Debug.Log($"当前纵向网格滚动位置:{mScrollRect.verticalNormalizedPosition}");
            
            // 逆向滚动容器的位置要反向计算
            int rownum = 0;
            int columnnum = 0;
            var benginoffset = Vector2.zero;
            Vector2 cellrectpos = Vector2.zero;
            Vector2 cellmaskbenginoffset = Vector2.zero;
            for (int i = 0, length = mCellDatas != null ? mCellDatas.Count : 0; i < length; i++)
            {
                rownum = (i + mNumCellPerRow) / mNumCellPerRow;
                columnnum = Mathf.Min((i % mNumCellPerRow + 1), mNumCellPerRow);
                var positionoffsetx = (columnnum - 1) * (mTemplateCellSize.x + CellSpace) + BeginOffset.x;
                var positionoffsety = (rownum - 1) * (mTemplateCellSize.y + CellSpace) + BeginOffset.y;
                cellrectpos.x = positionoffsetx;
                cellrectpos.y = mIsReverse == false ? -positionoffsety : positionoffsety;
                cellmaskbenginoffset.x = positionoffsetx;
                cellmaskbenginoffset.y = mIsReverse == false ? positionoffsety : RectContentTrasform.rect.height - positionoffsety - mTemplateCellSize.y; 
                mCellDatas[i].setRect(cellrectpos, cellmaskbenginoffset);
                mCellDatas[i].setAnchor(mScrollAnchorPosition, mScrollAnchorPosition, mScrollAnchorPosition);
                mCellDatas[i].CellIndex = i;
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
                    if (cellabspos.y - BeginOffset.y <= maxscrolloffset)
                    {
                        mMaxCorrectToCellIndex = i;
                        var indexrownumber = i / mTotalNumColume;
                        if(indexrownumber < mTotalNumRow)
                        {
                            mMaxCorrectToCellRowIndex = indexrownumber;
                        }
                    }
                }
            }
            else
            {
                mMaxCorrectToCellIndex = -1;
                mMaxCorrectToCellRowIndex = -1;
            }
            //Debug.Log($"单元格:{gameObject.name}最大可矫正到单元格索引:{mMaxCorrectToCellIndex}最大可矫正到单元格行索引:{mMaxCorrectToCellRowIndex}");
        }

        /// <summary>
        /// Initialization for Center Position Offset
        /// 初始化中心位置偏移
        /// </summary>
        protected override void initCenterPositionOffset()
        {
            mCenterPositionOffset = RectContentTrasform.rect.size / 2 - BeginOffset;
        }

        /// <summary>
        /// 矫正MoveToIndex
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        protected override int correctMoveToIndex(int index)
        {
            var finalmovetoindex = 0;
            if (!CorrectCellPostionSwitch)
            {
                finalmovetoindex = index;
            }
            else
            {
                var indexrowindex = index / mTotalNumColume;
                if(indexrowindex <= mMaxCorrectToCellRowIndex)
                {
                    var indexcolumeindex = index % mTotalNumColume;
                    var validerowindex = indexrowindex <= mMaxCorrectToCellRowIndex ? indexrowindex : mMaxCorrectToCellRowIndex;
                    finalmovetoindex = validerowindex * mTotalNumColume + indexcolumeindex;
                }
                else
                {
                    finalmovetoindex = mMaxCorrectToCellIndex;
                }
            }
            return finalmovetoindex;
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
        /// Get scroll index with specific scroll position
        /// 获取指定滚动位置的单元格滚动索引值
        /// </summary>
        /// <param name="scrollposition"></param>
        /// <returns></returns>
        protected override float getSpecificScrollPositonScrollIndex(float scrollposition)
        {
            if (mCellDatas != null)
            {
                var scrollindex = 0f;
                for (int i = 0, length = mTotalNumRow; i < length; i++)
                {
                    if (i + 1 < length)
                    {
                        var preindex = i * mNumCellPerRow;
                        var nextindex = (i + 1) * mNumCellPerRow;
                        if (scrollposition < mCellDatas[0].getAbsPos().y)
                        {
                            scrollindex = 0;
                            scrollindex += ((scrollposition - mCellDatas[0].getAbsPos().y) / mTemplateCellSize.y);
                            break;
                        }
                        else if (scrollposition >= mCellDatas[preindex].getAbsPos().y && scrollposition <= mCellDatas[nextindex].getAbsPos().y)
                        {
                            scrollindex = i;
                            var celloffset = mCellDatas[nextindex].getAbsPos().y - mCellDatas[preindex].getAbsPos().y;
                            scrollindex += ((scrollposition - mCellDatas[preindex].getAbsPos().y) / celloffset);
                            break;
                        }
                    }
                    else
                    {
                        scrollindex = (length - 1);
                        var preindex = (length - 1) * mNumCellPerRow;
                        scrollindex += ((scrollposition + mCellDatas[preindex].getAbsPos().y) / mTemplateCellSize.y);
                        break;
                    }
                }
                return scrollindex * mNumCellPerRow;
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
                        var destinationindex = -1;
                        if (scrolldir == EScrollDir.ScrollUp)
                        {
                            var nearestindex = mIsReverse == false ? Mathf.CeilToInt(CurrentScrollIndexValue) : Mathf.FloorToInt(CurrentScrollIndexValue);
                            destinationindex = Mathf.Clamp(nearestindex, 0, mTotalNumRow - 1) * mNumCellPerRow;
                        }
                        else if (scrolldir == EScrollDir.ScrollDown)
                        {
                            var nearestindex = mIsReverse == false ? Mathf.FloorToInt(CurrentScrollIndexValue) : Mathf.CeilToInt(CurrentScrollIndexValue);
                            destinationindex = Mathf.Clamp(nearestindex, 0, mTotalNumRow - 1) * mNumCellPerRow;
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
                            moveToIndex(destinationindex, CorrectCellTime);
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
