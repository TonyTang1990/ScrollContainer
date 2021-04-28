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