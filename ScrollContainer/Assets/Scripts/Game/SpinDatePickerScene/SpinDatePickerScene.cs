/*
 * Description:             SpinDatePickerScene.cs
 * Author:                  TANGHUAN
 * Create Date:             2021/04/27
 */

using System;
using System.Collections.Generic;
using TH.Modules.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 日期类型
/// </summary>
public enum EDateType
{
    Year = 1,
    Month,
    Day
}

/// <summary>
/// SpinDatePickerScene Script
/// </summary>
public class SpinDatePickerScene : MonoBehaviour 
{
    /// <summary>
    /// BackToMainMenu
    /// </summary>
    [Header("返回主菜单按钮")]
    public Button BtnBackToMainMenu;

    /// <summary>
    /// Current Date Chosen Text
    /// </summary>
    [Header("当前日期选择显示文本")]
    public Text TxtDateChosen;

    /// <summary>
    /// VerticalYearContainer
    /// </summary>
    [Header("竖向年份选择单元格容器")]
    public VerticalScrollContainer VerticalYearContainer;

    /// <summary>
    /// VerticalMonthContainer
    /// </summary>
    [Header("竖向月份选择单元格容器")]
    public VerticalScrollContainer VerticalMonthContainer;

    /// <summary>
    /// VerticalDayContainer
    /// </summary>
    [Header("竖向日期选择单元格容器")]
    public VerticalScrollContainer VerticalDayContainer;

    /// <summary>
    /// 当前选择日期
    /// </summary>
    private DateTime mCurrentChosenDateTime;

    /// <summary>
    /// Year Chosen Offset
    /// 年限选择偏差
    /// </summary>
    private const int YearChosenOffset = 20;

    /// <summary>
    /// Now DateTime
    /// 当前时间
    /// </summary>
    private DateTime mNowDateTime;

    /// <summary>
    /// Year Prefab Index Data List
    /// </summary>
    private List<int> mYearhPrefabIndexList;

    /// <summary>
    /// Month Prefab Index Data List
    /// </summary>
    private List<int> mMonthPrefabIndexList;

    /// <summary>
    /// Dat Prefab Index Data List
    /// </summary>
    private List<int> mDayPrefabIndexList;

    void Start()
    {
        mNowDateTime = DateTime.Now;
        mCurrentChosenDateTime = DateTime.Now;

        mYearhPrefabIndexList = new List<int>();
        // Insert two empty cell
        for (int i = 0; i < YearChosenOffset + 2; i++)
        {
            if(i == 0 || i == YearChosenOffset + 1)
            {
                mYearhPrefabIndexList.Add(1);
            }
            else
            {
                mYearhPrefabIndexList.Add(0);
            }
        }
        mMonthPrefabIndexList = new List<int>();
        // Insert two empty cell
        for (int i = 0; i < 12 + 2; i++)
        {
            if (i == 0 || i == 13)
            {
                mMonthPrefabIndexList.Add(1);
            }
            else
            {
                mMonthPrefabIndexList.Add(0);
            }
        }
        mDayPrefabIndexList = new List<int>();

        BtnBackToMainMenu.onClick.AddListener(OnBtnBackToMainMenu);

        VerticalYearContainer.BindContainerCallBack(OnCellYearShow, null, null, OnCellYearMoveTo);
        VerticalYearContainer.SetCellDatas(mYearhPrefabIndexList);

        VerticalMonthContainer.BindContainerCallBack(OnCellMonthShow, null, null, OnCellMonthMoveTo);
        VerticalMonthContainer.SetCellDatas(mMonthPrefabIndexList);

        VerticalDayContainer.BindContainerCallBack(OnCellDayShow, null, null, OnCellDayMoveTo);
        UpdateDayContainerView();

        UpdateDateChosenView();

        // Make sure current date is default chosen view
        VerticalMonthContainer.MoveToIndex(mCurrentChosenDateTime.Month - 1, 0f);
        VerticalDayContainer.MoveToIndex(mCurrentChosenDateTime.Day - 1, 0f);
    }

    /// <summary>
    /// Update Day COntainer Data
    /// 更新日期容器数据
    /// </summary>
    private void UpdateDayContainerData()
    {
        mDayPrefabIndexList.Clear();
        var monthTotalDayNum = DateTime.DaysInMonth(mCurrentChosenDateTime.Year, mCurrentChosenDateTime.Month);
        // Insert two empty cell
        for (int i = 0; i < monthTotalDayNum + 2; i++)
        {
            if (i == 0 || i == monthTotalDayNum + 1)
            {
                mDayPrefabIndexList.Add(1);
            }
            else
            {
                mDayPrefabIndexList.Add(0);
            }
        }
    }

