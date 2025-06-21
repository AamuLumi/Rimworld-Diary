using System.Collections.Generic;

namespace DiaryMod.Core.Text
{
    internal class RandomString : List<string>
    {
        public RandomString(params string[] names)
        {
            foreach (var name in names) Add(name);
        }

        public override string ToString()
        {
            return RandomTools.GetRandomItemFromList(this);
        }
    }
}