// Original Work Copyright (c) Ethan Moffat 2014-2017
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Linq;
using NUnit.Framework;
using XNAControls.Test.Helpers;

namespace XNAControls.Test.Controls
{
    [TestFixture]
    public class XNAControlTest
    {
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
        }

        [TearDown]
        public void TearDown()
        {
            _control.Dispose();
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
            var child1 = new FakeXNAControl();
            var parent = new FakeXNAControl();

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
            var oldParent = new FakeXNAControl();
            _control.SetParentControl(oldParent);

            var newParent = new FakeXNAControl();
            _control.SetParentControl(newParent);

            Assert.AreNotEqual(oldParent, _control.ImmediateParent);
            Assert.IsFalse(oldParent.ChildControls.Contains(_control));
        }

        [Test]
        public void SetParentControl_NewParentRelationshipIsMade()
        {
            var oldParent = new FakeXNAControl();
            _control.SetParentControl(oldParent);

            var newParent = new FakeXNAControl();
            _control.SetParentControl(newParent);

            Assert.AreEqual(newParent, _control.ImmediateParent);
            Assert.IsTrue(newParent.ChildControls.Contains(_control));
        }

        [Test]
        public void SetParentControl_ControlIsRemovedFromGameComponents()
        {
            _control.AddControlToDefaultGame();
            Assert.IsTrue(_gameManager.Game.Components.Contains(_control));

            var parent = new FakeXNAControl();
            _control.SetParentControl(parent);

            Assert.IsFalse(_gameManager.Game.Components.Contains(_control));
        }

        [Test]
        public void SetParentControl_SetsChildDrawOrderBasedOnParentDrawOrder()
        {
            _control.DrawOrder = 321;
            var parent = new FakeXNAControl {DrawOrder = 123};

            _control.SetParentControl(parent);

            Assert.AreEqual(124, _control.DrawOrder);
        }

        [Test]
        public void SetControlUnparented_BreaksParentRelationship()
        {
            var parent = new FakeXNAControl();
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
            var parent = new FakeXNAControl {DrawOrder = 10};
            var child = new FakeXNAControl();
            var child2 = new FakeXNAControl();

            child.SetParentControl(parent);
            child2.SetParentControl(parent);
            _control.SetParentControl(parent);

            parent.SetDrawOrder(15);

            CollectionAssert.AreEqual(Enumerable.Repeat(16, 3), parent.ChildControls.Select(x => x.DrawOrder));
        }

        [Test]
        public void SetDrawOrder_UpdatesChildControlDrawOrders_InHierarchy()
        {
            var parent = new FakeXNAControl { DrawOrder = 10 };
            var child = new FakeXNAControl();
            var child2 = new FakeXNAControl();
            var nestedChild = new FakeXNAControl();
            var nestedChild2 = new FakeXNAControl();

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
            var parent = new FakeXNAControl();
            _control.SetParentControl(parent);
            Assert.IsTrue(parent.ShouldClickDrag);

            _control.SuppressParentClickDragEvent(true);

            Assert.IsFalse(parent.ShouldClickDrag);
        }

        [Test]
        public void SuppressParentClickDragEvent_SetsParentClickDrag_False_InHierarchy()
        {
            var parent = new FakeXNAControl();
            var child = new FakeXNAControl();
            var child2 = new FakeXNAControl();
            var nestedChild = new FakeXNAControl();
            var nestedChild2 = new FakeXNAControl();

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
    }
}
