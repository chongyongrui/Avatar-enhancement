# Avatar Enhancement

## Table of Contents
- [Summary](#Summary)
- [Credits](#credits)
- [Installation](#installation)
- [Features](#features)
- [Walkthrough](#walkthrough)

## Summary
Avatar Enhancement is a project that harnesses the capabilities of Netcode, an advanced networking technology in Unity, to provide a secure and offline networking solution. Unlike other libraries like Photon and Fish, which may lack robust security measures or require constant online connectivity, Netcode offers a reliable and safe alternative that can be used offline.

The project includes essential features such as user movement controls, camera rotation, and an intuitive UI for hosting and connecting clients.Moreover, it operates offline, making it well-suited for LAN parties, local tournaments, and scenarios where an internet connection is not available or desired.

Avatar Enhancement is an evolving project, continuously introducing new features to enhance gameplay possibilities. The latest additions include a drone camera for capturing unique perspectives and waypoint placing for AI-controlled cars, unlocking exciting opportunities for your projects.

## Credits

This project was made possible thanks to the contributions of the following team members:

- [Lurking Goo](https://github.com/LurkingGoo) (Game development)
- [Aortz](https://github.com/Aortz/) (Authentication with bitcoin technology)
- [chongyongrui](https://github.com/chongyongrui) (Database)


Special thanks to our advisors and mentors for their valuable guidance and support throughout the development process.


## Installation

**Refer to the Installation guide [here](https://quill-carriage-fc8.notion.site/Avatar-Project-Installation-Guide-6158f3a430fb4f3b88495f2bebacaef4?pvs=4) for specific step-by-step instructions.**

**Refer to the documentation guide [here](https://equinox-sawfish-01c.notion.site/03b7c4c1c4db4030b03cb3264bd384b4?v=8474e109f1d54671b730f706de3fdfed&pvs=4)**

To set up the framework and libraries for this project, follow these steps:

1. Install .NET Framework.
2. Install .NET SDK.
3. Install Visual Studio Code.


### Setup
To clone this repository, use the following command:

``` 
git clone https://github.com/MandSFun/Avatar-enhancement.git

```
1. Create folder 'Avatar'.
2. Clone into folder.
3. Ensure that there is a folder called Build; This is folder you will Build and Run in.
4. Open project, under File > Build Settings > Build and Run > Choose the "Build" folder directory.
5. Select appropriate scenes to be built

![build](https://github.com/LurkingGoo/Avatar-enhancement/assets/100992221/48fe5f33-c35f-4e70-aa12-bf69a8e40d28)


### Setting up of blockchain technology

Refer to [Here](https://equinox-sawfish-01c.notion.site/How-to-set-up-Blockchain-ff080acd6d4f4bdb8453987feac23de9?pvs=4) 
### Import and Export of package files
To export specific files from the project clone,ensure the project importing the files has the following references.
```
"com.unity.inputsystem": "1.4.4",
"com.unity.netcode.gameobjects": "1.2.0",
"com.unity.animation.rigging": "1.1.1",
"com.unity.cinemachine": "2.8.9",

```
> To import the packages from **Package manager**
1. Open package manager and select **Unity registry** under Packages.
2. Search for the following:
- Netcode for Gameobjects
- Animation rigging
- Cinemachine
- input System
3. Under **Edit**, go to **Project Settings** -> **Player** -> **Configurations**
4. Change "Input Manager(Old)" to "Both"

> Ensure all samples are imported.
### Collaborator
To become a collaborator, follow these steps:

1. Click the **Fork** button on the GitHub repository page.
2. Clone the forked repository to your local machine.
3. Make the desired changes.
4. Push the changes to your forked repository.
5. Submit a pull request after pushing the changes.


## Features 

### What this project contains

- Movement of the user using WASD
- Free look with camera rotation
- Multiplayer
- UI for easy spawning of Host/Client

### Upcoming features

This project is continuously being enhanced with new features. Here are the latest additions:

- Drone camera
- Waypoint placing for car AI

## Walkthrough

For the time being, I shall key in major updates that are pushed. Each update will be tagged as Update1, Update2, etc.

### 10/4/2023

- Improved UI :scream_cat:
    - Added player count
    - Added game debug console
    - Holder for Network Buttons
- Working Multiplayer :100:
    - Stress tested with 5 players
    - Animations synced
    - Player count synchronized

### 1/5/2023

- Change of environment :scream_cat:
    - Larger world asset
- New Player Features
    - Scroll to zoom feature
    - First-person camera
    - Name update for clients
    - Password Authentication
- Footstep audio for walking and running

### 5/6/2023

- New car feature
    - Driving with WASD input
    - Drifting
- Gun picking up animation
    - Auto trigger zones to shoot
- Animation rigging

### 29/9/23
- Working blockchain authentication
- Persistent local and online database
- Addition of new features
     - Messaging system
     - Automated car waypoint system
     - Different prefab spawn
     - Racing game to test functionality

---

*Note: This README.md file was last updated on 29/9/2023.*
