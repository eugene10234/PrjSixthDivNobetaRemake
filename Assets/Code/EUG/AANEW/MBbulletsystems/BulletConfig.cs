using UnityEngine;
namespace Nobeta.EUG.AANEW.MBbulletsystems
{
    /// <summary>
    /// 子彈配置（ScriptableObject）
    /// 定義每種子彈的屬性
    /// </summary>
    [CreateAssetMenu(fileName = "BulletConfig", menuName = "Game/Bullet Config")]
    public class BulletConfig : ScriptableObject
    {
        [Header("基礎屬性")]
        public string bulletName = "Bullet";
        public float speed = 20f;
        public int damage = 10;
        public float lifetime = 5f;

        [Header("視覺效果")]
        public GameObject visualPrefab;  // 視覺 Prefab
        public Color trailColor = Color.white;
        public float trailWidth = 0.1f;

        [Header("特效")]
        public GameObject hitEffectPrefab;
        public GameObject muzzleEffectPrefab;

        [Header("音效")]
        public AudioClip fireSound;
        public AudioClip hitSound;

        [Header("特殊效果")]
        public BulletType bulletType;
        public float specialEffectValue;  // 火球：燃燒傷害，冰球：減速%
    }

    public enum BulletType
    {
        Normal,
        Fire,      // 火球：持續燃燒
        Ice,       // 冰球：減速
        Lightning, // 雷電：鏈式傷害
        Poison     // 毒球：持續中毒
    }
}