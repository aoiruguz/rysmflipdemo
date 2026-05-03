# Achievement Score System Implementation

## Overview
Implemented a comprehensive achievement score system that displays real-time score in the top-right corner during gameplay.

## Components Created/Modified

### 1. PlayDataCollector.cs
**Location**: `Assets/Scripts/PlayDataCollector.cs`

**Key Features**:
- Singleton pattern for global access
- Real-time data collection from ScoreManager
- Achievement score calculation (max 1,000,000)
- UI update system

**New Fields**:
```csharp
[Header("UI References")]
public TextMeshProUGUI achievementScoreText;  // Reference to UI text component
```

**Key Methods**:
- `UpdateAchievementScoreUI()`: Updates the UI every frame with current score
- `GetCurrentData()`: Fetches latest data from ScoreManager
- Achievement score is calculated in `PlayData.GetAchievementScore()`

### 2. PlayData Class
**Location**: Inside `PlayDataCollector.cs`

**Achievement Score Formula**:
```
Total Score = Judgment Score (90%) + Combo Score (10%)

Judgment Score:
- Perfect: 100% weight
- Great: 80% weight
- Good: 50% weight
- Miss: 0% weight

Combo Score:
- MaxCombo / TotalNotes × 100,000

Maximum Score: 1,000,000
```

**Example Calculation**:
- Total Notes: 100
- Perfect: 80, Great: 15, Good: 5, Miss: 0
- Max Combo: 95

```
Judgment Score = (80×1.0 + 15×0.8 + 5×0.5) / 100 = 0.945
Judgment Points = 0.945 × 900,000 = 850,500

Combo Score = 95 / 100 = 0.95
Combo Points = 0.95 × 100,000 = 95,000

Total = 850,500 + 95,000 = 945,500
```

## Unity Setup Instructions

### Step 1: Create Achievement Score UI
1. Open the **PlayScene** in Unity
2. In the Hierarchy, right-click on the Canvas and select **UI > Text - TextMeshPro**
3. Rename it to **"AchievementScoreText"**

### Step 2: Position the UI (Top-Right Corner)
1. Select **AchievementScoreText** in the Hierarchy
2. In the **Rect Transform** component:
   - Set **Anchor Preset** to **Top-Right** (hold Alt+Shift and click top-right preset)
   - Set **Pos X**: `-20` (20 pixels from right edge)
   - Set **Pos Y**: `-20` (20 pixels from top edge)
   - Set **Width**: `200`
   - Set **Height**: `50`

### Step 3: Configure Text Appearance
1. In the **TextMeshPro - Text (UI)** component:
   - **Text**: `0000000` (placeholder)
   - **Font Size**: `36` (or adjust to preference)
   - **Alignment**: Right-aligned, Middle-aligned
   - **Color**: White or any color that contrasts with background
   - **Font Style**: Bold (optional)

### Step 4: Add PlayDataCollector Component
1. In the Hierarchy, find or create an empty GameObject named **"PlayDataCollector"**
2. Add the **PlayDataCollector** component to it
3. In the Inspector, drag **AchievementScoreText** into the **Achievement Score Text** field

### Step 5: Verify Setup
1. Make sure **ScoreManager** exists in the scene
2. Make sure **SongSelectionManager** has a selected chart
3. Play the scene and verify:
   - Achievement score displays as `0000000` at start
   - Score updates in real-time as you hit notes
   - Score format is 7 digits with leading zeros (e.g., `0945500`)

## Testing Checklist

- [ ] UI text appears in top-right corner
- [ ] Score starts at `0000000`
- [ ] Score increases when hitting Perfect notes
- [ ] Score increases less for Great/Good notes
- [ ] Score doesn't increase for Miss
- [ ] Score updates smoothly in real-time
- [ ] Score format is always 7 digits
- [ ] Maximum score is `1000000`

## Troubleshooting

### Issue: Score shows as 0000000 throughout gameplay
**Solution**: 
- Check that PlayDataCollector's `achievementScoreText` field is assigned
- Verify ScoreManager exists and is working
- Check that `totalNotes` is set correctly (should match chart.notes.Count)

### Issue: UI text not visible
**Solution**:
- Check Canvas render mode (should be Screen Space - Overlay)
- Verify text color contrasts with background
- Check that TextMeshPro font asset is assigned

### Issue: Score calculation seems wrong
**Solution**:
- Verify judgment thresholds in ScoreManager:
  - Perfect: ≤50ms
  - Great: ≤100ms
  - Good: ≤150ms
- Check that ScoreManager is counting judgments correctly
- Use Debug.Log to print intermediate calculation values

## Integration with Existing Systems

### ScoreManager Integration
PlayDataCollector reads data from ScoreManager every frame:
- `GetPerfectCount()`
- `GetGreatCount()`
- `GetGoodCount()`
- `GetMissCount()`
- `GetMaxCombo()`

### SongSelectionManager Integration
On Start(), PlayDataCollector gets the selected chart to determine `totalNotes`:
```csharp
ChartData chart = SongSelectionManager.GetSelectedChart();
if (chart != null)
{
    CurrentPlayData.totalNotes = chart.notes.Count;
}
```

## Future Enhancements (Optional)

1. **Score Animation**: Smooth number transitions instead of instant updates
2. **Rank Display**: Show S/A/B/C/D rank alongside score
3. **Score Breakdown**: Show judgment score and combo score separately
4. **Score History**: Save high scores per chart
5. **Visual Effects**: Particle effects or color changes at score milestones

## Files Modified Summary

| File | Status | Changes |
|------|--------|---------|
| `PlayDataCollector.cs` | Modified | Added UI reference field and UpdateAchievementScoreUI() method |
| `PlayData` class | Modified | Added GetAchievementScore() calculation method |

## Notes

- The achievement score updates every frame in `Update()`, ensuring real-time display
- The score is clamped between 0 and 1,000,000
- The 7-digit format (`D7`) ensures consistent display width
- The system is designed to work seamlessly with the existing judgment and combo systems
