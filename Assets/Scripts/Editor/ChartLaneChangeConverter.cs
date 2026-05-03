using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// 自动将谱面中换轨道的第一个note转换为方向note
/// 在Unity编辑器中选择ChartData资源，右键选择 "Convert Lane Changes to Directional Notes"
/// </summary>
public class ChartLaneChangeConverter : MonoBehaviour
{
    [MenuItem("Assets/Convert Lane Changes to Directional Notes")]
    static void ConvertSelectedCharts()
    {
        Object[] selectedObjects = Selection.objects;
        int convertedCount = 0;

        foreach (Object obj in selectedObjects)
        {
            ChartData chart = obj as ChartData;
            if (chart != null)
            {
                if (ConvertChartLaneChanges(chart))
                {
                    convertedCount++;
                    EditorUtility.SetDirty(chart);
                    Debug.Log($"[Converter] Converted chart: {chart.songName}");
                }
            }
        }

        if (convertedCount > 0)
        {
            AssetDatabase.SaveAssets();
            Debug.Log($"=== Conversion Complete! Converted {convertedCount} charts ===");
        }
        else
        {
            Debug.LogWarning("No ChartData assets selected!");
        }
    }

    [MenuItem("Assets/Convert Lane Changes to Directional Notes", true)]
    static bool ValidateConvertSelectedCharts()
    {
        // Only show menu item if at least one ChartData is selected
        foreach (Object obj in Selection.objects)
        {
            if (obj is ChartData)
            {
                return true;
            }
        }
        return false;
    }

    static bool ConvertChartLaneChanges(ChartData chart)
    {
        if (chart == null || chart.notes == null || chart.notes.Count == 0)
        {
            return false;
        }

        int previousLane = -1;
        int changesCount = 0;
        List<NoteData> updatedNotes = new List<NoteData>();

        foreach (NoteData note in chart.notes)
        {
            NoteData updatedNote = note;

            // 如果是三色note且发生了换轨道
            if (note.noteType == NoteType.Color && previousLane != -1 && note.lane != previousLane)
            {
                // 根据换轨道方向决定方向note类型，并将note保持在原轨道
                if (note.lane < previousLane)
                {
                    updatedNote.noteType = NoteType.DirectionalLeft;
                    updatedNote.lane = previousLane; // 保持在原轨道（换轨道前的轨道）
                    changesCount++;
                    Debug.Log($"  Lane change {previousLane} -> {note.lane}: Changed to DirectionalLeft, staying at lane {previousLane} at {note.hitTime}ms");
                }
                else if (note.lane > previousLane)
                {
                    updatedNote.noteType = NoteType.DirectionalRight;
                    updatedNote.lane = previousLane; // 保持在原轨道（换轨道前的轨道）
                    changesCount++;
                    Debug.Log($"  Lane change {previousLane} -> {note.lane}: Changed to DirectionalRight, staying at lane {previousLane} at {note.hitTime}ms");
                }

                // 更新previousLane为目标轨道，这样后续同轨道的note不会被转换
                previousLane = note.lane;
            }
            else
            {
                // 没有换轨道，正常更新previousLane
                previousLane = updatedNote.lane;
            }

            updatedNotes.Add(updatedNote);
        }

        if (changesCount > 0)
        {
            chart.notes = updatedNotes;
            Debug.Log($"  Total lane changes converted: {changesCount}");
            return true;
        }

        return false;
    }

    [MenuItem("Tools/Convert All Charts to Directional Notes")]
    static void ConvertAllCharts()
    {
        string[] guids = AssetDatabase.FindAssets("t:ChartData");
        int convertedCount = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ChartData chart = AssetDatabase.LoadAssetAtPath<ChartData>(path);

            if (chart != null && ConvertChartLaneChanges(chart))
            {
                convertedCount++;
                EditorUtility.SetDirty(chart);
            }
        }

        if (convertedCount > 0)
        {
            AssetDatabase.SaveAssets();
            Debug.Log($"=== Conversion Complete! Converted {convertedCount} charts ===");
        }
        else
        {
            Debug.Log("No charts needed conversion.");
        }
    }
}
