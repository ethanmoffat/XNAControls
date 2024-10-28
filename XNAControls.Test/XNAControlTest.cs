using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Input;
using MonoGame.Extended.Input.InputListeners;
using Moq;
using NUnit.Framework;
using XNAControls.Input;
using XNAControls.Test.Controls;
using XNAControls.Test.Helpers;

namespace XNAControls.Test
{
    //todo: currently it is difficult to test the following
    // 1. Keeping the control within the bounds of the game window (see OnUpdateControl)

    [TestFixture]
    public class XNAControlTest
    {
        private List<IXNAControl> _createdControls;

        private static TestGameManager _gameManager;
        private FakeXNAControl _control;

        [OneTimeSetUp]
        public static void OneTimeSetUp()
        {
            _gameManager = new TestGameManager();
            GameRepository.SetGame(_gameManager.Game);
        }

        [SetUp]
        public void SetUp()
        {
            _control = new FakeXNAControl();
            _createdControls = new List<IXNAControl>();
        }

        [TearDown]
        public void TearDown()
        {
            _control.Dispose();

            foreach (var control in _createdControls)
                control.Dispose();
            _createdControls.Clear();
        }

        [OneTimeTearDown]
        public static void OneTimeTearDown()
        {
            GameRepository.SetGame(null);
            _gameManager.Dispose();
        }

        [Test]
        public void DrawPosition_MatchesDrawArea()
        {
            _control.DrawArea = new Rectangle(50, 100, 200, 400);
            Assert.That(_control.DrawPosition, Is.EqualTo(new Vector2(50, 100)));
        }

        [Test]
        public void DrawArea_MatchesDrawPosition()
        {
            _control.DrawPosition = new Vector2(100, 50);
            Assert.That(_control.DrawArea, Is.EqualTo(new Rectangle(100, 50, 0, 0)));
        }

        [Test]
        public void MouseOver_TrueWhenMouseEnterEventFired()
        {
            GivenControlsCanBeUpdated(_control);

            _control.PostMessage(EventType.MouseEnter, MouseStateExtendedWithPosition(8, 10));

            _control.DrawArea = new Rectangle(5, 5, 10, 10);
            _control.Update(new GameTime());

            Assert.That(_control.MouseOverDuringUpdate, Is.True);
        }

        [Test]
        public void MouseOver_TrueWhenMouseOverEventFired()
        {
            GivenControlsCanBeUpdated(_control);
            _control.PostMessage(EventType.MouseOver, MouseStateExtendedWithPosition(8, 10));

            _control.DrawArea = new Rectangle(5, 5, 10, 10);
            _control.Update(new GameTime());

            Assert.That(_control.MouseOverDuringUpdate, Is.True);
        }

        [Test]
        public void MouseOver_FalseWhenMouseLeaveEventFired()
        {
            GivenControlsCanBeUpdated(_control);
            _control.PostMessage(EventType.MouseOver, MouseStateExtendedWithPosition(8, 10));

            _control.DrawArea = new Rectangle(5, 5, 10, 10);
            _control.Update(new GameTime());

            Assert.That(_control.MouseOverDuringUpdate, Is.True);

            _control.PostMessage(EventType.MouseLeave, MouseStateExtendedWithPosition(11, 10));
            _control.Update(new GameTime());

            Assert.That(_control.MouseOverDuringUpdate, Is.False);
            Assert.That(_control.MouseOverPreviouslyDuringUpdate, Is.True);
        }

        [Test]
        public void MouseOver_TrueWhenMouseEnterEventFired_WithParent()
        {
            var parent = CreateFakeControl();
            GivenControlsCanBeUpdated(_control, parent);

            _control.PostMessage(EventType.MouseEnter, MouseStateExtendedWithPosition(105, 103));

            _control.SetParentControl(parent);
            _control.DrawArea = new Rectangle(5, 5, 10, 10);
            parent.DrawArea = new Rectangle(95, 95, 10, 10);

            _control.DrawArea = new Rectangle(5, 5, 10, 10);
            _control.Update(new GameTime());

            Assert.That(_control.MouseOverDuringUpdate, Is.True);
        }

        [Test]
        public void MouseOver_TrueWhenMouseOverEventFired_WithParent()
        {
            var parent = CreateFakeControl();
            GivenControlsCanBeUpdated(_control, parent);

            _control.PostMessage(EventType.MouseOver, MouseStateExtendedWithPosition(105, 103));

            _control.SetParentControl(parent);
            _control.DrawArea = new Rectangle(5, 5, 10, 10);
            parent.DrawArea = new Rectangle(95, 95, 10, 10);

            _control.Update(new GameTime());

            Assert.That(_control.MouseOverDuringUpdate, Is.True);
        }

