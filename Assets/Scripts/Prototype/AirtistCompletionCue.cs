using System.Collections.Generic;

namespace Airtist.Prototype
{
    // Session-only presentation state; never grants rewards or changes player progress.
    public sealed class AirtistCompletionCue
    {
        private readonly HashSet<string> shown=new HashSet<string>();
        private string pending;
        private int chapter=-1;

        public void Arm(int chapterIndex,string attemptId)
        {
            if(string.IsNullOrEmpty(attemptId))return;
            string key=chapterIndex+":"+attemptId;
            if(shown.Contains(key))return;
            chapter=chapterIndex;pending=key;
        }
        public bool Consume(int chapterIndex)
        {
            if(chapter!=chapterIndex || pending==null)return false;
            bool fresh=shown.Add(pending);pending=null;chapter=-1;
            return fresh;
        }
    }
}
