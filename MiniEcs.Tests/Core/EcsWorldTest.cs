using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MiniEcs.Core;

namespace MiniEcs.Tests.Core
{
 
    public class ComponentA : IEcsComponent
    {
    }

    public class ComponentB : IEcsComponent
    {
    }

    public class ComponentC : IEcsComponent
    {
    }

    public class ComponentD : IEcsComponent
    {
    }
    
    [TestClass]
    public class EcsWorldTest
    {
        private static EcsWorld _world;
        private static EcsEntity _entityAB;
        private static EcsEntity _entityABD;
        private static EcsEntity _entityAC;
        private static EcsEntity _entityAD;
        private static EcsEntity _entityBC;
        private static EcsEntity _entityBD0;
        private static EcsEntity _entityBD1;

        [ClassInitialize]
        public static void InitFilterWorld(TestContext testContext)
        {
            _world = new EcsWorld();

            _entityABD = _world.CreateEntity(new ComponentA(), new ComponentB(), new ComponentD());
            _entityAC = _world.CreateEntity(new ComponentA(), new ComponentC());
            _entityBD0 = _world.CreateEntity(new ComponentB(), new ComponentD());
            _entityBD1 = _world.CreateEntity(new ComponentD(), new ComponentB());
            _entityBC = _world.CreateEntity(new ComponentC(), new ComponentB());
            _entityAB = _world.CreateEntity(new ComponentB(), new ComponentA());
            _entityAD = _world.CreateEntity(new ComponentA(), new ComponentD());

        }

        [TestMethod]
        public void ArchetypeCountTest()
        {            
            /*
             *  Archetypes
             *  ----------
             *  1.  Empty
             *  2.  A
             *  3.  AB
             *  4.  ABD
             *  5.  AC
             *  6.  B
             *  7.  BD
             *  8.  D
             *  9.  C
             *  10. CB
             *  11. AD
             */
            
            Assert.AreEqual(11, _world.ArchetypeCount);
        }


        [TestMethod]
        public void GetSetRemoveComponentTest()
        {
            EcsWorld world = new EcsWorld();
            
            ComponentB componentB = new ComponentB();
            EcsEntity entity = world.CreateEntity();
            entity.AddComponent(new ComponentA());
            entity.AddComponent(componentB);
            entity.AddComponent(new ComponentC());

            Assert.IsNotNull(entity.GetComponent<ComponentA>());
            Assert.IsNotNull(entity.GetComponent<ComponentC>());
            Assert.AreEqual(componentB, entity.GetComponent<ComponentB>());
            Assert.IsFalse(entity.HasComponent<ComponentD>());

            entity.RemoveComponent<ComponentB>();
            Assert.IsFalse(entity.HasComponent<ComponentB>());
        }

        [TestMethod]
        public void AllFilterTest()
        {
            IEcsGroup group = _world.Filter(new EcsFilter().AllOf<ComponentB>());
            List<EcsEntity> entities = group.ToList();
            Assert.AreEqual(5, entities.Count);
            Assert.AreEqual(5, group.CalculateCount());
            
            Assert.IsTrue(entities.Contains(_entityABD));
            Assert.IsTrue(entities.Contains(_entityBD0));
            Assert.IsTrue(entities.Contains(_entityBD1));
            Assert.IsTrue(entities.Contains(_entityBC));
            Assert.IsTrue(entities.Contains(_entityAB));

            group = _world.Filter(new EcsFilter().AllOf<ComponentB, ComponentD>());
            entities = group.ToList();
            Assert.AreEqual(3, group.CalculateCount());
            Assert.AreEqual(3, entities.Count);
            
            Assert.IsTrue(entities.Contains(_entityABD));
            Assert.IsTrue(entities.Contains(_entityBD0));
            Assert.IsTrue(entities.Contains(_entityBD1));
            
        }

        [TestMethod]
        public void AnyFilterTest()
        {
            IEcsGroup group = _world.Filter(new EcsFilter().AllOf<ComponentB>());
            List<EcsEntity> entities = group.ToList();
            Assert.AreEqual(5, entities.Count);
            Assert.AreEqual(5, group.CalculateCount());
            
            Assert.IsTrue(entities.Contains(_entityABD));
            Assert.IsTrue(entities.Contains(_entityBD0));
            Assert.IsTrue(entities.Contains(_entityBD1));
            Assert.IsTrue(entities.Contains(_entityBC));
            Assert.IsTrue(entities.Contains(_entityAB));

            group = _world.Filter(new EcsFilter().AnyOf<ComponentB, ComponentD>());
            entities = group.ToList();
            Assert.AreEqual(6, entities.Count);
            Assert.AreEqual(6, group.CalculateCount());
            
            Assert.IsTrue(entities.Contains(_entityABD));
            Assert.IsTrue(entities.Contains(_entityBD0));
            Assert.IsTrue(entities.Contains(_entityBD1));
            Assert.IsTrue(entities.Contains(_entityBC));
            Assert.IsTrue(entities.Contains(_entityAB));
            Assert.IsTrue(entities.Contains(_entityAD));
        }
        
