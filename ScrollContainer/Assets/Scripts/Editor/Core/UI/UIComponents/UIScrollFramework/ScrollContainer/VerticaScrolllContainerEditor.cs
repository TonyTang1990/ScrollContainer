/*
 * Description:             VerticalScrollContainerEditor.cs
 * Author:                  TONYTANG
 * Create Date:             2020/02/29
 */

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TH.Modules.UI
{
    [CustomEditor(typeof(VerticalScrollContainer))]
    [CanEditMultipleObjects]
    /// <summary>
    /// VerticalScrollContainerEditor.cs
    /// 自定义横向单元格面板
    /// </summary>
    public class VerticalScrollContainerEditor : Editor
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
        private VerticalScrollContainer mTargetVContainer;

        [UnityEditor.MenuItem("GameObject/UI/ScrollContainer/VerticalScrollContainer", priority = 3)]
        private static void AddNewVerticalContainer(MenuCommand command)
        {
            GameObject go = command.context as GameObject;
            var vcontainergo = UIUtilitiesEditor.AddComponent<VerticalScrollContainer>(go).gameObject;
            vcontainergo.name = "NewVContainer";
            var childcontent = new GameObject("Content");
            childcontent.AddComponent<RectTransform>();
            childcontent.transform.SetParent(vcontainergo.transform, false);
        }

        void OnEnable()
        {
            mTargetVContainer = target as VerticalScrollContainer;
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
                mTargetVContainer.ResetCorrectStatus();
                mTargetVContainer.Awake();
                mTargetVContainer.Start();
                Debug.Assert(mSimulationCellNumber > 0 && mSimulationCellPrefabIndex < mTargetVContainer.SourcePrefabList.Count, $"模拟单元格测试不允许数量<=0或者设置预制件对象索引:{mSimulationCellPrefabIndex}大于等于最大预制件源数据长度:{mTargetVContainer.SourcePrefabList.Count}!");
                Debug.Log($"模拟测试单元格创建数量:{mSimulationCellNumber}");
                var originalactiveself = mTargetVContainer.SourcePrefabList[mSimulationCellPrefabIndex].activeSelf;
                mTargetVContainer.SourcePrefabList[mSimulationCellPrefabIndex].SetActive(true);
                mTargetVContainer.setCellDatasByCellCount(mSimulationCellNumber);
                mTargetVContainer.SourcePrefabList[mSimulationCellPrefabIndex].SetActive(originalactiveself);
            }
            if (GUILayout.Button("销毁模拟单元格"))
            {
                mTargetVContainer.setCellDatas(null);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }
    }
}