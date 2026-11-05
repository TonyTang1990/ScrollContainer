/*
 * Description:             KeepPosRefreshScene.cs
 * Author:                  TANGHUAN
 * Create Date:             2026/04/11
 */

using System.Collections.Generic;
using TH.Modules.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static TH.Modules.UI.BaseScrollContainer;

/// <summary>
/// KeepPosRefreshScene Script
/// </summary>
public class KeepPosRefreshScene : MonoBehaviour 
{
    /// <summary>
    /// 返回主菜单按钮
    /// </summary>
    [Header("返回主菜单按钮")]
    public Button BtnBackToMainMenu;

    /// <summary>
    /// 操作的单元格数量输入文本
    /// </summary>
    [Header("操作的单元格数量输入文本")]
    public InputField IFCellNum;

    /// <summary>
    /// 刷新单元格数量按钮
    /// </summary>
    [Header("刷新单元格数量按钮")]
    public Button BtnRefreshCellNum;

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
    /// 保持位置刷新数量单元格容器
    /// </summary>
    [Header("保持位置刷新数量单元格容器")]
    public VerticalScrollContainer KeepPosRefreshContainer;

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
    /// 单页单元格数量
    /// 用于测试滚动自动检测是否跨指定页面数量单元格
    /// </summary>
    private const int CellPageNum = 20;

    /// <summary>
    /// 滚动跨页偏移的单元格数量
    /// 用于在计算提前达到跨页数量时的单元格偏移数量
    /// </summary>
    private const int ChangePageOffsetCellNum = 10;

    /// <summary>
    /// Cell Num In Input Field
    /// </summary>
    private int mInputFiledCellNum;

    /// <summary>
    /// 是否显示自动滚动到顶部和底部按钮
    /// </summary>
    private bool mIsShowAutoScrollTopAndBottom;

    /// <summary>
    /// 置顶和置底按钮显示时间累计
    /// </summary>
    private float mScrollTopAndBottomShowTimePassed;

    /// <summary>
    /// 已经到达的滚动页数集合
    /// 用于检测第一次达到某个页数请求灵界点
    /// 每次重置数据后会清空，用于重新支持跨页判定
    /// </summary>
    private HashSet<int> mReachPageSet = new HashSet<int>();

    /// <summary>
    /// 是否禁用跨页检测
    /// 用于在置顶和置底操作时不做跨页检测
    /// </summary>
    private bool mIsDisableReachPage;

    /// <summary>
    /// 禁用跨页检测的时间累计
    /// </summary>
    private float mDisableReachPageTimePassed;

    void Start()
    {
        mReachPageSet.Clear();
        SetDisableReachPage(false);
        SetScrollTopAndBottomShow(false);
        mInputFiledCellNum = 0;
        IFCellNum.text = mInputFiledCellNum.ToString();
        BtnBackToMainMenu.onClick.AddListener(OnBtnBackToMainMenu);
        BtnRefreshCellNum.onClick.AddListener(OnBtnRefreshCellNum);
        BtnScrollToTop.onClick.AddListener(OnBtnScrollToTop);
        BtnScrollToBottom.onClick.AddListener(OnBtnScrollToBottom);
        KeepPosRefreshContainer.BindContainerCallBack(OnCellShow, null, null, null, OnBeginDrag, OnEndDrag, OnScroll);
        KeepPosRefreshContainer.SetCellCount(100);
    }

    void Update()
    {
        var deltaTime = Time.deltaTime;
        CheckAutoEnableReachPage(deltaTime);
        CheckScrollTopAndBottomAutoHide(deltaTime);
    }

