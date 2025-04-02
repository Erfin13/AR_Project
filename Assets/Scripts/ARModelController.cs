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
