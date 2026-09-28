# NINA plugin developer handbook

This handbook is for developers who know C# but are new to NINA. It describes the **NINA 3.3.0.1060** plugin contracts and the examples shipped with this template.

Read these chapters in order for your first plugin:

1. [Getting started](getting-started.md): prerequisites, creation and first load.
2. [Architecture and lifecycle](architecture.md): discovery, metadata, composition and teardown.
3. [Feature catalog](features.md): every selectable example and when to use it.
4. [Sequencer development](sequencer.md): execution, configuration, expressions and upgrades.
5. [UI and settings](ui-and-settings.md): WPF conventions, profiles and threading.
6. [Services and equipment](services-and-equipment.md): mediators, messaging, image hooks and behaviors.
7. [Testing and debugging](testing-and-debugging.md): reproduce problems at the right layer.
8. [Packaging and publishing](publishing.md): dependencies, releases, beta channels and manifests.
9. [Migration](migration.md): preserve existing users when modernizing a plugin.
10. [Template maintenance](maintaining.md): verify changes and update the baseline.

The [main NINA repository](https://github.com/isbeorn/nina) owns the public contracts. The [manifest repository](https://github.com/isbeorn/nina.plugin.manifests) indexes plugins and documents submission requirements. NINA's end-user documentation is maintained separately in [nina.docs](https://github.com/isbeorn/nina.docs).

Each feature chapter links to editable source examples. Select an example when creating a scratch project, run its tests and compare its behavior in NINA before adapting it.
