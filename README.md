## FPS advanced aim system
This repository contains a simple FPS project with an advanced raycast system for recognizing targets that are vulnerable only to weapons that can affect their type.
The game was created using Unity Engine.

*target invulnerable to current weapon*
![alt text](https://github.com/PiotrCynowski/FPS_advanced_aiming/blob/master/GitPics/fps_1.png?raw=true)

#### Main scene directory:
Assets\Scenes\Main.unity

#### CONTROLS
W, A, S, D: Move your character.
Space: Jump
Mouse: Rotate the camera.
Left Mouse Button: Fire your equipped weapon.
ESC: Open the in-game menu.

#### Weapons
Variety of weapons, each with targets types they are effective against.

*target vulnerable to current weapon*
![alt text](https://github.com/PiotrCynowski/FPS_advanced_aiming/blob/master/GitPics/fps_2.png?raw=true)

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
