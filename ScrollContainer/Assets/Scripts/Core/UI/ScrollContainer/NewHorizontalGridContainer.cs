/*
 * Description:             NewHorizontalGridContainer.cs
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
    public class NewHorizontalGridContainer : NewHorizontalContainer
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
                contentnewsize.x = mTotalNumColume * mTemplateCellSize.x + (mTotalNumColume - 1) * CellSpace + BeginOffset.x * 2;
                contentnewsize.y = mRootRectContentTrasform.rect.size.y;
            }
            //根据所有Cell的Size设置RectContent Rect大小
            RectContentTrasform.sizeDelta = contentrectsize.x >= contentnewsize.x ? contentrectsize : contentnewsize;
            //Debug.Log($"当前横向网格单元格滚动Size:{mRectContentTrasform.sizeDelta.ToString()}");
            mAvalibleScrollDistance = RectContentTrasform.rect.width - mRootRectContentTrasform.rect.width;
            //Debug.Log($"当前横向网格单元格可滚动距离:{mAvalibleScrollDistance}");
            //Debug.Log($"当前横向网格滚动位置:{mScrollRect.horizontalNormalizedPosition}");
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
            //Debug.Log($"当前横向网格新滚动位置:{newhorizontalnormalizedposition}");
            //Debug.Log($"当前横向网格单元格Mask信息:{mMaskRect.ToString()}");
            //Debug.Log($"当前横向网格滚动位置:{mScrollRect.horizontalNormalizedPosition}");

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
                cellrectpos.x = mIsReverse == false ? positionoffsetx : -positionoffsetx;
                cellrectpos.y = -positionoffsety;
                cellmaskbenginoffset.x = mIsReverse == false ? positionoffsetx : RectContentTrasform.rect.width - positionoffsetx - mTemplateCellSize.x;
                cellmaskbenginoffset.y = positionoffsety;
                mCellDatas[i].setRect(cellrectpos, cellmaskbenginoffset);
                mCellDatas[i].setAnchor(mScrollAnchorPosition, mScrollAnchorPosition, mScrollAnchorPosition);
                mCellDatas[i].CellIndex = i;
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
                    if (cellabspos.x - BeginOffset.x <= maxscrolloffset)
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
                var indexcolumeindex = index % mTotalNumColume;
                var validecorrectindex = indexcolumeindex <= mMaxCorrectToCellColumeIndex ? indexcolumeindex : mMaxCorrectToCellColumeIndex;
                finalmovetoindex = index - indexcolumeindex + validecorrectindex;
            }
            return finalmovetoindex;
        }

        /// <summary>
        /// 获取当前滚动单元格索引值
        /// </summary>
        /// <returns></returns>
        protected override float getCurrentScrollIndexValue()
        {
            var scrollindex = 0f;
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
            for (int i = 0, length = mTotalNumColume; i < length; i++)
            {
                if (i + 1 < length)
                {
                    if (currentscrollpos < mCellDatas[0].getAbsPos().x)
                    {
                        scrollindex = 0;
                        scrollindex += ((currentscrollpos - mCellDatas[0].getAbsPos().x) / mTemplateCellSize.x);
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
                    scrollindex += ((currentscrollpos - mCellDatas[length - 1].getAbsPos().x) / mTemplateCellSize.x);
                    break;
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
                            destinationindex = Mathf.Clamp(nearestindex, 0, mTotalNumColume - 1);
                        }
                        else if (scrolldir == EScrollDir.ScrollRight)
                        {
                            var nearestindex = mIsReverse == false ? Mathf.FloorToInt(CurrentScrollIndexValue) : Mathf.CeilToInt(CurrentScrollIndexValue);
                            destinationindex = Mathf.Clamp(nearestindex, 0, mTotalNumColume - 1);
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
