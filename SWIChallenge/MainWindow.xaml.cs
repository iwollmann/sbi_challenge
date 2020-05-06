using OxyPlot;
using OxyPlot.Series;
using SWIChallenge.Extensions;
using SWIChallenge.Models;
using SWIChallenge.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SWIChallenge
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public PlotModel MainChart;
        private PredictModel _PredictModel;
        public bool IsProcessing { get; set; }
        public MainWindow()
        {
            InitializeComponent();
        }

        #region Commands
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDlg = new Microsoft.Win32.OpenFileDialog();
            openFileDlg.DefaultExt = ".csv";
            openFileDlg.Filter = "Csv Data Files (.csv)|*.csv";
            openFileDlg.InitialDirectory = Environment.CurrentDirectory; // TODO: remove this, temporary to facilitate the development

            Nullable<bool> result = openFileDlg.ShowDialog();
            if (result == true)
            {
                tbFileName.Text = openFileDlg.SafeFileName;

                var reader = new CSVReader();
                _PredictModel = await ExecuteBackgroundAsyncProcess<PredictModel>(async () =>
                {
                    var success = await reader.ReadAsync(openFileDlg.FileName);
                    if (success) {
                        var model = new PredictModel(reader.Points);
                        await model.Process();

                        return model;
                    }

                    return null;
                });

                if (_PredictModel != null)
                {
                    if (_PredictModel.IsLoaded)
                    {
                        MainChart = new PlotModel
                        {
                            PlotType = PlotType.Cartesian
                        };

                        var series = new ScatterSeries
                        {
                            Title = "Points",
                            MarkerType = MarkerType.Cross, //better performance, less to render
                            MarkerStrokeThickness = 0.5,
                            MarkerStroke = OxyColors.DarkOliveGreen
                        };

                        series.Points.AddRange(_PredictModel.ValidPoints.ToScatterPoints());
                        MainChart.Series.Add(series);

                        ShowInvalidPoints();

                        pvMainChart.Model = MainChart;
                    }
                }
                else
                {
                    MessageBox.Show("Unable to load data, check the log files");
                    //TODO: Use a better notification window
                }
            }
        }

        private async void Linear_Predict(object sender, RoutedEventArgs e)
        {
            if (MainChart == null || !(_PredictModel?.IsLoaded ?? false))
                return;

            var (coefficients, points) = await ExecuteBackgroundProcess<((double, double), IEnumerable<(double, double)>)>(() => {
                return _PredictModel.Predict<LinearModel>();
            });

            LinearCoefficient.Text = _PredictModel.Format(coefficients);
            PlotLinearSeries(points.ToDataPoints(), "Linear");
        }

        private async void Exponential_Predict(object sender, RoutedEventArgs e)
        {
            if (MainChart == null || !(_PredictModel?.IsLoaded ?? false))
                return;

            var (coefficients, points) = await ExecuteBackgroundProcess<((double, double), IEnumerable<(double, double)>)>(() => {
                return _PredictModel.Predict<ExponentialModel>();
            });

            ExponentialCoefficient.Text = _PredictModel.Format(coefficients);
            PlotLinearSeries(points.ToDataPoints(), "Exponential");
        }

        private async void PowerFc_Predict(object sender, RoutedEventArgs e)
        {
            if (MainChart == null || !(_PredictModel?.IsLoaded ?? false))
                return;

            var (coefficients, points) = await ExecuteBackgroundProcess<((double, double), IEnumerable<(double, double)>)>(() => {
                return _PredictModel.Predict<PowerFunctionModel>();
            });

            PowerFcCoefficient.Text = _PredictModel.Format(coefficients);
            PlotLinearSeries(points.ToDataPoints(), "PowerFunction");
        }

        private void Clear_All(object sender, RoutedEventArgs e)
        {
            LinearCoefficient.Clear();
            ExponentialCoefficient.Clear();
            PowerFcCoefficient.Clear();
            tbFileName.Clear();

            pvMainChart.Model = null;
            _PredictModel = null;
        }

        private void showInvalid_Checked(object sender, RoutedEventArgs e)
        {
            ShowInvalidPoints();
        }
        #endregion

        #region Internal Methods
        private async Task<T> ExecuteBackgroundAsyncProcess<T>(Func<Task<T>> action)
        {
            IsProcessing = true;
            UpdateButtons();
            T result = default(T);
            try
            {
                result = await Task.Run(async () => await action());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            IsProcessing = false;
            UpdateButtons();
            return result;
        }

        private async Task<T> ExecuteBackgroundProcess<T>(Func<T> action)
        {
            IsProcessing = true;
            UpdateButtons();
            T result = default(T);
            try
            {
                result = await Task.Run(() => action());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            IsProcessing = false;
            UpdateButtons();
            return result;
        }
        private void ShowInvalidPoints()
        {
            if (_PredictModel == null)
                return;

            if (
                _PredictModel.IsLoaded &&
                _PredictModel.HasInvalidPoints())
            {
                var invalidSeries = MainChart.Series.OfType<ScatterSeries>().Where(x => x.Title == "Invalid Points").FirstOrDefault();

                if (showInvalid.IsChecked ?? false)
                {
                    if (invalidSeries == null)
                    {
                        invalidSeries = new ScatterSeries
                        {
                            Title = "Invalid Points",
                            MarkerType = MarkerType.Cross, //better performance, less to render
                            MarkerStrokeThickness = 0.5,
                            MarkerStroke = OxyColors.Red,
                        };

                        invalidSeries.Points.AddRange(_PredictModel.InvalidPoints.ToScatterPoints());
                        MainChart.Series.Add(invalidSeries);
                    }
                    else
                    {
                        invalidSeries.Points.AddRange(_PredictModel.InvalidPoints.ToScatterPoints());
                    }
                }
                else
                    MainChart.Series.Remove(invalidSeries);

                MainChart.InvalidatePlot(true);

            }
        }
        private void PlotLinearSeries(IEnumerable<DataPoint> points, string seriesTitle)
        {
            var currentSeries = MainChart.Series.OfType<LineSeries>().Where(x => x.Title == seriesTitle).FirstOrDefault();

            if (currentSeries == null)
            {
                var lineSeries = new LineSeries()
                {
                    Title = seriesTitle,
                    Decimator = Decimator.Decimate
            };

                lineSeries.InterpolationAlgorithm = InterpolationAlgorithms.CanonicalSpline;
                lineSeries.Points.AddRange(points);
                MainChart.Series.Add(lineSeries);
            }
            else
            {
                currentSeries.Points.Clear();
                currentSeries.Points.AddRange(points);
            }

            MainChart.InvalidatePlot(true);
        }

        private void UpdateButtons()
        {
            btnDialog.IsEnabled = !IsProcessing;
            panelCommands.IsEnabled = !IsProcessing;
        }
        #endregion
    }
}
