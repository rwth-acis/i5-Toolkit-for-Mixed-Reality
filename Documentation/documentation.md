
# Virtual Environments
<img src="https://github.com/EmilieHK/i5-Toolkit-for-Mixed-Reality/blob/features/virtualEnvironments/Assets/i5%20Toolkit%20for%20Mixed%20Reality/Runtime/virtualEnvironments/Icons/virtualEnvironmentsLogoColor.png" width="200" height="200">

## Introduction

Mixed Reality applications often suffer from the same problem. Since the application should work with Augmented Reality, it should have no virtual background or surroundings as users will still be able to see the real work in addition to the augmentations provided by the AR device. However, this also means that when using the application on Virtual Reality devices, there will be no background or virtual environment to give users a sense of location, direction and possibly most important immersion. This is where the Virtual Environments feature comes in as an easy way to add virtual environments to your Mixed Reality or Virtual Reality application since it facilitates a user-friendly interface to add virtual environment from local files or download them directly from a server. Additionally, the Virtual Environments manager is completely decoupled from the selection menu, allowing for users to simply add the manager to their own menu designs. Loading of the environments is achieved via [Asset Bundles](https://docs.unity3d.com/Manual/AssetBundlesIntro.html) which are, as the name suggests, bundles of Unity Assets. In our case this means the skybox, 3D models, materials, preview images, creator credits, etc. This way, you can simply download or create your own bundles and add them as a new environment to your selection.

## User Manual

### Using the sample menu

If you just want to have the basic menu to select Virtual Environments from or if you want to simply understand how this feature works before integrating it into your own project, take a look at the sample scene provided in the toolkit. It is located under `Assets/Scenes` and contains an example menu where a couple of Virtual Environments are already entered. The environments in this test scene are all loaded from local files, hence you first have to input the path to your project. To do this, open the inspector of the _SampleMenu_ GameObject and take a look at the _Virtual Environments Manager_ script. There you can see three possible foldouts which will be explained later on, but for now open the _Load Environments from Local_ foldout. ![foldouts](https://user-images.githubusercontent.com/19326682/134146219-795ed3d7-c707-449b-9f62-c49e42d4e028.PNG)

You will see an input field named _Local Loading Base Path_ where you have to input the path from which you should enter the path to the folder where your Asset Bundles are located. By default, the Asset Bundles provided in this project are under `YOUR_LOCAL_PROJECT_PATH` (for example: `C:\Users\USERNAME\Documents\MYPROJECTNAME`) + \Assets\AssetBundles\". Make sure that this path is entered correctly and if there are errors later, check if this path really works (for example by entering it into the file explorer).![basepath](https://user-images.githubusercontent.com/19326682/134146224-2990fcbc-90ef-47c7-8bff-a7b2f82b4c72.PNG)

If you open the AssetBundles folder (either in Unity or the file explorer) you can see all the AssetBundles. In the Inspector of the _SampleMenu_ under the Local Path you just entered, the virtual environments can now be added by clicking on the plus and inputing a custom display name you may choose yourself and the Loading Path in the form of just the name of the AssetBundle (for example: winterloft or artgallery).![addedenvironments](https://user-images.githubusercontent.com/19326682/134146233-8eda3930-9a14-445f-af3a-26910d6b72da.PNG)

  

You may add any number you like and if your setup looks as in the picture you can finally start the scene and experiment with the sample menu. By clicking onto the displayed menu pictures, the respective environment will be loaded. By clicking the up or down buttons on the menu, you can scroll through all the environments you added.![vemenu](https://user-images.githubusercontent.com/19326682/136000286-41c9c030-2f6d-43ba-82e7-068caffb3379.PNG)


### Advanced features of the sample menu
#### Loading environments from a server
The sample menu also provides many options to customize it and the display. Instead of loading the Virtual Environments from your local files you can also load them from a server by expanding the _Load Environments from Server_ foldout in the inspector of the _Virtual Environments Manager_ script. ![serverfoldout](https://user-images.githubusercontent.com/19326682/135719316-8c6eb3fa-8abf-4f69-afbc-ef175facb5ef.PNG)
For example you can load the example environments also provided in the project from the following Server Base URL: `https://github.com/JulianStaab/virtual_environments/raw/master/AssetBundles` You can enter this URL into the _Server Loading Base URL_ field and add a new entry to the _Environments from Server_ list by again entering a custom display name in the _Name_ field and the name of the Asset Bundle in the _Loading Path_ field.
![serverloading](https://user-images.githubusercontent.com/19326682/135719315-3b8490eb-00e6-43c6-82c7-2ac465eed51e.PNG)
Now the declared environments are loaded from the server in addition to potential environments that are loaded locally.

#### Changing menu display settings
The _Sample Menu_ GameObject has a child called _Environments_ which contains only the _Environments Display Manager_ script. This script controls the display of the menu items for the sample menu. In the inspector of the script, users can easily modify the display settings of the provided sample menu. ![displaymanager](https://user-images.githubusercontent.com/19326682/135719495-7647d076-136b-4b32-b2ce-05b24036f7c5.PNG)
The script provides the following customization options:

- **Display Preview Prefab** 
This is the menu item used to display the individually selectable environments. Users can replace this with their own menu items (usually buttons)
- **Display Offset** 
This vector controls the offset between the individual menu items and the style of display. A non-zero x-value would lead to a horizontal array of the menu item, while a non-zero y-value would make it a vertical display.
- **Entries Per Page**
This value toggles the amount of menu items displayed on one page of the menu.
- **Page Up Button**
This button allows users to scroll upwards through the menu. It can be replaced by a custom up-button.
- **Page Down Button**
This button allows users to scroll downwards through the menu. It can be replaced by a custom down-button.
- **Virtual Environments Manager**
This reference to the Virtual Environments Manager is used to instantiate the respective environment when it selected in the menu.

### Adding the feature to a custom menu

If you already have a menu design or pre-built menu you can also just add the feature to that. To start, add the _Virtual Environments Manager_ to your menu GameObject. This script handles all the loading and instantiating of environments. What still has to be linked is the spawning of the environments with the button presses in your menu. You can for example do this by implementing an event which triggers whenever one of the buttons in your menu is clicked. In this event you can then call the _InstantiateEnvironment_ method of the _Virtual Environments Manager_ with the selected environment as a parameter. There is a basic implementation of this provided in the _EnvironmentsDisplayManager_.

## Creating custom Virtual Environments
Virtual Environments, as used in this feature, consist of 3-4 objects bundled in an [Asset Bundles](https://docs.unity3d.com/Manual/AssetBundlesIntro.html). Custom Virtual Environments, in order to work with this feature, must have the following form:

- **Preview Image (as Sprite)** The preview image of the environment that will be displayed in the menu.
- **Skybox (as Unity Skybox)** The skybox as a .mat file that replaces the current skybox when the environment is loaded.
- **3D Model (as Unity Prefab) [optional]** The Unity Prefab of the 3D model(s) that will be instantiated when the environment is loaded. This is an optional object and your Asset Bundle will still work without this.
- **Credits (as .txt file)** The credits to the creator of the Skybox, 3D model(s) and the preview image.

Once you have these 3-4 objects assembled you can create an Asset Bundle from them by either using our _VirtualEnvironmentsCreationTool_ which is included in this toolkit (making the creation of custom virtual environments very easy) or by following [these instructions](https://docs.unity3d.com/Manual/AssetBundles-Workflow.html). If you choose to use our _VirtualEnvironmentsCreationTool_ just select the _SampleMenu_ in the VirtualEnvironmentsScene and find the _VirtualEnvironmentsCreationTool_ script attached to it.
![abct](https://user-images.githubusercontent.com/19326682/140026898-72a04573-e179-40d0-824d-5c683a66c513.PNG)

Under _Bundle Information_ you can enter your custom environments name as _Environment Name_ and the path to the folder where your environment should be saved as _Path To Target Folder_. Then, under _Virtual Environment Components_ you can add your previously selected/created objects as listed above into the respective fields. As mentioned above, you are not required to enter a 3D Model into the _Environment Model Prefab_ field. Press the _Build AssetBundle_ button just below to build the an asset bundle from the selected objects which you will then be able to find in the specified folder you entered as the _Path To Target Folder_.
Afterwards, you can add your environment from the Asset Bundle as described above.
