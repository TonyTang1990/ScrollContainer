/*
 * Description:             BaseScrollContainer.cs
 * Author:                  TONYTANG
 * Create Date:             2019/07/15
 */

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using DG.Tweening;
using System;

// Note:
// 网格单元格容器不支持动态大小，全部默认统一大小

namespace TH.Modules.UI
{
    [RequireComponent(typeof(ScrollRect))]
    [RequireComponent(typeof(Mask))]
    [RequireComponent(typeof(Image))]
    /// <summary>
    /// 滚动单元格Cell容器基类抽象
    /// </summary>
    public abstract class BaseScrollContainer : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerExitHandler
    {
        /// <summary>
        /// Scroll Direction
        /// 滚动方向
        /// </summary>
        public enum EScrollDir
        {
            None = 1,                   // 无
            ScrollLeft,                 // 像左滚动
            ScrollRight,                // 像右滚动
            ScrollUp,                   // 像上滚动
            ScrollDown,                 // 像下滚动
        }

        /// <summary>
        /// Avalible Drag Direction
        /// 允许的拖拽方向
        /// </summary>
        public enum EDragDirection
        {
            None = 1,                   // 不允许拖拽
            Horizontal,                 // 横向拖拽
            Vertical,                   // 纵向拖拽
        }

        /// <summary>
        /// Is always allowed scroll
        /// 是否允许滚动(总开关)
        /// </summary>
        [Header("是否允许滚动(总开关)")]
        public bool IsAllowedScroll = true;

        /// <summary>
        /// Is scrollable when scroll content is small than basic content size
        /// 是否允许内容没超出范围滚动
        /// </summary>
        [Header("是否允许内容未超框滚动")]
        public bool IsAllowedInsideScroll = false;

        /// <summary>
        /// Is interruptable when auto move 
        /// 自动滚动是否可被拖动打断
        /// </summary>
        [Header("自动滚动是否可被拖动打断")]
        public bool IsAutoScrollInteruptable = true;

        /// <summary>
        /// Cell position offset
        /// 起始位置偏移(含水平和垂直)
        /// </summary>
        [Header("起始位置偏移(含水平和垂直)")]
        public Vector2 BeginOffset = Vector2.one * 10;

        /// <summary>
        /// Cell space
        /// Cell单元格之间的间距
        /// </summary>
        [Header("单元格间距")]
        public float CellSpace = 10.0f;

        /// <summary>
        /// Is using correct effect(always stop at specific cell index after scrolling stop)
        /// 单元格位置矫正效果(矫正效果不支持容器内Cell单元格大小不同的情况)
        /// </summary>
        [Header("单元格位置矫正开关")]
        public bool CorrectCellPostionSwitch = false;

        /// <summary>
        /// The thredhold velocity to trigger correct cell
        /// 矫正速度阈值
        /// </summary>
        [Header("矫正速度阈值")]
        public float CorrectVelocityThredHold = 50f;

        /// <summary>
        /// Correct cell animation time
        /// 矫正时间(矫正时间越短速度越快)
        /// </summary>
        [Header("矫正时间")]
        public float CorrectCellTime = 0.5f;

        /// <summary>
        /// Parent container that is used to bubble scroll event(in order to support nested scrollable container)
        /// 向上传递滚动事件的父滚动容器
        /// </summary>
        [Header("向上传递滚动事件的父滚动容器")]
        public BaseScrollContainer EventBubbleContainerParent = null;

        /// <summary>
        /// Container prefab data list
        /// 预制件数据源列表
        /// </summary>
        [Header("预制件数据源列表")]
        public List<GameObject> SourcePrefabList;

        /// <summary>
        /// Parent scrollrect
        /// 向上传递滚动事件的父ScrollRect
        /// </summary>
        protected ScrollRect mEventBubbleScrollParent = null;

        /// <summary>
        /// Conatiner Game Object Pool
        /// 单元格对象池
        /// </summary>
        public GameObjectPool CellGoPool
        {
            get;
            private set;
        }

        /// <summary>
        /// ScrollRect Component
        /// 滚动组件
        /// </summary>
        public ScrollRect ScrollRect
        {
            get;
            protected set;
        }

        /// <summary>
        /// Scroll Mask Rect
        /// 显示过滤框
        /// </summary>
        public Rect MaskRect
        {
            get
            {
                return mMaskRect;
            }
        }
        protected Rect mMaskRect;

        /// <summary>
        /// Avalible Drag Direction
        /// 可拖拽的方向
        /// </summary>
        protected EDragDirection mAvalibleDragDirection;

        /// <summary>
        /// Current Scroll Index Value
        /// Note:
        /// If using grid container, this value is only caculated for first row or first colume cell
        /// 当前滚动到的索引值(当前滚动到的准确单元索引值)
        /// Note:
        /// 如果是使用网格容器，当前滚动值只会计算第一行或第一列的偏移滚动值
        /// </summary>
        public float CurrentScrollIndexValue
        {
            get;
            protected set;
        }

        /// <summary>
        /// Root RectTransform
        /// 根节点Rect信息
        /// </summary>
        protected RectTransform mRootRectContentTrasform;

        /// <summary>
        /// Content Transform
        /// 子节点(包含所有Cell的显示)
        /// </summary>
        protected Transform mRectContent;

        /// <summary>
        /// Content RectTransform
        /// 子节点Rect信息(根据Cell大小变化)
        /// </summary>
        public RectTransform RectContentTrasform
        {
            get;
            protected set;
        }

