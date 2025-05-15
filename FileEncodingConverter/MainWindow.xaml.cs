using Microsoft.UI;
using Microsoft.UI.Windowing;
using Windows.Graphics;
using WinRT.Interop;

namespace FileEncodingConverter;

public sealed partial class MainWindow
{
    private const double WindowDimensionScale = 0.6;

    public MainWindow()
    {
        InitializeComponent();

        WindowHandle = WindowNative.GetWindowHandle(this);
        WindowId = Win32Interop.GetWindowIdFromWindow(WindowHandle);

        SetWindowSizeAndPosition();
        EnableExtendedTitleBar();
    }

    public static nint WindowHandle { get; private set; }
    private static WindowId WindowId { get; set; }

    private void SetWindowSizeAndPosition()
    {
        var displayArea = DisplayArea.GetFromWindowId(WindowId, DisplayAreaFallback.Nearest);

        var screenWidth = displayArea.OuterBounds.Width;
        var screenHeight = displayArea.OuterBounds.Height;

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