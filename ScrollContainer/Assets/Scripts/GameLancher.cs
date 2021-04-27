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
public class GameLancher : MonoBehaviour
{
    void Start()
    {
        DontDestroyOnLoad(gameObject);  
    }


}