/*
 * Description:             ShowCellIndexCell.cs
 * Author:                  TONYTANG
 * Create Date:             2021/04/29
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ShowCellIndexCell.cs
/// 显示单元格索引单元格脚本
/// </summary>
public class ShowCellIndexCell : MonoBehaviour
{
    /// <summary>
    /// Cell Name Text
    /// </summary>
    [Header("单元格名字文本")]
    public Text TxtCellName;

    /// <summary>
    /// Initialization
    /// </summary>
    /// <param name="cellindex"></param>
    public void init(int cellindex)
    {
        TxtCellName.text = $"Cell Index:{cellindex}";
    }
}