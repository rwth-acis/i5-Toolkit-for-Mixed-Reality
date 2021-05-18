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

### Un- and Redoable Actions
In order to make tool actions easily un- and redoable, the Pie Menu already offers an implementation of the Command Pattern.
To use it, your tool actions need to implement the IToolAction interface.
To do that, they need to implement the `DoAction()` and `UndoAction()` function, and implement the actual action into the do function and an action that reverses the effect into undo. 
Now instead of assigning the do action as an event handler, you need to also implement a wrapper with a function that first setups the action and then calls the already provided command stack service with
`ServiceManager.GetService<CommandStackService>()` and then call `AddAndPerformAction(action)` from it. This function can then be assigned as eventhandler.
The service manager is provided by the i5 toolkit and is in the namespace i5.Toolkit.Core.ServiceCore.
Remember that the wrapper needs to inherit MonoBehavior in order to make its funtions assignable as event handler in the PieMenuManager.

To now un- and redo these actions, you need to call the Un- and RedoAction from the already provided Undoactions script.
You can for example assign these to the left and right click on the touchpad of a Vive Wand.

## Building a Small Sample Scene
This section describes how to build a sample scene with a Pie Menu that offers a delete, resize and move, and color change tool.
Every action will be un- and redoable. The resulting scene of this can also be seen in "i5 Toolkit for Mixed Reality/Samples/PieMenu"

### General Setup
Perform the general setup as described in the Usage section.
Now create a script called SampleObject.
Attach this script to some new 3D objects like default cubes.
They need to have colliders to make them selectable with MRTK input devices.
These objects will be the ones the tools will operate on.
Also create some basic scenery like a floor and some walls, without the SampleObject component attached.
These will be important later to see if your tools operate on the correct objects and are for example not able to delete the floor.

### Delete Tool
The delete tool should be able to delete all objects with the SampleObject component attached and only these, properly communicate to the user what can be deleted and its actions should be un- and redoable.
First, create a class called DelteAction, that implements the IToolAction interface.
As only field it has a GameObject called target.
The do action should delete it and the undo action should restore it again.
In a proper project, a good way to do this would be to first serialize the GameObject before destroying it, so it can be deserialized and instantiated in the undo action again.
However, for the sake of simplicity our delete action will simply activate and deactivate the target.
For a user this looks like the object was actually deleted.
The class then looks like this:

```
using UnityEngine;
using i5.Toolkit.MixedReality.PieMenu;

public class DeleteAction : IToolAction
{
    GameObject target;

    public DeleteAction(GameObject target)
    {
        this.target = target;
    }

    void IToolAction.DoAction()
    {
        target.SetActive(false);
    }

    void IToolAction.UndoAction()
    {
        target.SetActive(true);
    }
}
```
Now you need to create the wrapper, that acts as event handler.
It needs a delete function with the BaseInputEventData as argument that reads out the currently focused object, checks is it is deletable and if yes, preforms the delete action through the command stack manager.
To get the currently focused object, you can use the `GetTargetFromInputSource()` function from the ActionHelperFunctions class.
Checking if the object is deltable shouldn't be done directly here, but with the help of an ObjectTransformer.
This is because it doesn't only need to get checked here, but also in the event handler of the tools focus events, that will signal the user if an object is deletable.
The signaling will be done later through the already provided SpawnCurrentIconOverObject functionalities.

Create a script ObjectTransformer that implements the IObjectTransformer interface.
