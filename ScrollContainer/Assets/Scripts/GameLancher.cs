/*
 * Description:             GameLancher.cs
 * Author:                  TONYTANG
 * Create Date:             2021/04/25
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// GameLancher.cs
/// 游戏启动脚本
/// </summary>
[DisallowMultipleComponent]
public class GameLancher : MonoBehaviour
{
    /// <summary>
    /// 单例对象
    /// </summary>
    public static GameLancher Singleton;

    /// <summary>
    /// UI根节点
    /// </summary>
    [Header("UI根节点")]
    public Transform UIRoot;

    /// <summary>
    /// UI挂载节点
    /// </summary>
    [Header("UI挂载节点")]
    public Transform UIParent;

    void Start()
    {
        if(Singleton == null)
        {
            Singleton = this;
        }
        else
        {
            Debug.LogError($"重复挂载GameLancher脚本,请检查代码!");
        }
        DontDestroyOnLoad(gameObject);
    }


}