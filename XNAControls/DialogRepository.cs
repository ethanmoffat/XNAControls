using System.Collections.Generic;

namespace XNAControls
{
    internal class DialogRepository
    {
        static DialogRepository()
        {
            Singleton<DialogRepository>.MapIfMissing(new DialogRepository());
        }

        public Stack<IXNADialog> OpenDialogs { get; } = new Stack<IXNADialog>();
    }
}
