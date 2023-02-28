using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;

namespace XMLib.AM2.Timeline
{
    public class TrackPanel : VisualElement
    {
        private TwoPaneSplitView splitView;

        private Scroller horizontalScroller;
        private Scroller verticalScroller;

        private VisualElement rightContainer;
        private VisualElement trackContainer;
        private VisualElement trackContent;
        private VisualElement titleContainer;
        private VisualElement trackBodyContainer;
        private VisualElement trackBodyContent;

        private VisualElement leftContainer;
        private VisualElement headContainer;
        private VisualElement toolContainer;
        private VisualElement headBodyContainer;
        private VisualElement headBodyContent;

        public Vector2 scrollOffset
        {
            get
            {
                return new Vector2(horizontalScroller.value, verticalScroller.value);
            }
            set
            {
                if (value != scrollOffset)
                {
                    horizontalScroller.value = value.x;
                    verticalScroller.value = value.y;
                    UpdateContentViewTransform();
                }
            }
        }

        private float scrollableWidth => trackBodyContent.layout.width - trackContainer.layout.width;

        private float scrollableHeight => headBodyContent.layout.height - headBodyContainer.layout.height;

        public TrackPanel()
        {
            InitComponents();
        }

        private void InitComponents()
        {
            this.LoadStyle("TrackPanel.uss");

            //
            splitView = new TwoPaneSplitView(0, 200, TwoPaneSplitViewOrientation.Horizontal);
            this.Add(splitView);
            //
            leftContainer = new VisualElement() { name = "left-container" };
            splitView.Add(leftContainer);
            //
            rightContainer = new VisualElement() { name = "right-container" };
            splitView.Add(rightContainer);

            InitLeftComponents();
            InitRightComponents();
        }

        private void InitLeftComponents()
        {
            //
            headContainer = new VisualElement() { name = "head-container" };
            leftContainer.Add(headContainer);

            //
            toolContainer = new VisualElement() { name = "tool-container" };
            headContainer.Add(toolContainer);

            headBodyContainer = new VisualElement() { name = "head-body-container" };
            headContainer.Add(headBodyContainer);

            //
            headBodyContent = new VisualElement() { name = "head-body-content" };
            headBodyContainer.Add(headBodyContent);

            //
            headBodyContent.Add(new TrackBackground());
        }

        private void InitRightComponents()
        {
            //
            trackContainer = new VisualElement() { name = "track-container" };
            trackContainer.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            rightContainer.Add(trackContainer);

            horizontalScroller = new Scroller(0, 2.14748365E+09f, t =>
            {
                scrollOffset = new Vector2(t, scrollOffset.y);
                UpdateContentViewTransform();
            }, SliderDirection.Horizontal)
            { name = "horizontalbar" };
            rightContainer.Add(horizontalScroller);

            verticalScroller = new Scroller(0, 2.14748365E+09f, t =>
            {
                scrollOffset = new Vector2(scrollOffset.x, t);
                UpdateContentViewTransform();
            }, SliderDirection.Vertical)
            { name = "verticalbar" };
            rightContainer.Add(verticalScroller);

            //
            trackContent = new VisualElement() { name = "track-content" };
            trackContent.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            trackContainer.Add(trackContent);

            //
            titleContainer = new VisualElement() { name = "title-container" };
            trackContent.Add(titleContainer);
            trackBodyContainer = new VisualElement() { name = "track-body-container" };
            trackContent.Add(trackBodyContainer);

            //
            trackBodyContent = new VisualElement() { name = "track-body-content" };
            trackBodyContainer.Add(trackBodyContent);

            //

            titleContainer.Add(new TrackTitle());
            trackBodyContent.Add(new TrackBackground());
        }

        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            if (!(evt.oldRect.size == evt.newRect.size))
            {
                UpdateScrollers(true, true);
                UpdateContentViewTransform();
            }
        }

        private void UpdateContentViewTransform()
        {
            Vector2 vector = scrollOffset;
            vector.y += headBodyContent.resolvedStyle.top;
            float x = AMEditorUtility.RoundToPixelGrid(0f - vector.x);
            float y = AMEditorUtility.RoundToPixelGrid(0f - vector.y);

            trackContent.transform.position = new Vector3(x, 0);
            headBodyContent.transform.position = new Vector3(0, y);
            trackBodyContent.transform.position = new Vector3(0, y);

            trackContent.IncrementVersion(VersionChangeType.Repaint);
        }

        private void AdjustScrollers()
        {
            float factor = ((trackBodyContent.layout.width > 1E-30f) ? (trackContainer.layout.width / trackBodyContent.layout.width) : 1f);
            float factor2 = ((headBodyContent.layout.height > 1E-30f) ? (headContainer.layout.height / headBodyContent.layout.height) : 1f);
            horizontalScroller.Adjust(factor);
            verticalScroller.Adjust(factor2);
        }

