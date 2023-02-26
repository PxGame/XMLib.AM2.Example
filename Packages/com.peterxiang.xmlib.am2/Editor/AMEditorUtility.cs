using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System.IO;
using System.Reflection;
using System.Linq;

namespace XMLib.AM2
{
    public static class AMEditorUtility
    {
        private const string packageRoot = "Packages/com.peterxiang.xmlib.am2/Editor/Resources/";
        private const string iconRoot = packageRoot + "Images/Icons/";
        private const string styleRoot = packageRoot + "Styles/";
        private const string docRoot = packageRoot + "Docs/";

        public static Texture2D LoadIcon(string iconRelativePath)
        {
            var fullpath = Path.Combine(iconRoot, iconRelativePath);
            return AssetDatabase.LoadAssetAtPath<Texture2D>(fullpath);
        }

        public static StyleSheet LoadStyle(string ussRelativePath)
        {
            var fullpath = Path.Combine(styleRoot, ussRelativePath);
            return AssetDatabase.LoadAssetAtPath<StyleSheet>(fullpath);
        }

        public static VisualTreeAsset LoadDoc(string docRelativePath)
        {
            var fullpath = Path.Combine(docRoot, docRelativePath);
            return AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(fullpath);
        }

        #region internal method

        public static MethodInfo _applyWireMaterialMethod;
        public static MethodInfo _roundToPixelGrid;

        public static void ApplyWireMaterialMethod()
        {
            if (_applyWireMaterialMethod == null)
            {
                _applyWireMaterialMethod = typeof(HandleUtility)
                    .GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
                    .Where(t =>
                    t.Name.Equals("ApplyWireMaterial")
                    && (t.GetParameters().Length == 0))
                    .FirstOrDefault();
            }
            _applyWireMaterialMethod.Invoke(null, null);
        }

        public static float RoundToPixelGrid(float v)
        {
            if (_roundToPixelGrid == null)
            {
                _roundToPixelGrid = typeof(GUIUtility).GetMethod("RoundToPixelGrid", BindingFlags.Static | BindingFlags.NonPublic);
            }
            return (float)_roundToPixelGrid.Invoke(null, new object[] { v });
        }

        #endregion internal method
    }
}