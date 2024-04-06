namespace DTO
{
    public abstract class DTOBase { }

    /// <summary>
    /// プレイヤー
    /// </summary>
    public class Player : DTOBase
    {
        /// <summary>
        /// 名前
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// 体力
        /// </summary>
        public int Hp { get; set; }
    }

    /// <summary>
    /// ゲーム情報
    /// </summary>
    public class GameInfo : DTOBase
    {
        /// <summary>
        /// タイトル
        /// </summary>
        public string Title { get; set; } = "";

        /// <summary>
        /// 参加者
        /// </summary>
        public Player[] Players { get; set; } = [];
    }
}
