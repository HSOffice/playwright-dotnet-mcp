# ExternalBrowserWinForms

该示例解决方案展示了如何将 Windows Forms 界面与浏览器启动业务逻辑解耦。

## 结构

```
ExternalBrowserWinForms.sln
└── src
    ├── ExternalBrowserWinForms.Core      # 纯业务逻辑、模型与验证
    └── ExternalBrowserWinForms.App       # WinForms 界面层，依赖 Core
```

- `ExternalBrowserWinForms.Core` 暴露 `IBrowserLaunchService` 等接口，以业务可测试的形式封装浏览器启动行为。
- `ExternalBrowserWinForms.App` 仅负责用户交互，通过依赖注入获取业务服务。

## 运行

在 Windows 环境中使用 .NET 8：

```bash
cd ExternalBrowserWinForms
msbuild ExternalBrowserWinForms.sln
```

或者在 Visual Studio 中打开解决方案即可运行。
