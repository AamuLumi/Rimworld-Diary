using System.Collections.Generic;

namespace Diary.Core.Text
{
    internal class RandomString : List<string>
    {
        public RandomString(params string[] names)
        {
            foreach (string name in names)
            {
                Add(name);
            }
        }

        public override string ToString()
        {
            return RandomTools.GetRandomItemFromList(this);
        }
    }
}
