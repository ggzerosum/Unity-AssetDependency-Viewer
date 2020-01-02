using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using Debug = UnityEngine.Debug;

namespace ProvisGames.Core.AssetDependency.Utility
{
    public static class EditorAssetUtility
    {
        /// <summary>
        /// Get Asset's path in unity project (under asset folder)
        /// </summary>
        /// <param name="asset">unity object</param>
        /// <param name="path">path</param>
        /// <returns>found : true, couldn't find'' : false</returns>
        public static bool GetAssetPathSafely(UnityEngine.Object asset, out string path)
        {
            try
            {
                string assetPath = AssetDatabase.GetAssetPath(asset);
                if (string.IsNullOrEmpty(assetPath))
                {
                    path = string.Empty;
                    return false;
                }

                path = assetPath;
                return true;
            }
            catch (Exception e)
            {
                Debugger.Log(e);
            }

            path = string.Empty;
            return false;
        }
    }
}