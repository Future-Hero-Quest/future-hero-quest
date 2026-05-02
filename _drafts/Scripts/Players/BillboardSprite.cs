using UnityEngine;

namespace FutureHeroQuest.Players
{
    /// <summary>
    /// 让 Sprite 永远面向主相机（2.5D 经典 Billboard）。
    /// 默认只锁 Y 轴旋转（角色不会向上下倾斜），适合 30° 俯视斜角相机。
    /// </summary>
    public class BillboardSprite : MonoBehaviour
    {
        [SerializeField] private bool lockYOnly = true;

        private Camera _cam;

        private void LateUpdate()
        {
            if (_cam == null) _cam = Camera.main;
            if (_cam == null) return;

            Vector3 dir = transform.position - _cam.transform.position;
            if (lockYOnly) dir.y = 0f;
            if (dir.sqrMagnitude < 0.0001f) return;

            transform.rotation = Quaternion.LookRotation(dir.normalized);
        }
    }
}
