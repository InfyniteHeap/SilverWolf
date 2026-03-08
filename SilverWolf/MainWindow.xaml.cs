using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;

namespace SilverWolf;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        SetCustomTitleBar();
    }

    private void SetCustomTitleBar()
    {
        ExtendsContentIntoTitleBar = true;
        if (ExtendsContentIntoTitleBar)
        {
            AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Tall;
        }
        SetTitleBar(MainWindowTitleBar);
    }
}
