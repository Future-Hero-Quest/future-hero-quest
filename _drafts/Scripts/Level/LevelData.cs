using System;
using UnityEngine;

namespace FutureHeroQuest.Level
{
    /// <summary>
    /// ScriptableObject 关卡数据。比硬编码 Prefab 灵活，比 JSON 简单。
    /// 在 Unity 里 右键 Create -> FutureHero -> LevelData 创建实例。
    /// 关卡场景通过 LevelManager 引用并消费此数据。
    /// </summary>
    [CreateAssetMenu(fileName = "LevelData_Lvl01", menuName = "FutureHero/LevelData", order = 0)]
    public class LevelData : ScriptableObject
    {
        [Header("基本信息")]
        public int levelIndex = 1;
        public string displayName = "种树";
        public string sceneName = "Level01_Tree";

        [Header("时间显示")]
        public string pastDateLabel = "1996 年 4 月 15 日";
        public string futureDateLabel = "2026 年 4 月 15 日";

        [Header("种子(确定性随机用)")]
        public int randomSeed = 12345;

        [Header("台词配置")]
        [TextArea(1, 3)] public string[] pastDialogue;
        [TextArea(1, 3)] public string[] futureDialogue;

        [Header("被动提示(防卡死)")]
        public float passiveHintAfterSeconds = 300f;
        [TextArea(1, 3)] public string passiveHintForFuture = "你能找找有没有花圃？";
        [TextArea(1, 3)] public string passiveHintForPast = "看看有没有大树挡路？";

        [Header("过关条件")]
        public LevelCompleteCondition completeCondition;
        public string targetIdRequired;

        public enum LevelCompleteCondition
        {
            FuturePlayerReachZone,
            AllPuzzlesChanged,
            CustomScript,
        }
    }
}
