/*
 * Description:             CellData.cs
 * Author:                  TONYTANG
 * Create Date:             2021/04/19
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 单元格介绍:
// 支持功能:
// 1. 多预制件设置和选择支持
// 2. 多种回调绑定支持(OnShow, OnVisibleScroll, MoveToIndex, OnHide)
// 3. 动态单元格大小支持(仅横向竖向单元格支持，横向竖向网格单元格不支持)
// 4. 动态增删移动到指定单元格支持
// 5. 单元格矫正(最后准确滚到某个单元格上)设置支持
// 6. 横向单元格容器从左到右，从右到左，竖向单元格容器从上到下，从下到上设置支持
// 7. 支持初始化到指定百分比位置(0-1)
// 8. 支持嵌套单元格滚动
// 9. 支持编辑器本地模拟创建单元格看效果
// 10. 单元格初始偏移以及单元格间隔设置支持
// ......

// 多种回调支持介绍:
// 1. OnShow -- 单元格显示时触发(常规显示回调)
// 2. OnVisibleScroll -- 单元格初始化和滚动时触发(做特殊表现时使用e.g. 根据滚动距离动态改变Alpha,Scale等)
// 3. MoveToIndex -- 单元格滚动到指定单元格时触发(含手动调MoveToIndex和矫正完成时)(特殊需求时使用e.g. 开启矫正后回调单元格选择索引)
// 4. OnHide -- 单元格隐藏和销毁时触发(希望每个单元格清理自身相关逻辑时使用)

// 缺点:
// 1. 单元格容器必须一开始知道所有单元格大小(如果是动态单元格大小初始化计算开销会比较大)(其他滚动容器更多的是需要显示时才会去获取指定单元格大小信息)
// 2. 缺点1决定了动态单元格大小变化一定会影响总的单元格容器总大小(动态大小改变会触发位置变化)

/*
使用示例介绍:
示例1(使用默认第一个预制件且默认预制件大小):
    Container.bindContainerCallBack(onContainerOnShow);
    Container.setCellDatasByCellCount(mContainerSourceList.Count);

示例2(手动传单元格模板选择和大小信息) :
    Container.bindContainerCallBack(onContainerOnShow);
    Container.createNormalCellDataList(prefabindexlist, cellsizelist);

示例3(动态修改指定单元格大小) :
    Container.bindContainerCallBack(onContainerOnShow);
    Container.createNormalCellDataList(prefabindexlist, cellsizelist);
    // 动态改变单元格索引5的大小到(50, 50)
    var celldata = Container.getCellDataWithIndex(5);
    celldata.changeSize(new Vector2(50, 50);

示例4(动态增删指定索引) :
    Container.bindContainerCallBack(onContainerOnShow);
    Container.createNormalCellDataList(prefabindexlist, cellsizelist);

    // 增加单元格到索引位置5
    var celldata = Container.createNormalCellData(prefabindex, size);
    celldata.addCellDataWithIndex(celldata, 5);
        
    // 移除索引位置5的单元格
    celldata.removeCellWithIndex(5);
*/

namespace TH.Modules.UI
{
    /// <summary>
    /// CellData.cs
    /// 单元格数据抽象
    /// </summary>
    public class CellData : IRecycle
    {
        /// <summary>
        /// 当前Cell索引
        /// </summary>
        public int CellIndex
        {
            get;
            set;
        }

        /// <summary>
        /// 拥有者单元格
        /// </summary>
        protected BaseScrollContainer mOwnerContainer;

        /// <summary>
        /// Cell实例对象
        /// </summary>
        public GameObject CellGO
        {
            get;
            protected set;
        }

        /// <summary>
        /// Cell实例对象的RectTransform
        /// </summary>
        public RectTransform CellGORectTransform
        {
            get;
            protected set;
        }

        /// <summary>
        /// 单元格大小(宽和高)
        /// </summary>
        private Vector2 mCellSize;

        /// <summary>
        /// 单元格预制件索引
        /// </summary>
        public int CellPrefabIndex
        {
            get;
            private set;
        }

        /// <summary>
        /// 锚点最大值
        /// </summary>
        private Vector2 AnchorMax;

        /// <summary>
        /// 锚点最小值
        /// </summary>
        private Vector2 AnchorMin;

        /// <summary>
        /// 中心点
        /// </summary>
        private Vector2 Pivot;

        /// <summary>
        /// Cell的Rect的起始位置(x,y)
        /// </summary>
        private Vector2 mRectPos;

        /// <summary>
        /// Cell的Rect的绝对值起始位置(x,y)
        /// </summary>
        private Vector2 mRectAbsPos;

        /// <summary>
        /// Cell的Rect区域信息(用于判断是否在父节点有效显示区域)
        /// </summary>
        public Rect CellRect
        {
            get;
            private set;
        }
        
        public void onCreate()
        {
            //Debug.Log("NewCellData:onCreate()");
        }

        public void onDispose()
        {
            //Debug.Log("NewCellData:onDispose()");
            CellIndex = -1;
            mOwnerContainer = null;
            CellGO = null;
            CellGORectTransform = null;
            CellPrefabIndex = -1;
            mCellSize = Vector2.zero;
            mRectPos = Vector2.zero;
            mRectAbsPos = Vector2.zero;
            CellRect.Set(0.0f, 0.0f, 0.0f, 0.0f);
        }
        
