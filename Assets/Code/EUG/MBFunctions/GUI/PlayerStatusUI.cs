using Nobeta.EUG.Interfaces;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
// using TMPro; // 如果你有用到文字

namespace Nobeta.EUG.MBFunctions.PlayerUI
{
    public class PlayerStatusUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image healthBar;
        [SerializeField] private Image manaBar;
        [SerializeField] private Image staminaBar;
        [Inject]private readonly IEventBus _eventBus;
        private void OnEnable()
        {
            // 訂閱事件 (假設你的 EventBus 是全域單例或透過 DI 注入)
            // 這裡的語法取決於你 EventBus 的具體實作
            _eventBus.Subscribe<PlayerStatusUpdateEvent>(OnPlayerStatusChanged);
        }

        private void OnDisable()
        {
            // 取消訂閱，防止記憶體洩漏
            _eventBus.Unsubscribe<PlayerStatusUpdateEvent>(OnPlayerStatusChanged);
        }

        private void OnPlayerStatusChanged(PlayerStatusUpdateEvent evt)
        {
            // 更新血條 (Fill Amount 範圍是 0 ~ 1)
            if (healthBar != null)
                healthBar.fillAmount = evt.CurrentHealth / evt.MaxHealth;

            // 更新魔力條
            if (manaBar != null)
                manaBar.fillAmount = evt.CurrentMana / evt.MaxMana;

            // 更新耐力條
            if (staminaBar != null)
                staminaBar.fillAmount = evt.CurrentStamina / evt.MaxStamina;

            // 提示：如果要讓動畫更平滑，建議配合 DOTween 使用
            // healthBar.DOFillAmount(evt.CurrentHealth / evt.MaxHealth, 0.2f);
        }
    }
}
