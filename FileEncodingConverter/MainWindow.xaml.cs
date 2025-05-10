using Microsoft.UI.Windowing;
using Windows.Graphics;
using Windows.Win32;
using Windows.Win32.UI.WindowsAndMessaging;

namespace FileEncodingConverter;

public sealed partial class MainWindow
{
    private const double WindowDimensionScale = 0.6;

    public MainWindow()
    {
        InitializeComponent();

        SetWindowSizeAndPosition();
        EnableExtendedTitleBar();
    }

    private void SetWindowSizeAndPosition()
    {
        var screenWidth = (double)PInvoke.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_CXSCREEN);
        var screenHeight = (double)PInvoke.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_CYSCREEN);

        var windowWidth = screenWidth * WindowDimensionScale;
        var windowHeight = screenHeight * WindowDimensionScale;

        var xPosition = (screenWidth - windowWidth) / 2.0;
        var yPosition = (screenHeight - windowHeight) / 2.0;

        AppWindow.MoveAndResize(new RectInt32((int)xPosition, (int)yPosition, (int)windowWidth, (int)windowHeight));
    }

    private void EnableExtendedTitleBar()
    {
        ExtendsContentIntoTitleBar = true;
        AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Tall;
    }
}