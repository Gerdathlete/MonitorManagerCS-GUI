using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace MonitorManagerCS_GUI.Controls
{
    public class DebugAdorner : Adorner
    {
        public List<Action<DrawingContext>> DrawActions { get; } = [];

        public DebugAdorner(UIElement adornedElement) : base(adornedElement)
        {
            IsHitTestVisible = false;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            //Undo any transforming done by the adorned element
            drawingContext.PushTransform((Transform)AdornedElement.RenderTransform.Inverse);

            foreach (var action in DrawActions)
            {
                action(drawingContext);
            }

            drawingContext.Pop();
        }

        public void Invalidate() => InvalidateVisual();
    }
}
