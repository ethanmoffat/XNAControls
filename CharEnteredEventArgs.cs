// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;

namespace XNAControls
{
	internal delegate void CharEnteredHandler(object sender, CharEnteredEventArgs e);

	internal class CharEnteredEventArgs : EventArgs
	{
		internal char Character { get; private set; }

		internal bool IsWin32EventArgs
		{
			get { return this is Win32CharEnteredEventArgs; }
		}

		internal CharEnteredEventArgs(char character)
		{
			Character = character;
		}
	}

	internal class Win32CharEnteredEventArgs : CharEnteredEventArgs
	{
		private readonly int _lParam;

		public Win32CharEnteredEventArgs(char character, int lparam) : base(character)
		{
			_lParam = lparam;
		}

		public int Param
		{
			get { return _lParam; }
		}

		public int RepeatCount
		{
			get { return _lParam & 0xffff; }
		}

		public bool ExtendedKey
		{
			get { return (_lParam & (1 << 24)) > 0; }
		}

		public bool AltPressed
		{
			get { return (_lParam & (1 << 29)) > 0; }
		}

		public bool PreviousState
		{
			get { return (_lParam & (1 << 30)) > 0; }
		}

		public bool TransitionState
		{
			get { return (_lParam & (1 << 31)) > 0; }
		}
	}
}
