using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace XNAControls
{
	public abstract class XNAControl : DrawableGameComponent
	{
		/// <summary>
		/// Static members: Dialogs are pushed onto the stack as they are opened. Only the
		///		top-most dialog can be interacted with. Dialogs are popped off the stack
		///		once they are closed. Optionally, they are given a Closing event that has a
		///		particular dialog result.
		/// </summary>
		public static Stack<XNADialog> Dialogs = new Stack<XNADialog>();

		public SpriteBatch SpriteBatch { get; set; }

		/// <summary>
		/// Returns the DrawLocation on the screen (no offset)
		/// Sets the location to draw on the screen for moving controls
		/// Only way of moving the control
		/// </summary>
		public Vector2 DrawLocation
		{
			get { return this.drawLocation; }
			set
			{
				drawLocation = value;
				drawArea.X = (int)value.X;
				drawArea.Y = (int)value.Y;

				//update all the child offsets in this code recursively
				foreach (XNAControl child in children)
				{
					child.SetParent(this);
				}
			}
		}

		/// <summary>
		/// Returns the Draw Area (no offset)
		/// </summary>
		public Rectangle DrawArea { get { return drawArea; } }

		/// <summary>
		/// Returns the Draw Area, factoring in any offset (this would be set by parent control)
		/// </summary>
		public Rectangle DrawAreaWithOffset
		{
			get
			{
				return new Rectangle(drawArea.X + xOff, drawArea.Y + yOff, drawArea.Width, drawArea.Height);
			}
		}

		/// <summary>
		/// Gets the very top parent control of this control.
		/// </summary>
		public XNAControl TopParent
		{
			get
			{
				XNAControl ret = this.parent;
				if (ret != null)
					while (ret.parent != null)
						ret = ret.parent;
				return ret;
			}
		}

		/// <summary>
		/// Gets/sets whether this control should be drawn
		/// Setting this applies it to all child controls as well
		/// </summary>
		public new bool Visible
		{
			get { return base.Visible; }
			set
			{
				foreach(XNAControl child in children)
				{
					child.Visible = value;
				}

				base.Visible = value;
			}
		}

		protected bool MouseOver
		{
			get
			{
				MouseState state = Mouse.GetState();
				return DrawAreaWithOffset.ContainsPoint(state.X, state.Y);
			}
		}

		protected bool MouseOverPreviously
		{
			get
			{
				return DrawAreaWithOffset.ContainsPoint(PreviousMouseState.X, PreviousMouseState.Y);
			}
		}

		protected MouseState PreviousMouseState { get; set; }
		protected KeyboardState PreviousKeyState { get; set; }

		protected Vector2 drawLocation;
		protected Rectangle drawArea;

		protected void _setSize(int width, int height)
		{
			drawArea.Width = width;
			drawArea.Height = height;

			if (SizeChanged != null)
				SizeChanged(null, null);
		}

		protected XNAControl parent;
		protected int xOff, yOff;

		protected bool shouldClickDrag = true;

		protected EventHandler SizeChanged;

		protected List<XNAControl> children = new List<XNAControl>();
		
		/// <summary>
		/// Construct a generic XNAControl with an encapsulating game, a location on screen, and an area.
		/// </summary>
		/// <param name="game">The game object that "owns" the control</param>
		/// <param name="location">Location to draw the control on screen</param>
		/// <param name="area">The draw area of the control</param>
		public XNAControl(Game game, Vector2? location, Rectangle? area) : base(game)
		{
			drawLocation = location ?? new Vector2(0, 0);
			drawArea = area ?? new Rectangle((int)drawLocation.X, (int)drawLocation.Y, 0, 0);

			PreviousMouseState = Mouse.GetState();
			PreviousKeyState = Keyboard.GetState();

			parent = null;
			SpriteBatch = new SpriteBatch(game.GraphicsDevice);

			xOff = yOff = 0;

			DrawOrder = 0; //normal controls drawn at this level

			game.Components.Add(this); //add this control to the game components
		}

		//this constructor requires child controls to add themselves to game components (testing)
		public XNAControl(Game game) : base(game)
		{
			PreviousMouseState = Mouse.GetState();
			PreviousKeyState = Keyboard.GetState();

			parent = null;
			SpriteBatch = new SpriteBatch(game.GraphicsDevice);
			xOff = yOff = 0;

			DrawOrder = 0;
		}

		/// <summary>
		/// Set the parent of this control to the specified control and update relevant members
		/// </summary>
		/// <param name="newParent">The new parent control</param>
		public void SetParent(XNAControl newParent)
		{
			//When newParent is null:		this control is in Game.Components. Any children are removed from Game.Components. This is removed from parent's children.
			//When newParent is not null:	this control is removed from Game.Components. This is added to parent's child controls.
			if(newParent == null)
			{
				if (parent != null && parent.children.Contains(this))
				{
					parent.children.Remove(this);
					//update DrawOrder?
				}

				if (!Game.Components.Contains(this))
					Game.Components.Add(this);

				parent = newParent;
				xOff = yOff = 0;
			}
			else
			{
				if (Game.Components.Contains(this))
					Game.Components.Remove(this);

				parent = newParent;

				if (!parent.children.Contains(this))
					parent.children.Add(this);

				xOff = (int)parent.DrawAreaWithOffset.X;
				yOff = (int)parent.DrawAreaWithOffset.Y;
				DrawOrder = parent.DrawOrder + 1;

				//update offsets/draworder of children
				foreach(XNAControl child in children)
					child.SetParent(this);
			}

			if (children.Count > 0)
			{
				children.Sort((x, y) =>
				{
					return x.DrawOrder - y.DrawOrder;
				});
			}
		}
		
		public override void Update(GameTime gameTime)
		{
			if (drawArea.Width == 0 || drawArea.Height == 0)
				throw new InvalidOperationException("The drawn area of the control must not be 0.");

			if (!Visible)
				return;

			foreach (XNAControl child in children)
				child.Update(gameTime);

			if (TopParent == null) //child controls can have negative offsets! only check for TopParents
			{
				Rectangle gdm = Game.Window.ClientBounds;
				if (DrawLocation.X < 0) DrawLocation = new Vector2(0, DrawLocation.Y);
				if (DrawLocation.Y < 0) DrawLocation = new Vector2(DrawLocation.X, 0);
				if (DrawLocation.X > gdm.Width - DrawAreaWithOffset.Width) DrawLocation = new Vector2(gdm.Width - DrawAreaWithOffset.Width, DrawLocation.Y);
				if (DrawLocation.Y > gdm.Height - DrawAreaWithOffset.Height) DrawLocation = new Vector2(DrawLocation.X, gdm.Height - DrawAreaWithOffset.Height);
			}

			PreviousMouseState = Mouse.GetState();
			PreviousKeyState = Keyboard.GetState();

			base.Update(gameTime);
		}

		//private SpriteFont dbg;
		public override void Draw(GameTime gameTime)
		{
			if (!Visible)
				return;

			foreach (XNAControl child in children)
				child.Draw(gameTime);
			//used for debugging draworders: drawing the DrawOrder variable on each control so i could see what it was dynamically
			//if(dbg == null)
			//	dbg = EncapsulatingGame.Content.Load<SpriteFont>("dbg");
			//SpriteBatch.Begin();
			//SpriteBatch.DrawString(dbg, "" + DrawOrder, new Vector2(DrawAreaWithOffset.X + 3, DrawAreaWithOffset.Y + 3), Color.White);
			//SpriteBatch.End();
			base.Draw(gameTime);
		}
		
		/// <summary>
		/// Hide child controls when the "Visible" parameter is changed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		protected override void OnVisibleChanged(object sender, EventArgs args)
		{
			foreach (XNAControl child in children)
				child.Visible = this.Visible;

			base.OnVisibleChanged(sender, args);
		}
		
		/// <summary>
		/// Change drawing order for all child controls when this control's draw order is changed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		protected override void OnDrawOrderChanged(object sender, EventArgs args)
		{
			foreach(XNAControl child in children)
			{
				child.DrawOrder = this.DrawOrder + 1;
			}

			base.OnDrawOrderChanged(sender, args);
		}

		/// <summary>
		/// Called when a child control is being dragged and the parent should not respond to click drag
		/// Example: scroll bar being dragged within a dialog
		/// </summary>
		/// <param name="suppress"></param>
		protected void SuppressParentClickDrag(bool suppress)
		{
			if (parent != null)
			{
				parent.shouldClickDrag = !suppress;
				parent.SuppressParentClickDrag(suppress);
			}
		}

		/// <summary>
		/// Closes a component, removing it from the list of Game components in encapsulatingGame
		/// </summary>
		public virtual void Close()
		{			
			//remove all children from the game components list too
			foreach (XNAControl child in children)
				child.Close();

			if (Game.Components.Contains(this))
				Game.Components.Remove(this);

			Dispose();
		}
	}
}
