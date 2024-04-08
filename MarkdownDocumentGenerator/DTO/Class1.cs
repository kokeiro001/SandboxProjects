namespace DTO
{
    public abstract class DTOBase { }

    /// <summary>
    /// プレイヤー
    /// </summary>
    /// <remarks>ふつーのプレイヤー</remarks>
    public class Player : DTOBase
    {
        /// <summary>
        /// 名前
        /// </summary>
        /// <remarks>
        /// 名前の備考だよー。
        /// フルネームだったり、略称だったり、変わることもあるよ。
        /// ニックネームだね。
        /// </remarks>
        public string Name { get; set; } = "";

        /// <summary>
        /// 体力
        /// </summary>
        /// <remarks>
        /// 0になっても気合で動けるよ。
        /// </remarks>
        public int Hp { get; set; }
    }

    /// <summary>
    /// アイテム1
    /// </summary>
    public class Item1
    {
        /// <summary>
        /// 名前1
        /// </summary>
        public string Name1 { get; set; } = "";
    }

    /// <summary>
    /// アイテム2
    /// </summary>
    public class Item2
    {
        /// <summary>
        /// 名前2
        /// </summary>
        public string Name2 { get; set; } = "";
    }

    /// <summary>
    /// アイテム3
    /// </summary>
    public class Item3
    {
        /// <summary>
        /// 名前3
        /// </summary>
        public string Name3 { get; set; } = "";
    }

    /// <summary>
    /// ゲーム情報
    /// </summary>
    /// <remarks>
    /// 備考だよー
    /// </remarks>
    public class GameInfo : DTOBase
    {
        /// <summary>
        /// タイトル
        /// </summary>
        /// <remarks>タイトルの備考だよー</remarks>
        public string Title { get; set; } = "";

        /// <summary>
        /// 参加者(配列)
        /// </summary>
        /// <remarks>配列とList同じように扱いたいよー</remarks>
        public Player[] PlayersArray { get; set; } = [];

        /// <summary>
        /// 参加者(リスト)
        /// </summary>
        /// <remarks>ReadOnlyListとかくると都度対応が必要な実装でだるいよー</remarks>
        public List<Player> PlayersList { get; set; } = [];

        /// <summary>
        /// アイテム1あるかも
        /// </summary>
        public Item1? Item1 { get; set; }

        /// <summary>
        /// アイテム2あるかも
        /// </summary>
        public Item2? Item2 { get; set; }

        /// <summary>
        /// アイテム3あるかも
        /// </summary>
        public Item3? Item3 { get; set; }
    }
}
