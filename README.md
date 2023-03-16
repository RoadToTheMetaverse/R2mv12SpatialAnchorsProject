![R2MV](https://i.imgur.com/SAdxi7s.png)

# Episode 12 - Build geolocated and social experiences in AR

<img src="https://i.imgur.com/7ITpuLS.gif">

## About this project
Part of the [Road to Metaverse, Creator Series](https://create.unity.com/road-to-metaverse), this demo was used in Episode 12 to learn how to build a geolocated AR exeprience, using Azure Spatial Anchors!

## Synopsis
Place persistent content in the realworld, that can be found/located later. Using AR Foundation and Azure Spatial Anchors, this demo will show how to place and retrieve anchors!

<br>

---

<br>

## Demo
THe main scene to explore is under **R2mv12Assets/Scenes/**

### R2mvSpatialAnchorsDemo 

![Editor screenshot](https://i.imgur.com/Yccopr1.png)




<br>

### Setup Azure Credetials

<img src="https://i.imgur.com/sQCwXnc.png" width=400>
<img src="https://i.imgur.com/jraeExo.png" width=400>

 Before using this demo application, the **SpatialAnchorConfig** details must be added:
 - Account ID
 - Account Key
 - Account Domain

To setup your credentials on the [Azure Portal](https://portal.azure.com/), check out [these instructions](https://learn.microsoft.com/en-us/azure/spatial-anchors/quickstarts/get-started-unity-android?tabs=azure-portal#prerequisites).


<br>

## Demo Walkthrough

All code is contained in the **SpatialAnchorsDemo** component. Using the Azure SDK **SpatialAnchorManager** to manage authentication and interface with Azure backend services.

<br>

### Spatial Anchors Demo
- Defined 4 app states
    - Start
    - Placing Anchor
    - Creating Spatial Anchor
    - Locating Anchor
- Created 4 UI views for the app’s UX
- References
    - The prefab to use as visual anchor
    - SpatialAnchorManager
    - ARRaycastManager
- Public API -> handle user requests for
    - Restart
    - PlaceAnchor
    - SaveAnchor
    - LocateAnchor
    - ClearHistory
- Demo Lifecycle -> Manages the state of the application
    - Initialization
    - Update elements based on Demo State
    - Update UI
- Spatial Anchor Event Handlers -> Interface with the SpatialAnchorManager 
    - Mostly event handler (Delegates)
    - CloudManagerOnAnchorLocated -> Called when a cloud anchor is located! Use LocateAnchorStatus.Located to place an AR Anchor.
    - Show AddFoundCloudSpatialAnchor CloudNativeAnchor -> Azure SDK component 
CloudToNative(anchor) handles placing the anchor in the correct position and rotation!
- History and Anchor creation management
    - CreateSpatialAnchor
        - Complex method
        - Responsible creating the Azure Anchor using the local AR Foundation anchor
        - Handles communication with backend service for visually positioning anchors and saving the data on Azure
        - Waits for IsReadyForCreate
        - Uses CreateAnchorAsync
    - LoadSpatialAnchorHistory
    - SaveSpatialAnchorHistory
    - System uses the same data structure as the Google Cloud Anchor demo!
        - SpatialAnchorHistory
        - SpatialAnchorHistoryCollection
        - PlayerPrefs and JSON serialization / de-serialization 

<br>

---

<br>

## Resources
- [Azure Spatial Anchors Samples](https://github.com/Azure/azure-spatial-anchors-samples/tree/master/Unity)
- [Quickstart - Android](https://learn.microsoft.com/en-us/azure/spatial-anchors/quickstarts/get-started-unity-android?tabs=azure-portal)


Need more info, or have some questions? Head over to our [forums](https://forum.unity.com/threads/workshops-build-geolocated-and-social-experiences-in-ar.1293414/).