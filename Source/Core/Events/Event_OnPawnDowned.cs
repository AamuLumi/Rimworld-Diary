using Diary.Core.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace Diary.Core.Events
{
    internal class Event_OnPawnDowned : BaseEvent
    {
        private static readonly RandomString _huntingSentences = new RandomString(
            "While hunting !VICTIM.NAME!, !HUNTER.NAME! got hit and fell down !ADV.FALL_QUALIFICATION!.",
            "!HUNTER.NAME! fought !VICTIM.NAME!, but the !NAME.FIGHT! ends to !HUNTER.ARTICLE! ?ADJ.FALL_QUALIFICATION? fall.",
            "!HUNTER.NAME! tried to hunt !VICTIM.NAME!. !HUNTER.PRONOUN! fought like !NAME.ANIMAL/UNDEF! but !VICTIM.NAME! fell down."
        );

        private static readonly RandomString _attackSentences = new RandomString(
            "During the fight against !ENEMY.NAME!, !PAWN.NAME! fell down !ADV.FALL_QUALIFICATION!."
        );

        public Event_OnPawnDowned(Pawn p)
        {
            Log.Message($"{p} {p.jobs.curDriver} {p.jobs.jobQueue.Count}");
            if (p.jobs.curDriver is JobDriver_Hunt)
                CreatePawnDownedDuringHuntingSentence(p);
            else if (p.jobs.curDriver is JobDriver_AttackMelee || p.jobs.curDriver is JobDriver_AttackStatic)
                CreatePawnDownedDuringAttackSentence(p);
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

        private void CreatePawnDownedDuringAttackSentence(Pawn p)
        {
            var jobDriver = p.CurJob.GetCachedDriver(p) as JobDriver_AttackMelee;
            var target = jobDriver.job.targetA.Pawn;

            var currentDictionary = new TokensDictionary
            {
                { "PAWN", TokenTranslation.FromPawn(p) }
            };

            if (target != null)
                currentDictionary.Add("ENEMY", TokenTranslation.FromPawn(target));

            _diaryEntry = TextGenerator.Generate(_attackSentences.ToString(), currentDictionary);
        }
    }
}