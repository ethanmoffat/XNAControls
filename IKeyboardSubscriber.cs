// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using Microsoft.Xna.Framework.Input;

namespace XNAControls
{
	public interface IKeyboardSubscriber
	{
		void ReceiveTextInput(char inputChar);
		void ReceiveTextInput(string text);
		void ReceiveCommandInput(char command);
		void ReceiveSpecialInput(Keys key);

		bool Selected { get; set; } //or Focused
	}
}
