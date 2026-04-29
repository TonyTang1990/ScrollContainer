/*
 * Description:             PageViewCell.cs
 * Author:                  TONYTANG
 * Create Date:             2021/05/03
 */

using System.Collections;
using System.Collections.Generic;
using TH.Modules.UI;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// PageViewCell.cs
/// 页面展示容器
/// </summary>
public class PageViewCell : MonoBehaviour 
{
    /// <summary>
    /// PageViewNestedContainer
    /// </summary>
    [Header("竖向页面展示嵌套单元格容器")]
    public VerticalGridScrollContainer PageViewNestedContainer;

    /// <summary>
    /// Page Index Text
    /// </summary>
    [Header("页面索引文本")]
    public Text TxtPageIndex;

    /// <summary>
    /// Initialization
    /// </summary>
    /// <param name="cellIndex"></param>
    public void Init(int cellIndex)
    {
        TxtPageIndex.text = $"Page Index:{cellIndex}";
        PageViewNestedContainer.UnbindContainerCallBack();
        PageViewNestedContainer.BindContainerCallBack(OnCellShow);
        PageViewNestedContainer.SetCellCount(9);
    }

    /// <summary>
    /// 单元格显示
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