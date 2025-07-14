# LonghornCase

A small Unity demo where you pick up a cup, fill it with water, water a plant, then throw the cup away—powered by a clear state machine, DOTween animations, and a ScriptableObject-based event system.

## Features

- Step-by-step click-driven game flow (state machine)  
- Smooth DOTween animations (hover, float, pour, door open, etc.)  
- ScriptableObject “GameEvent” system—zero runtime allocations  
- Pooled audio sources and click effects  
- In-scene UI: Main Menu, Pause Menu, and Level Completed panel  
- Modular `IClickable` interface for clickable objects  

## Requirements

- Unity 2020.3 LTS or later  
- [DOTween](http://dotween.demigiant.com/) (free version)  
- TextMeshPro (via Package Manager)  
- C# 8.0+ compatible IDE (Rider, Visual Studio, etc.)

## Gameplay Video 
https://youtu.be/fW5mIlb0pDo


## Installation

1. Clone this repository:
   ```bash
   git clone (https://github.com/Lethrinus/LonghornCase/)
