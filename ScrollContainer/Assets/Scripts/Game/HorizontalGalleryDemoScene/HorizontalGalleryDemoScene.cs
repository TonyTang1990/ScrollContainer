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
        BtnBackToMainMenu.onClick.AddListener(onBtnBackToMainMenu);
        HorizontalGalleryDemoContainer.bindContainerCallBack(onCellShow, null, onCellVisibleScroll);
        HorizontalGalleryDemoContainer.setCellDatasByCellCount(20);
    }

    /// <summary>
    /// 返回主界面
    /// </summary>
    private void onBtnBackToMainMenu()
    {
        SceneManager.LoadScene(SceneNameDef.LauncherScene);
    }

    /// <summary>
    /// 单元格显示回调
    /// </summary>
    /// <param name="cellindex"></param>
    /// <param name="cellinstance"></param>
    private void onCellShow(int cellindex, GameObject cellinstance)
    {
        var gallerydemocell = cellinstance.GetComponent<GalleryDemoCell>();
        if (gallerydemocell == null)
        {
            gallerydemocell = cellinstance.AddComponent<GalleryDemoCell>();
        }
        gallerydemocell.init(cellindex);
    }

    /// <summary>
    /// 单元格可见滚动回调
    /// </summary>
    /// <param name="cellindex"></param>
    /// <param name="cellinstance"></param>
    /// <param name="currentscrollindex"></param>
    /// <param name="cellcenteroffsetposition"></param>
    private void onCellVisibleScroll(int cellindex, GameObject cellinstance, float currentscrollindex, float cellcenteroffsetposition)
    {
        //Debug.Log($"CellIndex:{cellindex} currentscrollindex:{currentscrollindex} cellcenteroffsetposition:{cellcenteroffsetposition}");
        var gallerydemocell = cellinstance.GetComponent<GalleryDemoCell>();
        if (gallerydemocell == null)
        {
            gallerydemocell = cellinstance.AddComponent<GalleryDemoCell>();
        }
        var cellcenteroffsetabsposition = Mathf.Abs(cellcenteroffsetposition);
        var newscale = 1 - cellcenteroffsetabsposition / 800;
        var newalpha = 1 - cellcenteroffsetabsposition / 800;
        gallerydemocell.updateScaleAndAlpha(newscale, newalpha);
    }
}