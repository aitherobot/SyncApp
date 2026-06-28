using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;
using FYPSampleApplication;
using Microsoft.Win32;

namespace SyncApp;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private PeerDiscovery? _discovery;
    private PeerTransfer? _transfer;
    private FolderWatcher? _watcher;
    private string _syncFolder = string.Empty;
    
    public MainWindow()
    {
        InitializeComponent();
        
        string documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        string defaultFolder = Path.Combine(documents, "SyncTest");
        
        var settings = AppSettings.Load();
        
        if (!string.IsNullOrWhiteSpace(settings.LastSyncFolder) && Directory.Exists(settings.LastSyncFolder)) 
            _syncFolder = settings.LastSyncFolder;
        else
            _syncFolder = defaultFolder;
        
        Directory.CreateDirectory(_syncFolder);
        FolderPathTextBox.Text = _syncFolder;

        StartSync();
    }

    private void StartSync()
    {
        _discovery = new PeerDiscovery();
        _transfer = new PeerTransfer(_syncFolder, Log);

        _discovery.StartListening();
        _transfer.StartServer();
        
        _watcher = new FolderWatcher(_syncFolder, _discovery, _transfer, Log);
        _watcher.Start();
        _transfer.Watcher = _watcher;

        SetWatching(true);
        StartButton.IsEnabled = false;
        RefreshButton.IsEnabled = true;

        Log("Started watching: " + _syncFolder);
        
        _discovery.StartBroadcasting();
        Log("Broadcasting presence on LAN");

        var broadcastTimer = new DispatcherTimer();
        broadcastTimer.Interval = TimeSpan.FromSeconds(5);
        broadcastTimer.Tick += (_, _) => _discovery.StartBroadcasting();
        broadcastTimer.Start();
        
        var peerTimer = new DispatcherTimer();
        peerTimer.Interval = TimeSpan.FromSeconds(5);
        peerTimer.Tick += (_, _) =>
        {
            int count = _discovery?.DiscoveredPeers.Count ?? 0;
            PeerCountText.Text = $"{count} found";
        };
        peerTimer.Start();
    }

    private void StartButton_OnClick(object sender, RoutedEventArgs e)
    {
        StartSync();
    }

    private void BrowseButton_OnClick(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog
        {
            Title = "Select Sync Folder",
            InitialDirectory = _syncFolder
        };
        
        if (dialog.ShowDialog() == true)
        {
            _syncFolder = dialog.FolderName;
            FolderPathTextBox.Text = _syncFolder;

            new AppSettings { LastSyncFolder = _syncFolder }.Save();
            Log("Sync folder changed to: " + _syncFolder);
        }
    }

    private void RefreshButton_OnClick(object sender, RoutedEventArgs e)
    {
        _discovery?.StartBroadcasting();
        Log("Manual broadcast sent");
    }
    
    private void Log(string message)
    {
        string timeStamped = $"[{DateTime.Now:HH:mm:ss}] {message}";
        Dispatcher.Invoke(() =>
        {
            ActivityLog.Items.Add(timeStamped);
            ActivityLog.ScrollIntoView(ActivityLog.Items[^1]);
        });
    }

    private void SetWatching(bool isWatching)
    {
        if (isWatching)
        {
            StatusText.Text = "Watching";
            StatusText.Foreground = new SolidColorBrush(Color.FromRgb(0x3F, 0xB9, 0x50)); // Green
            StatusDot.Fill = new SolidColorBrush(Color.FromRgb(0x3F, 0xB9, 0x50)); // Green
            var pulse = (Storyboard)FindResource("PulseAnim");
            pulse.Begin(this);
        }
        else
        {
            StatusText.Text = "Idle";
            StatusText.Foreground = new SolidColorBrush(Color.FromRgb(0x6B, 0x82, 0x99));
            StatusDot.Fill = new SolidColorBrush(Color.FromRgb(0x6B, 0x82, 0x99));
            var pulse = (Storyboard)FindResource("PulseAnim");
            pulse.Stop(this);
        }
    }
    
    protected override void OnClosing(CancelEventArgs e)
    {
        _watcher?.SaveSnapshot();
        base.OnClosing(e);
    }
}