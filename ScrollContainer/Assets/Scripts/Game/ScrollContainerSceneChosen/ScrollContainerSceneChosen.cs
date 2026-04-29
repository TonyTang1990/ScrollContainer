/*
 * Description:             ScrollContainerSceneChosen.cs
 * Author:                  TANGHUAN
 * Create Date:             2021/04/26
 */

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 滚动容器场景选择
/// </summary>
public class ScrollContainerSceneChosen : MonoBehaviour
{
    /// <summary>
    /// 从顶到底按钮
    /// </summary>
    [Header("从顶到底按钮")]
    public Button TopToBottom;

    /// <summary>
    /// 从底到顶按钮
    /// </summary>
    [Header("从底到顶按钮")]
    public Button BottomToTop;

    /// <summary>
    /// 从左到右按钮
    /// </summary>
    [Header("从左到右按钮")]
    public Button LeftToRight;

    /// <summary>
    /// 从右到左按钮
    /// </summary>
    [Header("从右到左按钮")]
    public Button RightToLeft;

    /// <summary>
    /// 网格滚动按钮
    /// </summary>
    [Header("网格滚动按钮")]
    public Button GridView;

    /// <summary>
    /// 聊天列表按钮
    /// </summary>
    [Header("聊天列表按钮")]
    public Button ChatMessageList;

    /// <summary>
    /// 动态修改大小按钮
    /// </summary>
    [Header("动态修改大小按钮")]
    public Button ChangeItemSize;

    /// <summary>
    /// 点击并加载更多按钮
    /// </summary>
    [Header("点击并加载更多按钮")]
    public Button ClickAndLoadMore;

    /// <summary>
    /// 横向画廊按钮
    /// </summary>
    [Header("横向画廊按钮")]
    public Button HorizontalGalleryDemo;

    /// <summary>
    /// 竖向画廊按钮
    /// </summary>
    [Header("竖向画廊按钮")]
    public Button VerticalGalleryDemo;

    /// <summary>
    /// 页面滚动按钮
    /// </summary>
    [Header("页面滚动按钮")]
    public Button PageViewDemo;

    /// <summary>
    /// 日期选择按钮
    /// </summary>
    [Header("日期选择按钮")]
    public Button SpinDatePicker;

    /// <summary>
    /// 选择滚动或删除按钮
    /// </summary>
    [Header("选择滚动或删除按钮")]
    public Button SelectAndDeleteOrMove;

    /// <summary>
    /// 保持位置刷新按钮
    /// </summary>
    [Header("保持位置刷新按钮")]
    public Button KeepPosRefresh;

    void Start ()
    {
        TopToBottom.onClick.AddListener(OnBtnTopToBottom);
        BottomToTop.onClick.AddListener(OnBtnBottomToTop);
        LeftToRight.onClick.AddListener(OnBtnLeftToRight);
        RightToLeft.onClick.AddListener(OnBtnRightToLeft);
        GridView.onClick.AddListener(OnBtnGridView);
        ChatMessageList.onClick.AddListener(OnBtnChatMessageList);
        ChangeItemSize.onClick.AddListener(OnBtnChangeItemSize);
        ClickAndLoadMore.onClick.AddListener(OnBtnClickAndLoadMore);
        HorizontalGalleryDemo.onClick.AddListener(OnBtnHorizontalGalleryDemo);
        VerticalGalleryDemo.onClick.AddListener(OnBtnVerticalGalleryDemo);
        PageViewDemo.onClick.AddListener(OnBtnPageViewDemo);
        SpinDatePicker.onClick.AddListener(OnBtnSpinDatePicker);
        SelectAndDeleteOrMove.onClick.AddListener(OnBtnSelectAndDeleteOrMove);
        KeepPosRefresh.onClick.AddListener(OnBtnKeepPosRefresh);
    }

    /// <summary>
    /// 从上往下按钮点击
    /// </summary>
    private void OnBtnTopToBottom()
    {
        SceneManager.LoadScene(SceneNameDef.TopToBottomScene);
    }

    /// <summary>
    /// 从下往上按钮点击
    /// </summary>
    private void OnBtnBottomToTop()
    {
        SceneManager.LoadScene(SceneNameDef.BottomToTopScene);
    }

    /// <summary>
    /// 从左往右按钮点击
    /// </summary>
    private void OnBtnLeftToRight()
    {
        SceneManager.LoadScene(SceneNameDef.LeftToRightScene);
    }

    /// <summary>
    /// 从右往左按钮点击
    /// </summary>
    private void OnBtnRightToLeft()
    {
        SceneManager.LoadScene(SceneNameDef.RightToLeftScene);
    }

    /// <summary>
    /// 网格按钮点击
    /// </summary>
    private void OnBtnGridView()
    {
        SceneManager.LoadScene(SceneNameDef.GridViewScene);
    }

    /// <summary>
    /// 聊天按钮点击
    /// </summary>
    private void OnBtnChatMessageList()
    {
        SceneManager.LoadScene(SceneNameDef.ChatMessageListScene);
    }

    /// <summary>
    /// 动态单元格大小按钮点击
    /// </summary>
    private void OnBtnChangeItemSize()
    {
        SceneManager.LoadScene(SceneNameDef.ChangeItemSizeScene);
    }

    /// <summary>
    /// 点击加载更多按钮点击
    /// </summary>
    private void OnBtnClickAndLoadMore()
    {
        SceneManager.LoadScene(SceneNameDef.ClickAndLoadMoreScene);
    }

    /// <summary>
    /// 横向画廊按钮点击
    /// </summary>
    private void OnBtnHorizontalGalleryDemo()
    {
        SceneManager.LoadScene(SceneNameDef.HorizontalGalleryDemoScene);
    }

    /// <summary>
    /// 竖向画廊按钮点击
    /// </summary>
    private void OnBtnVerticalGalleryDemo()
    {
        SceneManager.LoadScene(SceneNameDef.VerticalGalleryDemoScene);
    }

    /// <summary>
    /// 翻页按钮点击
    /// </summary>
    private void OnBtnPageViewDemo()
    {
        SceneManager.LoadScene(SceneNameDef.PageViewDemoScene);
    }

    /// <summary>
    /// 日期选择按钮点击
    /// </summary>
    private void OnBtnSpinDatePicker()
    {
        SceneManager.LoadScene(SceneNameDef.SpinDatePickerScene);
    }

    /// <summary>
    /// 选择滚动或删除按钮点击
    /// </summary>
    private void OnBtnSelectAndDeleteOrMove()
    {
        SceneManager.LoadScene(SceneNameDef.SelectAndDeleteOrMoveScene);
    }

    /// <summary>
    /// 保持位置刷新按钮点击
    /// </summary>
    private void OnBtnKeepPosRefresh()
    {
        SceneManager.LoadScene(SceneNameDef.KeepPosRefreshScene);
    }
}