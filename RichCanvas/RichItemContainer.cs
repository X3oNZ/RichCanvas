﻿using RichCanvas.Helpers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace RichCanvas
{
    public class RichItemContainer : ContentControl
    {
        static RichItemContainer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RichItemContainer), new FrameworkPropertyMetadata(typeof(RichItemContainer)));
        }
        public static DependencyProperty IsSelectedProperty = DependencyProperty.Register("IsSelected", typeof(bool), typeof(RichItemContainer));
        public bool IsSelected
        {
            get => (bool)GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value);
        }
        public static DependencyProperty TopProperty = DependencyProperty.Register("Top", typeof(double), typeof(RichItemContainer));
        public double Top
        {
            get => (double)GetValue(TopProperty);
            set => SetValue(TopProperty, value);
        }
        public static DependencyProperty LeftProperty = DependencyProperty.Register("Left", typeof(double), typeof(RichItemContainer));
        public double Left
        {
            get => (double)GetValue(LeftProperty);
            set => SetValue(LeftProperty, value);
        }
        public static DependencyProperty QuadrantProperty = DependencyProperty.Register("Quadrant", typeof(int), typeof(RichItemContainer));
        public int Quadrant
        {
            get => (int)GetValue(QuadrantProperty);
            set => SetValue(QuadrantProperty, value);
        }
       
        internal bool IsDrawn { get; set; }
        protected override void OnMouseEnter(MouseEventArgs e)
        {
            DragBehavior.SetIsDragging((RichItemContainer)e.OriginalSource, true);
        }
        protected override void OnMouseLeave(MouseEventArgs e)
        {
            DragBehavior.SetIsDragging((RichItemContainer)e.OriginalSource, false);
        }
        protected override GeometryHitTestResult HitTestCore(GeometryHitTestParameters hitTestParameters)
        {
            var intersection = hitTestParameters.HitGeometry.FillContainsWithDetail(new RectangleGeometry(new Rect(this.Left, this.Top, this.Width, this.Height)));
            return new GeometryHitTestResult(this, intersection);
        }
    }
}