using System.Collections.Generic;
using Verse;

namespace Diary.Core.Text
{
    internal class TokenTranslation : Dictionary<string, RandomString>
    {
        public TokenTranslation() { }

        public static TokenTranslation FromPawn(Pawn p)
        {
            Log.Message("token translation");
            var tokens = new TokenTranslation { { "NAME", new RandomString(p.LabelShort) } };

            switch (p.gender)
            {
                case Gender.Male:
                    tokens.Add("ARTICLE", new RandomString("her"));
                    tokens.Add("PRONOUN", new RandomString("he"));
                    break;
                case Gender.Female:
                    tokens.Add("ARTICLE", new RandomString("her"));
                    tokens.Add("PRONOUN", new RandomString("she"));
                    break;
                case Gender.None:
                    tokens.Add("ARTICLE", new RandomString("their"));
                    tokens.Add("PRONOUN", new RandomString("they"));
                    break;
                default:
                    tokens.Add("ARTICLE", new RandomString("its"));
                    tokens.Add("PRONOUN", new RandomString("it"));
                    break;
            }

            return tokens;
        }
    }
}
