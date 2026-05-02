using Microsoft.UI.Xaml;

namespace SilverWolf;

public partial class App : Application {
    public static Window? MainWindow { get; private set; }

    public App() {
        InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args) {
        MainWindow = new MainWindow();
        MainWindow.Activate();
    }
}
