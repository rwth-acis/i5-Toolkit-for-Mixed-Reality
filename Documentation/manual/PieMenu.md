# Pie Menu

![Pie Menu](../resources/Logos/PieMenuLogo.svg)

## Use Case

One great challenge of Mixed Reality is the usage of User Interfaces.
Conventional 2D User Interfaces don't work due to a different set of input devices and true 3D interfaces are hard to implement and often bound to one specific use case.
A convenient compromise are 3D widgets,which allow the usage of interfaces similar to 2D interfaces in a 3D environment.

This Pie Menu is such a 3D Widget implementation. It is highly customizable and allows manipulation of the virtual world through the selection virtual tools.

## Usage

Under "i5 Toolkit for Mixed Reality/Runtime/PieMenu/Prefabs", there is the PieMenuManager prefab.
Drag it into the scene.
Once the manager and the scene is setted up as described in the following sections, it spawns a 3D Pie Menu when the menu button is pressed or the menu gesture is performed.
In this menu, the user can select a virtual tool.
These virtual tools extend the input devices, by adding various event listeners and optic indicators what which button does to them.

The behavior and appearance of a tool can be configured by the developer in the inspector of the PieMenuManager. 
There the developer can easily set up handlers for the various events thrown by the tool, for example when the tool is selected, or when a button is pressed on the controller and setup things like an icon and description texts.
The icon is shown all the time on the input device and the description texts are displayed for an adjustable amount of time, after the tool was activated.

### Setting up for Usage With HTC Vive Wands
TODO

#### Adjusting the MRTK Settings
TODO

#### Binding Input Actions#
In the inspector of the Pie Menu you see five tabs on the left.
Select the on named ""

### Creating a Menu Entry

Select the "Menu Entries" tab and then click on the button labeled "Add Entry".
This adds an entry to the Pie Menu, which you can now customize.
First, you have to choose an icon, which can be any unity sprite, and then a name for the entry.
Now you can setup the event handlers to implement the actual functionality.
All input methods have the OnToolCreated, OnToolDestroyed, OnHoverOverTargetStart, OnHoverOverTargetActive and OnHoverOverTargetStop events.
The other events depend on the used input source, but are usually something like GripPressStarted and GripPressEnded.

The Pie Menu implementation already provides some useful event handlers, which are located in the GeneralToolAction Srcipt, which is already attached to the PieMenuManager.
To assign one of them to an event, press the small "+" button TODO
You can use any method as event handler, as long as it it is in a script inheriting MonoBehaviour and as it has either no arguments or one of the type BaseInputEventData (located in the Microsoft.MixedReality.Toolkit.Input namespace).
For the hover events, you need to use the type FocusEventData instead.
The class ActionHelperFunction from the i5.Toolkit.MixedReality.PieMenu namespace provides some helpful methods for designing own event handler.
TODO explain the object transformer


#### General Tool Actions

The event handler already provided by the Pie Menu aim at providing visual feedback to the user.

- SpawnCurrentIconOverObject: This method can be used to signal the user with which objects his current tool can interact. When the provided objectTransformer accepts the currently focused target, the icon from the current tool is spawned over the collider of the object. This needs to be set as handler for the OnHoverOverTargetStart event.
- UpdateCurrentIconOverVisualisation: This method needs to be set as handler for the OnHoverOverTargetActive event, in order to make the icon over object functionality work.
- DestroyCurrentIconOverVisualisation: This method needs to be set as handler for the OnHoverOverTargetStop event, in order to make the icon over object functionality work.
- ActivateDesciptionTexts: This method activates the description texts again. For the Vive Wands it is recommended to assign this handler to the OnInputActionStartedGrip event.
- DeactivateDesciptionTexts: Deactivates the description texts again. For the Vive Wands it is recommended to assign this handler to the OnInputActionEndedGrip event.

### The Default Entry
In the "Default Behavior" tab, the behavior for the case when no handler for a thrown event in the currently selected tool is specified can be adjusted. This is handy for defining handlers that should be used by every tool and saves the work of manually adding them to them. This can for example be used to use the Activate-/DeactivateDesciptionTexts for every tool, since giving the user a way to look up what which button does is always a good idea.

## Example Scene
In 