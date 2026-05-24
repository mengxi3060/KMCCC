# Minecraft 启动器 - 前端

这是一个现代化的 Minecraft 第三方启动器前端项目，使用 React + TypeScript + Vite 构建。

## 功能特性

### 1. 用户认证
- 账号登录/注册
- 微软 OAuth 登录
- 离线模式登录

### 2. 游戏启动
- 版本管理
- 自定义 Java 路径
- 内存配置
- 快速启动服务器

### 3. 资源社区
- 资源浏览和搜索
- 资源上传
- 一键安装
- 支持模组、整合包、光影、材质包

### 4. 管理后台
- 资源审核
- 用户管理
- 资源管理

## 技术栈

- React 18
- TypeScript
- Vite
- React Router
- Tailwind CSS
- Axios
- Lucide React (图标库)

## 项目结构

```
MinecraftLauncher.Web/
├── src/
│   ├── components/       # 组件
│   │   └── Sidebar.tsx  # 侧边栏导航
│   ├── contexts/        # React Context
│   │   └── AuthContext.tsx
│   ├── pages/          # 页面组件
│   │   ├── Home.tsx
│   │   ├── Login.tsx
│   │   ├── Versions.tsx
│   │   ├── Resources.tsx
│   │   ├── Settings.tsx
│   │   └── Admin.tsx
│   ├── services/       # API 服务
│   │   └── api.ts
│   ├── App.tsx
│   ├── main.tsx
│   └── index.css
├── index.html
├── package.json
├── tsconfig.json
├── vite.config.ts
├── tailwind.config.js
└── postcss.config.js
```

## 快速开始

### 安装依赖

```bash
npm install
```

### 启动开发服务器

```bash
npm run dev
```

前端将在 http://localhost:3000 启动。

### 构建生产版本

```bash
npm run build
```

### 预览生产版本

```bash
npm run preview
```

## API 代理配置

开发服务器会将 `/api` 请求代理到 `http://localhost:5000`，确保后端 API 服务在该端口运行。

## 配色方案

- 主色调：蓝色系 (#0ea5e9)
- 强调色：绿色系 (#22c55e)
- 背景：浅灰/米白渐变
- 圆角设计，现代化 UI

## 后端集成

前端需要与 MinecraftLauncher.API 后端配合使用。确保后端服务正在运行。
