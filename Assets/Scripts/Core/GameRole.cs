namespace FutureHeroQuest.Core
{
    /// <summary>
    /// 玩家角色定义。Master Client = Past (P1·阿虚原型 K)，Client = Future (P2·朝比奈原型 M)。
    /// 这个分配在 NetworkManager.OnJoinedRoom 完成。
    /// </summary>
    public enum GameRole
    {
        Past,
        Future,
    }
}
