using DevExpress.WinUI.Charts.Internal;
using DevExpress.WinUI.Charts;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

using TemplateStudioSample.ViewModels;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Dispatching;

namespace TemplateStudioSample.Views;

public sealed partial class MainPage : Page
{
    public DataGenerator Data1
    {
        get;
    }
    public DataGenerator Data2
    {
        get;
    }

    void OnLoaded(object sender, RoutedEventArgs e)
    {
        int pointsCountPerSeries = 100000;
        chart.ActualAxisX.VisualRangeStartValueInternal = pointsCountPerSeries / 10;
        chart.ActualAxisX.VisualRangeEndValueInternal = pointsCountPerSeries / 5;
    }

    public MainViewModel ViewModel
    {
        get;
    }

    public MainPage()
    {
        ViewModel = App.GetService<MainViewModel>();

        int pointsCountPerSeries = 100000;
        Data1 = new DataGenerator(pointsCountPerSeries);
        Data2 = new DataGenerator(pointsCountPerSeries);
        this.InitializeComponent();
        Loaded += OnLoaded;
    }

    private Window _myOtherWindow;

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        if (_myOtherWindow != null)
            return;

        var thread = new Thread(state =>
        {
            // create a DispatcherQueue on this new thread
            var dq = DispatcherQueueController.CreateOnCurrentThread();
            // initialize xaml in it
            WindowsXamlManager.InitializeForCurrentThread();
            // create a new window
            _myOtherWindow = new Window(); // some other Xaml window you've created
            _myOtherWindow.AppWindow.Show(true);
            // run message pump
            dq.DispatcherQueue.RunEventLoop();
        });
        thread.IsBackground = true; // will be destroyed when main window is closed, behavior can be changed
        thread.Start();

        // send some message to the second window to check it's handled from another thread
        // note: real code should wait for _myOtherWindow to be fully initialized...
        Task.Run(async () =>
        {
            for (var i = 0; i < 10; i++)
            {
                await Task.Delay(1000);
                _myOtherWindow.DispatcherQueue.TryEnqueue(() =>
                {
                    _myOtherWindow.Title = "#" + i + " on thread " + Environment.CurrentManagedThreadId;
                });
            }
        });
    }
}

public class DataGenerator : DataSourceBase
{
    List<SeriesPoint> points = new List<SeriesPoint>();
    Random rnd = new Random();
    Random additionalRnd = new Random();

    public List<SeriesPoint> PointsList
    {
        get
        {
            return points;
        }
        set
        {
            if ((value as List<SeriesPoint>) != null)
            {
                points = value;
            }
        }
    }
    protected override int RowsCount
    {
        get
        {
            return points.Count;
        }
    }

    public DataGenerator(int count)
    {
        Random random = new Random();
        double price1 = GenerateStartValue(random);
        points.Add(new SeriesPoint(0, GenerateStartValue(random)));
        for (int i = 0; i < count; i++)
        {
            points.Add(new SeriesPoint(i, price1));
            price1 += GenerateAddition(random);
        }
    }
    double GenerateStartValue(Random random)
    {
        return random.NextDouble() * 100;
    }
    double GenerateAddition(Random random)
    {
        double factor = random.NextDouble();
        if (factor == 1)
            factor = 50;
        else if (factor == 0)
            factor = -50;
        return (factor - 0.5) * 50;
    }
    protected override DateTime GetDateTimeValue(int index, ChartDataMemberType dataMember)
    {
        return DateTime.MinValue;
    }
    protected override object GetKey(int index)
    {
        return null;
    }
    protected override double GetNumericalValue(int index, ChartDataMemberType dataMember)
    {
        if (dataMember == ChartDataMemberType.Argument)
            return points[index].Argument;
        else
            return points[index].Value;
    }
    protected override string GetQualitativeValue(int index, ChartDataMemberType dataMember)
    {
        return null;
    }
    protected override ActualScaleType GetScaleType(ChartDataMemberType dataMember)
    {
        return ActualScaleType.Numerical;
    }
}
public class SeriesPoint
{
    public int Argument
    {
        get; private set;
    }
    public double Value
    {
        get; private set;
    }

    public SeriesPoint(int argument, double value)
    {
        Argument = argument;
        Value = value;
    }
}
