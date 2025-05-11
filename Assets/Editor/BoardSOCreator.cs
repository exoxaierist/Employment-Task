using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Project.Scripts.Data_Script;

public class BoardSOCreator : EditorWindow
{
    private const string SAVE_PATH_KEY = "BoardSOCreator_SavePath";
    
    private TextAsset jsonFile; // JSON 파일 드래그 앤 드롭용
    private string savePath; // 생성할 ScriptableObject 저장 경로

    [MenuItem("Tools/Stage Data Creator")]
    public static void ShowWindow()
    {
        GetWindow<BoardSOCreator>("Stage Data Creator");
    }

    private void OnEnable()
    {
        // 이전에 저장된 경로 불러오기
        savePath = EditorPrefs.GetString(SAVE_PATH_KEY, "Assets/StageData.asset");
    }

    private void OnGUI()
    {
        GUILayout.Label("JSON to StageData Converter", EditorStyles.boldLabel);

        // JSON 파일 드래그 앤 드롭
        jsonFile = EditorGUILayout.ObjectField("JSON File", jsonFile, typeof(TextAsset), false) as TextAsset;

        // 저장 경로 드래그 앤 드롭
        savePath = EditorGUILayout.TextField("Save Path", savePath);
        
        // 경로 선택 버튼 추가
        if (GUILayout.Button("Browse Save Path"))
        {
            string selectedPath = EditorUtility.SaveFilePanelInProject(
                "Save StageData", 
                "StageData.asset", 
                "asset", 
                "Choose a location to save the StageData"
            );

            if (!string.IsNullOrEmpty(selectedPath))
            {
                savePath = selectedPath;
            }
        }

        // 생성 버튼 (JSON 파일이 있을 때만 활성화)
        GUI.enabled = jsonFile != null;
        if (GUILayout.Button("Generate Stage Data"))
        {
            CreateStageDataFromJson();
        }
        GUI.enabled = true;
    }

    private void CreateStageDataFromJson()
    {
        if (jsonFile == null)
        {
            Debug.LogError("JSON 파일을 선택해주세요.");
            return;
        }

        // TextAsset의 text 속성 직접 사용
        string jsonContent = jsonFile.text;

        StageJsonWrapper wrapper = JsonUtility.FromJson<StageJsonWrapper>(jsonContent);

        if (wrapper == null || wrapper.Stage == null)
        {
            Debug.LogError("JSON 파일의 형식이 올바르지 않거나 데이터가 없습니다.");
            return;
        }

        // 저장 경로 처리
        string directoryPath = Path.GetDirectoryName(savePath);
        string fileName = $"StageData_{wrapper.Stage.stageIndex}.asset";
        string fullPath = Path.Combine(directoryPath, fileName);

        // 이미 파일이 존재하면 삭제
        if (File.Exists(fullPath))
        {
            AssetDatabase.DeleteAsset(fullPath);
        }

        // StageJsonData 데이터를 ScriptableObject에 저장
        StageData stageData = CreateInstance<StageData>();
        stageData.stageIndex = wrapper.Stage.stageIndex;
        stageData.boardBlocks = wrapper.Stage.boardBlocks;
        stageData.playingBlocks = wrapper.Stage.playingBlocks;
        stageData.Walls = wrapper.Stage.Walls;

        // 경로 저장
        savePath = fullPath;
        EditorPrefs.SetString(SAVE_PATH_KEY, savePath);

        // ScriptableObject 저장
        AssetDatabase.CreateAsset(stageData, savePath);
        AssetDatabase.SaveAssets();

        Debug.Log($"StageData ScriptableObject 생성 완료: {savePath}");
    }
}