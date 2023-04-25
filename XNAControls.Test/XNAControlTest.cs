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
            Assert.AreEqual(new Vector2(50, 100), _control.DrawPosition);
        }

        [Test]
        public void DrawArea_MatchesDrawPosition()
        {
            _control.DrawPosition = new Vector2(100, 50);
            Assert.AreEqual(new Rectangle(100, 50, 0, 0), _control.DrawArea);
        }

        [Test]
        public void MouseOver_TrueWhenMouseEnterEventFired()
        {
            GivenControlsCanBeUpdated(_control);

            _control.PostMessage(EventType.MouseEnter, MouseStateExtendedWithPosition(8, 10));

            _control.DrawArea = new Rectangle(5, 5, 10, 10);
            _control.Update(new GameTime());

            Assert.IsTrue(_control.MouseOverDuringUpdate);
        }

        [Test]
        public void MouseOver_TrueWhenMouseOverEventFired()
        {
            GivenControlsCanBeUpdated(_control);
            _control.PostMessage(EventType.MouseOver, MouseStateExtendedWithPosition(8, 10));

            _control.DrawArea = new Rectangle(5, 5, 10, 10);
            _control.Update(new GameTime());

            Assert.IsTrue(_control.MouseOverDuringUpdate);
        }

        [Test]
        public void MouseOver_FalseWhenMouseLeaveEventFired()
        {
            GivenControlsCanBeUpdated(_control);
            _control.PostMessage(EventType.MouseOver, MouseStateExtendedWithPosition(8, 10));

            _control.DrawArea = new Rectangle(5, 5, 10, 10);
            _control.Update(new GameTime());

            Assert.IsTrue(_control.MouseOverDuringUpdate);

            _control.PostMessage(EventType.MouseLeave, MouseStateExtendedWithPosition(11, 10));
            _control.Update(new GameTime());

            Assert.IsFalse(_control.MouseOverDuringUpdate);
            Assert.IsTrue(_control.MouseOverPreviouslyDuringUpdate);
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

            Assert.IsTrue(_control.MouseOverDuringUpdate);
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

            Assert.IsTrue(_control.MouseOverDuringUpdate);
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

            Assert.IsTrue(_control.MouseOverDuringUpdate);

            _control.PostMessage(EventType.MouseLeave, MouseStateExtendedWithPosition(11, 10));
            _control.Update(new GameTime());

            Assert.IsFalse(_control.MouseOverDuringUpdate);
            Assert.IsTrue(_control.MouseOverPreviouslyDuringUpdate);
        }

        [Test]
        public void DrawPositionWithParentOffset_OffsetsBasedOnParentDrawPosition()
        {
            var parent = CreateFakeControl();
            parent.DrawPosition = new Vector2(100, 100);

            _control.SetParentControl(parent);
            _control.DrawPosition = new Vector2(100, 100);

            Assert.AreEqual(new Vector2(200, 200), _control.DrawPositionWithParentOffset);
        }

        [Test]
        public void DrawAreaWithParentOffset_OffsetsBasedOnParentDrawPosition()
        {
            var parent = CreateFakeControl();
            parent.DrawPosition = new Vector2(100, 100);

            _control.SetParentControl(parent);
            _control.DrawArea = new Rectangle(100, 100, 200, 200);

            Assert.AreEqual(new Rectangle(200, 200, 200, 200), _control.DrawAreaWithParentOffset);
        }

        [Test]
        public void DrawPositionWithParentOffset_IsUpdated_WhenParentControlIsSet()
        {
            var parent = CreateFakeControl();
            parent.DrawPosition = new Vector2(50, 50);

            _control.DrawPosition = new Vector2(100, 100);
            _control.SetParentControl(parent);

            Assert.AreEqual(new Vector2(150, 150), _control.DrawPositionWithParentOffset);
        }

        [Test]
        public void DrawAreaWithParentOffset_IsUpdated_WhenParentControlIsSet()
        {
            var parent = CreateFakeControl();
            parent.DrawArea = new Rectangle(50, 50, 100, 100);

            _control.DrawArea = new Rectangle(150, 150, 200, 200);
            _control.SetParentControl(parent);

            Assert.AreEqual(new Rectangle(200, 200, 200, 200), _control.DrawAreaWithParentOffset);
        }

        [Test]
        public void TopParent_IsNull_WhenNoParent()
        {
            Assert.IsNull(_control.TopParent);
        }

        [Test]
        public void TopParent_IsImmediateParent_WhenImmediateParentHasNoParent()
        {
            var parent = CreateFakeControl();
            _control.SetParentControl(parent);
            Assert.AreEqual(parent, _control.TopParent);
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

            Assert.AreEqual(topParent, _control.TopParent);
        }

        [Test]
        public void AddToDefaultGame_AddsControlToDefaultGame()
        {
            _control.AddControlToDefaultGame();
            Assert.AreEqual(_control, _gameManager.Game.Components.Single());
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

            CollectionAssert.AreEqual(new[] {child1}, parent.ChildControls);
            Assert.IsNull(_control.ImmediateParent);
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

            Assert.AreNotEqual(oldParent, _control.ImmediateParent);
            Assert.IsFalse(oldParent.ChildControls.Contains(_control));
        }

        [Test]
        public void SetParentControl_NewParentRelationshipIsMade()
        {
            var oldParent = CreateFakeControl();
            _control.SetParentControl(oldParent);

            var newParent = CreateFakeControl();
            _control.SetParentControl(newParent);

            Assert.AreEqual(newParent, _control.ImmediateParent);
            Assert.IsTrue(newParent.ChildControls.Contains(_control));
        }

        [Test]
        public void SetParentControl_ControlIsRemovedFromGameComponents()
        {
            _control.AddControlToDefaultGame();
            Assert.IsTrue(_gameManager.Game.Components.Contains(_control));

            var parent = CreateFakeControl();
            _control.SetParentControl(parent);

            Assert.IsFalse(_gameManager.Game.Components.Contains(_control));
        }

        [Test]
        public void SetParentControl_SetsChildDrawOrderBasedOnParentDrawOrder()
        {
            _control.DrawOrder = 321;
            var parent = CreateFakeControl();
            parent.DrawOrder = 123;

            _control.SetParentControl(parent);

            Assert.AreEqual(124, _control.DrawOrder);
        }

        [Test]
        public void SetControlUnparented_BreaksParentRelationship()
        {
            var parent = CreateFakeControl();
            _control.SetParentControl(parent);
            Assert.AreEqual(parent, _control.ImmediateParent);
            Assert.IsTrue(parent.ChildControls.Contains(_control));

            _control.SetControlUnparented();
            Assert.IsNull(_control.ImmediateParent);
            Assert.IsFalse(parent.ChildControls.Contains(_control));
        }

        [Test]
        public void SetDrawOrder_UpdatesDrawOrder()
        {
            _control.DrawOrder = 11;
            _control.SetDrawOrder(12);
            Assert.AreEqual(12, _control.DrawOrder);
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

            CollectionAssert.AreEqual(Enumerable.Repeat(16, 3), parent.ChildControls.Select(x => x.DrawOrder));
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

            Assert.AreEqual(16, child.DrawOrder);
            Assert.AreEqual(16, child2.DrawOrder);
            Assert.AreEqual(16, _control.DrawOrder);
            Assert.AreEqual(17, nestedChild.DrawOrder);
            Assert.AreEqual(17, nestedChild2.DrawOrder);
        }

        [Test]
        public void Update_DoesNotUpdateControl_IfGameIsInactive()
        {
            GivenGameIsInactive(_control);

            _control.Update(new GameTime());
            _control.ResetUpdated();
            _control.Update(new GameTime());

            Assert.IsFalse(_control.Updated);
        }

        [Test]
        public void Update_DoesNotUpdateControl_IfControlIsNotVisible()
        {
            GivenGameIsActive(_control);
            _control.Visible = false;

            _control.Update(new GameTime());
            _control.ResetUpdated();
            _control.Update(new GameTime());

            Assert.IsFalse(_control.Updated);
        }

        [Test]
        public void Update_DoesNotUpdateControl_IfControlIsDisposed()
        {
            GivenGameIsActive(_control);
            _control.Visible = true;
            _control.Dispose();

            _control.Update(new GameTime());

            Assert.IsFalse(_control.Updated);
        }

        [Test]
        public void Update_UpdatesControl_WhenGameActive_AndControlVisible_AndNotDisposed()
        {
            GivenGameIsActive(_control);
            _control.Visible = true;

            _control.Update(new GameTime());

            Assert.IsTrue(_control.Updated);
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

            Assert.IsTrue(child.Updated);
            Assert.IsTrue(child2.Updated);
        }

        [Test]
        public void Draw_DoesNotDrawControl_IfControlIsNotVisible()
        {
            _control.Visible = false;

            _control.Draw(new GameTime());

            Assert.IsFalse(_control.Drawn);
        }

        [Test]
        public void Draw_DoesNotDrawControl_IfControlIsDisposed()
        {
            _control.Visible = true;
            _control.Dispose();

            _control.Draw(new GameTime());

            Assert.IsFalse(_control.Drawn);
        }

        [Test]
        public void Draw_DrawsControl_WhenGameActive_AndControlVisible_AndNotDisposed()
        {
            GivenGameIsActive(_control);
            _control.Visible = true;

            _control.Draw(new GameTime());

            Assert.IsTrue(_control.Drawn);
        }

        [Test]
        public void Draw_DrawsControl_WhenGameIsInActive()
        {
            GivenGameIsInactive(_control);
            _control.Visible = true;

            _control.Draw(new GameTime());

            Assert.IsTrue(_control.Drawn);
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

            Assert.IsTrue(child.Drawn);
            Assert.IsTrue(child2.Drawn);
        }

        [Test]
        public void Dispose_SetsValueForDisposed_ToTrue()
        {
            Assert.IsFalse(_control.IsDisposed);

            _control.Dispose();

            Assert.IsTrue(_control.IsDisposed);
        }

        [Test]
        public void Dispose_RemovesControlFromGameComponents()
        {
            _control.AddControlToDefaultGame();
            Assert.IsTrue(_gameManager.Game.Components.Contains(_control));

            _control.Dispose();
            Assert.IsFalse(_gameManager.Game.Components.Contains(_control));
        }

        [Test]
        public void Dispose_DisposesChildren()
        {
            var child1 = CreateFakeControl();
            var child2 = CreateFakeControl();

            child1.SetParentControl(_control);
            child2.SetParentControl(_control);

            _control.Dispose();

            Assert.IsTrue(child1.IsDisposed);
            Assert.IsTrue(child2.IsDisposed);
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