        public CellData()
        {
            CellIndex = -1;
            mOwnerContainer = null;
            CellGO = null;
            CellGORectTransform = null;
            CellPrefabIndex = -1;
            mCellSize = Vector2.zero;
            mRectPos = Vector2.zero;
            mRectAbsPos = Vector2.zero;
            CellRect.Set(0.0f, 0.0f, 0.0f, 0.0f);
        }

        /// <summary>
        /// 设置初始数据
        /// </summary>
        /// <param name="cellprefabindex">单元格模板索引</param>
        /// <param name="container">单元格容器</param>
        /// <param name="size">单元格大小(默认不传是预制件大小)</param>
        public void setData(int cellprefabindex, BaseScrollContainer container, Vector2? size = null)
        {
            Debug.Assert(container != null, "单元格容器不允许传空!");
            CellPrefabIndex = cellprefabindex;
            mOwnerContainer = container;
            mCellSize = size == null ? mOwnerContainer.getPrefabTemplateSizeWithPrefabIndex(CellPrefabIndex) : (Vector2)size;
            CellRect.Set(0.0f, 0.0f, mCellSize.x, mCellSize.y);
        }

        /// <summary>
        /// 初始化单元格实例对象
        /// </summary>
        /// <param name="cellinstance"></param>
        public void init(GameObject cellinstance)
        {
            if(cellinstance != null)
            {
                CellGO = cellinstance;
                // 不同单元格容器公用同一个模板对象可能会出现锚点不一致问题，所以每次强制设置
                CellGORectTransform = CellGO.transform as RectTransform;
                CellGORectTransform.anchorMax = AnchorMax;
                CellGORectTransform.anchorMin = AnchorMin;
                CellGORectTransform.pivot = Pivot;
                updateCellSizeAndPosition();
                if (!CellGO.activeSelf)
                {
                    CellGO.SetActive(true);
                }
            }
            else
            {
                CellGO = null;
                CellGORectTransform = null;
            }
        }

        /// <summary>
        /// 更新单元格大小和位置数据
        /// </summary>
        public void updateCellSizeAndPosition()
        {
            CellGORectTransform.sizeDelta = mCellSize;
            CellGO.transform.localPosition = mRectPos;
        }

        /// <summary>
        /// 设置Cell的Rect区域
        /// </summary>
        /// <param name="cellrectpos">Cell单元格显示位置</param>
        /// <param name="cellmaskrect">cell的rect区域用于判定是否在Mask显示区域</param>
        public void setRect(Vector2 cellrectpos, Vector2 cellmaskrect)
        {
            if (CellPrefabIndex == -1)
            {
                return;
            }
            mRectPos.x = cellrectpos.x;
            mRectPos.y = cellrectpos.y;
            mRectAbsPos.x = Mathf.Abs(mRectPos.x);
            mRectAbsPos.y = Mathf.Abs(mRectPos.y);
            CellRect = new Rect(cellmaskrect.x, cellmaskrect.y, mCellSize.x, mCellSize.y);
        }

        /// <summary>
        /// 设置Cell预制件的Anchor和Pivot
        /// </summary>
        /// <param name="anchormax"></param>
        /// <param name="anchormin"></param>
        /// <param name="pivot"></param>
        public void setAnchor(Vector2 anchormax, Vector2 anchormin, Vector2 pivot)
        {
            AnchorMax = anchormax;
            AnchorMin = anchormin;
            Pivot = pivot;
        }

        /// <summary>
        /// 获取当前Cell大小(默认为预制件宽高)
        /// </summary>
        /// <returns></returns>
        public Vector2 getSize()
        {
            return mCellSize;
        }

        /// <summary>
        /// 改变当前Cell大小(手动改Size走这里)
        /// </summary>
        /// <param name="newsize">新的单元格大小</param>
        /// <returns></returns>
        public void changeSize(Vector2 newsize)
        {
            if(mCellSize.Equals(newsize))
            {
                return;
            }
            mCellSize = newsize;
            mOwnerContainer.forceUpdateDisplay(true);
            //Debug.Log($"单元格索引:{CellIndex}改变Size:[{mCellSize.x},{mCellSize.y}]");
        }

        /// <summary>
        /// 当前单元格是否可见
        /// </summary>
        /// <returns></returns>
        public bool isVisible()
        {
            return mOwnerContainer.MaskRect.Overlaps(CellRect);
        }

        /// <summary>
        /// 获取当前Cell相对RectContent位置
        /// </summary>
        /// <returns></returns>
        public Vector2 getPos()
        {
            return mRectPos;
        }

        /// <summary>
        /// 获取当前Cell相对RectContent绝对值位置
        /// </summary>
        /// <returns></returns>
        public Vector2 getAbsPos()
        {
            return mRectAbsPos;
        }
        
        /// <summary>
        /// Cell数据清除
        /// </summary>
        public void clear()
        {
            mOwnerContainer = null;
            CellGO = null;
            CellGORectTransform = null;
            mCellSize = Vector2.zero;
            mRectPos = Vector2.zero;
            CellRect = Rect.zero;
            ObjectPool.Singleton.push<CellData>(this);
        }
    }
}
