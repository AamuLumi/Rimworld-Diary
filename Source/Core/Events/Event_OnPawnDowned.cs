using Diary.Core.Text;
using RimWorld;
using Verse;

namespace Diary.Core.Events
{
    internal class Event_OnPawnDowned : BaseEvent
    {
        private static readonly RandomString _huntingSentences = new RandomString(
            "While hunting !VICTIM.NAME!, !HUNTER.NAME! got hit and fell down !ADV.FALL_QUALIFICATION!.",
            "!HUNTER.NAME! fought !VICTIM.NAME!, but the !NAME.FIGHT! ends to !HUNTER.ARTICLE! ?ADJ.FALL_QUALIFICATION? fall.",
            "!HUNTER.NAME! tried to hunt !VICTIM.NAME!. !HUNTER.PRONOUN! fought like !NAME.ANIMAL/INDEF! but !VICTIM.NAME! fell down."
        );

        public Event_OnPawnDowned(Pawn p)
        {
            Log.Message($"{p} {p.jobs.curDriver} {p.jobs.jobQueue.Count}");
            if (p.jobs.curDriver is JobDriver_Hunt)
                CreatePawnDownedDuringHuntingSentence(p);
        }

        private void CreatePawnDownedDuringHuntingSentence(Pawn p)
        {
            var jobDriver = p.CurJob.GetCachedDriver(p) as JobDriver_Hunt;
            var target = jobDriver.Victim;

            var currentDictionary = new TokensDictionary
            {
                { "HUNTER", TokenTranslation.FromPawn(p) }
            };

            if (target != null)
                currentDictionary.Add("VICTIM", TokenTranslation.FromPawn(target));

            _diaryEntry = TextGenerator.Generate(_huntingSentences.ToString(), currentDictionary);
        }
    }
}
