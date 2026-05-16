using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// 按钮音效批量添加工具
/// 可以为场景中所有按钮批量添加ButtonSoundEffect组件
/// </summary>
public class ButtonSoundEffectEditor : EditorWindow
{
    private string clickSoundName = "ButtonClick";
    private AudioClip clickSoundClip;
    private string hoverSoundName = "";
    private AudioClip hoverSoundClip;
    private float clickVolumeScale = 1f;
    private float hoverVolumeScale = 0.5f;
    private bool autoAddListener = true;

    [MenuItem("Tools/Audio/批量添加按钮音效")]
    public static void ShowWindow()
    {
        GetWindow<ButtonSoundEffectEditor>("按钮音效批量工具");
    }

    private void OnGUI()
    {
        GUILayout.Label("按钮音效批量添加工具", EditorStyles.boldLabel);
        GUILayout.Space(10);

        EditorGUILayout.HelpBox("此工具会为场景中所有Button添加ButtonSoundEffect组件", MessageType.Info);
        GUILayout.Space(10);

        // 音效设置
        GUILayout.Label("音效设置", EditorStyles.boldLabel);
        clickSoundName = EditorGUILayout.TextField("点击音效名称", clickSoundName);
        clickSoundClip = (AudioClip)EditorGUILayout.ObjectField("点击音效文件", clickSoundClip, typeof(AudioClip), false);

        GUILayout.Space(5);
        hoverSoundName = EditorGUILayout.TextField("悬停音效名称", hoverSoundName);
        hoverSoundClip = (AudioClip)EditorGUILayout.ObjectField("悬停音效文件", hoverSoundClip, typeof(AudioClip), false);

        GUILayout.Space(10);

        // 音量设置
        GUILayout.Label("音量设置", EditorStyles.boldLabel);
        clickVolumeScale = EditorGUILayout.Slider("点击音量", clickVolumeScale, 0f, 1f);
        hoverVolumeScale = EditorGUILayout.Slider("悬停音量", hoverVolumeScale, 0f, 1f);

        GUILayout.Space(10);

        // 其他设置
        GUILayout.Label("其他设置", EditorStyles.boldLabel);
        autoAddListener = EditorGUILayout.Toggle("自动添加监听", autoAddListener);

        GUILayout.Space(20);

        // 操作按钮
        if (GUILayout.Button("为所有按钮添加音效组件", GUILayout.Height(30)))
        {
            AddSoundEffectToAllButtons();
        }

        GUILayout.Space(5);

        if (GUILayout.Button("移除所有按钮音效组件", GUILayout.Height(30)))
        {
            RemoveSoundEffectFromAllButtons();
        }

        GUILayout.Space(10);

        EditorGUILayout.HelpBox("提示：\n1. 点击音效名称和文件二选一即可\n2. 悬停音效可选，留空则不播放", MessageType.None);
    }

    private void AddSoundEffectToAllButtons()
    {
        Button[] buttons = FindObjectsOfType<Button>(true);
        int addedCount = 0;
        int skippedCount = 0;

        foreach (Button button in buttons)
        {
            // 检查是否已经有ButtonSoundEffect组件
            ButtonSoundEffect existing = button.GetComponent<ButtonSoundEffect>();
            if (existing != null)
            {
                skippedCount++;
                continue;
            }

            // 添加组件
            ButtonSoundEffect soundEffect = button.gameObject.AddComponent<ButtonSoundEffect>();
            soundEffect.clickSoundName = clickSoundName;
            soundEffect.clickSoundClip = clickSoundClip;
            soundEffect.hoverSoundName = hoverSoundName;
            soundEffect.hoverSoundClip = hoverSoundClip;
            soundEffect.clickVolumeScale = clickVolumeScale;
            soundEffect.hoverVolumeScale = hoverVolumeScale;
            soundEffect.autoAddListener = autoAddListener;

            EditorUtility.SetDirty(button.gameObject);
            addedCount++;
        }

        Debug.Log($"[ButtonSoundEffectEditor] 添加完成！新增: {addedCount}, 跳过: {skippedCount}");
        EditorUtility.DisplayDialog("完成", $"成功为 {addedCount} 个按钮添加音效组件\n跳过 {skippedCount} 个已有组件的按钮", "确定");
    }

    private void RemoveSoundEffectFromAllButtons()
    {
        if (!EditorUtility.DisplayDialog("确认", "确定要移除所有按钮的音效组件吗？", "确定", "取消"))
        {
            return;
        }

        ButtonSoundEffect[] soundEffects = FindObjectsOfType<ButtonSoundEffect>(true);
        int removedCount = soundEffects.Length;

        foreach (ButtonSoundEffect soundEffect in soundEffects)
        {
            DestroyImmediate(soundEffect);
        }

        Debug.Log($"[ButtonSoundEffectEditor] 移除完成！共移除 {removedCount} 个组件");
        EditorUtility.DisplayDialog("完成", $"成功移除 {removedCount} 个音效组件", "确定");
    }
}
