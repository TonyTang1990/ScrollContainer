/*
 * Description:             DatePickerCell.cs
 * Author:                  TONYTANG
 * Create Date:             2021/05/04
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// DatePickerCell.cs
/// 日期选择单元格脚本
/// </summary>
public class DatePickerCell : MonoBehaviour 
{
    /// <summary>
    /// Date Text
    /// </summary>
    [Header("日期文本")]
    public Text TxtDate;

    /// <summary>
    /// Cell Index
    /// 单元格索引
    /// </summary>
    private int mCellIndex;

    /// <summary>
    /// Date Type
    /// 日期类型
    /// </summary>
    private EDateType mDateType;

    /// <summary>
    /// Initialization
    /// </summary>
    /// <param name="cellIndex"></param>
    /// <param name="date"></param>
    /// <param name="dateType"></param>
    /// <param name="isChosenDate"></param>
    public void Init(int cellIndex, int date, EDateType dateType, bool isChosenDate = false)
    {
        mCellIndex = cellIndex;
        mDateType = dateType;
        TxtDate.text = date.ToString();
        TxtDate.color = isChosenDate ? Color.red : Color.white;
    }

}