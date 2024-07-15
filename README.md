# Visual Lobby System for Master Server Toolkit and Mirror Networking
Extention for Unity's Master Server Toolkit for infrastructure like Phantasy Star Online game

## What is Visual Lobby:
It is a term used by SEGA for Phantasy Star Online networking. It separates Lobby servers where large number of players can walk around as their avatars, perform basic actions and communicate with other players. And Room servers where actual game is being played with small number of players connected.

## What is this package:
This package expands Master Server Toolkit to provide basic functions of Visual Lobby in your game.  
This means differentiating between Lobby and Room servers, switching between different servers on fly, instantiating Lobby server automatically when Instance Spawner server is started on a machine, instantiating Room servers on demand, search filters to separate search of Lobby servers and Room servers, compatability with Mirror's Scene Interest Management.

## Package Contents:

### Mirror v89.12.2  
### Master Server Toolkit v4.17.24b 
### Visual Lobby System  
- Extensions: source collection of modified MST components to help solve errors and bugs when updating MST. For ease of use extensions marked by BF_MODIFIED region,
- Prefabs: UI prefabs for modular premade server search menu sample and core networking components prefab,
- Scenes: example scenes for package
- Scripts: custom scripts for package

## Scene Structure in sample:
### Master Server:
- MASTER scene contains modules for running Master Server. Can run Headless.

### Spawner Server:
- SPAWNER scene contains modules for running Instance Spawner server. Can run Headless.

### Client:
- CLIENT scene containing core client components and main menu. Loads first in Client.
- Lobby Scenes,
- Room Scenes.

### Visual Lobby server: 
- LOBBY_STARTER scene containing networking core and from which it is being decided which scene to load. Starts Visual Lobby server.
- LOBBY_ONLINE scene is base scene for Visual Lobby from which additional scenes are loaded and handled. See Mirror Additive Levels documentation for more.
- LOBBY_MAIN scene is scene for Visual Lobby to be played in. See Mirror Additive Levels documentation for more.

### Game Room Server:
- ROOM_STARTER containing networking core and from which it is being decided which scene to load. Starts Game Room server.
- ROOM_ONLINE scene is base scene for Game Room from which additional scenes are loaded and handled. See Mirror Additive Levels documentation for more.
- ROOM_SUBLEVELS are scenes for Game Room to be played in. See Mirror Additive Levels documentation for more.


## Third-Party used:

Mirror version - v89.2.2 [Link](https://github.com/MirrorNetworking/Mirror/releases/tag/v89.12.2)

Master Server Toolkit version - v4.17.24b [Link](https://github.com/aevien/master-server-toolkit)

## About Extentions:

Extention folder contains copies of core components changed by this system. This may help to keep track of things getting broken by updates of MST or Mirror.
