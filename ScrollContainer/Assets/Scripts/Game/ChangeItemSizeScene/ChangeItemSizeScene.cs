/*
 * Description:             ChangeItemSizeScene.cs
 * Author:                  TANGHUAN
 * Create Date:             2021/04/27
 */

using TH.Modules.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// ChangeItemSizeScene Script
/// </summary>
public class ChangeItemSizeScene : MonoBehaviour
{
    /// <summary>
    /// BackToMainMenu
    /// </summary>
    [Header("返回主菜单按钮")]
    public Button BtnBackToMainMenu;

    /// <summary>
    /// ChangeItemSizeContainer
    /// </summary>
    [Header("改变单元格大小单元格容器")]
    public VerticalScrollContainer ChangeItemSizeContainer;

    void Start()
    {
        BtnBackToMainMenu.onClick.AddListener(OnBtnBackToMainMenu);
        ChangeItemSizeContainer.BindContainerCallBack(OnCellShow);
        ChangeItemSizeContainer.SetCellCount(20);
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
        var changeItemSizeCell = cellInstance.GetComponent<ChangeItemSizeCell>();
        if (changeItemSizeCell == null)
        {
            changeItemSizeCell = cellInstance.AddComponent<ChangeItemSizeCell>();
        }
        changeItemSizeCell.init(cellIndex, ChangeItemSizeContainer);
    }
}