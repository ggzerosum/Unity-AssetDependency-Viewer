using System;
using ProvisGames.Core.AssetDependency.Utility;
using ProvisGames.Core.AssetDependency.View;
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

        [MenuItem("Assets/Show Dependency of selected Asset", false, 50)]
        public static void ShowDependencyOfAssetRefer()
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

        [MenuItem("Assets/Show Dependency of selected Asset", true, 50)]
        public static bool ValidateShowDependencyOfAssetRefer()
        {
            return Selection.transforms.Length == 0; // scene object is not allowed
        }

        private void OnGUI()
        {
            DrawMenu();
            DrawTree();
            Debugger.Log("Draw GUI");
        }
        private void OnEnable()
        {
            m_Instance = this; // internal usage singleton
            Debugger.Log("Enabled");
        }
        private void OnDisable()
        {
            if (m_Instance == this) // internal usage singleton
                m_Instance = null;

            Debugger.Log("Disabled");
        }
        private void OnFocus()
        {
            Debugger.Log("Focused");
        }
        private void OnLostFocus()
        {
            Debugger.Log("Lost Focus");
        }
    }

    partial class AssetDependencyViewer
    {
        private static AssetDependencyViewer m_Instance { get; set; } = null;
        private bool m_isInitialized = false;
        private SearchField m_SearchField;
        private string m_searchText = string.Empty;

        private TreeViewState m_TreeViewState;
        private DependencyTreeView m_TreeView;

        #region Initialization
        private void Initialize(bool overwrite)
        {
            // prevent duplicated Initialize
            if (m_isInitialized && !overwrite)
                return;

            if (m_TreeViewState == null) // m_TreeView State
                m_TreeViewState = new TreeViewState();

            m_TreeView = new DependencyTreeView(m_TreeViewState); // m_TreeView

            m_SearchField = new SearchField(); // Searching tool bar
            m_SearchField.downOrUpArrowKeyPressed += m_TreeView.SetFocusAndEnsureSelectedItem;
            m_isInitialized = true;
            Debugger.Log("Initialized");
        }
        private void ShowDependency(UnityEngine.Object asset)
        {
            if (asset == null)
                throw new ArgumentException("Asset does not be provided");

            if (m_TreeView?.AddAsset(asset) ?? false)
            {
                m_TreeView?.Reload();
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

                    m_TreeView.Reload();
                }

                m_TreeView.searchString = m_SearchField.OnToolbarGUI(m_TreeView.searchString);
            }
        }

        private void DrawTree()
        {
            if (m_TreeView == null)
                return;

            Rect lastRect = GUILayoutUtility.GetLastRect();
            m_TreeView.OnGUI(new Rect(
                new Vector2(lastRect.x, lastRect.y + lastRect.height),
                new Vector2(position.width, position.height - lastRect.y))
            );
        }
        #endregion
    }
}