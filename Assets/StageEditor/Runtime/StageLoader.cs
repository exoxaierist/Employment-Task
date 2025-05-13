using UnityEngine;

public class StageLoader : MonoBehaviour
{
    private BoardGenerator boardGenerator;
    [SerializeField] private StageData stageData;

    public void SetStage(StageData inStageData)
    {
        stageData = inStageData;
    }

    private void Start()
    {
        if (stageData == null) return;
        boardGenerator = new();
        boardGenerator.Generate(stageData);
        SetCamera();
    }

    private void SetCamera()
    {
        float cameraHeight = 10;
        Vector3 position = boardGenerator.GetStageBounds().center;
        Camera.main.transform.position = position + Vector3.back * Vector3.Dot(Camera.main.transform.forward, Vector3.forward) * cameraHeight + Vector3.up * cameraHeight;
    }
}
