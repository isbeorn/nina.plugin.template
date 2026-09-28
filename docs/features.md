# Feature catalog

All switches default to false. Each example owns its source, resources and tests. Tests default to enabled. Combine switches freely; the full set is tested together.

## `instruction`

A configurable notification instruction. Edit Message in the sequence and clone it: the edited value must be preserved. Cancellation is checked before notification and progress reporting.

Source: [NotificationInstruction](../templates/nina-plugin/src/NinaPlugin/Features/instruction/NotificationInstruction.cs). Generate with `dotnet new nina-plugin -n Example --instruction`.

## `condition`

Continue while Enabled is true. Toggle both values when testing. Conditions report whether their parent should continue; they should not perform expensive or irreversible work during a check.

Source: [BooleanCondition](../templates/nina-plugin/src/NinaPlugin/Features/condition/BooleanCondition.cs). Generate with `dotnet new nina-plugin -n Example --condition`.

## `trigger`

Run the trigger action container after every N completed instructions. The counter resets at its threshold and at block initialization. The configured interval and cloned action container are retained, while execution progress starts fresh.

Source: [PeriodicTrigger](../templates/nina-plugin/src/NinaPlugin/Features/trigger/PeriodicTrigger.cs). Generate with `dotnet new nina-plugin -n Example --trigger`.

## `container`

Group children under a sequential execution strategy. Cloning copies instructions, conditions and triggers separately and attaches each clone to its new parent. Base container validation includes child validation.

Source: [ExampleContainer](../templates/nina-plugin/src/NinaPlugin/Features/container/ExampleContainer.cs). Generate with `dotnet new nina-plugin -n Example --container`.

## `expressions`

Wait for a duration computed from an expression. The supported interval is 0 through 3600 seconds. The generator supplies storage, cloning and validation. Runtime code reads the evaluating scalar Seconds property.

Source: [ExpressionDelay](../templates/nina-plugin/src/NinaPlugin/Features/expressions/ExpressionDelay.cs). Generate with `dotnet new nina-plugin -n Example --expressions`.

## `sequence-upgrader`

Migrate the versioned instruction's legacy Text field to Message during BeforeCreate. The upgrader handles only its own requested type and preserves an already present new-format value.

Source: [ExampleUpgrader](../templates/nina-plugin/src/NinaPlugin/Features/sequence_upgrader/ExampleUpgrader.cs). Generate with `dotnet new nina-plugin -n Example --sequence-upgrader`.

## `settings`

Store a global message in the plugin-owned settings file and a separate message in the current NINA profile. Switching profiles refreshes bindings without overwriting another profile's values.

Source: [PluginSettings](../templates/nina-plugin/src/NinaPlugin/Features/settings/PluginSettings.cs). Generate with `dotnet new nina-plugin -n Example --settings`.

## `dock-panel`

Show telescope connection status on the Imaging tab. The panel receives mediator updates and dispatches UI changes when necessary. It unregisters the actual displayed instance on shutdown.

Source: [TelescopePanel](../templates/nina-plugin/src/NinaPlugin/Features/dock_panel/TelescopePanel.cs). Generate with `dotnet new nina-plugin -n Example --dock-panel`.

## `image-hooks`

Add an example FITS metadata header before save and a deterministic file-pattern value before finalization. The pattern key is scoped to the assembly and is removed on teardown.

Source: [ImageHooks](../templates/nina-plugin/src/NinaPlugin/Features/image_hooks/ImageHooks.cs). Generate with `dotnet new nina-plugin -n Example --image-hooks`.

## `weather-provider`

Add a simulated weather device with fixed temperature, humidity and dew point. Disconnected and unsupported readings are NaN.

Source: [WeatherProvider](../templates/nina-plugin/src/NinaPlugin/Features/weather_provider/WeatherProvider.cs). Generate with `dotnet new nina-plugin -n Example --weather-provider`.

## `messaging`

Publish a message from a sequence instruction and receive it through the host broker. A GUID-scoped topic, envelope version and expiry make ownership and message intent explicit.

