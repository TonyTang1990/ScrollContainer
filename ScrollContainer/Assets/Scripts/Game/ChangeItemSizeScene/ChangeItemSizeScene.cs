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
        BtnBackToMainMenu.onClick.AddListener(onBtnBackToMainMenu);
        ChangeItemSizeContainer.bindContainerCallBack(onCellShow);
        ChangeItemSizeContainer.setCellDatasByCellCount(20);
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
        var changeitemsizecell = cellinstance.GetComponent<ChangeItemSizeCell>();
        if (changeitemsizecell == null)
        {
            changeitemsizecell = cellinstance.AddComponent<ChangeItemSizeCell>();
        }
        changeitemsizecell.init(cellindex, ChangeItemSizeContainer);
    }
}