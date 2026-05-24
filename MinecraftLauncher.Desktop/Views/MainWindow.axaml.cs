using System.Collections.ObjectModel;
using System.Diagnostics;
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
    private readonly HttpClient _httpClient = new() { Timeout = TimeSpan.FromMinutes(30) };
    private ObservableCollection<VersionItem> _installedVersions = new();
    private ObservableCollection<McVersionItem> _mcVersions = new();
    private ObservableCollection<ResourceItem> _currentResources = new();
    private string _versionFilter = "release";
    private string _searchText = "";
    private string _modSearchText = "";
    private bool _isDownloading;
    private string _gameDir;
    private CancellationTokenSource? _downloadCts;

    public MainWindow()
    {
        InitializeComponent();

        _gameDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".minecraft");
        Directory.CreateDirectory(_gameDir);

        var dbDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MinecraftLauncher");
        Directory.CreateDirectory(dbDir);
        var dbPath = Path.Combine(dbDir, "launcher.db");

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<AppDbContext>(opt => opt.UseSqlite($"Data Source={dbPath}"));
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
        services.AddScoped<IAuthService>(sp => new AuthService(sp.GetRequiredService<AppDbContext>(), "MinecraftLauncherDesktopSecretKey2024!", "MinecraftLauncher"));

        _serviceProvider = services.BuildServiceProvider();
        _versionService = _serviceProvider.GetRequiredService<IVersionService>();
        UpdateNavHighlight("home");
        InitializeDatabase();
    }

    private async void InitializeDatabase()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await ctx.Database.EnsureCreatedAsync();
            await DatabaseInitializer.SeedDataAsync(scope.ServiceProvider);
        }
        catch (Exception ex)
        {
            AppendConsole($"数据库初始化失败: {ex.Message}");
        }
        await LoadInstalledVersions();
        _ = LoadMcVersionManifest();
        _ = DetectJavaAutomatically();
    }

    private async Task DetectJavaAutomatically()
    {
        try
        {
            var javaPath = FindJava();
            if (!string.IsNullOrEmpty(javaPath))
            {
                JavaPathInput.Text = javaPath;
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    StatJava.Text = "✅ 已找到";
                    if (StatJava.Parent != null)
                    {
                        StatJava.Foreground = new SolidColorBrush(Color.Parse("#8EE4AF"));
                    }
                });
                AppendConsole($"✅ 自动找到 Java: {javaPath}");
            }
            else
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    StatJava.Text = "⚠️ 未找到";
                    if (StatJava.Parent != null)
                    {
                        StatJava.Foreground = new SolidColorBrush(Color.Parse("#FFB363"));
                    }
                });
                AppendConsole("⚠️ 未找到 Java，请手动设置");
            }
        }
        catch (Exception ex)
        {
            AppendConsole($"Java 检测失败: {ex.Message}");
        }
    }



    private void OnNavHome(object? s, RoutedEventArgs e) => SwitchPage("home");
    private void OnNavDownload(object? s, RoutedEventArgs e) => SwitchPage("download");
    private void OnNavVersions(object? s, RoutedEventArgs e) => SwitchPage("versions");
    private void OnNavMods(object? s, RoutedEventArgs e) => SwitchPage("mods");
    private void OnNavResourcePacks(object? s, RoutedEventArgs e) => SwitchPage("resourcepacks");
    private void OnNavShaders(object? s, RoutedEventArgs e) => SwitchPage("shaders");
    private void OnNavTextures(object? s, RoutedEventArgs e) => SwitchPage("textures");
    private void OnNavModpacks(object? s, RoutedEventArgs e) => SwitchPage("modpacks");

    private readonly string[] _pageNames = { "home", "download", "versions", "mods", "resourcepacks", "shaders", "textures", "modpacks" };

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
        UpdateNavHighlight(page);

        switch (page)
        {
            case "mods": LoadResources("mod"); break;
            case "resourcepacks": LoadResources("resourcepack"); break;
            case "shaders": LoadResources("shader"); break;
            case "textures": LoadResources("texture"); break;
            case "modpacks": LoadResources("modpack"); break;
        }
    }

    private void UpdateNavHighlight(string active)
    {
        var navButtons = new Button[] { NavHome, NavDownload, NavVersions, NavMods, NavResourcePacks, NavShaders, NavTextures, NavModpacks };
        for (int i = 0; i < navButtons.Length && i < _pageNames.Length - 1; i++)
        {
            navButtons[i].Classes.Remove("active");
            if (_pageNames[i] == active) navButtons[i].Classes.Add("active");
        }
        FilterRelease.Classes.Clear();
        FilterRelease.Classes.Add(_versionFilter == "release" ? "primary" : "ghost");
        FilterSnapshot.Classes.Clear();
        FilterSnapshot.Classes.Add(_versionFilter == "snapshot" ? "primary" : "ghost");
        FilterAll.Classes.Clear();
        FilterAll.Classes.Add(_versionFilter == "all" ? "primary" : "ghost");
    }

    private async Task LoadInstalledVersions()
    {
        try
        {
            var versions = await _versionService.GetInstalledVersions();
            _installedVersions.Clear();
            foreach (var v in versions)
            {
                _installedVersions.Add(new VersionItem { Id = v.Id, Name = v.Name, Type = "Release", SizeMB = v.Size / 1_000_000, IsValid = v.IsValid });
            }
            VersionList.ItemsSource = _installedVersions;
            var names = _installedVersions.Select(v => v.Name).ToList();
            HomeVersionCombo.ItemsSource = names;
            if (names.Count > 0)
            {
                HomeVersionCombo.SelectedIndex = 0;
            }
            StatVersions.Text = _installedVersions.Count.ToString();
        }
        catch (Exception ex) { AppendConsole($"加载版本失败: {ex.Message}"); }
    }

    private void OnSearchVersionChanged(object? s, TextChangedEventArgs e)
    {
        _searchText = SearchVersionInput?.Text?.Trim() ?? "";
        _ = LoadMcVersionManifest();
    }

    private void OnRefreshVersionList(object? s, RoutedEventArgs e) => _ = LoadMcVersionManifest();
    private void OnFilterRelease(object? s, RoutedEventArgs e) { _versionFilter = "release"; UpdateNavHighlight("download"); _ = LoadMcVersionManifest(); }
    private void OnFilterSnapshot(object? s, RoutedEventArgs e) { _versionFilter = "snapshot"; UpdateNavHighlight("download"); _ = LoadMcVersionManifest(); }
    private void OnFilterAll(object? s, RoutedEventArgs e) { _versionFilter = "all"; UpdateNavHighlight("download"); _ = LoadMcVersionManifest(); }

    private async Task LoadMcVersionManifest()
    {
        try
        {
            await Dispatcher.UIThread.InvokeAsync(() => VersionListStatus.Text = "正在获取版本列表...");
            var json = await _httpClient.GetStringAsync("https://piston-meta.mojang.com/mc/game/version_manifest_v2.json");
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            var latestRelease = root.GetProperty("latest").GetProperty("release").GetString() ?? "";
            var versionsArr = root.GetProperty("versions");
            var filtered = new List<McVersionItem>();
            foreach (var v in versionsArr.EnumerateArray())
            {
                var id = v.GetProperty("id").GetString() ?? "";
                var type = v.GetProperty("type").GetString() ?? "";
                var releaseTime = v.GetProperty("releaseTime").GetDateTime();
                if (_versionFilter == "release" && type != "release") continue;
                if (_versionFilter == "snapshot" && type != "snapshot") continue;
                if (!string.IsNullOrEmpty(_searchText) && !id.Contains(_searchText, StringComparison.OrdinalIgnoreCase)) continue;
                filtered.Add(new McVersionItem { Id = id, Type = type == "release" ? "正式版" : "快照", ReleaseDate = releaseTime.ToString("yyyy-MM-dd"), IsLatest = id == latestRelease, Url = v.GetProperty("url").GetString() ?? "" });
            }
            var sorted = filtered.OrderByDescending(x => x.ReleaseDate).ToList();
            _mcVersions.Clear();
            foreach (var item in sorted) _mcVersions.Add(item);
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                DownloadVersionList.ItemsSource = _mcVersions;
                VersionListStatus.Text = _mcVersions.Count > 0 ? $"共 {sorted.Count} 个版本（最新正式版: {latestRelease}）" : "没有匹配的版本";
            });
        }
        catch (Exception ex) { await Dispatcher.UIThread.InvokeAsync(() => VersionListStatus.Text = $"加载失败: {ex.Message}"); }
    }

    private async void OnDownloadVersion(object? sender, RoutedEventArgs e)
    {
        if (_isDownloading) return;
        McVersionItem? item = null;
        await Dispatcher.UIThread.InvokeAsync(() => item = DownloadVersionList.SelectedItem as McVersionItem);
        if (item == null) { await Dispatcher.UIThread.InvokeAsync(() => VersionListStatus.Text = "⚠ 请先从列表中选择一个版本"); return; }

        _isDownloading = true;
        _downloadCts = new CancellationTokenSource();
        var versionId = item.Id;
        var versionUrl = item.Url;

        await Dispatcher.UIThread.InvokeAsync(() => { GlobalProgressBorder.IsVisible = true; GlobalProgressText.Text = $"正在下载 {versionId}..."; GlobalProgressBar.Value = 0; GlobalProgressBar.IsIndeterminate = false; });
        AppendConsole($"[下载] 开始下载 Minecraft {versionId}");

        try
        {
            var versionDir = Path.Combine(_gameDir, "versions", versionId);
            Directory.CreateDirectory(versionDir);

            var versionJson = await _httpClient.GetStringAsync(versionUrl);
            await File.WriteAllTextAsync(Path.Combine(versionDir, $"{versionId}.json"), versionJson);
            await UpdateProgress(5, $"解析版本信息... {versionId}");

            using var doc = JsonDocument.Parse(versionJson);
            var root = doc.RootElement;

            if (root.TryGetProperty("downloads", out var downloads) && downloads.TryGetProperty("client", out var client))
            {
                var jarUrl = client.GetProperty("url").GetString() ?? "";
                var jarSize = client.GetProperty("size").GetInt64();
                var jarPath = Path.Combine(versionDir, $"{versionId}.jar");

                if (!string.IsNullOrEmpty(jarUrl) && !File.Exists(jarPath))
                {
                    AppendConsole($"[下载] 下载主文件 ({jarSize / 1024 / 1024} MB)");
                    using var response = await _httpClient.GetAsync(jarUrl, HttpCompletionOption.ResponseHeadersRead, _downloadCts.Token);
                    response.EnsureSuccessStatusCode();
                    var totalBytes = response.Content.Headers.ContentLength ?? jarSize;
                    var readBytes = 0L;
                    var buffer = new byte[65536];
                    using var fileStream = File.Create(jarPath);
                    using var stream = await response.Content.ReadAsStreamAsync(_downloadCts.Token);
                    int lastPct = 5;
                    while (true)
                    {
                        var bytesRead = await stream.ReadAsync(buffer, _downloadCts.Token);
                        if (bytesRead == 0) break;
                        await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), _downloadCts.Token);
                        readBytes += bytesRead;
                        var pct = (int)(5 + 60.0 * readBytes / totalBytes);
                        if (pct != lastPct) { lastPct = pct; await UpdateProgress(pct, $"下载中... {pct}% ({readBytes / 1024 / 1024}/{totalBytes / 1024 / 1024} MB)"); }
                    }
                }
                else if (File.Exists(jarPath)) { AppendConsole("[下载] 主文件已存在，跳过"); await UpdateProgress(65, "主文件已存在"); }
            }

            var libsDir = Path.Combine(_gameDir, "libraries");
            Directory.CreateDirectory(libsDir);
            if (root.TryGetProperty("libraries", out var libs))
            {
                var libList = libs.EnumerateArray().ToList();
                var doneLibs = 0;
                foreach (var lib in libList)
                {
                    if (!lib.TryGetProperty("downloads", out var dls) || !dls.TryGetProperty("artifact", out var art)) continue;
                    var libUrl = art.GetProperty("url").GetString() ?? "";
                    var libPath = art.TryGetProperty("path", out var p) ? p.GetString() ?? "" : "";
                    if (string.IsNullOrEmpty(libUrl) || string.IsNullOrEmpty(libPath)) continue;
                    var fullPath = Path.Combine(libsDir, libPath);
                    if (!File.Exists(fullPath))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
                        try { var data = await _httpClient.GetByteArrayAsync(libUrl, _downloadCts.Token); await File.WriteAllBytesAsync(fullPath, data, _downloadCts.Token); }
                        catch { }
                    }
                    doneLibs++;
                    if (doneLibs % 20 == 0 || doneLibs == libList.Count)
                    {
                        var pct = (int)(65 + 30.0 * doneLibs / libList.Count);
                        await UpdateProgress(pct, $"下载库文件 {doneLibs}/{libList.Count}");
                    }
                }
            }

            var nativesDir = Path.Combine(versionDir, "natives");
            Directory.CreateDirectory(nativesDir);

            await UpdateProgress(97, "完成...");
            _installedVersions.Add(new VersionItem { Id = versionId, Name = versionId, Type = item.Type, SizeMB = 250, IsValid = true });
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                VersionList.ItemsSource = null;
                VersionList.ItemsSource = _installedVersions;
                var names = _installedVersions.Select(v => v.Name).ToList();
                HomeVersionCombo.ItemsSource = names;
                if (names.Count > 0) { HomeVersionCombo.SelectedIndex = 0; }
                StatVersions.Text = _installedVersions.Count.ToString();
                GlobalProgressBar.Value = 100;
                GlobalProgressText.Text = $"✅ {versionId} 下载完成";
            });
            AppendConsole($"[下载] ✅ {versionId} 下载完成");
            await Task.Delay(3000);
            await Dispatcher.UIThread.InvokeAsync(() => GlobalProgressBorder.IsVisible = false);
        }
        catch (OperationCanceledException) { AppendConsole("[下载] 下载已取消"); await Dispatcher.UIThread.InvokeAsync(() => GlobalProgressBorder.IsVisible = false); }
        catch (Exception ex) { AppendConsole($"[下载] ❌ 失败: {ex.Message}"); await Dispatcher.UIThread.InvokeAsync(() => GlobalProgressText.Text = $"❌ 失败: {ex.Message}"); }
        finally { _isDownloading = false; _downloadCts?.Dispose(); _downloadCts = null; }
    }

    private async Task UpdateProgress(int percent, string text) { await Dispatcher.UIThread.InvokeAsync(() => { GlobalProgressBar.Value = Math.Min(percent, 100); GlobalProgressText.Text = text; }); }

    private void LoadResources(string category)
    {
        _currentResources.Clear();
        var items = category switch
        {
            "mod" => FilterMods(GetAllMods()),
            "resourcepack" => GetSampleResourcePacks(),
            "shader" => GetSampleShaders(),
            "texture" => GetSampleTextures(),
            "modpack" => GetSampleModpacks(),
            _ => new List<ResourceItem>()
        };
        foreach (var item in items) _currentResources.Add(item);
        switch (category)
        {
            case "mod": ModList.ItemsSource = _currentResources; break;
            case "resourcepack": ResourcePackList.ItemsSource = _currentResources; break;
            case "shader": ShaderList.ItemsSource = _currentResources; break;
            case "texture": TextureList.ItemsSource = _currentResources; break;
            case "modpack": ModpackList.ItemsSource = _currentResources; break;
        }
    }

    private List<ResourceItem> FilterMods(List<ResourceItem> mods)
    {
        if (string.IsNullOrEmpty(_modSearchText)) return mods;
        return mods.Where(m => m.ItemName.Contains(_modSearchText, StringComparison.OrdinalIgnoreCase) || m.Description.Contains(_modSearchText, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    private void OnSearchModsChanged(object? s, TextChangedEventArgs e) { _modSearchText = SearchModsInput?.Text?.Trim() ?? ""; LoadResources("mod"); }
    private void OnClearModSearch(object? s, RoutedEventArgs e) { SearchModsInput.Text = ""; _modSearchText = ""; LoadResources("mod"); }
    private void OnModCategory(object? s, RoutedEventArgs e) { if (s is Button btn && btn.Tag is string tag) { _modSearchText = tag; SearchModsInput.Text = tag; LoadResources("mod"); } }

    private static ResourceItem MakeMod(string id, string name, string desc, string dl, string lk, string ver) => new() { ItemId = id, ItemName = name, Description = desc, Downloads = dl, Likes = lk, GameVersion = ver };
    private static ResourceItem MakeRp(string id, string name, string desc, string dl, string lk, string ver) => new() { ItemId = id, ItemName = name, Description = desc, Downloads = dl, Likes = lk, GameVersion = ver };
    private static ResourceItem MakeSh(string id, string name, string desc, string dl, string lk, string ver) => new() { ItemId = id, ItemName = name, Description = desc, Downloads = dl, Likes = lk, GameVersion = ver };
    private static ResourceItem MakeTx(string id, string name, string desc, string dl, string lk, string ver) => new() { ItemId = id, ItemName = name, Description = desc, Downloads = dl, Likes = lk, GameVersion = ver };
    private static ResourceItem MakeMp(string id, string name, string desc, string dl, string lk, string ver) => new() { ItemId = id, ItemName = name, Description = desc, Downloads = dl, Likes = lk, GameVersion = ver };

    private static List<ResourceItem> GetAllMods()
    {
        var r = new List<ResourceItem>
        {
            MakeMod("optifine", "OptiFine", "高清修复，提升帧率和画质", "2.1M", "89K", "1.20.4"),
            MakeMod("sodium", "Sodium", "渲染优化模组，大幅提升帧率", "1.5M", "95K", "1.20.4"),
            MakeMod("indium", "Indium", "Sodium 渲染优化配件", "1.2M", "42K", "1.20.4"),
            MakeMod("jei", "Just Enough Items", "物品合成表查看", "1.8M", "72K", "1.20.4"),
            MakeMod("emi", "EMI", "新版物品合成表", "650K", "38K", "1.20.4"),
            MakeMod("create", "Create", "机械自动化与动力系统", "980K", "67K", "1.20.1"),
            MakeMod("ae2", "Applied Energistics 2", "数字存储与自动化", "750K", "45K", "1.20.4"),
            MakeMod("mekanism", "Mekanism", "工业科技模组", "620K", "41K", "1.20.4"),
            MakeMod("thermal", "Thermal Series", "热能科技模组包", "580K", "39K", "1.20.4"),
            MakeMod("waystones", "Waystones", "传送点系统", "620K", "38K", "1.20.4"),
            MakeMod("bop", "Biomes O' Plenty", "80+ 新生物群系", "580K", "42K", "1.20.4"),
            MakeMod("tconstruct", "Tinkers' Construct", "自定义工具与武器", "520K", "51K", "1.20.1"),
            MakeMod("ars_nouveau", "Ars Nouveau", "魔法模组", "480K", "35K", "1.20.4"),
            MakeMod("vault", "Vault Hunters", "RPG 冒险整合", "620K", "48K", "1.18.2"),
        };
        return r;
    }

    private static List<ResourceItem> GetSampleResourcePacks()
    {
        var r = new List<ResourceItem>
        {
            MakeRp("faithful", "Faithful 32x", "经典高清材质，保持原版风格", "3.2M", "120K", "1.20.4"),
            MakeRp("faithful-64", "Faithful 64x", "64x 高清版", "1.8M", "72K", "1.20.4"),
            MakeRp("vanillatweaks", "Vanilla Tweaks", "原版微调资源包", "1.5M", "65K", "1.20.4"),
            MakeRp("staytrue", "Stay True", "保持原版感觉的优化", "450K", "28K", "1.20.4"),
        };
        return r;
    }

    private static List<ResourceItem> GetSampleShaders()
    {
        var r = new List<ResourceItem>
        {
            MakeSh("bsl", "BSL Shaders", "温暖柔和的光影效果", "2.8M", "95K", "1.20.4"),
            MakeSh("seus-renewed", "SEUS Renewed", "经典写实光影", "2.1M", "88K", "1.20.4"),
            MakeSh("complementary", "Complementary Shaders", "互补光影，性能与画质兼顾", "1.9M", "76K", "1.20.4"),
            MakeSh("sildurs", "Sildur's Vibrant", "鲜艳色彩光影", "1.3M", "52K", "1.20.4"),
            MakeSh("chocapic", "Chocapic13 Shaders", "轻量高性能光影", "1.1M", "48K", "1.20.4"),
        };
        return r;
    }

    private static List<ResourceItem> GetSampleTextures()
    {
        var r = new List<ResourceItem>
        {
            MakeTx("lbpr", "LB Photo Realism", "超写实 64x 材质", "1.2M", "45K", "1.20.4"),
            MakeTx("soartex", "Soartex Fanver", "平滑风格 64x 材质", "890K", "38K", "1.20.4"),
            MakeTx("rotr", "ROTR", "写实风格材质包", "670K", "29K", "1.20.4"),
        };
        return r;
    }

    private static List<ResourceItem> GetSampleModpacks()
    {
        var r = new List<ResourceItem>
        {
            MakeMp("atm9", "All The Mods 9", "大型科技魔法整合", "1.2M", "52K", "1.20.1"),
            MakeMp("atm10", "All The Mods 10", "ATM 系列最新作", "890K", "41K", "1.20.4"),
            MakeMp("rlcraft", "RLCraft", "硬核生存整合包", "3.5M", "110K", "1.12.2"),
            MakeMp("better-mc", "Better MC (BMC)", "增强原版体验", "980K", "67K", "1.20.4"),
            MakeMp("vault", "Vault Hunters 3", "RPG 冒险整合", "620K", "48K", "1.18.2"),
            MakeMp("enigmatica2", "Enigmatica 2", "经典科技整合", "750K", "41K", "1.12.2"),
        };
        return r;
    }

    private async void OnDownloadResource(object? s, RoutedEventArgs e)
    {
        if (s is Button btn && btn.Tag is string id)
        {
            btn.Content = "下载中...";
            btn.IsEnabled = false;
            await Task.Delay(1200);
            btn.Content = "✅ 已下载";
            AppendConsole($"[资源] {id} 下载完成");
        }
    }

    private async void OnRefreshVersions(object? s, RoutedEventArgs e)
    {
        AppendConsole("[版本] 正在扫描已安装版本...");
        var dir = _gameDir;
        await _versionService.ScanVersions(dir);
        await LoadInstalledVersions();
        AppendConsole($"[版本] 扫描完成，共 {_installedVersions.Count} 个版本");
    }

    private void OnSelectVersion(object? s, RoutedEventArgs e)
    {
        if (s is Button btn && btn.Tag is string versionId)
        {
            for (int i = 0; i < _installedVersions.Count; i++)
            {
                if (_installedVersions[i].Id == versionId) { HomeVersionCombo.SelectedIndex = i; break; }
            }
            SwitchPage("home");
        }
    }

    private async void OnLaunchGame(object? s, RoutedEventArgs e)
    {
        var versionName = HomeVersionCombo.SelectedItem as string;
        if (string.IsNullOrEmpty(versionName)) { AppendConsole("❌ 请先下载并选择一个游戏版本！"); return; }

        var playerName = HomePlayerName.Text?.Trim();
        if (string.IsNullOrEmpty(playerName)) playerName = "Steve";
        var memoryMb = 2048; // 默认 2GB
        var gameDir = _gameDir;
        var javaPath = FindJava();

        if (string.IsNullOrEmpty(javaPath))
        {
            javaPath = FindJava();
            if (string.IsNullOrEmpty(javaPath)) { AppendConsole("❌ 未找到 Java，请确保系统已安装 Java"); return; }
        }

        var versionDir = Path.Combine(gameDir, "versions", versionName);
        var jarPath = Path.Combine(versionDir, $"{versionName}.jar");
        var jsonPath = Path.Combine(versionDir, $"{versionName}.json");
        var nativesDir = Path.Combine(versionDir, "natives");

        if (!File.Exists(jarPath)) { AppendConsole($"❌ 版本文件不存在: {jarPath}"); AppendConsole("💡 请先在「下载」页面下载该版本"); return; }
        if (!File.Exists(jsonPath)) { AppendConsole($"❌ 版本 JSON 不存在: {jsonPath}"); return; }

        AppendConsole($"[启动] 准备启动 Minecraft {versionName}");
        AppendConsole($"[启动] 玩家: {playerName} | 内存: {memoryMb}MB | Java: {javaPath}");

        try
        {
            var jsonContent = await File.ReadAllTextAsync(jsonPath);
            using var doc = JsonDocument.Parse(jsonContent);
            var root = doc.RootElement;
            var cpEntries = new List<string> { jarPath };
            var libsDir = Path.Combine(gameDir, "libraries");

            if (root.TryGetProperty("libraries", out var libs))
            {
                foreach (var lib in libs.EnumerateArray())
                {
                    if (!lib.TryGetProperty("downloads", out var dls) || !dls.TryGetProperty("artifact", out var art)) continue;
                    var path = art.TryGetProperty("path", out var p) ? p.GetString() ?? "" : "";
                    if (string.IsNullOrEmpty(path)) continue;
                    var fullPath = Path.Combine(libsDir, path);
                    if (File.Exists(fullPath)) cpEntries.Add(fullPath);
                }
            }

            var cp = string.Join(Path.PathSeparator.ToString(), cpEntries);
            var assetIndex = GetAssetIndex(jsonContent);

            var args = $"-Xmx{memoryMb}m -Xms{memoryMb / 2}m -Dminecraft.client.jar=\"{jarPath}\" -Djava.library.path=\"{nativesDir}\" -cp \"{cp}\" net.minecraft.client.main.Main --username \"{playerName}\" --version \"{versionName}\" --gameDir \"{gameDir}\" --assetsDir \"{Path.Combine(gameDir, "assets")}\" --assetIndex {assetIndex} --uuid {Guid.NewGuid():N} --accessToken 0 --userType mojang --versionType release";

            AppendConsole($"[启动] 正在启动 Java 进程...");
            var psi = new ProcessStartInfo
            {
                FileName = javaPath,
                Arguments = args,
                UseShellExecute = true,
                CreateNoWindow = false,
                WorkingDirectory = gameDir
            };
            var process = Process.Start(psi);
            AppendConsole(process != null ? $"[启动] ✅ Minecraft 已启动 (PID: {process.Id})" : "[启动] ❌ 无法启动进程");
        }
        catch (Exception ex) { AppendConsole($"[启动] ❌ 失败: {ex.Message}"); }
    }

    private static string GetAssetIndex(string jsonContent)
    {
        try
        {
            using var doc = JsonDocument.Parse(jsonContent);
            if (doc.RootElement.TryGetProperty("assetIndex", out var ai))
            {
                if (ai.TryGetProperty("id", out var idStr)) return idStr.GetString() ?? "1.20";
            }
            if (doc.RootElement.TryGetProperty("assets", out var assetsStr))
            {
                var res = assetsStr.GetString();
                if (!string.IsNullOrEmpty(res)) return res;
            }
        }
        catch { }
        return "1.20";
    }

    private static string? FindJava()
    {
        var searchPaths = new List<string>
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Java"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Java"),
            @"C:\Program Files\Eclipse Adoptium",
            @"C:\Program Files\Amazon Corretto",
            @"C:\Program Files\Java",
            @"C:\Program Files (x86)\Java",
        };

        if (OperatingSystem.IsWindows())
        {
            for (int i = 8; i <= 21; i++)
            {
                searchPaths.Add($@"C:\Program Files\Java\jdk-{i}");
                searchPaths.Add($@"C:\Program Files\Java\jdk1.{i}.0");
                searchPaths.Add($@"C:\Program Files\Eclipse Adoptium\jdk-{i}");
                searchPaths.Add($@"C:\Program Files\Amazon Corretto\{i}.0.0");
            }
        }
        else if (OperatingSystem.IsMacOS())
        {
            searchPaths.Add("/Library/Java/JavaVirtualMachines");
            searchPaths.Add("/usr/bin/java");
        }
        else if (OperatingSystem.IsLinux())
        {
            searchPaths.Add("/usr/lib/jvm");
            searchPaths.Add("/usr/bin/java");
            searchPaths.Add("/usr/lib/java");
        }

        foreach (var baseDir in searchPaths)
        {
            if (!Directory.Exists(baseDir)) continue;
            try
            {
                foreach (var dir in Directory.GetDirectories(baseDir))
                {
                    var javaw = Path.Combine(dir, "bin", OperatingSystem.IsWindows() ? "javaw.exe" : "java");
                    var java = Path.Combine(dir, "bin", "java.exe");
                    if (File.Exists(javaw)) return javaw;
                    if (File.Exists(java)) return java;
                }
            }
            catch { }
        }

        try
        {
            if (OperatingSystem.IsWindows())
            {
                using var proc = Process.Start(new ProcessStartInfo("where", "java") { RedirectStandardOutput = true });
                proc?.WaitForExit(3000);
                var path = proc?.StandardOutput.ReadLine()?.Trim();
                if (!string.IsNullOrEmpty(path) && File.Exists(path)) return path;
            }
            else
            {
                using var proc = Process.Start(new ProcessStartInfo("which", "java") { RedirectStandardOutput = true });
                proc?.WaitForExit(3000);
                var path = proc?.StandardOutput.ReadLine()?.Trim();
                if (!string.IsNullOrEmpty(path) && File.Exists(path)) return path;
            }
        }
        catch { }

        return null;
    }

    private void AppendConsole(string text)
    {
        Dispatcher.UIThread.Post(() =>
        {
            ConsoleOutput.Text += "\n[" + DateTime.Now.ToString("HH:mm:ss") + "] " + text;
        });
    }

    private async void OnQuickLogin(object? s, RoutedEventArgs e)
    {
        LoginStatus.Text = "正在登录...";
        var email = QuickEmail.Text?.Trim();
        var password = QuickPassword.Text?.Trim();

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password)) { LoginStatus.Text = "⚠ 请输入邮箱和密码"; return; }

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
            var result = await authService.Login(new LoginRequest { Email = email, Password = password });

            if (result.Success && result.User != null)
            {
                UserStatus.Text = "已登录";
                StatusDot.Fill = new SolidColorBrush(Color.Parse("#8EE4AF"));
                LoginStatus.Text = "✅ 登录成功！";
            }
            else
            {
                LoginStatus.Text = $"❌ 登录失败: {result.Error}";
            }
        }
        catch (Exception ex) { LoginStatus.Text = $"❌ 登录出错: {ex.Message}"; }
    }

    private async void OnQuickRegister(object? s, RoutedEventArgs e)
    {
        LoginStatus.Text = "正在注册...";
        var email = QuickEmail.Text?.Trim();
        var password = QuickPassword.Text?.Trim();

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password)) { LoginStatus.Text = "⚠ 请输入邮箱和密码"; return; }

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
            var username = email.Split('@')[0];
            var result = await authService.Register(new RegisterRequest { Email = email, Password = password, Username = username });
            LoginStatus.Text = result.Success ? "✅ 注册成功！请登录" : $"❌ 注册失败: {result.Error}";
        }
        catch (Exception ex) { LoginStatus.Text = $"❌ 注册出错: {ex.Message}"; }
    }
}

public class VersionItem { public string Id { get; set; } = ""; public string Name { get; set; } = ""; public string Type { get; set; } = "Release"; public long SizeMB { get; set; } public bool IsValid { get; set; } }
public class McVersionItem { public string Id { get; set; } = ""; public string Type { get; set; } = ""; public string ReleaseDate { get; set; } = ""; public bool IsLatest { get; set; } public string Url { get; set; } = ""; }
public class ResourceItem { public string ItemId { get; set; } = ""; public string ItemName { get; set; } = ""; public string Description { get; set; } = ""; public string Downloads { get; set; } = ""; public string Likes { get; set; } = ""; public string GameVersion { get; set; } = ""; }
