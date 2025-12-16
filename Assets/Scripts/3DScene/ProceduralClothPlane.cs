using UnityEngine;

[ExecuteAlways]
public class ProceduralClothPlane : MonoBehaviour
{
    [Header("布料设置")]
    [Range(10, 100)] public int segments = 50;
    public float width = 5f;
    public float height = 4f;

    void Start()
    {
        GenerateMesh();
    }

    void Update()
    {
        // 游戏运行时，清理掉多余的“静止外壳”，只保留布料
        if (Application.isPlaying)
        {
            MeshRenderer staticRenderer = GetComponent<MeshRenderer>();
            if (staticRenderer != null)
            {
                Destroy(staticRenderer); // 销毁静止的渲染器
            }
        }
    }

    [ContextMenu("重置并生成网格")]
    public void GenerateMesh()
    {
        // 1. 生成高精度网格数据
        Mesh mesh = new Mesh();
        mesh.name = "ProceduralCloth";

        Vector3[] vertices = new Vector3[(segments + 1) * (segments + 1)];
        Vector2[] uv = new Vector2[vertices.Length];
        float xStep = width / segments;
        float yStep = height / segments;

        for (int i = 0, y = 0; y <= segments; y++)
        {
            for (int x = 0; x <= segments; x++, i++)
            {
                vertices[i] = new Vector3(x * xStep - width / 2, y * yStep - height / 2, 0);
                uv[i] = new Vector2((float)x / segments, (float)y / segments);
            }
        }

        int[] triangles = new int[segments * segments * 6];
        for (int ti = 0, vi = 0, y = 0; y < segments; y++, vi++)
        {
            for (int x = 0; x < segments; x++, ti += 6, vi++)
            {
                triangles[ti] = vi;
                triangles[ti + 3] = triangles[ti + 2] = vi + 1;
                triangles[ti + 4] = triangles[ti + 1] = vi + segments + 1;
                triangles[ti + 5] = vi + segments + 2;
            }
        }

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds(); // 关键：确保包围盒正确

        // 2. 优先处理 Cloth 需要的 SkinnedMeshRenderer
        SkinnedMeshRenderer smr = GetComponent<SkinnedMeshRenderer>();
        if (smr == null) smr = gameObject.AddComponent<SkinnedMeshRenderer>();

        smr.sharedMesh = mesh;
        smr.localBounds = mesh.bounds;
        smr.updateWhenOffscreen = true; // 关键：防止布料飞出屏幕被剔除

        // 3. 处理编辑模式预览用的 MeshFilter/Renderer
        // 注意：如果你已经加了Cloth，下面这两行其实是多余的，但为了编辑模式能看见，我们保留它
        MeshFilter mf = GetComponent<MeshFilter>();
        if (mf == null) mf = gameObject.AddComponent<MeshFilter>();
        mf.mesh = mesh;

        MeshRenderer mr = GetComponent<MeshRenderer>();
        if (mr == null) mr = gameObject.AddComponent<MeshRenderer>();
    }
}