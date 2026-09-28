# Getting started

## Prerequisites

Use Windows, the .NET 10.0.4xx SDK, PowerShell 7 and NINA **3.3.0.1060 or later**. Visual Studio 2026 needs the .NET desktop development workload and support for the pinned SDK feature band. The source generator uses newer Roslyn APIs, so an arbitrary older .NET 10 SDK is not sufficient.

The generated `global.json` selects 10.0.400 and allows patch updates. Run `dotnet --version` inside the generated directory to check resolution. If the SDK is missing, install a compatible version.

## Install the template

Install the published package from NuGet.org:

```powershell
dotnet new install NINA.Plugin.Templates
```

Alternatively, download [NINA.Plugin.Templates.nupkg](https://github.com/isbeorn/nina.plugin.template/releases/latest/download/NINA.Plugin.Templates.nupkg) and run `dotnet new install ./NINA.Plugin.Templates.nupkg` from the directory containing the file.

To install a specific template version, use `dotnet new install NINA.Plugin.Templates@1.0.0`. To update an installed template from NuGet.org, run `dotnet new update`.

## Create a project

Follow the [root quick start](../README.md). The CLI accepts `--namespace` independently of the project name and `--displayName` independently of both. Keep the project name concise. A dotted namespace is useful when publishing multiple plugins, such as `YourName.Nina.Plugins.FocusTools`.

```powershell
dotnet new nina-plugin -n FocusTools --namespace YourName.Nina.Plugins.FocusTools --displayName "Focus Tools" --plugin-author "Your Name" --repository https://github.com/you/FocusTools --instruction --settings
```

The default project has a manifest, metadata, build tooling and tests. It has no sequence instruction or panel until you select one. In NINA, the default plugin should appear under Installed even though it contributes no other UI.

For Visual Studio, install the same `.nupkg`, open Create a new project and search for **NINA Plugin**. Select the feature checkboxes in the additional options page. The generated solution includes the plugin and, unless disabled, its NUnit project. Open the generated `.slnx` if the IDE initially shows only one project.

## Edit metadata

Open `Plugin.props`. Set a meaningful title, author, description and public repository before publishing. The GUID is generated once and must remain unchanged for the lifetime of your plugin. SDK assembly attributes are generated from these properties; do not add a second handwritten set of version or title attributes.

`Product` follows the display title. Set `Copyright` for the assembly's copyright notice. Optional `PluginChangelogUrl`, `PluginFeaturedImageUrl`, `PluginScreenshotUrl` and `PluginAltScreenshotUrl` populate the changelog, logo and screenshots shown in NINA's plugin list.

Descriptions are stored as XML text. Escape XML-sensitive characters when editing the file directly. MSBuild-sensitive literals also need escaping: use `%24` for `$`, `%40` for `@`, `%3B` for `;` and `%25` for `%` when they must remain literal metadata. Template parameters are encoded automatically. The CLI uses `--plugin-author` because `dotnet new` reserves `--author`. For a new plugin with different identity, generate a new project rather than copying another author's GUID.

## Build, deploy and debug

```powershell
dotnet test
Copy-Item local.props.example local.props
dotnet build ./src/FocusTools/FocusTools.csproj -t:DeployPlugin
```

Set `NinaExecutable` in the ignored `local.props` to your NINA executable. Override `PluginDeployDirectory` if you maintain a separate development installation. Close NINA before deploying; the script checks destination file locks before copying. Reopen NINA to load the plugin. Normal builds never deploy automatically.

The default destination is `%LOCALAPPDATA%/NINA/Plugins/3.0.0/<sanitized display name>`. NINA uses `3.0.0` as its plugin compatibility directory; `MinimumNinaVersion` in `Plugin.props` sets the minimum required application version.

In Visual Studio, make the plugin the startup project. The MSBuild debug properties use the executable from `local.props`; attaching the managed debugger to a running NINA process also works. Break in `Plugin.Initialize()` to confirm loading. See [troubleshooting](testing-and-debugging.md) when the plugin is absent or rejected.

## Add another example later

The feature switches operate at project creation time. To add a feature to an existing plugin, generate a scratch project with that selection, copy its feature directory and required references, then incorporate the corresponding imports and lifecycle calls from its `Plugin.cs`. Preserve your existing metadata and GUID. Do not regenerate over a working plugin with `--force`.
