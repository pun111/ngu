using System.Drawing;

namespace Ngu
{
    public class Pixel : ObservableObject
    {
        private Point           point;
        public Point            Point { get => point; set { if (value != point) { point = value; OnPropertyChanged(); } } }
        private Color           color;
        public Color            Color { get => color; set { if (value != color) { color = value; OnPropertyChanged(); } } }

        public                  Pixel(Point point, Color color)
        {
            this.point = point;
            this.color = color;
        }

        public override string  ToString()
        {
            return $"X: {Point.X} Y: {Point.Y} R: {Color.R} G: {Color.G} B: {Color.B}";
        }
    }
}
