using System;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace ProvisGames.Core.Utility
{
    /// <summary>
    /// Show Selected Asset's Dependency
    /// </summary>
    public partial class AssetDependencyViewer : EditorWindow
    {
        string reloadLabel = "Reload Tree";
        [MenuItem("Assets/Open Asset Dependency Viewer", false, 50)]
        public static void Open()
        {
            AssetDependencyViewer viewPort = GetWindow<AssetDependencyViewer>("AssetDependencyViewer");
            viewPort.Initialize(false);
            viewPort.Show();
        }

        [MenuItem("Assets/Show Dependency of Asset", false, 50)]
        public static void Select()
        {
            if (Selection.transforms.Length > 0)
            {
                EditorUtility.DisplayDialog("Causation", "Only Can see Dependency of Asset", "OK");
                return;
            }

            string assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (string.IsNullOrEmpty(assetPath) || !AssetDatabase.IsMainAsset(Selection.activeObject))
            {
                EditorUtility.DisplayDialog("Can not find asset", "none exist Asset or should be Main Asset(Root Asset)", "OK");
                return;
            }

            if (m_Instance == null)
            {
                Open();

                if (m_Instance == null)
                    throw new NullReferenceException("Cannot find Window already opened");
            }

            foreach (UnityEngine.Object o in Selection.objects)
            {
                m_Instance.ShowDependency(o);
            }
        }

        [MenuItem("Assets/Show Dependency of Asset", true, 50)]
        public static bool ValidateSelect()
        {
            return Selection.transforms.Length == 0; // scene object is not allowed
        }

        private void OnGUI()
        {
            DrawMenu();
            DrawTree();
            Debug.Log("Draw GUI");
        }

        private void OnEnable()
        {
            m_Instance = this; // internal usage singleton
            Debug.Log("Enabled");
        }

        private void OnDisable()
        {
            if (m_Instance == this) // internal usage singleton
                m_Instance = null;

            Debug.Log("Disabled");
        }

        private void OnFocus()
        {
            Debug.Log("Focused");
        }

        private void OnLostFocus()
        {
            Debug.Log("Lost Focus");
        }
    }

    partial class AssetDependencyViewer
    {
        private static AssetDependencyViewer m_Instance { get; set; } = null;
        private bool m_isInitialized = false;
        private string m_searchText = string.Empty;

        private TreeViewState treeViewState;
        private DependencyTreeView treeView;

        #region Initialization
        private void Initialize(bool overwrite)
        {
            // prevent duplicated Initialize
            if (m_isInitialized && !overwrite)
                return;

            if (treeViewState == null)
                treeViewState = new TreeViewState();

            treeView = new DependencyTreeView(treeViewState);

            m_isInitialized = true;
            Debug.Log("Initialized");
        }
        private void ShowDependency(UnityEngine.Object asset)
        {
            if (asset == null)
                throw new ArgumentException("Asset does not be provided");

            if (treeView?.AddAsset(asset) ?? false)
            {
                treeView?.Reload();
            }
        }

        #endregion

        #region Draw GUI
        private void DrawMenu()
        {
            // Draw Search tool bar
            using (var horizontalScope = new EditorGUILayout.HorizontalScope(EditorStyles.toolbar, GUILayout.ExpandWidth(true)))
            {
                // Draw Force Reload Button
                Vector2 labelSize = GUI.skin.label.CalcSize(new GUIContent(reloadLabel));
                if (GUILayout.Button(reloadLabel, EditorStyles.toolbarButton, GUILayout.MinWidth(labelSize.x), GUILayout.MaxWidth(labelSize.x)))
                {

                    treeView.Reload();
                }

                using (var changeFlag = new EditorGUI.ChangeCheckScope())
                {
                    m_searchText = EditorGUILayout.DelayedTextField(m_searchText, EditorStyles.toolbarSearchField, GUILayout.ExpandWidth(true));

                    if (changeFlag.changed)
                    {
                        Debug.Log("Search Filter Changed");
                    }
                }
            }
        }

        private void DrawTree()
        {
            if (treeView == null)
                return;

            Rect lastRect = GUILayoutUtility.GetLastRect();
            treeView.OnGUI(new Rect(
                new Vector2(lastRect.x, lastRect.y + lastRect.height),
                new Vector2(position.width, position.height - lastRect.y))
            );
        }
        #endregion
    }
}