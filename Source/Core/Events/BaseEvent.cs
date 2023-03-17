using Verse;

namespace Diary.Core.Events
{
    internal class BaseEvent
    {
        protected string _diaryEntry;

        public BaseEvent() { }

        public void CommitEntry()
        {
            if (_diaryEntry != null)
            {
                Current.Game.GetComponent<DiaryService>().AppendEntryNow(_diaryEntry);
            }
        }

        override public string ToString()
        {
            return _diaryEntry;
        }
    }
}
