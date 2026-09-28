# Migrating from the old template

The old wizard generated a .NET 8 project pinned to an early NINA 3.0 beta, with every example and a post-build xcopy command. The new kit generates a configurable .NET 10 project for the pinned NINA 3.3 version.

1. Commit or back up the existing plugin and its release metadata.
2. Generate a scratch project with the matching features. Compare its project, metadata and build setup with your project.
3. Preserve the original plugin GUID. Preserve assembly/type names used in saved sequences unless you implement and test an upgrade path.
4. Move metadata into Plugin.props and remove duplicate handwritten SDK-generated attributes. Retain the old file version until you deliberately increment it.
5. Update the framework and NINA package baseline together. Add the matching generator package when adopting expressions and test with the required SDK feature band.
6. Replace automatic post-build deployment with the explicit staging/deploy targets. Add only your plugin's owned dependencies and assets.
7. Audit clone methods for every persisted property, including false/zero values. Audit event and consumer registrations for matching cleanup.
8. Use the new options and resource-key conventions. Compile XAML and construct affected views.
9. Add a publishing workflow only after local packaging and host loading work. Retain your public repository and release history.

When porting the old examples, copy configured fields in instruction and condition clones and replace the trigger's random firing with the intended condition.

Global settings in the new settings example use a plugin-owned JSON file. To preserve settings from a previous Properties.Settings file, migrate them with a backup and a one-time migration marker. Migrate profile option values when renaming their keys.
