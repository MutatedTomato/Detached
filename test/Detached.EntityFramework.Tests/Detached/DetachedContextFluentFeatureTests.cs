﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Detached.EntityFramework.Tests
{
    public class DetachedContextFluentFeatureTests
    {
        [Fact]
        public async Task when_root_persisted__children_are_persisted()
        {
            using (IDetachedContext<TestFluentDbContext> detachedContext = new DetachedContext<TestFluentDbContext>())
            {
                // GIVEN a context:
                detachedContext.DbContext.AddRange(new[]
                {
                    new AssociatedListItem { Id = 1, Name = "Associated 1" },
                    new AssociatedListItem { Id = 2, Name = "Associated 2" }
                });
                detachedContext.DbContext.Add(new AssociatedReference { Id = 1, Name = "Associated 1" });
                detachedContext.DbContext.Add(new AssociatedReference { Id = 2, Name = "Associated 2" });
                await detachedContext.DbContext.SaveChangesAsync();

                // WHEN an entity is persisted:
                await detachedContext.Set<FluentEntity>().UpdateAsync(new FluentEntity
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
                await detachedContext.SaveChangesAsync();

                // THEN the entity should be loaded correctly:
                FluentEntity persisted = await detachedContext.Set<FluentEntity>().LoadAsync("1");
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
        public async Task when_derived_root_persisted__children_are_persisted()
        {
            using (IDetachedContext<TestFluentDbContext> detachedContext = new DetachedContext<TestFluentDbContext>())
            {
                // GIVEN a context:
                detachedContext.DbContext.AddRange(new[]
                {
                    new AssociatedListItem { Id = 1, Name = "Associated 1" },
                    new AssociatedListItem { Id = 2, Name = "Associated 2" }
                });
                detachedContext.DbContext.Add(new AssociatedReference { Id = 1, Name = "Associated 1" });
                detachedContext.DbContext.Add(new AssociatedReference { Id = 2, Name = "Associated 2" });
                await detachedContext.DbContext.SaveChangesAsync();

                // WHEN an entity is persisted:
                await detachedContext.Set<FluentEntity>().UpdateAsync(new DerivedFluentEntity
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
                    DerivedOwnedReference = new OwnedReference { Name = "Derived Owned Reference"},
                });
                await detachedContext.SaveChangesAsync();

                // THEN the entity should be loaded correctly:
                FluentEntity persisted = await detachedContext.Set<FluentEntity>().GetBaseQuery().AsNoTracking().FirstAsync(e => e.Id == 1);
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
                Assert.NotNull(((DerivedFluentEntity)persisted).DerivedOwnedReference);
                Assert.Equal(3, ((DerivedFluentEntity)persisted).DerivedOwnedReference.Id);
            }
        }

        [Fact]
        public async Task when_add_item_to_collection__item_is_created()
        {
            using (TestFluentDbContext context = new TestFluentDbContext())
            {
                IDetachedContext<TestFluentDbContext> detachedContext = new DetachedContext<TestFluentDbContext>(context);

                // GIVEN an enity root with an owned list:
                context.Add(new FluentEntity
                {
                    OwnedList = new[]  {
                        new OwnedListItem { Id = 1, Name = "Owned Item A" }
                    }
                });
                context.SaveChanges();

                // WHEN the collection is modified:
                FluentEntity detachedEntity = new FluentEntity
                {
                    Id = 1,
                    OwnedList = new[]  {
                        new OwnedListItem { Id = 1, Name = "Owned Item A" },
                        new OwnedListItem { Id = 2, Name = "Owned Item B" }
                    }
                };

                await detachedContext.Set<FluentEntity>().UpdateAsync(detachedEntity);
                await detachedContext.SaveChangesAsync();

                // THEN the items is added to the the database.
                FluentEntity persistedEntity = await detachedContext.Set<FluentEntity>().LoadAsync(1);
                Assert.True(persistedEntity.OwnedList.Any(s => s.Name == "Owned Item A"));
                Assert.True(persistedEntity.OwnedList.Any(s => s.Name == "Owned Item B"));
            }
        }

        [Fact]
        public async Task when_remove_item_from_owned_collection__item_is_deleted()
        {
            using (TestFluentDbContext context = new TestFluentDbContext())
            {
                IDetachedContext<TestFluentDbContext> detachedContext = new DetachedContext<TestFluentDbContext>(context);

                // GIVEN an enity root with an owned list:
                context.Add(new FluentEntity
                {
                    OwnedList = new[]  {
                            new OwnedListItem { Id = 1, Name = "Owned Item A" },
                            new OwnedListItem { Id = 2, Name = "Owned Item B" }
                        }
                });
                context.SaveChanges();

                // WHEN the collection is modified:
                FluentEntity detachedEntity = new FluentEntity
                {
                    Id = 1,
                    OwnedList = new[]  {
                            new OwnedListItem { Id = 2, Name = "Owned Item B" },
                        }
                };

                await detachedContext.Set<FluentEntity>().UpdateAsync(detachedEntity);
                await detachedContext.SaveChangesAsync();

                // THEN the items is added to the the database.
                FluentEntity persistedEntity = await detachedContext.Set<FluentEntity>().LoadAsync(1);
                Assert.Equal(1, context.OwnedListItems.Count());
                Assert.True(persistedEntity.OwnedList.Any(s => s.Name == "Owned Item B"));
            }
        }

        [Fact]
        public async Task when_owned_reference_set_to_entity__entity_is_created()
        {
            using (TestFluentDbContext context = new TestFluentDbContext())
            {
                IDetachedContext<TestFluentDbContext> detachedContext = new DetachedContext<TestFluentDbContext>(context);

                // GIVEN an enity root with references:
                context.Add(new FluentEntity
                {
                    OwnedReference = new OwnedReference { Id = 1, Name = "Owned Reference 1" }
                });
                context.SaveChanges();

                // WHEN the owned reference is set:
                await detachedContext.Set<FluentEntity>().UpdateAsync(new FluentEntity
                {
                    Id = 1,
                    OwnedReference = new OwnedReference { Id = 2, Name = "Owned Reference 2" }
                });
                await detachedContext.SaveChangesAsync();

                // THEN the owned reference is replaced, the old reference is deleted:
                Assert.Equal(1, context.OwnedReferences.Count());
                Assert.False(context.OwnedReferences.Any(o => o.Name == "Owned Reference 1"));
            }
        }

        [Fact]
        public async Task when_owned_reference_set_to_null__entity_is_deleted()
        {
            using (TestFluentDbContext context = new TestFluentDbContext())
            {
                IDetachedContext<TestFluentDbContext> detachedContext = new DetachedContext<TestFluentDbContext>(context);

                // GIVEN an enity root with references:
                context.Add(new FluentEntity
                {
                    OwnedReference = new OwnedReference { Id = 1, Name = "Owned Reference 1" }
                });
                context.SaveChanges();

                // WHEN the owned and the associated references are set to null:
                FluentEntity detachedEntity = new FluentEntity
                {
                    Id = 1,
                    OwnedReference = null
                };

                await detachedContext.Set<FluentEntity>().UpdateAsync(detachedEntity);
                await detachedContext.SaveChangesAsync();

                // THEN the owned reference is removed:
                Assert.Equal(0, context.OwnedReferences.Count());
            }
        }
        
        [Fact]
        public async Task when_owned_reference_with_shadow_key_set_to_entity__entity_is_created()
        {
            using (TestFluentDbContext context = new TestFluentDbContext())
            {
                IDetachedContext<TestFluentDbContext> detachedContext = new DetachedContext<TestFluentDbContext>(context);

                // GIVEN an enity root with references:
                context.Add(new FluentEntity
                {
                    OwnedReferenceWithShadowKey = new OwnedReference { Id = 1, Name = "Owned Reference 1" }
                });
                context.SaveChanges();

                // WHEN the owned reference is set:
                await detachedContext.Set<FluentEntity>().UpdateAsync(new FluentEntity
                {
                    Id = 1,
                    OwnedReferenceWithShadowKey = new OwnedReference { Id = 2, Name = "Owned Reference 2" }
                });
                await detachedContext.SaveChangesAsync();

                // THEN the owned reference is replaced, the old reference is deleted:
                Assert.Equal(1, context.OwnedReferences.Count());
                Assert.False(context.OwnedReferences.Any(o => o.Name == "Owned Reference 1"));
            }
        }

        [Fact]
        public async Task when_owned_reference_with_shadow_key_set_to_null__entity_is_deleted()
        {
            using (TestFluentDbContext context = new TestFluentDbContext())
            {
                IDetachedContext<TestFluentDbContext> detachedContext = new DetachedContext<TestFluentDbContext>(context);

                // GIVEN an enity root with references:
                context.Add(new FluentEntity
                {
                    OwnedReferenceWithShadowKey = new OwnedReference { Id = 1, Name = "Owned Reference 1" }
                });
                context.SaveChanges();

                // WHEN the owned and the associated references are set to null:
                FluentEntity detachedEntity = new FluentEntity
                {
                    Id = 1,
                    OwnedReferenceWithShadowKey = null
                };

                await detachedContext.Set<FluentEntity>().UpdateAsync(detachedEntity);
                await detachedContext.SaveChangesAsync();

                // THEN the owned reference is removed:
                Assert.Equal(0, context.OwnedReferences.Count());
            }
        }

        [Fact]
        public async Task when_associated_reference_set_to_entity__entity_is_related_to_existing()
        {
            using (TestFluentDbContext context = new TestFluentDbContext())
            {
                IDetachedContext<TestFluentDbContext> detachedContext = new DetachedContext<TestFluentDbContext>(context);

                // GIVEN an enity root with references:
                AssociatedReference[] references = new[]
                {
                        new AssociatedReference { Id = 1, Name = "Associated Reference 1" },
                        new AssociatedReference { Id = 2, Name = "Associated Reference 2" }
                    };
                context.AddRange(references);
                context.Add(new FluentEntity
                {
                    AssociatedReference = references[0],
                });
                await context.SaveChangesAsync();

                // WHEN the owned and the associated references are set to null:
                FluentEntity detachedEntity = new FluentEntity
                {
                    Id = 1,
                    AssociatedReference = new AssociatedReference { Id = 1, Name = "Modified Associated Reference 1" },
                };

                await detachedContext.Set<FluentEntity>().UpdateAsync(detachedEntity);
                await detachedContext.SaveChangesAsync();

                // THEN the associated reference still exsits:
                Assert.True(context.AssociatedReferences.Any(a => a.Name == "Associated Reference 1"));
            }
        }

        [Fact]
        public async Task when_associated_reference_set_to_null__entity_is_preserved()
        {
            using (TestFluentDbContext context = new TestFluentDbContext())
            {
                IDetachedContext<TestFluentDbContext> detachedContext = new DetachedContext<TestFluentDbContext>(context);

                // GIVEN an enity root with references:
                AssociatedReference[] references = new[]
                {
                        new AssociatedReference { Id = 1, Name = "Associated Reference 1" },
                        new AssociatedReference { Id = 2, Name = "Associated Reference 2" }
                    };
                context.AddRange(references);
                context.Add(new FluentEntity
                {
                    AssociatedReference = references[0],
                });
                context.SaveChanges();

                // WHEN the owned and the associated references are set to null:
                FluentEntity detachedEntity = new FluentEntity
                {
                    Id = 1,
                    AssociatedReference = null,
                    OwnedReference = null
                };

                await detachedContext.Set<FluentEntity>().UpdateAsync(detachedEntity);
                await detachedContext.SaveChangesAsync();

                // THEN the associated reference still exsits:
                Assert.True(context.AssociatedReferences.Any(a => a.Name == "Associated Reference 1"));
            }
        }

        [Fact]
        public async Task when_associated_reference_with_shadow_key_set_to_entity__entity_is_related_to_existing()
        {
            using (TestFluentDbContext context = new TestFluentDbContext())
            {
                IDetachedContext<TestFluentDbContext> detachedContext = new DetachedContext<TestFluentDbContext>(context);

                // GIVEN an enity root with references:
                AssociatedReference[] references = new[]
                {
                        new AssociatedReference { Id = 1, Name = "Associated Reference 1" },
                        new AssociatedReference { Id = 2, Name = "Associated Reference 2" }
                    };
                context.AddRange(references);
                context.Add(new FluentEntity
                {
                    AssociatedReferenceWithShadowKey = references[0],
                });
                await context.SaveChangesAsync();

                // WHEN the owned and the associated references are set to null:
                FluentEntity detachedEntity = new FluentEntity
                {
                    Id = 1,
                    AssociatedReferenceWithShadowKey = new AssociatedReference { Id = 1, Name = "Modified Associated Reference 1" },
                };

                await detachedContext.Set<FluentEntity>().UpdateAsync(detachedEntity);
                await detachedContext.SaveChangesAsync();

                // THEN the associated reference still exsits:
                Assert.True(context.AssociatedReferences.Any(a => a.Name == "Associated Reference 1"));
            }
        }

        [Fact]
        public async Task when_associated_reference_with_shadow_key_set_to_null__entity_is_preserved()
        {
            using (TestFluentDbContext context = new TestFluentDbContext())
            {
                IDetachedContext<TestFluentDbContext> detachedContext = new DetachedContext<TestFluentDbContext>(context);

                // GIVEN an enity root with references:
                AssociatedReference[] references = new[]
                {
                        new AssociatedReference { Id = 1, Name = "Associated Reference 1" },
                        new AssociatedReference { Id = 2, Name = "Associated Reference 2" }
                    };
                context.AddRange(references);
                context.Add(new FluentEntity
                {
                    AssociatedReferenceWithShadowKey = references[0],
                });
                context.SaveChanges();

                // WHEN the owned and the associated references are set to null:
                FluentEntity detachedEntity = new FluentEntity
                {
                    Id = 1,
                    AssociatedReferenceWithShadowKey = null,
                    OwnedReference = null
                };

                await detachedContext.Set<FluentEntity>().UpdateAsync(detachedEntity);
                await detachedContext.SaveChangesAsync();

                // THEN the associated reference still exsits:
                Assert.True(context.AssociatedReferences.Any(a => a.Name == "Associated Reference 1"));
            }
        }

        [Fact]
        public async Task when_associalted_children_are_updated_then_root_is_updated()
        {
            using (IDetachedContext<TestFluentDbContext> detachedContext = new DetachedContext<TestFluentDbContext>())
            {
                // GIVEN a context:
                detachedContext.DbContext.AddRange(new[]
                {
                    new AssociatedListItem { Id = 1, Name = "Associated 1" },
                    new AssociatedListItem { Id = 2, Name = "Associated 2" }
                });
                detachedContext.DbContext.Add(new AssociatedReference { Id = 1, Name = "Associated 1" });
                detachedContext.DbContext.Add(new AssociatedReference { Id = 2, Name = "Associated 2" });
                detachedContext.DbContext.Add(new FluentEntity
                {
                    Name = "Test entity",
                    Id = 1,
                });
                await detachedContext.DbContext.SaveChangesAsync();

                // WHEN the entity and associated children are updated
                await detachedContext.Set<AssociatedReference>().UpdateAsync(new AssociatedReference
                {
                    Id = 1,
                    Name = "Associated 1 updated",
                });
                await detachedContext.Set<AssociatedReference>().UpdateAsync(new AssociatedReference
                {
                    Id = 2,
                    Name = "Associated 2 updated",
                });
                await detachedContext.Set<AssociatedListItem>().UpdateAsync(new AssociatedListItem
                {
                    Id = 1,
                    Name = "Associated 1 updated",
                });
                await detachedContext.Set<FluentEntity>().UpdateAsync(new FluentEntity
                {
                    Id = 1,
                    Name = "Test entity updated",
                    AssociatedList = new[]
                    {
                        new AssociatedListItem { Id = 1 },
                        new AssociatedListItem { Id = 2 }
                    },
                    AssociatedReference = new AssociatedReference { Id = 1 },
                    AssociatedReferenceWithShadowKey = new AssociatedReference { Id = 2 },
                });
                await detachedContext.SaveChangesAsync();

                // THEN the entity should be loaded correctly:
                FluentEntity persisted = await detachedContext.Set<FluentEntity>().LoadAsync("1");
                Assert.NotNull(persisted);
                Assert.Equal("Test entity updated", persisted.Name);
                Assert.Equal(2, persisted.AssociatedList.Count);
                Assert.NotNull(persisted.AssociatedList.FirstOrDefault(a => a.Id == 1));
                Assert.Equal("Associated 1 updated", persisted.AssociatedList.First(a => a.Id == 1).Name);
                Assert.NotNull(persisted.AssociatedReference);
                Assert.Equal(1, persisted.AssociatedReference.Id);
                Assert.Equal("Associated 1 updated", persisted.AssociatedReference.Name);
                Assert.NotNull(persisted.AssociatedReferenceWithShadowKey);
                Assert.Equal(2, persisted.AssociatedReferenceWithShadowKey.Id);
                Assert.Equal("Associated 2 updated", persisted.AssociatedReferenceWithShadowKey.Name);
            }
        }

        [Fact]
        public async Task when_root_is_updated_then_associalted_child_is_updated()
        {
            using (IDetachedContext<TestFluentDbContext> detachedContext = new DetachedContext<TestFluentDbContext>())
            {
                // GIVEN a context:
                detachedContext.DbContext.Add(new AssociatedReference { Id = 1, Name = "Associated 1" });
                detachedContext.DbContext.Add(new FluentEntity
                {
                    Name = "Test entity",
                    Id = 1,
                });
                await detachedContext.DbContext.SaveChangesAsync();

                //WHEN the entity and associated child are updated
                var associated = new AssociatedReference { Id = 1, Name = "child" };
                await detachedContext.Set<FluentEntity>().UpdateAsync(new FluentEntity
                {
                    Id = 1,
                    AssociatedReference = associated,
                });

                await detachedContext.Set<AssociatedReference>().UpdateAsync(new AssociatedReference()
                {
                    Id = 1,
                    Name = "Associated 1 updated",
                });
                await detachedContext.SaveChangesAsync();

                // THEN the entity should be loaded correctly:
                Assert.Equal(1, detachedContext.DbContext.AssociatedReferences.Count());
                FluentEntity persisted = await detachedContext.Set<FluentEntity>().LoadAsync("1");
                Assert.NotNull(persisted);
                Assert.NotNull(persisted.AssociatedReference);
                Assert.Equal(1, persisted.AssociatedReference.Id);
                Assert.Equal("Associated 1 updated", persisted.AssociatedReference.Name);
            }
        }

        [Fact]
        public async Task when_entity_deleted__owned_properties_are_deleted()
        {
            using (TestFluentDbContext context = new TestFluentDbContext())
            {
                IDetachedContext<TestFluentDbContext> detachedContext = new DetachedContext<TestFluentDbContext>(context);

                // GIVEN an enity root with an owned list:
                var associatedItems = new List<AssociatedListItem>(new[]
                {
                    new AssociatedListItem { Name = "Associated Item 1" },
                    new AssociatedListItem { Name = "Associated Item 2" }
                });
                context.AddRange(associatedItems);
                await context.SaveChangesAsync();

                context.Add(new FluentEntity
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
                await detachedContext.Set<FluentEntity>().DeleteAsync(1);
                await detachedContext.SaveChangesAsync();

                // THEN owned items are removed:
                Assert.Equal(0, context.OwnedListItems.Count());

                // and the associated items are not removed:
                Assert.Equal(2, context.AssociatedListItems.Count());
                Assert.True(context.AssociatedListItems.Any(e => e.Name == "Associated Item 1"));
                Assert.True(context.AssociatedListItems.Any(e => e.Name == "Associated Item 2"));
            }
        }

        [Fact]
        public async Task when_load_by_key__type_conversion_is_made()
        {
            using (TestFluentDbContext context = new TestFluentDbContext())
            {
                IDetachedContext<TestFluentDbContext> detachedContext = new DetachedContext<TestFluentDbContext>(context);

                await detachedContext.Set<FluentEntity>().UpdateAsync(new FluentEntity
                {
                    Name = "Test entity"
                });
                await detachedContext.SaveChangesAsync();


                FluentEntity persisted = await detachedContext.Set<FluentEntity>().LoadAsync("1");
                Assert.NotNull(persisted);

                persisted = await detachedContext.Set<FluentEntity>().LoadAsync(1);
                Assert.NotNull(persisted);
            }
        }

        [Fact]
        public async Task when_load_sorted__result_is_ordered()
        {
            using (TestFluentDbContext dbContext = new TestFluentDbContext())
            {
                IDetachedContext<TestFluentDbContext> detachedContext = new DetachedContext<TestFluentDbContext>(dbContext);
                dbContext.AddRange(new[]
                {
                    new FluentEntity { Name = "Order By Entity 2" },
                    new FluentEntity { Name = "Order By Entity 1" },
                    new FluentEntity { Name = "Order By Entity 3" }
                });
                await dbContext.SaveChangesAsync();
            }
        }

        [Fact]
        public async Task when_2_entities_same_type_persisted_values_are_updated()
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

            using (IDetachedContext<TestFluentDbContext> detachedContext = new DetachedContext<TestFluentDbContext>())
            {
                await detachedContext.Set<TwoReferencesSameTypeEntity>().UpdateAsync(entity);
                await detachedContext.SaveChangesAsync();

                TwoReferencesSameTypeEntity entity2 = await detachedContext.Set<TwoReferencesSameTypeEntity>().LoadAsync(1);

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
        public async Task when_deleting_multiple_ids__entities_are_deleted()
        {
            using (TestFluentDbContext context = new TestFluentDbContext())
            {
                // GIVEN: 3 entities
                IDetachedContext<TestFluentDbContext> detachedContext = new DetachedContext<TestFluentDbContext>(context);

                await detachedContext.Set<FluentEntity>().UpdateAsync(new FluentEntity
                {
                    Name = "Entity 1"
                });

                await detachedContext.Set<FluentEntity>().UpdateAsync(new FluentEntity
                {
                    Name = "Entity 2"
                });

                await detachedContext.Set<FluentEntity>().UpdateAsync(new FluentEntity
                {
                    Name = "Entity 3"
                });

                await detachedContext.SaveChangesAsync();

                List<FluentEntity> all = context.Entities.ToList();
                Assert.Equal(3, all.Count);

                // WHEN: two are deleted in batch
                await detachedContext.Set<FluentEntity>().DeleteAsync(new KeyValue(1), new KeyValue(2));
                await detachedContext.SaveChangesAsync();

                // THEN: only one remains
                all = context.Entities.ToList();
                Assert.Equal(1, all.Count);
            }
        }
    }
}