// Original Work Copyright (c) Ethan Moffat 2014-2017
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Linq;
using Moq;
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
    }
}
