using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// JSON可序列化的谱面数据结构
/// 用于保存和加载外部谱面文件
/// </summary>
[Serializable]
public class ChartDataJson
{
    public string songName;
    public string difficulty; // "Easy", "Normal", "Hard"
    public float bpm;
    public float offset;
    public List<NoteDataJson> notes = new List<NoteDataJson>();
    public string audioFileName; // 音频文件名（相对路径）
    public int chapterIndex = 0;
    public bool isBossStage = false;

    /// <summary>
    /// 从ChartData转换为JSON格式
    /// </summary>
    public static ChartDataJson FromChartData(ChartData chartData)
    {
        ChartDataJson json = new ChartDataJson
        {
            songName = chartData.songName,
            difficulty = chartData.difficulty.ToString(),
            bpm = chartData.bpm,
            offset = chartData.offset,
            chapterIndex = chartData.chapterIndex,
            isBossStage = chartData.isBossStage,
            audioFileName = chartData.audioClip != null ? chartData.audioClip.name : ""
        };

        foreach (var note in chartData.notes)
        {
            json.notes.Add(NoteDataJson.FromNoteData(note));
        }

        return json;
    }

    /// <summary>
    /// 转换为ChartData（不包含AudioClip，需要单独加载）
    /// </summary>
    public ChartData ToChartData()
    {
        ChartData chartData = ScriptableObject.CreateInstance<ChartData>();
        chartData.songName = songName;

        // 解析难度
        if (Enum.TryParse(difficulty, out ChartDifficulty diff))
        {
            chartData.difficulty = diff;
        }

        chartData.bpm = bpm;
        chartData.offset = offset;
        chartData.chapterIndex = chapterIndex;
        chartData.isBossStage = isBossStage;

        foreach (var noteJson in notes)
        {
            chartData.notes.Add(noteJson.ToNoteData());
        }

        return chartData;
    }
}

/// <summary>
/// JSON可序列化的Note数据
/// </summary>
[Serializable]
public class NoteDataJson
{
    public float spawnTime;
    public float hitTime;
    public float duration;
    public int lane;
    public string color; // "J_Color0_Red", "L_Color1_Yellow", "K_Color2_Blue" (Legacy: "ColorA", etc.)
    public string noteType; // "Color", "DirectionalLeft", "DirectionalRight"

    public static NoteDataJson FromNoteData(NoteData noteData)
    {
        return new NoteDataJson
        {
            spawnTime = noteData.spawnTime,
            hitTime = noteData.hitTime,
            duration = noteData.duration,
            lane = noteData.lane,
            color = noteData.color.ToString(),
            noteType = noteData.noteType.ToString()
        };
    }

    public NoteData ToNoteData()
    {
        NoteData noteData = new NoteData
        {
            spawnTime = spawnTime,
            hitTime = hitTime,
            duration = duration,
            lane = lane
        };

        // 解析颜色
        if (Enum.TryParse(color, out GameColor gameColor))
        {
            noteData.color = gameColor;
        }
        else if (color == "ColorA") noteData.color = GameColor.J_Color0_Red;
        else if (color == "ColorB") noteData.color = GameColor.L_Color1_Yellow;
        else if (color == "ColorC") noteData.color = GameColor.K_Color2_Blue;

        // 解析类型
        if (Enum.TryParse(noteType, out NoteType type))
        {
            noteData.noteType = type;
        }

        return noteData;
    }
}
