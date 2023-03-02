using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;
using UnityEditor.UIElements;

namespace XMLib.AM2.Timeline
{
    public class TrackPanel : VisualElement
    {
        private TwoPaneSplitView splitView;

        private Scroller horizontalScroller;
        private Scroller verticalScroller;

        private VisualElement leftContainer;
        private VisualElement leftContent;
        private VisualElement toolContainer;
        private VisualElement trackHeadContainer;
        private VisualElement trackHeadContent;

        private VisualElement rightContainer;
        private VisualElement rightContent;
        private VisualElement titleTrackBodyContainer;
        private VisualElement titleContainer;
        private VisualElement trackBodyContainer;
        private VisualElement trackBodyContent;

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

        private float scrollableWidth => trackBodyContent.layout.width - rightContent.layout.width;

        private float scrollableHeight => trackHeadContent.layout.height - trackHeadContainer.layout.height;

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

            Test();
        }

        #region Test

        private void Test()
        {
            var addbtn = new ToolbarButton(() =>
            {
                TrackItem item = new TrackItem()
                {
                    head = new TrackHead() { name = "track-head" },
                    body = new TrackBody() { name = "track-body" }
                };

                trackHeadContent.Add(item.head);
                trackBodyContent.Add(item.body);

                UpdateScrollers();
                UpdateContentViewTransform();
            })
            { };
            addbtn.text = "Add";
            toolContainer.Add(addbtn);
        }

        #endregion Test

        private void InitLeftComponents()
        {
            //
            leftContent = new VisualElement() { name = "left-content" };
            leftContainer.Add(leftContent);

            //
            toolContainer = new Toolbar() { name = "tool-container" };
            leftContent.Add(toolContainer);

            trackHeadContainer = new VisualElement() { name = "track-head-container" };
            leftContent.Add(trackHeadContainer);

            //
            trackHeadContent = new VisualElement() { name = "track-head-content" };
            trackHeadContainer.Add(trackHeadContent);

            //
            trackHeadContent.Add(new TrackBackground());
        }

