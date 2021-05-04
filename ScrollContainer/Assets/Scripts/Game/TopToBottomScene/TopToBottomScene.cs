/*
 * Description:             TopToBottomScene.cs
 * Author:                  TANGHUAN
 * Create Date:             2021/04/27
 */

using TH.Modules.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// TopToBottomScene Script
/// </summary>
public class TopToBottomScene : MonoBehaviour
{
    /// <summary>
    /// BackToMainMenu
    /// </summary>
    [Header("返回主菜单按钮")]
    public Button BtnBackToMainMenu;

    /// <summary>
    /// TopToBottomContainer
    /// </summary>
    [Header("竖向从上往下单元格容器")]
    public VerticalScrollContainer TopToBottomContainer;

    void Start()
    {
        BtnBackToMainMenu.onClick.AddListener(onBtnBackToMainMenu);
        TopToBottomContainer.bindContainerCallBack(onCellShow);
        TopToBottomContainer.setCellDatasByCellCount(20);
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
        var toptobottomcell = cellinstance.GetComponent<ShowCellIndexCell>();
        if(toptobottomcell == null)
        {
            toptobottomcell = cellinstance.AddComponent<ShowCellIndexCell>();
        }
        toptobottomcell.init(cellindex);
    }
}