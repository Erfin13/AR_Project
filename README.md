一、專案概述
此專案是一個 AR 物件互動專案，主要功能包括：
1. 在 AR 環境中放置 3D 物件。
2. 透過手勢進行旋轉、縮放操作。
3. 使用 Depth Mask 遮罩防止模型「穿透」畫面。
4. 相機與物件的適當距離調整。

二、物件組成與層級結構
AR Session Origin
  ├── AR Camera
  │   ├── Quad (Depth Mask 遮罩)
  │   ├── ARCameraController.cs
  │
  ├── Drone_Custom (3D 物件)
  │   ├── ARObjectInteraction.cs
  │   ├── ARModelController.cs
  │   ├── Mesh Renderer
  │
  ├── UI (Canvas 與按鈕)
  ├── Scripts
  │   ├── ARCameraController.cs
  │   ├── ARObjectInteraction.cs
  │   ├── ARModelController.cs
  │   ├── DepthMaskController.cs
  ├── Materials
  │   ├── DepthMaskMaterial
  ├── Shaders
      ├── DepthMask.shader

三、腳本功能與掛載位置

1. ARCameraController.cs
   - 管理 AR 相機的 nearClipPlane 與 farClipPlane
   - 掛載之物件: AR Camera
   - 重要變數:
     - camera.nearClipPlane = 0.3f;
     - camera.farClipPlane = 50f;

2. ARObjectInteraction.cs
   - 控制物件的旋轉與縮放
   - 掛載之物件: Drone_Custom
   - 重要變數:
     - rotationSpeed = 100f;
     - scaleSmoothness = 10f;

3. ARModelController.cs
   - 防止 3D 物件離相機太近
   - 掛載之物件: Drone_Custom
   - 重要變數:
     - minDistance = 0.5f;
     - scaleSmoothness = 12f;

4. DepthMaskController.cs
   - 管理 Depth Mask 遮罩作用區域
   - 掛載之物件: Quad
   - 重要變數:
     - activationDistance = 0.8f;

四、Shader 設定
1. DepthMask.shader
   - 功能: 讓 Quad 不顯示，但對 3D 物件生效
   - 位置: Assets/Shaders/DepthMask.shader
   - 代碼:
----------------------------------------------------------------------------
Shader "Custom/DepthMask"
{
    SubShader
    {
        Tags { "Queue" = "Geometry-1" }
        Pass
        {
            ColorMask 0
            ZWrite On
            ZTest LEqual
        }
    }
}
----------------------------------------------------------------------------

   - 掛載說明:
     - 材質: DepthMaskMaterial
     - 遮罩物件: Quad

五、物件位置
1. AR Camera
   - Position: (0, 0, 0)

2. Drone_Custom (3D 模型)
   - Position: (0, 0, 4)

3. Quad (Depth Mask 遮罩)
   - Position: (0, 0, 0.3)
   - Scale: (2, 2, 1)

六、腳本
DepthMaskController
----------------------------------------------------------------------------
using UnityEngine;

public class DepthMaskController : MonoBehaviour
{
    public GameObject depthMaskQuad;  // 拖入你的 Quad 物件
    public float activationDistance = 0.8f;  // 物體距離相機的閾值

    void Start()
    {
        if (depthMaskQuad == null)
        {
            Debug.LogError("Depth Mask Quad 未設定！");
        }
    }

    void Update()
    {
        // 計算物體與相機的距離
        float distanceToCamera = Vector3.Distance(Camera.main.transform.position, transform.position);

        // 根據距離啟動/禁用深度遮罩
        if (distanceToCamera < activationDistance)
        {
            depthMaskQuad.SetActive(true);  // 啟動深度遮罩
        }
        else
        {
            depthMaskQuad.SetActive(false);  // 禁用深度遮罩
        }
    }
}

----------------------------------------------------------------------------

ARObjectInteraction
----------------------------------------------------------------------------
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.XR.ARFoundation;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

public class ARObjectInteraction : MonoBehaviour
{
    private Transform objectTransform;
    private float initialDistance;
    private Vector3 initialScale;
    private float rotationSpeed = 100f;
    private float scaleSmoothness = 10f;

    void Awake()
    {
        EnhancedTouchSupport.Enable();
    }

