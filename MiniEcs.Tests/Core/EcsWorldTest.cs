using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MiniEcs.Core;

namespace MiniEcs.Tests.Core
{
    
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
            _world = new EcsWorld(ComponentType.TotalComponents);

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
            EcsWorld world = new EcsWorld(ComponentType.TotalComponents);
            
            ComponentB componentB = new ComponentB();
            EcsEntity entity = world.CreateEntity();
            entity[ComponentType.A] = new ComponentA();
            entity[ComponentType.B] = componentB;
            entity[ComponentType.C] = new ComponentC();

            Assert.IsNotNull(entity[ComponentType.A]);
            Assert.IsNotNull(entity[ComponentType.C]);
            Assert.AreEqual(componentB, entity[ComponentType.B]);
            Assert.IsNull(entity[ComponentType.D]);

            entity[ComponentType.B] = null;
            Assert.IsNull(entity[ComponentType.B]);
        }

        [TestMethod]
        public void AllFilterTest()
        {
            List<EcsEntity> entities = _world.Filter(new EcsFilter().AllOf(ComponentType.B)).ToList();
            Assert.AreEqual(5, entities.Count);
            
            Assert.IsTrue(entities.Contains(_entityABD));
            Assert.IsTrue(entities.Contains(_entityBD0));
            Assert.IsTrue(entities.Contains(_entityBD1));
            Assert.IsTrue(entities.Contains(_entityBC));
            Assert.IsTrue(entities.Contains(_entityAB));
            
            entities = _world.Filter(new EcsFilter().AllOf(ComponentType.B, ComponentType.D)).ToList();
            Assert.AreEqual(3, entities.Count);
            
            Assert.IsTrue(entities.Contains(_entityABD));
            Assert.IsTrue(entities.Contains(_entityBD0));
            Assert.IsTrue(entities.Contains(_entityBD1));
            
        }

        [TestMethod]
        public void AnyFilterTest()
        {
            List<EcsEntity> entities = _world.Filter(new EcsFilter().AnyOf(ComponentType.B)).ToList();
            Assert.AreEqual(5, entities.Count);
            
            Assert.IsTrue(entities.Contains(_entityABD));
            Assert.IsTrue(entities.Contains(_entityBD0));
            Assert.IsTrue(entities.Contains(_entityBD1));
            Assert.IsTrue(entities.Contains(_entityBC));
            Assert.IsTrue(entities.Contains(_entityAB));
            
            entities = _world.Filter(new EcsFilter().AnyOf(ComponentType.B, ComponentType.D)).ToList();
            Assert.AreEqual(6, entities.Count);
            
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
            List<EcsEntity> entities = _world.Filter(new EcsFilter().NoneOf(ComponentType.B, ComponentType.D)).ToList();
            Assert.AreEqual(1, entities.Count);
            
            Assert.IsTrue(entities.Contains(_entityAC));
            
            entities = _world.Filter(new EcsFilter().NoneOf(ComponentType.B, ComponentType.D, ComponentType.B)).ToList();
            Assert.AreEqual(1, entities.Count);
            
            Assert.IsTrue(entities.Contains(_entityAC));
        }
      
        [TestMethod]
        public void AllAnyFilterTest()
        {
            List<EcsEntity> entities = _world.Filter(new EcsFilter().AllOf(ComponentType.B, ComponentType.B, ComponentType.D).AnyOf(ComponentType.A)).ToList();
            Assert.AreEqual(1, entities.Count);
            Assert.IsTrue(entities.Contains(_entityABD));
            
            entities = _world.Filter(new EcsFilter().AllOf(ComponentType.D, ComponentType.D).AnyOf(ComponentType.B, ComponentType.C, ComponentType.C)).ToList();
            Assert.AreEqual(3, entities.Count);
            
            Assert.IsTrue(entities.Contains(_entityABD));
            Assert.IsTrue(entities.Contains(_entityBD0));
            Assert.IsTrue(entities.Contains(_entityBD1));
        }
        
  
        [TestMethod]
        public void AllNoneFilterTest()
        {
            List<EcsEntity> entities = _world.Filter(new EcsFilter().AllOf(ComponentType.B).NoneOf(ComponentType.A)).ToList();
            Assert.AreEqual(3, entities.Count);
            
            Assert.IsTrue(entities.Contains(_entityBD0));
            Assert.IsTrue(entities.Contains(_entityBD1));
            Assert.IsTrue(entities.Contains(_entityBC));
            
            entities = _world.Filter(new EcsFilter().AllOf(ComponentType.B, ComponentType.D).NoneOf(ComponentType.A)).ToList();
            Assert.AreEqual(2, entities.Count);
            
            Assert.IsTrue(entities.Contains(_entityBD0));
            Assert.IsTrue(entities.Contains(_entityBD1));
        }
        
        [TestMethod]
        public void GroupIncVersionFilterTest()
        {
            EcsWorld world = new EcsWorld(ComponentType.TotalComponents);            
            EcsEntity entity = world.CreateEntity(new ComponentA(), new ComponentB());

            List<EcsEntity> entities = world.Filter(new EcsFilter().AllOf(ComponentType.B)).ToList();
            Assert.AreEqual(1, entities.Count);
            
            entity[ComponentType.C] = new ComponentC();
            
            world.CreateEntity(new ComponentC(), new ComponentD());
            
            entities = world.Filter(new EcsFilter().AllOf(ComponentType.B)).ToList();
            Assert.AreEqual(1, entities.Count);
        }

        
        [TestMethod]
        public void GetOrCreateSingletonTest()
        {
            EcsWorld world = new EcsWorld(ComponentType.TotalComponents);

            ComponentA componentA0 = world.GetOrCreateSingleton<ComponentA>(ComponentType.A);
            ComponentA componentA1 = world.GetOrCreateSingleton<ComponentA>(ComponentType.A);
            
            Assert.AreEqual(componentA0, componentA1);
        }

    }
}