using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// FPS显示组件
/// </summary>
public class FPSDisplay : SingletonMonoBehaviourTemplate<FPSDisplay>
{
    /// <summary> FPS显示开关 /// </summary>
    public bool mEnableFPS = true;

    /// <summary> FPS更新间隔 /// </summary>
    public float mFPSInterval = 1.0f;

    /// <summary> 前一次FPS判定更新间隔，用于判定是否动态修改了mFPSInterval值，好更新mCurrentWFS /// </summary>
    private float mPreFPSInterval;

    /// <summary> 当前FPS更新需要等待的携程等待对象(缓存是为了避免每次去new) /// </summary>
    private WaitForSeconds mCurrentWFS;

    /// <summary> FPS Delta时间记录 /// </summary>
    private float mDeltaTime = 0.0f;

    /// <summary> 当前FPS /// </summary>
    private float mFPS = 0.0f;

    /// <summary> 当前帧率的ms /// </summary>
    private float mMSec = 0.0f;

    /// <summary> 当前帧率 /// </summary>
    private int mFrameCount = 0;

    /// <summary> FPS UI显示风格 /// </summary>
    private GUIStyle mFPSStyle;

    void Awake()
    {
        mFPSStyle = new GUIStyle();
        mFPSStyle.fontSize = 20;
        mFPSStyle.normal.textColor = Color.red;
        mCurrentWFS = new WaitForSeconds(mFPSInterval);
        mPreFPSInterval = mFPSInterval;
        StartCoroutine(fpsDisplay());
    }

    void Update()
    {
        if (mEnableFPS)
        {
            mDeltaTime += Time.deltaTime;
            mFrameCount++;
        }
        else
        {
            mDeltaTime = 0.0f;
            mFrameCount = 0;
        }
    }

    void OnGUI()
    {
        if (mEnableFPS)
        {
            GUI.Label(new Rect(Screen.width - 200, Screen.height - 40, 200, 40), string.Format("{0:0.0} ms ({1:0.} fps)", mMSec, mFPS), mFPSStyle);
        }
    }

    /// <summary>
    /// FPS计算携程
    /// </summary>
    /// <returns></returns>
    IEnumerator fpsDisplay()
    {
        while (true)
        {
            if (mEnableFPS)
            {
                //说明mFPSInterval有变化
                if (mPreFPSInterval != mFPSInterval)
                {
                    Debug.Log("mFPSInterval值更新 : " + mFPSInterval);
                    mCurrentWFS = new WaitForSeconds(mFPSInterval);
                }
                mPreFPSInterval = mFPSInterval;
                yield return mCurrentWFS;
                mFPS = mFrameCount / mFPSInterval;
                mMSec = 1.0f / mFPS * 1000.0f;
                mDeltaTime = 0.0f;
                mFrameCount = 0;
            }
            else
            {
                yield return new WaitForSeconds(mFPSInterval);
            }
        }
    }
}