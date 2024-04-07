﻿namespace DTO
{
    /// <summary>
    /// GODプレイヤー
    /// </summary>
    /// <remarks>神</remarks>
    public class GodPlayer : Player
    {
        /// <summary>
        /// GODランク.
        /// nothing remarks.
        /// </summary>
        public int Rank { get; set; }
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
}
