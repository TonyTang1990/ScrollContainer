/*
 * Description:             AutoLoadMoreScene.cs
 * Author:                  TANGHUAN
 * Create Date:             2026/04/11
 */

using System.Collections.Generic;
using TH.Modules.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static TH.Modules.UI.BaseScrollContainer;
using static TH.Modules.UI.VerticalScrollContainer;

/// <summary>
/// AutoLoadMoreScene Script
/// </summary>
public class AutoLoadMoreScene : MonoBehaviour 
{
    /// <summary>
    /// 返回主菜单按钮
    /// </summary>
    [Header("返回主菜单按钮")]
    public Button BtnBackToMainMenu;

    /// <summary>
    /// 滚动到顶部
    /// </summary>
    [Header("滚动到顶部")]
    public Button BtnScrollToTop;

    /// <summary>
    /// 滚动到底部按钮
    /// </summary>
    [Header("滚动到底部按钮")]
    public Button BtnScrollToBottom;

    /// <summary>
    /// 自动添加单元格格容器
    /// </summary>
    [Header("自动添加单元格格容器")]
    public VerticalScrollContainer AutoLoadMoreContainer;

    /// <summary>
    /// 当前单元格数量显示文本
    /// </summary>
    [Header("当前单元格数量显示文本")]
    public Text CurrentCellNumTxt;

    /// <summary>
    /// 置顶和置底显示自动隐藏的时间间隔
    /// </summary>
    private const float ScrollTopAndBottomAutoHideSeconds = 3f;

    /// <summary>
    /// 自动重新启用跨页检测的时间间隔
    /// 考虑到OnMoveTo回调可能会被用户操作打断，回调不可靠，
    /// 所以滚动到底部和顶部的操作禁用跨页检测用间隔固定时长自动回复
    /// </summary>
    private const float AutoEnableReachPageSeconds = 1f;

    /// <summary>
    /// 自动加载更多的单元格临界数量(即离最后一个单元格的数量差)
    /// </summary>
    private const int AutoLoadMoreThresholdNum = 10;

    /// <summary>
    /// 自动添加的单元格数量
    /// </summary>
    private const int AutoAddedCellNum = 10;

    /// <summary>
    /// 最大单元格数量
    /// </summary>
    private const int MaxCellNum = 100;

    /// <summary>
    /// 是否显示自动滚动到顶部和底部按钮
    /// </summary>
    private bool mIsShowAutoScrollTopAndBottom;

    /// <summary>
    /// 置顶和置底按钮显示时间累计
    /// </summary>
    private float mScrollTopAndBottomShowTimePassed;

    /// <summary>
    /// 当前单元格数量
    /// </summary>
    private int mCellNum;

    /// <summary>
    /// 是否正在自动滚动
    /// </summary>
    private bool mIsAutoScrolling;

    /// <summary>
    /// 往最后一个单元格滚动的方向
    /// </summary>
    private EScrollDir mToLastCellScrollDir;

    void Start()
    {
        mCellNum = 20;
        mToLastCellScrollDir = EScrollDir.ScrollUp;
        if(AutoLoadMoreContainer.LayoutDirection == EVerticalLayoutDirection.BottomToTop)
        {
            mToLastCellScrollDir = EScrollDir.ScrollDown;
        }
        SetScrollTopAndBottomShow(false);
        BtnBackToMainMenu.onClick.AddListener(OnBtnBackToMainMenu);
        BtnScrollToTop.onClick.AddListener(OnBtnScrollToTop);
        BtnScrollToBottom.onClick.AddListener(OnBtnScrollToBottom);
        AutoLoadMoreContainer.BindContainerCallBack(OnCellShow, null, null, OnMoveTo, OnBeginDrag, OnEndDrag, OnScroll);
        AutoLoadMoreContainer.SetCellCount(mCellNum);
        RefreshCurrentCellNumView();
    }

    /// <summary>
    /// 刷新当前单元格数量显示文本
    /// </summary>
    private void RefreshCurrentCellNumView()
    {
        if(CurrentCellNumTxt != null)
        {
            CurrentCellNumTxt.text = $"当前单元格数量: {mCellNum}";
        }
    }

    void Update()
    {
        var deltaTime = Time.deltaTime;
        CheckScrollTopAndBottomAutoHide(deltaTime);
    }

    /// <summary>
    /// 检查置顶和置底按钮自动隐藏
    /// </summary>
    /// <param name="deltaTime"></param>
    private void CheckScrollTopAndBottomAutoHide(float deltaTime)
    {
        if(!mIsShowAutoScrollTopAndBottom)
        {
            return;
        }
        mScrollTopAndBottomShowTimePassed += deltaTime;
        if(mScrollTopAndBottomShowTimePassed >= ScrollTopAndBottomAutoHideSeconds)
        {
            SetScrollTopAndBottomShow(false);
        }
    }

    /// <summary>
    /// 设置置顶和置底按钮显示状态
    /// </summary>
    /// <param name="isShow"></param>
    private void SetScrollTopAndBottomShow(bool isShow)
    {
        mIsShowAutoScrollTopAndBottom = isShow;
        BtnScrollToTop.gameObject.SetActive(mIsShowAutoScrollTopAndBottom);
        BtnScrollToBottom.gameObject.SetActive(mIsShowAutoScrollTopAndBottom);
        mScrollTopAndBottomShowTimePassed = 0f;
    }

