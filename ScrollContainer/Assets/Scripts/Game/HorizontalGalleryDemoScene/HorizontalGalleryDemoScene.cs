/*
 * Description:             HorizontalGalleryDemoScene.cs
 * Author:                  TANGHUAN
 * Create Date:             2021/04/27
 */

using TH.Modules.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// HorizontalGalleryDemoScene Script
/// </summary>
public class HorizontalGalleryDemoScene : MonoBehaviour 
{
    /// <summary>
    /// BackToMainMenu
    /// </summary>
    [Header("返回主菜单按钮")]
    public Button BtnBackToMainMenu;

    /// <summary>
    /// HorizontalGalleryDemoContainer
    /// </summary>
    [Header("横向展示单元格容器")]
    public HorizontalScrollContainer HorizontalGalleryDemoContainer;

    void Start()
    {
        BtnBackToMainMenu.onClick.AddListener(OnBtnBackToMainMenu);
        HorizontalGalleryDemoContainer.BindContainerCallBack(OnCellShow, null, OnCellVisibleScroll);
        HorizontalGalleryDemoContainer.SetCellCount(20);
    }

    /// <summary>
    /// 返回主界面
    /// </summary>
    private void OnBtnBackToMainMenu()
    {
        SceneManager.LoadScene(SceneNameDef.LauncherScene);
    }

    /// <summary>
    /// 单元格显示回调
    /// </summary>
    /// <param name="cellIndex"></param>
    /// <param name="cellInstance"></param>
    private void OnCellShow(int cellIndex, GameObject cellInstance)
    {
        var galleryDemoCell = cellInstance.GetComponent<GalleryDemoCell>();
        if (galleryDemoCell == null)
        {
            galleryDemoCell = cellInstance.AddComponent<GalleryDemoCell>();
        }
        galleryDemoCell.Init(cellIndex);
    }

    /// <summary>
    /// 单元格可见滚动回调
    /// </summary>
    /// <param name="cellIndex"></param>
    /// <param name="cellInstance"></param>
    /// <param name="currentScrollIndex"></param>
    /// <param name="cellCenterOffsetPos"></param>
    private void OnCellVisibleScroll(int cellIndex, GameObject cellInstance, float currentScrollIndex, float cellCenterOffsetPos)
    {
        //Debug.Log($"CellIndex:{cellindex} currentscrollindex:{currentscrollindex} cellcenteroffsetposition:{cellcenteroffsetposition}");
        var galleryDemoCell = cellInstance.GetComponent<GalleryDemoCell>();
        if (galleryDemoCell == null)
        {
            galleryDemoCell = cellInstance.AddComponent<GalleryDemoCell>();
        }
        var cellCenterOffsetAbsPos = Mathf.Abs(cellCenterOffsetPos);
        var newScale = 1 - cellCenterOffsetAbsPos / 800;
        var newAlpha = 1 - cellCenterOffsetAbsPos / 800;
        galleryDemoCell.UpdateScaleAndAlpha(newScale, newAlpha);
    }
}