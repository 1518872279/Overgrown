# Ivy Placement System Guide

## Overview
The Ivy Placement System allows you to control exactly where ivy grows in your scene. You can now choose between automatic random placement or precise manual placement of ivy roots.

## Setup Instructions

1. Select your GrowthManager object in the scene hierarchy
2. In the Inspector, find the "Placement Mode" dropdown and select either:
   - **Automatic**: Randomly generates ivy roots around the center (original behavior)
   - **Manual**: Places ivy roots at specific positions you define

## Manual Placement Mode

When "Manual" mode is selected, you'll see new options in the Inspector:

### Settings
- **Orient Toward Center**: When enabled, ivy will grow toward the center object
- **Keep Placeholders**: When enabled, placeholder objects remain in the scene after play mode

### Placeholder Management
- **Add New Placeholder**: Creates a new ivy root position marker
- **Select All Placeholders**: Selects all existing placeholders for group editing
- **Arrange in Circle**: Automatically positions all placeholders in a circle
- **Orient All Toward Center**: Sets all placeholders to point toward the center

## Workflow Tips

1. Switch to Manual placement mode
2. Add placeholders using the "Add New Placeholder" button
3. Position each placeholder where you want ivy to grow from
4. If "Orient Toward Center" is disabled, rotate the placeholders to control growth direction
5. Use the arrangement tools to quickly set up common patterns
6. Enter Play mode to see your ivy grow from the specified positions

## Gizmo Visualization

In the Scene view, you'll see:
- Green spheres showing each ivy root position
- Yellow arrows showing the direction ivy will grow

## Advanced Usage

- You can manually add any Transform to the "Root Placeholders" list
- Combine with other scene objects to create interesting growth patterns
- For precise control, disable "Orient Toward Center" and manually rotate each placeholder 