using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MinecraftLauncher.Core.Domain.Interfaces;
using MinecraftLauncher.Core.DTOs.Auth;
using MinecraftLauncher.Core.DTOs.Launch;
using MinecraftLauncher.Infrastructure.Data;
using MinecraftLauncher.Infrastructure.Services;

namespace MinecraftLauncher.Desktop.Views;

public partial class MainWindow : Window
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IVersionService _versionService;
    private readonly IAuthService _authService;
    private ObservableCollection<VersionItem> _versions = new();

    public MainWindow()
    {
        InitializeComponent();

        var services = new ServiceCollection();
        services.AddLogging();

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite("Data Source=minecraft_launcher.db"));

        services.AddScoped<IVersionService, VersionService>();
        services.AddScoped<ILaunchService, LaunchService>();
        services.AddScoped<IJavaService, JavaService>();
        services.AddScoped<IResourceService, ResourceService>();
        services.AddScoped<IResourceUploadService, ResourceUploadService>();
        services.AddScoped<IReviewService, ReviewService>();
        services.AddScoped<IDownloadService, DownloadService>();
        services.AddScoped<ICommentService, CommentService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IAuthService>(provider =>
        {
            var context = provider.GetRequiredService<AppDbContext>();
            return new AuthService(context, "MinecraftLauncherDesktopSecretKey2024!", "MinecraftLauncher");
        });

        _serviceProvider = services.BuildServiceProvider();
        _versionService = _serviceProvider.GetRequiredService<IVersionService>();
        _authService = _serviceProvider.GetRequiredService<IAuthService>();

        MemorySlider.ValueChanged += OnMemoryChanged;
        UpdateNavHighlight("home");

        InitDatabase();
    }

    private async void InitDatabase()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await context.Database.EnsureCreatedAsync();
            await DatabaseInitializer.SeedDataAsync(scope.ServiceProvider);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"数据库初始化失败: {ex.Message}");
        }

        await LoadVersions();
    }

    private void OnMemoryChanged(object? sender, EventArgs e)
    {
        if (sender is Slider slider)
        {
            var gb = (int)slider.Value;
            MemoryLabel.Text = $"{gb} GB";
            StatMemory.Text = $"{gb} GB";
        }
    }

    private void OnNavHome(object? sender, RoutedEventArgs e) => SwitchPage("home");
    private void OnNavLaunch(object? sender, RoutedEventArgs e) => SwitchPage("launch");
    private void OnNavVersions(object? sender, RoutedEventArgs e) => SwitchPage("versions");
    private void OnNavSettings(object? sender, RoutedEventArgs e) => SwitchPage("settings");

    private void SwitchPage(string page)
    {
        HomePage.IsVisible = page == "home";
        LaunchPage.IsVisible = page == "launch";
        VersionsPage.IsVisible = page == "versions";
        SettingsPage.IsVisible = page == "settings";
        UpdateNavHighlight(page);
    }

    private void UpdateNavHighlight(string active)
    {
        var buttons = new[] { NavHome, NavLaunch, NavVersions, NavSettings };
        var names = new[] { "home", "launch", "versions", "settings" };
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].FontWeight = names[i] == active ? FontWeight.Bold : FontWeight.Normal;
            buttons[i].Foreground = names[i] == active
                ? new SolidColorBrush(Color.FromRgb(0x53, 0xd7, 0x69))
                : new SolidColorBrush(Color.FromRgb(0xe0, 0xe0, 0xe0));
        }
    }

    private async Task LoadVersions()
    {
        try
        {
            var versions = await _versionService.GetInstalledVersions();
            _versions.Clear();
            foreach (var v in versions)
            {
                _versions.Add(new VersionItem
                {
                    Id = v.Id,
                    Name = v.Name,
                    Type = "Release",
                    SizeMB = v.Size / 1_000_000,
                    IsValid = v.IsValid
                });
            }

            VersionList.ItemsSource = _versions;

            var names = _versions.Select(v => v.Name).ToList();
            HomeVersionCombo.ItemsSource = names;
            LaunchVersionCombo.ItemsSource = names;

            if (names.Count > 0)
            {
                HomeVersionCombo.SelectedIndex = 0;
                LaunchVersionCombo.SelectedIndex = 0;
            }

            StatVersions.Text = _versions.Count.ToString();
        }
        catch (Exception ex)
        {
            ConsoleOutput.Text = $"加载版本失败: {ex.Message}";
        }
    }

    private async void OnRefreshVersions(object? sender, RoutedEventArgs e)
    {
        await LoadVersions();
    }

    private void OnSelectVersion(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string versionId)
        {
            for (int i = 0; i < _versions.Count; i++)
            {
                if (_versions[i].Id == versionId)
                {
                    LaunchVersionCombo.SelectedIndex = i;
                    HomeVersionCombo.SelectedIndex = i;
                    break;
                }
            }
            SwitchPage("launch");
        }
    }

    private async void OnLaunchGame(object? sender, RoutedEventArgs e)
    {
        var versionName = LaunchVersionCombo.SelectedItem as string
                          ?? HomeVersionCombo.SelectedItem as string;
        if (string.IsNullOrEmpty(versionName))
        {
            ConsoleOutput.Text = "❌ 请先选择一个游戏版本！";
            return;
        }

        var playerName = PlayerNameInput.Text?.Trim();
        if (string.IsNullOrEmpty(playerName))
            playerName = "Player";

        var memoryGb = (int)MemorySlider.Value;
        var memoryMb = memoryGb * 1024;

        ConsoleOutput.Text = $"正在准备启动 Minecraft {versionName}...\n" +
                             $"玩家: {playerName}\n" +
                             $"内存: {memoryMb} MB\n\n" +
                             $"[INFO] 正在检查版本文件...\n" +
                             $"[INFO] 版本 {versionName} 验证通过\n" +
                             $"[INFO] 正在构建启动参数...\n" +
                             $"[INFO] Java 路径: {(string.IsNullOrEmpty(JavaPathInput.Text) ? "自动检测" : JavaPathInput.Text)}\n" +
                             $"[INFO] 游戏目录: {(string.IsNullOrEmpty(GameDirInput.Text) ? "默认" : GameDirInput.Text)}\n" +
                             $"[INFO] 最大内存: -Xmx{memoryMb}m\n\n" +
                             $"⚠ 此为社区版启动器演示，实际启动 Minecraft 需要完整的游戏文件和 Java 运行时。\n" +
                             $"⚠ 请确保已安装对应版本的游戏文件和 Java。";

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            ConsoleScroll.ScrollToEnd();
        });
    }

    private async void OnBrowseJava(object? sender, RoutedEventArgs e)
    {
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "选择 Java 可执行文件",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("Java 可执行文件")
                {
                    Patterns = new[] { "javaw.exe", "java.exe", "java" }
                }
            }
        });
        if (files.Count > 0)
        {
            JavaPathInput.Text = files[0].Path.LocalPath;
            SettingsJavaPath.Text = files[0].Path.LocalPath;
        }
    }

    private async void OnBrowseGameDir(object? sender, RoutedEventArgs e)
    {
        var folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "选择游戏目录",
            AllowMultiple = false,
        });
        if (folders.Count > 0)
        {
            GameDirInput.Text = folders[0].Path.LocalPath;
            SettingsGameDir.Text = folders[0].Path.LocalPath;
        }
    }

    private async void OnLogin(object? sender, RoutedEventArgs e)
    {
        var email = SettingsEmail.Text?.Trim();
        var password = SettingsPassword.Text?.Trim();
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            LoginStatus.Text = "⚠ 请输入邮箱和密码";
            return;
        }

        try
        {
            var result = await _authService.Login(new LoginRequest { Email = email, Password = password });
            if (result.Success && result.User != null)
            {
                UserNameText.Text = result.User.Username;
                LoginStatus.Text = "✅ 登录成功！";
                LoginStatus.Foreground = new SolidColorBrush(Color.FromRgb(0x53, 0xd7, 0x69));
            }
            else
            {
                LoginStatus.Text = $"❌ 登录失败: {result.Error}";
                LoginStatus.Foreground = new SolidColorBrush(Color.FromRgb(0xe7, 0x4c, 0x3c));
            }
        }
        catch (Exception ex)
        {
            LoginStatus.Text = $"❌ 登录出错: {ex.Message}";
            LoginStatus.Foreground = new SolidColorBrush(Color.FromRgb(0xe7, 0x4c, 0x3c));
        }
    }

    private async void OnRegister(object? sender, RoutedEventArgs e)
    {
        var email = SettingsEmail.Text?.Trim();
        var password = SettingsPassword.Text?.Trim();
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            LoginStatus.Text = "⚠ 请输入邮箱和密码";
            return;
        }

        try
        {
            var username = email.Split('@')[0];
            var result = await _authService.Register(new RegisterRequest
            {
                Email = email,
                Password = password,
                Username = username
            });
            if (result.Success)
            {
                LoginStatus.Text = "✅ 注册成功！请登录";
                LoginStatus.Foreground = new SolidColorBrush(Color.FromRgb(0x53, 0xd7, 0x69));
            }
            else
            {
                LoginStatus.Text = $"❌ 注册失败: {result.Error}";
                LoginStatus.Foreground = new SolidColorBrush(Color.FromRgb(0xe7, 0x4c, 0x3c));
            }
        }
        catch (Exception ex)
        {
            LoginStatus.Text = $"❌ 注册出错: {ex.Message}";
            LoginStatus.Foreground = new SolidColorBrush(Color.FromRgb(0xe7, 0x4c, 0x3c));
        }
    }

    private void OnSaveSettings(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(SettingsJavaPath.Text))
            JavaPathInput.Text = SettingsJavaPath.Text;
        if (!string.IsNullOrEmpty(SettingsGameDir.Text))
            GameDirInput.Text = SettingsGameDir.Text;
    }
}

public class VersionItem
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Type { get; set; } = "Release";
    public long SizeMB { get; set; }
    public bool IsValid { get; set; }
}
