# TrackPopup 🎵

A small Windows overlay that shows the currently playing media when the track changes.

TrackPopup is designed to be minimal: a compact black popup with a white border, rounded corners, album artwork, track title, and artist. It slides down from the top of the screen, stays visible briefly, and then slides back up.

## Features

- 🎵 Detects Windows media sessions
- 🖼️ Shows album/track artwork when available
- ⬆️ Smooth slide-in / slide-out animation from the top of the screen
- ⬛ Black background with a thin white border
- ◼️ Rounded corners
- 🎮 Does not steal focus from the active application
- 🖥️ Designed for games and fullscreen applications
- 🔔 System tray support
- 🚀 Optional launch at Windows sign-in
- 📦 Self-contained `win-x64` publishing
- 🧩 No Visual Studio is required to run the published executable

## Requirements

For development:

- Windows 10/11
- Visual Studio with the **.NET desktop development** workload
- .NET 8 SDK

For the published application:

- 64-bit Windows 10/11
- No separate .NET installation is required when using the self-contained publish output

## Build from source

1. Open `TrackPopup.sln` in Visual Studio.
2. Select **Release**.
3. Build the solution with **Build → Build Solution**.
4. For a standalone executable, use **Publish** and publish for `win-x64` as a **self-contained** application.
5. The published `TrackPopup.exe` can be copied to any permanent folder and run without Visual Studio.

The project is already configured for self-contained, single-file `win-x64` publishing.

## Autostart

After launching the published executable, use the TrackPopup icon in the Windows system tray and enable **Start with Windows**.

Keep the executable in the same location after enabling autostart because Windows stores its path.

## Repository structure

```text
TrackPopup/
├── App.xaml
├── App.xaml.cs
├── MainWindow.xaml
├── MainWindow.xaml.cs
├── TrackPopup.csproj
├── TrackPopup.sln
├── .gitignore
├── .gitattributes
└── README.md
```

## License

No license has been selected yet. Until a license is added, the source code should not be assumed to be available for unrestricted reuse.
