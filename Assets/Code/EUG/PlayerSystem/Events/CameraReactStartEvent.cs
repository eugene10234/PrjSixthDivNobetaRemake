using UnityEngine;

namespace Nobeta.EUG.Events
{
    public struct CameraReactStartEvent
    {
        public float TargetFOV ; // 縮小視野，模擬瞄準效果
        public float TargetDistance; // 拉近相機距離，模擬瞄準效果
        public float TargetShoulderOffset;
        public float SmoothSpeed; // 攝影機過渡速度
    }
}
