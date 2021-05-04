/*
 * Description:             PageViewDemoScene.cs
 * Author:                  TANGHUAN
 * Create Date:             2021/04/27
 */

using System.Collections.Generic;
using TH.Modules.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// PageViewDemoScene Script
/// </summary>
public class PageViewDemoScene : MonoBehaviour
{
    /// <summary>
    /// BackToMainMenu
    /// </summary>
    [Header("返回主菜单按钮")]
    public Button BtnBackToMainMenu;

    /// <summary>
    /// PageViewContainer
    /// </summary>
    [Header("横向页面展示单元格容器")]
    public HorizontalScrollContainer PageViewContainer;

    /// <summary>
    /// Page Chosen Parent
    /// </summary>
    [Header("页面选择父节点")]
    public Transform PageChosenParent;

    /// <summary>
    /// Page Chosen Image Template
    /// </summary>
    [Header("页面选择模板图片")]
    public Image ImgPageChosenTempalte;

    /// <summary>
    /// Page Chosen Image List
    /// 页面选择提示图片列表
    /// </summary>
    private List<Image> mImgPageChosenList;

    /// <summary>
    /// Pre Chosen Page Index
    /// 之前选择页面索引
    /// </summary>
    private int mPreChosenPageIndex;

    /// <summary>
    /// Current Chosen Page Index
    /// 当前选择页面索引
    /// </summary>
    private int mCurrentChosenPageIndex;

    /// <summary>
    /// Total Page Number
    /// 总的页数
    /// </summary>
    private const int PageTotalNumber = 5;


    void Start()
    {
        mImgPageChosenList = new List<Image>();
        mPreChosenPageIndex = -1;
        mCurrentChosenPageIndex = 0;
        ImgPageChosenTempalte.gameObject.SetActive(true);
        for (int i = 0; i < PageTotalNumber; i++)
        {
            var imginstance = GameObject.Instantiate<Image>(ImgPageChosenTempalte, PageChosenParent);
            mImgPageChosenList.Add(imginstance);
            mImgPageChosenList[i].color = i == mCurrentChosenPageIndex ? Color.white : Color.gray;
        }
        ImgPageChosenTempalte.gameObject.SetActive(false);

        BtnBackToMainMenu.onClick.AddListener(onBtnBackToMainMenu);
        PageViewContainer.bindContainerCallBack(onCellShow, null, null, onCellMoveTo);
        PageViewContainer.setCellDatasByCellCount(PageTotalNumber);
    }

    /// <summary>
    /// 返回主界面
    /// </summary>
    private void onBtnBackToMainMenu()
    {
        SceneManager.LoadScene(SceneNameDef.LauncherScene);
    }

    /// <summary>
    /// 单元格显示回调
    /// </summary>
    /// <param name="cellindex"></param>
    /// <param name="cellinstance"></param>
    private void onCellShow(int cellindex, GameObject cellinstance)
    {
        var pageviewcell = cellinstance.GetComponent<PageViewCell>();
        if (pageviewcell == null)
        {
            pageviewcell = cellinstance.AddComponent<PageViewCell>();
        }
        pageviewcell.init(cellindex);
    }

    /// <summary>
    /// 单元格移动到指定单元格
    /// </summary>
    /// <param name="cellindex"></param>
    /// <param name="cellinstance"></param>
    private void onCellMoveTo(int cellindex, GameObject cellinstance)
    {
        if(mCurrentChosenPageIndex != cellindex)
        {
            mPreChosenPageIndex = mCurrentChosenPageIndex;
            mCurrentChosenPageIndex = cellindex;
            updatePageChosenView();
        }
    }

    /// <summary>
    /// Update Page Chosen View
    /// 更新页面选择显示
    /// </summary>
    private void updatePageChosenView()
    {
        if(mPreChosenPageIndex != -1 && mPreChosenPageIndex != mCurrentChosenPageIndex)
        {
            mImgPageChosenList[mPreChosenPageIndex].color = Color.gray;
            mImgPageChosenList[mCurrentChosenPageIndex].color = Color.white;
        }
        else
        {
            mImgPageChosenList[mCurrentChosenPageIndex].color = Color.white;
        }
    }
}