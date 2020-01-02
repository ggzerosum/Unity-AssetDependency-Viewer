using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ProvisGames.Core.Utility
{
    class DependencyTreeView : TreeView
    {
        public static GUIStyle lineStyle = (GUIStyle)"TV Line";
        private List<UnityEngine.Object> assets = new List<Object>();

        public DependencyTreeView(TreeViewState state):
            base(state)
        {
            Reload();
        }
        public DependencyTreeView(TreeViewState state, MultiColumnHeader multiColumnHeader):
            base(state, multiColumnHeader)
        {
            Reload();
        }

        /// <summary>
        /// Check Whether asset is already added in dependency graph list
        /// </summary>
        /// <param name="asset">asset in Project</param>
        /// <returns></returns>
        public bool HasAsset(UnityEngine.Object asset)
        {
            if (!Utility.GetAssetPathSafely(asset, out string assetPath))
            {
                throw new ArgumentException("is Not Proper Asset");
            }

            string guid = AssetDatabase.AssetPathToGUID(assetPath);
            if (!string.IsNullOrEmpty(guid) && FindAsset(guid, out int index))
            {
                return true;
            }

            return false;
        }
        /// <summary>
        /// Add Asset to dependency Graph. If you want to visualize current state of graph, call Reload function.
        /// </summary>
        /// <param name="asset">Asset in Project</param>
        /// <returns>whether adding asset in graph was succeed</returns>
        public bool AddAsset(UnityEngine.Object asset)
        {
            try
            {
                if (HasAsset(asset))
                {
                    Debug.Log("Already Registered Asset");
                    return false;
                }

                assets.Add(asset);
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
                return false;
            }

            return true;
        }
        /// <summary>
        /// Remove Asset from dependency Graph. If you want to visualize current state of graph, call Reload function.
        /// </summary>
        /// <param name="asset">Asset in Project</param>
        /// <returns>whether removing asset in graph was succeed</returns>
        public bool RemoveAsset(UnityEngine.Object asset)
        {
            try
            {
                if (!HasAsset(asset))
                    return false;

                if (!Utility.GetAssetPathSafely(asset, out string assetPath))
                {
                    Debug.LogError("Is Not Proper Asset");
                    return false;
                }

                string assetGuid = AssetDatabase.AssetPathToGUID(assetPath);
                int removedCount = this.assets.RemoveAll(element => assetGuid == AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(element)));
                Debug.Log($"Removed Count : {removedCount}");
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return false;
            }

            return true;
        }
        public void OnDestroy()
        {
            assets.Clear();
        }

        protected override TreeViewItem BuildRoot()
        {
            TreeViewItem root = new TreeViewItem {id = 0, depth = -1};
            return root;
        }
        protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
        {
            // fetch rows if exist
            var rows = GetRows() ?? new List<TreeViewItem>(50);
            rows.Clear();

            foreach (var asset in assets)
            {
                if (Utility.GetAssetPathSafely(asset, out string path))
                {
                    string[] dependencies = AssetDatabase.GetDependencies(path, false);

                    var item = CreateTreeViewAssetItem(asset, asset.GetInstanceID(), AssetDatabase.AssetPathToGUID(path), dependencies.Length);
                    root.AddChild(item);
                    rows.Add(item);

                    if (dependencies.Length > 0 && IsExpanded(item.id))
                    {
                        AddChildrenRecursive(asset, item, rows);
                    }
                    else
                    {
                        item.children = CreateChildListForCollapsedParent();
                    }
                }
            }

            SetupDepthsFromParentsAndChildren(root);
            return rows;
        }
        protected override void RowGUI(RowGUIArgs args)
        {
            Event evt = Event.current;
            extraSpaceBeforeIconAndLabel = 18f;
            Rect currentRect = args.rowRect;

            // Draw GUI
            currentRect.x += GetContentIndent(args.item);
            currentRect.width = 16f;

            string postFix = string.Empty;
            if (args.item is TreeViewAssetItem assetItem)
            {
                int dependencyCount = assetItem.assetInfo.dependencyCount;
                postFix = dependencyCount > 0 ? dependencyCount.ToString() : string.Empty;

                GUIContent guiContent = LoadAssetEditorContent(assetItem);
                GUI.DrawTexture(currentRect, guiContent.image, ScaleMode.ScaleToFit);
                currentRect.x += extraSpaceBeforeIconAndLabel;
            }

            GUIStyle style = lineStyle;
            string labelText = $"{args.label}{(string.IsNullOrEmpty(postFix) ? postFix : $" [D:{postFix}]")}";
            Vector2 labelRect = style.CalcSize(new GUIContent(labelText));
            currentRect = new Rect(currentRect.x + currentRect.width, currentRect.y, labelRect.x, labelRect.y);
            GUI.Label(currentRect, labelText, style);

            // Click Event Processing
            if (evt.type == EventType.MouseDown && args.rowRect.Contains(evt.mousePosition))
            {
                SelectionClick(args.item, false);
                EditorGUIUtility.PingObject(LoadAsset(args.item as TreeViewAssetItem));
            }
            else if (evt.type == EventType.ContextClick && args.rowRect.Contains(evt.mousePosition))
            {
                if (args.item is TreeViewAssetItem targetItem &&
                    LoadAsset(targetItem) is UnityEngine.Object loadedAsset &&
                    HasAsset(loadedAsset))
                {
                    GenericMenu genericMenu = new GenericMenu();
                    genericMenu.AddItem(new GUIContent("Remove from list"), false,
                        () =>
                        {
                            RemoveAndReload(loadedAsset);
                        });
                    genericMenu.ShowAsContext();
                }
            }
        }

        private void RemoveAndReload(UnityEngine.Object asset)
        {
            if (HasAsset(asset))
            {
                RemoveAsset(asset);
                Reload();
            }
        }
        private GUIContent LoadAssetEditorContent(TreeViewAssetItem assetItem)
        {
            return EditorGUIUtility.ObjectContent(
                LoadAsset(assetItem), 
                assetItem.assetInfo.assetType);
        }
        private Object LoadAsset(TreeViewAssetItem assetItem)
        {
            return AssetDatabase.LoadAssetAtPath(
                AssetDatabase.GUIDToAssetPath(assetItem.assetInfo.assetReferenceGuid), assetItem.assetInfo.assetType);
        }

        /// <summary>
        /// Find Asset in Cached List Which have same Reference guid
        /// </summary>
        /// <param name="assetGuid">Asset guid</param>
        /// <param name="index">index in list</param>
        /// <returns></returns>
        private bool FindAsset(string assetGuid, out int index)
        {
            int targetIndex = assets.FindIndex(element => ExtractGuid(element) == assetGuid);
            bool result = targetIndex >= 0;
            index = targetIndex;
            return result;

            // Local Function
            string ExtractGuid(UnityEngine.Object e)
            {
                string path = AssetDatabase.GetAssetPath(e);

                if (string.IsNullOrEmpty(path))
                    throw new ArgumentException("Cannot find Path of Asset");

                return AssetDatabase.AssetPathToGUID(path);
            }
        }

        private TreeViewItem CreateTreeViewAssetItem(UnityEngine.Object asset, int id, string guid, int dependencyCount)
        {
            return new TreeViewAssetItem(id, - 1, asset.name, guid, asset.GetType(), dependencyCount);
        }
        private void AddChildrenRecursive(UnityEngine.Object asset, TreeViewItem parentItem, IList<TreeViewItem> rows)
        {
            if (!Utility.GetAssetPathSafely(asset, out string assetPath))
            {
                return;
            }

            string[] dependencies = AssetDatabase.GetDependencies(assetPath, false);
            parentItem.children = new List<TreeViewItem>(dependencies.Length);

            foreach (string dependency in dependencies)
            {
                UnityEngine.Object aChildAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(dependency);
                if (aChildAsset != null)
                {
                    string[] childDependencies = AssetDatabase.GetDependencies(dependency, false);

                    var aChildItem = CreateTreeViewAssetItem(aChildAsset, aChildAsset.GetInstanceID(), AssetDatabase.AssetPathToGUID(dependency), childDependencies.Length);
                    parentItem.AddChild(aChildItem);
                    rows.Add(aChildItem);

                    if (childDependencies.Length > 0)
                    {
                        if (IsExpanded(aChildItem.id))
                        {
                            AddChildrenRecursive(aChildAsset, aChildItem, rows);
                        }
                        else
                        {
                            aChildItem.children = CreateChildListForCollapsedParent();
                        }
                    }
                }
            }
        }
    }

    class TreeViewAssetItem : TreeViewItem
    {
        public AssetInfo assetInfo { get; }

        public TreeViewAssetItem(
            int id, 
            int depth, 
            string displayName,
            string assetGuid,
            Type assetType,
            int dependencyCount
            ) : base(id, depth, displayName)
        {
            assetInfo = new AssetInfo()
            {
                assetReferenceGuid = assetGuid,
                assetType = assetType,
                dependencyCount = dependencyCount
            };
        }


        public struct AssetInfo
        {
            public string assetReferenceGuid;
            public Type assetType;
            public int dependencyCount;
        }
    }
}