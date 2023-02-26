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
        private VisualElement trackContent;
        private Scroller horizontalScroller;
        private Scroller verticalScroller;
        private VisualElement trackContainer;
        private VisualElement leftContainer;
        private VisualElement rightContainer;
        private VisualElement titleContainer;
        private VisualElement bodyContainer;
        private VisualElement bodyContent;

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

        private float scrollableWidth => bodyContent.layout.width - trackContainer.layout.width;

        private float scrollableHeight => bodyContent.layout.height - bodyContainer.layout.height;

        public TrackPanel()
        {
            InitComponents();
        }

        private void InitComponents()
        {
            this.LoadStyle("TrackPanel.uss");

            InitLeftComponents();
            InitRightComponents();
        }

        private void InitLeftComponents()
        {
            leftContainer = new VisualElement() { name = "left-container" };
            this.Add(leftContainer);
        }

        private void InitRightComponents()
        {
            //main container
            rightContainer = new VisualElement() { name = "right-container" };
            this.Add(rightContainer);

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
            bodyContainer = new VisualElement() { name = "body-container" };
            trackContent.Add(bodyContainer);

            //
            bodyContent = new VisualElement() { name = "body-content" };
            bodyContainer.Add(bodyContent);

            //

            titleContainer.Add(new TrackBackground());
            bodyContainer.Add(new TrackBackground());
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
            Vector3 position = bodyContent.transform.position;
            Vector2 vector = scrollOffset;
            if (true)
            {
                vector.y += bodyContent.resolvedStyle.top;
            }
            position.x = AMEditorUtility.RoundToPixelGrid(0f - vector.x);
            position.y = AMEditorUtility.RoundToPixelGrid(0f - vector.y);

            trackContent.transform.position = new Vector3(position.x, 0);
            bodyContent.transform.position = new Vector3(0, position.y);

            trackContent.IncrementVersion(VersionChangeType.Repaint);
        }

        private void AdjustScrollers()
        {
            float factor = ((bodyContent.layout.width > 1E-30f) ? (trackContainer.layout.width / bodyContent.layout.width) : 1f);
            float factor2 = ((bodyContent.layout.height > 1E-30f) ? (bodyContainer.layout.height / bodyContent.layout.height) : 1f);
            horizontalScroller.Adjust(factor);
            verticalScroller.Adjust(factor2);
        }

        internal void UpdateScrollers(bool displayHorizontal, bool displayVertical)
        {
            AdjustScrollers();
            horizontalScroller.SetEnabled(bodyContent.layout.width - trackContainer.layout.width > 0f);
            verticalScroller.SetEnabled(bodyContent.layout.height - bodyContainer.layout.height > 0f);
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
        protected override void ImmediateRepaint()
        {
        }
    }

    public class TrackBackground : ImmediateModeElement
    {
        private static CustomStyleProperty<Color> LineColorProperty = new CustomStyleProperty<Color>("--line-color");
        private static readonly Color DefaultLineColor = new Color(0f, 0f, 0f, 0.18f);
        private Color _lineColor = Color.red;

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
}