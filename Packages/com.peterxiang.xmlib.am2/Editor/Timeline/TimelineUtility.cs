using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace XMLib.AM2.Timeline
{
    public static class TimelineUtility
    {
        [MenuItem("Window/Action Machine 2/Timeline", false, 1)]
        public static void ShowWindow()
        {
            var win = EditorWindow.GetWindow<TimelineWindow>();
            win.Show();
        }
    }
}