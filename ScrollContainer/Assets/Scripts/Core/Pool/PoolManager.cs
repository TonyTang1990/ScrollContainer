/*
 * Description:             PoolManager.cs
 * Author:                  TONYTANG
 * Create Date:             2018/12/31
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 全局GameObject缓存单例管理类
/// </summary>
public class PoolManager : SingletonTemplate<PoolManager> {

    /// <summary>
    /// 对象池重用Map
    /// Key为GameObject名字
    /// Value为可重用的对象池对象列表
    /// </summary>
    private Dictionary<string, List<GameObject>> mObjectPoolMap;

    public PoolManager()
    {
        mObjectPoolMap = new Dictionary<string, List<GameObject>>();
    }

    /// <summary>
    /// 缓存特定实例对象到对象池
    /// </summary>
    /// <param name="goname">缓存对象名字</param>
    /// <param name="instance">放入对象池的实例对象</param>
    public void push(string goname, GameObject instance)
    {
        if(instance == null)
        {
            Debug.LogError("不能缓存为null的实例对象!");
            return;
        }
        else
        {
            if (instance.activeSelf || instance.activeInHierarchy)
            {
                Debug.LogError("当前实例对象正在场景里显示!不能被缓存重用!");
                return;
            }
        }

        if (mObjectPoolMap.ContainsKey(goname))
        {
            mObjectPoolMap[goname].Add(instance);
        }
        else
        {
            var objectlist = new List<GameObject>();
            objectlist.Add(instance);
            mObjectPoolMap.Add(goname, objectlist);
        }
    }

    /// <summary>
    /// 返回可用的实例对象
    /// </summary>
    /// <param name="goname">缓存对象名字</param>
    /// <param name="prefab">预制件对象</param>
    /// <returns></returns>
    public GameObject pop(string goname, GameObject prefab)
    {
        if(prefab == null)
        {
            Debug.LogError("不能弹出为null的实例对象!");
            return null;
        }
        else
        {
            if (mObjectPoolMap.ContainsKey(goname) && mObjectPoolMap[goname].Count > 0)
            {
                var instance = mObjectPoolMap[goname][0];
                mObjectPoolMap[goname].RemoveAt(0);
                return instance;
            }
            else
            {
                prefab.SetActive(false);
                var instance = GameObject.Instantiate(prefab) as GameObject;
                return instance;
            }
        }
    }
    
    /// <summary>
    /// 清除特定预制件所缓存的实例对象
    /// </summary>
    /// <param name="goname">缓存对象名字</param>
    public void clear(string goname)
    {
        if(mObjectPoolMap.ContainsKey(goname))
        {
            for(int i = 0; i < mObjectPoolMap[goname].Count; i++)
            {
                GameObject.Destroy(mObjectPoolMap[goname][i]);
                mObjectPoolMap[goname][i] = null;
            }
            mObjectPoolMap[goname] = null;
            mObjectPoolMap.Remove(goname);
        }
        else
        {
            Debug.LogError(string.Format("找不到GameObject : {0}的缓存对象！", goname));
        }
    }

    /// <summary>
    /// 清除所有缓存的对象
    /// </summary>
    public void clearAll()
    {
        foreach(var objectlist in mObjectPoolMap)
        {
            for(int i = 0; i < objectlist.Value.Count; i++)
            {
                GameObject.Destroy(objectlist.Value[i]);
                objectlist.Value[i] = null;
            }
            objectlist.Value.Clear();
        }
        mObjectPoolMap.Clear(); ;
    }
}