        private void InitRightComponents()
        {
            //
            rightContent = new VisualElement() { name = "right-content" };
            rightContent.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            rightContainer.Add(rightContent);

            horizontalScroller = new Scroller(0, 2.14748365E+09f, t =>
            {
                scrollOffset = new Vector2(t, scrollOffset.y);
                UpdateContentViewTransform();
            }, SliderDirection.Horizontal)
            { name = "horizontal-scroller" };
            rightContainer.Add(horizontalScroller);

            verticalScroller = new Scroller(0, 2.14748365E+09f, t =>
            {
                scrollOffset = new Vector2(scrollOffset.x, t);
                UpdateContentViewTransform();
            }, SliderDirection.Vertical)
            { name = "vertical-scroller" };
            rightContainer.Add(verticalScroller);

            //
            titleTrackBodyContainer = new VisualElement() { name = "title-track-body-container" };
            titleTrackBodyContainer.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            rightContent.Add(titleTrackBodyContainer);

            //
            titleContainer = new VisualElement() { name = "title-container" };
            titleTrackBodyContainer.Add(titleContainer);
            trackBodyContainer = new VisualElement() { name = "track-body-container" };
            titleTrackBodyContainer.Add(trackBodyContainer);

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
                UpdateScrollers();
                UpdateContentViewTransform();
            }
        }

        private void UpdateContentViewTransform()
        {
            Vector2 vector = scrollOffset;
            vector.y += trackHeadContent.resolvedStyle.top;
            float x = AMEditorUtility.RoundToPixelGrid(0f - vector.x);
            float y = AMEditorUtility.RoundToPixelGrid(0f - vector.y);

            titleTrackBodyContainer.transform.position = new Vector3(x, 0);
            trackHeadContent.transform.position = new Vector3(0, y);
            trackBodyContent.transform.position = new Vector3(0, y);

            this.IncrementVersion(VersionChangeType.Repaint);
        }

        private void AdjustScrollers()
        {
            float factor = ((trackBodyContent.layout.width > 1E-30f) ? (rightContent.layout.width / trackBodyContent.layout.width) : 1f);
            float factor2 = ((trackHeadContent.layout.height > 1E-30f) ? (leftContent.layout.height / trackHeadContent.layout.height) : 1f);
            horizontalScroller.Adjust(factor);
            verticalScroller.Adjust(factor2);
        }

        internal void UpdateScrollers()
        {
            AdjustScrollers();
            horizontalScroller.SetEnabled(trackBodyContent.layout.width - rightContent.layout.width > 0f);
            verticalScroller.SetEnabled(trackHeadContent.layout.height - leftContent.layout.height > 0f);
            DisplayStyle displayStyle = DisplayStyle.Flex;
            DisplayStyle displayStyle2 = DisplayStyle.Flex;
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
            //base.pickingMode = PickingMode.Ignore;
            this.style.position = Position.Absolute;
            this.StretchToParentSize();

            this.LoadStyle("TimelineBackground.uss");

            RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);

            Test();
        }

        #region Test

        private VisualElement cursor;

        private void Test()
        {
            cursor = new VisualElement();
            cursor.pickingMode = PickingMode.Ignore;
            cursor.style.position = Position.Absolute;
            cursor.style.height = 16;
            cursor.style.width = 12;
            cursor.style.backgroundColor = Color.red;
            cursor.transform.position = Vector3.zero;
            this.Add(cursor);

            this.AddManipulator(new TestManipulator());
        }

        public class TestManipulator : MouseManipulator
        {
            private bool _isActive = false;

            public TestManipulator()
            {
                base.activators.Add(new ManipulatorActivationFilter
                {
                    button = MouseButton.LeftMouse
                });
                _isActive = false;
            }

            protected override void RegisterCallbacksOnTarget()
            {
                target.RegisterCallback<MouseDownEvent>(OnMouseDownEvent);
                target.RegisterCallback<MouseUpEvent>(OnMouseUpEvent);
                target.RegisterCallback<MouseMoveEvent>(OnMouseMoveEvent);
            }

            protected override void UnregisterCallbacksFromTarget()
            {
                target.UnregisterCallback<MouseDownEvent>(OnMouseDownEvent);
                target.UnregisterCallback<MouseUpEvent>(OnMouseUpEvent);
                target.UnregisterCallback<MouseMoveEvent>(OnMouseMoveEvent);
            }

            private void OnMouseMoveEvent(MouseMoveEvent evt)
            {
                if (!_isActive) { return; }
                Debug.Log("OnMouseMoveEvent");

                var cursor = (target as TrackTitle).cursor;
                cursor.transform.position = new Vector3(evt.localMousePosition.x, 0, 0);

                evt.StopPropagation();
            }

            private void OnMouseDownEvent(MouseDownEvent evt)
            {
                if (_isActive)
                {
                    evt.StopImmediatePropagation();
                }
                else if (CanStartManipulation(evt))
                {
                    Debug.Log("OnMouseDownEvent");
                    _isActive = true;

                    target.CaptureMouse();

                    var cursor = (target as TrackTitle).cursor;
                    cursor.transform.position = new Vector3(evt.localMousePosition.x, 0, 0);

                    evt.StopPropagation();
                }
            }

            private void OnMouseUpEvent(MouseUpEvent evt)
            {
                if (_isActive && CanStopManipulation(evt))
                {
                    Debug.Log("OnMouseUpEvent");
                    _isActive = false;
                    target.ReleaseMouse();
                    evt.StopPropagation();
                }
            }
        }

        #endregion Test

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

    public class TrackItem
    {
        public TrackHead head;
        public TrackBody body;
    }

    public class TrackHead : VisualElement
    {
    }

    public class TrackBody : VisualElement
    {
    }
}