
# Virtual Environments

## Introduction

Mixed Reality applications often suffer from the same problem. Since the application should work with Augmented Reality, it should have no virtual background or surroundings as users will still be able to see the real work in addition to the augmentations provided by the AR device. However, this also means that when using the application on Virtual Reality devices, there will be no background or virtual environment to give users a sense of location, direction and possibly most important immersion. This is where the Virtual Environments feature comes in as an easy way to add virtual environments to your Mixed Reality or Virtual Reality application since it facilitates a user friendly interface to add virtual environment from local files or download them directly from a server. Additionally, the Virtual Environments manager is completely decoupled from the selection menu allowing for users to simply add the manager to their own menu designs. Loading of the environments is achieved via Asset Bundles which are, as the name suggests, bundles of Unity Assets. In our case this means the skybox, 3D models, materials, preview images, creator credits, etc. This way you can simply download or create your own bundles and add them as a new environment to your selection.

## User Manual

### Using the sample menu

If you just want to have the basic menu to select Virtual Environments from or if you want to simply understand how this feature works before integrating it into your own project, take a look at the sample scene provided in the toolkit. It is located under `Assets/Scenes` and contains an example menu where a couple of Virtual Environments are already entered. The environments in this test scene are all loaded from local files so you first have to input the path to your project. To do this, open the inspector of the _SampleMenu_ gameobject and take a look at the _Virtual Environments Manager_ script. There you can see three possible foldouts which will be explained later on, but for now open the _Load Environments from Local_ foldout. ![foldouts](https://user-images.githubusercontent.com/19326682/134146219-795ed3d7-c707-449b-9f62-c49e42d4e028.PNG)

You will see an input field named _Local Loading Base Path_ where you have to input the path from which you should enter the path to the folder where your Asset Bundles are located. By default, the Asset Bundles provided in this project are under `YOUR_LOCAL_PROJECT_PATH` (for example: `C:\Users\USERNAME\Documents\MYPROJECTNAME`) + \Assets\AssetBundles\". Make sure that this path is entered correctly and if there are errors later, check if this path really works (for example by entering it into the file explorer).![basepath](https://user-images.githubusercontent.com/19326682/134146224-2990fcbc-90ef-47c7-8bff-a7b2f82b4c72.PNG)

If you open the AssetBundles folder (either in Unity or the file explorer) you can see all the AssetBundles. In the Inspector of the _SampleMenu_ under the Local Path you just entered, the virtual environments can now be added by clicking on the plus and inputing a custom display name you may choose yourself and the Loading Path in the form of just the name of the AssetBundle (for example: winterloft or artgallery).![addedenvironments](https://user-images.githubusercontent.com/19326682/134146233-8eda3930-9a14-445f-af3a-26910d6b72da.PNG)

  

You may add any number you like and if your setup looks as in the picture you can finally start the scene and experiment with the sample menu. By clicking onto the displayed menu pictures, the respective environment will be loaded. By clicking the up or down buttons on the menu, you can scroll through all the environments you added.![newsamplemenu](https://user-images.githubusercontent.com/19326682/134146854-bb09406d-a3b6-4d1e-a853-653740e75ce0.PNG)

  

### Adding the feature to a custom menu

If you already have a menu design or pre-built menu you can also just add the feature to that. To start, add the _Virtual Environments Manager_ to your menu gameobject. This script handles all the loading and instantiating of environments. What still has to be linked is the spawning of the environments with the button presses in your menu. You can for example do this by implementing an event which triggers whenever one of the buttons in your menu is clicked. In this event you can then call the _InstantiateEnvironment_ method of the _Virtual Environments Manager_ with the selected environment as a parameter. There is a basic implementation of this provided in the _EnvironmentsDisplayManager_.