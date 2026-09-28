# Testing and debugging

## Start at the behavior boundary

Run `dotnet test` from the generated solution. The default tests verify metadata and MEF composition. Feature selections add tests for their actual behavior. The test project targets Windows/WPF and uses NUnit, Moq and the same pinned host libraries as the plugin.

Exercise commands, bindings and serialization with nondefault values: cloning a default-valued property can conceal an omission.

Important paired checks include initialization/disposal, subscription/unsubscription, connect/disconnect, clone/attachment and undo/redo. Expressions need valid and invalid boundary tests. A trigger needs before/after timing, reset and repeated firing tests.

## WPF and host integration

Run view construction on STA and mark fixtures NonParallelizable when they touch application resources or host globals. The composition tests emulate NINA's separate manifest and part catalogs. The resource test creates the host's required RootGrid and shared resource dictionaries before loading plugin templates.

The editor test uses real TextBox bindings with both PropertyChanged and LostFocus updates. NINA's journal is internal, so only the pinned test harness reflects its construction and commit methods. Application code must not depend on those internals. If a NINA update changes the journal, update this test adapter after verifying the supported public editor contract.

Test data goes under the test output's `runtime/<unique id>` directory. NINA can retain log handles for the life of the test process, so the fixture does not recursively delete it while the process is running. You can remove the test output after the run.

## Debugging a loaded plugin

Set the NINA executable in local.props and use the generated debug properties, or attach the managed debugger to NINA. Set a breakpoint in Plugin.Initialize. If it is never reached, inspect NINA's log for assembly, metadata, compatibility or MEF errors.

| Symptom | Check |
| --- | --- |
| Plugin absent | Correct version bucket, exported IPluginManifest and no duplicate copy in another subfolder |
| Incompatible plugin | Installed NINA version versus MinimumApplicationVersion and target framework |
| Composition rejection | Every required constructor/property import exists in PluginLoader.GetContainer |
| Missing dependency | Explicit package file list, dependency version and host-provided assembly conflicts |
| Empty options page | Exact display-name-based `_Options` resource key and exported dictionary |
| Missing sequence editor | Correct DataType, fully qualified mini key and merged resources |
| Values reset on clone | Copy constructor, generated Clone contract and deep copies of mutable state |
| UI no longer updates | PropertyChanged notification, current profile and dispatcher ownership |
| Deployment fails | NINA still has a file open, destination permissions or missing package file |
| CI fails only for expressions | SDK feature band and matching generator/runtime dependencies |

## Test in NINA

Close NINA, deploy to a dedicated development destination and restart the matching NINA version. Verify the Installed entry, options page and selected extensions. Add each entity from the palette, edit nondefault values, clone it, save and reload the sequence. Exercise cancellation. Test both profile switching and application restart for settings.

Select the simulated weather driver and star annotation behavior to exercise those examples. Check NINA's logs for binding and composition failures.
