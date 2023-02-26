using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace XMLib.AM2.Timeline
{
    public class MenubarPanel : Toolbar
    {
        public MenubarPanel()
        {
            InitComponents();
        }

        private void InitComponents()
        {
            this.LoadStyle("MenubarPanel.uss");

            var playToggle = new ToolbarToggle() { name = "play-toggle" };
            var prevFrameBtn = new ToolbarButton() { name = "prev-frame-button" };
            var nextFrameBtn = new ToolbarButton() { name = "next-frame-button" };

            this.Add(prevFrameBtn);
            this.Add(playToggle);
            this.Add(nextFrameBtn);
        }
    }
}