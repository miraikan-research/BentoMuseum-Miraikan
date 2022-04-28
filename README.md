<img src="https://user-images.githubusercontent.com/91875613/165704060-6c390969-6308-4ba6-8677-0347e060426b.JPG" width="400" alt="One floor of 3D printed ObentoMiraikan is placed on an iPad" align="right" />

# ObentoMiraikan

## About
This iOS app provides audio-tactile interactions to the 3D printed stackable Miraikan floor maps.
The text-to-speech audio explainations are given when a user is touching the interactive parts (exhibits and structural attractions), when a 3D printed floor is laying on the iPad. 

## Environment
- Build
  - 12.9 inch iPad Pro (built on iOS 15.1)
- Development
  - MacOSX
  - Unity 2021.1.25f1
  - Unity assets (included in this repository)
    - Speech-And-Text-Unity-iOS-Android
        - [Source](https://github.com/j1mmyto9/Speech-And-Text-Unity-iOS-Android)
        - [Tutorial](https://www.youtube.com/watch?v=XRXbVtr1fog)
    - UI Accessibility Plugin (UAP)
        - [Source and Tutorial](https://assetstore.unity.com/packages/tools/gui/ui-accessibility-plugin-uap-87935)
        - Replaced by *Speech-And-Text-Unity-iOS-Android* from V2
      
## Build
1. Open the scene *V2_7F_Graph* in scenes folder.
2. Set iOS build in Unity (a [tutorial](http://www.monobitengine.com/doc/mun/contents/Platform/Build_iOS.htm) in Japanese).
    - Scenes in build: *V2_7F_Graph*
    - Run in Xcode as: *Debug* (to print Debug.Log in Xcode)
4. Add framework and permissions in Xcode to use iOS native text-to-speech.
    - In UnityFramework's *Build Phases*, add following *Link Binary with Libraries*.
    ```
      - Speech.framework
      - AVFoundation.framework
    ```
    - In Unity-iPhone's *info.plist*, add following *Custom iOS Target Properties*. (Optional: for STT)
    ```
      - Privacy – Microphone Usage Description      
      - Privacy – Speech Recognition Usage Description
      ```
5. Add log data-saving (to iOS files) permissions in Xcode.
    - In Unity-iPhone's *info.plist*, add following *Custom iOS Target Properties* and set them to YES.
    ```
      - UIFileSharingEnabled (Application supports iTunes file sharing)
      - LSSupportsOpeningDocumentsInPlace (Supports opening documents in place)
    ```
