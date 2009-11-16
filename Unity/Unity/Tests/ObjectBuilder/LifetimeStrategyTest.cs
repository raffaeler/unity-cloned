﻿//===============================================================================
// Microsoft patterns & practices
// Unity Application Block
//===============================================================================
// Copyright © Microsoft Corporation.  All rights reserved.
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY
// OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
// FITNESS FOR A PARTICULAR PURPOSE.
//===============================================================================

using Microsoft.Practices.ObjectBuilder2.Tests.Utility;
using Microsoft.Practices.Unity.TestSupport;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Practices.ObjectBuilder2.Tests
{
    [TestClass]
    public class LifetimeStrategyTest
    {
        [TestMethod]
        public void LifetimeStrategyDefaultsToTransient()
        {
            MockBuilderContext context = CreateContext();
            var key = new NamedTypeBuildKey<object>();
            object result = context.ExecuteBuildUp(key, null);
            object result2 = context.ExecuteBuildUp(key, null);
            Assert.IsNotNull(result);
            Assert.IsNotNull(result2);
            Assert.AreNotSame(result, result2);
        }

        [TestMethod]
        public void SingletonPolicyAffectsLifetime()
        {
            MockBuilderContext context = CreateContext();
            var key = new NamedTypeBuildKey<object>();
            context.Policies.Set<ILifetimePolicy>(new SingletonLifetimePolicy(), key);

            object result = context.ExecuteBuildUp(key, null);
            object result2 = context.ExecuteBuildUp(key, null);
            Assert.IsNotNull(result);
            Assert.AreSame(result, result2);
        }

        [TestMethod]
        public void LifetimeStrategyAddsRecoveriesToContext()
        {
            MockBuilderContext context = CreateContext();
            var key = new NamedTypeBuildKey<object>();
            RecoverableLifetime recovery = new RecoverableLifetime();
            context.PersistentPolicies.Set<ILifetimePolicy>(recovery, key);

            context.ExecuteBuildUp(key, null);

            Assert.AreEqual(1, context.RecoveryStack.Count);

            context.RecoveryStack.ExecuteRecovery();
            Assert.IsTrue(recovery.WasRecovered);
        }

        [TestMethod]
        public void LifetimeStrategyUsesFactoryToGetLifetimePolicyForGenericType()
        {
            MockBuilderContext context = CreateContext();
            var openKey = new NamedTypeBuildKey(typeof (MyFoo<>));
            context.PersistentPolicies.Set<ILifetimeFactoryPolicy>(
                new LifetimeFactoryPolicy<RecoverableLifetime>(), openKey);

            context.ExecuteBuildUp(new NamedTypeBuildKey<MyFoo<string>>(), null);
            MyFoo<string> stringFoo = (MyFoo<string>)context.Existing;

            context.ExecuteBuildUp(new NamedTypeBuildKey<MyFoo<int>>(), null);
            MyFoo<int> intFoo = (MyFoo<int>)context.Existing;

            ILifetimePolicy stringLifetime =
                context.Policies.GetNoDefault<ILifetimePolicy>(new NamedTypeBuildKey(typeof(MyFoo<string>)), false);
            ILifetimePolicy intLifetime =
                context.Policies.GetNoDefault<ILifetimePolicy>(new NamedTypeBuildKey(typeof(MyFoo<int>)), false);

            Assert.IsNotNull(stringLifetime);
            Assert.IsNotNull(intLifetime);
            Assert.IsInstanceOfType(stringLifetime, typeof(RecoverableLifetime));
            Assert.IsInstanceOfType(intLifetime, typeof(RecoverableLifetime));
            Assert.AreNotSame(stringLifetime, intLifetime);
        }
        
        private MockBuilderContext CreateContext()
        {
            MockBuilderContext context = new MockBuilderContext();
            context.Strategies.Add(new LifetimeStrategy());
            context.Strategies.Add(new ActivatorCreationStrategy());
            return context;
        }

        private class RecoverableLifetime : ILifetimePolicy, IRequiresRecovery
        {
            public bool WasRecovered = false;

            public object GetValue()
            {
                return null;
            }

            public void SetValue(object newValue)
            {
            }

            public void RemoveValue()
            {
            }

            public void Recover()
            {
                WasRecovered = true;
            }
        }

        public interface IFoo<T>
        {
            
        }

        public class MyFoo<T> : IFoo<T>
        {
            
        }

        class LifetimeFactoryPolicy<T> : ILifetimeFactoryPolicy 
            where T : ILifetimePolicy, new()
        {
            public ILifetimePolicy CreateLifetimePolicy()
            {
                return new T();
            }
        }
    }
}