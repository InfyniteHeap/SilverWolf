using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;

using Windows.Graphics;

using WinRT.Interop;

namespace SilverWolf;

public sealed partial class MainWindow : Window {
    private const double WindowDimensionScale = 0.75;

    private readonly nint _windowHandle;
    private readonly WindowId _windowId;

    public MainWindow() {
        _windowHandle = WindowNative.GetWindowHandle(this);
        _windowId = Win32Interop.GetWindowIdFromWindow(_windowHandle);

        InitializeComponent();

        SetWindowSizeAndPosition();
        SetCustomTitleBar();
    }

    private void SetWindowSizeAndPosition() {
        var displayArea = DisplayArea.GetFromWindowId(_windowId, DisplayAreaFallback.Nearest);

        var workArea = displayArea.WorkArea;

        var windowWidth = workArea.Width * WindowDimensionScale;
        var windowHeight = workArea.Height * WindowDimensionScale;

        var xPosition = workArea.X + (workArea.Width - windowWidth) / 2.0;
        var yPosition = workArea.Y + (workArea.Height - windowHeight) / 2.0;

        AppWindow.MoveAndResize(new RectInt32((int)xPosition, (int)yPosition, (int)windowWidth, (int)windowHeight));
    }

    private void SetCustomTitleBar() {
        ExtendsContentIntoTitleBar = true;
        AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Tall;
        SetTitleBar(MainWindowTitleBar);
    }
}