    void Start()
    {
        objectTransform = gameObject.transform.parent != null ? gameObject.transform.parent : gameObject.transform;

        if (objectTransform == null)
        {
            Debug.LogError("🚨 ARObjectInteraction is not attached to any object!");
        }

        Debug.Log("🟢 AR Session State: " + ARSession.state);
        if (ARSession.state < ARSessionState.SessionTracking)
        {
            Debug.LogWarning("⚠️ AR Session has not fully started. Please check the AR setup!");
        }

        if (gameObject.GetComponent<ARAnchor>() == null)
        {
            Debug.LogWarning("⚠️ No AR Anchor found on this object. Consider adding one!");
        }
    }

    void Update()
    {
        var activeTouches = Touch.activeTouches;
        Debug.Log("🖐 Active touch count: " + activeTouches.Count);

        if (activeTouches.Count == 1) // Single finger rotation
        {
            var touch = activeTouches[0];
            Debug.Log("📌 Single touch DeltaX: " + touch.delta.x);

            if (touch.phase == TouchPhase.Moved)
            {
                Vector3 rotation = objectTransform.localEulerAngles;
                rotation.y -= touch.delta.x * rotationSpeed * Time.deltaTime;
                objectTransform.localEulerAngles = rotation;
            }
        }
        else if (activeTouches.Count == 2) // Two-finger scaling
        {
            var touch0 = activeTouches[0];
            var touch1 = activeTouches[1];

            float currentDistance = Vector2.Distance(touch0.screenPosition, touch1.screenPosition);
            Debug.Log($"📏 Current two-finger distance: {currentDistance}");

            if (touch0.phase == TouchPhase.Began || touch1.phase == TouchPhase.Began)
            {
                initialDistance = currentDistance > 1e-5f ? currentDistance : initialDistance;
                initialScale = objectTransform.localScale;
                Debug.Log($"🎯 Initial distance recorded: {initialDistance}, Initial scale: {initialScale}");
            }
            else if (touch0.phase == TouchPhase.Moved || touch1.phase == TouchPhase.Moved)
            {
                if (initialDistance > 1e-5f)
                {
                    float scaleFactor = Mathf.Clamp(currentDistance / initialDistance, 0.5f, 2f);
                    Vector3 targetScale = initialScale * scaleFactor;
                    Debug.Log($"🔍 Target scale: {targetScale}");
                    objectTransform.localScale = Vector3.Lerp(objectTransform.localScale, targetScale, Time.deltaTime * scaleSmoothness);
                }
            }
        }
    }

    void OnDisable()
    {
        EnhancedTouchSupport.Disable();
    }
}

----------------------------------------------------------------------------

ARModelController
----------------------------------------------------------------------------
using UnityEngine;

public class ARModelController : MonoBehaviour
{
    private Renderer modelRenderer;
    private Vector3 initialScale;
    public float scaleSmoothness = 12f;  // 縮放平滑度
    public float minDistance = 0.5f;  // 最小距離，避免模型過於靠近相機

    void Start()
    {
        modelRenderer = GetComponent<Renderer>();
        initialScale = transform.localScale;
    }

    void Update()
    {
        // 計算相機與物體的距離
        float distanceToCamera = Vector3.Distance(Camera.main.transform.position, transform.position);

        // 根據距離調整透明度
        if (distanceToCamera < minDistance)
        {
            // 讓物體隱形
            Color color = modelRenderer.material.color;
            color.a = 0f;  // 設為完全透明
            modelRenderer.material.color = color;
        }
        else
        {
            // 讓物體可見
            Color color = modelRenderer.material.color;
            color.a = 1f;  // 設為完全不透明
            modelRenderer.material.color = color;
        }

        // 根據距離調整物體縮放
        float scaleFactor = Mathf.Clamp(1 / (distanceToCamera + 0.1f), 0.5f, 1.5f);
        transform.localScale = Vector3.Lerp(transform.localScale, initialScale * scaleFactor, Time.deltaTime * scaleSmoothness);
    }
}

----------------------------------------------------------------------------

ARCameraController
----------------------------------------------------------------------------
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

----------------------------------------------------------------------------


七、Debug & 注意事項
1. 如果 3D 物件被隱藏：調整 activationDistance
2. 如果縮放失效：檢查 ARObjectInteraction.cs
3. 如果物件穿透：檢查 Quad 是否掛載 DepthMaskMaterial

八、交接注意事項
1. 確保已安裝 AR Foundation
2. 將所有腳本正確掛載到對應物件
3. 測試 AR 運行是否正常
4. 確保模型可見，操作可用

