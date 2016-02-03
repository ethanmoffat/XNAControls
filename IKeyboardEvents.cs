// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace XNAControls
{
	internal interface IKeyboardEvents
	{
		event CharEnteredHandler CharEntered;
		event KeyEventHandler KeyDown;
		event KeyEventHandler KeyUp;
	}
}
