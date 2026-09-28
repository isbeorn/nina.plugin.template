# Welcome to the N.I.N.A. Plugin Template Repository

This repository contains examples and guidelines on how to develop a plugin for the astrophotography imaging suite [N.I.N.A. - Nighttime Imaging 'N' Astronomy](https://nighttime-imaging.eu/).

## General

The plugins for N.I.N.A. are C# class libraries, that expose certain classes to be imported by the application using the [Managed Extensibility Framework (MEF)](https://docs.microsoft.com/en-us/dotnet/framework/mef/).
Currently plugins are capable to extend the advanced sequencer's functionality by creating new instructions, instruction sets, triggers or conditions.

## Setting up the project

A Visual Studio Extension can be downloaded and installed in the [release section of the template repository](https://github.com/isbeorn/nina.plugin.template/releases/tag/1.0).
Simply install the extension and create a new project afterwards by selecting the new template.  
⚠️ Make sure to enable "Place solution and project in the same directory"! Otherwise the nuget package reference paths in the project definition are incorrect.  
⚠️ The wizard may prompt you to choose .NET Framework 4.8 instead of .NET 8. You can safely ignore this suggestion and select .NET Framework 4.8. Despite this, the project will still be created as a .NET 8 project, as this is a current limitation of the extension.
With it you can quickly setup an example project containing all necessary presets and some quick examples to get you started.


## Important Topics to consider
### Namespaces and Type names

⚠️ Once a plugin is published the namespaces and type names of the exported classes **are highly recommended to not change**.⚠️  
The reason for this is that with saving of sequences a JSON file will be generated. This JSON file will contain the fully qualified type name for each instruction. So if a namespace or type name will change for a plugin and someone will try to load a sequence that contains an instruction from a previous version of that plugin, the deserializer will fail to locate the instruction, due to looking for the old name and just insert an unknown instruction.

## Plugin Meta Data

Each plugin must implement a set of assembly attributes inside the AssemblyInfo.cs to expose the necessary meta data for a plugin to be correctly identified by N.I.N.A.

`[AssemblyTitle]` - **Required**

The name of your plugin. This name will be used by the N.I.N.A. plugin manager to show inside the list of plugins as well as using the name as a folder name for putting the plugin content inside the general plugin folder

`[Guid]` - **Required**

This is a unique identifier - using a GUID - of your plugin and must not be changed throughout the lifetime of your plugin for version increases. It is used to identify your assembly during the installation and deinstallation process.

`[AssemblyVersion] & [AssemblyFileVersion]` - **Required**

It consists of a string following "Major.Minor.Patch.Build" describing the plugin version.  

`[AssemblyMetadata(ShortDescription)]` - **Required**

A quick summary of your plugin's capabilities and features

`[AssemblyCompany]` - *Recommended*

The author (you) of the plugin

`[AssemblyMetadata(License)]` -  *Recommended*

A short name of the license in use (e.g.  MPL-2.0, MS-PL, MIT)

`[AssemblyMetadata(LicenseURL)]` -  *Recommended*

Link leading to the license text

`[AssemblyMetadata(Repository)]` -  *Recommended*

A link to the remote repository, where the source code of the plugin is available

`[AssemblyMetadata(MinimumApplicationVersion)]` -  *Recommended*

This field describes the minimum version of N.I.N.A. that this plugin is compatible with. Similar to the plugin version it consists of Major, Minor, Patch and Build.  
If multiple versions of a plugin are available, the plugin manager inside the application will serve the plugin manifest with the highest version that is compatible with the currently running application using the minimum application version.

`[AssemblyMetadata(ChangelogURL)]`

If you want to maintain a list of detailed changelogs you can add a url to your manifest that leads to the list of changes

`[AssemblyMetadata(Tags)]`

Some quick search terms to enable users to quickly search for

`[AssemblyMetadata(Homepage)]`

Homepage of the plugin creator where the plugin and more is found  

`[AssemblyMetadata(LongDescription)]`

An in-depth description of your plugin, with all the content description that is part of the plugin

`[AssemblyMetadata(FeaturedImageURL)]`

URL to a logo for the plugin. This image will be shown prominently in the app next to the name 

`[AssemblyMetadata(ScreenshotURL)]`

An image URL showing the plugin in action

`[AssemblyMetadata(AltScreenshotURL)]`

An alternative image URL showing the plugin in action from a different angle compared to the ScreenshotURL

## Exportable Interfaces

The following interfaces are available to export via MEF.

### IPluginManifest

**Mandatory to be exported once!**

The interface that defines the *plugin meta data*. Each plugin requires an export of this interface to be able to be displayed inside N.I.N.A. and for the users to see basic info about the plugin.

### ISequenceItem

*Defines an instruction for the advanced sequencer*

### ISequenceTrigger

*Defines a trigger for the advanced sequencer*

### ISequenceCondition

*Defines a condition for the advanced sequencer*

### ISequenceContainer

*Defines an instruction set for the advanced sequencer*

### IDockableVM

*Defines a dockable panel for the imaging tab*

### IPluggableBehavior

*An interface used to exchange functionality for certain operations in N.I.N.A. - currently it is possible to exchange IStarDetection, IStarAnnotator and IAutoFocusVMFactory*

### IEquipmentProvider

*An interface to export custom device drivers to use as equipment in N.I.N.A.  
The export needs to use the base type, but the implementation should inherit from `IEquipmentProvider<[ISpecificDevice]>` (e.g. `IEquipmentProvider<ICamera>`)
*  

## Available Base Classes

The N.I.N.A. packages provide a set of base classes that can be inherited from, that will already handle most of the boilerplate required for the exportable interfaces.  
Each base class provides a set of overridable methods as well as some methods that need to be implemented in the child class.

### NINA.Plugin.PluginManifest

*Implements IPluginManifest*  
This base class can be used to grab all required plugin meta data automatically.  
All required properties from the interface IPluginManifest will then be automatically populated out of the assembly meta data defined in AssemblyInfo.cs

### NINA.Sequencer.SequenceItem

*Implements ISequenceItem*

### NINA.Sequencer.SequenceTrigger

*Implements ISequenceTrigger*

### NINA.Sequencer.SequenceCondition

*Implements ISequenceCondition*

### NINA.Sequencer.SequenceContainer

*Implements ISequenceContainer*

### NINA.WPF.Base.DockableVM

*Implements IDockableVM*

Wether the dock panel button to hide/show the panel is added to the Info or the Tool side is driven by the "IsTool" property. When true it is considered a tool pane, when false it is added to the info panels.

## Constructor Injection

Exports using entities for the advanced sequencer have the ability to inject various instances from the N.I.N.A. application to be able to interact with the main application.
To inject an instance, a sequence entity just has to add the corresponding interface to be injected into the constructor. When an instance is then created in the advanced sequencer, the requested instances that correspond to the interface are injected.

The following interfaces can be injected:  
        - *IProfileService*: Get or set profile specific values  
        - *ICameraMediator*: Get camera specific info and interact with the camera  
        - *ITelescopeMediator*: Get telescope specific info and interact with the telescope  
        - *IFocuserMediator*: Get focuser specific info and interact with the focuser    
        - *IFilterWheelMediator*: Get filter wheel specific info and interact with the filter wheel    
        - *IGuiderMediator*: Get guider specific info and interact with the guider    
        - *IRotatorMediator*: Get rotator specific info and interact with the rotator    
        - *IFlatDeviceMediator*: Get flat device specific info and interact with the flat device    
        - *IWeatherDataMediator*: Get weather data specific info and interact with the weather data device  
        - *IDomeMediator*: Get dome specific info and interact with the dome      
        - *ISwitchMediator*: Get switch specific info and interact with the switch    
        - *ISafetyMonitorMediator*: Get safety monitor specific info and interact with the safety monitor  
        - *IImagingMediator*: Capture images using a capture sequence  
        - *IApplicationStatusMediator*: Notify the application of status updates, that will be displayed in the bottom status bar  
        - *INighttimeCalculator*: Retrieve nighttime data, like start of dusk, dawn etc.  
        - *IPlanetariumFactory*: Retrieve the currently selected planetarium interaction and interact with the planetarium app  
        - *IImageHistoryVM*: An object holding all captured images and their meta data  
        - *IDeepSkyObjectSearchVM*: An object to search the database for deep sky objects  
        - *IImageSaveMediator*: Save images by pushing image data to this object    
        - *IApplicationMediator*: Interact with the general application, like switching tabs  
        - *IApplicationResourceDictionary*: Retrieve application resources with this dictionary  
        - *IFramingAssistantVM*: Interact with the framing assistant using this instance  
        - *IList&lt;IDateTimeProvider&gt;*: A list of providers to get DateTimes for various astronomical events like dusk/dawn/meridian etc.  
        - *IPlateSolverFactory*: A factory to create plate solver instances  
        - *IWindowServiceFactory*: A service to create IWindowService instances  
        - *IDomeFollower*: Interaction with the dome and telescope for the dome to follow or not follow the scope  
        - *IPluggableBehaviorSelector<IStarDetection>*: This is used to select different behaviors for star detection  
        - *IPluggableBehaviorSelector<IStarAnnotator>*: This is used to select different behaviors for star annotation  
        - *IImageDataFactory*: A factory to create Image Data  
        - *IMeridianFlipVMFactory*: A factory to create a meridian flip viewmodel instance  
        - *IAutoFocusVMFactory*: A factory to create an autofocus viewmodel instance  
        - *IImageControlVM*: Control that is holding the image for display  
        - *IImageStatisticsVM*: Statistics of a session  
        - *IDomeSynchronization*: Access to dome synchronization  
        - *ISequenceMediator*: Control of the sequencer. **Must be initialized first before you can use it which is after all plugins are loaded!**  
        - *IOptionsVM*: Offers utility to inject custom image file patterns to save files with  
        - *IExposureDataFactory*: Create exposure data from in memory or files  

Example:

```csharp
[Exports(ISequenceItem)]
public class MyPluginItem : SequenceItem {
    IProfileService profileService;
    ICameraMediator cameraMediator;

    [ImportingConstructor]
    MyPluginItem(IProfileService profileService, ICameraMediator cameraMediator) {
        this.profileService = profileService;
        this.cameraMediator = cameraMediator;
    }
}
```

## Plugin DataTemplate / User Interface

### Main Page Template

Inside N.I.N.A. each plugin will have a dedicated page containing information about the plugin as well as showing available global customizations when available.
To retrieve the datatemplate for these global plugin customization options, the application will search for a datatemplate with a specific naming pattern of `<IPluginManifest.Name>_Options`. If your plugin manifest name for example is "MyAwesomePlugin" then the Datatemplate must have the key `MyAwesomePlugion_Options`  
Furthermore to be imported correctly by the application the ResourceDictionary where this DataTemplate is defined must add the correct export in the code behind using the MEF attribute `[Export(typeof(ResourceDictionary))]`.

```xml
<DataTemplate x:Key="<IPluginManifest.Name>_Options">
    <StackPanel DataContext="{Binding}" Orientation="Vertical">
        <!-- Your plugin specific options or general controls -->
    </StackPanel>
</DataTemplate>
```

### Instruction Detail Template

Each advanced sequence entity can define its own look and feel on the advanced sequencer main page.   
For ease of use a base implementation for these entities is availabe using the `SequenceBlockView` which already handles most of the layout. Custom controls can then be added into the `SequenceBlockView.SequenceItemContent`.  
Furthermore to be imported correctly by the application the ResourceDictionary where this DataTemplate is defined must add the correct export in the code behind using the MEF attribute `[Export(typeof(ResourceDictionary))]`.

```xml
<DataTemplate DataType="{x:Type local:<EntityDataType>}">
        <nina:SequenceBlockView DataContext="{Binding}">
            <nina:SequenceBlockView.SequenceItemContent>
                <StackPanel Orientation="Horizontal">                    
                    <!-- Your entity specific settings and controls -->
                </StackPanel>
            </nina:SequenceBlockView.SequenceItemContent>
        </nina:SequenceBlockView>
    </DataTemplate>
```

### Instruction Mini Template

Inside the imaging tab, there is a compact version of the advanced sequencer. Each sequence entity can define its minified version in a special datatemplate. 
This datatemplate has to follow a specific naming pattern `<Fully Qualified EntityDataType TypeName>_Mini`. 
For example if your fully qualified entity is called "MyAwesomePluginNamespace.MyAwesomeInstruction" the datatemplate key should be `MyAwesomePluginNamespace.MyAwesomeInstruction_Mini`.  
Furthermore to be imported correctly by the application the ResourceDictionary where this DataTemplate is defined must add the correct export in the code behind using the MEF attribute `[Export(typeof(ResourceDictionary))]`.

```xml
    <DataTemplate x:Key="<Fully Qualified EntityDataType TypeName>_Mini">
        <mini:MiniSequenceItem>
            <mini:MiniSequenceItem.SequenceItemContent>
                <StackPanel Orientation="Horizontal">
                    <!-- Your entity specific details in compact form -->
                </StackPanel>
            </mini:MiniSequenceItem.SequenceItemContent>
        </mini:MiniSequenceItem>
    </DataTemplate>
```

### Imaging Tab Dockable Template

Inside the imaging tab new dockable windows can be defined. For each IDockableVM interface that is exported a new panel will be available. To assign the correct ui template to it a special datatemplate needs to be exported. 
This datatemplate has to follow a specific naming pattern `<Fully Qualified DockableVMDataType TypeName>_Dockable`. 
For example if your fully qualified entity is called "MyAwesomePluginNamespace.MyAwesomeDockableVM" the datatemplate key should be `MyAwesomePluginNamespace.MyAwesomeDockableVM_Dockable`.  
Furthermore to be imported correctly by the application the ResourceDictionary where this DataTemplate is defined must add the correct export in the code behind using the MEF attribute `[Export(typeof(ResourceDictionary))]`.

```xml
    <DataTemplate x:Key="<Fully Qualified DockableVMDataType TypeName>_Dockable">
        <Grid>
            <!-- Your dock panel interface-->
        </Grid>
    </DataTemplate>
```

### Equipment Settings

Each equipment page has a separate section for settings. When a plugin provides a custom device driver, these sections can be filled with custom settings for this specific device.
Simply export a datatemplate following the postfix `<Fully Qualified Device Type TypeName>_<DeviceType>Settings`. The specific values can also be found in the static Object NINA.WPF.Base.Utility.DataTemplatePostfix.  
  
```xml
    <DataTemplate x:Key="<Fully Qualified Device Type TypeName>_CameraSettings">
        <Grid>
            <!-- Your camera specific device interface-->
        </Grid>
    </DataTemplate>
```

## Plugin Distribution

### Official Plugin Repository

N.I.N.A. has the capability to download plugins inside the application using a plugin manager. To be able to show your plugin inside the app, a manifest has to be created and uploaded to the official manifest repository.  
Please refer to the guide at the [official community plugin manifest repository](https://bitbucket.org/Isbeorn/nina.plugin.manifests/) that will describe in detail how it is done.

### Manual File Distribution

In addition to the offical distribution, you can also simply distribute your plugin by sharing the compiled file(s). To use the plugin the user has to copy the files into the folder at `%localappdata%\NINA\Plugins`

## Template License

In order to make work with the template easy, the template project is using [the Unlicense](https://unlicense.org/) and is therefore part of the public domain.
I dedicate any and all copyright interest in this plugin template to the public domain. I make this dedication for the benefit of the public at large and to the detriment of my heirs and successors. I intend this dedication to be an overt act of relinquishment in perpetuity of all present and future rights to this software under copyright law. 

## Migrate Plugin to .NET Core
The following steps describe how to upgrade a plugin to .NET 8 or above. This is required for N.I.N.A. Version 3.0 and beyond.

### Solution upgrade
1. Download [.NET 8.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
2. Download [.NET upgrade assistant](https://dotnet.microsoft.com/en-us/platform/upgrade-assistant)
```bash
# It can be installed via dotnet cli
dotnet tool install -g --add-source "https://api.nuget.org/v3/index.json" --ignore-failed-sources upgrade-assistant
```
3. Start to upgrade the project with the following command
```bash
upgrade-assistant upgrade <Your Poject Name>.csproj
```
3.a Select `In-place project upgrade (framework.inplace)`  
3.b Select `.NET 8.0 (Supported until November, 2026)`  
4. The assistant will run you through all required steps.
4.a. Depending on the complexity of your plugin there might be manual steps required. For most projects the migration will be successful without any manual steps  
4.b. Confirm the upgrade. Afterwards the upgrade-assistant will migrate the project  
4.c. Revert the deletion of your "AssemblyInfo.cs" as it is still required  
### Package update
1. It is recommended to remove all PackageReferences from the csproj file. Dotnet core handles them much better and far less direct dependencies have to be specified.
2. The next step consists of updating the N.I.N.A. nugets  
```bash
# Open the package manager console in visual studio and run the update
Update-Package NINA.Plugin -IncludePrerelease
```
3. Re-add and upgrade all necessary third party nugets that your plugin needs.
4. Change the AssemblyMetaData for `MinimumApplicationVersion` to the NINA.Plugin package version
### XAML migrations
1. NINACustomControlLibrary has been renamed to NINA.CustomControlLibrary
```
In your XAML code replace:
clr-namespace:NINACustomControlLibrary;assembly=NINACustomControlLibrary
With:
clr-namespace:NINA.CustomControlLibrary;assembly=NINA.CustomControlLibrary
```
2. OxyPlot major version has had some changes
```
WPF Plots have been moved into a separate libray.
In your XAML code replace:
clr-namespace:OxyPlot.Wpf;assembly=OxyPlot.Wpf
With:
clr-namespace:OxyPlot.Wpf;assembly=OxyPlot.Contrib.Wpf

The TrackerDefinition however resides in the WPF Shared library. Here is an example:
<UserControl 
xmlns:oxy="clr-namespace:OxyPlot.Wpf;assembly=OxyPlot.Contrib.Wpf"
xmlns:oxys="clr-namespace:OxyPlot.Wpf;assembly=OxyPlot.Wpf.Shared">
  <oxy:Plot>
    <oxys:TrackerDefinition TrackerKey="NoiseProperties">
    </oxys:TrackerDefinition>
  </oxy:Plot>
</UserControl>
```
### Code migrations
Due to the big changes in the .NET framework some concepts are moved to different namespaces or need to be replaced with a different technology. This fully depends on your plugin code and can't be covered in these steps, as they need to be done per plugin separately. However some common problems are listed below.

- Starting an external process
    - UseShellExecute for starting a process was set to true in .NET4.8, however with .NET5 and above this now defaults to false. Simply pass the flag into your process start routine
    - Process.Start(new ProcessStartInfo(&lt;path to exe&gt;) { UseShellExecute = true });
- If you are using mutexes, they have a different signature for their constructors

### Post-build events
Post Build events are transformed to xml and might have some characters replaced in error. For example %localappdata% will be %25localappdata%25. Just replace them with the original value again.  
For some reason the post build events might also fail and can't resolve the dollar tokens. Currently you can simply remove the post build event and re-add it in the build section of the UI and it will just work again