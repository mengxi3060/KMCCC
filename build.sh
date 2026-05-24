#!/bin/bash

echo "=========================================="
echo "  Minecraft Launcher 项目初始化脚本"
echo "=========================================="
echo ""

# 检查 .NET SDK
echo "检查 .NET SDK..."
if ! command -v dotnet &> /dev/null; then
    echo "错误: 未找到 .NET SDK"
    echo "请从 https://dotnet.microsoft.com/download 安装 .NET 8.0 SDK"
    exit 1
fi

dotnet --version

# 还原依赖
echo ""
echo "正在还原 NuGet 包..."
dotnet restore MinecraftLauncher.sln

if [ $? -ne 0 ]; then
    echo "错误: 还原依赖失败"
    exit 1
fi

# 构建项目
echo ""
echo "正在构建项目..."
dotnet build MinecraftLauncher.sln --configuration Release --no-restore

if [ $? -ne 0 ]; then
    echo "错误: 构建失败"
    exit 1
fi

echo ""
echo "=========================================="
echo "  构建成功!"
echo "=========================================="
echo ""
echo "下一步:"
echo "1. 配置数据库连接 (appsettings.json)"
echo "2. 配置 Minecraft 路径"
echo "3. 运行: cd MinecraftLauncher.API && dotnet run"
echo ""
echo "API 文档地址: http://localhost:5000/swagger"
echo ""
