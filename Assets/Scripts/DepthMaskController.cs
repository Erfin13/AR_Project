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
