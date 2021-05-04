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
    /// <param name="cellindex"></param>
    public void init(int cellindex)
    {
        TxtPageIndex.text = $"Page Index:{cellindex}";
        PageViewNestedContainer.unbindContainerCallBack();
        PageViewNestedContainer.bindContainerCallBack(onCellShow);
        PageViewNestedContainer.setCellDatasByCellCount(9);
    }

    /// <summary>
    /// 单元格显示
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