Source: [PluginMessaging](../templates/nina-plugin/src/NinaPlugin/Features/messaging/PluginMessaging.cs). Generate with `dotnet new nina-plugin -n Example --messaging`.

## `symbols`

Register ExampleValue and an assembly-prefixed doubling function for expressions. The function takes one numeric argument and captures no service instance. Symbols are removed on disposal.

Source: [PluginSymbols](../templates/nina-plugin/src/NinaPlugin/Features/symbols/PluginSymbols.cs). Generate with `dotnet new nina-plugin -n Example --symbols`.

## `star-annotation`

Offer a selectable IStarAnnotator behavior that draws markers over supplied detections. It preserves the input bitmap, returns a frozen result and checks cancellation while processing stars.

Source: [ExampleStarAnnotator](../templates/nina-plugin/src/NinaPlugin/Features/star_annotation/ExampleStarAnnotator.cs). Generate with `dotnet new nina-plugin -n Example --star-annotation`.

## `editor-integration`

Demonstrate a Width setter that also updates Height. A custom property snapshot captures both values, allowing NINA's binding history to undo and redo the entire edit.

Source: [CoupledInstruction](../templates/nina-plugin/src/NinaPlugin/Features/editor_integration/CoupledInstruction.cs). Generate with `dotnet new nina-plugin -n Example --editor-integration`.

## Tooling selections

`--tests false` omits the test project and its solution entry. `--github-actions` adds CI and tag-based publishing. `--manifest-pr` additionally adds central manifest submission and implies GitHub Actions. `--no-restore` skips the post-generation restore for offline inspection or a scripted build matrix.

The generated README lists only the functional examples and publishing features selected for that project. For advanced work, read [sequencer development](sequencer.md), [services](services-and-equipment.md) and [UI/settings](ui-and-settings.md) alongside the code.

## Pinned advanced contracts

These links point to the source revision recorded by the pinned NINA package. Read the contract alongside the corresponding example above.

| Extension | Host contract |
| --- | --- |
| Sequence upgrades | [ISequenceEntityUpgrader](https://github.com/isbeorn/nina/blob/483abcbfd5ded4a479ca33207cb1e37c4c11ec43/NINA.Sequencer/Interfaces/ISequenceEntityUpgrader.cs) |
| Editor history | [ISequenceCustomPropertyEditProvider](https://github.com/isbeorn/nina/blob/483abcbfd5ded4a479ca33207cb1e37c4c11ec43/NINA.Sequencer/Editing/ISequenceCustomPropertyEditProvider.cs) |
| Star annotation | [IStarAnnotator](https://github.com/isbeorn/nina/blob/483abcbfd5ded4a479ca33207cb1e37c4c11ec43/NINA.Image/ImageAnalysis/IStarAnnotator.cs) |
| Messaging | [IMessageBroker](https://github.com/isbeorn/nina/blob/483abcbfd5ded4a479ca33207cb1e37c4c11ec43/NINA.Plugin/Interfaces/IMessageBroker.cs) |
| Symbols and functions | [ISymbolBroker](https://github.com/isbeorn/nina/blob/483abcbfd5ded4a479ca33207cb1e37c4c11ec43/NINA.Sequencer/Logic/ISymbolBroker.cs) |
| Equipment providers | [IEquipmentProvider](https://github.com/isbeorn/nina/blob/483abcbfd5ded4a479ca33207cb1e37c4c11ec43/NINA.Equipment/Interfaces/ViewModel/IEquipmentProvider.cs) |
| Image hooks | [IImageSaveMediator](https://github.com/isbeorn/nina/blob/483abcbfd5ded4a479ca33207cb1e37c4c11ec43/NINA.WPF.Base/Interfaces/Mediator/IImageSaveMediator.cs) |
| Telescope consumers | [ITelescopeConsumer](https://github.com/isbeorn/nina/blob/483abcbfd5ded4a479ca33207cb1e37c4c11ec43/NINA.Equipment/Interfaces/Mediator/ITelescopeConsumer.cs) |
