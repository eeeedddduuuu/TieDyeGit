using UnityEngine;

public class ClothPhysicsManager : MonoBehaviour
{
    [Header("布料引用")]
    public GameObject clothModel;
    public Cloth clothComponent;

    [Header("物理设置")]
    public float clothStiffness = 0.5f;
    public float clothDamping = 0.1f;

    private float timer;
    private float originalWindStrength;

    void Start()
    {
        CheckModelComponents();
        InitializeClothPhysics();
    }


    // 检查模型组件
    void CheckModelComponents()
    {
        if (clothModel == null)
        {
            Debug.LogError("ClothModel引用为空！请在Inspector中设置");
            return;
        }

        MeshFilter meshFilter = clothModel.GetComponent<MeshFilter>();
        if (meshFilter == null || meshFilter.sharedMesh == null)
        {
            Debug.LogError($"布料模型 {clothModel.name} 缺少MeshFilter或网格数据");

            if (meshFilter == null)
            {
                meshFilter = clothModel.AddComponent<MeshFilter>();
            }
            if (meshFilter.sharedMesh == null)
            {
                meshFilter.sharedMesh = CreateSimplePlaneMesh();
                Debug.Log("已创建简单平面网格作为替代");
            }
        }

        MeshRenderer renderer = clothModel.GetComponent<MeshRenderer>();
        if (renderer == null)
        {
            Debug.LogError($"布料模型 {clothModel.name} 缺少MeshRenderer");
            renderer = clothModel.AddComponent<MeshRenderer>();
        }

        if (renderer.material == null)
        {
            Debug.LogWarning("为布料模型创建默认材质");
            renderer.material = new Material(Shader.Find("Standard"));
            renderer.material.color = Color.blue;
        }
    }

    // 在 CreateSimplePlaneMesh 方法中，改进UV设置
    Mesh CreateSimplePlaneMesh()
    {
        Mesh mesh = new Mesh();

        // 增加顶点密度以获得更好的变形效果
        int resolution = 10; // 增加分辨率
        Vector3[] vertices = new Vector3[(resolution + 1) * (resolution + 1)];
        Vector2[] uv = new Vector2[vertices.Length];
        int[] triangles = new int[resolution * resolution * 6];

        // 创建顶点和UV
        for (int z = 0; z <= resolution; z++)
        {
            for (int x = 0; x <= resolution; x++)
            {
                float xPos = (float)x / resolution * 2f - 1f;
                float zPos = (float)z / resolution * 2f - 1f;
                vertices[z * (resolution + 1) + x] = new Vector3(xPos, 0, zPos);
                uv[z * (resolution + 1) + x] = new Vector2((float)x / resolution, (float)z / resolution);
            }
        }

        // 创建三角形
        int triIndex = 0;
        for (int z = 0; z < resolution; z++)
        {
            for (int x = 0; x < resolution; x++)
            {
                int bottomLeft = z * (resolution + 1) + x;
                int bottomRight = bottomLeft + 1;
                int topLeft = (z + 1) * (resolution + 1) + x;
                int topRight = topLeft + 1;

                triangles[triIndex++] = bottomLeft;
                triangles[triIndex++] = topLeft;
                triangles[triIndex++] = bottomRight;

                triangles[triIndex++] = topLeft;
                triangles[triIndex++] = topRight;
                triangles[triIndex++] = bottomRight;
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    // 初始化布料物理
    void InitializeClothPhysics()
    {
        if (clothModel == null)
        {
            Debug.LogError("未指定布料模型！");
            return;
        }

        // 添加布料组件
        clothComponent = clothModel.GetComponent<Cloth>();
        if (clothComponent == null)
        {
            clothComponent = clothModel.AddComponent<Cloth>();
        }

        // 配置布料参数
        ConfigureClothComponent();
        Debug.Log("布料物理系统初始化完成");
    }

    // 配置布料组件
    void ConfigureClothComponent()
    {
        if (clothComponent == null) return;

        clothComponent.bendingStiffness = clothStiffness;
        clothComponent.damping = clothDamping;
        clothComponent.useGravity = true;
        clothComponent.stretchingStiffness = 1f;
        clothComponent.friction = 0.3f;
        clothComponent.collisionMassScale = 1.0f;
        clothComponent.enableContinuousCollision = true;
    }


    // 公共方法：重置布料
    public void ResetCloth()
    {
        if (clothComponent != null)
        {
            clothComponent.enabled = false;
            clothComponent.enabled = true;
        }
    }

    public void ApplyTieDyeTexture(Texture2D texture)
    {
        if (clothModel == null)
        {
            Debug.LogError("布料模型引用为空！请检查ClothPhysicsManager的设置");
            return;
        }

        MeshRenderer renderer = clothModel.GetComponent<MeshRenderer>();
        if (renderer == null)
        {
            Debug.LogError($"布料模型 {clothModel.name} 缺少MeshRenderer组件！");
            renderer = clothModel.AddComponent<MeshRenderer>();
            Debug.Log("已自动添加MeshRenderer组件");
        }

        if (renderer.material == null)
        {
            Debug.LogWarning("布料模型材质为空，创建新材质");
            Material newMaterial = new Material(Shader.Find("Standard"));
            renderer.material = newMaterial;
        }

        if (renderer.material != null && texture != null)
        {
            // 关键设置：确保纹理映射模式正确
            texture.wrapMode = TextureWrapMode.Repeat;
            renderer.material.mainTexture = texture;
            renderer.material.mainTextureScale = Vector2.one;
            renderer.material.mainTextureOffset = Vector2.zero;

            // 启用材质的关键属性
            renderer.material.EnableKeyword("_NORMALMAP");
            renderer.material.EnableKeyword("_SPECULARHIGHLIGHTS_OFF");

            Debug.Log($"扎染纹理已成功应用到布料模型: {clothModel.name}");
        }
        else
        {
            Debug.LogError("无法应用纹理：材质或纹理为空");
        }
    }
}