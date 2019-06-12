# Fork of GA Plugin for Unity, updating it for Windows

## Installation:
1. Open "unity_project_name/Packages/manifest.json"
2. Under "dependencies" add:
  ```  
  "com.ks.windows.googleanalytics": "https://github.com/kamyker/google-analytics-unity-windows.git", 
  ```
## Changes to old version:
 - Serialized fields like productName, version removed and pulled from Player Settings
 - Game end sends EndSession event (OnDestroy in GAv4 prefab)
 - Launch event sends StartSession
 - Updated WWW to UnityWebRequest

# OLD:
_Copyright (c) 2014 Google Inc. All rights reserved._

The __Google Analytics__ Plugin for Unity allows game developers to easily implement __Google Analytics__ in their Unity games on all platforms, without having to write separate implementations. Note that this is a beta and as such may contains bugs or other issues. Please report them through the Github [issue tracker](https://github.com/googleanalytics/google-analytics-plugin-for-unity/issues) or submit a pull request. The plugin comes with no guarantees.

_Unity is a trademark of Unity Technologies._ This project is not in any way endorsed or supervised by Unity Technologies.

## Google Analytics Plugin Documentation

Visit [Google Analytics Developers](https://developers.google.com/analytics/) for the latest documentation on the [Google Analytics Plugin for Unity](https://developers.google.com/analytics/devguides/collection/unity/).


### Quick links
  - [Dev Guide](https://developers.google.com/analytics/devguides/collection/unity/devguide) - Learn how to setup, configure and get started with the Google Analytics Plugin for Unity.
  - [API Reference](https://developers.google.com/analytics/devguides/collection/unity/reference) - Describes how to send data and lists all of the methods for the Google Analytics Plugin for Unity.
  - [Troubleshooting](https://developers.google.com/analytics/devguides/collection/unity/troubleshoot) - Tips on debugging and troubleshooting problems with the Google Analytics Plugin for Unity.


## Thanks
  - [Knoxx-](https://github.com/Knoxx-) for fixing a typo in the Campaign tracking permissions
  - [mataneine](https://github.com/mataneine) for filtering out meta files during iOS build post processing
  - [g8minhquan](https://github.com/g8minhquan) for identifying the sqlite3.dylib library needs to be added if using the -ObjC linker flag
  - [coquifrogs](https://github.com/coquifrogs/) for updating the HTTP status code logic
