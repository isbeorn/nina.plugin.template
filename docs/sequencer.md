# Sequencer development

## Export and execution contracts

Instructions implement `ISequenceItem`, usually by deriving from `SequenceItem`. Conditions derive from `SequenceCondition`; triggers derive from `SequenceTrigger`. Export the appropriate interface and provide Name, Description, Category and Icon metadata. NINA applies these labels when it builds its sequence palette. Use an existing host icon key or a plugin-specific resource key.

Persist configuration with `[JsonObject(MemberSerialization.OptIn)]` and `[JsonProperty]`. Runtime progress, cancellation sources, host services and cached measurements should not be serialized. Raise property-change notifications when values change, including changes made outside the UI.

An instruction's `Execute` receives progress and a cancellation token. Forward the token to asynchronous APIs. Check it before side effects and inside long loops. Never use `async void` for execution. Let meaningful failures propagate so NINA's sequence error handling can act on them. The instruction base class owns the surrounding run state.

Conditions should be quick and side-effect free. Trigger checks distinguish before-item and after-item timing. The periodic example uses `ShouldTriggerAfter`, ignores an absent previous item and resets after each threshold. Its `Execute` runs `TriggerRunner`, allowing users to choose the triggered actions. The runner uses the host's isolated trigger context so it can access target data without firing its own parent trigger recursively. Its previous attachment is restored even on failure.

## Cloning and parent attachment

NINA's factories clone the exported prototypes when users add items. A clone that copies only metadata loses the user's configuration. Copy every persisted property and deep-copy mutable children. The instruction and condition tests deliberately use nondefault values to catch this mistake.

Copy metadata through the base copy constructor or `CopyMetaData`. For containers, deep-copy items, conditions and triggers, then call `AttachNewParent` on every child. Do not assign Parent directly where attachment hooks are required. Reset execution counters and cancellation state rather than copying an in-progress run.

Use base container behavior for normal sequential execution and validation. A custom container should not bypass child validation or omit parent hooks. Changes to child ownership can also affect expression context and editor history.

## Expressions and the generator

The `expressions` selection adds `NINA.Sequencer.Generators` at exactly the host package version. Its assembly contains attribute definitions needed at runtime as well as the compiler generator. The project keeps the runtime dependency available to tests while keeping analyzer/build assets private. NINA supplies its own host assemblies at deployment.

Use a top-level partial class with `[UsesExpressions(GenerateValidation = true)]` and public partial properties marked `[IsExpression]`. The generated members provide expression objects, scalar accessors, cloning and validation. Keep the copy constructor expected by the generator and do not add a conflicting handwritten Clone or Validate method.

```csharp
[IsExpression(Default = 1, Range = [0, 3600])]
public partial double Seconds { get; set; }
```

Bind the host `ExprControl.Exp` to `SecondsExpression`. Read `Seconds` in execution code: it evaluates the current value. Do not substitute a cached expression value or proxy field. Validate invalid expressions and range boundaries through `IValidatable`. The sample validates on parent changes and before execution.

Generated validation can be extended with preparation and additional-validation hooks. Add errors to the supplied issue list. Read the [generator contract](https://github.com/isbeorn/nina/blob/483abcbfd5ded4a479ca33207cb1e37c4c11ec43/NINA.Sequencer.Generators/ARCHITECTURE.md) before using proxies, conditional validation or custom validators.

## Saved configuration upgrades

An `ISequenceEntityUpgrader` can participate before creation, during creation, after creation or after population. Use the earliest stage needed for the change. The example transforms a JSON field before object creation and returns the existing object unchanged.

Always constrain an upgrader to types and versions you own. Preserve new-format values if both old and new fields occur. Make the migration idempotent and test unrelated JSON. Renaming an assembly or class is more involved than renaming a field because NINA must resolve the old serialized type; inspect the host serialization path before doing so.

## Undo and redo

Ordinary persisted scalar fields normally participate in NINA's binding history without plugin-specific code. Implement `ISequenceCustomPropertyEditProvider` when one setter modifies several configuration values. Return a snapshot only for the property requiring custom handling, and return false for ordinary fields.

The coupled example serializes Width and Height. Changing Width changes both, so the snapshot records both before and after the gesture. Restoring a snapshot uses normal setters and then restores the second value.

The tests create a real TextBox binding, simulate a text gesture and use NINA's actual editing behavior to undo and redo both values. The journal itself is internal to this NINA version, so the test harness reflects that pinned host type. Production plugin code only uses the public snapshot contract. For custom collections, attachment-state snapshots or explicit transactions, consult [NINA's editor integration guide](https://github.com/isbeorn/nina/blob/483abcbfd5ded4a479ca33207cb1e37c4c11ec43/NINA.Sequencer/Editing/README.md).
