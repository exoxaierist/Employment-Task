using UnityEngine;

public partial class BoardManager : MonoBehaviour
{
    [SerializeField] private StageData[] stages;
    private BoardGenerator boardGenerator;

    private int currentStage = 0;
    private bool isStarted = false;

    private void Start() => StartGame();
    public void StartGame()
    {
        if (isStarted) return;
        isStarted = true;

        Initialize();
        LoadStage(0);
    }

    private void Initialize()
    {
        boardGenerator = new();
    }

    public void LoadStage(int stageIndex)
    {
        currentStage = Mathf.Clamp(stageIndex, 0, stages.Length - 1);
        boardGenerator.Generate(stages[currentStage]);
        SetCamera();
    }

    [ContextMenu("Load Next Stage")]
    public void LoadNextStage() => LoadStage(currentStage + 1);

    private void SetCamera()
    {
        float cameraHeight = 10;
        Vector3 position = boardGenerator.GetStageBounds().center;
        Camera.main.transform.position = position + Vector3.back * Vector3.Dot(Camera.main.transform.forward, Vector3.forward) * cameraHeight + Vector3.up * cameraHeight;
    }
}