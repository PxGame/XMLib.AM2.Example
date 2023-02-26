using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace XMLib.AM2.Timeline
{
    public class TimelineWindow : EditorWindow, IHasCustomMenu
    {
        public void AddItemsToMenu(GenericMenu menu)
        {
        }

        private void OnEnable()
        {
            titleContent = new GUIContent("AM2 Timeline", AMEditorUtility.LoadIcon("TimelineWindow.png"));

            InitUI();
        }

        private void InitUI()
        {
            var timeline = new TimelinePanel();
            rootVisualElement.Add(timeline);
        }
    }
}