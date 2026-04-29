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
        BtnBackToMainMenu.onClick.AddListener(OnBtnBackToMainMenu);
        ChatMessageListContainer.BindContainerCallBack(OnCellShow);
        mChatContentList = new List<string>();
        List<int> prefabIndexList = new List<int>();
        List<Vector2> cellSizeList = new List<Vector2>();
        var chatLeftPrefabTemplate = ChatMessageListContainer.GetPrefabIndexTemplate(0);
        var chatLeftPrefabTemplateSize = chatLeftPrefabTemplate.GetComponent<RectTransform>().rect.size;
        var chatRightPrefabTemplate = ChatMessageListContainer.GetPrefabIndexTemplate(1);
        var chatRightPrefabTemplateSize = chatRightPrefabTemplate.GetComponent<RectTransform>().rect.size;

        TimeCounter tc = new TimeCounter();
        tc.Start("ChatContainer");
        for (int i = 0; i < 20; i++)
        {
            var prefabIndex = i % 2;
            prefabIndexList.Add(prefabIndex);
            var chatContent = $"Cell Index:{i}。";
            for (int j = 0; j < i; j++)
            {
                chatContent += $"Chat Content Index:{j}.";
            }
            mChatContentList.Add(chatContent);
            // Content Chat
            var chatPrefab = prefabIndex == 0 ? chatLeftPrefabTemplate : chatRightPrefabTemplate;
            var chatPrefabSize = prefabIndex == 0 ? chatLeftPrefabTemplateSize : chatRightPrefabTemplateSize;
            if (i % 3 != 2)
            {
                var chatMessageCell = chatPrefab.GetComponent<ChatMessageCell>();
                var cellSize = TextUtils.GetTextStringRealSize(chatContent, chatMessageCell.TxtChatContent);
                cellSize.x = chatPrefabSize.x;
                // 25 is TxtChatContent offset
                if (cellSize.y - 25 <= chatPrefabSize.y)
                {
                    cellSize.y = chatPrefabSize.y;
                }
                cellSizeList.Add(cellSize);
            }
            else
            {
                // Emoji Chat
                cellSizeList.Add(chatPrefabSize);
            }
        }
        ChatMessageListContainer.SetCellDatas(prefabIndexList, cellSizeList);
        tc.End();
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
        var chatMessageCell = cellInstance.GetComponent<ChatMessageCell>();
        if (chatMessageCell == null)
        {
            chatMessageCell = cellInstance.AddComponent<ChatMessageCell>();
        }
        chatMessageCell.init(cellIndex, cellIndex % 3 == 2, mChatContentList[cellIndex]);
    }
}