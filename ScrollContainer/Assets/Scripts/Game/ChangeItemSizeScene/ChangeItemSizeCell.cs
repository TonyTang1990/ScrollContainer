/*
 * Description:             ChangeItemSizeCell.cs
 * Author:                  TONYTANG
 * Create Date:             2021/05/02
 */

using System.Collections;
using System.Collections.Generic;
using TH.Modules.UI;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ChangeItemSizeCell.cs
/// </summary>
public class ChangeItemSizeCell : MonoBehaviour 
{
    /// <summary>
    /// Cell Content Text
    /// </summary>
    [Header("单元格内容文本")]
    public Text TxtCellContent;

    /// <summary>
    /// Change Cell Size Button
    /// </summary>
    [Header("改变单元格大小按钮")]
    public Button BtnChangeSize;

    /// <summary>
    /// Expand Area Rect
    /// </summary>
    [Header("扩展区域节点对象")]
    public RectTransform RectExpandArea;

    /// <summary>
    /// Is under expand
    /// 是否处于扩展状态
    /// </summary>
    private bool mIsExpanding;

    /// <summary>
    /// Cell Index
    /// 单元格索引
    /// </summary>
    private int mCellIndex;

    /// <summary>
    /// Owner Container
    /// 所属容器
    /// </summary>
    private BaseScrollContainer mOwnerContainer;

    /// <summary>
    /// Initialization
    /// </summary>
    /// <param name="cellindex"></param>
    public void init(int cellindex, BaseScrollContainer ownercontainer)
    {
        TxtCellContent.text = $"Cell Index:{cellindex}";
        BtnChangeSize.onClick.RemoveAllListeners();
        BtnChangeSize.onClick.AddListener(onBtnChangeSize);
        RectExpandArea.gameObject.SetActive(mIsExpanding);
        mCellIndex = cellindex;
        mOwnerContainer = ownercontainer;
    }

    /// <summary>
    /// On Click Change Size Button
    /// </summary>
    private void onBtnChangeSize()
    {
        mIsExpanding = !mIsExpanding;
        RectExpandArea.gameObject.SetActive(mIsExpanding);
        var prefabsize = mOwnerContainer.getPrefabTemplateSizeWithCellIndex(mCellIndex);
        var newsize = prefabsize;
        if(mIsExpanding)
        {
            newsize.y += RectExpandArea.rect.size.y;
        }
        var celldata = mOwnerContainer.getCellDataWithIndex(mCellIndex);
        celldata.changeSize(newsize);
    }
}