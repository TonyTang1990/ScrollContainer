/*
 * Description:             GameObjectPool.cs
 * Author:                  TANGHUAN
 * Create Date:             2019/05/26
 */

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 局部GameObject对象池
/// </summary>
public class GameObjectPool
{
    /// <summary>
    /// GameObject对象池策略
    /// </summary>
    public enum EPoolStrategy
    {
        CanvasGroup = 1,                            // CanvasGroup设置.a = 0的缓存形式(UWA官方推荐方式，避免SetActive带来的开销)
        Active,                                     // SetActive(true or false)的缓存形式
    }

    /// <summary>
    /// 对象池根节点
    /// </summary>
    private static GameObject mGameObjectPoolRoot;

    /// <summary>
    /// 对象池默认缓存策略
    /// </summary>
    private EPoolStrategy mGameObjectPoolStrategy;

    /// <summary>
    /// 对象池重用Map
    /// Key为模板对象的InstanceID
    /// Value为可重用的对象池对象列表
    /// </summary>
    private Dictionary<int, List<GameObject>> mGameObjectPoolMap;

    /// <summary>
    /// 当前对象池挂载节点
    /// </summary>
    private Transform mCurrentGameObjectPoolParent;

    /// <summary>
    /// 父挂载节点名字
    /// </summary>
    private string mParentName;

    /// <summary>
    /// 默认父节点挂载名
    /// </summary>
    private const string mDefaultParentName = "DefaultPoolParent";

    /// <summary>
    /// 强制必须走带参构造函数
    /// </summary>
    private GameObjectPool()
    {

    }

    public GameObjectPool(string parentname, EPoolStrategy poolstrategy = EPoolStrategy.CanvasGroup)
    {
        if (mGameObjectPoolRoot == null)
        {
            mGameObjectPoolRoot = new GameObject("GameObjectPoolRoot");
            Object.DontDestroyOnLoad(mGameObjectPoolRoot);
        }
        mParentName = string.IsNullOrEmpty(parentname) == false ? parentname : mDefaultParentName;
        mGameObjectPoolStrategy = poolstrategy;
        mGameObjectPoolMap = new Dictionary<int, List<GameObject>>();
        mCurrentGameObjectPoolParent = mGameObjectPoolRoot.transform.Find(mParentName);
        mCurrentGameObjectPoolParent = mCurrentGameObjectPoolParent != null ? mCurrentGameObjectPoolParent : new GameObject(mParentName).transform;
        mCurrentGameObjectPoolParent.SetParent(mGameObjectPoolRoot.transform, false);
        if (mGameObjectPoolStrategy == EPoolStrategy.CanvasGroup)
        {
            if (mCurrentGameObjectPoolParent.gameObject.GetComponent<CanvasGroup>() == null)
            {
                var canvasgroup = mCurrentGameObjectPoolParent.gameObject.AddComponent<CanvasGroup>();
                canvasgroup.alpha = 0;
                canvasgroup.interactable = false;
                canvasgroup.blocksRaycasts = false;
            }
        }
    }

    /// <summary>
    /// 初始化缓存指定数量的实例对象进池
    /// </summary>
    /// <param name="template">模板对象</param>
    /// <param name="number">初始化数量</param>
    public void init(GameObject template, int number)
    {
        if (template != null)
        {
            var instanceid = template.GetInstanceID();
            for (var i = 0; i < number; i++)
            {
                var instance = GameObject.Instantiate<GameObject>(template);
                push(instanceid, instance);
            }
        }
    }

    /// <summary>
    /// 缓存特定实例对象到对象池
    /// </summary>
    /// <param name="instanceid">模板对象对象InstanceID</param>
    /// <param name="instance">放入对象池的实例对象</param>
    public void push(int instanceid, GameObject instance)
    {
        if (instance == null)
        {
            Debug.LogError("不能缓存为null的实例对象!");
            return;
        }

        if (mGameObjectPoolStrategy == EPoolStrategy.Active)
        {
            instance.SetActive(false);
        }
        else if (mGameObjectPoolStrategy == EPoolStrategy.CanvasGroup)
        {
            if (!instance.activeSelf)
            {
                instance.SetActive(true);
            }
        }
        if(mCurrentGameObjectPoolParent != null)
        {
            instance.transform.SetParent(mCurrentGameObjectPoolParent, false);
        }
        else
        {
            // 为空的情况是游戏退出时挂载节点都为空了，会触发Unity报以外错误
        }
        if (mGameObjectPoolMap.ContainsKey(instanceid))
        {
            mGameObjectPoolMap[instanceid].Add(instance);
        }
        else
        {
            var objectlist = new List<GameObject>();
            objectlist.Add(instance);
            mGameObjectPoolMap.Add(instanceid, objectlist);
        }
    }

    /// <summary>
    /// 返回可用的实例对象
    /// </summary>
    /// <param name="template">模板对象预制件对象</param>
    /// <returns></returns>
    public GameObject pop(GameObject template)
    {
        if (template == null)
        {
            Debug.LogError("模板对象不能为空，无法弹出正确对象!");
            return null;
        }
        else
        {
            var instanceid = template.GetInstanceID();
            if (mGameObjectPoolMap.ContainsKey(instanceid) && mGameObjectPoolMap[instanceid].Count > 0)
            {
                var instance = mGameObjectPoolMap[instanceid][0];
                mGameObjectPoolMap[instanceid].RemoveAt(0);
                if (mGameObjectPoolStrategy == EPoolStrategy.Active)
                {
                    instance.SetActive(true);
                }
                return instance;
            }
            else
            {
                if (!template.activeSelf)
                {
                    template.SetActive(true);
                }
                var instance = GameObject.Instantiate(template) as GameObject;
                template.SetActive(false);
                return instance;
            }
        }
    }

    /// <summary>
    /// 清除特定预制件所缓存的实例对象
    /// </summary>
    /// <param name="instanceid">模板对象对象InstanceID</param>
    public void clear(int instanceid)
    {
        if (mGameObjectPoolMap.ContainsKey(instanceid))
        {
            for (int i = 0; i < mGameObjectPoolMap[instanceid].Count; i++)
            {
                GameObject.Destroy(mGameObjectPoolMap[instanceid][i]);
                mGameObjectPoolMap[instanceid][i] = null;
            }
            mGameObjectPoolMap[instanceid] = null;
            mGameObjectPoolMap.Remove(instanceid);
        }
        else
        {
            Debug.LogError(string.Format("找不到InstanceID : {0}的缓存对象！", instanceid));
        }
    }

    /// <summary>
    /// 清除所有缓存的对象
    /// </summary>
    public void clearAll()
    {
        foreach (var objectlist in mGameObjectPoolMap)
        {
            for (int i = 0; i < objectlist.Value.Count; i++)
            {
                if(objectlist.Value[i] != null)
                {
#if !UNITY_EDITOR
                    GameObject.Destroy(objectlist.Value[i]);
#else
                    GameObject.Destroy(objectlist.Value[i]);
#endif
                    objectlist.Value[i] = null;
                }
            }
            objectlist.Value.Clear();
        }
        mGameObjectPoolMap.Clear();
    }
}
