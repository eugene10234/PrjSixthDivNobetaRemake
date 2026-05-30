// ============================================================================
// Core Interfaces - 核心接口定義
// 定義所有服務的契約，遵循 SOLID 的 DIP（依賴倒置原則）
// ============================================================================

using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GDCO.Core.Interfaces
{
    // ========================================================================
    // 輸入系統接口
    // ========================================================================
    
    /// <summary>
    /// 輸入服務接口
    /// </summary>
    public interface IInputService
    {
        /// <summary>
        /// 是否啟用輸入
        /// </summary>
        bool InputEnabled { get; set; }
        
        /// <summary>
        /// 獲取軸向輸入
        /// </summary>
        Vector2 GetMovementInput();
        
        /// <summary>
        /// 獲取視角輸入
        /// </summary>
        Vector2 GetLookInput();
        
        /// <summary>
        /// 檢查按鈕按下
        /// </summary>
        bool GetButtonDown(string buttonName);
        
        /// <summary>
        /// 檢查按鈕持續按住
        /// </summary>
        bool GetButton(string buttonName);
        
        /// <summary>
        /// 檢查按鈕釋放
        /// </summary>
        bool GetButtonUp(string buttonName);
        
        /// <summary>
        /// 設置輸入模式（鍵鼠 / 手把）
        /// </summary>
        void SetInputMode(InputMode mode);
    }
    
    public enum InputMode
    {
        KeyboardMouse,
        Gamepad
    }
}
