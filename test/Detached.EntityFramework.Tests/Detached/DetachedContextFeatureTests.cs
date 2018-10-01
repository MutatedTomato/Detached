﻿using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Detached.EntityFramework.Tests
{
    public class DetachedContextFeatureTests
    {
        [Fact]
        public void when_root_persisted__children_are_persisted()
        {
            using (IDetachedContext<TestDbContext> detachedContext = new DetachedContext<TestDbContext>())
            {
                // GIVEN a context:
                detachedContext.DbContext.AddRange(new[]
                {
                    new AssociatedListItem { Id = 1, Name = "Associated 1" },
                    new AssociatedListItem { Id = 2, Name = "Associated 2" }
                });
                detachedContext.DbContext.Add(new AssociatedReference { Id = 1, Name = "Associated 1" });
                detachedContext.DbContext.Add(new AssociatedReference { Id = 2, Name = "Associated 2" });
                detachedContext.DbContext.SaveChanges();

                // WHEN an entity is persisted:
                detachedContext.Set<Entity>().Update(new Entity
                {
                    Name = "Test entity",
                    AssociatedList = new[]
                    {
                        new AssociatedListItem { Id = 1, Name = "Sarlanga" },
                        new AssociatedListItem { Id = 2 }
                    },
                    AssociatedReference = new AssociatedReference { Id = 1 },
                    AssociatedReferenceWithShadowKey = new AssociatedReference { Id = 2 },
                    OwnedList = new[]
                    {
                        new OwnedListItem { Name = "Owned 1" },
                        new OwnedListItem { Name = "Owned 2" }
                    },
                    OwnedReference = new OwnedReference { Name = "Owned Reference 1" },
                    OwnedReferenceWithShadowKey = new OwnedReference { Name = "Owned Reference 2" },
                });
                detachedContext.SaveChanges();

                // THEN the entity should be loaded correctly:
                Entity persisted = detachedContext.Set<Entity>().Load("1");
                Assert.NotNull(persisted);
                Assert.Equal(2, persisted.AssociatedList.Count);
                Assert.NotNull(persisted.AssociatedReference);
                Assert.Equal(1, persisted.AssociatedReference.Id);
                Assert.NotNull(persisted.AssociatedReferenceWithShadowKey);
                Assert.Equal(2, persisted.AssociatedReferenceWithShadowKey.Id);
                Assert.Equal(2, persisted.OwnedList.Count);
                Assert.NotNull(persisted.OwnedReference);
                Assert.Equal("Owned Reference 1", persisted.OwnedReference.Name);
                Assert.NotNull(persisted.OwnedReferenceWithShadowKey);
                Assert.Equal("Owned Reference 2", persisted.OwnedReferenceWithShadowKey.Name);
            }
        }

        [Fact]
        public void when_add_item_to_collection__item_is_created()
        {
            using (TestDbContext context = new TestDbContext())
            {
                IDetachedContext<TestDbContext> detachedContext = new DetachedContext<TestDbContext>(context);

                // GIVEN an enity root with an owned list:
                context.Add(new Entity
                {
                    OwnedList = new[]  {
                        new OwnedListItem { Id = 1, Name = "Owned Item A" }
                    }
                });
                context.SaveChanges();

                // WHEN the collection is modified:
                Entity detachedEntity = new Entity
                {
                    Id = 1,
                    OwnedList = new[]  {
                        new OwnedListItem { Id = 1, Name = "Owned Item A" },
                        new OwnedListItem { Id = 2, Name = "Owned Item B" }
                    }
                };

                detachedContext.Set<Entity>().Update(detachedEntity);
                detachedContext.SaveChanges();

                // THEN the items is added to the the database.
                Entity persistedEntity =  detachedContext.Set<Entity>().Load(1);
                Assert.True(persistedEntity.OwnedList.Any(s => s.Name == "Owned Item A"));
                Assert.True(persistedEntity.OwnedList.Any(s => s.Name == "Owned Item B"));
            }
        }

        [Fact]
        public void when_remove_item_from_owned_collection__item_is_deleted()
        {
            using (TestDbContext context = new TestDbContext())
            {
                IDetachedContext<TestDbContext> detachedContext = new DetachedContext<TestDbContext>(context);

                // GIVEN an enity root with an owned list:
                context.Add(new Entity
                {
                    OwnedList = new[]  {
                            new OwnedListItem { Id = 1, Name = "Owned Item A" },
                            new OwnedListItem { Id = 2, Name = "Owned Item B" }
                        }
                });
                context.SaveChanges();

                // WHEN the collection is modified:
                Entity detachedEntity = new Entity
                {
                    Id = 1,
                    OwnedList = new[]  {
                            new OwnedListItem { Id = 2, Name = "Owned Item B" },
                        }
                };

                detachedContext.Set<Entity>().Update(detachedEntity);
                detachedContext.SaveChanges();

                // THEN the items is added to the the database.
                Entity persistedEntity =  detachedContext.Set<Entity>().Load(1);
                Assert.Equal(1, context.OwnedListItems.Count());
                Assert.True(persistedEntity.OwnedList.Any(s => s.Name == "Owned Item B"));
            }
        }

        [Fact]
        public void when_owned_reference_set_to_entity__entity_is_created()
        {
            using (TestDbContext context = new TestDbContext())
            {
                IDetachedContext<TestDbContext> detachedContext = new DetachedContext<TestDbContext>(context);

                // GIVEN an enity root with references:
                context.Add(new Entity
                {
                    OwnedReference = new OwnedReference { Id = 1, Name = "Owned Reference 1" }
                });
                context.SaveChanges();

                // WHEN the owned reference is set:
                detachedContext.Set<Entity>().Update(new Entity
                {
                    Id = 1,
                    OwnedReference = new OwnedReference { Id = 2, Name = "Owned Reference 2" }
                });
                detachedContext.SaveChanges();

                // THEN the owned reference is replaced, the old reference is deleted:
                Assert.Equal(1, context.OwnedReferences.Count());
                Assert.False(context.OwnedReferences.Any(o => o.Name == "Owned Reference 1"));
            }
        }

        [Fact]
        public void when_owned_reference_set_to_null__entity_is_deleted()
        {
            using (TestDbContext context = new TestDbContext())
            {
                IDetachedContext<TestDbContext> detachedContext = new DetachedContext<TestDbContext>(context);

                // GIVEN an enity root with references:
                context.Add(new Entity
                {
                    OwnedReference = new OwnedReference { Id = 1, Name = "Owned Reference 1" }
                });
                context.SaveChanges();

                // WHEN the owned and the associated references are set to null:
                Entity detachedEntity = new Entity
                {
                    Id = 1,
                    OwnedReference = null
                };

                detachedContext.Set<Entity>().Update(detachedEntity);
                detachedContext.SaveChanges();

                // THEN the owned reference is removed:
                Assert.Equal(0, context.OwnedReferences.Count());
            }
        }


        [Fact]
        public void when_owned_reference_with_shadow_key_set_to_entity__entity_is_created()
        {
            using (TestDbContext context = new TestDbContext())
            {
                IDetachedContext<TestDbContext> detachedContext = new DetachedContext<TestDbContext>(context);

                // GIVEN an enity root with references:
                context.Add(new Entity
                {
                    OwnedReference = new OwnedReference { Id = 1, Name = "Owned Reference 1" }
                });
                context.SaveChanges();

                // WHEN the owned reference is set:
                detachedContext.Set<Entity>().Update(new Entity
                {
                    Id = 1,
                    OwnedReferenceWithShadowKey = new OwnedReference { Id = 2, Name = "Owned Reference 2" }
                });
                detachedContext.SaveChanges();

                // THEN the owned reference is replaced, the old reference is deleted:
                Assert.Equal(1, context.OwnedReferences.Count());
                Assert.False(context.OwnedReferences.Any(o => o.Name == "Owned Reference 1"));
            }
        }

        [Fact]
        public void when_owned_reference_with_shadow_key_set_to_null__entity_is_deleted()
        {
            using (TestDbContext context = new TestDbContext())
            {
                IDetachedContext<TestDbContext> detachedContext = new DetachedContext<TestDbContext>(context);

                // GIVEN an enity root with references:
                context.Add(new Entity
                {
                    OwnedReference = new OwnedReference { Id = 1, Name = "Owned Reference 1" }
                });
                context.SaveChanges();

                // WHEN the owned and the associated references are set to null:
                Entity detachedEntity = new Entity
                {
                    Id = 1,
                    OwnedReferenceWithShadowKey = null
                };

                detachedContext.Set<Entity>().Update(detachedEntity);
                detachedContext.SaveChanges();

                // THEN the owned reference is removed:
                Assert.Equal(0, context.OwnedReferences.Count());
            }
        }

        [Fact]
        public void when_associated_reference_set_to_entity__entity_is_related_to_existing()
        {
            using (TestDbContext context = new TestDbContext())
            {
                IDetachedContext<TestDbContext> detachedContext = new DetachedContext<TestDbContext>(context);

                // GIVEN an enity root with references:
                AssociatedReference[] references = new[]
                {
                        new AssociatedReference { Id = 1, Name = "Associated Reference 1" },
                        new AssociatedReference { Id = 2, Name = "Associated Reference 2" }
                    };
                context.AddRange(references);
                context.Add(new Entity
                {
                    AssociatedReference = references[0],
                });
                context.SaveChanges();

                // WHEN the owned and the associated references are set to null:
                Entity detachedEntity = new Entity
                {
                    Id = 1,
                    AssociatedReference = new AssociatedReference { Id = 1, Name = "Modified Associated Reference 1" },
                };

                detachedContext.Set<Entity>().Update(detachedEntity);
                detachedContext.SaveChanges();

                // THEN the associated reference still exsits:
                Assert.True(context.AssociatedReferences.Any(a => a.Name == "Associated Reference 1"));
            }
        }

        [Fact]
        public void when_associated_reference_set_to_null__entity_is_preserved()
        {
            using (TestDbContext context = new TestDbContext())
            {
                IDetachedContext<TestDbContext> detachedContext = new DetachedContext<TestDbContext>(context);

                // GIVEN an enity root with references:
                AssociatedReference[] references = new[]
                {
                        new AssociatedReference { Id = 1, Name = "Associated Reference 1" },
                        new AssociatedReference { Id = 2, Name = "Associated Reference 2" }
                    };
                context.AddRange(references);
                context.Add(new Entity
                {
                    AssociatedReference = references[0],
                });
                context.SaveChanges();

                // WHEN the owned and the associated references are set to null:
                Entity detachedEntity = new Entity
                {
                    Id = 1,
                    AssociatedReference = null,
                    OwnedReference = null
                };

                detachedContext.Set<Entity>().Update(detachedEntity);
                detachedContext.SaveChanges();

                // THEN the associated reference still exsits:
                Assert.True(context.AssociatedReferences.Any(a => a.Name == "Associated Reference 1"));
            }
        }

        [Fact]
        public void when_entity_deleted__owned_properties_are_deleted()
        {
            using (TestDbContext context = new TestDbContext())
            {
                IDetachedContext<TestDbContext> detachedContext = new DetachedContext<TestDbContext>(context);

                // GIVEN an enity root with an owned list:
                var associatedItems = new List<AssociatedListItem>(new[]
                {
                    new AssociatedListItem { Name = "Associated Item 1" },
                    new AssociatedListItem { Name = "Associated Item 2" }
                });
                context.AddRange(associatedItems);
                context.SaveChanges();

                context.Add(new Entity
                {
                    OwnedList = new List<OwnedListItem>(new[]  {
                        new OwnedListItem { Name = "Owned Item A" },
                        new OwnedListItem { Name = "Owned Item B" },
                        new OwnedListItem { Name = "Owned Item C" }
                    }),
                    AssociatedList = associatedItems
                });
                context.SaveChanges();

                // WHEN the entity is deleted:
                detachedContext.Set<Entity>().Delete(1);
                detachedContext.SaveChanges();

                // THEN owned items are removed:
                Assert.Equal(0, context.OwnedListItems.Count());

                // and the associated items are not removed:
                Assert.Equal(2, context.AssociatedListItems.Count());
                Assert.True(context.AssociatedListItems.Any(e => e.Name == "Associated Item 1"));
                Assert.True(context.AssociatedListItems.Any(e => e.Name == "Associated Item 2"));
            }
        }
        
        [Fact]
        public void when_associated_reference_with_shadow_key_set_to_entity__entity_is_related_to_existing()
        {
            using (TestDbContext context = new TestDbContext())
            {
                IDetachedContext<TestDbContext> detachedContext = new DetachedContext<TestDbContext>(context);

                // GIVEN an enity root with references:
                AssociatedReference[] references = new[]
                {
                        new AssociatedReference { Id = 1, Name = "Associated Reference 1" },
                        new AssociatedReference { Id = 2, Name = "Associated Reference 2" }
                    };
                context.AddRange(references);
                context.Add(new Entity
                {
                    AssociatedReferenceWithShadowKey = references[0],
                });
                context.SaveChanges();

                // WHEN the owned and the associated references are set to null:
                Entity detachedEntity = new Entity
                {
                    Id = 1,
                    AssociatedReferenceWithShadowKey = new AssociatedReference { Id = 1, Name = "Modified Associated Reference 1" },
                };

                detachedContext.Set<Entity>().Update(detachedEntity);
                detachedContext.SaveChanges();

                // THEN the associated reference still exsits:
                Assert.True(context.AssociatedReferences.Any(a => a.Name == "Associated Reference 1"));
            }
        }

        [Fact]
        public void when_associated_reference_with_shadow_key_set_to_null__entity_is_preserved()
        {
            using (TestDbContext context = new TestDbContext())
            {
                IDetachedContext<TestDbContext> detachedContext = new DetachedContext<TestDbContext>(context);

                // GIVEN an enity root with references:
                AssociatedReference[] references = new[]
                {
                        new AssociatedReference { Id = 1, Name = "Associated Reference 1" },
                        new AssociatedReference { Id = 2, Name = "Associated Reference 2" }
                    };
                context.AddRange(references);
                context.Add(new Entity
                {
                    AssociatedReferenceWithShadowKey = references[0],
                });
                context.SaveChanges();

                // WHEN the owned and the associated references are set to null:
                Entity detachedEntity = new Entity
                {
                    Id = 1,
                    AssociatedReferenceWithShadowKey = null,
                    OwnedReference = null
                };

                detachedContext.Set<Entity>().Update(detachedEntity);
                detachedContext.SaveChanges();

                // THEN the associated reference still exsits:
                Assert.True(context.AssociatedReferences.Any(a => a.Name == "Associated Reference 1"));
            }
        }

        [Fact]
        public void when_load_by_key__type_conversion_is_made()
        {
            using (TestDbContext context = new TestDbContext())
            {
                IDetachedContext<TestDbContext> detachedContext = new DetachedContext<TestDbContext>(context);

                detachedContext.Set<Entity>().Update(new Entity
                {
                    Name = "Test entity"
                });
                detachedContext.SaveChanges();


                Entity persisted = detachedContext.Set<Entity>().Load("1");
                Assert.NotNull(persisted);

                persisted = detachedContext.Set<Entity>().Load(1);
                Assert.NotNull(persisted);
            }
        }

        [Fact]
        public void when_load_sorted__result_is_ordered()
        {
            using (TestDbContext dbContext = new TestDbContext())
            {
                IDetachedContext<TestDbContext> detachedContext = new DetachedContext<TestDbContext>(dbContext);
                dbContext.AddRange(new[]
                {
                    new Entity { Name = "Order By Entity 2" },
                    new Entity { Name = "Order By Entity 1" },
                    new Entity { Name = "Order By Entity 3" }
                });
                dbContext.SaveChanges();
            }
        }

        [Fact]
        public void when_2_entities_same_type_persisted_values_are_updated()
        {
            TwoReferencesSameTypeEntity entity = new TwoReferencesSameTypeEntity();
            entity.ReferenceA = new TwoReferencesSameTypeReference
            {
                Name = "Reference A",
                Items = new[]
                {
                    new TwoReferencesSameTypeItem { Name = "Reference A Item 1" },
                    new TwoReferencesSameTypeItem { Name = "Reference A Item 2" },
                    new TwoReferencesSameTypeItem { Name = "Reference A Item 3" }
                }
            };
            entity.ReferenceB = new TwoReferencesSameTypeReference
            {
                Name = "Reference B",
                Items = new[]
                {
                    new TwoReferencesSameTypeItem { Name = "Reference B Item 1" },
                    new TwoReferencesSameTypeItem { Name = "Reference B Item 2" },
                    new TwoReferencesSameTypeItem { Name = "Reference B Item 3" }
                }
            };
            entity.ReferenceC = new TwoReferencesSameTypeReference
            {
                Name = "Reference C",
                Items = new[]
                {
                    new TwoReferencesSameTypeItem { Name = "Reference C Item 1" },
                    new TwoReferencesSameTypeItem { Name = "Reference C Item 2" },
                    new TwoReferencesSameTypeItem { Name = "Reference C Item 3" }
                }
            };

            using (IDetachedContext<TestDbContext> detachedContext = new DetachedContext<TestDbContext>())
            {
                detachedContext.Set<TwoReferencesSameTypeEntity>().Update(entity);
                detachedContext.SaveChanges();

                TwoReferencesSameTypeEntity entity2 = detachedContext.Set<TwoReferencesSameTypeEntity>().Load(1);

                Assert.Equal("Reference A Item 1", entity2.ReferenceA.Items[0].Name);
                Assert.Equal("Reference A Item 2", entity2.ReferenceA.Items[1].Name);
                Assert.Equal("Reference A Item 3", entity2.ReferenceA.Items[2].Name);

                Assert.Equal("Reference B Item 1", entity2.ReferenceB.Items[0].Name);
                Assert.Equal("Reference B Item 2", entity2.ReferenceB.Items[1].Name);
                Assert.Equal("Reference B Item 3", entity2.ReferenceB.Items[2].Name);

                Assert.Equal("Reference C Item 1", entity2.ReferenceC.Items[0].Name);
                Assert.Equal("Reference C Item 2", entity2.ReferenceC.Items[1].Name);
                Assert.Equal("Reference C Item 3", entity2.ReferenceC.Items[2].Name);
            }
        }

        [Fact]
        public void when_deleting_multiple_ids__entities_are_deleted()
        {
            using (TestDbContext context = new TestDbContext())
            {
                // GIVEN: 3 entities
                IDetachedContext<TestDbContext> detachedContext = new DetachedContext<TestDbContext>(context);

                detachedContext.Set<Entity>().Update(new Entity
                {
                    Name = "Entity 1"
                });

                detachedContext.Set<Entity>().Update(new Entity
                {
                    Name = "Entity 2"
                });

                detachedContext.Set<Entity>().Update(new Entity
                {
                    Name = "Entity 3"
                });

                detachedContext.SaveChanges();

                List<Entity> all = context.Entities.ToList();
                Assert.Equal(3, all.Count);

                // WHEN: two are deleted in batch
                detachedContext.Set<Entity>().Delete(new KeyValue(1), new KeyValue(2));
                detachedContext.SaveChanges();

                // THEN: only one remains
                all = context.Entities.ToList();
                Assert.Equal(1, all.Count);
            }
        }
    }
}