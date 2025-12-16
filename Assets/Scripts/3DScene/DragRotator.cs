using UnityEngine;

public class DragRotator : MonoBehaviour
{
    [Header("旋转设置")]
    [Tooltip("旋转速度")]
    public float sensitivity = 10f;

    [Tooltip("惯性阻尼 (越小滑得越久，越大停得越快)")]
    public float damping = 2f;

    [Tooltip("是否反转旋转方向")]
    public bool inverseDirection = true;

    // 内部变量
    private float _currentRotationVelocity;
    private bool _isDragging = false;

    void Update()
    {
        HandleInput();
        ApplyRotation();
    }

    void HandleInput()
    {
        // --- 鼠标/触摸 输入检测 ---

        // 按下瞬间
        if (Input.GetMouseButtonDown(0))
        {
            _isDragging = true;
        }

        // 松开瞬间
        if (Input.GetMouseButtonUp(0))
        {
            _isDragging = false;
        }

        // 拖拽中
        if (_isDragging)
        {
            // 获取鼠标在X轴上的移动距离
            float mouseX = Input.GetAxis("Mouse X");

            // 计算目标速度
            float targetVelocity = mouseX * sensitivity;

            if (inverseDirection) targetVelocity *= -1;

            // 赋值给当前速度
            _currentRotationVelocity = targetVelocity;
        }
    }

    void ApplyRotation()
    {
        // 如果有速度，就应用旋转
        if (Mathf.Abs(_currentRotationVelocity) > 0.001f)
        {
            // 绕 Y 轴旋转
            transform.Rotate(Vector3.up, _currentRotationVelocity, Space.World);

            // 如果没在拖拽，就施加阻尼（让速度慢慢减小）
            if (!_isDragging)
            {
                _currentRotationVelocity = Mathf.Lerp(_currentRotationVelocity, 0, Time.deltaTime * damping);
            }
        }
        else
        {
            _currentRotationVelocity = 0;
        }
    }
}