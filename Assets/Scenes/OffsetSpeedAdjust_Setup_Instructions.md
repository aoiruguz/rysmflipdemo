# Offset Speed Adjust Scene Setup Instructions

## Overview
This scene allows you to adjust two critical parameters:
1. **Global Offset** - Timing offset in milliseconds (stored in GameSettings.GlobalOffset)
2. **Note Travel Time** - Time for notes to travel from spawn to judgment line (used in PlayScene)

The scene supports two modes:
- **Simple Mode** (default): Spawns notes at regular intervals
- **Chart Mode**: Loops a test chart with music, spawning notes based on chart timing (all in one lane)

## Automatic Setup

1. Open Unity Editor
2. Go to menu: **Tools > Setup Offset Speed Adjust Scene**
3. This will automatically create all UI elements and wire up the manager

## Manual Setup (if automatic setup fails)

### Step 1: Open the Scene
- Open `Assets/Scenes/Offest Speed Adjust.unity`

### Step 2: Run the Setup Script
- The Editor script `SetupOffsetSpeedAdjustScene.cs` will create:
  - Canvas with UI controls on the left (35% width)
  - Visualization area on the right (65% width)
  - Judgment line at Y = -4
  - Manager GameObject with OffsetSpeedAdjustManager component

### Step 3: Assign References in Inspector
Select the `OffsetSpeedAdjustManager` GameObject and assign:

**Required References:**
- `Note Prefab`: `Assets/Prefab/NotePrefab 1.prefab` (or NotePrefab 2)
- `Hit Sound`: `Assets/SFX/drum-hitnormal.wav`
- `Particle Material`: (Optional) Assign a particle material for better visual effects
- `Offset Value Text`: LeftPanel/OffsetGroup/OffsetValue
- `Offset Increase Button`: LeftPanel/OffsetGroup/OffsetIncreaseButton
- `Offset Decrease Button`: LeftPanel/OffsetGroup/OffsetDecreaseButton
- `Travel Time Value Text`: LeftPanel/TravelTimeGroup/TravelTimeValue
- `Travel Time Increase Button`: LeftPanel/TravelTimeGroup/TravelTimeIncreaseButton
- `Travel Time Decrease Button`: LeftPanel/TravelTimeGroup/TravelTimeDecreaseButton

**Settings (should be auto-configured):**
- `Note Travel Time`: 2.0 (default, matches PlayScene)
- `Global Offset`: 0.0 (will load from GameSettings)
- `Spawn Y`: 6.0
- `Judgment Line Y`: -4.0
- `Lane X`: 0.0 (center lane for demo)
- `Spawn Interval`: 2.0 (spawn a note every 2 seconds, only used in Simple Mode)
- `Offset Step`: 0.01 (10ms per click)
- `Travel Time Step`: 0.1 (0.1s per click)

**Chart Mode Settings (optional):**
- `Test Chart`: Assign a ChartData asset to enable chart mode
- `Use Chart Mode`: Check this to enable chart mode (loops the test chart with music)
- `Chart Loop Duration`: Duration in seconds before looping (default 22s)

## How It Works

### Left Panel (UI Controls)
- **Offset Controls**: Adjust global timing offset
  - Click `+` to increase offset by 10ms
  - Click `-` to decrease offset by 10ms
  - Changes are saved to `GameSettings.GlobalOffset` and persist across scenes

- **Travel Time Controls**: Adjust note fall speed
  - Click `+` to increase travel time by 0.1s (slower notes)
  - Click `-` to decrease travel time by 0.1s (faster notes)
  - Minimum value: 0.5s

### Right Panel (Visualization)
- Notes spawn at the top (Y = 6) and fall down
- Orange judgment line at Y = -4
- When a note reaches the judgment line:
  - **Note is destroyed immediately** (doesn't pass through)
  - **Hit sound plays** (drum-hitnormal.wav)
  - **Particle effect spawns** - bright yellow-orange burst that fades out
- The offset affects when the sound plays relative to the visual crossing

### Consistency with PlayScene
The parameters adjusted here directly match PlayScene:
- `noteTravelTime` → Used in `NoteManager.noteTravelTime`
- `globalOffset` → Used in `GameSettings.GlobalOffset` (applied in NoteManager line 161)

## Testing

### Simple Mode (Default)
1. Play the scene
2. Observe notes falling and hitting the judgment line every 2 seconds
3. Adjust offset and travel time using the buttons
4. Verify the values update in real-time

### Chart Mode (With Test Chart)
1. Assign a test ChartData asset to `Test Chart` field
2. Check `Use Chart Mode` checkbox
3. Set `Chart Loop Duration` to desired loop length (default 22 seconds)
4. Play the scene
5. The chart's music will play for the specified duration, then loop from the beginning
6. Only notes within the loop duration will spawn (notes after the loop point are skipped)
7. Notes spawn based on chart timing (all in center lane)
8. Notes auto-hit at the judgment line with sound and particle effects
9. Adjust offset to sync the hit sound with the music beat
10. Adjust travel time to change note fall speed

The adjusted values will be used in PlayScene automatically.

## Troubleshooting

**No notes spawning:**
- Check that `notePrefab` is assigned in the Inspector
- Check Console for errors

**No sound playing:**
- Check that `hitSound` is assigned
- Check that `audioSource` is created (auto-created in Start)
- Verify GameSettings.NoteVolume is not 0

**UI not visible:**
- Check that Canvas is set to Screen Space Overlay
- Check that Camera is positioned at (0, 0, -10)

**Notes not hitting judgment line correctly:**
- Verify `spawnY` = 6 and `judgmentLineY` = -4
- Check that speed calculation is correct (distance / travelTime)

**No particle effects visible:**
- Particle effects are created automatically when notes hit the judgment line
- Check Console for any particle system errors
- Optionally assign a particle material in the Inspector for better visuals
- Default uses "Particles/Standard Unlit" shader

**Chart mode not working:**
- Ensure `Test Chart` is assigned in the Inspector
- Ensure `Use Chart Mode` checkbox is checked
- Check that the ChartData has valid audioClip and notes
- Check Console for "[OffsetSpeedAdjust]" log messages

**Music not looping:**
- Music Source has `loop` enabled automatically
- Check that the ChartData's audioClip is assigned correctly
