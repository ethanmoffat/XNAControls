// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;

namespace XNAControls
{
    internal class DialogRepository
    {
        static DialogRepository()
        {
            Singleton<DialogRepository>.Map(new DialogRepository());
        }

        public Stack<IXNADialog> OpenDialogs { get; } = new Stack<IXNADialog>();
    }
}
