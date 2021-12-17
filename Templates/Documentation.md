# Documentation of the i5 Toolkit for Mixed Reality

Click [here](https://rwth-acis.github.io/i5-Toolkit-for-Mixed-Reality/${version}/index.html) to go to the documentation pages for version ${version} of the package.

# Menu Placement System

This page is for developers who want to use this system in their applications. If you are a user, you only need to read [the user manual to this system](https://github.com/rwth-acis/VIAProMa/wiki/Menu-Placement-System#menu-placement-system). However, if you are a developer, it is recommended to read both pages. The functionalities of this system will **not** be introduced on this page.

The menu placement system (MPS) updates the position, rotation, and scale continuously to ensure that the menus are always within your reach. It places the menus based on the spatial mapping and your position. For example, it can prevent the menus from being initialized behind the wall if you are standing in front of it.

## Setup up the Environment

The system is implemented using the [_Service Core_](https://rwth-acis.github.io/i5-Toolkit-for-Unity/1.6.4/manual/Service-Core.html). To use this system, first, you need to create a scriptable object _Menu Placement Service_, which can be directly created in the asset menu under "i5 Mixed Reality Toolkit". Then you need an empty object in the scene with the _MenuPlacementServiceBootstrapper_ script and add the _Menu Placement System_ to it. You can find the already created objects in the _MenuPlacementService_ folder. Moreover, there is also a demo scene in this folder.

### Setup up the Menu Placement Service 

The system consists of two components, the _MenuPlacementService_ and _MenuHandler_. After creating the _MenuPlacementService_, you need to add all your menus to the system in the inspector, including floating variants and compact variants. The menus objects must be in the "Menu" layer to ensure that the raycast works correctly. If you don't want a certain variant of a menu, you can just leave it "None". In this case, please make the menu a "floating" variant.

### Setup up the Menu Handler

After adding the menus, you need to add the _MenuHandler_ script to all menus you added to the service. You should also set the properties correctly, especially the _Menu Variant Type_, _Compact_, and the _MenuID_. The _MenuID_ should be identical for each menu. The _MenuPlacementService_ will check these properties at the start. For a detailed description of the properties, please read the tooltips in the inspector.

## Change the User Interface Components

If you want to enable the app bar, it is recommended to add a well-fitted BoxCollider to your menu manually because the automatic calculation might be imprecise. If you want to add more functionalities, it is recommended to add an additional script for them or edit/inherit the _AppBarControllerMPS_ script. You can also change the _Suggestion Panel_ and _System Control Panel_.

## Add Your Own Scripts for Menu Functionalities

Generally, you need to write a script that realizes the functionalities of your menu. To initialize it correctly, you need to inherit the _MenuBase_ abstract class and override the _Initialize()_ and _OnClose()_ methods. For example, you can set the "target object" directly using the _TargetObject_ of the _MenuHandler_. Since the menus are stored in the [_Object Pool_](https://rwth-acis.github.io/i5-Toolkit-for-Unity/1.6.4/manual/Object-Pool.html), it is recommended to restore the menus to a proper state in the _OnClose()_ method, e.g. the state before _Initialize()_.


