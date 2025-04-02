using UnityEngine;

public class ARCameraController : MonoBehaviour
{

    void Start()
    {
        // 設定相機的近裁剪和遠裁剪面
        Camera.main.nearClipPlane = 0.3f;
        Camera.main.farClipPlane = 50f;
    }

    void Update()
    {
        // 可以動態調整近裁剪面或遠裁剪面
        // 例如可以根據 AR 的狀態進行更改
    }
}
