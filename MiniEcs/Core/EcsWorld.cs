using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace MiniEcs.Core
{
    /// <summary>
    /// EcsWorld manages the filtering of all entities in the World.
    /// EcsWorld maintains a list of archetypes and organizes object-related data
    /// for optimal performance.
    /// </summary>
    public class EcsWorld
    {
        /// <summary>
        /// Number of existing archetypes
        /// </summary>
        public int ArchetypeCount => _archetypeManager.ArchetypeCount;

        /// <summary>
        /// Number Entities In Processing
        /// </summary>
        public int EntitiesInProcessing => _entitiesPool.Count;

        /// <summary>
        /// Entity unique identifier generator
        /// </summary>
        private uint _entityCounter;

        /// <summary>
        /// Stores a group of archetypes for the specified filter.
        /// </summary>
        private readonly Dictionary<EcsFilter, EcsGroup> _groups = new Dictionary<EcsFilter, EcsGroup>();

        /// <summary>
        /// Pool of entities sent for processing
        /// </summary>
        private readonly Queue<EcsEntityExtended> _entitiesPool = new Queue<EcsEntityExtended>();

        private readonly EcsArchetypeManager _archetypeManager;

        public EcsWorld()
        {
            _archetypeManager = new EcsArchetypeManager();
        }

        public IEcsEntity CreateEntity()
        {
            EcsEntityExtended entity = _entitiesPool.Count <= 0
                ? new EcsEntityExtended(_entitiesPool, _archetypeManager)
                : _entitiesPool.Dequeue();
            entity.Initialize(_entityCounter++);
            return entity;
        }

        public IEcsEntity CreateEntity<TC0>(TC0 component0) where TC0 : IEcsComponent
        {
            EcsEntityExtended entity = _entitiesPool.Count <= 0
                ? new EcsEntityExtended(_entitiesPool, _archetypeManager)
                : _entitiesPool.Dequeue();
            entity.Initialize(_entityCounter++, component0);
            return entity;
        }

        public IEcsEntity CreateEntity<TC0, TC1>(TC0 component0, TC1 component1)
            where TC0 : IEcsComponent where TC1 : IEcsComponent
        {
            EcsEntityExtended entity = _entitiesPool.Count <= 0
                ? new EcsEntityExtended(_entitiesPool, _archetypeManager)
                : _entitiesPool.Dequeue();
            entity.Initialize(_entityCounter++, component0, component1);
            return entity;
        }

        public IEcsEntity CreateEntity<TC0, TC1, TC2>(TC0 component0, TC1 component1, TC2 component2) where TC0 : IEcsComponent
            where TC1 : IEcsComponent where TC2 : IEcsComponent
        {
            EcsEntityExtended entity = _entitiesPool.Count <= 0
                ? new EcsEntityExtended(_entitiesPool, _archetypeManager)
                : _entitiesPool.Dequeue();
            entity.Initialize(_entityCounter++, component0, component1, component2);
            return entity;
        }

        public IEcsEntity CreateEntity<TC0, TC1, TC2, TC3>(TC0 component0, TC1 component1, TC2 component2, TC3 component3)
            where TC0 : IEcsComponent where TC1 : IEcsComponent where TC2 : IEcsComponent where TC3 : IEcsComponent
        {
            EcsEntityExtended entity = _entitiesPool.Count <= 0
                ? new EcsEntityExtended(_entitiesPool, _archetypeManager)
                : _entitiesPool.Dequeue();
            entity.Initialize(_entityCounter++, component0, component1, component2, component3);
            return entity;
        }

        public TC GetOrCreateSingleton<TC>() where TC : class, IEcsComponent, new()
        {
            EcsArchetype archetype = _archetypeManager.FindOrCreateArchetype(EcsComponentType<TC>.Index);
            EcsEntity[] entities = archetype.GetEntities(out int length);

            for (int i = 0; i < length;)
                return entities[i].GetComponent<TC>();

            TC component = new TC();
            CreateEntity(component);
            return component;
        }

        public IEcsArchetype GetArchetype<TC>() where TC : IEcsComponent
        {
            return _archetypeManager.FindOrCreateArchetype(EcsComponentType<TC>.Index);
        }
        
        public IEcsArchetype GetArchetype<TC0, TC1>() where TC0 : IEcsComponent where TC1 : IEcsComponent
        {
            return _archetypeManager.FindOrCreateArchetype(EcsComponentType<TC0>.Index, EcsComponentType<TC1>.Index);
        }
        
        public IEcsArchetype GetArchetype<TC0, TC1, TC2>() where TC0 : IEcsComponent where TC1 : IEcsComponent where TC2 : IEcsComponent
        {
            return _archetypeManager.FindOrCreateArchetype(EcsComponentType<TC0>.Index, EcsComponentType<TC1>.Index, EcsComponentType<TC2>.Index);
        }

        /// <summary>
        /// Get a collection of archetypes for the specified filter.
        /// Each request caches the resulting set of archetypes for future use.
        /// As new archetypes are added to the world, the group of archetypes is updated.
        /// </summary>
        /// <param name="filter">
        /// A query defines a set of types of components that
        /// an archetype should include
        /// </param>
        /// <returns>Archetypes group</returns>
        public IEcsGroup Filter(EcsFilter filter)
        {
            int version = _archetypeManager.ArchetypeCount - 1;
            if (_groups.TryGetValue(filter, out EcsGroup group))
            {
                if (group.Version >= version)
                    return group;
            }

            byte[] all = filter.All?.ToArray();
            byte[] any = filter.Any?.ToArray();
            byte[] none = filter.None?.ToArray();

            if (group != null)
            {
                group.Update(version, GetArchetypes(all, any, none, group.Version));
                return group;
            }

            group = new EcsGroup(version, GetArchetypes(all, any, none, 0));
            _groups.Add(filter.Clone(), group);
            return group;
        }

        /// <summary>
        /// Retrieves all archetypes that match the search criteria.
        /// </summary>
        /// <param name="all">All component types in this array must exist in the archetype</param>
        /// <param name="any">At least one of the component types in this array must exist in the archetype</param>
        /// <param name="none">None of the component types in this array can exist in the archetype</param>
        /// <param name="startId">Archetype start id</param>
        /// <returns>Archetype enumerator</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IEnumerable<EcsArchetype> GetArchetypes(byte[] all, byte[] any, byte[] none, int startId)
        {
            HashSet<EcsArchetype> buffer0 = null;
            HashSet<EcsArchetype> buffer1 = null;

            if (all != null || any != null)
            {
                if (all != null)
                {
                    IReadOnlyList<EcsArchetype>[] archetypes = new IReadOnlyList<EcsArchetype>[all.Length];
                    for (int i = 0; i < all.Length; i++)
                    {
                        archetypes[i] = _archetypeManager.GetArchetypes(all[i], startId).ToArray();
                    }

                    Array.Sort(archetypes, (a, b) => a.Count - b.Count);

                    buffer0 = new HashSet<EcsArchetype>(archetypes[0]);
                    for (int i = 1; i < all.Length; i++)
                    {
                        buffer0.IntersectWith(archetypes[i]);
                    }
                }

                if (any != null)
                {
                    buffer1 = new HashSet<EcsArchetype>(_archetypeManager.GetArchetypes(any[0], startId));
                    for (int i = 1; i < any.Length; i++)
                    {
                        buffer1.UnionWith(_archetypeManager.GetArchetypes(any[i], startId));
                    }
                }

                if (buffer0 != null && buffer1 != null)
                {
                    buffer0.IntersectWith(buffer1);
                }
                else if (buffer1 != null)
                {
                    buffer0 = buffer1;
                }
            }
            else
            {
                buffer0 = new HashSet<EcsArchetype>(_archetypeManager.GetArchetypes(startId));
            }

            if (none != null)
            {
                foreach (byte type in none)
                {
                    buffer0.ExceptWith(_archetypeManager.GetArchetypes(type, startId));
                }
            }

            return buffer0;
        }


        private class EcsEntityExtended : EcsEntity
        {
            private readonly Queue<EcsEntityExtended> _entitiesPool;

            public EcsEntityExtended(Queue<EcsEntityExtended> entitiesPool, EcsArchetypeManager archetypeManager) :
                base(archetypeManager)
            {
                _entitiesPool = entitiesPool;
            }

            protected override void OnDestroy()
            {
                _entitiesPool.Enqueue(this);
            }
        }
    }
}