    /// <summary>
    /// 返回主界面
    /// </summary>
    private void OnBtnBackToMainMenu()
    {
        SceneManager.LoadScene(SceneNameDef.LauncherScene);
    }

    /// <summary>
    /// 滚动到顶部
    /// </summary>
    private void OnBtnScrollToTop()
    {
        mIsAutoScrolling = true;
        if(AutoLoadMoreContainer.LayoutDirection == EVerticalLayoutDirection.TopToBottom)
        {
            AutoLoadMoreContainer.MoveToFirst();
        }
        else
        {
            AutoLoadMoreContainer.MoveToLast();
        }
    }

    /// <summary>
    /// 滚动到底部
    /// </summary>
    private void OnBtnScrollToBottom()
    {
        mIsAutoScrolling = true;
        if(AutoLoadMoreContainer.LayoutDirection == EVerticalLayoutDirection.TopToBottom)
        {
            AutoLoadMoreContainer.MoveToLast();
        }
        else
        {
            AutoLoadMoreContainer.MoveToFirst();
        }
    }

    /// <summary>
    /// 单元格显示回调
    /// </summary>
    /// <param name="cellIndex"></param>
    /// <param name="cellInstance"></param>
    private void OnCellShow(int cellIndex, GameObject cellInstance)
    {
        var showCellIndexCell = cellInstance.GetComponent<ShowCellIndexCell>();
        if (showCellIndexCell == null)
        {
            showCellIndexCell = cellInstance.AddComponent<ShowCellIndexCell>();
        }
        showCellIndexCell.Init(cellIndex);
    }

    /// <summary>
    /// 滚动到指定单元格完成回调
    /// </summary>
    /// <param name="currentScrollIndex"></param>
    /// <param name="cellInstance"></param>
    private void OnMoveTo(int currentScrollIndex, GameObject cellInstance)
    {
        Debug.Log($"OnMoveTo: {currentScrollIndex}");
        // 检测自动滚动完成后的自动加载更多效果
        mIsAutoScrolling = false;
        CheckAutoLoadMoreCell(currentScrollIndex, EScrollDir.None, true);
    }

    /// <summary>
    /// 拖拽开始回调<当前滚动索引值(带小数)>
    /// </summary>
    /// <param name="currentScrollIndex"></param>
    /// </summary>
    /// <param name="currentScrollIndex"></param>
    private void OnBeginDrag(float currentScrollIndex)
    {
        Debug.Log($"OnBeginDrag: {currentScrollIndex}");
        mIsAutoScrolling = false;
        SetScrollTopAndBottomShow(true);
    }

    /// <summary>
    /// 响应滚动<当前滚动索引值(带小数), 滚动方向>
    /// </summary>
    /// <param name="currentScrollIndex"></param>
    /// <param name="scrollDir"></param>
    private void OnScroll(float currentScrollIndex, EScrollDir scrollDir)
    {
        //Debug.Log($"OnScroll: {currentScrollIndex}, ScrollDir: {scrollDir}");
        CheckAutoLoadMoreCell(currentScrollIndex, scrollDir);
    }

    /// <summary>
    /// 检查是否需要自动加载更多单元格
    /// </summary>
    /// <param name="currentScrollIndex"></param>
    /// <param name="scrollDir"></param>
    /// <param name="isForceDetect">是否强制检测</param>
    private void CheckAutoLoadMoreCell(float currentScrollIndex, EScrollDir scrollDir = EScrollDir.None,
                                       bool isForceDetect = false)
    {
        // 避免自动滚动时触发加载更多导致单元格停止滑动问题
        if(mIsAutoScrolling)
        {
            return;
        }
        if(mCellNum >= MaxCellNum)
        {
            return;
        }
        if(!isForceDetect && scrollDir != mToLastCellScrollDir)
        {
            return;
        }
        var currentScrollCeilIndex = Mathf.CeilToInt(currentScrollIndex);
        currentScrollCeilIndex = Mathf.Min(currentScrollCeilIndex, mCellNum);
        var toLastCellOffsetNum = mCellNum - currentScrollCeilIndex;
        toLastCellOffsetNum = Mathf.Max(0, toLastCellOffsetNum);
        if(toLastCellOffsetNum >= AutoLoadMoreThresholdNum)
        {
            return;
        }
        mCellNum += AutoAddedCellNum;
        AutoLoadMoreContainer.SetCellCount(mCellNum, null, true);
        RefreshCurrentCellNumView();
        Debug.Log($"当前滚动Cell单元格Cell数量:{currentScrollCeilIndex}，toLastCellOffsetNum：{toLastCellOffsetNum}，自动增加单元格数量:{AutoAddedCellNum}，最新单元格数量：{mCellNum}！");
    }

    /// <summary>
    /// 拖拽结束回调<当前滚动索引值(带小数)>
    /// </summary>
    /// <param name="currentScrollIndex"></param>
    private void OnEndDrag(float currentScrollIndex)
    {
        Debug.Log($"OnEndDrag: {currentScrollIndex}");
        mIsAutoScrolling = false;
        // 强制拖拽结束检测一次，避免滚动到底部等操作无法触发自动加载更多单元格
        CheckAutoLoadMoreCell(currentScrollIndex, EScrollDir.None, true);
    }
}