        /// <summary>
        /// Cell Data that is used to create cells
        /// Cell数据，用于创建Cell
        /// </summary>
        protected List<CellData> mCellDatas;

        /// <summary>
        /// Is under correct behaviour
        /// 是否正在矫正滚动
        /// </summary>
        protected bool mIsCorrectScrolling;

        /// <summary>
        /// Current scroll direction
        /// 当前滚动方向
        /// </summary>
        protected EScrollDir mCurrentScrollDir;

        /// <summary>
        /// Is drag end
        /// 是否结束拖拽(结束拖拽后才允许矫正判定)
        /// </summary>
        protected bool mIsEndDrag;

        /// <summary>
        /// MoveToIndex Tweener
        /// 单元格移动Tweener
        /// </summary>
        protected Tweener mCellMoveToTweener;

        /// <summary>
        /// Max scrollable distance
        /// 最大可滚动的距离
        /// </summary>
        protected float mAvalibleScrollDistance;

        /// <summary>
        /// Is bubbling event up
        /// 是否正在传递事件
        /// </summary>
        protected bool mIsDistaching = false;

        /// <summary>
        /// Is triggered awake
        /// 是否执行过Awake
        /// </summary>
        protected bool mIsAwakeComplete = false;

        /// <summary>
        /// Is correct container data completed(In order to complete auto size setting off root recttransform)
        /// 数据矫正完成(为了支持自适应的父节点)
        /// </summary>
        protected bool mIsCorrectDataComplete = false;

        /// <summary>
        /// Scroll Anchor Position
        /// 滚动锚点位置
        /// </summary>
        protected Vector2 mScrollAnchorPosition;

        /// <summary>
        /// Current Move To Cel  Index(Its valide when MoveToIndex called or correct cell triggered)
        /// 当前滚动到的单元格索引值(只在MoveToIndex和矫正到指定位置时有效,请勿使用)
        /// </summary>
        protected int CurrentMoveToCellIndex;

        /// <summary>
        /// Cell OnShow Delegate
        /// 单元格容器单元格显示回调
        /// </summary>
        protected Action<int, GameObject> mOnShowDelegate;

        /// <summary>
        /// Visible Cell Scroll Delegate(delegate that is triggered when the cell first time visible in content)
        /// 单元格容器单元格滚动回调
        /// </summary>
        protected Action<int, GameObject, float, float> mOnVisibleScrollDelegate;

        /// <summary>
        /// Container Move To Specific Index Delegate(delegate that is called when MoToIndex called or correct cell behaviour completed)
        /// 单元格容器滚动到指定索引单元格回调(矫正和调用MoveToIndex的时候会触发)
        /// </summary>
        protected Action<int, GameObject> mMoveToIndexDelegate;

        /// <summary>
        /// Cell OnHide Delegate(delegate that is triggered When the cell fist time invisible in content)
        /// 单元格容器单元格隐藏回调
        /// </summary>
        protected Action<int, GameObject> mOnHideDelegate;

        /// <summary>
        /// The max correct cell index value
        /// 最大矫正到单元格索引(仅当开启矫正时有用)
        /// </summary>
        protected int mMaxCorrectToCellIndex;

        /// <summary>
        /// Cell prefabs size info list
        /// 单元格模板大小信息列表
        /// </summary>
        protected List<Vector2> mCellTemplateSizeList;

        /// <summary>
        /// Cell prefgabs instance id info list
        /// 单元格模板InstanceID列表
        /// </summary>
        protected List<int> mCellTemplateInstanceIDList;

        /// <summary>
        /// Offset Position between Center of content and first cell position
        /// 中心点位置偏移
        /// </summary>
        protected Vector2 mCenterPositionOffset;

        public virtual void Awake()
        {
            // 如果是编辑器非运行模式不使用对象池，
            // 避免删除对象池根节点以及对象池相关对象带来的问题
            if (Application.isPlaying)
            {
                CellGoPool = new GameObjectPool(GetType().Name);
            }
            else
            {
                CellGoPool = null;
            }
            if(EventBubbleContainerParent != null)
            {
                mEventBubbleScrollParent = EventBubbleContainerParent.GetComponent<ScrollRect>();
            }
            mMaskRect = Rect.zero;
            ScrollRect = gameObject.GetComponent<ScrollRect>();
            if (ScrollRect == null)
            {
                ScrollRect = gameObject.AddComponent<ScrollRect>();
            }

            mRootRectContentTrasform = gameObject.transform.GetComponent<RectTransform>();
            mRectContent = gameObject.transform.GetChild(0);
            RectContentTrasform = mRectContent.GetComponent<RectTransform>();

            ScrollRect.content = RectContentTrasform;
            ScrollRect.elasticity = 0.15f;
            mIsCorrectScrolling = false;
            CurrentScrollIndexValue = 0.0f;
            mAvalibleDragDirection = EDragDirection.None;
            mCurrentScrollDir = EScrollDir.None;
            mIsEndDrag = true;
            updateScrollable();
            mIsAwakeComplete = true;
            CurrentMoveToCellIndex = -1;
            mMoveToIndexDelegate = null;
            mMaxCorrectToCellIndex = -1;
            initPrefabTemplateInfo();
        }

        // Use this for initialization
        public virtual void Start()
        {
            ScrollRect.onValueChanged.AddListener(onScrollChanged);
            TryCorrectData();
        }

