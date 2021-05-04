/*
 * Description:             ClickAndLoadMoreScene.cs
 * Author:                  TONYTANG
 * Create Date:             2021/05/02
 */

using System.Collections;
using System.Collections.Generic;
using TH.Modules.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// ClickAndLoadMoreScene Script
/// </summary>
public class ClickAndLoadMoreScene : MonoBehaviour 
{
    /// <summary>
    /// BackToMainMenu
    /// </summary>
    [Header("返回主菜单按钮")]
    public Button BtnBackToMainMenu;

    /// <summary>
    /// ClickAndLoadMoreContainer
    /// </summary>
    [Header("改变单元格大小单元格容器")]
    public VerticalScrollContainer ClickAndLoadMoreContainer;

    void Start()
    {
        BtnBackToMainMenu.onClick.AddListener(onBtnBackToMainMenu);
        ClickAndLoadMoreContainer.bindContainerCallBack(onCellShow);
        var prefabindexlist = new List<int>();
        for(int i = 0; i < 20; i++)
        {
            if(i != 19)
            {
                prefabindexlist.Add(0);
            }
            else
            {
                prefabindexlist.Add(1);
            }
        }
        ClickAndLoadMoreContainer.setCellDatasByDataList(prefabindexlist);
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
        var totalcellcount = ClickAndLoadMoreContainer.getCellTotalCount();
        if(cellindex != (totalcellcount - 1))
        {
            var showcellindexcell = cellinstance.GetComponent<ShowCellIndexCell>();
            if (showcellindexcell == null)
            {
                showcellindexcell = cellinstance.AddComponent<ShowCellIndexCell>();
            }
            showcellindexcell.init(cellindex);
        }
        else
        {
            var clickloadmorecell = cellinstance.GetComponent<ClickLoadMoreCell>();
            if (clickloadmorecell == null)
            {
                clickloadmorecell = cellinstance.AddComponent<ClickLoadMoreCell>();
            }
            clickloadmorecell.init(cellindex, ClickAndLoadMoreContainer);
        }
    }
}