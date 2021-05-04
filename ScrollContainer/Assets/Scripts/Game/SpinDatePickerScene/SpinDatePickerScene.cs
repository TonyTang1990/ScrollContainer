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

        BtnBackToMainMenu.onClick.AddListener(onBtnBackToMainMenu);

        VerticalYearContainer.bindContainerCallBack(onCellYearShow, null, null, onCellYearMoveTo);
        VerticalYearContainer.setCellDatasByDataList(mYearhPrefabIndexList);

        VerticalMonthContainer.bindContainerCallBack(onCellMonthShow, null, null, onCellMonthMoveTo);
        VerticalMonthContainer.setCellDatasByDataList(mMonthPrefabIndexList);

        VerticalDayContainer.bindContainerCallBack(onCellDayShow, null, null, onCellDayMoveTo);
        updateDayContainerView();

        updateDateChosenView();

        // Make sure current date is default chosen view
        VerticalMonthContainer.moveToIndex(mCurrentChosenDateTime.Month - 1, 0f);
        VerticalDayContainer.moveToIndex(mCurrentChosenDateTime.Day - 1, 0f);
    }

    /// <summary>
    /// Update Day COntainer Data
    /// 更新日期容器数据
    /// </summary>
    private void updateDayContainerData()
    {
        mDayPrefabIndexList.Clear();
        var monthtotaldaynumber = DateTime.DaysInMonth(mCurrentChosenDateTime.Year, mCurrentChosenDateTime.Month);
        // Insert two empty cell
        for (int i = 0; i < monthtotaldaynumber + 2; i++)
        {
            if (i == 0 || i == monthtotaldaynumber + 1)
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
    private void updateDayContainerView()
    {
        updateDayContainerData();
        VerticalDayContainer.setCellDatasByDataList(mDayPrefabIndexList);
    }

    /// <summary>
    /// Update Date Chosen Text View
    /// </summary>
    private void updateDateChosenView()
    {
        TxtDateChosen.text = mCurrentChosenDateTime.ToShortDateString();
    }

    /// <summary>
    /// 返回主界面
    /// </summary>
    private void onBtnBackToMainMenu()
    {
        SceneManager.LoadScene(SceneNameDef.LauncherScene);
    }

    /// <summary>
    /// 年单元格显示回调
    /// </summary>
    /// <param name="cellindex"></param>
    /// <param name="cellinstance"></param>
    private void onCellYearShow(int cellindex, GameObject cellinstance)
    {
        updateYearCellView(cellindex, cellinstance);
    }

    /// <summary>
    /// 年单元格滚动到指定单元格回调
    /// </summary>
    /// <param name="cellindex"></param>
    /// <param name="cellinstance"></param>
    private void onCellYearMoveTo(int cellindex, GameObject cellinstance)
    {
        var newyear = mNowDateTime.Year + cellindex;
        var newyearindex = cellindex + 1;
        if (newyear != mCurrentChosenDateTime.Year)
        {
            var oldyearindex = mCurrentChosenDateTime.Year - mNowDateTime.Year + 1;
            var isdaycontainerneedupdate = isDayContainerNeedUpdate(newyear, mCurrentChosenDateTime.Month, mCurrentChosenDateTime.Year, mCurrentChosenDateTime.Month);
            mCurrentChosenDateTime = new DateTime(newyear, mCurrentChosenDateTime.Month, isdaycontainerneedupdate ? 1 : mCurrentChosenDateTime.Day);
            if(isdaycontainerneedupdate)
            {
                updateDayContainerView();
            }
            updateDateChosenView();
            var oldyearcelldata = VerticalYearContainer.getCellDataWithIndex(oldyearindex);
            if (oldyearcelldata.isVisible())
            {
                updateDayCellView(oldyearindex, oldyearcelldata.CellGO);
            }
            var newyearcelldata = VerticalYearContainer.getCellDataWithIndex(newyearindex);
            updateYearCellView(newyearindex, newyearcelldata.CellGO);
        }
    }

    /// <summary>
    /// 更新指定年份单元格显示
    /// </summary>
    /// <param name="cellindex"></param>
    /// <param name="cellinstance"></param>
    private void updateYearCellView(int cellindex, GameObject cellinstance)
    {
        if (cellindex != 0 && cellindex != YearChosenOffset + 1)
        {
            var newyearoffset = cellindex - 1;
            var datepickercell = cellinstance.GetComponent<DatePickerCell>();
            if (datepickercell == null)
            {
                datepickercell = cellinstance.AddComponent<DatePickerCell>();
            }
            datepickercell.init(cellindex, mNowDateTime.Year + newyearoffset, EDateType.Year, mCurrentChosenDateTime.Year == (mNowDateTime.Year + newyearoffset));
        }
    }

    /// <summary>
    /// 月单元格显示回调
    /// </summary>
    /// <param name="cellindex"></param>
    /// <param name="cellinstance"></param>
    private void onCellMonthShow(int cellindex, GameObject cellinstance)
    {
        updateMonthCellView(cellindex, cellinstance);
    }

    /// <summary>
    /// 月单元格滚动到指定单元格回调
    /// </summary>
    /// <param name="cellindex"></param>
    /// <param name="cellinstance"></param>
    private void onCellMonthMoveTo(int cellindex, GameObject cellinstance)
    {
        var newmonth = cellindex + 1;
        if (newmonth != mCurrentChosenDateTime.Month)
        {
            var oldmonth = mCurrentChosenDateTime.Month;
            var isdaycontainerneedupdate = isDayContainerNeedUpdate(mCurrentChosenDateTime.Year, newmonth, mCurrentChosenDateTime.Year, mCurrentChosenDateTime.Month);
            mCurrentChosenDateTime = new DateTime(mCurrentChosenDateTime.Year, newmonth, isdaycontainerneedupdate ? 1 : mCurrentChosenDateTime.Day);
            if (isdaycontainerneedupdate)
            {
                updateDayContainerView();
            }
            updateDateChosenView();
            var oldmonthcelldata = VerticalMonthContainer.getCellDataWithIndex(oldmonth);
            if (oldmonthcelldata.isVisible())
            {
                updateDayCellView(oldmonth, oldmonthcelldata.CellGO);
            }
            var newmonthcelldata = VerticalMonthContainer.getCellDataWithIndex(newmonth);
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
            datepickercell.init(cellindex, cellmonth, EDateType.Month, mCurrentChosenDateTime.Month == cellmonth);
        }
    }

    /// <summary>
    /// 日单元格显示回调
    /// </summary>
    /// <param name="cellindex"></param>
    /// <param name="cellinstance"></param>
    private void onCellDayShow(int cellindex, GameObject cellinstance)
    {
        updateDayCellView(cellindex, cellinstance);
    }

    /// <summary>
    /// 日单元格滚动到指定单元格回调
    /// </summary>
    /// <param name="cellindex"></param>
    /// <param name="cellinstance"></param>
    private void onCellDayMoveTo(int cellindex, GameObject cellinstance)
    {
        var newday = cellindex + 1;
        if(newday != mCurrentChosenDateTime.Day)
        {
            var oldday = mCurrentChosenDateTime.Day;
            mCurrentChosenDateTime = new DateTime(mCurrentChosenDateTime.Year, mCurrentChosenDateTime.Month, newday);
            updateDateChosenView();
            var olddaycelldata = VerticalDayContainer.getCellDataWithIndex(oldday);
            if(olddaycelldata.isVisible())
            {
                updateDayCellView(oldday, olddaycelldata.CellGO);
            }
            var newdaycelldata = VerticalDayContainer.getCellDataWithIndex(newday);
            updateDayCellView(newday, newdaycelldata.CellGO);
        }
    }

    /// <summary>
    /// 更新指定天单元格显示
    /// </summary>
    /// <param name="cellindex"></param>
    /// <param name="cellinstance"></param>
    private void updateDayCellView(int cellindex, GameObject cellinstance)
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
            datepickercell.init(cellindex, cellday, EDateType.Day, mCurrentChosenDateTime.Day == cellday);
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