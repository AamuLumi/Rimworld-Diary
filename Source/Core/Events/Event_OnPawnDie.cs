using DiaryMod.Core.Text;
using RimWorld;
using Verse;

namespace DiaryMod.Core.Events
{
    internal class Event_OnPawnDie : BaseEvent
    {
        private static readonly RandomString _huntingSentences = new RandomString(
            "While hunting !VICTIM.NAME!, !HUNTER.NAME! got hit and died !ADV.DEATH_QUALIFICATION!.",
            "!HUNTER.NAME! runs after !VICTIM.NAME!, but the !NAME.FIGHT! ends to !HUNTER.ARTICLE! ?ADJ.DEATH_QUALIFICATION? death.",
            "!HUNTER.NAME! tried to hunt !VICTIM.NAME!. !HUNTER.PRONOUN! fought like !NAME.ANIMAL/INDEF! but !VICTIM.NAME! got the victory and !HUNTER.NAME!'s life."
        );

        public Event_OnPawnDie(Pawn p)
        {
            if (p.CurJob.def == JobDefOf.Hunt) CreatePawnDieDuringHuntingSentence(p);
        }

        private void CreatePawnDieDuringHuntingSentence(Pawn p)
        {
            var jobDriver = p.CurJob.GetCachedDriver(p) as JobDriver_Hunt;
            var target = jobDriver.Victim;

            var currentDictionary = new TokensDictionary
            {
                { "HUNTER", TokenTranslation.FromPawn(p) }
            };

            if (target != null) currentDictionary.Add("VICTIM", TokenTranslation.FromPawn(target));

            _diaryEntry = TextGenerator.Generate(_huntingSentences.ToString(), currentDictionary);
        }
    }
}