# Documentation of the i5 Toolkit for Mixed Reality

Click [here](https://rwth-acis.github.io/i5-Toolkit-for-Mixed-Reality/${version}/index.html) to go to the documentation pages for version ${version} of the package.

# Menu Placement System

This page is for developers who want to use this system in their applications. If you are a user, you only need to read [_the user manual to this system_](https://github.com/rwth-acis/VIAProMa/wiki/Menu-Placement-System#menu-placement-system) to know what this system can do. However, if you are a developer, it is recommended to read both pages in order to get a complete view of this system. Note that the functionalities of this system will _**not**_ be introduced on this page. A demo scene can be found at `i5 Toolkit for Mixed Reality/Runtime/MenuPlacementSystem/Demo`.

The menu placement system (MPS) updates the position, rotation, and scale continuously to ensure that the menus are always within a user's reach. It places the menus based on the spatial mapping and your position. For example, it can prevent the menus from being initialized behind the wall if you are standing in front of it. Beyond this basic functionality, it also provides other advanced functionalities like manipulating the menus, adding runtime menus, and saving & loading the offsets. Please read the tooltip in the inspector and comments to methods for detailed information.

## Setup up the Environment

The system is implemented using the [_Service Core_](https://rwth-acis.github.io/i5-Toolkit-for-Unity/1.6.4/manual/Service-Core.html). To use this system, first, you need to create a scriptable object `Menu Placement Service`, which can be directly created in the asset menu under "i5 Mixed Reality Toolkit". Then you need an empty object in the scene with the `MenuPlacementServiceBootstrapper` script and add the `Menu Placement System` to it. You can find the already created objects in the `MenuPlacementService` folder. Moreover, there is also a demo scene in this folder.

### Setup up the Menu Placement Service 

The system consists of two components, the `MenuPlacementService` and `MenuHandler`. After creating the `MenuPlacementService`, you need to add all your menus to the system in the inspector, including floating variants and compact variants. The menus objects must be in the "Menu" layer to ensure that the raycast works correctly. If you don't want a certain variant of a menu, you can just leave it `None`. In this case, please make the menu a `Floating` variant.

### Setup up the Menu Handler

After adding the menus, you need to add the `MenuHandler` script to all menus you added to the service. You should also set the properties correctly, especially the `Menu Type`, `Variant Type`, and the `MenuID`. The `MenuID` should be identical for each menu. The system will check these properties at the start and raise errors or warnings if any. For a detailed description of the properties, please read the tooltips in the inspector.

## Change the User Interface Components

If you want to enable the app bar, it is recommended to add a well-fitted `BoxCollider` to your menu manually because the automatic calculation might be imprecise. If you want to add more functionalities, it is recommended to add an additional script for them or edit/inherit the `AppBarController` script. You can also change other UI components, but make sure there are no errors.

## Add Your Own Scripts for Menu Functionalities

Generally, you might need to write a script that realizes the functionalities of your menu. To initialize it correctly, you need to inherit the `MenuBase` abstract class and override the `Initialize()` and `OnClose()` methods. For example, you can set the "target object" directly using the `TargetObject` of the `MenuHandler`. Since the menus are stored in the [_Object Pool_](https://rwth-acis.github.io/i5-Toolkit-for-Unity/1.6.4/manual/Object-Pool.html), it is recommended to restore the menus to a proper state in the `OnClose()` method, e.g. the state before `Initialize()`.

## Notice for Runtime Menus

The menu placement system allows developers (and users) to add runtime menus. They are menus that are added in the runtime and are especially not specified in the inspector. It is supposed to make it possible for developers to discuss menus simultaneously and virtually in the runtime through the internet, e.g. in the [_MockUp-Editor_](https://github.com/rwth-acis/VIAProMa/tree/origin/mockupEditor). Although it is possible to use this functionality to add an arbitrary object to the menu system and let them behave like a menu, which doesn't need to be a menu, i.e. has a `MenuBase` instance attached, it might lead to unexpected behaviors while people use this functionality in some applications. As a developer, you can either just disable it or specify the `Runtime Menu Layer` in the inspector of `Menu Placement Service` to limit the selectable objects.

