# Services and equipment

## Mediators and dock panels

Ask NINA for the relevant mediator through MEF. A mediator separates the plugin from the host's concrete device view model. For continuous state, implement the matching consumer interface and register the consumer. For commands, use the mediator's supported asynchronous operations and pass cancellation through.

The telescope example implements `IDockableVM` and `ITelescopeConsumer` directly. It owns visibility and connection status, contributes a unique ContentId and supplies an icon. It registers the actual exported view model and removes it at shutdown. Read [the composition lifecycle](architecture.md) before assuming a manifest-imported panel would be the same object NINA displays.

Do not block the UI waiting for device I/O. Capture the values needed for a UI update, then dispatch them. Keep cleanup idempotent because a host may dispose the MEF container as well as call plugin teardown.

## Equipment providers

Export `IEquipmentProvider` and implement `IEquipmentProvider<TDevice>`. `GetEquipment()` returns discoverable devices. A stable Id lets NINA remember the selected device; do not generate a new random identifier every enumeration.

The weather example owns no physical hardware and returns deterministic values only while connected. Connect checks cancellation before changing state, and Disconnect is repeatable. Unsupported measurements use NaN rather than fabricated zeroes. Unsupported actions throw NotSupportedException and are not advertised in SupportedActions.

For a real driver, define connection failure handling, timeouts, polling ownership and disconnection behavior. Device state changes should notify bindings. Extend the closest device interface rather than repurposing another type merely to get a UI entry.

## Image save hooks

`BeforeImageSaved` permits additions to image metadata before file output. Choose FITS keys of at most eight characters and use the correct typed metadata header. Do not assume every image contains equipment data.

`BeforeFinalizeImageSaved` can supply a value for a registered filename pattern. The example uses a deterministic value; production patterns must produce valid path fragments. Register the pattern with IOptionsVM once and remove it on teardown. Both save event subscriptions must also be removed. Keep handlers short or honor the surrounding pipeline's asynchronous contract.

## Messaging

`IMessageBroker` supports topic subscription and asynchronous publishing. Use a stable, plugin-scoped topic and a documented envelope version. The sample message carries sender identity, UTC timestamp, expiry and message identity. Its subscriber ignores expired or unrelated messages.

The example publishes through NINA's in-process broker when its sequence instruction is executed. If you use the broker for communication with another plugin, document payload types, error handling and compatibility. Validate the payload type and version before reading its contents.

Subscriptions retain the subscriber. Unsubscribe using the same topic and object instance. A subscriber exception can affect the publisher depending on broker behavior; report useful context and avoid throwing for messages you do not own.

## Expression symbols and functions

Use ISymbolBroker to register a provider with a unique assembly-based name. Add or update named values through the returned ISymbolProvider. Tokens belong to that provider and can be removed individually.

A function registration supplies a name, usage example, argument limits and implementation. The example doubles one numeric input using invariant conversion. It is pure and does not retain disposed service state. For live functions, understand volatility and evaluation frequency before connecting them to I/O.

NINA does not expose provider/function unregister methods in this baseline. The example retains its provider for reinitialization, removes its values on teardown and documents the remaining host-lifetime function registration.

## Pluggable behaviors

A behavior implements a specific contract such as IStarAnnotator while exporting the umbrella IPluggableBehavior contract. Its Name is the user-facing choice and ContentId is a stable identity. NINA decides how that behavior is selected; merely exporting it should not silently replace the user's chosen implementation.

The annotator draws lime markers from existing detections. It returns a separate frozen bitmap, checks cancellation and respects the maximum count. Tests verify actual output pixels and the zero-count case.

For a larger example of this extension point, see [Hocus Focus](https://github.com/ghilios/hocus-focus). Use community plugins as references, but check their target NINA version before copying APIs. The [central manifest repository](https://github.com/isbeorn/nina.plugin.manifests) links many more maintained plugins.
