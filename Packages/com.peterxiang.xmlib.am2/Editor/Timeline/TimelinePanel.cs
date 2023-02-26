using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace XMLib.AM2.Timeline
{
    public class TimelinePanel : VisualElement
    {
        public TimelinePanel()
        {
            InitComponents();
        }

        private void InitComponents()
        {
            //
            this.LoadStyle("TimelinePanel.uss");
            this.name = "root-container";

            //
            var menubar = new MenubarPanel();
            var track = new TrackPanel();

            this.Add(menubar);
            this.Add(track);
        }
    }
}