        [TestMethod]
        public void NoneFilterTest()
        {
            List<EcsEntity> entities = _world.Filter(new EcsFilter().NoneOf<ComponentB, ComponentD>()).ToList();
            Assert.AreEqual(1, entities.Count);
            
            Assert.IsTrue(entities.Contains(_entityAC));
            
            entities = _world.Filter(new EcsFilter().NoneOf<ComponentB, ComponentD, ComponentB>()).ToList();
            Assert.AreEqual(1, entities.Count);
            
            Assert.IsTrue(entities.Contains(_entityAC));
        }
      
        
        [TestMethod]
        public void AllAnyFilterTest()
        {
            List<EcsEntity> entities = _world.Filter(new EcsFilter().AllOf<ComponentB, ComponentB, ComponentD>().AnyOf<ComponentA>()).ToList();
            Assert.AreEqual(1, entities.Count);
            Assert.IsTrue(entities.Contains(_entityABD));
            
            entities = _world.Filter(new EcsFilter().AllOf<ComponentD, ComponentD>().AnyOf<ComponentB, ComponentC, ComponentC>()).ToList();
            Assert.AreEqual(3, entities.Count);
            
            Assert.IsTrue(entities.Contains(_entityABD));
            Assert.IsTrue(entities.Contains(_entityBD0));
            Assert.IsTrue(entities.Contains(_entityBD1));
        }
        
  
        [TestMethod]
        public void AllNoneFilterTest()
        {
            List<EcsEntity> entities = _world.Filter(new EcsFilter().AllOf<ComponentB>().NoneOf<ComponentA>()).ToList();
            Assert.AreEqual(3, entities.Count);
            
            Assert.IsTrue(entities.Contains(_entityBD0));
            Assert.IsTrue(entities.Contains(_entityBD1));
            Assert.IsTrue(entities.Contains(_entityBC));
            
            entities = _world.Filter(new EcsFilter().AllOf<ComponentB, ComponentD>().NoneOf<ComponentA>()).ToList();
            Assert.AreEqual(2, entities.Count);
            
            Assert.IsTrue(entities.Contains(_entityBD0));
            Assert.IsTrue(entities.Contains(_entityBD1));
        }
        
        [TestMethod]
        public void GroupIncVersionFilterTest()
        {
            EcsWorld world = new EcsWorld();            
            EcsEntity entity = world.CreateEntity(new ComponentA(), new ComponentB());

            List<EcsEntity> entities = world.Filter(new EcsFilter().AllOf<ComponentB>()).ToList();
            Assert.AreEqual(1, entities.Count);
            
            entity.AddComponent(new ComponentC());
            
            world.CreateEntity(new ComponentC(), new ComponentD());
            
            entities = world.Filter(new EcsFilter().AllOf<ComponentB>()).ToList();
            Assert.AreEqual(1, entities.Count);
        }

        
        [TestMethod]
        public void GetOrCreateSingletonTest()
        {
            EcsWorld world = new EcsWorld();

            ComponentA componentA0 = world.GetOrCreateSingleton<ComponentA>();
            ComponentA componentA1 = world.GetOrCreateSingleton<ComponentA>();
            
            Assert.AreEqual(componentA0, componentA1);
        }

        [TestMethod]
        public void CreateEntityFromProcessingTest()
        {
            EcsWorld world = new EcsWorld();
            Assert.AreEqual(0, world.EntitiesInProcessing);

            EcsEntity entity = world.CreateEntity(new ComponentA());
            uint entityId = entity.Id;
            entity.Destroy();
            
            Assert.AreEqual(1, world.EntitiesInProcessing);
            EcsEntity newEntity = world.CreateEntity(new ComponentB());
            uint newEntityId = newEntity.Id;
            Assert.AreEqual(0, world.EntitiesInProcessing);
            
            Assert.IsFalse(newEntity.HasComponent<ComponentA>());
            Assert.IsTrue(newEntity.HasComponent<ComponentB>());

            Assert.IsTrue(newEntityId > entityId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void AddComponentThrowExceptionTest()
        {
            EcsWorld world = new EcsWorld();
            EcsEntity entity = world.CreateEntity();
            entity.AddComponent(new ComponentA());
            entity.AddComponent(new ComponentA());
        }
        
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void RemoveComponentThrowExceptionTest()
        {
            EcsWorld world = new EcsWorld();
            EcsEntity entity = world.CreateEntity();
            entity.RemoveComponent<ComponentA>();
        }
    }
}