    /// <summary>
    /// Update Day Container View
    /// 更新日期选择容器显示
    /// </summary>
    private void UpdateDayContainerView()
    {
        UpdateDayContainerData();
        VerticalDayContainer.SetCellDatas(mDayPrefabIndexList);
    }

    /// <summary>
    /// Update Date Chosen Text View
    /// </summary>
    private void UpdateDateChosenView()
    {
        TxtDateChosen.text = mCurrentChosenDateTime.ToShortDateString();
    }

    /// <summary>
    /// 返回主界面
    /// </summary>
    private void OnBtnBackToMainMenu()
    {
        SceneManager.LoadScene(SceneNameDef.LauncherScene);
    }

    /// <summary>
    /// 年单元格显示回调
    /// </summary>
    /// <param name="cellIndex"></param>
    /// <param name="cellInstance"></param>
    private void OnCellYearShow(int cellIndex, GameObject cellInstance)
    {
        UpdateYearCellView(cellIndex, cellInstance);
    }

    /// <summary>
    /// 年单元格滚动到指定单元格回调
    /// </summary>
    /// <param name="cellIndex"></param>
    /// <param name="cellInstance"></param>
    private void OnCellYearMoveTo(int cellIndex, GameObject cellInstance)
    {
        var newYear = mNowDateTime.Year + cellIndex;
        var newYearIndex = cellIndex + 1;
        if (newYear != mCurrentChosenDateTime.Year)
        {
            var oldYearIndex = mCurrentChosenDateTime.Year - mNowDateTime.Year + 1;
            var isDayContainerNeedUpdate = this.isDayContainerNeedUpdate(newYear, mCurrentChosenDateTime.Month, mCurrentChosenDateTime.Year, mCurrentChosenDateTime.Month);
            mCurrentChosenDateTime = new DateTime(newYear, mCurrentChosenDateTime.Month, isDayContainerNeedUpdate ? 1 : mCurrentChosenDateTime.Day);
            if(isDayContainerNeedUpdate)
            {
                UpdateDayContainerView();
            }
            UpdateDateChosenView();
            var oldYearCellData = VerticalYearContainer.GetCellData(oldYearIndex);
            if (oldYearCellData.IsVisible())
            {
                UpdateDayCellView(oldYearIndex, oldYearCellData.CellGO);
            }
            var newYearCellData = VerticalYearContainer.GetCellData(newYearIndex);
            UpdateYearCellView(newYearIndex, newYearCellData.CellGO);
        }
    }

    /// <summary>
    /// 更新指定年份单元格显示
    /// </summary>
    /// <param name="cellIndex"></param>
    /// <param name="cellInstance"></param>
    private void UpdateYearCellView(int cellIndex, GameObject cellInstance)
    {
        if (cellIndex != 0 && cellIndex != YearChosenOffset + 1)
        {
            var newYearOffset = cellIndex - 1;
            var datePickerCell = cellInstance.GetComponent<DatePickerCell>();
            if (datePickerCell == null)
            {
                datePickerCell = cellInstance.AddComponent<DatePickerCell>();
            }
            datePickerCell.Init(cellIndex, mNowDateTime.Year + newYearOffset, EDateType.Year, mCurrentChosenDateTime.Year == (mNowDateTime.Year + newYearOffset));
        }
    }

    /// <summary>
    /// 月单元格显示回调
    /// </summary>
    /// <param name="cellindex"></param>
    /// <param name="cellinstance"></param>
    private void OnCellMonthShow(int cellindex, GameObject cellinstance)
    {
        updateMonthCellView(cellindex, cellinstance);
    }

