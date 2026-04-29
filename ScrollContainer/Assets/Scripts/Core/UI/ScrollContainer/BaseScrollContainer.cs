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
            UpdateScrollable();
            mIsAwakeComplete = true;
            CurrentMoveToCellIndex = -1;
            mMoveToIndexDelegate = null;
            mMaxCorrectToCellIndex = -1;
            InitPrefabTemplateInfo();
            HideAllPrefabTemplate();
        }

        // Use this for initialization
        public virtual void Start()
        {
            ScrollRect.onValueChanged.AddListener(OnScrollChanged);
            TryCorrectData();
        }

        /// <summary>
        /// Init prefab relative info
        /// 初始化预制件大小信息
        /// </summary>
        protected void InitPrefabTemplateInfo()
        {
            mCellTemplateSizeList = new List<Vector2>();
            mCellTemplateInstanceIDList = new List<int>();
            for (int i = 0, length = SourcePrefabList.Count; i < length; i++)
            {
                var prefabSize = SourcePrefabList[i].GetComponent<RectTransform>().rect.size;
                mCellTemplateSizeList.Add(prefabSize);
                mCellTemplateInstanceIDList.Add(SourcePrefabList[i].GetInstanceID());
            }
        }

        /// <summary>
        /// 隐藏所有预制件模板
        /// </summary>
        protected void HideAllPrefabTemplate()
        {
            if(SourcePrefabList == null || SourcePrefabList.Count == 0)
            {
                return;
            }
            foreach(var prefab in SourcePrefabList)
            {
                prefab.SetActive(false);
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
                    InitCenterPositionOffset();
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
        protected abstract void InitCenterPositionOffset();

        protected void OnEnable()
        {
            mIsDistaching = false;
        }

        public void OnDestroy()
        {
            // 优先解绑，避免逻辑回调导致上层已经清理的情况出问题
            UnbindContainerCallBack();
            if (mCellDatas != null)
            {
                for (int i = 0; i < mCellDatas.Count; i++)
                {
                    OnCellClear(i);
                }
            }
            CellGoPool?.ClearAll();
            CellGoPool = null;
            EventBubbleContainerParent = null;
            mEventBubbleScrollParent = null;
            mCellDatas = null;
            ScrollRect = null;
            mRootRectContentTrasform = null;
            mRectContent = null;
            RectContentTrasform = null;
        }

        /// <summary>
        /// Change scrollable status
        /// 改变可滚动状态
        /// </summary>
        /// <param name="isEnable"></param>
        public abstract void ChangeScrollable(bool isEnable);

        /// <summary>
        /// Move to specific index
        /// 移动到特定Cell单元格位置
        /// </summary>
        /// <param name="index">Cell Index</param>
        /// <param name="moveTime">移动时长</param>
        public virtual bool MoveToIndex(int index = 0, float moveTime = 1.0f)
        {
            mCellMoveToTweener?.Kill();
            return index >= 0 && index < mCellDatas.Count;
        }

        /// <summary>
        /// Bind container delegate
        /// 绑定单元格回调(此方法在使用单元格前比调)
        /// </summary>
        /// <param name="onShow"></param>
        /// <param name="onHide"></param>
        /// <param name="onVisibleScroll"></param>
        /// <param name="onMoveto"></param>
        public void BindContainerCallBack(Action<int, GameObject> onShow,
                                          Action<int, GameObject> onHide = null,
                                          Action<int, GameObject, float, float> onVisibleScroll = null,
                                          Action<int, GameObject> onMoveto = null)
        {
            mOnShowDelegate = onShow;
            mOnVisibleScrollDelegate = onVisibleScroll;
            mMoveToIndexDelegate = onMoveto;
            mOnHideDelegate = onHide;
        }

        /// <summary>
        /// Unbind container delegate
        /// 解除绑定单元格回调(一般不需要手动调用)
        /// </summary>
        public void UnbindContainerCallBack()
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
        /// <param name="prefabIndex"></param>
        /// <returns></returns>
        public GameObject GetPrefabIndexTemplate(int prefabIndex)
        {
            Debug.Assert(prefabIndex < SourcePrefabList.Count, $"预制件索引:{prefabIndex}超出最大预制件数量设置:{SourcePrefabList.Count},获取对应预制件模板失败!");
            return SourcePrefabList[prefabIndex];
        }

        /// <summary>
        /// Get prefab size with prefab index
        /// 获取指定预制件索引的单元格大小信息
        /// </summary>
        /// <param name="prefabIndex"></param>
        /// <returns></returns>
        public Vector2 GetPrefabIndexTemplateSize(int prefabIndex)
        {
            Debug.Assert(prefabIndex < SourcePrefabList.Count, $"预制件索引:{prefabIndex}超出最大预制件数量设置:{SourcePrefabList.Count},获取对应预制件模板大小失败!");
            return mCellTemplateSizeList[prefabIndex];
        }

        /// <summary>
        /// Get prefab size with cell index
        /// 获取指定预制件索引的单元格大小信息
        /// </summary>
        /// <param name="cellIndex"></param>
        /// <returns></returns>
        public Vector2 GetCellIndexTemplateSize(int cellIndex)
        {
            Debug.Assert(cellIndex < mCellDatas.Count, $"单元格索引:{cellIndex}超出最大单元格数量设置:{mCellDatas.Count},获取对应预制件模板大小失败!");            
            return GetPrefabIndexTemplateSize(mCellDatas[cellIndex].CellPrefabIndex);
        }

        /// <summary>
        /// Get prefab instance id with prefab index
        /// 获取指定预制件索引的单元格InstanceID
        /// </summary>
        /// <param name="prefabIndex"></param>
        /// <returns></returns>
        public int GetPrefabIndexTemplateInstanceID(int prefabIndex)
        {
            Debug.Assert(prefabIndex < SourcePrefabList.Count, $"预制件索引:{prefabIndex}超出最大预制件数量设置:{SourcePrefabList.Count},获取对应预制件模板InstanceID失败!");
            return mCellTemplateInstanceIDList[prefabIndex];
        }

        /// <summary>
        /// Set container celldatas by pass data
        /// 设置Container的cell数据通过列表数据
        /// </summary>
        /// <param name="prefabIndexList">预制件索引列表</param>
        /// <param name="cellSizeList">单元格大小列表(为空表示采用预制件默认大小)</param>
        /// <param name="scrollNormalizedPos">单元格初始滚动位置</param>
        public void SetCellDatas(List<int> prefabIndexList, List<Vector2> cellSizeList = null, Vector2? scrollNormalizedPos = null)
        {
            var cellDataList = CreateCellDatas(prefabIndexList, cellSizeList);
            SetCellDatas(cellDataList, scrollNormalizedPos);
        }

        /// <summary>
        /// Set container celldatas by pass cell count
        /// 设置Container的cell数据通过单元格数量
        /// </summary>
        /// <param name="cellCount">单元格数量</param>
        /// <param name="scrollNormalizedPos">单元格初始滚动位置</param>
        public void SetCellCount(int cellCount, Vector2? scrollNormalizedPos = null)
        {
            var cellDataList = CreateCellDatas(cellCount);
            SetCellDatas(cellDataList, scrollNormalizedPos);
        }

        /// <summary>
        /// Set container celldatas by pass celldata list
        /// 设置Container的cell数据
        /// </summary>
        /// <param name="cellDatas">所有的Cell数据</param>
        /// <param name="scrollNormalizedPos">单元格初始滚动位置</param>
        public void SetCellDatas(List<CellData> cellDatas, Vector2? scrollNormalizedPos = null)
        {
            // 为了支持比容器先进入Start执行数据设置的情况
            if (mIsCorrectDataComplete == false)
            {
                TryCorrectData();
            }
            if (mIsCorrectDataComplete)
            {
                // 记录前一次的容器大小和滚动比例
                var preContentSize = RectContentTrasform.sizeDelta;
                var preScrollNormalizedPos = ScrollRect.normalizedPosition;
                // 提前获取最新单元格数据的容器大小
                var newContentSize = GetContentSizeByDatas(cellDatas);

                ClearCellDatas();
                mCellDatas = cellDatas;

                var finalScrollNormalizedPos = scrollNormalizedPos;
                if (scrollNormalizedPos == null)
                {
                    // 比较前后两次容器大小来计算是否保持之前的相对比例滚动位置
                    if (ShouldKeepAbsoluteScrollPos(preContentSize, newContentSize))
                    {
                        finalScrollNormalizedPos = CalculateNewNormalizedPos(preScrollNormalizedPos, preContentSize, newContentSize);
                    }
                    else
                    {
                        finalScrollNormalizedPos = preScrollNormalizedPos;
                    }
                }

                UpdateContainerData(finalScrollNormalizedPos);
                UpdateMaxCorrectToIndex();
                // 如果开启了矫正强制矫正一次，确保初始化时的位置是正确的
                CheckCellPosCorrect(mCurrentScrollDir, true, false);
            }
            else
            {
                Debug.LogError($"组件:{this.gameObject.name}未完成矫正数据，不允许设置数据，请在Start里执行初始化流程!");
            }
            Display();
        }

        /// <summary>
        /// Decide whether keep absolute content anchored position when rebuilding cells.
        /// 当容器内容可滚动距离变大或保持不变时，保持绝对滚动位置；
        /// 当可滚动距离变小时，走比例位置还原。
        /// </summary>
        protected virtual bool ShouldKeepAbsoluteScrollPos(Vector2 preContentSize, Vector2 newContentSize)
        {
            var viewportSize = mRootRectContentTrasform.rect.size;
            var preScrollableX = Mathf.Max(0f, preContentSize.x - viewportSize.x);
            var preScrollableY = Mathf.Max(0f, preContentSize.y - viewportSize.y);
            var newScrollableX = Mathf.Max(0f, newContentSize.x - viewportSize.x);
            var newScrollableY = Mathf.Max(0f, newContentSize.y - viewportSize.y);

            if (ScrollRect.horizontal && !ScrollRect.vertical)
            {
                return newScrollableX >= preScrollableX;
            }

            if (ScrollRect.vertical && !ScrollRect.horizontal)
            {
                return newScrollableY >= preScrollableY;
            }

            // Fallback for unexpected mixed-axis scroll mode.
            return newScrollableX >= preScrollableX && newScrollableY >= preScrollableY;
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
        public Vector2 ClearCellDatas()
        {
            var finalScrollNormalizedPos = ScrollRect.normalizedPosition;
            if (mCellDatas != null)
            {
                // 调整容器数据时，强行停止滚动相关操作，避免刷新或进池出池等带来的过多问题
                ForceScrollStop();
                // 这里专门返回清除单元格后的滚动位置，方便上层记录后下次快速还原位置
                // 因为嵌套单元格可能在滚动过程中就进池了，所以进池前必须强制停止并矫正到正确位置
                finalScrollNormalizedPos = ScrollRect.normalizedPosition;
                // 清理之前的滚动数据
                for (int i = 0; i < mCellDatas.Count; i++)
                {
                    OnCellClear(i);
                }
                mCellDatas = null;
                // 还原初始的单元格容器位置数据
                UpdateContainerData();
            }
            mMaxCorrectToCellIndex = -1;
            return finalScrollNormalizedPos;
        }

        /// <summary>
        /// Get total number of cells
        /// 获取总的单元格数量
        /// </summary>
        /// <returns></returns>
        public int GetCellTotalCount()
        {
            return mCellDatas != null ? mCellDatas.Count : 0;
        }

        /// <summary>
        /// Get current scroll position(0 -- 1)
        /// 获取滚动进度
        /// </summary>
        /// <returns></returns>
        public Vector2 GetScrollNormalizedPos()
        {
            return ScrollRect != null ? ScrollRect.normalizedPosition : Vector2.zero;
        }

        /// <summary>
        /// Get cell data with index
        /// 获取指定索引位置的单元格
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public CellData GetCellData(int index)
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
        /// Add with index
        /// 指定位子添加单元格数据
        /// </summary>
        /// <param name="index">单元格添加位置索引</param>
        /// <param name="prefabIndex">模板索引</param>
        /// <param name="size">单元格大小</param>
        /// <returns></returns>
        public bool AddCellWithIndex(int index = 0, int prefabIndex = 0, Vector2? size = null)
        {
            var cellData = CreateCellData(prefabIndex, size);
            return AddCellDataWithIndex(cellData, index);
        }

        /// <summary>
        /// Add celldata with index
        /// 指定位子添加单元格数据
        /// </summary>
        /// <param name="cellData">需要添加的单元格</param>
        /// <param name="index">单元格添加位置索引</param>
        /// <returns></returns>
        public bool AddCellDataWithIndex(CellData cellData, int index = 0)
        {
            Debug.Assert(cellData != null, "不允许添加空的单元格数据!");
            var maxIndex = ((mCellDatas != null) ? mCellDatas.Count : 0);
            if (index >= 0 && index <= maxIndex)
            {
                ForceScrollStop();
                if (mCellDatas == null)
                {
                    mCellDatas = new List<CellData>();
                }
                mCellDatas.Insert(index, cellData);
                ForceUpdateDisplay(true);
                return true;
            }
            else
            {
                Debug.LogError($"无效的索引位置:{index},有效范围0-{maxIndex},添加单元格失败!");
                return false;
            }
        }

        /// <summary>
        /// Add celldata list with index
        /// 指定位子添加单元格数据列表
        /// </summary>
        /// <param name="cellDataList">需要添加的单元格信息列表</param>
        /// <param name="index">单元格添加位置索引</param>
        /// <returns></returns>
        public bool AddCellDatas(List<CellData> cellDataList, int index = 0)
        {
            Debug.Assert(cellDataList != null, "不允许添加空的单元格数据列表!");
            var maxIndex = ((mCellDatas != null) ? mCellDatas.Count : 0);
            if (index >= 0 && index <= maxIndex)
            {
                ForceScrollStop();
                if (mCellDatas == null)
                {
                    mCellDatas = new List<CellData>();
                }
                mCellDatas.InsertRange(index, cellDataList);
                ForceUpdateDisplay(true);
                return true;
            }
            else
            {
                Debug.LogError($"无效的索引位置:{index},有效范围0-{maxIndex},添加单元格列表失败!");
                return false;
            }
        }

        /// <summary>
        /// Add celldata to beginning
        /// 添加单元格数据到头部
        /// </summary>
        /// <param name="cellData">需要添加到头部的单元格</param>
        /// <returns></returns>
        public bool AddCellDataToStart(CellData cellData)
        {
            return AddCellDataWithIndex(cellData, 0);
        }

        /// <summary>
        /// Add celldata to end
        /// 添加单元格数据到尾部
        /// </summary>
        /// <param name="cellData">需要添加到尾部的单元格</param>
        /// <returns></returns>
        public bool AddCellDataToEnd(CellData cellData)
        {
            var maxIndex = ((mCellDatas != null) ? mCellDatas.Count : 0);
            return AddCellDataWithIndex(cellData, maxIndex);
        }

        /// <summary>
        /// Delete celldata with index
        /// 删除指定位子单元格
        /// </summary>
        /// <param name="index">单元格移除位置索引</param>
        /// <returns></returns>
        public bool RemoveCellIndex(int index = 0)
        {
            var maxIndex = ((mCellDatas != null) ? mCellDatas.Count : 0);
            if (mCellDatas != null)
            {
                if (index >= 0 && index < maxIndex)
                {
                    ForceScrollStop();
                    OnCellClear(index);
                    mCellDatas.RemoveAt(index);
                    ForceUpdateDisplay();
                    return true;
                }
                else
                {
                    Debug.LogError($"无效的索引位置:{index},有效范围0-{maxIndex - 1},删除单元格失败!");
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
        public Vector2 GetSize()
        {
            return RectContentTrasform.sizeDelta;
        }

        /// <summary>
        /// Force update display
        /// 强制容器刷新显示(包含数据更新和显示刷新，显示更新仅当已经触发显示的情况下)
        /// </summary>
        /// <returns></returns>
        public void ForceUpdateDisplay(bool keepRectcontentPos = false)
        {
            // 默认保留原始滚动位置的刷新方式
            UpdateContainerData(ScrollRect.normalizedPosition, keepRectcontentPos);
            Display(true, true);
            ForceScrollStop();
        }

        /// <summary>
        /// Refresh all celld display
        /// 刷新显示所有Cell(用于只触发刷新逻辑 e.g. BaseCell:OnShow())
        /// </summary>
        public void RefreshDisplay()
        {
            if (mCellDatas != null)
            {
                for (int i = 0; i < mCellDatas.Count; i++)
                {
                    OnCellDisplay(i);
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
            if (ShouldBubblehEvent(eventData))
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
                DoBeginDrag(eventData);
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
                EventBubbleContainerParent.UpdateScrollDir(eventData);

                mEventBubbleScrollParent.OnDrag(eventData);
            }
            else
            {
                //Debug.Log($"容器:{this.gameObject.name}拖拽!");
                DoDrag(eventData);
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
                DoEndDrag(eventData);
            }
        }

        /// <summary>
        /// Display all celldatas
        /// 显示更新所有Cell
        /// </summary>
        /// <param name="forceRefreshCellsize">是否强制刷新单元格大小</param>
        /// <param name="forceRefreshShow">是否强制刷新单元格显示</param>
        protected abstract void Display(bool forceRefreshCellsize = false, bool forceRefreshShow = false);

        /// <summary>
        /// Update scrollable status
        /// 更新可滚动状态
        /// </summary>
        public abstract void UpdateScrollable();
        
        /// <summary>
        /// Execute begin drag
        /// 执行开始拖拽
        /// </summary>
        /// <param name="eventData"></param>
        protected void DoBeginDrag(PointerEventData eventData)
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
        protected void DoDrag(PointerEventData eventData)
        {
            UpdateScrollDir(eventData);
        }

        /// <summary>
        /// Execute end drag
        /// 执行结束拖拽
        /// </summary>
        /// <param name="eventData"></param>
        protected void DoEndDrag(PointerEventData eventData)
        {
            //Debug.Log($"单元格容器:{gameObject.name}结束拖拽!");
            mIsEndDrag = true;
            // 避免结束拖拽时没有速度导致未触发onScrollChanged
            OnScrollChanged(Vector2.zero);
        }

        /// <summary>
        /// Is bubbling event up to parent
        /// 是否应该转发本次事件传递至父级
        /// </summary>
        protected bool ShouldBubblehEvent(PointerEventData eventData)
        {
            if (mEventBubbleScrollParent != null)
            {
                if (IsAllowedScroll == true)
                {
                    var delta = eventData.delta;
                    var moveDir = DetermineMoveDirection(delta.x, delta.y, 0.6f);
                    switch (mAvalibleDragDirection)
                    {
                        case EDragDirection.Vertical:
                            return moveDir == MoveDirection.Left || moveDir == MoveDirection.Right;
                        case EDragDirection.Horizontal:
                            return moveDir == MoveDirection.Up || moveDir == MoveDirection.Down;
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
        protected MoveDirection DetermineMoveDirection(float x, float y, float deadZone)
        {
            MoveDirection moveDir = MoveDirection.None;
            if (new Vector2(x, y).sqrMagnitude > deadZone * deadZone)
            {
                if (Mathf.Abs(x) > Mathf.Abs(y))
                {
                    moveDir = x > 0 ? MoveDirection.Right : MoveDirection.Left;
                }
                else
                {
                    moveDir = y > 0 ? MoveDirection.Up : MoveDirection.Down;
                }
            }
            return moveDir;
        }

        /// <summary>
        /// Force stop scrolling
        /// 强制停止滚动移动
        /// </summary>
        protected virtual void ForceScrollStop()
        {
            // 强制停止移动，确保单元格矫正和滚动相关逻辑正确完成
            ScrollRect.StopMovement();
            CheckCellPosCorrect(mCurrentScrollDir, true, false);
            mCellMoveToTweener?.Kill(true);
        }

        /// <summary>
        /// Check whether trigger correct cell behaviour
        /// 检查单元格位置矫正(子类重写去支持自定义矫正)
        /// </summary>
        /// <param name="scrollDir"></param>
        /// <param name="igorePosCheckThredhold">是否忽略单元格矫正速度判定</param>
        /// <param name="requireScrollDir">是否要求有效滚动方向才矫正</param>
        /// <returns></returns>
        protected virtual bool CheckCellPosCorrect(EScrollDir scrollDir, bool igorePosCheckThredhold = false, bool requireScrollDir = true)
        {
            return false;
        }

        /// <summary>
        /// 获取指定单元格数据列表的容器大小
        /// </summary>
        /// <param name="cellDatas"></param>
        /// <returns></returns>
        protected abstract Vector2 GetContentSizeByDatas(List<CellData> cellDatas = null);

        /// <summary>
        /// Calculate normalized position for keeping absolute scroll offset when content size changed.
        /// 内容尺寸变化时，为保持绝对滚动位置而计算新的滚动归一化位置。
        /// </summary>
        protected abstract Vector2 CalculateNewNormalizedPos(Vector2 preScrollNormalizedPos, Vector2 preContentSize, Vector2 newContentSize);

        /// <summary>
        /// Update container relative datas
        /// 更新容器数据
        /// </summary>
        /// <param name="scrollNormalizedPos">单元格初始滚动位置</param>
        /// <param name="keepRectcontentPos">是否保持rect content的相对位置(优先于scrollnormalizaedposition)</param>
        protected abstract void UpdateContainerData(Vector2? scrollNormalizedPos = null, bool keepRectcontentPos = false);

        /// <summary>
        /// Update max correct cell index
        /// 更新最大矫正到单元格索引值
        /// </summary>
        protected abstract void UpdateMaxCorrectToIndex();

        /// <summary>
        /// Get specific cell index center position offset
        /// 获取指定单元格离中心点位置的偏移
        /// </summary>
        /// <param name="currentScrollAbsPos"></param>
        /// <param name="cellIndex"></param>
        /// <returns></returns>
        protected abstract float GetCellCenterPosOffset(float currentScrollAbsPos, int cellIndex);

        /// <summary>
        /// Get Current Scroll Position
        /// 获取当前滚动到的位置
        /// </summary>
        /// <returns></returns>
        protected abstract float GetCurrentScrollPos();

        /// <summary>
        /// Update scroll direction
        /// 更新滚动方向数据
        /// </summary>
        /// <param name="eventData"></param>
        public abstract void UpdateScrollDir(PointerEventData eventData);

        /// <summary>
        /// On scrolling
        /// 响应滚动
        /// </summary>
        /// <param name="scrollPos"></param>
        protected abstract void OnScrollChanged(Vector2 scrollPos);

        /// <summary>
        /// On pointer exit
        /// 响应点击退出
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerExit(PointerEventData eventData)
        {
            // 避免手动强制停止滚动无法矫正问题
            CheckCellPosCorrect(mCurrentScrollDir);
        }

        /// <summary>
        /// On cell display
        /// 单元格显示判定
        /// </summary>
        /// <param name="index"></param>
        /// <param name="forceRefreshCellSize">是否强制刷新单元格大小</param>
        /// <param name="forceRefreshShow">是否强制刷新单元格显示</param>
        protected virtual void OnCellDisplay(int index, bool forceRefreshCellSize = false, bool forceRefreshShow = false)
        {
            if (!mCellDatas[index].IsVisible())
            {
                if (mCellDatas[index].CellGO != null)
                {
                    OnCellHide(index);
                }
            }
            else
            {
                if (forceRefreshCellSize && mCellDatas[index].CellGO)
                {
                    mCellDatas[index].UpdateCellSizeAndPosition();
                }
                if (!mCellDatas[index].CellGO)
                {
                    OnCellInit(index);
                    OnCellShow(index);
                }
                else
                {
                    if(forceRefreshShow)
                    {
                        OnCellShow(index);
                    }
                }
                var currentScrollPos = GetCurrentScrollPos();
                //统一换算成正的偏移位置，方便统一正向和逆向的滚动计算
                currentScrollPos = Mathf.Abs(currentScrollPos);
                var cellCenterOffsetPos = GetCellCenterPosOffset(currentScrollPos, index);
                OnCellVisibleScroll(index, CurrentScrollIndexValue, cellCenterOffsetPos);
            }
        }

        /// <summary>
        /// On Cell Initialization
        /// 响应单元格第一次初始化
        /// </summary>
        /// <param name="index"></param>
        protected virtual void OnCellInit(int index)
        {
            if (mCellDatas[index].CellGO == null)
            {
                var cellPrefabTemplate = GetPrefabIndexTemplate(mCellDatas[index].CellPrefabIndex);
                var cellGo = CellGoPool != null ? CellGoPool.Pop(cellPrefabTemplate) : GameObject.Instantiate(cellPrefabTemplate);
                cellGo.transform.SetParent(RectContentTrasform, false);
                mCellDatas[index].Init(cellGo);
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
        protected virtual void OnCellShow(int index)
        {
            // OnShow逻辑
            mOnShowDelegate?.Invoke(index, mCellDatas[index].CellGO);
        }

        /// <summary>
        /// On cell hide
        /// 响应单元格隐藏
        /// </summary>
        /// <param name="index"></param>
        protected virtual void OnCellHide(int index)
        {
            if (CellGoPool != null)
            {
                var cellPrefabInstanceId = GetPrefabIndexTemplateInstanceID(mCellDatas[index].CellPrefabIndex);
                CellGoPool.Push(cellPrefabInstanceId, mCellDatas[index].CellGO);
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
            mCellDatas[index].Init(null);
        }

        /// <summary>
        /// On cell visible scroll
        /// 响应单元格可见滚动
        /// </summary>
        /// <param name="index">单元格索引</param>
        /// <param name="scrollIndexValue">当前滚动到的单元格索引值</param>
        /// <param name="centerOffsetPos">当前单元格离中心点位置偏移</param>
        protected virtual void OnCellVisibleScroll(int index, float scrollIndexValue, float centerOffsetPos)
        {
            // onVisibleScroll单元格可见滚动逻辑
            mOnVisibleScrollDelegate?.Invoke(index, mCellDatas[index].CellGO, scrollIndexValue, centerOffsetPos);
        }

        /// <summary>
        /// On cell clear
        /// 响应单元格清理
        /// </summary>
        /// <param name="index"></param>
        protected virtual void OnCellClear(int index)
        {
            if (mCellDatas[index].IsVisible())
            {
                if (mCellDatas[index].CellGO != null)
                {
                    OnCellHide(index);
                }
            }
            mCellDatas[index].Clear();
        }

        #region Create Cell Datas API(创建单元格数据接口)
        /// <summary>
        /// Create celldata
        /// 创建CellData方法
        /// </summary>
        /// <param name="prefabIndex">预制件索引</param>
        /// <param name="size">单元格大小</param>
        /// <returns></returns>
        public CellData CreateCellData(int prefabIndex = 0, Vector2? size = null)
        {
            if (prefabIndex >= SourcePrefabList.Count)
            {
                Debug.LogError($"容器:{gameObject.name}单元格数据总数量:{SourcePrefabList.Count},预制件索引:{prefabIndex}超出范围!");
                return null;
            }
            var cell = ObjectPool.Singleton.Pop<CellData>();
            cell.SetData(prefabIndex, this, size);
            return cell;
        }

        /// <summary>
        /// Create celldata list by pass cell info
        /// 创建CellData方法列表(优化CreateCellData调用多次问题)
        /// </summary>
        /// <param name="prefabIndexList">预制件索引列表</param>
        /// <param name="cellSizeList">单元格大小列表(为空表示采用预制件默认大小)</param>
        /// <returns></returns>
        public List<CellData> CreateCellDatas(List<int> prefabIndexList, List<Vector2> cellSizeList = null)
        {
            if(cellSizeList != null)
            {
                Debug.Assert(prefabIndexList.Count == cellSizeList.Count, $"如果单元格大小不为空，那么单元格大小数据列表必须和预制件索引列表长度一致!");
            }
            var cellDataList = new List<CellData>();
            for(int i = 0, length = prefabIndexList.Count; i < length; i++)
            {
                var prefabIndex = prefabIndexList[i];
                if (prefabIndex >= SourcePrefabList.Count)
                {
                    Debug.LogError($"容器:{gameObject.name}单元格数据总数量:{SourcePrefabList.Count},预制件索引:{prefabIndex}超出范围!");
                    return null;
                }
                var cell = ObjectPool.Singleton.Pop<CellData>();
                Vector2? sizeValue = null;
                if(cellSizeList != null)
                {
                    sizeValue = cellSizeList[i];
                }
                cell.SetData(prefabIndex, this, sizeValue);
                cellDataList.Add(cell);
            }
            return cellDataList;
        }

        /// <summary>
        /// Create celldata list by pass cell total number
        /// 创建CellData方法列表(优化CreateCellData调用多次问题)
        /// </summary>
        /// <param name="cellCount">单元格数量(默认全部用第一个预制件)</param>
        /// <param name="cellSizeList">单元格大小列表(为空表示采用预制件默认大小)</param>
        /// <returns></returns>
        public List<CellData> CreateCellDatas(int cellCount, List<Vector2> cellSizeList = null)
        {
            if (cellSizeList != null)
            {
                Debug.Assert(cellCount == cellSizeList.Count, $"如果单元格大小不为空，那么单元格大小数据列表必须和预制件索引列表长度一致!");
            }
            if (SourcePrefabList.Count == 0)
            {
                Debug.LogError($"容器:{gameObject.name}预制件总数量:{SourcePrefabList.Count}不能为0!");
                return null;
            }
            var cellDataList = new List<CellData>();
            for (int i = 0; i < cellCount; i++)
            {
                var cell = ObjectPool.Singleton.Pop<CellData>();
                Vector2? sizeValue = null;
                if (cellSizeList != null)
                {
                    sizeValue = cellSizeList[i];
                }
                cell.SetData(0, this, sizeValue);
                cellDataList.Add(cell);
            }
            return cellDataList;
        }
        #endregion
    }
}
