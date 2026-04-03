using UnityEngine;
using UnityEngine.UI;
namespace Nobeta.EUG.Tests
{
    public class RotateTranforms : MonoBehaviour
    {
        float angle = 0;
        public float speed = 5f;
        public float radius1 = 2f;

        void Update1()
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;

            angle += speed * Time.deltaTime;

            float x = mousePos.x + Mathf.Cos(angle) * radius1;
            float y = mousePos.y + Mathf.Sin(angle) * radius1;

            transform.position = new Vector3(x, y, 0);
        }
        public float rotateSpeed = 100f; // 旋轉速度
        public float radius2 = 2f;      // 繞行半徑

        void Update2()
        {
            // 1. 取得鼠標的世界空間座標
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0; // 確保在 2D 平面

            // 2. 讓物件繞著鼠標位置旋轉
            // 參數：中心點、旋轉軸 (2D 通常是 Z 軸)、旋轉角度
            transform.RotateAround(mousePos, Vector3.forward, rotateSpeed * Time.deltaTime);

            // 3. (選配) 如果你想維持固定的半徑，可以在這裡微調位置
            Vector3 dir = (transform.position - mousePos).normalized;
            transform.position = mousePos + dir * radius2;
        }
    }
}