        [Test]
        public void MouseOver_FalseWhenMouseLeaveEventFired_WithParent()
        {
            var parent = CreateFakeControl();
            GivenControlsCanBeUpdated(_control, parent);

            _control.PostMessage(EventType.MouseOver, MouseStateExtendedWithPosition(105, 103));

            _control.SetParentControl(parent);
            _control.DrawArea = new Rectangle(5, 5, 10, 10);
            parent.DrawArea = new Rectangle(95, 95, 10, 10);

            _control.Update(new GameTime());

            Assert.That(_control.MouseOverDuringUpdate, Is.True);

            _control.PostMessage(EventType.MouseLeave, MouseStateExtendedWithPosition(11, 10));
            _control.Update(new GameTime());

            Assert.That(_control.MouseOverDuringUpdate, Is.False);
            Assert.That(_control.MouseOverPreviouslyDuringUpdate, Is.True);
        }

        [Test]
        public void DrawPositionWithParentOffset_OffsetsBasedOnParentDrawPosition()
        {
            var parent = CreateFakeControl();
            parent.DrawPosition = new Vector2(100, 100);

            _control.SetParentControl(parent);
            _control.DrawPosition = new Vector2(100, 100);

            Assert.That(_control.DrawPositionWithParentOffset, Is.EqualTo(new Vector2(200, 200)));
        }

        [Test]
        public void DrawAreaWithParentOffset_OffsetsBasedOnParentDrawPosition()
        {
            var parent = CreateFakeControl();
            parent.DrawPosition = new Vector2(100, 100);

            _control.SetParentControl(parent);
            _control.DrawArea = new Rectangle(100, 100, 200, 200);

            Assert.That(_control.DrawAreaWithParentOffset, Is.EqualTo(new Rectangle(200, 200, 200, 200)));
        }

        [Test]
        public void DrawPositionWithParentOffset_IsUpdated_WhenParentControlIsSet()
        {
            var parent = CreateFakeControl();
            parent.DrawPosition = new Vector2(50, 50);

            _control.DrawPosition = new Vector2(100, 100);
            _control.SetParentControl(parent);

            Assert.That(_control.DrawPositionWithParentOffset, Is.EqualTo(new Vector2(150, 150)));
        }

        [Test]
        public void DrawAreaWithParentOffset_IsUpdated_WhenParentControlIsSet()
        {
            var parent = CreateFakeControl();
            parent.DrawArea = new Rectangle(50, 50, 100, 100);

            _control.DrawArea = new Rectangle(150, 150, 200, 200);
            _control.SetParentControl(parent);

            Assert.That(_control.DrawAreaWithParentOffset, Is.EqualTo(new Rectangle(200, 200, 200, 200)));
        }

        [Test]
        public void TopParent_IsNull_WhenNoParent()
        {
            Assert.That(_control.TopParent, Is.Null);
        }

        [Test]
        public void TopParent_IsImmediateParent_WhenImmediateParentHasNoParent()
        {
            var parent = CreateFakeControl();
            _control.SetParentControl(parent);
            Assert.That(_control.TopParent, Is.EqualTo(parent));
        }

        [Test]
        public void TopParent_IsHighestParentInHierarchy_WhenImmediateParentHasParent()
        {
            var topParent = CreateFakeControl();
            var nextParent = CreateFakeControl();
            var parent = CreateFakeControl();

            _control.SetParentControl(parent);
            parent.SetParentControl(nextParent);
            nextParent.SetParentControl(topParent);

            Assert.That(_control.TopParent, Is.EqualTo(topParent));
        }

        [Test]
        public void AddToDefaultGame_AddsControlToDefaultGame()
        {
            _control.AddControlToDefaultGame();
            Assert.That(_gameManager.Game.Components.Single(), Is.EqualTo(_control));
        }

        [Test]
        public void AddToDefaultGame_DoesNotAddControlToDefaultGame_WhenControlIsAlreadyInComponents()
        {
            _gameManager.Game.Components.Add(_control);
            _control.AddControlToDefaultGame();
        }

        [Test]
        public void AddToDefaultGame_UnsetsParentControl()
        {
            var child1 = CreateFakeControl();
            var parent = CreateFakeControl();

            child1.SetParentControl(parent);
            _control.SetParentControl(parent);

            _control.AddControlToDefaultGame();

            Assert.That(parent.ChildControls, Is.EqualTo(new[] {child1}));
            Assert.That(_control.ImmediateParent, Is.Null);
        }

