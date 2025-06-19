## FPS advanced aim system
This repository contains a simple FPS project with an advanced raycast system for recognizing targets that are vulnerable only to weapons that can affect their type.
The game was created using Unity Engine.

*target invulnerable to current weapon*
![alt text](https://github.com/PiotrCynowski/FPS_advanced_aiming/blob/master/GitPics/fps_1.png?raw=true)

#### Main scene directory:
Assets\Scenes\Main.unity

#### CONTROLS
- **W, A, S, D** – Move your character  
- **Space** – Jump  
- **Shift** – Sprint (Hold to run)  
- **Mouse** – Rotate the camera  
- **Left Mouse Button** – Fire equipped weapon or throw item (Hold for continuous fire)  
- **Middle Mouse Button / Mouse Scroll Down** – Switch to next weapon  
- **Mouse Scroll Up** – Switch to previous weapon  
- **Right Mouse Button** – Grab or focus on an item (Hold to maintain focus)  
- **E** – Interact with objects  
- **ESC** – Open or close the in-game pause menu

#### Weapons
Includes a variety of weapons, each with unique shot types, firing rates, and magazine sizes. Weapons are effective against specific target types, defined by customizable configurations.
Features include:
- Different weapon and rifle types for varied gameplay.
- Configurable shot intervals, reload times, and ammo capacity.
- Bullet templates with impact effects and muzzle flashes.
- 3D models with precise positioning and dynamic crosshair integration.
- Smooth weapon switching and animations.
- Realistic weapon sway and bobbing for immersion.
  
*target vulnerable to current weapon*
![alt text](https://github.com/PiotrCynowski/FPS_advanced_aiming/blob/master/GitPics/fps_2.png?raw=true)

#### Grab
Some items can be grabbed; these items will have an indicator when the player is close enough, signaling that they can be picked up. A second indicator will appear when the player is close enough to the item, pointing at it, signifying that the item can be grabbed. Once the item is grabbed, the player can throw it using the left mouse button.

#### Game UI
The game's User Interface (UI) provides information to the player: 
Crosshair: The crosshair changes color to indicate whether you can damage a target. Green means the target is vulnerable, while red means the target is invulnerable to your current weapon.
Weapon Info Window: A small window in the UI displays information about your currently equipped weapon, including its name, damage, and other relevant details.

#### Target Prefabs
Interactive target prefabs, each with distinct behaviors and responses. 
Door: Can be opened by destroying their power supply.
Energy Field: Certain targets generate an energy field that moves and adapts when attacked. 
Hydrant, Water Barrel, Oil Barrel: attacking an oil barrel leads to fluid effects

*editor view*
![alt text](https://github.com/PiotrCynowski/FPS_advanced_aiming/blob/master/GitPics/fps_3.png?raw=true)
