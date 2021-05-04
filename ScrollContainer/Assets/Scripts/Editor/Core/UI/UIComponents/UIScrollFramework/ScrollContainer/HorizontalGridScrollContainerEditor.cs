/*
 * Description:             HorizontalGridScrollContainerEditor.cs
 * Author:                  TONYTANG
 * Create Date:             2020/02/29
 */

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TH.Modules.UI
{
    [CustomEditor(typeof(HorizontalGridScrollContainer))]
    [CanEditMultipleObjects]
    /// <summary>
    /// HorizontalGridScrollContainerEditor.cs
    /// 自定义横向单元格面板
    /// </summary>
    public class HorizontalGridScrollContainerEditor : Editor
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
        private HorizontalGridScrollContainer mTargetHGContainer;
        
        [UnityEditor.MenuItem("GameObject/UI/ScrollContainer/HorizontalGridScrollContainer", priority = 2)]
        private static void AddHorizontalGridScrollContainer(MenuCommand command)
        {
            GameObject go = command.context as GameObject;
            var hgcontainergo = UIUtilitiesEditor.AddComponent<HorizontalGridScrollContainer>(go).gameObject;
            hgcontainergo.name = "HGScrollContainer";
            var childcontent = new GameObject("Content");
            childcontent.AddComponent<RectTransform>();
            childcontent.transform.SetParent(hgcontainergo.transform, false);
        }

        void OnEnable()
        {
            mTargetHGContainer = target as HorizontalGridScrollContainer;
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.BeginHorizontal();
            var originalcolor = GUI.color;
            GUI.color = Color.yellow;
            GUILayout.Label("横向网格单元格容器不支持不同大小的单元格!", "Box", GUILayout.ExpandWidth(true));
            GUI.color = originalcolor;
            EditorGUILayout.EndHorizontal();
            base.OnInspectorGUI();
            serializedObject.Update();
            serializedObject.ApplyModifiedProperties();
            EditorGUILayout.BeginVertical("box");
            mSimulationCellNumber = EditorGUILayout.IntField("模拟单元格数量:", mSimulationCellNumber);
            mSimulationCellPrefabIndex = EditorGUILayout.IntField("模拟单元格预制件索引:", mSimulationCellPrefabIndex);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("模拟单元格创建"))
            {
                mTargetHGContainer.ResetCorrectStatus();
                mTargetHGContainer.Awake();
                mTargetHGContainer.Start();
                Debug.Assert(mSimulationCellNumber > 0 && mSimulationCellPrefabIndex < mTargetHGContainer.SourcePrefabList.Count, $"模拟单元格测试不允许数量<=0或者设置预制件对象索引:{mSimulationCellPrefabIndex}大于等于最大预制件源数据长度:{mTargetHGContainer.SourcePrefabList.Count}!");
                Debug.Log($"模拟测试单元格创建数量:{mSimulationCellNumber}");
                var originalactiveself = mTargetHGContainer.SourcePrefabList[mSimulationCellPrefabIndex].activeSelf;
                mTargetHGContainer.SourcePrefabList[mSimulationCellPrefabIndex].SetActive(true);
                mTargetHGContainer.setCellDatasByCellCount(mSimulationCellNumber);
                mTargetHGContainer.SourcePrefabList[mSimulationCellPrefabIndex].SetActive(originalactiveself);
            }
            if (GUILayout.Button("销毁模拟单元格"))
            {
                mTargetHGContainer.setCellDatas(null);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }
    }
}