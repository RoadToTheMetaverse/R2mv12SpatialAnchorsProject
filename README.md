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
 Before using this demo application, the **SpatialAnchorConfig** details must be added:
 - Account ID
 - Account Key
 - Account Domain


![Project View](https://i.imgur.com/sQCwXnc.png)
![Inspector View](https://i.imgur.com/jraeExo.png)

### Working with M2MQTT

- M2MQTT for Unity adds a **M2MqttUnityClient** MonoBehaviour that wraps the M2QTT .NET client
- Created a new manager class **MqttBrokerConnectionManager** to expose more events than M2MqttUnityClient, and implement a Scriptable Object **MqttBrokerConnectionSettings** to hold broker settings. Using a Scriptable Object makes switching between brokers quick and easy
- **MqttBrokerConnectionSettings** also holds a list of Topics to subscribe to
    - R2mvDemo/cc1 - 8 (continuous change) handle knobs values from 0 to 1
    - R2mvDemo/t1 - 8 (toggle) handle pads / taps toggle as True or False
- **MqttBrokerVSNotificationRelay** connects to the connection manager and handles sending or receiving Visual Scripting Notifications
    - Holds 4 Notifications
        -  Message Decoded (Incoming MQTT message)
        - Publish (Send message to MQTT Broker)
        - Connection Succeeded
        - Connection Failed
- **Notifications** are custom Visual Scripting units 
    - handle or send messages through the Event Bus
    - Use a scriptable object to define notifications types
    - Can include arguments (variable / values)


---

The project uses the following resources:
- Fork of [M2MQTT for Unity](https://github.com/gpvigano/M2MqttUnity) by [Giovanni Paolo Viganò](https://github.com/gpvigano)
- [MinisVS](https://github.com/keijiro/MinisVS) for midi support
- [Notifications for Visual Scripting](https://github.com/RoadToTheMetaverse/visualscripting-notifications)


Need more info, or have some questions? Head over to our [forums](https://forum.unity.com/threads/workshops-integrate-cloud-based-iot-data-into-your-xr-experience.1293402/).