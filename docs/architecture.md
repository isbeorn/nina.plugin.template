# Architecture and lifecycle

## How a plugin is found

NINA scans its versioned plugin directory and reads assembly metadata before composing code. An assembly must export `IPluginManifest`; the generated `Plugin` derives from `PluginBase`, which reads the GUID, file version, title and other metadata from assembly attributes.

The runtime manifest describes the installed assembly. The JSON repository manifest describes what NINA can download, including its URL and checksum. Both identities and versions must agree. The publishing tools derive JSON from the compiled assembly to avoid maintaining duplicate metadata.

See [PluginBase](https://github.com/isbeorn/nina/blob/483abcbfd5ded4a479ca33207cb1e37c4c11ec43/NINA.Plugin/PluginBase.cs) and [PluginLoader](https://github.com/isbeorn/nina/blob/483abcbfd5ded4a479ca33207cb1e37c4c11ec43/NINA.Plugin/PluginLoader.cs) for the pinned host implementation.

## Composition and dependencies

NINA uses MEF, `System.ComponentModel.Composition`. An `[Export]` advertises a contract. `[ImportingConstructor]` requests dependencies by their public interface. `[Import]` on the generated manifest requests its optional feature services. Missing required imports cause composition failure, so only ask for services that NINA actually supplies.

Host exports include `IProfileService`, equipment mediators, `IImageSaveMediator`, `IOptionsVM`, `IMessageBroker`, `ISymbolBroker` and several factories. The definitive list is `PluginLoader.GetContainer`. Prefer those interfaces to reaching into application view models through global state. Constructor work should stay cheap; do not block startup with network calls or long computations.

**MEF sharing is scoped to a container.** The pinned NINA version composes the manifest and extension parts in separate containers. Two exports of one type with `CreationPolicy.Shared` do not make a process-wide singleton. The panel example registers the actual displayed instance during construction and subscribes to the assembly-scoped manifest shutdown event. Its disposal removes both subscriptions. The cross-container composition test verifies this exact host behavior.

The messaging instruction's publisher may be a different service instance from the manifest's subscriber. Both use the same host broker and GUID-scoped topic. They do not rely on object identity. Settings in the options page belong to the manifest instance used as that page's data context.

## Initialization and teardown

NINA composes extension parts, then calls `Initialize()` on the manifest. Use this stage to subscribe to host events or register patterns and symbols. The generated services make initialization and disposal idempotent: calling them twice must not duplicate subscriptions or cleanup.

Every registration needs an owner and a matching cleanup path. For example, image hooks remove both event handlers and the filename pattern; the panel removes its mediator consumer and shutdown subscription. Stop owned background work before releasing resources. Honor cancellation and observe task failures rather than launching unobserved work in constructors.

NINA's symbol API currently has no public function/provider unregister operation. The example removes its value on teardown and registers a pure function that captures no plugin instance. It reuses its provider on reinitialization.

## Assembly identity and compatibility

A plugin GUID identifies the product across releases. Its assembly name and namespaces identify serialized types. Changing these can break saved sequences even when the GUID stays the same. Keep public sequence type names stable or supply a targeted upgrader.

`MinimumApplicationVersion` controls host compatibility. The generated value matches the pinned NINA version because examples use that baseline's APIs. Before lowering it, build and test against the older NINA version.

Plugin dependencies are resolved in the host's load environment. Never distribute private copies of NINA assemblies to solve a compilation or loading problem. See [packaging](publishing.md) for the explicit dependency list.
