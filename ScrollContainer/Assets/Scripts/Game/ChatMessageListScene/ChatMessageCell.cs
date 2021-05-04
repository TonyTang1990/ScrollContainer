/*
 * Description:             ChatMessageCell.cs
 * Author:                  TONYTANG
 * Create Date:             2021/04/29
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ChatMessageCell.cs
/// 聊天单元格脚本
/// </summary>
public class ChatMessageCell : MonoBehaviour
{
    /// <summary>
    /// TxtChatContent
    /// </summary>
    [Header("聊天内容文本")]
    public Text TxtChatContent;

    /// <summary>
    /// ImgChatEmoji
    /// </summary>
    [Header("聊天表情图片")]
    public Image ImgChatEmoji;

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="cellindex"></param>
    /// <param name="useemoji"></param>
    /// <param name="chatcontent"></param>
    public void init(int cellindex, bool useemoji, string chatcontent)
    {
        TxtChatContent.gameObject.SetActive(!useemoji);
        ImgChatEmoji.gameObject.SetActive(useemoji);
        TxtChatContent.text = chatcontent;
    }
}