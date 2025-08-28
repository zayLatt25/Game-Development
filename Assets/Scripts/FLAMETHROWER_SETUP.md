# Flamethrower Weapon Setup Guide

## Overview
The flamethrower weapon has been added to your Unity game! It's a continuous-fire weapon that deals damage over time in a cone-shaped area.

## What Was Added

### 1. FlamethrowerWeapon.cs
- Main script for the flamethrower weapon
- Handles continuous flame damage in a cone pattern
- Includes visual effects and audio
- Uses raycasting for damage detection

### 2. FlameEffect.cs
- Visual flame effect script
- Handles flickering, scaling, and fade-out effects
- Can be attached to flame prefabs

### 3. Updated Player.cs
- Added WeaponType enum (Firearm, Melee, Flamethrower)
- Modified weapon handling to support flamethrower
- Added flamethrower-specific firing logic

### 4. Updated DroppableItem.cs
- Added Flamethrower = 5 to the Type enum

## Setup Instructions

### Step 1: Create Flamethrower Prefab
1. Create a new GameObject in your scene
2. Add a SpriteRenderer component (use a flamethrower sprite)
3. Add the FlamethrowerWeapon script
4. Configure the settings:
   - Damage Per Tick: 2 (damage per damage tick)
   - Damage Tick Rate: 0.1 (how often damage is applied)
   - Range: 3 (flame range)
   - Spread Angle: 30 (cone spread in degrees)
   - Target Layers: Set to include zombies/enemies

### Step 2: Create Flame Effect Prefab
1. Create a new GameObject
2. Add a SpriteRenderer with a flame sprite
3. Add the FlameEffect script
4. Optionally add a ParticleSystem for extra effects
5. Configure the visual settings

### Step 3: Configure Player Weapon Array
1. In the Player GameObject, find the Weapons array
2. Set element 4 (index 4) to include:
   - IsFirearm: false
   - Type: Flamethrower
   - GunRenderer: Reference to your flamethrower prefab
   - FireRate: 10 (or adjust as needed)
   - Sfx: Flamethrower sound effect

### Step 4: Add Flamethrower to Droppable Items
1. Create a new DroppableItem GameObject
2. Set ItemType to Flamethrower
3. Place it in your scene for players to pick up

## How It Works

### Firing Mechanism
- Hold left mouse button to start flamethrower
- Release to stop
- Continuous damage is applied to enemies in the cone
- Uses raycasting for accurate hit detection

### Damage System
- Damage is applied every tick (configurable)
- Multiple rays are cast in a cone pattern
- Each ray can hit different enemies
- Perfect for crowd control

### Visual Effects
- Flame effect prefab spawns when firing
- Flickering and scaling effects
- Fade-out animation
- Particle system support

## Customization Options

### Damage Settings
- `_damagePerTick`: Damage per damage tick
- `_damageTickRate`: How often damage is applied
- `_range`: Maximum flame range
- `_spreadAngle`: Width of the flame cone

### Visual Settings
- `_flameEffectPrefab`: The flame visual effect
- `_muzzlePoint`: Where flames spawn from
- `_flameSound`: Continuous flame sound
- `_igniteSound`: Sound when starting

### Flame Effect Settings
- `_lifetime`: How long flames last
- `_fadeOutDuration`: Fade-out animation time
- `_scaleRange`: Random scale variation
- `_flickerSpeed`: Flame flicker rate
- `_flickerIntensity`: How much flames flicker

## Tips for Best Results

1. **Audio**: Use looped flame sounds for continuous firing
2. **Particles**: Add particle systems for smoke and embers
3. **Sprites**: Use flame sprites with transparency
4. **Performance**: Adjust ray count based on performance needs
5. **Balance**: Tune damage and range for game balance

## Troubleshooting

### Flamethrower Not Working
- Check that the FlamethrowerWeapon script is attached
- Verify the weapon is set to index 4 in the Player's weapons array
- Ensure the weapon type is set to Flamethrower

### No Visual Effects
- Check that the flame effect prefab is assigned
- Verify the FlameEffect script is attached to the prefab
- Ensure the sprite renderer is properly configured

### No Damage
- Check the target layers in the FlamethrowerWeapon
- Verify zombies have the Zombie component
- Check that the damage values are reasonable

## Example Weapon Configuration

```
Weapon Index 4 (Flamethrower):
- IsFirearm: false
- Type: Flamethrower
- GunRenderer: Flamethrower GameObject
- FireRate: 10
- Sfx: flamethrower_loop.wav
```

The flamethrower is now fully integrated into your weapon system! Players can pick it up and use it to burn through hordes of zombies with continuous flame damage.
