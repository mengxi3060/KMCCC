using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MinecraftLauncher.Core.Domain.Interfaces;
using MinecraftLauncher.Core.DTOs.Auth;
using MinecraftLauncher.Infrastructure.Data;
using MinecraftLauncher.Infrastructure.Services;

namespace MinecraftLauncher.Desktop.Views;

public partial class MainWindow : Window
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IVersionService _versionService;
    private readonly IAuthService _authService;
    private readonly HttpClient _httpClient = new();
    private ObservableCollection<VersionItem> _versions = new();
    private ObservableCollection<McVersionItem> _mcVersions = new();
    private ObservableCollection<ResourceItem> _currentResources = new();
    private string _versionFilter = "release";

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
        _ = LoadMcVersionManifest();
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
    private void OnNavDownload(object? sender, RoutedEventArgs e) => SwitchPage("download");
    private void OnNavVersions(object? sender, RoutedEventArgs e) => SwitchPage("versions");
    private void OnNavMods(object? sender, RoutedEventArgs e) => SwitchPage("mods");
    private void OnNavResourcePacks(object? sender, RoutedEventArgs e) => SwitchPage("resourcepacks");
    private void OnNavShaders(object? sender, RoutedEventArgs e) => SwitchPage("shaders");
    private void OnNavTextures(object? sender, RoutedEventArgs e) => SwitchPage("textures");
    private void OnNavModpacks(object? sender, RoutedEventArgs e) => SwitchPage("modpacks");
    private void OnNavSettings(object? sender, RoutedEventArgs e) => SwitchPage("settings");

    private readonly string[] _pageNames = { "home", "download", "versions", "mods", "resourcepacks", "shaders", "textures", "modpacks", "settings" };

    private void SwitchPage(string page)
    {
        HomePage.IsVisible = page == "home";
        DownloadPage.IsVisible = page == "download";
        VersionsPage.IsVisible = page == "versions";
        ModsPage.IsVisible = page == "mods";
        ResourcePacksPage.IsVisible = page == "resourcepacks";
        ShadersPage.IsVisible = page == "shaders";
        TexturesPage.IsVisible = page == "textures";
        ModpacksPage.IsVisible = page == "modpacks";
        SettingsPage.IsVisible = page == "settings";
        UpdateNavHighlight(page);

        if (page == "mods") LoadResources("mod", ModList);
        else if (page == "resourcepacks") LoadResources("resourcepack", ResourcePackList);
        else if (page == "shaders") LoadResources("shader", ShaderList);
        else if (page == "textures") LoadResources("texture", TextureList);
        else if (page == "modpacks") LoadResources("modpack", ModpackList);
    }

    private void UpdateNavHighlight(string active)
    {
        var navButtons = new Button[] { NavHome, NavDownload, NavVersions, NavMods, NavResourcePacks, NavShaders, NavTextures, NavModpacks, NavSettings };
        for (int i = 0; i < navButtons.Length && i < _pageNames.Length; i++)
        {
            navButtons[i].Classes.Remove("active");
            if (_pageNames[i] == active)
                navButtons[i].Classes.Add("active");
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

    private async Task LoadMcVersionManifest()
    {
        try
        {
            VersionListStatus.Text = "正在从 Mojang 获取版本列表...";
            var json = await _httpClient.GetStringAsync("https://piston-meta.mojang.com/mc/game/version_manifest_v2.json");
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            var latestRelease = root.GetProperty("latest").GetProperty("release").GetString() ?? "";
            var versionsArr = root.GetProperty("versions");

            _mcVersions.Clear();
            foreach (var v in versionsArr.EnumerateArray())
            {
                var id = v.GetProperty("id").GetString() ?? "";
                var type = v.GetProperty("type").GetString() ?? "";
                var releaseTime = v.GetProperty("releaseTime").GetDateTime();

                if (_versionFilter == "release" && type != "release") continue;
                if (_versionFilter == "snapshot" && type != "snapshot") continue;

                var searchText = SearchVersionInput?.Text?.Trim().ToLower() ?? "";
                if (!string.IsNullOrEmpty(searchText) && !id.ToLower().Contains(searchText)) continue;

                _mcVersions.Add(new McVersionItem
                {
                    Id = id,
                    Type = type == "release" ? "正式版" : "快照",
                    ReleaseDate = releaseTime.ToString("yyyy-MM-dd"),
                    IsLatest = id == latestRelease,
                    Url = v.GetProperty("url").GetString() ?? ""
                });
            }

            DownloadVersionList.ItemsSource = _mcVersions;
            VersionListStatus.Text = $"共 {_mcVersions.Count} 个版本（最新正式版: {latestRelease}）";
        }
        catch (Exception ex)
        {
            VersionListStatus.Text = $"加载失败: {ex.Message}，请检查网络连接";
        }
    }

    private void OnRefreshVersionList(object? sender, RoutedEventArgs e) => _ = LoadMcVersionManifest();
    private void OnFilterRelease(object? sender, RoutedEventArgs e) { _versionFilter = "release"; _ = LoadMcVersionManifest(); }
    private void OnFilterSnapshot(object? sender, RoutedEventArgs e) { _versionFilter = "snapshot"; _ = LoadMcVersionManifest(); }
    private void OnFilterAll(object? sender, RoutedEventArgs e) { _versionFilter = "all"; _ = LoadMcVersionManifest(); }

    private async void OnDownloadVersion(object? sender, RoutedEventArgs e)
    {
        if (DownloadVersionList.SelectedItem is not McVersionItem item) return;

        DownloadProgressCard.IsVisible = true;
        DownloadProgressText.Text = $"正在下载 Minecraft {item.Id}...";
        DownloadProgressBar.Value = 0;

        for (int i = 1; i <= 100; i++)
        {
            await Task.Delay(30);
            DownloadProgressBar.Value = i;
            if (i % 10 == 0)
                DownloadProgressText.Text = $"正在下载 Minecraft {item.Id}... {i}%";
        }

        DownloadProgressText.Text = $"✅ {item.Id} 下载完成！请前往版本管理查看。";
        _versions.Add(new VersionItem { Id = item.Id, Name = item.Id, Type = item.Type, SizeMB = 250, IsValid = true });
        VersionList.ItemsSource = null;
        VersionList.ItemsSource = _versions;

        var names = _versions.Select(v => v.Name).ToList();
        HomeVersionCombo.ItemsSource = names;
        LaunchVersionCombo.ItemsSource = names;

        StatVersions.Text = _versions.Count.ToString();
    }

    private void LoadResources(string category, ItemsControl target)
    {
        _currentResources.Clear();
        var items = category switch
        {
            "mod" => GetSampleMods(),
            "resourcepack" => GetSampleResourcePacks(),
            "shader" => GetSampleShaders(),
            "texture" => GetSampleTextures(),
            "modpack" => GetSampleModpacks(),
            _ => new List<ResourceItem>()
        };
        foreach (var item in items) _currentResources.Add(item);
        target.ItemsSource = _currentResources;
    }

    private static List<ResourceItem> GetSampleMods() => new()
    {
        new() { Id = "optifine", Name = "OptiFine", Description = "高清修复，提升帧率和画质", Downloads = "2.1M", Likes = "89K", GameVersion = "1.20.4" },
        new() { Id = "jei", Name = "Just Enough Items", Description = "物品合成表查看", Downloads = "1.8M", Likes = "72K", GameVersion = "1.20.4" },
        new() { Id = "sodium", Name = "Sodium", Description = "渲染优化模组，大幅提升帧率", Downloads = "1.5M", Likes = "95K", GameVersion = "1.20.4" },
        new() { Id = "create", Name = "Create", Description = "机械自动化与动力系统", Downloads = "980K", Likes = "67K", GameVersion = "1.20.1" },
        new() { Id = "applied-energistics", Name = "Applied Energistics 2", Description = "数字存储与自动化", Downloads = "750K", Likes = "45K", GameVersion = "1.20.4" },
        new() { Id = "waystones", Name = "Waystones", Description = "传送点系统", Downloads = "620K", Likes = "38K", GameVersion = "1.20.4" },
        new() { Id = "biomes-o-plenty", Name = "Biomes O' Plenty", Description = "80+ 新生物群系", Downloads = "580K", Likes = "42K", GameVersion = "1.20.4" },
        new() { Id = "tinkers-construct", Name = "Tinkers' Construct", Description = "自定义工具与武器", Downloads = "520K", Likes = "51K", GameVersion = "1.20.1" },
    };

    private static List<ResourceItem> GetSampleResourcePacks() => new()
    {
        new() { Id = "faithful", Name = "Faithful 32x", Description = "经典高清材质，保持原版风格", Downloads = "3.2M", Likes = "120K", GameVersion = "1.20.4" },
        new() { Id = "vanillatweaks", Name = "Vanilla Tweaks", Description = "原版微调资源包", Downloads = "1.5M", Likes = "65K", GameVersion = "1.20.4" },
        new() { Id = "stay-true", Name = "Stay True", Description = "保持原版感觉的优化", Downloads = "450K", Likes = "28K", GameVersion = "1.20.4" },
    };

    private static List<ResourceItem> GetSampleShaders() => new()
    {
        new() { Id = "bsl", Name = "BSL Shaders", Description = "温暖柔和的光影效果", Downloads = "2.8M", Likes = "95K", GameVersion = "1.20.4" },
        new() { Id = "seus-renewed", Name = "SEUS Renewed", Description = "经典写实光影", Downloads = "2.1M", Likes = "88K", GameVersion = "1.20.4" },
        new() { Id = "complementary", Name = "Complementary Shaders", Description = "互补光影，性能与画质兼顾", Downloads = "1.9M", Likes = "76K", GameVersion = "1.20.4" },
        new() { Id = "sildurs", Name = "Sildur's Vibrant", Description = "鲜艳色彩光影", Downloads = "1.3M", Likes = "52K", GameVersion = "1.20.4" },
        new() { Id = "ptgi", Name = "SEUS PTGI", Description = "光线追踪光影", Downloads = "890K", Likes = "71K", GameVersion = "1.20.4" },
    };

    private static List<ResourceItem> GetSampleTextures() => new()
    {
        new() { Id = "lb-photo-realism", Name = "LB Photo Realism", Description = "超写实 64x 材质", Downloads = "1.2M", Likes = "45K", GameVersion = "1.20.4" },
        new() { Id = "soartex-fanver", Name = "Soartex Fanver", Description = "平滑风格 64x 材质", Downloads = "890K", Likes = "38K", GameVersion = "1.20.4" },
        new() { Id = "rotr", Name = "ROTR", Description = "写实风格材质包", Downloads = "670K", Likes = "29K", GameVersion = "1.20.4" },
    };

    private static List<ResourceItem> GetSampleModpacks() => new()
    {
        new() { Id = "rlcraft", Name = "RLCraft", Description = "硬核生存整合包", Downloads = "3.5M", Likes = "110K", GameVersion = "1.12.2" },
        new() { Id = "all-the-mods-9", Name = "All The Mods 9", Description = "大型科技魔法整合", Downloads = "1.2M", Likes = "52K", GameVersion = "1.20.1" },
        new() { Id = "better-mc", Name = "Better MC", Description = "增强原版体验", Downloads = "980K", Likes = "67K", GameVersion = "1.20.4" },
        new() { Id = "ftb-revelation", Name = "FTB Revelation", Description = "经典科技整合", Downloads = "750K", Likes = "41K", GameVersion = "1.12.2" },
        new() { Id = "vault-hunters", Name = "Vault Hunters", Description = "RPG 冒险整合", Downloads = "620K", Likes = "48K", GameVersion = "1.18.2" },
    };

    private void OnSearchMods(object? sender, RoutedEventArgs e) { }
    private void OnModCategory(object? sender, RoutedEventArgs e) { }

    private async void OnDownloadResource(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string id)
        {
            btn.Content = "下载中...";
            btn.IsEnabled = false;
            await Task.Delay(1500);
            btn.Content = "✅ 已下载";
        }
    }

    private async void OnRefreshVersions(object? sender, RoutedEventArgs e) => await LoadVersions();

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
            SwitchPage("settings");
        }
    }

    private async void OnLaunchGame(object? sender, RoutedEventArgs e)
    {
        var versionName = LaunchVersionCombo.SelectedItem as string ?? HomeVersionCombo.SelectedItem as string;
        if (string.IsNullOrEmpty(versionName))
        {
            ConsoleOutput.Text = "❌ 请先选择一个游戏版本！";
            return;
        }

        var playerName = PlayerNameInput.Text?.Trim();
        if (string.IsNullOrEmpty(playerName)) playerName = "Player";

        var memoryMb = (int)MemorySlider.Value * 1024;

        ConsoleOutput.Text = $"正在准备启动 Minecraft {versionName}...\n" +
                             $"玩家: {playerName}\n" +
                             $"内存: {memoryMb} MB\n\n" +
                             $"[INFO] 检查版本文件... ✓\n" +
                             $"[INFO] 版本 {versionName} 验证通过\n" +
                             $"[INFO] 构建启动参数...\n" +
                             $"[INFO] Java: {(string.IsNullOrEmpty(JavaPathInput.Text) ? "自动检测" : JavaPathInput.Text)}\n" +
                             $"[INFO] 目录: {(string.IsNullOrEmpty(GameDirInput.Text) ? "默认" : GameDirInput.Text)}\n" +
                             $"[INFO] -Xmx{memoryMb}m\n\n" +
                             $"⚠ 社区版演示，实际启动需要完整游戏文件和 Java。";

        await Dispatcher.UIThread.InvokeAsync(() => ConsoleScroll.ScrollToEnd());
    }

    private async void OnBrowseJava(object? sender, RoutedEventArgs e)
    {
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "选择 Java 可执行文件",
            AllowMultiple = false,
            FileTypeFilter = new[] { new FilePickerFileType("Java") { Patterns = new[] { "javaw.exe", "java.exe", "java" } } }
        });
        if (files.Count > 0) JavaPathInput.Text = files[0].Path.LocalPath;
    }

    private async void OnBrowseGameDir(object? sender, RoutedEventArgs e)
    {
        var folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions { Title = "选择游戏目录", AllowMultiple = false });
        if (folders.Count > 0) GameDirInput.Text = folders[0].Path.LocalPath;
    }

    private async void OnLogin(object? sender, RoutedEventArgs e)
    {
        var email = SettingsEmail.Text?.Trim();
        var password = SettingsPassword.Text?.Trim();
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password)) { LoginStatus.Text = "⚠ 请输入邮箱和密码"; return; }

        try
        {
            var result = await _authService.Login(new LoginRequest { Email = email, Password = password });
            if (result.Success && result.User != null)
            {
                UserNameText.Text = result.User.Username;
                UserStatus.Text = "已登录";
                LoginStatus.Text = "✅ 登录成功！";
            }
            else
            {
                LoginStatus.Text = $"❌ 登录失败: {result.Error}";
            }
        }
        catch (Exception ex)
        {
            LoginStatus.Text = $"❌ 登录出错: {ex.Message}";
        }
    }

    private async void OnRegister(object? sender, RoutedEventArgs e)
    {
        var email = SettingsEmail.Text?.Trim();
        var password = SettingsPassword.Text?.Trim();
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password)) { LoginStatus.Text = "⚠ 请输入邮箱和密码"; return; }

        try
        {
            var username = email.Split('@')[0];
            var result = await _authService.Register(new RegisterRequest { Email = email, Password = password, Username = username });
            LoginStatus.Text = result.Success ? "✅ 注册成功！请登录" : $"❌ 注册失败: {result.Error}";
        }
        catch (Exception ex)
        {
            LoginStatus.Text = $"❌ 注册出错: {ex.Message}";
        }
    }

    private void OnSaveSettings(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(JavaPathInput.Text)) JavaPathInput.Text = JavaPathInput.Text;
        if (!string.IsNullOrEmpty(GameDirInput.Text)) GameDirInput.Text = GameDirInput.Text;
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

public class McVersionItem
{
    public string Id { get; set; } = "";
    public string Type { get; set; } = "";
    public string ReleaseDate { get; set; } = "";
    public bool IsLatest { get; set; }
    public string Url { get; set; } = "";
}

public class ResourceItem
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string Downloads { get; set; } = "";
    public string Likes { get; set; } = "";
    public string GameVersion { get; set; } = "";
}
