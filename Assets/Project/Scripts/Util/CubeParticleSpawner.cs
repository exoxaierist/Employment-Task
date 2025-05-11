using UnityEngine;
using System.Collections;

public class SequentialCubeParticleSpawner : MonoBehaviour
{
    public GameObject cubePrefab; // 작은 큐브 프리팹
    public int numberOfParticles = 20; // 총 파티클 수
    public float launchSpeed = 10f; // 발사 속도
    public float launchInterval = 0.05f; // 파티클 사이 간격
    public float spreadWidth = 1f; // 초기 퍼짐 폭
    public float directionalSpread = 0.1f;
    public bool isAnimating;// 직선 방향의 흩어짐 정도

    public void StartSequentialLaunch(float spread_Width, Vector3 pos, LaunchDirection direction, Material cubeMaterial = null)
    {
        this.transform.position = pos;
        spreadWidth = spread_Width;
        StartCoroutine(LaunchParticlesSequentially(direction, cubeMaterial));
    }

    IEnumerator LaunchParticlesSequentially(LaunchDirection direction, Material cubeMaterial = null)
    {
        isAnimating = true;
        // 발사 기본 방향 설정
        Vector3 baseDirection;
        Vector3 spreadAxis;
        switch (direction)
        {
            case LaunchDirection.Up:
                baseDirection = Vector3.forward;
                spreadAxis = Vector3.right;
                break;
            case LaunchDirection.Down:
                baseDirection = Vector3.back;
                spreadAxis = Vector3.left;
                break;
            case LaunchDirection.Left:
                baseDirection = Vector3.left;
                spreadAxis = Vector3.back;
                break;
            case LaunchDirection.Right:
                baseDirection = Vector3.right;
                spreadAxis = Vector3.forward;
                break;
            default:
                baseDirection = Vector3.forward;
                spreadAxis = Vector3.right;
                break;
        }

        for (int i = 0; i < numberOfParticles; i++)
        {
            // 시작부터 이미 흩어진 위치에서 생성
            Vector3 initialSpreadPosition = transform.position + 
                spreadAxis * Random.Range(-spreadWidth / 2f, spreadWidth / 2f) +
                Vector3.up * Random.Range(-spreadWidth / 2f, spreadWidth / 2f);

            // 파티클 생성
            GameObject particle = Instantiate(cubePrefab, initialSpreadPosition, Quaternion.identity);
            
            // 머티리얼 설정
            if (cubeMaterial != null)
            {
                Renderer particleRenderer = particle.GetComponent<Renderer>();
                if (particleRenderer != null)
                {
                    particleRenderer.material = cubeMaterial;
                }
            }
            
            Rigidbody rb = particle.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // 중력 비활성화
                rb.useGravity = false;
                
                // 기본 방향에 약간의 랜덤 흩어짐 추가
                Vector3 randomSpread = Random.insideUnitSphere * directionalSpread;
                Vector3 finalDirection = (baseDirection + randomSpread).normalized;

                // 속도로 이동
                rb.linearVelocity = finalDirection * launchSpeed;
            }

            // 파티클 제거
            Destroy(particle, 0.5f);

            // 다음 파티클까지 대기
            yield return new WaitForSeconds(launchInterval);
        }

        isAnimating = false;
    }
}

public enum LaunchDirection
{
    Up,
    Down,
    Left,
    Right
}