        /// <summary>
        /// Init prefab relative info
        /// 初始化预制件大小信息
        /// </summary>
        protected void initPrefabTemplateInfo()
        {
            mCellTemplateSizeList = new List<Vector2>();
            mCellTemplateInstanceIDList = new List<int>();
            for (int i = 0, length = SourcePrefabList.Count; i < length; i++)
            {
                var prefabsize = SourcePrefabList[i].GetComponent<RectTransform>().rect.size;
                mCellTemplateSizeList.Add(prefabsize);
                mCellTemplateInstanceIDList.Add(SourcePrefabList[i].GetInstanceID());
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// Reset correct data status(In order to support editor container create cell simulation working)
        /// 重置矫正状态(为了支持编辑器模拟能够每次模拟都走正常流程)
        /// Note:
        /// 编辑器模拟前调用此方法，然后再调用Awake和Start方法
        /// </summary>
        public void ResetCorrectStatus()
        {
            mIsAwakeComplete = false;
            mIsCorrectDataComplete = false;
        }
#endif

        /// <summary>
        /// Correct Container Data(Only trigger once)
        /// 矫正数据(只矫正一次)
        /// </summary>
        protected void TryCorrectData()
        {
            if(mIsCorrectDataComplete == false)
            {
                if (mIsAwakeComplete)
                {
                    // Note:
                    // Awake里如果做了自适应设置，无法拿到mRootRectContentTransform的正确宽高，只能在Awake之后触发
                    RectContentTrasform.sizeDelta = new Vector2(mRootRectContentTrasform.rect.width, mRootRectContentTrasform.rect.height);
                    mMaskRect.width = mRootRectContentTrasform.rect.width;
                    mMaskRect.height = mRootRectContentTrasform.rect.height;
                    mIsCorrectDataComplete = true;
                    initCenterPositionOffset();
                }
                else
                {
                    Debug.LogError($"未执行Awake不允许执行数据矫正!");
                }
            }
        }

        /// <summary>
        /// Initialization for Center Position Offset
        /// 初始化中心位置偏移
        /// </summary>
        protected abstract void initCenterPositionOffset();

        protected void OnEnable()
        {
            mIsDistaching = false;
        }

        public void OnDestroy()
        {
            if (mCellDatas != null)
            {
                for (int i = 0; i < mCellDatas.Count; i++)
                {
                    onCellClear(i);
                }
            }
            CellGoPool?.clearAll();
            CellGoPool = null;
            EventBubbleContainerParent = null;
            mEventBubbleScrollParent = null;
            mCellDatas = null;
            ScrollRect = null;
            mRootRectContentTrasform = null;
            mRectContent = null;
            RectContentTrasform = null;
            unbindContainerCallBack();
        }

        /// <summary>
        /// Change scrollable status
        /// 改变可滚动状态
        /// </summary>
        /// <param name="isenable"></param>
        public abstract void changeScrollable(bool isenable);

        /// <summary>
        /// Move to specific index
        /// 移动到特定Cell单元格位置
        /// </summary>
        /// <param name="index">Cell Index</param>
        /// <param name="movetime">移动时长</param>
        public virtual bool moveToIndex(int index = 0, float movetime = 1.0f)
        {
            mCellMoveToTweener?.Kill();
            return index >= 0 && index < mCellDatas.Count;
        }

        /// <summary>
        /// Bind container delegate
        /// 绑定单元格回调(此方法在使用单元格前比调)
        /// </summary>
        /// <param name="onshow"></param>
        /// <param name="onhide"></param>
        /// <param name="onvisiblescroll"></param>
        /// <param name="onmoveto"></param>
        public void bindContainerCallBack(Action<int, GameObject> onshow, Action<int, GameObject> onhide = null, Action<int, GameObject, float, float> onvisiblescroll = null, Action<int, GameObject> onmoveto = null)
        {
            mOnShowDelegate = onshow;
            mOnVisibleScrollDelegate = onvisiblescroll;
            mMoveToIndexDelegate = onmoveto;
            mOnHideDelegate = onhide;
        }

        /// <summary>
        /// Unbind container delegate
        /// 解除绑定单元格回调(一般不需要手动调用)
        /// </summary>
        public void unbindContainerCallBack()
        {
            mOnShowDelegate = null;
            mOnVisibleScrollDelegate = null;
            mMoveToIndexDelegate = null;
            mOnHideDelegate = null;
        }

        /// <summary>
        /// Get prefab gameobject with prefab index
        /// 获取指定预制件索引的单元格模板
        /// </summary>
        /// <param name="prefabindex"></param>
        /// <returns></returns>
        public GameObject getPrefabTemplateWithPrefabIndex(int prefabindex)
        {
            Debug.Assert(prefabindex < SourcePrefabList.Count, $"预制件索引:{prefabindex}超出最大预制件数量设置:{SourcePrefabList.Count},获取对应预制件模板失败!");
            return SourcePrefabList[prefabindex];
        }

        /// <summary>
        /// Get prefab size with prefab index
        /// 获取指定预制件索引的单元格大小信息
        /// </summary>
        /// <param name="prefabindex"></param>
        /// <returns></returns>
        public Vector2 getPrefabTemplateSizeWithPrefabIndex(int prefabindex)
        {
            Debug.Assert(prefabindex < SourcePrefabList.Count, $"预制件索引:{prefabindex}超出最大预制件数量设置:{SourcePrefabList.Count},获取对应预制件模板大小失败!");
            return mCellTemplateSizeList[prefabindex];
        }

        /// <summary>
        /// Get prefab size with cell index
        /// 获取指定预制件索引的单元格大小信息
        /// </summary>
        /// <param name="cellindex"></param>
        /// <returns></returns>
        public Vector2 getPrefabTemplateSizeWithCellIndex(int cellindex)
        {
            Debug.Assert(cellindex < mCellDatas.Count, $"单元格索引:{cellindex}超出最大单元格数量设置:{mCellDatas.Count},获取对应预制件模板大小失败!");            
            return getPrefabTemplateSizeWithPrefabIndex(mCellDatas[cellindex].CellPrefabIndex);
        }

        /// <summary>
        /// Get prefab instance id with prefab index
        /// 获取指定预制件索引的单元格InstanceID
        /// </summary>
        /// <param name="prefabindex"></param>
        /// <returns></returns>
        public int getPrefabTemplateInstanceIDWithPrefabIndex(int prefabindex)
        {
            Debug.Assert(prefabindex < SourcePrefabList.Count, $"预制件索引:{prefabindex}超出最大预制件数量设置:{SourcePrefabList.Count},获取对应预制件模板InstanceID失败!");
            return mCellTemplateInstanceIDList[prefabindex];
        }

        /// <summary>
        /// Set container celldatas by pass data
        /// 设置Container的cell数据通过列表数据
        /// </summary>
        /// <param name="prefabindexlist">预制件索引列表</param>
        /// <param width="cellsizelist">单元格大小列表(为空表示采用预制件默认大小)</param>
        /// <param name="scrollnormalizedposition">单元格初始滚动位置</param>
        public void setCellDatasByDataList(List<int> prefabindexlist, List<Vector2> cellsizelist = null, Vector2? scrollnormalizedposition = null)
        {
            var celldatalist = createNormalCellDataList(prefabindexlist, cellsizelist);
            setCellDatas(celldatalist, scrollnormalizedposition);
        }

        /// <summary>
        /// Set container celldatas by pass cell count
        /// 设置Container的cell数据通过单元格数量
        /// </summary>
        /// <param name="prefabindexlist">预制件索引列表</param>
        /// <param width="cellsizelist">单元格大小列表(为空表示采用预制件默认大小)</param>
        /// <param name="scrollnormalizedposition">单元格初始滚动位置</param>
        public void setCellDatasByCellCount(int cellcount, Vector2? scrollnormalizedposition = null)
        {
            var celldatalist = createNormalCellDataListWithCount(cellcount);
            setCellDatas(celldatalist, scrollnormalizedposition);
        }

        /// <summary>
        /// Set container celldatas by pass celldata list
        /// 设置Container的cell数据
        /// </summary>
        /// <param name="celldatas">所有的Cell数据</param>
        /// <param name="scrollnormalizedposition">单元格初始滚动位置</param>
        public void setCellDatas(List<CellData> celldatas, Vector2? scrollnormalizedposition = null)
        {
            // 为了支持比容器先进入Start执行数据设置的情况
            if (mIsCorrectDataComplete == false)
            {
                TryCorrectData();
            }
            if (mIsCorrectDataComplete)
            {
                clearCellDatas();
                mCellDatas = celldatas;
                updateContainerData(scrollnormalizedposition);
                updateMaxCorrectToCellIndex();
                // 如果开启了矫正强制矫正一次，确保初始化时的位置是正确的
                checkCellPostionCorrect(mCurrentScrollDir, true, false);
            }
            else
            {
                Debug.LogError($"组件:{this.gameObject.name}未完成矫正数据，不允许设置数据，请在Start里执行初始化流程!");
            }
            display();
        }

        /// <summary>
        /// Clear cell datas
        /// 清除单元格数据
        /// Note:
        /// 嵌套单元格容器进池前必须调用此函数，否则会造成滚动过程中入对象池出对象池后带来的各种问题
        /// 如果嵌套单元格想还原准确的单元格滚动位置，建议如下：
        /// 上层单元格自行记录单元格滚动位置(ScrollRect的horizontalNormalizedPosition或者verticalNormalizedPosition)
        /// 然后创建时设置单元格数据(setCellDatas)时，直接传入ScrollRect的滚动位置
        /// 最后再OnDestroy的时候再清空自行记录单元格滚动位置数据即可
        /// </summary>
        /// <returns>返回清除单元格数据时最终停留(如果开启矫正，则为矫正后位置)的滚动位置</returns>
        public Vector2 clearCellDatas()
        {
            var finalscrollnormalizedposition = Vector2.zero;
            finalscrollnormalizedposition = ScrollRect.normalizedPosition;
            if (mCellDatas != null)
            {
                // 调整容器数据时，强行停止滚动相关操作，避免刷新或进池出池等带来的过多问题
                forceScrollStop();
                // 这里专门返回清除单元格后的滚动位置，方便上层记录后下次快速还原位置
                // 因为嵌套单元格可能在滚动过程中就进池了，所以进池前必须强制停止并矫正到正确位置
                finalscrollnormalizedposition = ScrollRect.normalizedPosition;
                // 清理之前的滚动数据
                for (int i = 0; i < mCellDatas.Count; i++)
                {
                    onCellClear(i);
                }
                mCellDatas = null;
                // 还原初始的单元格容器位置数据
                updateContainerData();
            }
            mMaxCorrectToCellIndex = -1;
            return finalscrollnormalizedposition;
        }

        /// <summary>
        /// Get total number of cells
        /// 获取总的单元格数量
        /// </summary>
        /// <returns></returns>
        public int getCellTotalCount()
        {
            return mCellDatas != null ? mCellDatas.Count : 0;
        }

        /// <summary>
        /// Get current scroll position(0 -- 1)
        /// 获取滚动进度
        /// </summary>
        /// <returns></returns>
        public Vector2 getScrollNormalizedPosition()
        {
            return ScrollRect != null ? ScrollRect.normalizedPosition : Vector2.zero;
        }

        /// <summary>
        /// Get cell data with index
        /// 获取指定索引位置的单元格
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public CellData getCellDataWithIndex(int index)
        {
            if (mCellDatas != null && index >= 0 && index < mCellDatas.Count)
            {
                return mCellDatas[index];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Add celldata with index
        /// 指定位子添加单元格数据
        /// </summary>
        /// <param name="celldata">需要添加的单元格</param>
        /// <param name="index">单元格添加位置索引</param>
        /// <returns></returns>
        public bool addCellDataWithIndex(CellData celldata, int index = 0)
        {
            Debug.Assert(celldata != null, "不允许添加空的单元格数据!");
            var maxindex = ((mCellDatas != null) ? mCellDatas.Count : 0);
            if (index >= 0 && index <= maxindex)
            {
                forceScrollStop();
                if (mCellDatas == null)
                {
                    mCellDatas = new List<CellData>();
                }
                mCellDatas.Insert(index, celldata);
                forceUpdateDisplay();
                return true;
            }
            else
            {
                Debug.LogError($"无效的索引位置:{index},有效范围0-{maxindex},添加单元格失败!");
                return false;
            }
        }

        /// <summary>
        /// Add celldata list with index
        /// 指定位子添加单元格数据列表
        /// </summary>
        /// <param name="celldatalist">需要添加的单元格信息列表</param>
        /// <param name="index">单元格添加位置索引</param>
        /// <returns></returns>
        public bool addCellDataListWithIndex(List<CellData> celldatalist, int index = 0)
        {
            Debug.Assert(celldatalist != null, "不允许添加空的单元格数据列表!");
            var maxindex = ((mCellDatas != null) ? mCellDatas.Count : 0);
            if (index >= 0 && index <= maxindex)
            {
                forceScrollStop();
                if (mCellDatas == null)
                {
                    mCellDatas = new List<CellData>();
                }
                mCellDatas.InsertRange(index, celldatalist);
                forceUpdateDisplay();
                return true;
            }
            else
            {
                Debug.LogError($"无效的索引位置:{index},有效范围0-{maxindex},添加单元格列表失败!");
                return false;
            }
        }

        /// <summary>
        /// Add celldata to beginning
        /// 添加单元格数据到头部
        /// </summary>
        /// <param name="celldata">需要添加到头部的单元格</param>
        /// <returns></returns>
        public bool addCellDataToStart(CellData celldata)
        {
            return addCellDataWithIndex(celldata, 0);
        }

        /// <summary>
        /// Add celldata to end
        /// 添加单元格数据到尾部
        /// </summary>
        /// <param name="celldata">需要添加到尾部的单元格</param>
        /// <returns></returns>
        public bool addCellDataToEnd(CellData celldata)
        {
            var maxindex = ((mCellDatas != null) ? mCellDatas.Count : 0);
            return addCellDataWithIndex(celldata, maxindex);
        }

        /// <summary>
        /// Delete celldata with index
        /// 删除指定位子单元格
        /// </summary>
        /// <param name="index">单元格移除位置索引</param>
        /// <returns></returns>
        public bool removeCellWithIndex(int index = 0)
        {
            var maxindex = ((mCellDatas != null) ? mCellDatas.Count : 0);
            if (mCellDatas != null)
            {
                if (index >= 0 && index < maxindex)
                {
                    forceScrollStop();
                    onCellClear(index);
                    mCellDatas.RemoveAt(index);
                    forceUpdateDisplay();
                    return true;
                }
                else
                {
                    Debug.LogError($"无效的索引位置:{index},有效范围0-{maxindex - 1},删除单元格失败!");
                    return false;
                }
            }
            else
            {
                Debug.LogError($"当前没有有效的单元格数据,删除单元格索引:{index}失败!");
                return false;
            }
        }

        /// <summary>
        /// Get container size
        /// 获取整个滚动容器的大小
        /// </summary>
        /// <returns></returns>
        public Vector2 getSize()
        {
            return RectContentTrasform.sizeDelta;
        }

        /// <summary>
        /// Force update display
        /// 强制容器刷新显示(包含数据更新和显示刷新，显示更新仅当已经触发显示的情况下)
        /// </summary>
        /// <returns></returns>
        public void forceUpdateDisplay(bool keeprectcontentpos = false)
        {
            // 默认保留原始滚动位置的刷新方式
            updateContainerData(ScrollRect.normalizedPosition, keeprectcontentpos);
            display(true);
            forceScrollStop();
        }

        /// <summary>
        /// Refresh all celld display
        /// 刷新显示所有Cell(用于只触发刷新逻辑 e.g. BaseCell:OnShow())
        /// </summary>
        public void refreshDisplay()
        {
            if (mCellDatas != null)
            {
                for (int i = 0; i < mCellDatas.Count; i++)
                {
                    onCellDisplay(i);
                }
            }
        }

        /// <summary>
        /// On begin drag
        /// 响应开始拖拽
        /// </summary>
        /// <param name="eventData"></param>
        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            //Debug.Log($"单元格容器:{gameObject.name}响应OnBeginDrag!");
            //Debug.Log($"是否应该分发事件到父滚动容器:{shouldBubblehEvent(eventData)}");
            if (shouldBubblehEvent(eventData))
            {
                //Debug.Log($"分发事件OnBeginDrag到父滚动容器:{EventBubbleScrollParent.gameObject.name}");
                // 如果需要传递，那么确保当前滚动栏是不允许响应拖拽的，
                // 不然横向纵向双向移动，会带来很多问题(比如入池出池依然被控制滚动，单元格矫正无法在入池出池时正确进行)
                ScrollRect.enabled = false;
                mIsDistaching = true;
                EventBubbleContainerParent.OnBeginDrag(eventData);
                mEventBubbleScrollParent.OnBeginDrag(eventData);
                //Debug.Log($"容器:{this.gameObject.name}通知父滚动容器:{mEventBubbleScrollParent.gameObject.name}开始拖拽!");
            }
            else
            {
                //Debug.Log($"容器:{this.gameObject.name}开始拖拽!");
                mIsDistaching = false;
                doBeginDrag(eventData);
            }
        }

        /// <summary>
        /// On drag
        /// 响应拖拽
        /// </summary>
        /// <param name="eventData"></param>
        public virtual void OnDrag(PointerEventData eventData)
        {
            //Debug.Log($"单元格容器:{gameObject.name}响应OnDrag!");
            if (mIsDistaching)
            {
                //Debug.Log($"分发事件OnDrag到父滚动容器:{EventBubbleScrollParent.gameObject.name}");
                //Debug.Log($"容器:{this.gameObject.name}通知父滚动容器:{EventBubbleScrollParent.gameObject.name}拖拽!!");
                // 这里往父类分发事件有几率父类BaseContainer的OnBeginDrag,OnDrag,OnEndDrag响应不到，原因不明
                // 为了确保父类BaseContainer正确触发滚动方向判定，确保最后的单元格矫正计算，这里强制触发单元格方向刷新
                EventBubbleContainerParent.OnDrag(eventData);
                EventBubbleContainerParent.updateScrollDir(eventData);

                mEventBubbleScrollParent.OnDrag(eventData);
            }
            else
            {
                //Debug.Log($"容器:{this.gameObject.name}拖拽!");
                doDrag(eventData);
            }
        }

        /// <summary>
        /// On end drag
        /// 响应结束拖拽
        /// </summary>
        /// <param name="eventData"></param>
        public virtual void OnEndDrag(PointerEventData eventData)
        {
            if (mIsDistaching)
            {
                //Debug.Log($"分发事件OnEndDrag到父滚动容器:{EventBubbleScrollParent.gameObject.name}");
                mIsDistaching = false;
                EventBubbleContainerParent.OnEndDrag(eventData);
                mEventBubbleScrollParent.OnEndDrag(eventData);
                //Debug.Log($"容器:{this.gameObject.name}通知父滚动容器:{mEventBubbleScrollParent.gameObject.name}结束拖拽!");
                ScrollRect.enabled = true;
            }
            else
            {
                //Debug.Log($"容器:{this.gameObject.name}结束拖拽!");
                doEndDrag(eventData);
            }
        }

        /// <summary>
        /// Display all celldatas
        /// 显示更新所有Cell
        /// </summary>
        /// <param name="forcerefreshcellsize">是否强制刷新单元格大小</param>
        protected abstract void display(bool forcerefreshcellsize = false);

        /// <summary>
        /// Update scrollable status
        /// 更新可滚动状态
        /// </summary>
        public abstract void updateScrollable();
        
        /// <summary>
        /// Execute begin drag
        /// 执行开始拖拽
        /// </summary>
        /// <param name="eventData"></param>
        protected void doBeginDrag(PointerEventData eventData)
        {
            //Debug.Log($"单元格容器:{gameObject.name}开始拖拽!");
            if (IsAutoScrollInteruptable == true)
            {
                mCellMoveToTweener?.Kill();
                mIsEndDrag = false;
                if (CorrectCellPostionSwitch && mIsCorrectScrolling)
                {
                    mIsCorrectScrolling = false;
                }
            }
        }

        /// <summary>
        /// Execute on drag
        /// 执行拖拽
        /// </summary>
        /// <param name="eventData"></param>
        protected void doDrag(PointerEventData eventData)
        {
            updateScrollDir(eventData);
        }

        /// <summary>
        /// Execute end drag
        /// 执行结束拖拽
        /// </summary>
        /// <param name="eventData"></param>
        protected void doEndDrag(PointerEventData eventData)
        {
            //Debug.Log($"单元格容器:{gameObject.name}结束拖拽!");
            mIsEndDrag = true;
            // 避免结束拖拽时没有速度导致未触发onScrollChanged
            onScrollChanged(Vector2.zero);
        }

        /// <summary>
        /// Is bubbling event up to parent
        /// 是否应该转发本次事件传递至父级
        /// </summary>
        protected bool shouldBubblehEvent(PointerEventData eventData)
        {
            if (mEventBubbleScrollParent != null)
            {
                if (IsAllowedScroll == true)
                {
                    var delta = eventData.delta;
                    var movedir = determineMoveDirection(delta.x, delta.y, 0.6f);
                    switch (mAvalibleDragDirection)
                    {
                        case EDragDirection.Vertical:
                            return movedir == MoveDirection.Left || movedir == MoveDirection.Right;
                        case EDragDirection.Horizontal:
                            return movedir == MoveDirection.Up || movedir == MoveDirection.Down;
                        case EDragDirection.None:
                            return true;
                    }
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Detect move direction
        /// 计算手势移动方向
        /// </summary>
        protected MoveDirection determineMoveDirection(float x, float y, float deadZone)
        {
            MoveDirection movedir = MoveDirection.None;
            if (new Vector2(x, y).sqrMagnitude > deadZone * deadZone)
            {
                if (Mathf.Abs(x) > Mathf.Abs(y))
                {
                    movedir = x > 0 ? MoveDirection.Right : MoveDirection.Left;
                }
                else
                {
                    movedir = y > 0 ? MoveDirection.Up : MoveDirection.Down;
                }
            }
            return movedir;
        }

        /// <summary>
        /// Force stop scrolling
        /// 强制停止滚动移动
        /// </summary>
        protected virtual void forceScrollStop()
        {
            // 强制停止移动，确保单元格矫正和滚动相关逻辑正确完成
            ScrollRect.StopMovement();
            checkCellPostionCorrect(mCurrentScrollDir, true, false);
            mCellMoveToTweener?.Kill(true);
        }

        /// <summary>
        /// Check whether trigger correct cell behaviour
        /// 检查单元格位置矫正(子类重写去支持自定义矫正)
        /// </summary>
        /// <param name="scrolldir"></param>
        /// <param name="igorepositioncheckthredhold">是否忽略单元格矫正速度判定</param>
        /// <param name="requirescrolldir">是否要求有效滚动方向才矫正</param>
        /// <returns></returns>
        protected virtual bool checkCellPostionCorrect(EScrollDir scrolldir, bool igorepositioncheckthredhold = false, bool requirescrolldir = true)
        {
            return false;
        }

        /// <summary>
        /// Update container relative datas
        /// 更新容器数据
        /// </summary>
        /// <param name="scrollnormalizaedposition">单元格初始滚动位置</param>
        /// <param name="keeprectcontentpos">是否保持rect content的相对位置(优先于scrollnormalizaedposition)</param>
        protected abstract void updateContainerData(Vector2? scrollnormalizaedposition = null, bool keeprectcontentpos = false);

        /// <summary>
        /// Update max correct cell index
        /// 更新最大矫正到单元格索引值
        /// </summary>
        protected abstract void updateMaxCorrectToCellIndex();

        /// <summary>
        /// Get specific cell index center position offset
        /// 获取指定单元格离中心点位置的偏移
        /// </summary>
        /// <param name="currentscrollabspos"></param>
        /// <param name="cellindex"></param>
        /// <returns></returns>
        protected abstract float getCellCenterPositionOffset(float currentscrollabspos, int cellindex);

        /// <summary>
        /// Get Current Scroll Position
        /// 获取当前滚动到的位置
        /// </summary>
        /// <returns></returns>
        protected abstract float getCurrentScrollPosition();

        /// <summary>
        /// Update scroll direction
        /// 更新滚动方向数据
        /// </summary>
        /// <param name="eventData"></param>
        public abstract void updateScrollDir(PointerEventData eventData);

        /// <summary>
        /// On scrolling
        /// 响应滚动
        /// </summary>
        /// <param name="scrollpos"></param>
        protected abstract void onScrollChanged(Vector2 scrollpos);

        /// <summary>
        /// On pointer exit
        /// 响应点击退出
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerExit(PointerEventData eventData)
        {
            // 避免手动强制停止滚动无法矫正问题
            checkCellPostionCorrect(mCurrentScrollDir);
        }

        /// <summary>
        /// On cell display
        /// 单元格显示判定
        /// </summary>
        /// <param name="index"></param>
        /// <param name="forcerefreshcellsize">是否强制刷新单元格大小</param>
        protected virtual void onCellDisplay(int index, bool forcerefreshcellsize = false)
        {
            if (!mCellDatas[index].isVisible())
            {
                if (mCellDatas[index].CellGO != null)
                {
                    onCellHide(index);
                }
            }
            else
            {
                if (forcerefreshcellsize && mCellDatas[index].CellGO)
                {
                    mCellDatas[index].updateCellSizeAndPosition();
                }
                if (!mCellDatas[index].CellGO)
                {
                    onCellInit(index);
                    onCellShow(index);
                }
                var currentscrollpos = getCurrentScrollPosition();
                //统一换算成正的偏移位置，方便统一正向和逆向的滚动计算
                currentscrollpos = Mathf.Abs(currentscrollpos);
                var cellcenteroffsetposition = getCellCenterPositionOffset(currentscrollpos, index);
                onCellVisibleScroll(index, CurrentScrollIndexValue, cellcenteroffsetposition);
            }
        }

        /// <summary>
        /// On Cell Initialization
        /// 响应单元格第一次初始化
        /// </summary>
        /// <param name="index"></param>
        protected virtual void onCellInit(int index)
        {
            if (mCellDatas[index].CellGO == null)
            {
                var cellprefabtemplate = getPrefabTemplateWithPrefabIndex(mCellDatas[index].CellPrefabIndex);
                var cellgo = CellGoPool != null ? CellGoPool.pop(cellprefabtemplate) : GameObject.Instantiate(cellprefabtemplate);
                cellgo.transform.SetParent(RectContentTrasform, false);
                mCellDatas[index].init(cellgo);
            }
            else
            {
                Debug.LogError($"单元格索引:{index}实例对象不为空,不应该进入这里!");
            }
        }

        /// <summary>
        /// On cell show
        /// 响应单元格显示
        /// </summary>
        /// <param name="index"></param>
        protected virtual void onCellShow(int index)
        {
            // OnShow逻辑
            mOnShowDelegate?.Invoke(index, mCellDatas[index].CellGO);
        }

        /// <summary>
        /// On cell hide
        /// 响应单元格隐藏
        /// </summary>
        /// <param name="index"></param>
        protected virtual void onCellHide(int index)
        {
            if (CellGoPool != null)
            {
                var cellprefabinstanceid = getPrefabTemplateInstanceIDWithPrefabIndex(mCellDatas[index].CellPrefabIndex);
                CellGoPool.push(cellprefabinstanceid, mCellDatas[index].CellGO);
            }
            else
            {
#if UNITY_EDITOR
                // 支持编辑器模式模拟创建销毁单元格
                if (Application.isPlaying)
                {
                    GameObject.Destroy(mCellDatas[index].CellGO);
                }
                else
                {
                    GameObject.DestroyImmediate(mCellDatas[index].CellGO);
                }
#else
                GameObject.Destroy(mCellDatas[index].CellGO);
#endif
            }
            // OnHide隐藏逻辑
            mOnHideDelegate?.Invoke(index, mCellDatas[index].CellGO);
            mCellDatas[index].init(null);
        }

        /// <summary>
        /// On cell visible scroll
        /// 响应单元格可见滚动
        /// </summary>
        /// <param name="index">单元格索引</param>
        /// <param name="currentscrollindexvalue">当前滚动到的单元格索引值</param>
        /// <param name="centeroffsetposition">当前单元格离中心点位置偏移</param>
        protected virtual void onCellVisibleScroll(int index, float currentscrollindexvalue, float centeroffsetposition)
        {
            // onVisibleScroll单元格可见滚动逻辑
            mOnVisibleScrollDelegate?.Invoke(index, mCellDatas[index].CellGO, currentscrollindexvalue, centeroffsetposition);
        }

        /// <summary>
        /// On cell clear
        /// 响应单元格清理
        /// </summary>
        /// <param name="index"></param>
        protected virtual void onCellClear(int index)
        {
            if (mCellDatas[index].isVisible())
            {
                if (mCellDatas[index].CellGO != null)
                {
                    onCellHide(index);
                }
            }
            mCellDatas[index].clear();
        }

        #region Create Cell Datas API(创建单元格数据接口)
        /// <summary>
        /// Create celldata
        /// 创建CellData方法
        /// </summary>
        /// <param name="prefabindex">预制件索引</param>
        /// <param width="size">单元格大小</param>
        /// <returns></returns>
        public CellData createNormalCellData(int prefabindex = 0, Vector2? size = null)
        {
            if (prefabindex >= SourcePrefabList.Count)
            {
                Debug.LogError($"容器:{gameObject.name}单元格数据总数量:{SourcePrefabList.Count},预制件索引:{prefabindex}超出范围!");
                return null;
            }
            var cell = ObjectPool.Singleton.pop<CellData>();
            cell.setData(prefabindex, this, size);
            return cell;
        }

        /// <summary>
        /// Create celldata list by pass cell info
        /// 创建CellData方法列表(优化CreateNormalCellData调用多次问题)
        /// </summary>
        /// <param name="prefabindexlist">预制件索引列表</param>
        /// <param width="cellsizelist">单元格大小列表(为空表示采用预制件默认大小)</param>
        /// <returns></returns>
        public List<CellData> createNormalCellDataList(List<int> prefabindexlist, List<Vector2> cellsizelist = null)
        {
            if(cellsizelist != null)
            {
                Debug.Assert(prefabindexlist.Count == cellsizelist.Count, $"如果单元格大小不为空，那么单元格大小数据列表必须和预制件索引列表长度一致!");
            }
            var celldatalist = new List<CellData>();
            for(int i = 0, length = prefabindexlist.Count; i < length; i++)
            {
                var prefabindex = prefabindexlist[i];
                if (prefabindex >= SourcePrefabList.Count)
                {
                    Debug.LogError($"容器:{gameObject.name}单元格数据总数量:{SourcePrefabList.Count},预制件索引:{prefabindex}超出范围!");
                    return null;
                }
                var cell = ObjectPool.Singleton.pop<CellData>();
                Vector2? sizevalue = null;
                if(cellsizelist != null)
                {
                    sizevalue = cellsizelist[i];
                }
                cell.setData(prefabindex, this, sizevalue);
                celldatalist.Add(cell);
            }
            return celldatalist;
        }

        /// <summary>
        /// Create celldata list by pass cell total number
        /// 创建CellData方法列表(优化CreateNormalCellData调用多次问题)
        /// </summary>
        /// <param name="cellcount">单元格数量(默认全部用第一个预制件)</param>
        /// <param width="cellsizelist">单元格大小列表(为空表示采用预制件默认大小)</param>
        /// <returns></returns>
        public List<CellData> createNormalCellDataListWithCount(int cellcount, List<Vector2> cellsizelist = null)
        {
            if (cellsizelist != null)
            {
                Debug.Assert(cellcount == cellsizelist.Count, $"如果单元格大小不为空，那么单元格大小数据列表必须和预制件索引列表长度一致!");
            }
            if (SourcePrefabList.Count == 0)
            {
                Debug.LogError($"容器:{gameObject.name}预制件总数量:{SourcePrefabList.Count}不能为0!");
                return null;
            }
            var celldatalist = new List<CellData>();
            for (int i = 0; i < cellcount; i++)
            {
                var cell = ObjectPool.Singleton.pop<CellData>();
                Vector2? sizevalue = null;
                if (cellsizelist != null)
                {
                    sizevalue = cellsizelist[i];
                }
                cell.setData(0, this, sizevalue);
                celldatalist.Add(cell);
            }
            return celldatalist;
        }
        #endregion
    }
}
