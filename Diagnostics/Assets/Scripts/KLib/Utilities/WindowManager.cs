using System;
using System.Runtime.InteropServices;
using UnityEngine;

public static class WindowManager
{
    [DllImport("user32.dll")] static extern IntPtr GetActiveWindow();
    [DllImport("user32.dll")] static extern bool SetForegroundWindow(IntPtr hWnd);
    [DllImport("user32.dll")]
    static extern bool SetWindowPos(IntPtr hWnd, IntPtr insert,
        int x, int y, int cx, int cy, uint flags);
    [DllImport("user32.dll")] static extern int GetWindowLong(IntPtr hWnd, int index);
    [DllImport("user32.dll")] static extern int SetWindowLong(IntPtr hWnd, int index, int value);

    const int GWL_STYLE = -16;
    const int WS_CAPTION = 0x00C00000;  // title bar + border
    const int WS_THICKFRAME = 0x00040000; // resizable frame
    const uint SWP_SHOWWINDOW = 0x0040;
    const uint SWP_FRAMECHANGED = 0x0020;  // forces style recalc

    static IntPtr Handle =>
        System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;

    /// <summary>
    /// Call once at startup. Removes chrome and centers the window.
    /// </summary>
    public static void InitializeWindow(int width, int height)
    {
        var hwnd = Handle;

        // Strip title bar and resize border — window becomes immovable
        int style = GetWindowLong(hwnd, GWL_STYLE);
        style &= ~(WS_CAPTION | WS_THICKFRAME);
        SetWindowLong(hwnd, GWL_STYLE, style);

        // Center on the display
        int x = (Screen.currentResolution.width - width) / 2;
        int y = (Screen.currentResolution.height - height) / 2;
        SetWindowPos(hwnd, IntPtr.Zero, x, y, width, height,
            SWP_SHOWWINDOW | SWP_FRAMECHANGED);
    }

    /// <summary>
    /// Bring this app's window to the foreground.
    /// Each app calls this on itself — never on the other process.
    /// </summary>
    public static void BringToFront()
    {
        SetForegroundWindow(Handle);
    }
}