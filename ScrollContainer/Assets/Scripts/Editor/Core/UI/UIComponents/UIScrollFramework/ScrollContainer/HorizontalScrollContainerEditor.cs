/*
 * Description:             HorizontalScrollContainerEditor.cs
 * Author:                  TONYTANG
 * Create Date:             2020/02/29
 */

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TH.Modules.UI
{
    [CustomEditor(typeof(HorizontalScrollContainer))]
    [CanEditMultipleObjects]
    /// <summary>
    /// HorizontalScrollContainerEditor.cs
    /// 自定义横向单元格面板
    /// </summary>
    public class HorizontalScrollContainerEditor : Editor
    {
        /// <summary>
        /// 模拟单元格数量
        /// </summary>
        private int mSimulationCellNumber;

        /// <summary>
        /// 模拟单元格预制件索引
        /// </summary>
        private int mSimulationCellPrefabIndex;

        /// <summary>
        /// 目标测试单元格对象
        /// </summary>
        private HorizontalScrollContainer mTargetHContainer;
        
        [UnityEditor.MenuItem("GameObject/UI/ScrollContainer/HorizontalScrollContainer", priority = 1)]
        private static void AddNewHorizontalContainer(MenuCommand command)
        {
            GameObject go = command.context as GameObject;
            var hcontainergo = UIUtilitiesEditor.AddComponent<HorizontalScrollContainer>(go).gameObject;
            hcontainergo.name = "HScrollContainer";
            var childcontent = new GameObject("Content");
            childcontent.AddComponent<RectTransform>();
            childcontent.transform.SetParent(hcontainergo.transform, false);
        }

        void OnEnable()
        {
            mTargetHContainer = target as HorizontalScrollContainer;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            serializedObject.Update();
            serializedObject.ApplyModifiedProperties();
            EditorGUILayout.BeginVertical("box");
            mSimulationCellNumber = EditorGUILayout.IntField("模拟单元格数量:", mSimulationCellNumber);
            mSimulationCellPrefabIndex = EditorGUILayout.IntField("模拟单元格预制件索引:", mSimulationCellPrefabIndex);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("模拟单元格创建"))
            {
                mTargetHContainer.ResetCorrectStatus();
                mTargetHContainer.Awake();
                mTargetHContainer.Start();
                Debug.Assert(mSimulationCellNumber > 0 && mSimulationCellPrefabIndex < mTargetHContainer.SourcePrefabList.Count, $"模拟单元格测试不允许数量<=0或者设置预制件对象索引:{mSimulationCellPrefabIndex}大于等于最大预制件源数据长度:{mTargetHContainer.SourcePrefabList.Count}!");
                Debug.Log($"模拟测试单元格创建数量:{mSimulationCellNumber}");
                var originalactiveself = mTargetHContainer.SourcePrefabList[mSimulationCellPrefabIndex].activeSelf;
                mTargetHContainer.SourcePrefabList[mSimulationCellPrefabIndex].SetActive(true);
                mTargetHContainer.setCellDatasByCellCount(mSimulationCellNumber);
                mTargetHContainer.SourcePrefabList[mSimulationCellPrefabIndex].SetActive(originalactiveself);
            }
            if (GUILayout.Button("销毁模拟单元格"))
            {
                mTargetHContainer.setCellDatas(null);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }
    }
}