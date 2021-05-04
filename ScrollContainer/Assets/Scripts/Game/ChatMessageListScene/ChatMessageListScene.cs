/*
 * Description:             ChatMessageListScene.cs
 * Author:                  TANGHUAN
 * Create Date:             2021/04/27
 */

using System.Collections.Generic;
using TH.Modules.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// ChatMessageListScene Script
/// </summary>
public class ChatMessageListScene : MonoBehaviour
{
    /// <summary>
    /// BackToMainMenu
    /// </summary>
    [Header("返回主菜单按钮")]
    public Button BtnBackToMainMenu;

    /// <summary>
    /// LeftToRightContainer
    /// </summary>
    [Header("横向从左往右单元格容器")]
    public VerticalScrollContainer ChatMessageListContainer;

    /// <summary>
    /// 聊天内容列表
    /// </summary>
    private List<string> mChatContentList;

    void Start()
    {
        BtnBackToMainMenu.onClick.AddListener(onBtnBackToMainMenu);
        ChatMessageListContainer.bindContainerCallBack(onCellShow);
        mChatContentList = new List<string>();
        List<int> prefabindexlist = new List<int>();
        List<Vector2> cellsizelist = new List<Vector2>();
        var chatleftprefabtemplate = ChatMessageListContainer.getPrefabTemplateWithPrefabIndex(0);
        var chatleftprefabtemplatesize = chatleftprefabtemplate.GetComponent<RectTransform>().rect.size;
        var chatrightprefabtemplate = ChatMessageListContainer.getPrefabTemplateWithPrefabIndex(1);
        var chatrightprefabtemplatesize = chatrightprefabtemplate.GetComponent<RectTransform>().rect.size;

        TimeCounter tc = new TimeCounter();
        tc.Start("ChatContainer");
        for (int i = 0; i < 20; i++)
        {
            var prefabindex = i % 2;
            prefabindexlist.Add(prefabindex);
            var chatcontent = $"Cell Index:{i}。";
            for (int j = 0; j < i; j++)
            {
                chatcontent += $"Chat Content Index:{j}.";
            }
            mChatContentList.Add(chatcontent);
            // Content Chat
            var chatprefab = prefabindex == 0 ? chatleftprefabtemplate : chatrightprefabtemplate;
            var chatprefabsize = prefabindex == 0 ? chatleftprefabtemplatesize : chatrightprefabtemplatesize;
            if (i % 3 != 2)
            {
                var chatmessagecell = chatprefab.GetComponent<ChatMessageCell>();
                var cellsize = TextUtils.GetTextStringRealSize(chatcontent, chatmessagecell.TxtChatContent);
                cellsize.x = chatprefabsize.x;
                // 25 is TxtChatContent offset
                if (cellsize.y - 25 <= chatprefabsize.y)
                {
                    cellsize.y = chatprefabsize.y;
                }
                cellsizelist.Add(cellsize);
            }
            else
            {
                // Emoji Chat
                cellsizelist.Add(chatprefabsize);
            }
        }
        ChatMessageListContainer.setCellDatasByDataList(prefabindexlist, cellsizelist);
        tc.End();
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
        var toptobottomcell = cellinstance.GetComponent<ChatMessageCell>();
        if (toptobottomcell == null)
        {
            toptobottomcell = cellinstance.AddComponent<ChatMessageCell>();
        }
        toptobottomcell.init(cellindex, cellindex % 3 == 2, mChatContentList[cellindex]);
    }
}