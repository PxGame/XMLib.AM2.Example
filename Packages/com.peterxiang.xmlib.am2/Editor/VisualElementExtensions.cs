using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UIElements;

namespace XMLib.AM2
{
    [Flags]
    public enum VersionChangeType
    {
        Bindings = 1,
        ViewData = 2,
        Hierarchy = 4,
        Layout = 8,
        StyleSheet = 0x10,
        Styles = 0x20,
        Overflow = 0x40,
        BorderRadius = 0x80,
        BorderWidth = 0x100,
        Transform = 0x200,
        Size = 0x400,
        Repaint = 0x800,
        Opacity = 0x1000,
        Color = 0x2000,
        RenderHints = 0x4000,
        TransitionProperty = 0x8000
    }

    public static class VisualElementExtensions
    {
        public static StyleSheet LoadStyle(this VisualElement ve, string ussRelativePath)
        {
            var style = AMEditorUtility.LoadStyle(ussRelativePath);
            if (style == null) { throw new Exception($"not found {ussRelativePath}"); }
            ve.styleSheets.Add(style);
            return style;
        }

        public static VisualElement LoadDoc(this VisualElement ve, string docRelativePath)
        {
            var doc = AMEditorUtility.LoadDoc(docRelativePath);
            if (doc == null) { throw new Exception($"not found {docRelativePath}"); }
            var tree = doc.CloneTree();
            ve.Add(tree);
            return tree;
        }

        public static MethodInfo _incrementVersion;

        public static void IncrementVersion(this VisualElement ve, VersionChangeType changeType)
        {
            if (_incrementVersion == null)
            {
                _incrementVersion = typeof(VisualElement).GetMethod("IncrementVersion", BindingFlags.Instance | BindingFlags.NonPublic);
            }
            _incrementVersion.Invoke(ve, new object[] { (int)changeType });
        }
    }
}