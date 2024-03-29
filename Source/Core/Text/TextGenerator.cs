using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Diary.Core.Text
{
    internal class TextGenerator
    {
        private static readonly Regex requiredTerm = new Regex(
            @"!(?<Key1>\w+)\.(?<Key2>\w+)(\/(?<Option>\w+))?!",
            RegexOptions.IgnoreCase | RegexOptions.Compiled
        );

        private static readonly Regex optionalTerm = new Regex(
            @"\?(?<Key1>\w+)\.(?<Key2>\w+)(\/(?<Option>\w+))?\?",
            RegexOptions.IgnoreCase | RegexOptions.Compiled
        );

        private static readonly Regex dotTerm = new Regex(
            @"\.",
            RegexOptions.IgnoreCase | RegexOptions.Compiled
        );

        private static Regex letterTerm = new Regex(
            @"\w",
            RegexOptions.IgnoreCase | RegexOptions.Compiled
        );

        private static readonly string[] vowels = { "a", "e", "i", "o", "u" };

        private static string ComputeTranslation(
            Match requiredMatch,
            TokensDictionary dict,
            TokensDictionary defaultDictionary
        )
        {
            var key1 = requiredMatch.Groups["Key1"].Value;
            var key2 = requiredMatch.Groups["Key2"].Value;
            var optionGroup = requiredMatch.Groups["Option"];
            string option = null;

            if (optionGroup != null)
                option = optionGroup.Value;

            RandomString associatedStr = null;

            if (dict.TryGetValue(key1, out var translations))
                translations.TryGetValue(key2, out associatedStr);

            if (associatedStr == null)
            {
                if (defaultDictionary.TryGetValue(key1, out translations))
                {
                    if (!translations.TryGetValue(key2, out associatedStr))
                        associatedStr = new RandomString("__UNKNOWN__");
                }
                else
                {
                    associatedStr = new RandomString("__UNKNOWN__");
                }
            }

            var finalString = associatedStr.ToString();

            if (option == "DEF")
            {
                finalString = $"the ${finalString}";
            }
            else if (option == "UNDEF")
            {
                if (vowels.Any(vowel => finalString.StartsWith(vowel)))
                    finalString = $"an {finalString}";
                else
                    finalString = $"a {finalString}";
            }

            return finalString;
        }

        public static string Generate(string baseText, TokensDictionary dict)
        {
            var defaultDictionary = DefaultDictionary.GetInstance().Dictionary;
            var resultText = new StringBuilder(baseText);

            var requiredMatches = requiredTerm.Matches(baseText);

            foreach (Match requiredMatch in requiredMatches)
            {
                var term = requiredMatch.Value;

                resultText.Replace(
                    term,
                    ComputeTranslation(requiredMatch, dict, defaultDictionary)
                );
            }

            var optionalMatches = optionalTerm.Matches(baseText);

            foreach (Match optionalMatch in optionalMatches)
            {
                var term = optionalMatch.Value;

                if (RandomTools.GetRandomBool())
                    resultText.Replace(
                        term,
                        ComputeTranslation(optionalMatch, dict, defaultDictionary)
                    );
                else
                    resultText.Replace(term, "");
            }

            // Remove long whitespaces
            for (var i = 0; i < resultText.Length - 1; i++)
                while (
                    i < resultText.Length - 1 && resultText[i] == ' ' && resultText[i + 1] == ' '
                )
                    resultText.Remove(i, 1);

            // Uppercase necessary letters
            var dotMatches = dotTerm.Matches(resultText.ToString());

            foreach (Match dotMatch in dotMatches)
            {
                var index = dotMatch.Index;

                for (var i = index; i < resultText.Length; i++)
                {
                    var currentChar = resultText[i];

                    if (!char.IsLetterOrDigit(currentChar)) continue;

                    if (char.IsLetter(currentChar) && char.IsLower(currentChar))
                        resultText[i] = char.ToUpper(currentChar);

                    break;
                }
            }

            return resultText.ToString();
        }
    }
}