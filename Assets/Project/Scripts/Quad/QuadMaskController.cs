using UnityEngine;

public class QuadMaskController : MonoBehaviour
{
    public Camera mainCamera;
    public Renderer quadRenderer;
    
    void Start()
    {
        // 참조 가져오기
        if (mainCamera == null)
            mainCamera = Camera.main;
            
        if (quadRenderer == null)
            quadRenderer = GetComponent<Renderer>();
            
        if (mainCamera == null || quadRenderer == null)
        {
            Debug.LogError("카메라나 렌더러를 찾을 수 없습니다.");
            return;
        }
        
        try
        {
            // 기존 머티리얼을 기반으로 새 머티리얼 생성
            Material currentMaterial = quadRenderer.material;
            Material maskMaterial = new Material(currentMaterial);
            
            // 카메라 배경색과 동일하게 설정
            maskMaterial.color = mainCamera.backgroundColor;
            
            // 머티리얼 적용
            quadRenderer.material = maskMaterial;
        }
        catch (System.Exception e)
        {
            Debug.LogError("머티리얼 설정 중 오류 발생: " + e.Message);
        }
    }
}