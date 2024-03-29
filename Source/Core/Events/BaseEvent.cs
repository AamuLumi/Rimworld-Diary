using Verse;

namespace Diary.Core.Events
{
    internal class BaseEvent
    {
        protected string _diaryEntry;

        protected BaseEvent()
        {
        }

        public void CommitEntry()
        {
            if (_diaryEntry != null) Current.Game.GetComponent<DiaryService>().AppendEntryNow(_diaryEntry);
        }

        public override string ToString()
        {
            return _diaryEntry;
        }
    }
}