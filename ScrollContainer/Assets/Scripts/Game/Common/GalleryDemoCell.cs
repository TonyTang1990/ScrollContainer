/*
 * Description:             GalleryDemoCell.cs
 * Author:                  TONYTANG
 * Create Date:             2021/05/03
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// GalleryDemoCell.cs
/// 展示单元格脚本
/// </summary>
public class GalleryDemoCell : MonoBehaviour 
{
    /// <summary>
    /// Cell Parent Node CanvasGroup
    /// </summary>
    [Header("父挂在节点")]
    public CanvasGroup CGParentNode;

    /// <summary>
    /// Cell Name Text
    /// </summary>
    [Header("单元格名字文本")]
    public Text TxtCellName;

    /// <summary>
    /// 
    /// </summary>
    private Vector3 mTempScale;

    /// <summary>
    /// Initialization
    /// </summary>
    /// <param name="cellIndex"></param>
    public void Init(int cellIndex)
    {
        TxtCellName.text = $"Cell Index:{cellIndex}";
        mTempScale = Vector3.one;
    }

    /// <summary>
    /// 更新
    /// </summary>
    /// <param name="newScale"></param>
    /// <param name="newAlpha"></param>
    public void UpdateScaleAndAlpha(float newScale, float newAlpha)
    {
        mTempScale.x = newScale;
        mTempScale.y = newScale;
        CGParentNode.transform.localScale = mTempScale;
        CGParentNode.alpha = newAlpha;
    }
}