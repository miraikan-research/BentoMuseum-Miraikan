# BentoMuseum

### [Project Page](https://xiyue-w.github.io/projects/BentoMuseum) | [Paper](https://xiyue-w.github.io/assets/pdf/XWang_ASSETS_2022_BentoMuseum.pdf) | [Data](https://www.thingiverse.com/thing:5555502)
 [BentoMuseum: 3D and Layered Interactive Museum Map for Blind Visitors](https://doi.org/10.1145/3517428.3544811)  
 [Xiyue Wang](https://xiyue-w.github.io/)<sup>1</sup>,
 [Seita Kayukawa](https://wotipati.github.io/)<sup>1</sup>,
 [Hironobu Takagi](https://researcher.watson.ibm.com/researcher/view.php?person=jp-TAKAGIH)<sup>1</sup>,
 [Chieko Asakawa](https://researcher.watson.ibm.com/researcher/view.php?person=us-chiekoa)<sup>1</sup> <br>
 <sup>1</sup>Miraikan – The National Museum of Emerging Science and Innovation.  
in ASSETS 2022 (Technical paper)

<img src="https://user-images.githubusercontent.com/91875613/165704060-6c390969-6308-4ba6-8677-0347e060426b.JPG" width="350" alt="One floor of 3D printed ObentoMiraikan is placed on an iPad" align="right" />

## About
This iOS app provides audio-tactile interactions to the [3D-printed stackable museum floor maps](https://www.thingiverse.com/thing:5555502).  
Text-to-speech audio explanations are given when a user touches the interactive parts (exhibits and structural attractions) when a 3D-printed floor is on the iPad. 

## Environment
- Target OS
  - 12.9 inch iPad Pro (built on iOS 15.1)
- Build and Development
  - Unity 2021.1.25f1
  - Unity assets (included in this repository)
    - Speech-And-Text-Unity-iOS-Android
        - [Source](https://github.com/j1mmyto9/Speech-And-Text-Unity-iOS-Android)
        - [Tutorial](https://www.youtube.com/watch?v=XRXbVtr1fog)
    - UI Accessibility Plugin (UAP)
        - [Source and Tutorial](https://assetstore.unity.com/packages/tools/gui/ui-accessibility-plugin-uap-87935)
        - Replaced by *Speech-And-Text-Unity-iOS-Android* from V2
  - Xcode 13.4.1 and Apple Development Certificate
      
## Build
1. In Unity, open the scene *V2_7F_Graph* in scenes folder.
2. In Unity, set iOS build (a [tutorial](http://www.monobitengine.com/doc/mun/contents/Platform/Build_iOS.htm) in Japanese).
    - Scenes in build: *V2_7F_Graph*
    - Run in Xcode as: *Debug* (to print Debug.Log in Xcode)
4. In Xcode, add framework and permissions to use iOS native text-to-speech.
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
5. In Xcode, add log data-saving (to iOS files) permissions in Xcode (Optional: if you want to save double-tap's log data). 
    - In Unity-iPhone's *info.plist*, add following *Custom iOS Target Properties* and set them to YES.
    ```
      - UIFileSharingEnabled (Application supports iTunes file sharing)
      - LSSupportsOpeningDocumentsInPlace (Supports opening documents in place)
    ```
6. In Xcode, select your target system, build and run.
