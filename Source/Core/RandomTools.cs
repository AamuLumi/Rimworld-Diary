using System;
using System.Collections.Generic;

namespace DiaryMod.Core
{
    internal class RandomTools
    {
        private static readonly Random _random = new Random();

        public static T GetRandomItemFromList<T>(List<T> list)
        {
            var index = _random.Next(list.Count);

            return list[index];
        }

        public static bool GetRandomBool()
        {
            return _random.Next(1) == 1;
        }
    }
}