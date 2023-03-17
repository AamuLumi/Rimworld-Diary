using System;
using System.Collections.Generic;

namespace Diary.Core
{
    internal class RandomTools
    {
        private static Random _random = new Random();

        public static T GetRandomItemFromList<T>(List<T> list)
        {
            int index = _random.Next(list.Count);

            return list[index];
        }

        public static bool GetRandomBool()
        {
            return _random.Next(1) == 1;
        }
    }
}
