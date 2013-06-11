using MingControls.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MingControls.Controls
{
    public partial class ScrollingLineChart : UserControl
    {
        private Polygon _chart;
        private Line _chartCurrent;
        private Line[] _chartYScale = new Line[9];
        private Line[] _chartXScale = new Line[9];
        private TextBlock _currentText;
        private Border _currentTextBorder;
        private INumberFormatter<double> _formatter;

        private int _dataPoints;

        private double[] _data;

        private double _maxValue = 1;

        public bool ScaleIsWholeNumber { get; set; }

        public int DataPoints
        {
            get { return _dataPoints; }
            set
            {
                if (_dataPoints > 0)
                {
                    var old = new double[_dataPoints];
                    Array.Copy(_data, old, _dataPoints);
                    _data = new double[value];
                    if (value > _dataPoints)
                    {
                        Array.Copy(old, 0, _data, value - _dataPoints, _dataPoints);
                    }
                    else
                    {
                        Array.Copy(old, _dataPoints - value, _data, 0, value);
                    }
                    _dataPoints = value;
                }
                else
                {
                    _dataPoints = value;
                    _data = new double[value];
                }
            }
        }

        public string ChartTitle
        {
            set
            {
                ChartHeader.Text = value;
            }
        }

        public INumberFormatter<double> Formatter
        {
            set
            {
                _formatter = value;
            }
        }

        public Color BorderColor
        {
            set
            {
                CanvasBorder.BorderBrush = new SolidColorBrush(value);
                _chart.Stroke = new SolidColorBrush(value);
            }
        }

        public Color ScaleColor
        {
            set
            {
                for (int i = 0; i < 9; i++)
                {
                    _chartYScale[i].Stroke = new SolidColorBrush(value);
                    _chartXScale[i].Stroke = new SolidColorBrush(value);
                }
                DrawScale();
            }
        }

        public Color FillColor
        {
            set
            {
                _chart.Fill = new SolidColorBrush(value) { Opacity = 0.5 };
            }
        }

        public string XAxisMaxDesc
        {
            set
            {
                XAxisMax.Text = value;
            }
        }

        public void ResetData()
        {
            for (int i = 0; i < _dataPoints; i++)
            {
                _data[i] = 0;
            }
            _maxValue = 1;
        }

        public void ResetYAxisScale()
        {
            _maxValue = 1;
        }

        public ScrollingLineChart()
        {
            InitializeComponent();
            this.SizeChanged += LineChartSizeChanged;

            Canvas.SnapsToDevicePixels = true;

            for (int i = 0; i < 9; i++)
            {
                var line = new Line();
                line.SnapsToDevicePixels = true;
                Canvas.Children.Add(line);
                _chartYScale[i] = line;

                line = new Line();
                line.SnapsToDevicePixels = true;
                Canvas.Children.Add(line);
                _chartXScale[i] = line;
            }

            _chart = new Polygon();
            _chart.SnapsToDevicePixels = true;
            _chart.StrokeThickness = 1;
            _chart.Stroke = Brushes.Green;
            _chart.Fill = Brushes.LightGreen;
            Canvas.Children.Add(_chart);

            _chartCurrent = new Line();
            _chartCurrent.SnapsToDevicePixels = true;
            _chartCurrent.Stroke = new SolidColorBrush(Color.FromRgb(0, 0, 0));
            Canvas.Children.Add(_chartCurrent);

            _currentText = new TextBlock();
            _currentText.Background = new SolidColorBrush(Color.FromRgb(255, 255, 255));
            _currentText.SnapsToDevicePixels = true;
            _currentText.Padding = new Thickness(3, 0, 3, 0);

            _currentTextBorder = new Border();
            _currentTextBorder.SnapsToDevicePixels = true;
            Canvas.SetLeft(_currentTextBorder, 10);
            _currentTextBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(0, 0, 0));
            _currentTextBorder.BorderThickness = new Thickness(1);
            _currentTextBorder.Child = _currentText;
            Canvas.Children.Add(_currentTextBorder);
        }

        public void AddDataPoint(double value)
        {
            Array.Copy(_data, 1, _data, 0, _dataPoints - 1);
            _data[_dataPoints - 1] = value;
            DrawPolygon();
        }

        public void RefreshChart()
        {
            DrawPolygon();
        }

        private void DrawPolygon()
        {
            var h = Canvas.ActualHeight + 1;
            var max = _data.Max() * 1.05;
            if (max > _maxValue)
            {
                _maxValue = max;
                if (ScaleIsWholeNumber)
                {
                    _maxValue = Math.Ceiling(_maxValue);
                }
            }
            var scale = h / _maxValue;

            var points = new PointCollection();
            var x = -1d;
            var w = Canvas.ActualWidth + 3;
            points.Add(new Point(-1, h + 1));
            foreach (var value in _data)
            {
                points.Add(new Point(x, h - (value * scale)));
                x += w / (double) (_dataPoints - 1);
            }
            points.Add(new Point(w + 2, h + 1));
            _chart.Points = points;

            _chartCurrent.X1 = -1;
            _chartCurrent.Y1 = h - (_data[_dataPoints - 1] * scale);
            _chartCurrent.X2 = w + 1;
            _chartCurrent.Y2 = _chartCurrent.Y1;

            _currentText.Text = _formatter.Format(_data[_dataPoints - 1]);
            var y = _chartCurrent.Y1 - 1;
            if (y - (_currentTextBorder.ActualHeight / 2) > 0)
            {
                y -= (_currentTextBorder.ActualHeight / 2 );
            }
            if (y > Canvas.ActualHeight - _currentTextBorder.ActualHeight - 2)
            {
                y = Canvas.ActualHeight - _currentTextBorder.ActualHeight - 2;
            }
            Canvas.SetTop(_currentTextBorder, y);

            YAxisMax.Text = _formatter.Format(_maxValue);
        }

        private void DrawScale()
        {
            var w = Canvas.ActualWidth + 3;
            var h = Canvas.ActualHeight;
            for (int i = 0; i < 9; i++)
            {
                var line = _chartYScale[i];
                line.X1 = -1;
                line.X2 = w + 1;
                line.Y1 = h / 10 * (i + 1);
                line.Y2 = line.Y1;

                line = _chartXScale[i];
                line.X1 = w / 10 * (i + 1);
                line.X2 = line.X1;
                line.Y1 = -1;
                line.Y2 = h + 1;
            }
        }

        private void LineChartSizeChanged(object sender, SizeChangedEventArgs e)
        {
            DrawPolygon();
            DrawScale();
        }
    }
}
