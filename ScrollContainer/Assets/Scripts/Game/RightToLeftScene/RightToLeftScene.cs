/*
 * Description:             RightToLeftScene.cs
 * Author:                  TANGHUAN
 * Create Date:             2021/04/27
 */

using TH.Modules.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// RightToLeftScene Script
/// </summary>
public class RightToLeftScene : MonoBehaviour
{
    /// <summary>
    /// BackToMainMenu
    /// </summary>
    [Header("返回主菜单按钮")]
    public Button BtnBackToMainMenu;

    /// <summary>
    /// RightToLeftContainer
    /// </summary>
    [Header("横向从右往左单元格容器")]
    public HorizontalScrollContainer RightToLeftContainer;

    void Start()
    {
        BtnBackToMainMenu.onClick.AddListener(OnBtnBackToMainMenu);
        RightToLeftContainer.BindContainerCallBack(OnCellShow);
        RightToLeftContainer.SetCellCount(20);
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
        var shoCellIndexCell = cellInstance.GetComponent<ShowCellIndexCell>();
        if (shoCellIndexCell == null)
        {
            shoCellIndexCell = cellInstance.AddComponent<ShowCellIndexCell>();
        }
        shoCellIndexCell.Init(cellIndex);
    }
}