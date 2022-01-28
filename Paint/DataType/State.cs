using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Diagnostics;
using Contract;

namespace Paint.DataType
{
    class State
    {
        public List<IShape> Shapes { get; set; }
        public string Background { get; set; }
        public State()
        {

        }
        public State(List<IShape> shapes, string background)
        {
            Shapes = new List<IShape>();
            foreach(var item in shapes)
            {
                IShape shape = item.Clone();
                shape.HandleStart(item.GetStart().X, item.GetStart().Y);
                shape.HandleEnd(item.GetEnd().X, item.GetEnd().Y);
                shape.StrokeType = item.StrokeType;
                shape.PenWidth = item.PenWidth;
                shape.OutlineColor = item.OutlineColor;
                shape.FillColor = item.FillColor;
                if (shape.Name == "Text") (shape as Text2D.Text2D).Text = (item as Text2D.Text2D).Text;
                Shapes.Add(shape);
            }
            Background = background;
        }
        public bool Equals(State state)
        {
            if(state.Background != Background)
            {
                return false;
            }
            if (Shapes.Count != state.Shapes.Count)
                return false;
            for(int i = 0; i < Shapes.Count; i++)
            {
                if (Shapes[i].FillColor != state.Shapes[i].FillColor)
                    return false;
            }
            return true;
        }
    }
}
