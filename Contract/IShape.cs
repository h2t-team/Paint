using System;
using System.Windows;
using System.Windows.Media;

namespace Contract
{
    public interface IShape
    {
        string Name { get; }
        Color OutlineColor { get; set; }
        int PenWidth { get; set; }
        public DoubleCollection StrokeType { get; set; }
        void HandleStart(double x, double y);
        void HandleEnd(double x, double y);

        Point2D GetStart();
        Point2D GetEnd();

        UIElement Draw();
        IShape Clone();
    }
}
