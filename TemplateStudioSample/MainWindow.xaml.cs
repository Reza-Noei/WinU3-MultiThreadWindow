using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Hosting;
using TemplateStudioSample.Helpers;

using Windows.UI.ViewManagement;

namespace TemplateStudioSample;

public sealed partial class MainWindow : Window
{
    private Microsoft.UI.Dispatching.DispatcherQueue dispatcherQueue;

    private UISettings settings;

    public MainWindow()
    {
        InitializeComponent();

        // Theme change code picked from https://github.com/microsoft/WinUI-Gallery/pull/1239
        dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
        settings = new UISettings();
        settings.ColorValuesChanged += Settings_ColorValuesChanged; // cannot use FrameworkElement.ActualThemeChanged event
    }

    // this handles updating the caption button colors correctly when windows system theme is changed
    // while the app is open
    private void Settings_ColorValuesChanged(UISettings sender, object args)
    {
        // This calls comes off-thread, hence we will need to dispatch it to current app's thread
        dispatcherQueue.TryEnqueue(() =>
        {
            TitleBarHelper.ApplySystemThemeToCaptionButtons();
        });
    }

    private MyOtherWindow _myOtherWindow;
    private void Button_Click(object sender, RoutedEventArgs e)
    {
        if (_myOtherWindow != null)
            return;

        Title = "On thread " + Environment.CurrentManagedThreadId;
        var thread = new Thread(state =>
        {
            // create a DispatcherQueue on this new thread
            var dq = DispatcherQueueController.CreateOnCurrentThread();

            // initialize xaml in it
            WindowsXamlManager.InitializeForCurrentThread();

            // create a new window
            _myOtherWindow = new MyOtherWindow(); // some other Xaml window

            _myOtherWindow.AppWindow.Show(true);

            // run message pump
            dq.DispatcherQueue.RunEventLoop();
        });
        thread.IsBackground = true; // will be destroyed when main is closed, can be changed
        thread.Start();

        // send some message to the second window
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
