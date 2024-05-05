namespace DTO
{
    /// <summary>
    /// 移動タイプ
    /// </summary>
    /// <remarks>
    /// 動くよ
    /// </remarks>
    public enum MoveType
    {
        /// <summary>
        /// 動きなし
        /// </summary>
        None,

        /// <summary>
        /// 歩く
        /// </summary>
        Walk,

        /// <summary>
        /// 走る
        /// </summary>
        Run,
    }

    /// <summary>
    /// GODプレイヤー
    /// 試しに<see cref="CircularReferencePlayer"/>にseeするテスト。
    /// </summary>
    /// <remarks>神</remarks>
    public class GodPlayer : Player
    {
        /// <summary>
        /// GODランク.
        /// nothing remarks.
        /// </summary>
        public int Rank { get; set; }

        /// <summary>
        /// 現在の移動タイプ
        /// </summary>
        /// <remarks>
        /// 本当は神だからいつでもどこにでも行けるよ。
        /// </remarks>
        public MoveType CurrentMoveType { get; set; }
    }

    /// <summary>
    /// 循環参照プレイヤー
    /// </summary>
    /// <remarks>
    /// 関連クラスから除外しないと無限に再帰しちゃうよー
    /// </remarks>
    public class CircularReferencePlayer : Player
    {
        /// <summary>
        /// 親
        /// </summary>
        /// <remarks>
        /// 親がいないこともあるよ。
        /// </remarks>
        public CircularReferencePlayer? Parent { get; set; }

        /// <summary>
        /// 子供
        /// </summary>
        /// <remarks>
        /// 子供ががいないこともあるよ。
        /// </remarks>
        public CircularReferencePlayer[] Children { get; set; } = [];
    }


    /// <summary>
    /// GODゲーム情報
    /// </summary>
    /// <remarks>
    /// 神ゲー
    /// </remarks>
    public class GodGameInfo : GameInfo
    {
        /// <summary>
        /// ワールドID
        /// </summary>
        /// <remarks>
        /// ワールドなどいくらでもあるのじゃ。
        /// </remarks>
        public int WorldId { get; set; }
    }


    /// <summary>
    /// Nullableパース検証用
    /// </summary>
    public class NullableDTO : DTOBase
    {
        /// <summary>
        /// intだよー
        /// </summary>
        public int? NullableInt { get; set; }

        /// <summary>
        /// DateTimeだよー
        /// </summary>
        public DateTime? NullableDateTime { get; set; }

        /// <summary>
        /// stringだよー
        /// </summary>
        public string? NullableString { get; set; }

        /// <summary>
        /// Item1だよー
        /// </summary>
        public Item1? NullableItem1 { get; set; }

        /// <summary>
        /// int[]だよー
        /// </summary>
        public int[] IntArray { get; set; } = [];

        /// <summary>
        /// int[]?だよー
        /// </summary>
        public int[]? IntNullableArray { get; set; } = [];

        /// <summary>
        /// int?[]だよー
        /// </summary>
        public int?[] NullableIntArray { get; set; } = [];

        /// <summary>
        /// int[]??だよー
        /// </summary>
        public int?[]? NullableIntNullableArray { get; set; }

        /// <summary>
        /// intのListだよー
        /// </summary>
        public List<int> IntList { get; set; } = [];

        /// <summary>
        /// intのList?だよー
        /// </summary>
        public List<int>? IntNullableList { get; set; }

        /// <summary>
        /// int?のListだよー
        /// </summary>
        public List<int?> NullableIntList { get; set; } = [];

        /// <summary>
        /// int?のList?だよー
        /// </summary>
        public List<int?>? NullableIntNullableList { get; set; }

        /// <summary>
        /// int?のList?のList?だよー
        /// </summary>
        public List<List<int?>?>? NullableIntNullableListNullableList { get; set; }
    }

    public class DictionaryDTO : DTOBase
    {
        public Dictionary<int, Player> Players { get; set; } = [];
    }

    /// <summary>
    /// アルファベット列挙してるよー
    /// </summary>
    /// <remarks>
    /// 全部列挙するのは面倒だから3つだけだよー
    /// </remarks>
    public enum AlphabetEnum
    {
        /// <summary>Aだよー</summary>
        A,

        /// <summary>Bだよー</summary>
        B,

        /// <summary>Cだよー</summary>
        C,
    }

    /// <summary>
    /// 数字列挙してるよー
    /// </summary>
    /// <remarks>
    /// ちょこっとだけ列挙だよー
    /// </remarks>
    public enum NumberEnum
    {
        /// <summary>1だよー</summary>
        /// <remarks>おね</remarks>
        One,

        /// <summary>2だよー</summary>
        /// <remarks>とぉ</remarks>
        Two,

        /// <summary>3だよー</summary>
        /// <remarks>ｔｈれえ</remarks>
        Three,
    }

    /// <summary>
    /// Enumの検証用DTO
    /// </summary>
    public class EnumDTO : DTOBase
    {
        /// <summary>
        /// お気に入りのアルファベット
        /// </summary>
        public AlphabetEnum FavoriteAlpabet { get; set; }

        /// <summary>
        /// お気に入りの数字
        /// </summary>
        public NumberEnum FavoriteNumber { get; set; }
    }

    /// <summary>
    /// プロパティとしてもたせる検証用構造体
    /// </summary>
    public struct InnerStruct
    {
        /// <summary>
        /// 適当な値
        /// </summary>
        public int Value { get; set; }

        /// <summary>
        /// 適当な名前
        /// </summary>
        public string Name { get; set; }
    }

    /// <summary>
    /// Structをパースできるか検証だよー
    /// </summary>
    public class HasStrutDTO : DTOBase
    {
        /// <summary>
        /// 構造体持たせてみるよー
        /// </summary>
        public InnerStruct InnerStruct { get; set; }

        /// <summary>
        /// 構造体のnullable持たせてみるよー
        /// </summary>
        public InnerStruct? NullableInnerStruct { get; set; }

        /// <summary>
        /// 構造体をListで持たせてみるよー
        /// </summary>
        public List<InnerStruct> InnerStructList { get; set; } = [];
    }

    /// <summary>
    /// Listの関連クラスを認識できるかの検証用DTO
    /// </summary>
    public class ListAssociationDTO : DTOBase
    {
        public List<Player> Players { get; set; } = [];
        public List<MoveType> MoveTypes { get; set; } = [];
    }

    /// <summary>
    /// Arrayの関連クラスを認識できるかの検証用DTO
    /// </summary>
    public class ArrayAssociationDTO : DTOBase
    {
        public Player[] Players { get; set; } = [];
        public MoveType[] MoveTypes { get; set; } = [];
    }
}
