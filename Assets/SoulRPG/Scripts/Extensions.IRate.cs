using System;
using System.Collections.Generic;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

namespace HK.Framework
{
    /// <summary>
    /// <see cref="IRate"/>に関する拡張関数
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// 抽選を行う
        /// </summary>
        public static T Lottery<T>(this IList<T> self) where T : IRate
        {
            return Lottery(self, () => Random.value);
        }

        /// <summary>
        /// 抽選を行う
        /// </summary>
        public static int LotteryIndex<T>(this IList<T> self) where T : IRate
        {
            return LotteryIndex(self, () => Random.value);
        }

        /// <summary>
        /// 抽選を行う
        /// </summary>
        /// <remarks>
        /// 乱数を独自で実装したい場合に利用します
        /// </remarks>
        public static T Lottery<T>(this IList<T> self, Func<float> randomSelector) where T : IRate
        {
            var current = 0;
            var random = randomSelector();
            foreach (var i in self)
            {
                if (random >= current && random < current + i.Rate)
                {
                    return i;
                }

                current += i.Rate;
            }

            Assert.IsTrue(false, "未定義の動作です");
            return default(T);
        }
        /// <summary>
        /// 抽選を行う
        /// </summary>
        /// <remarks>
        /// 乱数を独自で実装したい場合に利用します
        /// </remarks>
        public static int LotteryIndex<T>(this IList<T> self, Func<float> randomSelector) where T : IRate
        {
            var current = 0;
            var random = randomSelector();
            for (var i = 0; i < self.Count; i++)
            {
                var element = self[i];
                if (random >= current && random < current + element.Rate)
                {
                    return i;
                }

                current += element.Rate;
            }

            Assert.IsTrue(false, "未定義の動作です");
            return -1;
        }
    }
}
