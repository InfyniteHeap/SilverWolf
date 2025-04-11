using Microsoft.UI.Xaml;

namespace FileEncodingConverter;

public partial class App
{
    public App()
    {
        InitializeComponent();
    }

    public static Window? Window { get; private set; }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        Window = new MainWindow();
        Window.Activate();
    }
}