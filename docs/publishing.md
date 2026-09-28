# Packaging and publishing

## Define the distributable

Normal builds do not copy files into NINA. `DeployPlugin` and `PackagePlugin` first run StagePlugin, using the same explicit PluginPackageFile items in `build/Plugin.targets`.

The default list contains the plugin DLL, PDB and license. Add plugin-owned dependencies and assets with a relative Destination:

```xml
<PluginPackageFile Include="$(TargetDir)YourDependency.dll" Destination="YourDependency.dll" />
<PluginPackageFile Include="$(PluginRoot)/assets/help.txt" Destination="assets/help.txt" />
```

The plugin project enables `CopyLocalLockFileAssemblies` so NuGet runtime dependencies reach the build output. Add the plugin-owned DLLs, including their transitive dependencies, to `PluginPackageFile`. NINA assemblies, its generator assembly and other host-provided libraries should not be bundled privately. Test your package on a clean installation of a supported NINA version to catch dependencies accidentally supplied by your development machine.

Staging rejects missing inputs, duplicate destination paths and traversal outside its staging root. Its ownership marker permits cleaning only a folder previously created by the staging script. Deployment replaces staged files but preserves unrelated files and settings. Remove obsolete plugin-owned files deliberately when changing package contents.

## Package locally

```powershell
dotnet build ./src/MyPlugin/MyPlugin.csproj -c Release -t:PackagePlugin
```

The output directory is `artifacts/release`. The default installer URL uses example.invalid for local validation only. Supply PluginInstallerUrl for a real release, or run:

```powershell
./build/Build-Release.ps1 -Tag v1.2.3.4 -Repository you/MyPlugin -Channel Release
```

This command validates inputs, restores, builds, tests and creates the ZIP and JSON manifest without uploading. Set the real title and author first. Versions use four numeric components from 0 through 65534. Tags must have a lowercase `v` prefix. The tag's version is passed to SDK-generated assembly attributes, so the DLL, archive name and manifest agree.

The vendored CreateManifest script reads assembly metadata without executing the plugin. The wrapper filters empty tags, creates the archive, computes its SHA-256 and validates against the vendored central schema. The manifest's checksum refers to the final ZIP bytes. Rebuilding or recreating that ZIP afterwards requires a new checksum.

## GitHub Actions

Select `--github-actions` when creating the project. Commit the generated solution, source, build scripts and workflows to the repository in Plugin.props. CI restores, builds, runs tests when present and validates a local package on pull requests and branch pushes.

The release workflow runs on `v*` tags and rejects malformed versions. A successful package job is required before the publishing job. Push a version tag only when you intend to publish:

```powershell
git tag v1.2.3.4
git push origin v1.2.3.4
```

Only the publishing job has contents:write. Other jobs have read access. The built-in GITHUB_TOKEN is sufficient for a release in the same repository. Actions are pinned to verified commit SHAs and Dependabot proposes updates.

Release assets are immutable: the workflow does not overwrite existing assets on rerun. It downloads the assets actually present in the release and verifies their manifest/checksum. Those published files become the input to manifest submission, avoiding the different ZIP timestamps of a new build. Never move an already published release tag to another commit.

## Beta channel

Set the repository Actions variable PLUGIN_CHANNEL to `Beta` to create a GitHub prerelease and add `Channel: Beta` to the NINA manifest. Omit it or set `Release` for the regular plugin channel. This choice is separate from the host baseline: both channels use the same minimum NINA version.

Plugin users must opt into NINA's beta plugin repository to see beta manifests. See the [central repository's instructions](https://github.com/isbeorn/nina.plugin.manifests#releasing-a-manifest-as-a-beta-version). Use a new four-part version for each published build.

## Optional manifest PR submission

Select `--manifest-pr` to include submission tooling. It also enables GitHub Actions. Fork isbeorn/nina.plugin.manifests and set these repository settings:

- Actions variable MANIFEST_FORK: the exact owner/repository of your fork.
- Actions secret MANIFEST_TOKEN: a token allowed to update your fork and open the upstream PR under your account. Follow GitHub's current token permission guidance and scope access to the repositories/actions needed. Do not put it into source or local.props.

If either setting is missing, releases still publish and the workflow reports how to configure submission. Submission runs only after successful publication. It validates the manifest again, writes it to a GUID/tag-specific branch in the fork and opens an upstream PR. Reruns reuse the branch and any existing PR, including a merged one.

You can inspect the intended destination without credentials or network writes:

```powershell
./build/Submit-Manifest.ps1 -Manifest ./artifacts/release/MyPlugin.1.2.3.4.manifest.json -Fork you/nina.plugin.manifests -Tag v1.2.3.4 -DryRun
```

Without automatic submission, download the manifest release asset and submit it manually following the [manifest repository](https://github.com/isbeorn/nina.plugin.manifests).
