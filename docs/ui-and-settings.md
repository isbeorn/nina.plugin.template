# UI and settings

## Resource dictionaries and template keys

Every UI feature ships a WPF ResourceDictionary and a code-behind class exporting `ResourceDictionary` through MEF. NINA merges these dictionaries into application resources. XAML compilation catches missing types; constructing the templates catches additional resource and constructor failures.

Sequence editors use a DataTemplate with a DataType matching the sequence entity. Compact display templates use the fully qualified type name followed by `_Mini`. Dock panels use the fully qualified view-model type followed by `_Dockable`. These names must follow the actual namespace, not the display name.

The options page uses `<plugin display name>_Options` and receives the manifest as DataContext. The settings resource dictionary derives this key from `new Plugin().Name`, which reads assembly metadata. It avoids duplicating or interpolating a user-provided display name in XAML. Options bindings therefore use paths such as `Settings.ProfileMessage`.

Use NINA's existing controls where they express host behavior, such as `SequenceBlockView` and `ExprControl`. Own resource keys should be namespaced or otherwise unique. A generic key such as `OptionsTemplate` must not remain globally merged; the example replaces its temporary key with the proper plugin key.

## Binding and threading

A visible property needs notification when code changes it. Text fields normally commit when focus leaves the control; use UpdateSourceTrigger intentionally if immediate updates are required. Enable exception validation for setters with input validation. Do not silently coerce invalid input into an unrelated value.

WPF objects belong to their dispatcher thread. Mediator callbacks can arrive off the UI thread, so the panel marshals updates through the application dispatcher. Queued updates check whether the panel is still initialized before applying state. Image annotators freeze their returned bitmap so the host can use it safely across threads.

Use dynamic host resources when appropriate for theme changes. Add a localization resource strategy before distributing a plugin to multiple languages. Do not edit NINA's translated resources from a plugin; those belong to the host's localization pipeline.

## Global settings

The example stores global settings under NINA's application data root in `PluginSettings/<plugin GUID>/settings.json`. This folder is separate from deployed binaries and is not overwritten by DeployPlugin. The global value is JSON-encoded and written through a temporary file before replacement. Files are read during initialization rather than on every property access.

The example deliberately fails initialization for corrupt JSON instead of silently discarding the user's file. Production plugins can offer recovery with a backup and clear error message. Add a schema/version field when the persisted format grows beyond a single value. Never store credentials as ordinary plugin options.

## Profile settings

`PluginOptionsAccessor` stores typed values in the active NINA profile under the plugin GUID. Read through the accessor rather than retaining a reference to the old active profile. Subscribe to ProfileChanged to notify bindings that values may have changed, then unsubscribe on disposal.

Defaults must be meaningful when a key is absent. Renaming a key requires a migration if existing values should survive. Global settings should remain unchanged when the user switches profiles. Tests cover separate values in two profiles and global persistence across service instances.

## Testing a view

Run WPF tests on an STA thread and avoid parallel application-global resource changes. The generated composition test creates the minimal NINA window/resource environment, instantiates every selected dictionary and loads every DataTemplate. The fixture redirects NINA data storage to the test output directory before host resources create their profile services.

In NINA, load the plugin, open its options, add each sequence entity, switch profiles and exercise the panel. Binding diagnostics in Visual Studio and NINA logs help find resource keys or property paths that compile but fail at runtime.
