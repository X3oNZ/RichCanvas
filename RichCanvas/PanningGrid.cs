﻿using RichCanvas.Gestures;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace RichCanvas
{
    internal class PanningGrid : Grid, IScrollInfo
    {
        private const double DEFAULT_DELTA = 10;

        private TranslateTransform _translateTransform;
        private ScaleTransform _scaleTransform;
        private Zoom _zoomGesture;
        private Vector _offset;
        private Size _extent;
        private Size _initialExtent;
        private Size _viewport;
        private Point _panInitialPosition;
        private RichItemsControl _parent;
        private Point _viewportBottomRightInitial;
        private Point _viewportTopLeftInitial;

        private double HighestElement => _parent.IsDrawing ? _parent.TopLimit : _parent.ItemsHost.TopLimit;

        private double LowestElement => _parent.IsDrawing ? _parent.BottomLimit : _parent.ItemsHost.BottomLimit;

        private double MostLeftElement => _parent.IsDrawing ? _parent.LeftLimit : _parent.ItemsHost.LeftLimit;

        private double MostRightElement => _parent.IsDrawing ? _parent.RightLimit : _parent.ItemsHost.RightLimit;

        internal double TopOffset => Math.Abs(TopLimit - HighestElement) * _scaleTransform.ScaleY;

        internal double BottomOffset => (BottomLimit - LowestElement) * _scaleTransform.ScaleY;

        internal double LeftOffset => Math.Abs(LeftLimit - MostLeftElement) * _scaleTransform.ScaleY;

        internal double RightOffset => (RightLimit - MostRightElement) * _scaleTransform.ScaleY;

        internal double TopLimit => TranslatePoint(_viewportTopLeftInitial, _parent.ItemsHost).Y;

        internal double BottomLimit => TranslatePoint(_viewportBottomRightInitial, _parent.ItemsHost).Y;

        internal double LeftLimit => TranslatePoint(_viewportTopLeftInitial, _parent.ItemsHost).X;

        internal double RightLimit => TranslatePoint(_viewportBottomRightInitial, _parent.ItemsHost).X;

        public bool CanHorizontallyScroll { get; set; }

        public bool CanVerticallyScroll { get; set; }

        public double ExtentHeight => _extent.Height;

        public double ExtentWidth => _extent.Width;

        public double HorizontalOffset => _offset.X;

        public ScrollViewer ScrollOwner { get; set; }

        public double VerticalOffset => _offset.Y;

        public double ViewportHeight => _viewport.Height;

        public double ViewportWidth => _viewport.Width;

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.Space) && _parent.IsPanning)
            {
                _panInitialPosition = e.GetPosition(this);
                CaptureMouse();
            }
        }

        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.Space) && Mouse.LeftButton == MouseButtonState.Pressed && _parent.IsPanning)
            {
                var currentPosition = e.GetPosition(this);
                var deltaHeight = currentPosition.Y - _panInitialPosition.Y;
                var deltaWidth = currentPosition.X - _panInitialPosition.X;

                if (deltaWidth != 0)
                {
                    PanHorizontally(-deltaWidth);
                }

                if (deltaHeight != 0)
                {
                    PanVertically(-deltaHeight);
                }
                ScrollOwner.InvalidateScrollInfo();
                _panInitialPosition = currentPosition;
            }
        }
        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            if (IsMouseCaptured)
            {
                ReleaseMouseCapture();
            }
        }
        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                var position = e.GetPosition(this);
                _zoomGesture.ZoomToPosition(position, e.Delta);

                SetVerticalOffset(TopOffset);
                UpdateExtentHeight();

                SetHorizontalOffset(LeftOffset);
                UpdateExtentWidth();

                ScrollOwner.InvalidateScrollInfo();
                if (_parent.EnableVirtualization)
                {
                    _parent.NeedMeasure = true;
                    _parent.ItemsHost.InvalidateMeasure();
                }
            }
        }
        protected override Size MeasureOverride(Size constraint)
        {
            if (ScrollOwner != null)
            {
                if (_viewport != constraint)
                {
                    _viewportTopLeftInitial = new Point(0, 0);
                    _viewport = constraint;
                    _viewportBottomRightInitial = new Point(_viewport.Width, _viewport.Height);
                    _initialExtent = _viewport;

                    if (TopLimit < HighestElement && BottomLimit > LowestElement)
                    {
                        _extent.Height = _viewport.Height;
                    }
                    if (LeftLimit < MostLeftElement && RightLimit > MostRightElement)
                    {
                        _extent.Width = _viewport.Width;
                    }

                    AdjustScrollVertically();
                    AdjustScrollHorizontally();
                }
                ScrollOwner.InvalidateScrollInfo();
            }
            return base.MeasureOverride(constraint);
        }
        protected override Size ArrangeOverride(Size arrangeSize)
        {
            if (ScrollOwner != null)
            {
                if (_viewport != arrangeSize)
                {
                    _viewportTopLeftInitial = new Point(0, 0);
                    _viewport = arrangeSize;
                    _viewportBottomRightInitial = new Point(_viewport.Width, _viewport.Height);
                    _initialExtent = _viewport;

                    if (TopLimit > HighestElement && BottomLimit > LowestElement)
                    {
                        _extent.Height = _viewport.Height;
                    }
                    if (LeftLimit < MostLeftElement && RightLimit > MostRightElement)
                    {
                        _extent.Width = _viewport.Width;
                    }

                    AdjustScrollVertically();
                    AdjustScrollHorizontally();
                }
                ScrollOwner.InvalidateScrollInfo();
            }
            return base.ArrangeOverride(arrangeSize);
        }

        public void LineDown()
        {
            if (!_parent.IsZooming)
            {
                var scrollOffset = 10;
                PanVertically(scrollOffset);
                ScrollOwner.InvalidateScrollInfo();
            }
        }

        public void LineLeft()
        {
            if (!_parent.IsZooming)
            {
                var scrollOffset = 10;
                PanHorizontally(scrollOffset);
                ScrollOwner.InvalidateScrollInfo();
            }
        }

        public void LineRight()
        {
            if (!_parent.IsZooming)
            {
                var scrollOffset = 10;

                PanHorizontally(-scrollOffset);
                ScrollOwner.InvalidateScrollInfo();
            }
        }

        public void LineUp()
        {
            if (!_parent.IsZooming)
            {
                var scrollOffset = 10;

                PanVertically(-scrollOffset);
                ScrollOwner.InvalidateScrollInfo();
            }
        }

        public Rect MakeVisible(Visual visual, Rect rectangle)
        {
            throw new System.NotImplementedException();
        }

        public void MouseWheelDown()
        {
            if (!_parent.IsZooming)
            {
                var scrollOffset = 10;
                PanVertically(scrollOffset);
                ScrollOwner.InvalidateScrollInfo();
            }
        }

        public void MouseWheelLeft()
        {
            if (!_parent.IsZooming)
            {
                var scrollOffset = 10;
                PanHorizontally(scrollOffset);
                ScrollOwner.InvalidateScrollInfo();
            }
        }

        public void MouseWheelRight()
        {
            if (!_parent.IsZooming)
            {
                var scrollOffset = 10;

                PanHorizontally(-scrollOffset);
                ScrollOwner.InvalidateScrollInfo();
            }
        }

        public void MouseWheelUp()
        {
            if (!_parent.IsZooming)
            {
                var scrollOffset = 10;

                PanVertically(-scrollOffset);
                ScrollOwner.InvalidateScrollInfo();
            }
        }

        public void PageDown()
        {
            if (!_parent.IsZooming)
            {
                var scrollOffset = 10;
                PanVertically(scrollOffset);
                ScrollOwner.InvalidateScrollInfo();
            }
        }

        public void PageLeft()
        {
            if (!_parent.IsZooming)
            {
                var scrollOffset = 10;
                PanHorizontally(scrollOffset);
                ScrollOwner.InvalidateScrollInfo();
            }
        }

        public void PageRight()
        {
            if (!_parent.IsZooming)
            {
                var scrollOffset = 10;

                PanHorizontally(-scrollOffset);
                ScrollOwner.InvalidateScrollInfo();
            }
        }

        public void PageUp()
        {
            if (!_parent.IsZooming)
            {
                var scrollOffset = 10;

                PanVertically(-scrollOffset);
                ScrollOwner.InvalidateScrollInfo();
            }
        }

        public void SetHorizontalOffset(double offset)
        {
            if (offset == 0)
            {
                // reset
                _offset.X = 0;
                _extent.Width = _viewport.Width;
            }
            if (LeftLimit > MostLeftElement)
            {
                _offset.X = LeftOffset;
            }
            else if (RightLimit < MostRightElement)
            {
                _offset.X = Math.Min(_offset.X + offset, RightOffset);
            }
            else
            {
                // reset
                _offset.X = 0;
                _extent.Width = _viewport.Width;
            }
        }

        public void SetVerticalOffset(double offset)
        {
            if (offset == 0)
            {
                // reset
                _offset.Y = 0;
                _extent.Height = _viewport.Height;
            }
            if (TopLimit > HighestElement)
            {
                _offset.Y = TopOffset;
            }
            else if (BottomLimit < LowestElement)
            {
                _offset.Y = Math.Min(_offset.Y + offset, BottomOffset);
            }
            else
            {
                // reset
                _offset.Y = 0;
                _extent.Height = _viewport.Height;
            }
        }

        public void AdjustScrollVertically()
        {
            SetVerticalOffset(TopOffset);
            UpdateExtentHeight();

            ScrollOwner.InvalidateScrollInfo();
        }

        public void AdjustScrollHorizontally()
        {
            SetHorizontalOffset(LeftOffset);
            UpdateExtentWidth();

            ScrollOwner.InvalidateScrollInfo();
        }

        internal void PanVertically(double offset, bool reverseScroll = false)
        {
            if (_parent.EnableVirtualization)
            {
                _parent.NeedMeasure = true;
                _parent.ItemsHost.InvalidateMeasure();
            }

            if (reverseScroll)
            {
                ScrollVertically(-offset);
            }
            else
            {
                ScrollVertically(offset);
            }
            if (TopLimit > HighestElement || BottomLimit < LowestElement)
            {
                SetVerticalOffset(offset);
                UpdateExtentHeight();
            }
            else
            {
                SetVerticalOffset(0);
            }
            ScrollOwner.InvalidateScrollInfo();
        }
        internal void PanHorizontally(double offset, bool reverseScroll = false)
        {
            if (_parent.EnableVirtualization)
            {
                _parent.NeedMeasure = true;
                _parent.ItemsHost.InvalidateMeasure();
            }

            if (reverseScroll)
            {
                ScrollHorizontally(-offset);
            }
            else
            {
                ScrollHorizontally(offset);
            }
            if (LeftLimit > MostLeftElement || RightLimit < MostRightElement)
            {
                SetHorizontalOffset(offset);
                UpdateExtentWidth();
            }
            else
            {
                SetHorizontalOffset(0);
            }
            ScrollOwner.InvalidateScrollInfo();
        }

        internal void Initalize(RichItemsControl richItemsControl)
        {
            _parent = richItemsControl;
            _translateTransform = (TranslateTransform)(richItemsControl.AppliedTransform).Children[1];
            _scaleTransform = (ScaleTransform)(richItemsControl.AppliedTransform).Children[0];
            _zoomGesture = new Zoom(_scaleTransform, _translateTransform);
        }

        private void UpdateExtentWidth()
        {
            if (LeftLimit > MostLeftElement && RightLimit > MostRightElement)
            {
                _extent.Width = _initialExtent.Width + Math.Abs(LeftOffset);
            }
            else if (RightLimit < MostRightElement && LeftLimit < MostLeftElement)
            {
                _extent.Width = _initialExtent.Width + Math.Abs(RightOffset);
            }
            else if (LeftLimit > MostLeftElement && RightLimit < MostRightElement)
            {
                _extent.Width = _initialExtent.Width + LeftOffset + Math.Abs(RightOffset);
            }
        }
        private void ScrollVertically(double offset)
        {
            _translateTransform.Y += -offset;
        }

        private void ScrollHorizontally(double offset)
        {
            _translateTransform.X += -offset;
        }

        private void UpdateExtentHeight()
        {
            if (TopLimit > HighestElement && BottomLimit > LowestElement)
            {
                _extent.Height = _initialExtent.Height + Math.Abs(TopOffset);
            }
            else if (BottomLimit < LowestElement && TopLimit < HighestElement)
            {
                _extent.Height = _initialExtent.Height + Math.Abs(BottomOffset);
            }
            else if (TopLimit > HighestElement && BottomLimit < LowestElement)
            {
                _extent.Height = _initialExtent.Height + TopOffset + Math.Abs(BottomOffset);
            }
        }
    }
}
