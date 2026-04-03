using UnityEngine;
using UnityEngine.UI;
namespace Nobeta.EUG.Tests
{
    public class UIRotateAroundMouse : MonoBehaviour
    {
        public float radius = 150f;      // 旋轉半徑 (像素)
        public float rotateSpeed = 5f;  // 旋轉速度
        public Canvas myCanvas;         // 拖入你的 Canvas

        private float currentAngle = 0f;
        private RectTransform rectTransform;

        void Start()
        {
            rectTransform = GetComponent<RectTransform>();

            // 如果沒有手動指定 Canvas，嘗試從父層抓取
            if (myCanvas == null)
                myCanvas = GetComponentInParent<Canvas>();
        }

        void Update()
        {
            // 1. 取得鼠標在 Canvas 上的本地座標
            Vector2 localMousePos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                myCanvas.transform as RectTransform,
                Input.mousePosition,
                myCanvas.worldCamera,
                out localMousePos
            );

            // 2. 更新角度
            currentAngle += rotateSpeed * Time.deltaTime;

            // 3. 計算圓周位置 (三角函數)
            float x = localMousePos.x + Mathf.Cos(currentAngle) * radius;
            float y = localMousePos.y + Mathf.Sin(currentAngle) * radius;

            // 4. 套用位置
            rectTransform.anchoredPosition = new Vector2(x, y);

            // 5. (選配) 讓圖片自己也會轉動，增加動感
            rectTransform.localRotation = Quaternion.Euler(0, 0, currentAngle * Mathf.Rad2Deg);
        }
    }
}