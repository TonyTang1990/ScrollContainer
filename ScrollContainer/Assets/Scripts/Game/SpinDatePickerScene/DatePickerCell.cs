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
    /// <param name="cellindex"></param>
    /// <param name="date"></param>
    /// <param name="datetype"></param>
    /// <param name="ischosendate"></param>
    public void init(int cellindex, int date, EDateType datetype, bool ischosendate = false)
    {
        mCellIndex = cellindex;
        mDateType = datetype;
        TxtDate.text = date.ToString();
        TxtDate.color = ischosendate ? Color.red : Color.white;
    }

}