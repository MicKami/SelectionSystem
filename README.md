# Selection System

## Overview
This project provides a powerful and versatile selection mechanism that allows you to select game objects with pixel-precise accuracy in a 3D environment. 

### Features:

-   **Pixel-Precise Selection:** Select objects with precision down to the pixel level in your 3D scene. Instead of using a common approach of utilizing physics system to determine selection, it uses the rendering process to identify selected objects, ensuring an efficient and visually pleasing selection experience.
    
-   **Drag Rectangle Selection:** Support for drag rectangle selection, allowing selecting multiple objects at once with ease.
    
-   **Selection Outline:** Adds outline effect to selected and highlighted objects.

### Implementation Details:
- At startup each selectable object gets assigned unique ID as *uint*.
- It then uses URP *Scriptable Renderer Feature* to render those IDs as a *Color32* to an ID Map (whose resolution can be downscaled up to x4). It utilizes modified URP *Lit* shader with additional unlit pass and *PerMaterial* properties taking advantage of *SRP Batcher*, effectively reducing the number of *draw calls*.
- At runtime the ID Map is sampled using *AsyncGPUReadback* API for performance friendly access to GPU resource. In case of rect selection, *compute shader* is used to read the data in parallel and output the resulting IDs into compute buffer.
- The outline effect around selected objects is also achieved using *Scriptable Renderer Feature*. Selection is rendered to separate render target which is then processed with simple dilate shader and blitted to the final screen output.


## Screenshots
![](https://i.imgur.com/yCgG8wM.jpg)
![](https://i.imgur.com/x2UBx6x.jpg)
## Demo
Download [**DEMO**](https://mickami.itch.io/selectionsystem-demo)
The Demo includes 2 simple scenes:
- First one (on screenshots) allows for using selection as well as simple camera controller (WASD for movement, scroll for zoom) and unit control (right click to move selected cubes).
- The second one allows only for using selection on dynamic objects (demonstrates depth sorting).

## Credits
Inspired by: https://medium.com/@philippchristoph/pixel-perfect-selection-using-shaders-16070d3094d

## License
 [![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
