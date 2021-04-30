using ClassLibrary1;
using System;
using System.Numerics;
using System.Windows.Data;

namespace WpfApp1
{
    [ValueConversion(typeof(Grid2D), typeof(string))]
    public class GridConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Grid2D grid = (Grid2D)value;
            return $"NodesNumberX = {grid.NodesNumberX}, NodesNumberY = {grid.NodesNumberY}" +
                $"StepSizeX = {grid.StepSizeX}, StepSizeY = {grid.StepSizeY}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }

    [ValueConversion(typeof(double), typeof(string))]
    public class ValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double valueDouble = (double)value;
            return $"Значение: {valueDouble:f4}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }

    [ValueConversion(typeof(Vector2), typeof(string))]
    public class CoordConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Vector2 vector2 = (Vector2)value;
            return $"Координаты: ({vector2.X:f3}, {vector2.Y:f3})";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
}
