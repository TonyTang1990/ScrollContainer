/*
 * Description:             ClickLoadMoreCell.cs
 * Author:                  TONYTANG
 * Create Date:             2021/05/02
 */

using System.Collections;
using System.Collections.Generic;
using TH.Modules.UI;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ClickLoadMoreCell.cs
/// 点击添加更多单元格单元格
/// </summary>
public class ClickLoadMoreCell : MonoBehaviour 
{
    /// <summary>
    /// Load More Button
    /// </summary>
    [Header("加载更多单元格按钮")]
    public Button BtnLoadMore;

    /// <summary>
    /// Cell Index
    /// </summary>
    private int mCellIndex;

    /// <summary>
    /// Owner Scroll Container
    /// </summary>
    private BaseScrollContainer mOwnerContainer;

    /// <summary>
    /// Initialization
    /// </summary>
    /// <param name="cellindex"></param>
    /// <param name="ownercontainer"></param>
    public void init(int cellindex, BaseScrollContainer ownercontainer)
    {
        BtnLoadMore.onClick.RemoveAllListeners();
        BtnLoadMore.onClick.AddListener(onBtnLoadMore);
        mCellIndex = cellindex;
        mOwnerContainer = ownercontainer;
    }

    /// <summary>
    /// On Click Load More Button
    /// </summary>
    private void onBtnLoadMore()
    {
        var celldatalist = mOwnerContainer.createNormalCellDataListWithCount(20);
        mOwnerContainer.addCellDataListWithIndex(celldatalist, mCellIndex);
    }
}