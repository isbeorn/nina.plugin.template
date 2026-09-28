# Maintaining the template

## Repository layout

`templates/nina-plugin` contains the one source template. Its .template.config directory defines CLI symbols, conditional files and Visual Studio presentation. `NINA.Plugin.Templates.csproj` packs it without compiling the unprocessed examples. The generated projects are compiled by the verification script.

## Verify a change

Developers changing the template can build and install their checkout locally:

```powershell
dotnet pack NINA.Plugin.Templates.csproj -c Release
dotnet new install ./artifacts/packages/NINA.Plugin.Templates.1.0.0.nupkg
```

Run the automated checks before submitting changes:

```powershell
pwsh ./scripts/Verify-Template.ps1
pwsh ./scripts/Test-Repository.ps1
pwsh ./scripts/Test-TemplateRelease.ps1
```

The default matrix covers the minimal starter, every individual feature, all features and every two-feature combination. It also covers tests disabled, CI disabled, CI enabled and manifest PR implying CI. Every generated combination is built and its included tests run. Use `-Quick` to skip the 91 pairwise cases while iterating, then run the full matrix before release.

Verification installs the built package into a fresh isolated template-engine hive under artifacts. Logs and generated solutions stay under artifacts. Inspect the JSON summary for the exact cases and results. The packaging checks exercise archive contents, checksum, deployment preservation, file locks and both release channels. Release scripts are tested without publishing.

Template text directives must work in every host. C# uses `#if` directives, XML uses XML-comment directives and YAML uses hash-comment directives. A deselected feature must leave no dangling references, resources or test imports. Use CLI host aliases for hyphenated switches and expose the same boolean symbols in ide.host.json.

When adding a feature, update its source, optional resources, tests, feature switch, CLI/IDE presentation, generated README and handbook. Update the verification feature list and inspect the pairwise matrix cost.

## Upgrade the NINA baseline

Check the live NuGet feed for both NINA.Plugin and NINA.Sequencer.Generators, then inspect the corresponding NINA source revision. Pin one verified version in Directory.Build.props and update MinimumNinaVersion in Plugin.props. Update the metadata assertion, handbook baseline and generated README together.

Check NINA's global.json and the generator's Roslyn dependencies before changing the SDK. Rebuild every feature, run the complete matrix and test loading in the corresponding NINA version.

Do not use floating package versions. A generated project should restore the version it was created for until its author intentionally upgrades it. The template package version is independent of the NINA package version.

## Update publishing dependencies

The official CreateManifest script, schema and license are vendored unchanged under build/vendor with their source commit recorded in PROVENANCE.md. Download replacements from a reviewed revision and update provenance. Keep adaptations in the wrapper so upstream changes remain reviewable.

Review action release notes, resolve the version tags to commits and update the pinned SHA with a version comment. Dependabot proposes action updates for this repository and generated repositories. SDK and NINA updates remain deliberate compatibility changes.

## Release the template

Configure the repository Actions secret `NUGET_API_KEY` with a NuGet.org API key allowed to publish `NINA.Plugin.Templates`. The secret is used only by the publication job.

Commit the source and push a three-part version tag. The tag sets the NuGet package version:

```powershell
git tag v1.0.0
git push origin v1.0.0
```

The Publish template workflow derives `TemplatePackageVersion` from the tag and passes it to the reusable verification workflow. That workflow builds and tests the package at that version. Publication uses its tested `.nupkg` artifact and verifies that the package version matches the tag before uploading it to NuGet.org and GitHub Releases. Only the publication job receives write permission and the API key. A failed validation or verification job prevents publication.

Each GitHub release contains the versioned package and an identical copy named `NINA.Plugin.Templates.nupkg`, keeping the README's latest-release download URL constant. Reruns skip an already published NuGet version and preserve existing GitHub assets. Use a new version tag for each release. Missing credentials fail publication with a setup message.

Template tags such as `v1.0.0` use three components. Generated plugins have their own four-part version tags, such as `v1.2.3.4`. NuGet.org may take a short time to index a new template version before it becomes installable by package ID.
