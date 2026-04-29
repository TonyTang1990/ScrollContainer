/*
 * Description:             BottomToTopScene.cs
 * Author:                  TANGHUAN
 * Create Date:             2021/04/27
 */

using TH.Modules.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// BottomToTopScene Script
/// </summary>
public class BottomToTopScene : MonoBehaviour
{
    /// <summary>
    /// BackToMainMenu
    /// </summary>
    [Header("返回主菜单按钮")]
    public Button BtnBackToMainMenu;

    /// <summary>
    /// BottomToTopContainer
    /// </summary>
    [Header("竖向从下往上单元格容器")]
    public VerticalScrollContainer BottomToTopContainer;

    void Start()
    {
        BtnBackToMainMenu.onClick.AddListener(OnBtnBackToMainMenu);
        BottomToTopContainer.BindContainerCallBack(OnCellShow);
        BottomToTopContainer.SetCellCount(20);
    }

    /// <summary>
    /// 返回主界面
    /// </summary>
    private void OnBtnBackToMainMenu()
    {
        SceneManager.LoadScene(SceneNameDef.LauncherScene);
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
}