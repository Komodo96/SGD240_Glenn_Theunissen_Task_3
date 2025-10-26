# SGD240_Glenn_Theunissen_Task_3

# Procedural Terrain Generation System - README

## Overview

This project implements a **procedural terrain generation system** in Unity using Perlin noise and customizable parameters for scale, height, and regions. The system dynamically generates landmasses with unique features each time, providing infinite variation for gameplay environments.

## Key Features

* **Procedural Mesh Generation:** Uses Perlin noise to generate realistic terrain heightmaps.
* **Dynamic Level of Detail (LOD):** Reduces mesh complexity at distance for better performance.
* **Falloff Map Integration:** Smooths terrain edges to create island-like formations.
* **ScriptableObject-Based Configuration:** Modular data-driven design using `TerrainData`, `NoiseData`, and `TextureData` for easy tuning in the Unity Editor.
* **Auto-Update System:** Terrain updates in real-time when parameters are modified in the editor.

## Technical Implementation

* Implemented with **custom mesh generation** using vertex height modification and a blend of **height curves** and **multipliers**.
* Uses **C# events** to handle updates efficiently (`OnValuesUpdated`).
* Includes options for **flat shading** vs. **smooth shading** to vary the visual style.
* Designed for **runtime expansion**, allowing future systems like tree and rock spawning based on terrain height.

## Experimentation and Design Process

* Tested different **Perlin noise frequencies, amplitudes, and seed values** to produce natural variations.
* Experimented with **falloff strength and mesh height curves** to balance flat and mountainous regions.
* Planned future feature: **ProceduralObjectSpawner** that instantiates prefabs (trees, rocks, etc.) based on terrain height and biome rules.

## Future Improvements

* Implement runtime **object spawning** (trees, rocks, foliage) tied to terrain generation.
* Add **biome blending** for desert, grassland, and snow regions.
