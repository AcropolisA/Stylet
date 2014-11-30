﻿using Moq;
using NUnit.Framework;
using Stylet;
using Stylet.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace StyletUnitTests
{
    [TestFixture, RequiresSTA]
    public class BootstrapperBaseTests
    {
        private class RootViewModel { }

        private class MyBootstrapperBase<T> : BootstrapperBase<T> where T : class
        {
            private IViewManager viewManager;
            private IWindowManager windowManager;

            public MyBootstrapperBase(IViewManager viewManager, IWindowManager windowManager)
            {
                this.viewManager = viewManager;
                this.windowManager = windowManager;

                this.Start();
            }

            public new Application Application
            {
                get { return base.Application; }
            }

            public bool GetInstanceCalled;
            protected override TInstance GetInstance<TInstance>()
            {
                var service = typeof(TInstance);
                this.GetInstanceCalled = true;
                if (service == typeof(IViewManager))
                    return (TInstance)this.viewManager;
                if (service == typeof(IWindowManager))
                    return (TInstance)this.windowManager;
                if (service == typeof(RootViewModel))
                    return (TInstance)(object)new RootViewModel();
                return default(TInstance);
            }

            public bool OnExitCalled;
            protected override void OnApplicationExit(object sender, ExitEventArgs e)
            {
                this.OnExitCalled = true;
            }

            public bool ConfigureCalled;
            protected override void Configure()
            {
                this.ConfigureCalled = true;
                base.Configure();
            }

            public new void Start()
            {
                base.Start();
            }
        }

        
        private MyBootstrapperBase<RootViewModel> bootstrapper;
        private Mock<IViewManager> viewManager;
        private Mock<IWindowManager> windowManager;

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            Execute.TestExecuteSynchronously = true;
            AssemblySource.Assemblies.Clear();
        }

        [SetUp]
        public void SetUp()
        {
            this.viewManager = new Mock<IViewManager>();
            this.windowManager = new Mock<IWindowManager>();
            this.bootstrapper = new MyBootstrapperBase<RootViewModel>(this.viewManager.Object, this.windowManager.Object);
        }

        [Test]
        public void StartAssignsExecuteDispatcher()
        {
            Execute.Dispatcher = null;
            this.bootstrapper.Start();
            Assert.NotNull(Execute.Dispatcher); // Can't test any further, unfortunately
        }

        [Test]
        public void StartSetsUpAssemblySource()
        {
            AssemblySource.Assemblies.Add(null);
            this.bootstrapper.Start();
            Assert.That(AssemblySource.Assemblies, Is.EquivalentTo(new[] { typeof(BootstrapperBase<>).Assembly, this.bootstrapper.GetType().Assembly }));
        }

        [Test]
        public void StartCallsConfigure()
        {
            this.bootstrapper.Start();
            Assert.True(this.bootstrapper.ConfigureCalled);
        }

        [Test]
        public void StartAssignsViewManager()
        {
            this.bootstrapper.Start();
            Assert.AreEqual(View.ViewManager, this.viewManager.Object);
        }
    }
}
