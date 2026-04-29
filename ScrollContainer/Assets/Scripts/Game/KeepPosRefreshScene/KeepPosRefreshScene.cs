/*
 * Description:             KeepPosRefreshScene.cs
 * Author:                  TANGHUAN
 * Create Date:             2026/04/11
 */

using TH.Modules.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// KeepPosRefreshScene Script
/// </summary>
public class KeepPosRefreshScene : MonoBehaviour 
{
    /// <summary>
    /// BackToMainMenu
    /// </summary>
    [Header("返回主菜单按钮")]
    public Button BtnBackToMainMenu;

    /// <summary>
    /// Operation Cell Num
    /// </summary>
    [Header("操作的单元格数量输入文本")]
    public InputField IFCellNum;

    /// <summary>
    /// BtnRefreshCell
    /// </summary>
    [Header("刷新单元格数量按钮")]
    public Button BtnRefreshCellNum;

    /// <summary>
    /// KeepPosRefreshContainer
    /// </summary>
    [Header("保持位置刷新数量单元格容器")]
    public VerticalScrollContainer KeepPosRefreshContainer;

    /// <summary>
    /// Cell Num In Input Field
    /// </summary>
    private int mInputFiledCellNum;

    void Start()
    {
        mInputFiledCellNum = 0;
        IFCellNum.text = mInputFiledCellNum.ToString();
        BtnBackToMainMenu.onClick.AddListener(OnBtnBackToMainMenu);
        BtnRefreshCellNum.onClick.AddListener(OnBtnRefreshCellNum);
        KeepPosRefreshContainer.BindContainerCallBack(OnCellShow);
        KeepPosRefreshContainer.SetCellCount(20);
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
}