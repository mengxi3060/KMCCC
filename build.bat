@echo off
REM Minecraft Launcher 项目初始化脚本 (Windows)

echo ==========================================
echo   Minecraft Launcher 项目初始化脚本
echo ==========================================
echo.

REM 检查 .NET SDK
echo 检查 .NET SDK...
where dotnet >nul 2>nul
if %ERRORLEVEL% NEQ 0 (
    echo 错误: 未找到 .NET SDK
    echo 请从 https://dotnet.microsoft.com/download 安装 .NET 8.0 SDK
    exit /b 1
)

dotnet --version

REM 还原依赖
echo.
echo 正在还原 NuGet 包...
dotnet restore MinecraftLauncher.sln

if %ERRORLEVEL% NEQ 0 (
    echo 错误: 还原依赖失败
    exit /b 1
)

REM 构建项目
echo.
echo 正在构建项目...
dotnet build MinecraftLauncher.sln --configuration Release --no-restore

if %ERRORLEVEL% NEQ 0 (
    echo 错误: 构建失败
    exit /b 1
)

echo.
echo ==========================================
echo   构建成功!
echo ==========================================
echo.
echo 下一步:
echo 1. 配置数据库连接 (appsettings.json)
echo 2. 配置 Minecraft 路径
echo 3. 运行: cd MinecraftLauncher.API ^&^& dotnet run
echo.
echo API 文档地址: http://localhost:5000/swagger
echo.
pause
