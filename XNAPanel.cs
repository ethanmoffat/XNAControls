using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;

namespace XNAControls
{
	/// <summary>
	/// Note: I've changed XNAPanel to act more like an actual panel of controls, as in WinForms
	/// It derives from control so we have a lot of the convenience functions and can specify an
	/// offset for the child components
	/// </summary>
	public class XNAPanel : XNAControl
	{
		//Note: the Components object was removed to support encapsulation of child controls more fluidly
		//In order to update the components collection of a game to a panel, simply set the 'Visible' property
		//The collection should not be modified mid-game

		[Obsolete("Components has been deprecated. Use the Visible property of XNAPanel to show/hide the controls instead. AddComponent and RemoveComponent can be used to change the collection.")]
		public List<DrawableGameComponent> Components
		{
			get { throw new MemberAccessException("The Components collection of XNAPanel has been deprecated. Do not use this property."); }
		}

		public XNAPanel(Game game, Rectangle? area = null)
			: base(game, area == null ? new Nullable<Vector2>(new Vector2(area.Value.X, area.Value.Y)) : null, area) { }
		
		public void ClearTextBoxes()
		{
			IEnumerable<IGameComponent> children = GetChildren();
			foreach(IGameComponent component in children)
			{
				if(component is XNATextBox)
					(component as XNATextBox).Text = "";
			}
		}

		/// <summary>
		/// Sets the parent of the control to this XNAPanel instance. Rendering offsets are updated accordingly for the control and it's children.
		/// </summary>
		/// <param name="ctrl">The control to add this panel</param>
		/// <param name="force">Force override any existing parent for ctrl, default value false.</param>
		public void AddControl(XNAControl ctrl, bool force = false)
		{
			if(force || ctrl.TopParent == null)
				ctrl.SetParent(this);
		}

		/// <summary>
		/// Sets the parent of the control to null if the controls parent is this XNAPanel instance. Rendering offsets are updated accordingly for the control and it's children.
		/// </summary>
		/// <param name="ctrl">The control to remove from this panel</param>
		public void RemoveControl(XNAControl ctrl)
		{
			if (ctrl.TopParent == this)
				ctrl.SetParent(null);
		}

		//hide the base class method to set parent for this XNAPanel instance...
		public new void SetParent(XNAControl ctrl)
		{
			throw new InvalidOperationException("A panel may not have a parent (it breaks the model)");
		}
	}
}