        internal void UpdateScrollers(bool displayHorizontal, bool displayVertical)
        {
            AdjustScrollers();
            horizontalScroller.SetEnabled(trackBodyContent.layout.width - trackContainer.layout.width > 0f);
            verticalScroller.SetEnabled(headBodyContent.layout.height - headContainer.layout.height > 0f);
            bool flag = displayHorizontal;
            bool flag2 = displayVertical;
            DisplayStyle displayStyle = ((!flag) ? DisplayStyle.None : DisplayStyle.Flex);
            DisplayStyle displayStyle2 = ((!flag2) ? DisplayStyle.None : DisplayStyle.Flex);
            if (displayStyle != horizontalScroller.style.display)
            {
                horizontalScroller.style.display = displayStyle;
            }
            if (displayStyle2 != verticalScroller.style.display)
            {
                verticalScroller.style.display = displayStyle2;
            }
            verticalScroller.lowValue = 0f;
            verticalScroller.highValue = scrollableHeight;
            horizontalScroller.lowValue = 0f;
            horizontalScroller.highValue = scrollableWidth;
            if (!(scrollableHeight > 0f))
            {
                verticalScroller.value = 0f;
            }
            if (!(scrollableWidth > 0f))
            {
                horizontalScroller.value = 0f;
            }
        }
    }

    public class TrackTitle : ImmediateModeElement
    {
        private static CustomStyleProperty<Color> LineColorProperty = new CustomStyleProperty<Color>("--line-color");
        private Color _lineColor = Color.gray;

        public TrackTitle()
        {
            base.pickingMode = PickingMode.Ignore;
            this.style.position = Position.Absolute;
            this.StretchToParentSize();

            this.LoadStyle("TimelineBackground.uss");

            RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);
        }

        private void OnCustomStyleResolved(CustomStyleResolvedEvent evt)
        {
            Color value = Color.clear;
            if (customStyle.TryGetValue(LineColorProperty, out value))
            {
                _lineColor = value;
            }
        }

        protected override void ImmediateRepaint()
        {
            AMEditorUtility.ApplyWireMaterialMethod();

            float currentXPos = layout.xMin;
            float stepWidth = 100;

            while (currentXPos < layout.xMax)
            {
                currentXPos += stepWidth;

                GL.Begin(GL.LINES);
                GL.Color(_lineColor);
                GL.Vertex(new Vector3(currentXPos, layout.yMin));
                GL.Vertex(new Vector3(currentXPos, layout.yMax));
                GL.End();
            }

            currentXPos = 0;
            GUIContent content = new GUIContent();
            while (currentXPos < layout.xMax)
            {
                currentXPos += stepWidth;
                content.text = currentXPos.ToString();
                content.tooltip = "Test";
                var size = GUI.skin.label.CalcSize(content);
                GUI.Label(new Rect(Vector2.right * currentXPos, size), content);
            }
        }
    }

    public class TrackBackground : ImmediateModeElement
    {
        private static CustomStyleProperty<Color> LineColorProperty = new CustomStyleProperty<Color>("--line-color");
        private Color _lineColor = Color.gray;

        public TrackBackground()
        {
            base.pickingMode = PickingMode.Ignore;
            this.style.position = Position.Absolute;
            this.StretchToParentSize();

            this.LoadStyle("TimelineBackground.uss");

            RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);
        }

        private void OnCustomStyleResolved(CustomStyleResolvedEvent evt)
        {
            Color value = Color.clear;
            if (customStyle.TryGetValue(LineColorProperty, out value))
            {
                _lineColor = value;
            }
        }

        protected override void ImmediateRepaint()
        {
            AMEditorUtility.ApplyWireMaterialMethod();

            GUIContent content = new GUIContent();

            float currentXPos = layout.xMin;
            float stepWidth = 100;

            float currentYPos = layout.yMin;
            float stepHeight = 100;

            while (currentXPos < layout.xMax)
            {
                currentXPos += stepWidth;

                GL.Begin(GL.LINES);
                GL.Color(_lineColor);
                GL.Vertex(new Vector3(currentXPos, layout.yMin));
                GL.Vertex(new Vector3(currentXPos, layout.yMax));
                GL.End();
            }

            while (currentYPos < layout.yMax)
            {
                currentYPos += stepHeight;

                GL.Begin(GL.LINES);
                GL.Color(_lineColor);
                GL.Vertex(new Vector3(layout.xMin, currentYPos));
                GL.Vertex(new Vector3(layout.xMax, currentYPos));
                GL.End();
            }

            currentXPos = 0;
            while (currentXPos < layout.xMax)
            {
                currentXPos += stepWidth;
                content.text = currentXPos.ToString();
                content.tooltip = "Test";
                var size = GUI.skin.label.CalcSize(content);
                GUI.Label(new Rect(Vector2.right * currentXPos, size), content);
            }

            currentYPos = 0;
            while (currentYPos < layout.yMax)
            {
                currentYPos += stepHeight;
                content.text = currentYPos.ToString();
                content.tooltip = "Test";
                var size = GUI.skin.label.CalcSize(content);
                GUI.Label(new Rect(Vector2.up * currentYPos, size), content);
            }
        }
    }
}