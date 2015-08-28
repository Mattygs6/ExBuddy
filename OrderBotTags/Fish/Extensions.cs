namespace ExBuddy.OrderBotTags
{
    using System.Collections.Generic;
    using System.Security.Cryptography;

    /// <summary>
    ///     Extensions
    /// </summary>
    public static partial class Extensions
    {
        public static void Shuffle<T>(this IList<T> list)
        {
            if (list.Count > 1)
            {
                using (var provider = new RNGCryptoServiceProvider())
                {
                    var n = list.Count;
                    while (n > 1)
                    {
                        var box = new byte[1];
                        do
                        {
                            provider.GetBytes(box);
                        }
                        while (!(box[0] < n * (byte.MaxValue / n)));

                        var k = box[0] % n;
                        n--;
                        var value = list[k];
                        list[k] = list[n];
                        list[n] = value;
                    }
                }
            }
        }
    }
}