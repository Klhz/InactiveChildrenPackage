using System.Collections.Generic;

using UnityEditor;

using UnityEngine.SceneManagement;
using UnityEngine;

namespace CleanUpScene.Editor
{
    public class SceneCleaner : EditorWindow
    {
        #region Publics

        public struct ObjectToScan
        {
            public GameObject m_gameObject;
            public bool m_isActive;
            public bool m_isSelectedForCleanup;

            public int m_indexLevel;
        }


        [MenuItem("Tools/Scene Cleaner")]
        public static void ShowWindow()
        {

            EditorWindow window = GetWindow<SceneCleaner>("Scene Cleaner");
            window.minSize = new Vector2(400, 400);

        }

        #endregion


        #region UnityAPI

        private void OnGUI()
        {
            GUILayout.Label("Hierarchy Cleaner", EditorStyles.boldLabel);
            if (GUILayout.Button("Scan Scene"))
                GetAllParentsInScene();
            EditorGUILayout.Space(3);

            if (Selection.activeObject == null) EditorGUILayout.HelpBox("No object selected", MessageType.Info);
            else EditorGUILayout.ObjectField($"Selected Object + {Selection.activeObject.name}", Selection.activeObject, typeof(Object), false);

            EditorGUILayout.Space(3);
            //_scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            //EditorGUILayout.EndScrollView();

            for (int i = 0; i < _entriesList.Count; i++)
            {
                ObjectToScan entry = _entriesList[i];
                if (entry.m_gameObject == null)
                    continue;
                string indent = new string(' ', entry.m_indexLevel * 4);
                string label = indent + entry.m_gameObject.name;
                Color previousColor = GUI.color;
                GUI.color = entry.m_isActive ? Color.white : Color.gray;
                if (entry.m_isActive) EditorGUILayout.LabelField(label);
                else
                {
                    entry.m_isSelectedForCleanup = EditorGUILayout.ToggleLeft(
                        label,
                        entry.m_isSelectedForCleanup
                    );

                }

                GUI.color = previousColor;
                _entriesList[i] = entry;
            }

            if (GUILayout.Button("Remove Selected Objects"))
                RemoveSelectedObjects();

            if (GUILayout.Button("Remove Inactive Objects"))
                RemoveInactiveObjects();
        }

        private void RemoveInactiveObjects()
        {
            foreach (ObjectToScan entry in _entriesList)
            {
                if (entry.m_gameObject == null)
                    continue;
                if (!entry.m_isActive)
                    DestroyImmediate(entry.m_gameObject);
            }
        }

        private void RemoveSelectedObjects()
        {
            foreach (ObjectToScan entry in _entriesList)
            {
                if (entry.m_gameObject == null)
                    continue;
                if (entry.m_isSelectedForCleanup)
                    DestroyImmediate(entry.m_gameObject);
            }
        }

        private void OnSelectionChange()
        {
            if (Selection.activeGameObject == null) return;
            GetParentsAndChildrenFrom(Selection.activeGameObject);
        }

        private void OnEnable()
        {
            GetAllParentsInScene();
        }

        #endregion


        #region Main API

        private void GetAllParentsInScene()
        {
            _entriesList.Clear();
            foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
                _entriesList.AddRange(GetParentsAndChildrenFrom(root));
            Repaint();
        }

        private List<ObjectToScan> GetParentsAndChildrenFrom(GameObject parent)
        {
            List<ObjectToScan> childrenList = new List<ObjectToScan>();
            ObjectToScan entry = new ObjectToScan
            {
                m_gameObject = parent,
                m_isActive = parent.activeSelf,
                m_isSelectedForCleanup = !parent.activeSelf,
                m_indexLevel = 0
            };

            childrenList.Add(entry);

            if (!entry.m_isActive) RecursiveLoopGetChildrenObjectsFrom(parent, childrenList, 0);
            return childrenList;

        }

        private List<ObjectToScan> RecursiveLoopGetChildrenObjectsFrom(GameObject parent, List<ObjectToScan> childrenList, int depth)
        {
            depth++;

            for (int i = 0; i < parent.transform.childCount; i++)
            {
                GameObject child = parent.transform.GetChild(i).gameObject;

                ObjectToScan entry = new ObjectToScan
                {
                    m_gameObject = child,
                    m_isActive = child.activeSelf,
                    m_isSelectedForCleanup = !child.activeSelf,
                    m_indexLevel = depth
                };
                childrenList.Add(entry);

                RecursiveLoopGetChildrenObjectsFrom(child, childrenList, depth);

            }
            return childrenList;
        }

        #endregion


        #region Private and Protected

        private List<ObjectToScan> _entriesList = new List<ObjectToScan>();

        #endregion
    }
}