    /// <summary>
    /// 月单元格滚动到指定单元格回调
    /// </summary>
    /// <param name="cellindex"></param>
    /// <param name="cellinstance"></param>
    private void OnCellMonthMoveTo(int cellindex, GameObject cellinstance)
    {
        var newmonth = cellindex + 1;
        if (newmonth != mCurrentChosenDateTime.Month)
        {
            var oldmonth = mCurrentChosenDateTime.Month;
            var isdaycontainerneedupdate = isDayContainerNeedUpdate(mCurrentChosenDateTime.Year, newmonth, mCurrentChosenDateTime.Year, mCurrentChosenDateTime.Month);
            mCurrentChosenDateTime = new DateTime(mCurrentChosenDateTime.Year, newmonth, isdaycontainerneedupdate ? 1 : mCurrentChosenDateTime.Day);
            if (isdaycontainerneedupdate)
            {
                UpdateDayContainerView();
            }
            UpdateDateChosenView();
            var oldmonthcelldata = VerticalMonthContainer.GetCellData(oldmonth);
            if (oldmonthcelldata.IsVisible())
            {
                UpdateDayCellView(oldmonth, oldmonthcelldata.CellGO);
            }
            var newmonthcelldata = VerticalMonthContainer.GetCellData(newmonth);
            updateMonthCellView(newmonth, newmonthcelldata.CellGO);
        }
    }

    /// <summary>
    /// 更新指定月份单元格显示
    /// </summary>
    /// <param name="cellindex"></param>
    /// <param name="cellinstance"></param>
    private void updateMonthCellView(int cellindex, GameObject cellinstance)
    {
        if (cellindex != 0 && cellindex != 13)
        {
            var cellmonth = cellindex;
            var datepickercell = cellinstance.GetComponent<DatePickerCell>();
            if (datepickercell == null)
            {
                datepickercell = cellinstance.AddComponent<DatePickerCell>();
            }
            datepickercell.Init(cellindex, cellmonth, EDateType.Month, mCurrentChosenDateTime.Month == cellmonth);
        }
    }

    /// <summary>
    /// 日单元格显示回调
    /// </summary>
    /// <param name="cellindex"></param>
    /// <param name="cellinstance"></param>
    private void OnCellDayShow(int cellindex, GameObject cellinstance)
    {
        UpdateDayCellView(cellindex, cellinstance);
    }

    /// <summary>
    /// 日单元格滚动到指定单元格回调
    /// </summary>
    /// <param name="cellindex"></param>
    /// <param name="cellinstance"></param>
    private void OnCellDayMoveTo(int cellindex, GameObject cellinstance)
    {
        var newday = cellindex + 1;
        if(newday != mCurrentChosenDateTime.Day)
        {
            var oldday = mCurrentChosenDateTime.Day;
            mCurrentChosenDateTime = new DateTime(mCurrentChosenDateTime.Year, mCurrentChosenDateTime.Month, newday);
            UpdateDateChosenView();
            var olddaycelldata = VerticalDayContainer.GetCellData(oldday);
            if(olddaycelldata.IsVisible())
            {
                UpdateDayCellView(oldday, olddaycelldata.CellGO);
            }
            var newdaycelldata = VerticalDayContainer.GetCellData(newday);
            UpdateDayCellView(newday, newdaycelldata.CellGO);
        }
    }

    /// <summary>
    /// 更新指定天单元格显示
    /// </summary>
    /// <param name="cellindex"></param>
    /// <param name="cellinstance"></param>
    private void UpdateDayCellView(int cellindex, GameObject cellinstance)
    {
        var monthtotaldaynumber = DateTime.DaysInMonth(mCurrentChosenDateTime.Year, mCurrentChosenDateTime.Month);
        if (cellindex != 0 && cellindex != monthtotaldaynumber + 1)
        {
            var cellday = cellindex;
            var datepickercell = cellinstance.GetComponent<DatePickerCell>();
            if (datepickercell == null)
            {
                datepickercell = cellinstance.AddComponent<DatePickerCell>();
            }
            datepickercell.Init(cellindex, cellday, EDateType.Day, mCurrentChosenDateTime.Day == cellday);
        }
    }

    /// <summary>
    /// 检查天数容器是否需要更新
    /// </summary>
    /// <param name="newyear"></param>
    /// <param name="newmonth"></param>
    /// <param name="oldmonth"></param>
    /// <param name="oldyear"></param>
    /// <returns></returns>
    private bool isDayContainerNeedUpdate(int newyear, int newmonth, int oldyear, int oldmonth)
    {
        var newmonthtotaldaynumber = DateTime.DaysInMonth(newyear, newmonth);
        var oldmonthtotaldaynumber = DateTime.DaysInMonth(oldyear, oldmonth);
        return newmonthtotaldaynumber != oldmonthtotaldaynumber;
    }
}