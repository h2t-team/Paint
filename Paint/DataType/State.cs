using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Diagnostics;

namespace Paint.DataType
{
    class State
    {
        public List<UIElement> Elements { get; set; }
        public string Background { get; set; }
        public State()
        {

        }
        public State(List<UIElement> elements, string background)
        {
            Elements = new List<UIElement>(elements);
            Background = background;
        }
        public bool Equals(State state)
        {
            if(state.Background != Background)
            {
                return false;
            }
            if (Elements.Count != state.Elements.Count)
                return false;
            return true;
        }
    }
}
