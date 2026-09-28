# NinaPlugin

A plugin for NINA 3.3.0.1060 or later, targeting .NET 10. Metadata lives in Plugin.props. The plugin GUID is permanent.

## Develop

Install the .NET 10.0.4xx SDK and PowerShell 7 on Windows. Open NinaPlugin.slnx in a compatible Visual Studio 2026 installation or use the CLI.

```powershell
dotnet build
```

<!--#if (tests) -->
Run `dotnet test` for metadata, composition and selected feature tests. WPF tests use STA and isolated host data under their output directory.
<!--#endif -->

Copy local.props.example to the ignored local.props and set NinaExecutable to your NINA installation. Close NINA before running the DeployPlugin target on the project under src. Normal builds do not deploy.

## Selected examples

<!--#if (instruction) -->
- **instruction**: Notification instruction. Source: src/NinaPlugin/Features/instruction.
<!--#endif -->
<!--#if (condition) -->
- **condition**: Boolean sequence condition. Source: src/NinaPlugin/Features/condition.
<!--#endif -->
<!--#if (trigger) -->
- **trigger**: Deterministic sequence trigger. Source: src/NinaPlugin/Features/trigger.
<!--#endif -->
<!--#if (container) -->
- **container**: Sequential container. Source: src/NinaPlugin/Features/container.
<!--#endif -->
<!--#if (expressions) -->
- **expressions**: Expression-backed delay. Source: src/NinaPlugin/Features/expressions.
<!--#endif -->
<!--#if (sequence_upgrader) -->
- **sequence-upgrader**: Legacy configuration migration. Source: src/NinaPlugin/Features/sequence_upgrader.
<!--#endif -->
<!--#if (settings) -->
- **settings**: Global and profile settings. Source: src/NinaPlugin/Features/settings.
<!--#endif -->
<!--#if (dock_panel) -->
- **dock-panel**: Telescope status panel. Source: src/NinaPlugin/Features/dock_panel.
<!--#endif -->
<!--#if (image_hooks) -->
- **image-hooks**: Image metadata and file patterns. Source: src/NinaPlugin/Features/image_hooks.
<!--#endif -->
<!--#if (weather_provider) -->
- **weather-provider**: Simulated weather provider. Source: src/NinaPlugin/Features/weather_provider.
<!--#endif -->
<!--#if (messaging) -->
- **messaging**: Message broker example. Source: src/NinaPlugin/Features/messaging.
<!--#endif -->
<!--#if (symbols) -->
- **symbols**: Expression symbols and function. Source: src/NinaPlugin/Features/symbols.
<!--#endif -->
<!--#if (star_annotation) -->
- **star-annotation**: Star annotation behavior. Source: src/NinaPlugin/Features/star_annotation.
<!--#endif -->
<!--#if (editor_integration) -->
- **editor-integration**: Coupled edits with undo/redo. Source: src/NinaPlugin/Features/editor_integration.
<!--#endif -->
<!--#if (!has_features) -->
This project starts with the manifest and lifecycle skeleton. Generate a scratch project with feature switches to try the examples.
<!--#endif -->

## Package

Set your real title, author, description and repository in Plugin.props. Add plugin-owned dependencies and assets to PluginPackageFile in build/Plugin.targets. Do not include NINA assemblies.

```powershell
./build/Build-Release.ps1 -Tag v1.0.0.0 -Repository you/your-plugin
```

This builds, tests and packages without uploading. ZIP and manifest files go to artifacts/release. Plugin versions have four numeric components.

<!--#if (has_ci) -->
## GitHub publishing

CI builds, tests and validates packages for pull requests and branch pushes. Pushing a valid tag such as v1.0.0.0 publishes verified release assets. Set the Actions variable PLUGIN_CHANNEL to Beta for a prerelease and beta manifest, or leave it as Release. Only publication receives write permissions.
<!--#endif -->
<!--#if (manifest_pr) -->

Automatic manifest PR submission is included. Configure MANIFEST_FORK (owner/repository) and MANIFEST_TOKEN in repository Actions settings after creating your fork of isbeorn/nina.plugin.manifests. Missing settings skip submission with a notice; release publishing still works. Reruns reuse existing branches and PRs.
<!--#endif -->

## Learn more

Read the [NINA plugin developer handbook](https://github.com/isbeorn/nina.plugin.template/blob/master/docs/README.md), particularly [publishing](https://github.com/isbeorn/nina.plugin.template/blob/master/docs/publishing.md), [testing/debugging](https://github.com/isbeorn/nina.plugin.template/blob/master/docs/testing-and-debugging.md) and [adding examples later](https://github.com/isbeorn/nina.plugin.template/blob/master/docs/getting-started.md#add-another-example-later).
