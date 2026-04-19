Overview

Seagull VR Simulator is a virtual reality experience developed with Unity for the Meta Quest 3. The player takes on the role of a seagull flying above an island, with the ability to explore the environment, interact with elements, and hunt fish.

This project was developed as part of an academic work in virtual reality, with a focus on immersion, intuitive interaction, and fluid movement.

Concept

The objective of the experience is to recreate the sensations of flight and to simulate the behavior of a seagull in a dynamic environment. The player can freely navigate the space, gain speed, dive, and catch fish.

Particular attention was given to making the controls feel natural and responsive in VR.

Features
Flight system

A custom flight system was implemented to provide smooth and immersive movement:

Movement based on the position of the headset and controllers
Speed variation depending on the player’s posture
Progressive acceleration and deceleration
Dash mechanic triggered when the hands are brought close to the body
Dive mechanic activated when looking downward to increase speed
Fish spawning system

A procedural spawning system generates fish dynamically in the environment:

Randomized spawn positions within defined areas
Adjustable spawn frequency and zones
Fish movement and jumping behavior
Prevention of spawning in invalid areas using layer filtering
Interaction system

The project uses the XR Interaction Toolkit for VR interactions:

Ray-based interaction for UI
Hand-based interaction system
Integration of VR controllers for navigation and input
Game systems

Several gameplay systems were implemented:

Score system based on captured fish
Game states management (gameplay, tutorial, end menu)
VR-adapted user interface
Immersion and feedback
First-person perspective linked to the player’s head
Work on movement sensations and verticality
Emphasis on game feel to reinforce immersion
Technical details
Engine: Unity
Language: C#
VR SDK: OpenXR
Toolkit: XR Interaction Toolkit
Platform: Meta Quest 3
Challenges

The main challenges of this project were:

Designing a flight system that feels natural in VR
Managing player speed and movement without causing discomfort
Creating a dynamic spawning system that avoids invalid areas
Ensuring consistent interaction with UI elements in VR
Possible improvements
Add sound design (wind, environment, feedback)
Improve fish behavior and AI
Introduce additional interactions with the environment
Add haptic feedback
Extend the experience with new gameplay mechanics
