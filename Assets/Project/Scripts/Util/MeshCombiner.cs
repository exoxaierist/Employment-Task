using System.Collections.Generic;
using UnityEngine;

namespace Project.Scripts.Util
{
    public class MeshCombiner
    {
        public static Mesh CombineMeshes(List<GameObject> go)
        {
            // 유효한 메시 필터만 찾기
            List<MeshFilter> validMeshFilters = new List<MeshFilter>();
            
            for(int i = 0; i < go.Count; i++)
            {
                MeshFilter filter = go[i].GetComponent<MeshFilter>();
                
                // 메시필터가 존재하고 메시가 비어있지 않은지 확인
                if(filter != null && filter.sharedMesh != null)
                {
                    validMeshFilters.Add(filter);
                }
                else
                {
                    Debug.LogWarning($"GameObject {go[i].name}에 유효한 메시가 없습니다.");
                }
            }
            
            if(validMeshFilters.Count == 0)
            {
                Debug.LogError("결합할 유효한 메시가 없습니다!");
                return null;
            }
            
            CombineInstance[] combine = new CombineInstance[validMeshFilters.Count];
            
            for(int i = 0; i < validMeshFilters.Count; i++)
            {
                combine[i].mesh = validMeshFilters[i].sharedMesh;
                combine[i].transform = validMeshFilters[i].transform.localToWorldMatrix;
            }
            
            Mesh mesh = new Mesh();
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; // 더 큰 메시 지원
            mesh.CombineMeshes(combine, true); // 두 번째 매개변수는 머티리얼 병합 여부
            
            // 디버깅용 정보
            Debug.Log($"결합된 메시: 버텍스 수: {mesh.vertexCount}, 삼각형 수: {mesh.triangles.Length/3}");
            
            return mesh;
        }
    }
}