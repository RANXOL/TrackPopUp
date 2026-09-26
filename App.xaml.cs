using Microsoft.Win32;
using System;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;

namespace TrackPopup;

public partial class App : System.Windows.Application
{
    private MainWindow? _window;
    private NotifyIcon? _trayIcon;
    private ToolStripMenuItem? _startupItem;

    private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string AppName = "TrackPopup";

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _window = new MainWindow();
        _window.Show();
        _window.Hide();

        CreateTrayIcon();
        _ = _window.StartMediaWatcherAsync();
    }

    private void CreateTrayIcon()
    {
        _startupItem = new ToolStripMenuItem("Запускать с Windows")
        {
            Checked = IsStartupEnabled(),
            CheckOnClick = true
        };
        _startupItem.Click += (_, _) => SetStartup(_startupItem.Checked);

        var menu = new ContextMenuStrip();
        menu.Items.Add(_startupItem);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Выход", null, (_, _) => Shutdown());

        _trayIcon = new NotifyIcon
        {
            Icon = SystemIcons.Application,
            Text = "TrackPopup",
            Visible = true,
            ContextMenuStrip = menu
        };
    }

    private static bool IsStartupEnabled()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKey, writable: false);
        var value = key?.GetValue(AppName)?.ToString();
        return !string.IsNullOrWhiteSpace(value);
    }

    private static void SetStartup(bool enabled)
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKey, writable: true)
            ?? Registry.CurrentUser.CreateSubKey(RunKey);

        if (enabled)
        {
            var exePath = Environment.ProcessPath;
            if (!string.IsNullOrWhiteSpace(exePath))
                key.SetValue(AppName, $"\"{exePath}\"");
        }
        else
        {
            key.DeleteValue(AppName, throwOnMissingValue: false);
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _trayIcon?.Dispose();
        _window?.Dispose();
        base.OnExit(e);
    }
}