    /// <summary>
    /// 检查是否需要自动启用跨页检测
    /// </summary>
    /// <param name="deltaTime"></param>
    private void CheckAutoEnableReachPage(float deltaTime)
    {
        if(!mIsDisableReachPage)
        {
            return;
        }
        mDisableReachPageTimePassed += deltaTime;
        if(mDisableReachPageTimePassed >= AutoEnableReachPageSeconds)
        {
            SetDisableReachPage(false);
        }
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
    /// 设置是否禁用跨页检测
    /// </summary>
    /// <param name="isDisable"></param>
    private void SetDisableReachPage(bool isDisable)
    {
        mIsDisableReachPage = isDisable;
        mDisableReachPageTimePassed = 0f;
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
    /// Parse Cell Num in Input field 
    /// 解析单元格参数输入
    /// </summary>
    private bool ParseCellNumInput()
    {
        if(int.TryParse(IFCellNum.text, out mInputFiledCellNum))
        {
            Debug.Log($"New Input Cell Num:{mInputFiledCellNum}");
            return true;
        }
        else
        {
            Debug.LogError($"Invalide cell Num:{IFCellNum.text}");
            return false;
        }
    }

    /// <summary>
    /// 返回主界面
    /// </summary>
    private void OnBtnBackToMainMenu()
    {
        SceneManager.LoadScene(SceneNameDef.LauncherScene);
    }

    /// <summary>
    /// Refresh Cell With Cell Num
    /// 刷新单元格数量点击
    /// </summary>
    private void OnBtnRefreshCellNum()
    {
        if(ParseCellNumInput())
        {
            KeepPosRefreshContainer.SetCellCount(mInputFiledCellNum);
            mReachPageSet.Clear();
        }
    }

    /// <summary>
    /// 滚动到顶部
    /// </summary>
    private void OnBtnScrollToTop()
    {
        SetDisableReachPage(true);
        KeepPosRefreshContainer.MoveToTop();
    }

    /// <summary>
    /// 滚动到底部
    /// </summary>
    private void OnBtnScrollToBottom()
    {
        SetDisableReachPage(true);
        KeepPosRefreshContainer.MoveToBottom();
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
    /// 拖拽开始回调<当前滚动索引值(带小数)>
    /// </summary>
    /// <param name="currentScrollIndex"></param>
    private void OnBeginDrag(float currentScrollIndex)
    {
        Debug.Log($"OnBeginDrag: {currentScrollIndex}");
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
        if(mIsDisableReachPage)
        {
            return;
        }
        var currentScrollPage = Mathf.CeilToInt(currentScrollIndex / CellPageNum);
        if(mReachPageSet.Contains(currentScrollPage))
        {
            return;
        }
        var pageOffsetScrollIndex = currentScrollPage * CellPageNum;
        var isReachPageOffsetScrollIndex = false;
        var nextScrollPage = currentScrollPage;
        if(scrollDir == EScrollDir.ScrollUp)
        {
            pageOffsetScrollIndex -= ChangePageOffsetCellNum;
            if(currentScrollIndex >= pageOffsetScrollIndex)
            {
                isReachPageOffsetScrollIndex = true;
                nextScrollPage = currentScrollPage + 1;
            }
        }
        else if(scrollDir == EScrollDir.ScrollDown)
        {
            pageOffsetScrollIndex -= ChangePageOffsetCellNum;
            if(currentScrollIndex <= pageOffsetScrollIndex)
            {
                isReachPageOffsetScrollIndex = true;
                nextScrollPage = currentScrollPage - 1;
            }
        }
        if(isReachPageOffsetScrollIndex)
        {
            var cellTotalNum = KeepPosRefreshContainer.GetCellTotalCount();
            var totalPageNum = Mathf.CeilToInt((float)cellTotalNum / CellPageNum);
            nextScrollPage = Mathf.Clamp(nextScrollPage, 1, totalPageNum);
            if(currentScrollPage == nextScrollPage)
            {
                return;
            }
            // 可以做动态分页请求的逻辑
            Debug.Log($"滚动页数变化:{currentScrollPage}->{nextScrollPage}，currentScrollIndex:{currentScrollIndex}, ScrollDir: {scrollDir}！");
            mReachPageSet.Add(currentScrollPage);
        }
    }

    /// <summary>
    /// 拖拽结束回调<当前滚动索引值(带小数)>
    /// </summary>
    /// <param name="currentScrollIndex"></param>
    private void OnEndDrag(float currentScrollIndex)
    {
        Debug.Log($"OnEndDrag: {currentScrollIndex}");
    }
}