/*
 * Description:             VerticalGridScrollContainerEditor.cs
 * Author:                  TONYTANG
 * Create Date:             2020/02/29
 */

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TH.Modules.UI
{
    [CustomEditor(typeof(VerticalGridScrollContainer))]
    [CanEditMultipleObjects]
    /// <summary>
    /// VerticalGridScrollContainerEditor.cs
    /// 自定义横向单元格面板
    /// </summary>
    public class VerticalGridScrollContainerEditor : Editor
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
        private VerticalGridScrollContainer mTargetVGContainer;

        [UnityEditor.MenuItem("GameObject/UI/ScrollContainer/VerticalGridScrollContainer", priority = 4)]
        private static void AddNewVerticalGridContainer(MenuCommand command)
        {
            GameObject go = command.context as GameObject;
            var vgcontainergo = UIUtilitiesEditor.AddComponent<VerticalGridScrollContainer>(go).gameObject;
            vgcontainergo.name = "NewVGContainer";
            var childcontent = new GameObject("Content");
            childcontent.AddComponent<RectTransform>();
            childcontent.transform.SetParent(vgcontainergo.transform, false);
        }

        void OnEnable()
        {
            mTargetVGContainer = target as VerticalGridScrollContainer;
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.BeginHorizontal();
            var originalcolor = GUI.color;
            GUI.color = Color.yellow;
            GUILayout.Label("竖向网格单元格容器不支持不同大小的单元格!", "Box", GUILayout.ExpandWidth(true));
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
                mTargetVGContainer.ResetCorrectStatus();
                mTargetVGContainer.Awake();
                mTargetVGContainer.Start();
                Debug.Assert(mSimulationCellNumber > 0 && mSimulationCellPrefabIndex < mTargetVGContainer.SourcePrefabList.Count, $"模拟单元格测试不允许数量<=0或者设置预制件对象索引:{mSimulationCellPrefabIndex}大于等于最大预制件源数据长度:{mTargetVGContainer.SourcePrefabList.Count}!");
                Debug.Log($"模拟测试单元格创建数量:{mSimulationCellNumber}");
                var originalactiveself = mTargetVGContainer.SourcePrefabList[mSimulationCellPrefabIndex].activeSelf;
                mTargetVGContainer.SourcePrefabList[mSimulationCellPrefabIndex].SetActive(true);
                mTargetVGContainer.setCellDatasByCellCount(mSimulationCellNumber);
                mTargetVGContainer.SourcePrefabList[mSimulationCellPrefabIndex].SetActive(originalactiveself);
            }
            if (GUILayout.Button("销毁模拟单元格"))
            {
                mTargetVGContainer.setCellDatas(null);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }
    }
}