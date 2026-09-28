# NINA plugin developer kit

Create a plugin for **NINA 3.3**, using .NET 10, from either the command line or Visual Studio. Start with a minimal manifest and NUnit tests, then select only the examples you need.

The template pins `NINA.Plugin 3.3.0.1060-nightly` and sets the minimum NINA version to `3.3.0.1060`.

## Quick start

On Windows, install the .NET **10.0.4xx SDK**, PowerShell 7 and, for IDE development, Visual Studio 2026 with the .NET desktop development workload and support for that SDK feature band.

Install the template from NuGet.org, then create your plugin:

```powershell
dotnet new install NINA.Plugin.Templates
dotnet new nina-plugin -n MyPlugin --displayName "My First Plugin" --plugin-author "Your Name" --repository https://github.com/you/MyPlugin
cd MyPlugin
dotnet test
```

The same installed template appears as **NINA Plugin** in Visual Studio's Create a new project dialog. Restart Visual Studio after installation if necessary.

For a direct download, get [NINA.Plugin.Templates.nupkg from the latest release](https://github.com/isbeorn/nina.plugin.template/releases/latest/download/NINA.Plugin.Templates.nupkg), then run `dotnet new install ./NINA.Plugin.Templates.nupkg` from your download directory.

Add a working sequence instruction and a settings page when creating the project:

```powershell
dotnet new nina-plugin -n MyPlugin --instruction --settings --github-actions --manifest-pr --displayName "My First Plugin" --plugin-author "Your Name" --repository https://github.com/you/MyPlugin
```

Use `dotnet new nina-plugin --help` for all selections. Every functional example is opt-in. Tests are included unless you specify `--tests false`. GitHub Actions are optional; `--manifest-pr` also enables them.

## Next steps

- [Developer handbook](docs/README.md): setup, architecture, every extension point, testing and publishing.
- [Feature catalog](docs/features.md): choose examples and understand their behavior.
- [Publishing](docs/publishing.md): tag-driven releases and optional central manifest PRs.
- [Migration](docs/migration.md): move from the old wizard without changing your plugin identity.
- [Maintaining this template](docs/maintaining.md): packaging, test matrix and NINA version upgrades.
