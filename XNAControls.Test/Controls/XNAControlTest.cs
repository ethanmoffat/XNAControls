// Original Work Copyright (c) Ethan Moffat 2014-2017
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using NUnit.Framework;
using XNAControls.Test.Helpers;

namespace XNAControls.Test.Controls
{
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
        public void SuppressParentClickDragEvent_SetsParentClickDrag_False()
        {
            var parent = CreateFakeControl();
            _control.SetParentControl(parent);
            Assert.IsTrue(parent.ShouldClickDrag);

            _control.SuppressParentClickDragEvent(true);

            Assert.IsFalse(parent.ShouldClickDrag);
        }

        [Test]
        public void SuppressParentClickDragEvent_SetsParentClickDrag_False_InHierarchy()
        {
            var parent = CreateFakeControl();
            var child = CreateFakeControl();
            var child2 = CreateFakeControl();
            var nestedChild = CreateFakeControl();
            var nestedChild2 = CreateFakeControl();

            nestedChild.SetParentControl(child);
            nestedChild2.SetParentControl(child2);
            child.SetParentControl(parent);
            child2.SetParentControl(parent);
            _control.SetParentControl(parent);

            nestedChild.SuppressParentClickDragEvent(true);

            Assert.IsTrue(nestedChild.ShouldClickDrag);
            Assert.IsTrue(nestedChild2.ShouldClickDrag);
            Assert.IsTrue(child2.ShouldClickDrag);
            Assert.IsTrue(_control.ShouldClickDrag);
            Assert.IsFalse(child.ShouldClickDrag);
            Assert.IsFalse(parent.ShouldClickDrag);
        }

        [Test]
        public void Update_DoesNotUpdateControl_IfGameIsInactive()
        {
            GivenGameIsInactive(_control);

            _control.Update(new GameTime());

            Assert.IsFalse(_control.Updated);
        }

        [Test]
        public void Update_DoesNotUpdateControl_IfControlIsNotVisible()
        {
            GivenGameIsActive(_control);
            _control.Visible = false;

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

        private static void GivenGameIsActive(FakeXNAControl control)
        {
            control.SetIsActive(true);
        }

        private static void GivenGameIsInactive(FakeXNAControl control)
        {
            control.SetIsActive(false);
        }

        private FakeXNAControl CreateFakeControl()
        {
            var control = new FakeXNAControl();
            _createdControls.Add(control);
            return control;
        }
    }
}
