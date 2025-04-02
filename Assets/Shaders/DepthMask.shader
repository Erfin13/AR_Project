Shader "Custom/DepthMask"
{
    SubShader
    {
        Tags { "Queue" = "Geometry-1" } // 確保比一般物件先渲染
        Pass
        {
            ColorMask 0  // 不渲染顏色
            ZWrite On    // 啟用深度寫入
            ZTest LEqual // 只渲染深度較小的部分
        }
    }
}
