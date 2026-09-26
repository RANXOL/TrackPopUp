using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Windows.Media.Control;
using Windows.Storage.Streams;

namespace TrackPopup;

public partial class MainWindow : Window
{
    private GlobalSystemMediaTransportControlsSessionManager? _manager;
    private GlobalSystemMediaTransportControlsSession? _session;
    private readonly System.Windows.Threading.DispatcherTimer _hideTimer;
    private bool _isHiding;
    private string? _lastTrackId;

    public MainWindow()
    {
        InitializeComponent();

        _hideTimer = new System.Windows.Threading.DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(3)
        };
        _hideTimer.Tick += (_, _) =>
        {
            _hideTimer.Stop();
            HidePopup();
        };
    }

    public async Task StartMediaWatcherAsync()
    {
        try
        {
            _manager = await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();
            _manager.CurrentSessionChanged += Manager_CurrentSessionChanged;
            _manager.SessionsChanged += Manager_SessionsChanged;
            await SetCurrentSessionAsync();
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(
                $"Не удалось подключиться к Windows Media Session API.\n\n{ex.Message}",
                "TrackPopup",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private async void Manager_CurrentSessionChanged(GlobalSystemMediaTransportControlsSessionManager sender, CurrentSessionChangedEventArgs args)
        => await Dispatcher.InvokeAsync(SetCurrentSessionAsync);

    private async void Manager_SessionsChanged(GlobalSystemMediaTransportControlsSessionManager sender, SessionsChangedEventArgs args)
        => await Dispatcher.InvokeAsync(SetCurrentSessionAsync);

    private async Task SetCurrentSessionAsync()
    {
        if (_manager is null) return;

        var newSession = _manager.GetCurrentSession();

        if (_session != null)
        {
            _session.MediaPropertiesChanged -= Session_MediaPropertiesChanged;
        }

        _session = newSession;

        if (_session == null)
        {
            HidePopup();
            return;
        }

        _session.MediaPropertiesChanged += Session_MediaPropertiesChanged;
        await RefreshTrackAsync(showPopup: true);
    }

    private async void Session_MediaPropertiesChanged(GlobalSystemMediaTransportControlsSession sender, MediaPropertiesChangedEventArgs args)
        => await Dispatcher.InvokeAsync(async () => await RefreshTrackAsync(showPopup: true));

    private async Task RefreshTrackAsync(bool showPopup)
    {
        if (_session is null) return;

        try
        {
            var properties = await _session.TryGetMediaPropertiesAsync();

            string title = string.IsNullOrWhiteSpace(properties.Title) ? "Unknown title" : properties.Title;
            string artist = string.IsNullOrWhiteSpace(properties.Artist) ? "Unknown artist" : properties.Artist;
            string trackId = $"{title}\u001F{artist}";
            bool changed = _lastTrackId != trackId;
            _lastTrackId = trackId;

            TitleText.Text = title;
            ArtistText.Text = artist;
            await LoadThumbnailAsync(properties);

            if (showPopup && changed)
                ShowPopup();
        }
        catch
        {
            // Media applications can disappear while properties are being queried.
        }
    }

    private async Task LoadThumbnailAsync(GlobalSystemMediaTransportControlsSessionMediaProperties properties)
    {
        try
        {
            if (properties.Thumbnail is null)
            {
                CoverImage.Source = null;
                return;
            }

            using IRandomAccessStream stream = await properties.Thumbnail.OpenReadAsync();
            if (stream.Size == 0)
            {
                CoverImage.Source = null;
                return;
            }

            // Read the WinRT stream directly; no AsStream/AsStreamForRead extension is required.
            var reader = new DataReader(stream.GetInputStreamAt(0));
            uint size = checked((uint)stream.Size);
            await reader.LoadAsync(size);
            byte[] bytes = new byte[size];
            reader.ReadBytes(bytes);
            reader.Dispose();

            using var memory = new MemoryStream(bytes, writable: false);
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.StreamSource = memory;
            bitmap.EndInit();
            bitmap.Freeze();
            CoverImage.Source = bitmap;
        }
        catch
        {
            CoverImage.Source = null;
        }
    }

    private void ShowPopup()
    {
        _isHiding = false;
        _hideTimer.Stop();

        var workArea = SystemParameters.WorkArea;
        double targetTop = workArea.Top + 45;
        double startTop = workArea.Top - Height - 4;

        BeginAnimation(TopProperty, null);
        BeginAnimation(OpacityProperty, null);

        Left = workArea.Left + (workArea.Width - Width) / 2;
        Top = startTop;
        Opacity = 0;
        Show();

        var ease = new CubicEase { EasingMode = EasingMode.EaseOut };
        BeginAnimation(TopProperty, new DoubleAnimation(startTop, targetTop, TimeSpan.FromMilliseconds(280)) { EasingFunction = ease });
        BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(220)) { EasingFunction = ease });
        _hideTimer.Start();
    }

    private void HidePopup()
    {
        if (!IsVisible || _isHiding) return;

        _isHiding = true;
        _hideTimer.Stop();

        var workArea = SystemParameters.WorkArea;
        double endTop = workArea.Top - Height - 4;
        double currentTop = Top;

        BeginAnimation(TopProperty, null);
        BeginAnimation(OpacityProperty, null);

        var ease = new CubicEase { EasingMode = EasingMode.EaseIn };
        var slideOut = new DoubleAnimation(currentTop, endTop, TimeSpan.FromMilliseconds(240)) { EasingFunction = ease };
        slideOut.Completed += (_, _) =>
        {
            _isHiding = false;
            Hide();
            BeginAnimation(TopProperty, null);
            BeginAnimation(OpacityProperty, null);
            Opacity = 0;
        };

        BeginAnimation(TopProperty, slideOut);
        BeginAnimation(OpacityProperty, new DoubleAnimation(Opacity, 0, TimeSpan.FromMilliseconds(180)) { EasingFunction = ease });
    }

    public void Dispose()
    {
        _hideTimer.Stop();
        if (_session != null)
            _session.MediaPropertiesChanged -= Session_MediaPropertiesChanged;
        if (_manager != null)
        {
            _manager.CurrentSessionChanged -= Manager_CurrentSessionChanged;
            _manager.SessionsChanged -= Manager_SessionsChanged;
        }
    }
}