        [Test]
        public void SetParentControl_ParentIsNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _control.SetParentControl(null));
        }

        [Test]
        public void SetParentControl_ParentIsSelf_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => _control.SetParentControl(_control));
        }

        [Test]
        public void SetParentControl_ExistingParentRelationshipIsBroken()
        {
            var oldParent = CreateFakeControl();
            _control.SetParentControl(oldParent);

            var newParent = CreateFakeControl();
            _control.SetParentControl(newParent);

            Assert.That(_control.ImmediateParent, Is.Not.EqualTo(oldParent));
            Assert.That(oldParent.ChildControls.Contains(_control), Is.False);
        }

        [Test]
        public void SetParentControl_NewParentRelationshipIsMade()
        {
            var oldParent = CreateFakeControl();
            _control.SetParentControl(oldParent);

            var newParent = CreateFakeControl();
            _control.SetParentControl(newParent);

            Assert.That(_control.ImmediateParent, Is.EqualTo(newParent));
            Assert.That(newParent.ChildControls.Contains(_control), Is.True);
        }

        [Test]
        public void SetParentControl_ControlIsRemovedFromGameComponents()
        {
            _control.AddControlToDefaultGame();
            Assert.That(_gameManager.Game.Components.Contains(_control), Is.True);

            var parent = CreateFakeControl();
            _control.SetParentControl(parent);

            Assert.That(_gameManager.Game.Components.Contains(_control), Is.False);
        }

        [Test]
        public void SetParentControl_SetsChildDrawOrderBasedOnParentDrawOrder()
        {
            _control.DrawOrder = 321;
            var parent = CreateFakeControl();
            parent.DrawOrder = 123;

            _control.SetParentControl(parent);

            Assert.That(_control.DrawOrder, Is.EqualTo(124));
        }

        [Test]
        public void SetControlUnparented_BreaksParentRelationship()
        {
            var parent = CreateFakeControl();
            _control.SetParentControl(parent);
            Assert.That(_control.ImmediateParent, Is.EqualTo(parent));
            Assert.That(parent.ChildControls.Contains(_control), Is.True);

            _control.SetControlUnparented();
            Assert.That(_control.ImmediateParent, Is.Null);
            Assert.That(parent.ChildControls.Contains(_control), Is.False);
        }

        [Test]
        public void SetDrawOrder_UpdatesDrawOrder()
        {
            _control.DrawOrder = 11;
            _control.SetDrawOrder(12);
            Assert.That(_control.DrawOrder, Is.EqualTo(12));
        }

        [Test]
        public void SetDrawOrder_UpdatesChildControlDrawOrders()
        {
            var parent = CreateFakeControl();
            parent.DrawOrder = 10;

            var child = CreateFakeControl();
            var child2 = CreateFakeControl();

            child.SetParentControl(parent);
            child2.SetParentControl(parent);
            _control.SetParentControl(parent);

            parent.SetDrawOrder(15);

            Assert.That(parent.ChildControls.Select(x => x.DrawOrder), Has.Exactly(3).Items.EqualTo(16));
        }

        [Test]
        public void SetDrawOrder_UpdatesChildControlDrawOrders_InHierarchy()
        {
            var parent = CreateFakeControl();
            parent.DrawOrder = 10;

            var child = CreateFakeControl();
            var child2 = CreateFakeControl();
            var nestedChild = CreateFakeControl();
            var nestedChild2 = CreateFakeControl();

            nestedChild.SetParentControl(child);
            nestedChild2.SetParentControl(child2);
            child.SetParentControl(parent);
            child2.SetParentControl(parent);
            _control.SetParentControl(parent);

            parent.SetDrawOrder(15);

            Assert.That(child.DrawOrder, Is.EqualTo(16));
            Assert.That(child2.DrawOrder, Is.EqualTo(16));
            Assert.That(_control.DrawOrder, Is.EqualTo(16));
            Assert.That(nestedChild.DrawOrder, Is.EqualTo(17));
            Assert.That(nestedChild2.DrawOrder, Is.EqualTo(17));
        }

        [Test]
        public void Update_DoesNotUpdateControl_IfGameIsInactive()
        {
            GivenGameIsInactive(_control);

            _control.Update(new GameTime());
            _control.ResetUpdated();
            _control.Update(new GameTime());

            Assert.That(_control.Updated, Is.False);
        }

        [Test]
        public void Update_DoesNotUpdateControl_IfControlIsNotVisible()
        {
            GivenGameIsActive(_control);
            _control.Visible = false;

            _control.Update(new GameTime());
            _control.ResetUpdated();
            _control.Update(new GameTime());

            Assert.That(_control.Updated, Is.False);
        }

        [Test]
        public void Update_DoesNotUpdateControl_IfControlIsDisposed()
        {
            GivenGameIsActive(_control);
            _control.Visible = true;
            _control.Dispose();

            _control.Update(new GameTime());

            Assert.That(_control.Updated, Is.False);
        }

        [Test]
        public void Update_UpdatesControl_WhenGameActive_AndControlVisible_AndNotDisposed()
        {
            GivenGameIsActive(_control);
            _control.Visible = true;

            _control.Update(new GameTime());

            Assert.That(_control.Updated, Is.True);
        }

        [Test]
        public void Update_UpdatesChildren()
        {
            var child = CreateFakeControl();
            var child2 = CreateFakeControl();

            child.SetParentControl(_control);
            child2.SetParentControl(_control);

            GivenGameIsActive(_control, child, child2);
            _control.Visible = true;
            child.Visible = true;
            child2.Visible = true;

            _control.Update(new GameTime());

            Assert.That(child.Updated, Is.True);
            Assert.That(child2.Updated, Is.True);
        }

        [Test]
        public void Draw_DoesNotDrawControl_IfControlIsNotVisible()
        {
            _control.Visible = false;

            _control.Draw(new GameTime());

            Assert.That(_control.Drawn, Is.False);
        }

        [Test]
        public void Draw_DoesNotDrawControl_IfControlIsDisposed()
        {
            _control.Visible = true;
            _control.Dispose();

            _control.Draw(new GameTime());

            Assert.That(_control.Drawn, Is.False);
        }

        [Test]
        public void Draw_DrawsControl_WhenGameActive_AndControlVisible_AndNotDisposed()
        {
            GivenGameIsActive(_control);
            _control.Visible = true;

            _control.Draw(new GameTime());

            Assert.That(_control.Drawn, Is.True);
        }

        [Test]
        public void Draw_DrawsControl_WhenGameIsInActive()
        {
            GivenGameIsInactive(_control);
            _control.Visible = true;

            _control.Draw(new GameTime());

            Assert.That(_control.Drawn, Is.True);
        }

        [Test]
        public void Update_DrawsChildren()
        {
            var child = CreateFakeControl();
            var child2 = CreateFakeControl();

            child.SetParentControl(_control);
            child2.SetParentControl(_control);

            _control.Visible = true;
            child.Visible = true;
            child2.Visible = true;

            _control.Draw(new GameTime());

            Assert.That(child.Drawn, Is.True);
            Assert.That(child2.Drawn, Is.True);
        }

        [Test]
        public void Dispose_SetsValueForDisposed_ToTrue()
        {
            Assert.That(_control.IsDisposed, Is.False);

            _control.Dispose();

            Assert.That(_control.IsDisposed, Is.True);
        }

        [Test]
        public void Dispose_RemovesControlFromGameComponents()
        {
            _control.AddControlToDefaultGame();
            Assert.That(_gameManager.Game.Components.Contains(_control), Is.True);

            _control.Dispose();
            Assert.That(_gameManager.Game.Components.Contains(_control), Is.False);
        }

        [Test]
        public void Dispose_DisposesChildren()
        {
            var child1 = CreateFakeControl();
            var child2 = CreateFakeControl();

            child1.SetParentControl(_control);
            child2.SetParentControl(_control);

            _control.Dispose();

            Assert.That(child1.IsDisposed, Is.True);
            Assert.That(child2.IsDisposed, Is.True);
        }

        private static void GivenGameIsActive(params FakeXNAControl[] controls)
        {
            foreach (var control in controls)
                control.SetIsActive(true);
        }

        private static void GivenGameIsInactive(params FakeXNAControl[] controls)
        {
            foreach (var control in controls)
                control.SetIsActive(false);
        }

        private void GivenControlsCanBeUpdated(params FakeXNAControl[] controls)
        {
            GivenGameIsActive(controls);
            foreach (var control in controls)
                control.Visible = true;
        }

        private FakeXNAControl CreateFakeControl()
        {
            var control = new FakeXNAControl();
            _createdControls.Add(control);
            return control;
        }

        private static MouseStateExtended MouseStateExtendedWithPosition(int x, int y)
        {
            var ms = new MouseState(x, y, 0, ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released);
            return new MouseStateExtended(ms, ms);
        }
    }
}
