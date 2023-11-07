using Microsoft.UI.Xaml.Controls;

using TemplateStudioSample.ViewModels;

namespace TemplateStudioSample.Views;

public sealed partial class BlankPage : Page
{
    public BlankViewModel ViewModel
    {
        get;
    }

    public BlankPage()
    {
        ViewModel = App.GetService<BlankViewModel>();
        InitializeComponent();
    }
}
