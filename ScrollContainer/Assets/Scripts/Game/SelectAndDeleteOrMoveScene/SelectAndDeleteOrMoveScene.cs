/*
 * Description:             SelectAndDeleteOrMoveScene.cs
 * Author:                  TANGHUAN
 * Create Date:             2021/04/27
 */

using TH.Modules.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// SelectAndDeleteOrMoveScene Script
/// </summary>
public class SelectAndDeleteOrMoveScene : MonoBehaviour 
{
    /// <summary>
    /// BackToMainMenu
    /// </summary>
    [Header("返回主菜单按钮")]
    public Button BtnBackToMainMenu;

    /// <summary>
    /// Operation Cell Index
    /// </summary>
    [Header("操作的单元格索引输入文本")]
    public InputField IFCellIndex;

    /// <summary>
    /// BtnInsertCell
    /// </summary>
    [Header("插入单元格按钮")]
    public Button BtnInsertCell;

    /// <summary>
    /// BtnDeleteCell
    /// </summary>
    [Header("删除单元格按钮")]
    public Button BtnDeleteCell;

    /// <summary>
    /// BtnMoveToCell
    /// </summary>
    [Header("移动到指定单元格按钮")]
    public Button BtnMoveToCell;

    /// <summary>
    /// SelectAndDeleteOrMoveContainer
    /// </summary>
    [Header("增删移动单元格容器")]
    public VerticalScrollContainer SelectAndDeleteOrMoveContainer;

    /// <summary>
    /// Cell Index In Input Field
    /// </summary>
    private int mInputFiledCellIndex;

    void Start()
    {
        mInputFiledCellIndex = 0;
        IFCellIndex.text = mInputFiledCellIndex.ToString();
        BtnBackToMainMenu.onClick.AddListener(onBtnBackToMainMenu);
        BtnInsertCell.onClick.AddListener(onBtnInsertCell);
        BtnDeleteCell.onClick.AddListener(onBtnDeleteCell);
        BtnMoveToCell.onClick.AddListener(onBtnMoveToCell);
        SelectAndDeleteOrMoveContainer.bindContainerCallBack(onCellShow);
        SelectAndDeleteOrMoveContainer.setCellDatasByCellCount(20);
    }

    /// <summary>
    /// Parse Cell Index in Input field 
    /// 解析单元格参数输入
    /// </summary>
    private bool parseCellIndexInput()
    {
        if(int.TryParse(IFCellIndex.text, out mInputFiledCellIndex))
        {
            Debug.Log($"New Input Cell Index:{mInputFiledCellIndex}");
            return true;
        }
        else
        {
            Debug.LogError($"Invalide cell index:{IFCellIndex.text}");
            return false;
        }
    }

    /// <summary>
    /// 返回主界面
    /// </summary>
    private void onBtnBackToMainMenu()
    {
        SceneManager.LoadScene(SceneNameDef.LauncherScene);
    }

    /// <summary>
    /// Insert Cell With Cell Index
    /// 插入指定位置单元格
    /// </summary>
    private void onBtnInsertCell()
    {
        if(parseCellIndexInput())
        {
            var newcelldata = SelectAndDeleteOrMoveContainer.createNormalCellData();
            SelectAndDeleteOrMoveContainer.addCellDataWithIndex(newcelldata, mInputFiledCellIndex);
        }
    }

    /// <summary>
    /// Delete Specific Cell Index
    /// 删除指定位置单元格
    /// </summary>
    private void onBtnDeleteCell()
    {
        if (parseCellIndexInput())
        {
            SelectAndDeleteOrMoveContainer.removeCellWithIndex(mInputFiledCellIndex);
        }
    }

    /// <summary>
    /// Move To Specific Cell Index
    /// 移动到指定位置单元格
    /// </summary>
    private void onBtnMoveToCell()
    {
        if (parseCellIndexInput())
        {
            SelectAndDeleteOrMoveContainer.moveToIndex(mInputFiledCellIndex);
        }
    }

    /// <summary>
    /// 单元格显示回调
    /// </summary>
    /// <param name="cellindex"></param>
    /// <param name="cellinstance"></param>
    private void onCellShow(int cellindex, GameObject cellinstance)
    {
        var toptobottomcell = cellinstance.GetComponent<ShowCellIndexCell>();
        if (toptobottomcell == null)
        {
            toptobottomcell = cellinstance.AddComponent<ShowCellIndexCell>();
        }
        toptobottomcell.init(cellindex);
    }
}