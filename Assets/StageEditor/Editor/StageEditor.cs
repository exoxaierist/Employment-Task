using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public class StageEditor : EditorWindow
{
    private enum EditType
    {
        Select,
        SetBoard,
        SetBlocks,
        SetGoal,
    }

    //editor
    private StageData selectedObject;
    private Vector2 scrollPosition;
    private TileContainer selectedTile = null;
    private Rect viewportRect;

    //save
    private string savePath = "Assets/SerializedStageData";

    //brush
    private EditType editType;
    private ColorType colorType;
    private Direction goalDirection;

    //viewport transform
    private float cellSize = 50f;
    private int gridCount = 10;
    private Vector2 panOffset;
    private float zoom = 1f;
    private Vector2 mousePosition;

    //lookups
    private Texture2D boardTexture;
    private Dictionary<ColorType, Texture2D> colorMap = new();

    [MenuItem("Assets/Open Stage Editor", false, 0)]
    private static void OpenFromContextMenu()
    {
        StageData asset = Selection.activeObject as StageData;
        if (asset != null)
        {
            StageEditor window = GetWindow<StageEditor>("Stage Editor");
            window.selectedObject = asset;
        }
    }

    [MenuItem("Assets/Open Stage Editor", true)]
    private static bool ValidateOpenFromContextMenu()
    {
        return Selection.activeObject is StageData;
    }

    private void OnFocus()
    {
        selectedTile = null;
        boardTexture = MakeTexture(2, 2, new(0.1f, 0.1f, 0.1f));
        MakeColormap();
    }

    private void OnGUI()
    {
        GUILayout.BeginHorizontal();

        DrawDetailsPanel();
        DrawViewport();

        GUILayout.EndHorizontal();
        HandleInput();
    }

    private void OnEnable()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChange;
    }

    private const string prevScenePathKey = "STAGE_EDITOR_PREV_SCENE";
    private void PlayTestScene()
    {
        if (selectedObject == null) return;
        const string scenePath = "Assets/StageEditor/TestScene.unity";
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath) != null)
        {
            EditorPrefs.SetString(prevScenePathKey, EditorSceneManager.GetActiveScene().path);
            EditorSceneManager.sceneOpened += OnPlayTestSceneOpened;
            EditorSceneManager.OpenScene(scenePath);
        }
    }

    private void OnPlayTestSceneOpened(UnityEngine.SceneManagement.Scene scene, OpenSceneMode mode)
    {
        EditorSceneManager.sceneOpened -= OnPlayTestSceneOpened;

        StageLoader loader = GameObject.Find("StageLoader").GetComponent<StageLoader>();
        if (loader == null)
        {
            Debug.Log("StageLoader not located");
        }
        loader.SetStage(selectedObject);

        EditorApplication.EnterPlaymode();
    }

    private void OnPlayModeStateChange(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredEditMode && EditorPrefs.HasKey(prevScenePathKey))
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChange;

            EditorSceneManager.OpenScene(EditorPrefs.GetString(prevScenePathKey));
            EditorPrefs.DeleteKey(prevScenePathKey);
        }
    }

    private void SaveSerialized()
    {
        if (selectedObject == null) return;

        string path = savePath;
        if (!path.ToLower().EndsWith(".json")) path += ".json";
        File.WriteAllText(path, selectedObject.SerializeToJson());
        AssetDatabase.Refresh();
    }

    private void DrawDetailsPanel()
    {
        GUILayout.BeginVertical(GUILayout.Width(position.width * 0.3f));
        GUILayout.Label("Editor Settings", EditorStyles.boldLabel);

        scrollPosition = GUILayout.BeginScrollView(scrollPosition);

        selectedObject = (StageData)EditorGUILayout.ObjectField(
            "Stage Data",
            selectedObject,
            typeof(StageData),
            false
        );

        if (selectedObject == null)
        {
            GUILayout.Label("No Stage Data Asset Selected");
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            return;
        }

        savePath = EditorGUILayout.TextField("Save Path", savePath);
        if (GUILayout.Button("Save To Json"))
        {
            SaveSerialized();
        }

        EditorGUILayout.Separator();

        if (GUILayout.Button("Play Test") && !EditorApplication.isPlaying)
        {
            PlayTestScene();
        }

        EditorGUILayout.Separator();

        gridCount = EditorGUILayout.IntSlider("Grid Count", gridCount, 4, 20);

        EditorGUILayout.Separator();
        GUILayout.Label("Brush Settings");
        editType = (EditType)EditorGUILayout.EnumPopup("Edit Type", editType);
        switch (editType)
        {
            case EditType.Select:
                break;
            case EditType.SetBoard:
                break;
            case EditType.SetBlocks:
                colorType = (ColorType)EditorGUILayout.EnumPopup("Block Color", colorType);
                break;
            case EditType.SetGoal:
                colorType = (ColorType)EditorGUILayout.EnumPopup("Goal Color", colorType);
                if (colorType != ColorType.None) goalDirection = (Direction)EditorGUILayout.EnumPopup("Direction", goalDirection);
                break;
        }

        EditorGUILayout.Separator();
        GUILayout.Label("Tile Details");
        if (selectedTile != null)
        {
            GUILayout.Label($"X:{selectedTile.x} Y:{selectedTile.y}");
            selectedTile.flags = (TileContainer.TileFlags)EditorGUILayout.EnumFlagsField("Tile Flags", selectedTile.flags);
            selectedTile.colorType = (ColorType)EditorGUILayout.EnumPopup("Color Type", selectedTile.colorType);
            if (selectedTile.HasFlag(TileContainer.TileFlags.Gimmick))
            {
                selectedTile.gimmickType = (TileContainer.GimmickType)EditorGUILayout.EnumPopup("Gimmick", selectedTile.gimmickType);
            }

            if (selectedTile.goals.Count > 0)
            {
                GUILayout.Label("Goals (Read Only)");
                for (int i = 0; i < selectedTile.goals.Count; i++)
                {
                    var _ = EditorGUILayout.EnumPopup("Color Type", selectedTile.goals[i].colorType);
                    var _1 = EditorGUILayout.EnumPopup("Direction", selectedTile.goals[i].direction);
                    EditorGUILayout.Separator();
                }

                if (GUILayout.Button("Clear Goals"))
                {
                    selectedTile.goals.Clear();
                }
            }
        }
        else
        {
            GUILayout.Label("No Tile Selected");
        }

        GUILayout.EndScrollView();
        GUILayout.EndVertical();
    }

    private void DrawViewport()
    {
        GUILayout.BeginVertical();
        GUILayout.Label("Viewport", EditorStyles.boldLabel);

        if (selectedObject != null)
        {
            viewportRect = GUILayoutUtility.GetRect(position.width * 0.7f, position.height);
            GUI.Box(viewportRect, GUIContent.none);

            GUI.BeginGroup(viewportRect);
            Rect canvasRect = new Rect(0, 0, viewportRect.width, viewportRect.height);

            //grid background
            float gridWorldSize = gridCount * cellSize * zoom;
            Vector2 gridCenter = new Vector2(gridWorldSize / 2, gridWorldSize / 2);
            Vector2 gridOffset = (new Vector2(500, 500) + panOffset) - 0.5f * gridWorldSize * Vector2.one;
            Rect gridRect = new Rect(gridOffset.x, gridOffset.y, gridWorldSize, gridWorldSize);
            GUI.Box(gridRect, GUIContent.none, EditorStyles.helpBox);

            //tiles
            for (int x = Mathf.FloorToInt(-gridCount * 0.5f); x < Mathf.FloorToInt(gridCount * 0.5f); x++)
            {
                for (int y = Mathf.FloorToInt(-gridCount * 0.5f); y < Mathf.FloorToInt(gridCount * 0.5f); y++)
                {
                    Rect cellRect = new Rect(
                        x * cellSize * zoom + gridOffset.x + gridCenter.x + (gridCount % 2 != 0 ? 0.5f * cellSize * zoom : 0),
                        (-y - 1) * cellSize * zoom + gridOffset.y + gridCenter.y + (gridCount % 2 != 0 ? 0.5f * cellSize * zoom : 0),
                        cellSize * zoom,
                        cellSize * zoom
                    );

                    TileContainer tile = selectedObject.tiles.Find(t => t.x == x && t.y == y);

                    if (tile != null)
                    {
                        //draw board
                        if (tile.HasFlag(TileContainer.TileFlags.Board))
                        {
                            GUIStyle style = new GUIStyle(GUI.skin.box);
                            style.alignment = TextAnchor.MiddleCenter;
                            style.normal.textColor = Color.white;
                            style.normal.background = boardTexture;
                            style.fontSize = (int)(12 * zoom);
                            GUI.Box(cellRect, "", style);
                        }
                        //draw block
                        if (tile.HasFlag(TileContainer.TileFlags.Block))
                        {
                            GUIStyle style = new GUIStyle(GUI.skin.box);

                            Rect blockRect = cellRect;
                            const float padding = 0.2f;
                            blockRect.x += cellSize * zoom * padding;
                            blockRect.y += cellSize * zoom * padding;
                            blockRect.width -= cellSize * zoom * padding * 2;
                            blockRect.height -= cellSize * zoom * padding * 2;

                            style.alignment = TextAnchor.MiddleCenter;
                            style.normal.textColor = Color.white;
                            style.normal.background = colorMap[tile.colorType];
                            style.fontSize = (int)(12 * zoom);
                            GUI.Box(blockRect, "", style);
                        }
                        //draw goal
                        if (tile.goals.Count > 0)
                        {
                            foreach (var goal in tile.goals)
                            {
                                GUIStyle style = new GUIStyle(GUI.skin.box);

                                Rect goalRect = GetGoalRect(goal.direction, cellRect);

                                style.alignment = TextAnchor.MiddleCenter;
                                style.normal.textColor = Color.white;
                                style.normal.background = colorMap[goal.colorType];
                                style.fontSize = (int)(12 * zoom);
                                GUI.Box(goalRect, "", style);
                            }
                        }
                    }
                    else
                    {
                        //draw empty tile
                        GUI.Box(cellRect, "", new GUIStyle(GUI.skin.box));
                    }

                    //set hovering tile
                    if (cellRect.Contains(Event.current.mousePosition))
                    {
                        mousePosition = new Vector2(x, y);
                    }
                    //set selected tile highlight
                    if (tile != null && selectedTile == tile)
                    {
                        GUI.Box(cellRect, GUIContent.none, EditorStyles.selectionRect);
                    }
                }
            }

            //grid lines
            Handles.color = Color.gray;
            //draw vertical line
            for (int x = 0; x <= gridCount; x++)
            {
                float xPos = x * cellSize * zoom + gridOffset.x;
                Handles.DrawLine(new Vector3(xPos, gridOffset.y, 0), new Vector3(xPos, gridWorldSize + gridOffset.y, 0));
            }
            //draw horizontal line
            for (int y = 0; y <= gridCount; y++)
            {
                float yPos = y * cellSize * zoom + gridOffset.y;
                Handles.DrawLine(new Vector3(gridOffset.x, yPos, 0), new Vector3(gridWorldSize + gridOffset.x, yPos, 0));
            }

            Handles.color = Color.red;
            Handles.DrawLine((Vector3)(gridCenter + gridOffset) + Vector3.left * 5, (Vector3)(gridCenter + gridOffset) + Vector3.right * 5);
            Handles.DrawLine((Vector3)(gridCenter + gridOffset) + Vector3.down * 5, (Vector3)(gridCenter + gridOffset) + Vector3.up * 5);

            //GUI.matrix = originalMatrix;
            GUI.EndGroup();
        }
        else
        {
            GUILayout.Label("Select a StageData to edit grid");
        }

        GUILayout.EndVertical();
    }

    private void HandleInput()
    {
        Event e = Event.current;
        if (!viewportRect.Contains(e.mousePosition)) return;
        //pan with middle mouse
        if (e.type == EventType.MouseDrag && e.button == 2)
        {
            panOffset += e.delta;
            Repaint();
        }

        //zoom with scroll
        if (e.type == EventType.ScrollWheel)
        {
            zoom = Mathf.Clamp(zoom - e.delta.y * 0.05f, 0.5f, 2f);
            Repaint();
        }

        //mouse down
        if (selectedObject != null && e.type == EventType.MouseDown)
        {
            Vector2 gridPos = mousePosition;
            int x = (int)gridPos.x;
            int y = (int)gridPos.y;
            if (x >= Mathf.FloorToInt(-gridCount * 0.5f) && x < Mathf.FloorToInt(gridCount * 0.5f) &&
                y >= Mathf.FloorToInt(-gridCount * 0.5f) && y < Mathf.FloorToInt(gridCount * 0.5f))
            {
                if (e.button == 0) // Left click to add/edit tile
                {
                    TileContainer tile = selectedObject.tiles.Find(t => t.x == x && t.y == y);
                    if (tile == null && editType != EditType.Select)
                    {
                        tile = new() { x = x, y = y };
                        selectedObject.tiles.Add(tile);
                    }
                    switch (editType)
                    {
                        case EditType.Select:
                            break;
                        case EditType.SetBoard:
                            //toggle board
                            tile.SetFlag(TileContainer.TileFlags.Board, !tile.HasFlag(TileContainer.TileFlags.Board));
                            break;
                        case EditType.SetBlocks:
                            //create block
                            if (!tile.HasFlag(TileContainer.TileFlags.Block))
                                tile.SetFlag(TileContainer.TileFlags.Block, true);
                            //change block color
                            tile.colorType = colorType;
                            break;
                        case EditType.SetGoal:
                            tile.SetGoalInfo(new() { direction = goalDirection, colorType = colorType });
                            break;
                    }
                    selectedTile = tile;
                    if (tile != null && tile.flags == 0 && tile.goals.Count == 0)
                    {
                        selectedTile = null;
                        selectedObject.tiles.Remove(tile);
                    }
                }
            }
            selectedObject.ClearEmpty();
            Repaint();
        }
    }

    private void MakeColormap()
    {
        colorMap.Clear();
        foreach (ColorType colorType in Enum.GetValues(typeof(ColorType)))
        {
            colorMap.Add(colorType, MakeTexture(2, 2, colorType.ToRawColor()));
        }
    }

    private Texture2D MakeTexture(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; ++i)
        {
            pix[i] = col;
        }
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }

    private Rect GetGoalRect(Direction direction, Rect cellRect)
    {
        const float thickness = 0.2f;
        Rect result = cellRect;
        switch (direction)
        {
            case Direction.Up:
                result.height = thickness * cellSize * zoom;
                break;
            case Direction.Down:
                result.height = thickness * cellSize * zoom;
                result.y += cellSize * zoom * (1 - thickness);
                break;
            case Direction.Left:
                result.width = thickness * cellSize * zoom;
                break;
            case Direction.Right:
                result.width = thickness * cellSize * zoom;
                result.x += cellSize * zoom * (1 - thickness);
                break;
        }
        return result;
    }
}