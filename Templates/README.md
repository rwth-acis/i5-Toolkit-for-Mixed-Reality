# i5 Toolkit for Mixed Reality

![i5 Toolkit for Mixed Reality](https://raw.githubusercontent.com/rwth-acis/i5-Toolkit-for-Mixed-Reality/master/Logos/Logo%20wide.svg)

This toolkit contains a collection of features for mixed reality development which can be reused in Unity projects.
It is a foundation for new projects, kickstarting the development with already completed tools and modules.

![Continuous Integration](https://github.com/rwth-acis/i5-Toolkit-for-Mixed-Reality/workflows/Continuous%20Integration/badge.svg)

![${version}](https://img.shields.io/badge/version-${version}-blue)

## Modules

The i5 Toolkit provides a series of modules and features that can be used in projects.

<table style="text-align: center; vertical-align: middle">
</table>

## Setup

There are different ways to add the package to a project.

### Alternative 1: Unity Dependency File with Git (Unity 2018.3 or later) (Recommended)

The toolkit is available as a package for the Unity Package Manager.
It can be included in new projects by referencing the git-repository on GitHub in the dependency file of the Unity project:

1. Open your project's root folder in a file explorer.
2. Navigate to the Packages folder and open the file manifest.json.
   It contains a list of package dependencies which are loaded into the project.
3. To add a specific version of the tool to the dependencies, add the following line inside of the "dependencies" object and replace [version] with "v", followed by the release number, e.g. "v${version}".
   To receive the latest version, replace [version] with upm.
   `"com.i5.toolkit.mixedreality": "https://github.com/rwth-acis/i5-Toolkit-for-Mixed-Reality.git#[version]",`
4. The i5 Toolkit for Mixed Reality has dependencies to the [i5 Toolkit for Unity](https://github.com/rwth-acis/i5-Toolkit-for-Unity) and the [Mixed Reality Toolkit](https://github.com/microsoft/MixedRealityToolkit-Unity).   
   These dependencies will automatically be fetched.
   However, the packages are loaded from scoped registries which you need to define in your manifest.json.
   Add the following two entries to the `scopedRegistries` list in your manifest.json (you may need to create the scopedRegistries object):
   ```
   {
      "name": "i5 Toolkit",
      "url": "https://package.openupm.com",
      "scopes": [
        "com.i5.toolkit"
      ]
    },
    {
      "name": "Microsoft Mixed Reality",
      "url": "https://pkgs.dev.azure.com/aipmr/MixedReality-Unity-Packages/_packaging/Unity-packages/npm/registry/",
      "scopes": [
        "com.microsoft.mixedreality",
        "com.microsoft.spatialaudio"
      ]
    }
   ```
    
    Overall, your manifest.json should now look like this:
    ```
    {
    "scopedRegistries": [
      {
        "name": "i5 Toolkit",
        "url": "https://package.openupm.com",
        "scopes": [
          "com.i5.toolkit"
        ]
      },
      {
        "name": "Microsoft Mixed Reality",
        "url": "https://pkgs.dev.azure.com/aipmr/MixedReality-Unity-Packages/_packaging/Unity-packages/npm/registry/",
        "scopes": [
          "com.microsoft.mixedreality",
          "com.microsoft.spatialaudio"
        ]
      },
      ...some other scoped registries of your project
    ],
    "dependencies": {
      "com.i5.toolkit.mixedreality": "https://github.com/rwth-acis/i5-Toolkit-for-Mixed-Reality.git#v${version}",
      ...some other dependencies of your project
    }
    ```

After that, Unity will automatically download and import the package.

If you specify "upm" to get the latest version, be aware that the package is not automatically updated.
This command just pulls the latest version which is available at that time.
To update to the newest current version, remove the package again and re-download it.

### Alternative 2: Import custom package (Unity 2017 or later)

Another option is to import the package as a .unitypackage.

1. Download the .unitypackage-file which is supplied with the corresponding release on the releases page.
2. With your project opened, perform a right-click on the assets browser in Unity. Select "Import Package > Custom Packge" from the context menu.
3. Navigate to the path where you downloaded the .unitypackage-file, select it and confirm by clicking the "Open" buttom
4. A dialog window opens where you can select which files should be imported. Select everything and click on "Import".

This import option does not import the dependencies.
Please manually import the following packages by following the instructions in each package's documentation.
Each of these packages also support an import option of custom packages.
- [i5 Toolkit for Unity](https://github.com/rwth-acis/i5-Toolkit-for-Unity)
- [Mixed Reality Toolkit](https://github.com/microsoft/MixedRealityToolkit-Unity)

Important for this alternative: If you are updating from an earlier version, it is recommended to delete the existing "i5 Toolkit for Mixed Reality" folder.
After that, import the new package.

## Example Scenes

The different modules and features are presented in example scenes which can be found in the [GitHub repository](https://github.com/rwth-acis/i5-Toolkit-for-Unity).
You can use the example scenes as an interactive documentation, an experimentation playground and to test the features.

## Unit Tests
The project is tested using Unit tests.
Continuous integration using GitHub Actions has been set up to test and deploy new versions of the package.

## Related Projects

For general Unity development, also check out the [i5 Toolkit for Unity](https://github.com/rwth-acis/i5-Toolkit-for-Unity).
It is a core package which is also used by this package to realize reusable features for general Unity projects.

## Disclaimer

This repository is not sponsored by or affiliated with Unity Technologies or its affiliates.
"Unity" is a trademark or registered trademark of Unity Technologies or its affiliates in the U.S. and elsewhere.