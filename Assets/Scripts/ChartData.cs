using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public enum ChartDifficulty
{
    Easy,
    Normal,
    Hard
}

[System.Serializable]
public struct NoteData
{
    public float spawnTime;   // When the note should be instantiated (startTime - travelTime)
    public float hitTime;     // When the note should be hit (from .osu)
    public float duration;    // Slider duration in ms (0 for circles)
    public int lane;          // 0-3
    public GameColor color;   // Color state (only used for Color type notes)
    public NoteType noteType; // Type of note: Color, DirectionalLeft, or DirectionalRight
}

[CreateAssetMenu(fileName = "NewChart", menuName = "RhythmGame/ChartData")]
public class ChartData : ScriptableObject
{
    public string songName;
    public ChartDifficulty difficulty = ChartDifficulty.Normal;
    public float bpm;
    public float offset;
    public List<NoteData> notes = new List<NoteData>();
    public AudioClip audioClip;

    [Header("Chapter System")]
    [Tooltip("章节索引: 0=第1章, 1=第2章, 2=第3章")]
    public int chapterIndex = 0;

    [Tooltip("是否为Boss关卡（高风险高回报）")]
    public bool isBossStage = false;
}
