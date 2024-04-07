namespace DTO
{
    /// <summary>
    /// GODプレイヤー
    /// </summary>
    public class GodPlayer : Player
    {
        /// <summary>
        /// GODランク
        /// </summary>
        public int Rank { get; set; }
    }

    /// <summary>
    /// 循環参照プレイヤー
    /// </summary>
    public class CircularReferencePlayer : Player
    {
        /// <summary>
        /// 親
        /// </summary>
        public CircularReferencePlayer? Parent { get; set; }

        /// <summary>
        /// 子供
        /// </summary>
        public CircularReferencePlayer[] Children { get; set; } = [];
    }


    /// <summary>
    /// GODゲーム情報
    /// </summary>
    public class GodGameInfo : GameInfo
    {
        /// <summary>
        /// ワールドID
        /// </summary>
        public int WorldId { get; set; }
    }
}
