namespace Diary.Core;

internal static class RandomTools
{
    private static readonly Random Random = new();

    public static T GetRandomItemFromList<T>(List<T> list)
    {
        var index = Random.Next(list.Count);

        return list[index];
    }

    public static bool GetRandomBool()
    {
        return Random.Next(1) == 1;
    }
}