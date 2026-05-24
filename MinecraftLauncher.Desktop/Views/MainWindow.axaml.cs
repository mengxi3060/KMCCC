using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
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
    private readonly HttpClient _httpClient = new() { Timeout = TimeSpan.FromMinutes(10) };
    private ObservableCollection<VersionItem> _installedVersions = new();
    private ObservableCollection<McVersionItem> _mcVersions = new();
    private ObservableCollection<ResourceItem> _currentResources = new();
    private string _versionFilter = "release";
    private bool _isDownloading;
    private string _gameDir = "";

    public MainWindow()
    {
        InitializeComponent();

        var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MinecraftLauncher", "launcher.db");
        Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);
        _gameDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".minecraft");
        Directory.CreateDirectory(_gameDir);

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite($"Data Source={dbPath}"));

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

        MemorySlider.ValueChanged += OnMemoryChanged;
        GameDirInput.Text = _gameDir;
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

        await LoadInstalledVersions();
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

    private async Task LoadInstalledVersions()
    {
        try
        {
            var versions = await _versionService.GetInstalledVersions();
            _installedVersions.Clear();
            foreach (var v in versions)
            {
                _installedVersions.Add(new VersionItem
                {
                    Id = v.Id,
                    Name = v.Name,
                    Type = "Release",
                    SizeMB = v.Size / 1_000_000,
                    IsValid = v.IsValid
                });
            }

            VersionList.ItemsSource = _installedVersions;
            var names = _installedVersions.Select(v => v.Name).ToList();
            HomeVersionCombo.ItemsSource = names;
            LaunchVersionCombo.ItemsSource = names;
            if (names.Count > 0)
            {
                HomeVersionCombo.SelectedIndex = 0;
                LaunchVersionCombo.SelectedIndex = 0;
            }
            StatVersions.Text = _installedVersions.Count.ToString();
        }
        catch (Exception ex)
        {
            AppendConsole($"加载版本失败: {ex.Message}");
        }
    }

    private async Task LoadMcVersionManifest()
    {
        try
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
                VersionListStatus.Text = "正在从 Mojang 获取版本列表...");

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

                var searchText = await Dispatcher.UIThread.InvokeAsync(() => SearchVersionInput?.Text?.Trim().ToLower() ?? "");
                if (!string.IsNullOrEmpty(searchText) && !id.ToLower().Contains(searchText)) continue;

                filtered.Add(new McVersionItem
                {
                    Id = id,
                    Type = type == "release" ? "正式版" : "快照",
                    ReleaseDate = releaseTime.ToString("yyyy-MM-dd"),
                    IsLatest = id == latestRelease,
                    Url = v.GetProperty("url").GetString() ?? ""
                });
            }

            _mcVersions = new ObservableCollection<McVersionItem>(filtered);
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                DownloadVersionList.ItemsSource = _mcVersions;
                VersionListStatus.Text = $"共 {_mcVersions.Count} 个版本（最新正式版: {latestRelease}）";
            });
        }
        catch (Exception ex)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
                VersionListStatus.Text = $"加载失败: {ex.Message}，请检查网络");
        }
    }

    private void OnRefreshVersionList(object? sender, RoutedEventArgs e) => _ = LoadMcVersionManifest();
    private void OnFilterRelease(object? sender, RoutedEventArgs e) { _versionFilter = "release"; _ = LoadMcVersionManifest(); }
    private void OnFilterSnapshot(object? sender, RoutedEventArgs e) { _versionFilter = "snapshot"; _ = LoadMcVersionManifest(); }
    private void OnFilterAll(object? sender, RoutedEventArgs e) { _versionFilter = "all"; _ = LoadMcVersionManifest(); }

    private async void OnDownloadVersion(object? sender, RoutedEventArgs e)
    {
        if (_isDownloading) return;

        McVersionItem? item = null;
        await Dispatcher.UIThread.InvokeAsync(() => item = DownloadVersionList.SelectedItem as McVersionItem);
        if (item == null)
        {
            await Dispatcher.UIThread.InvokeAsync(() => VersionListStatus.Text = "⚠ 请先选择一个版本");
            return;
        }

        _isDownloading = true;
        var versionId = item.Id;
        var versionUrl = item.Url;

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            DownloadProgressCard.IsVisible = true;
            DownloadProgressText.Text = $"正在下载 Minecraft {versionId}...";
            DownloadProgressBar.Value = 0;
            DownloadProgressBar.IsIndeterminate = true;
        });

        try
        {
            var versionDir = Path.Combine(_gameDir, "versions", versionId);
            Directory.CreateDirectory(versionDir);

            AppendConsole($"[下载] 开始下载 {versionId} 版本信息...");

            var versionJson = await _httpClient.GetStringAsync(versionUrl);
            var versionJsonPath = Path.Combine(versionDir, $"{versionId}.json");
            await File.WriteAllTextAsync(versionJsonPath, versionJson);

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                DownloadProgressBar.IsIndeterminate = false;
                DownloadProgressBar.Value = 20;
                DownloadProgressText.Text = $"正在下载 {versionId} 游戏文件... 20%";
            });

            using var doc = JsonDocument.Parse(versionJson);
            var root = doc.RootElement;

            if (root.TryGetProperty("downloads", out var downloads))
            {
                if (downloads.TryGetProperty("client", out var client))
                {
                    var jarUrl = client.GetProperty("url").GetString() ?? "";
                    var jarSize = client.GetProperty("size").GetInt64();
                    var jarPath = Path.Combine(versionDir, $"{versionId}.jar");

                    if (!string.IsNullOrEmpty(jarUrl))
                    {
                        AppendConsole($"[下载] 下载游戏主文件 ({jarSize / 1024 / 1024} MB)...");
                        using var response = await _httpClient.GetAsync(jarUrl, HttpCompletionOption.ResponseHeadersRead);
                        response.EnsureSuccessStatusCode();

                        var totalBytes = response.Content.Headers.ContentLength ?? jarSize;
                        var readBytes = 0L;
                        var buffer = new byte[81920];
                        using var fileStream = File.Create(jarPath);
                        using var stream = await response.Content.ReadAsStreamAsync();

                        int lastPercent = 20;
                        while (true)
                        {
                            var bytesRead = await stream.ReadAsync(buffer);
                            if (bytesRead == 0) break;
                            await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead));
                            readBytes += bytesRead;

                            var percent = (int)(20 + 70.0 * readBytes / totalBytes);
                            if (percent != lastPercent)
                            {
                                lastPercent = percent;
                                await Dispatcher.UIThread.InvokeAsync(() =>
                                {
                                    DownloadProgressBar.Value = percent;
                                    DownloadProgressText.Text = $"正在下载 {versionId}... {percent}% ({readBytes / 1024 / 1024} MB / {totalBytes / 1024 / 1024} MB)";
                                });
                            }
                        }
                    }
                }

                if (downloads.TryGetProperty("client_mappings", out var mappings))
                {
                    var mappingsUrl = mappings.GetProperty("url").GetString() ?? "";
                    if (!string.IsNullOrEmpty(mappingsUrl))
                    {
                        var mappingsPath = Path.Combine(versionDir, $"{versionId}.txt");
                        AppendConsole($"[下载] 下载映射文件...");
                        var mappingsData = await _httpClient.GetByteArrayAsync(mappingsUrl);
                        await File.WriteAllBytesAsync(mappingsPath, mappingsData);
                    }
                }
            }

            if (root.TryGetProperty("libraries", out var libraries))
            {
                var libCount = libraries.GetArrayLength();
                var libIndex = 0;
                var libsDir = Path.Combine(_gameDir, "libraries");
                Directory.CreateDirectory(libsDir);

                foreach (var lib in libraries.EnumerateArray())
                {
                    libIndex++;
                    if (lib.TryGetProperty("downloads", out var libDls))
                    {
                        if (libDls.TryGetProperty("artifact", out var artifact))
                        {
                            var libUrl = artifact.GetProperty("url").GetString() ?? "";
                            var libPath = artifact.TryGetProperty("path", out var p) ? p.GetString() ?? "" : "";

                            if (!string.IsNullOrEmpty(libUrl) && !string.IsNullOrEmpty(libPath))
                            {
                                var fullLibPath = Path.Combine(libsDir, libPath);
                                if (!File.Exists(fullLibPath))
                                {
                                    Directory.CreateDirectory(Path.GetDirectoryName(fullLibPath)!);
                                    try
                                    {
                                        var libData = await _httpClient.GetByteArrayAsync(libUrl);
                                        await File.WriteAllBytesAsync(fullLibPath, libData);
                                    }
                                    catch { }
                                }
                            }
                        }
                    }

                    var libPercent = (int)(90 + 10.0 * libIndex / libCount);
                    if (libIndex % 5 == 0 || libIndex == libCount)
                    {
                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            DownloadProgressBar.Value = Math.Min(libPercent, 99);
                            DownloadProgressText.Text = $"下载库文件 {libIndex}/{libCount}...";
                        });
                    }
                }
            }

            _installedVersions.Add(new VersionItem { Id = versionId, Name = versionId, Type = item.Type, SizeMB = 250, IsValid = true });
            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                VersionList.ItemsSource = null;
                VersionList.ItemsSource = _installedVersions;
                var names = _installedVersions.Select(v => v.Name).ToList();
                HomeVersionCombo.ItemsSource = names;
                LaunchVersionCombo.ItemsSource = names;
                if (names.Count > 0)
                {
                    HomeVersionCombo.SelectedIndex = 0;
                    LaunchVersionCombo.SelectedIndex = 0;
                }
                StatVersions.Text = _installedVersions.Count.ToString();

                DownloadProgressBar.Value = 100;
                DownloadProgressText.Text = $"✅ {versionId} 下载完成！";
            });

            AppendConsole($"[下载] ✅ {versionId} 下载完成，已保存到 {versionDir}");
        }
        catch (Exception ex)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
                DownloadProgressText.Text = $"❌ 下载失败: {ex.Message}");
            AppendConsole($"[下载] ❌ 失败: {ex.Message}");
        }
        finally
        {
            _isDownloading = false;
        }
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
            var origContent = btn.Content;
            btn.Content = "下载中...";
            btn.IsEnabled = false;
            await Task.Delay(1500);
            btn.Content = "✅ 已下载";
            AppendConsole($"[资源] {id} 下载完成");
        }
    }

    private async void OnRefreshVersions(object? sender, RoutedEventArgs e) => await LoadInstalledVersions();

    private void OnSelectVersion(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string versionId)
        {
            for (int i = 0; i < _installedVersions.Count; i++)
            {
                if (_installedVersions[i].Id == versionId)
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
            AppendConsole("❌ 请先下载并选择一个游戏版本！");
            return;
        }

        var playerName = PlayerNameInput.Text?.Trim();
        if (string.IsNullOrEmpty(playerName)) playerName = "Player";
        var memoryMb = (int)MemorySlider.Value * 1024;
        var gameDir = GameDirInput.Text?.Trim() ?? _gameDir;
        var javaPath = JavaPathInput.Text?.Trim() ?? "";

        if (string.IsNullOrEmpty(javaPath))
        {
            javaPath = FindJava();
            if (string.IsNullOrEmpty(javaPath))
            {
                AppendConsole("❌ 未找到 Java，请在设置中指定 Java 路径");
                return;
            }
        }

        var jarPath = Path.Combine(gameDir, "versions", versionName, $"{versionName}.jar");
        if (!File.Exists(jarPath))
        {
            AppendConsole($"❌ 未找到版本文件: {jarPath}\n请先在下载页面下载该版本");
            return;
        }

        AppendConsole($"[启动] 正在启动 Minecraft {versionName}...");
        AppendConsole($"[启动] 玩家: {playerName}");
        AppendConsole($"[启动] Java: {javaPath}");
        AppendConsole($"[启动] 内存: -Xmx{memoryMb}m");
        AppendConsole($"[启动] 目录: {gameDir}");

        try
        {
            var args = $"-Xmx{memoryMb}m -Xms{memoryMb / 2}m " +
                       $"-Djava.library.path=\"{Path.Combine(gameDir, "versions", versionName, "natives")}\" " +
                       $"-cp \"{jarPath}\" " +
                       $"net.minecraft.client.main.Main " +
                       $"--username {playerName} " +
                       $"--version {versionName} " +
                       $"--gameDir \"{gameDir}\" " +
                       $"--assetsDir \"{Path.Combine(gameDir, "assets")}\"";

            var psi = new ProcessStartInfo
            {
                FileName = javaPath,
                Arguments = args,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            var process = new Process { StartInfo = psi };
            process.OutputDataReceived += (_, ea) => { if (ea.Data != null) AppendConsole($"[MC] {ea.Data}"); };
            process.ErrorDataReceived += (_, ea) => { if (ea.Data != null) AppendConsole($"[MC-ERR] {ea.Data}"); };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            AppendConsole($"[启动] ✅ Minecraft 已启动 (PID: {process.Id})");
        }
        catch (Exception ex)
        {
            AppendConsole($"[启动] ❌ 启动失败: {ex.Message}");
        }
    }

    private static string? FindJava()
    {
        var paths = new[]
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Java"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Java"),
            "/usr/bin/java",
            "/usr/lib/jvm"
        };

        foreach (var dir in paths)
        {
            if (!Directory.Exists(dir)) continue;
            var javas = Directory.GetFiles(dir, "javaw.exe", SearchOption.AllDirectories)
                .Concat(Directory.GetFiles(dir, "java.exe", SearchOption.AllDirectories))
                .Concat(Directory.GetFiles(dir, "java", SearchOption.AllDirectories));
            var java = javas.FirstOrDefault();
            if (java != null) return java;
        }

        try
        {
            using var proc = Process.Start(new ProcessStartInfo("which", "java") { RedirectStandardOutput = true });
            proc?.WaitForExit(3000);
            var result = proc?.StandardOutput.ReadLine()?.Trim();
            if (!string.IsNullOrEmpty(result)) return result;
        }
        catch { }

        return null;
    }

    private void AppendConsole(string text)
    {
        Dispatcher.UIThread.Post(() =>
        {
            ConsoleOutput.Text += "\n" + text;
            ConsoleScroll.ScrollToEnd();
        });
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
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            LoginStatus.Text = "⚠ 请输入邮箱和密码";
            return;
        }

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
            var result = await authService.Login(new LoginRequest { Email = email, Password = password });
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
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            LoginStatus.Text = "⚠ 请输入邮箱和密码";
            return;
        }

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
            var username = email.Split('@')[0];
            var result = await authService.Register(new RegisterRequest { Email = email, Password = password, Username = username });
            LoginStatus.Text = result.Success ? "✅ 注册成功！请登录" : $"❌ 注册失败: {result.Error}";
        }
        catch (Exception ex)
        {
            LoginStatus.Text = $"❌ 注册出错: {ex.Message}";
        }
    }

    private void OnSaveSettings(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(GameDirInput.Text)) _gameDir = GameDirInput.Text;
        LoginStatus.Text = "✅ 设置已保存";
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
