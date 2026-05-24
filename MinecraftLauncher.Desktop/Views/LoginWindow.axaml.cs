using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MinecraftLauncher.Core.DTOs.Auth;
using MinecraftLauncher.Core.Domain.Interfaces;
using MinecraftLauncher.Infrastructure.Data;
using MinecraftLauncher.Infrastructure.Services;

namespace MinecraftLauncher.Desktop.Views;

public partial class LoginWindow : Window
{
    private readonly IServiceProvider _serviceProvider;
    private bool _isLoggedIn;

    public string? LoggedInEmail { get; private set; }
    public string? LoggedInUsername { get; private set; }

    public LoginWindow()
    {
        InitializeComponent();

        var dbDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MinecraftLauncher");
        Directory.CreateDirectory(dbDir);
        var dbPath = Path.Combine(dbDir, "launcher.db");

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<AppDbContext>(opt => opt.UseSqlite($"Data Source={dbPath}"));
        services.AddScoped<IAuthService>(sp => new AuthService(sp.GetRequiredService<AppDbContext>(), "MinecraftLauncherDesktopSecretKey2024!", "MinecraftLauncher"));

        _serviceProvider = services.BuildServiceProvider();

        _ = InitializeDbAsync();
    }

    private async System.Threading.Tasks.Task InitializeDbAsync()
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
            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
            {
                StatusText.Text = $"初始化失败: {ex.Message}";
            });
        }
    }

    private async void OnLogin(object? sender, RoutedEventArgs e)
    {
        var email = EmailInput.Text?.Trim();
        var password = PasswordInput.Text?.Trim();

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            StatusText.Text = "⚠ 请输入邮箱和密码";
            return;
        }

        LoginBtn.IsEnabled = false;
        LoginBtn.Content = "登录中...";
        StatusText.Text = "";

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
            var result = await authService.Login(new LoginRequest { Email = email, Password = password });

            if (result.Success && result.User != null)
            {
                LoggedInEmail = email;
                LoggedInUsername = result.User.Username;
                StatusText.Text = "✅ 登录成功！";
                StatusText.Foreground = new SolidColorBrush(Color.Parse("#8EE4AF"));

                await System.Threading.Tasks.Task.Delay(800);
                _isLoggedIn = true;
                Close();
            }
            else
            {
                StatusText.Text = $"❌ {result.Error}";
                LoginBtn.IsEnabled = true;
                LoginBtn.Content = "登录";
            }
        }
        catch (Exception ex)
        {
            StatusText.Text = $"❌ 登录出错: {ex.Message}";
            LoginBtn.IsEnabled = true;
            LoginBtn.Content = "登录";
        }
    }

    private async void OnRegister(object? sender, RoutedEventArgs e)
    {
        var username = RegUsernameInput.Text?.Trim();
        var email = RegEmailInput.Text?.Trim();
        var password = RegPasswordInput.Text?.Trim();
        var confirm = RegConfirmInput.Text?.Trim();

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            RegStatusText.Text = "⚠ 请填写所有字段";
            return;
        }

        if (password != confirm)
        {
            RegStatusText.Text = "⚠ 两次密码输入不一致";
            return;
        }

        if (password.Length < 6)
        {
            RegStatusText.Text = "⚠ 密码至少6个字符";
            return;
        }

        RegisterBtn.IsEnabled = false;
        RegisterBtn.Content = "注册中...";
        RegStatusText.Text = "";

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
            var result = await authService.Register(new RegisterRequest { Email = email, Password = password, Username = username });

            if (result.Success)
            {
                RegStatusText.Text = "✅ 注册成功！请登录";
                RegStatusText.Foreground = new SolidColorBrush(Color.Parse("#8EE4AF"));

                await System.Threading.Tasks.Task.Delay(1000);

                RegUsernameInput.Text = "";
                RegEmailInput.Text = email;
                RegPasswordInput.Text = "";
                RegConfirmInput.Text = "";
                OnSwitchToLogin(sender, e);
            }
            else
            {
                RegStatusText.Text = $"❌ {result.Error}";
                RegisterBtn.IsEnabled = true;
                RegisterBtn.Content = "注册";
            }
        }
        catch (Exception ex)
        {
            RegStatusText.Text = $"❌ 注册出错: {ex.Message}";
            RegisterBtn.IsEnabled = true;
            RegisterBtn.Content = "注册";
        }
    }

    private void OnSwitchToRegister(object? sender, RoutedEventArgs e)
    {
        LoginPanel.IsVisible = false;
        RegisterPanel.IsVisible = true;
        RegStatusText.Text = "";
        StatusText.Text = "";
    }

    private void OnSwitchToLogin(object? sender, RoutedEventArgs e)
    {
        RegisterPanel.IsVisible = false;
        LoginPanel.IsVisible = true;
        StatusText.Text = "";
        RegStatusText.Text = "";
    }

    public bool GetLoginResult() => _isLoggedIn;
}
