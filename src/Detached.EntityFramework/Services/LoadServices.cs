using Detached.EntityFramework.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Detached.EntityFramework.Services
{
    public class LoadServices : ILoadServices
    {
        #region Fields

        IEventManager _eventManager;
        IEntityServicesFactory _entityServicesFactory;
        DbContext _dbContext;

        #endregion

        #region Ctor.

        public LoadServices(DbContext dbContext,
                            IEventManager eventManager,
                            IEntityServicesFactory entityServicesFactory)
        {
            _eventManager = eventManager;
            _entityServicesFactory = entityServicesFactory;
            _dbContext = dbContext;
        }

        #endregion

        public virtual IQueryable<TEntity> GetBaseQuery<TEntity>()
            where TEntity : class
        {
            IEntityType entityType = _dbContext.Model.FindEntityType(typeof(TEntity));

            // compute paths from owned and associated navigations.
            List<string> paths = new List<string>();
            GetIncludePaths(entityType, null, ref paths);

            // get base query.
            IQueryable<TEntity> query = _dbContext.Set<TEntity>();

            // include all paths.
            foreach (string path in paths)
            {
                query = query.Include(path);
            }

            return query;
        }

        public virtual void GetIncludePaths( IEntityType entityType, string currentPath, ref List<string> paths)
        {
            // gets a list of navigations.
            var navigations = entityType.GetNavigations().ToList();
            foreach (var type in entityType.GetDerivedTypes())
            {
                navigations.AddRange(type.GetNavigations());
            }
            var navs = navigations
                                 .Select(n => new
                                 {
                                     Navigation = n,
                                     IsOwned = n.IsOwned(),
                                     IsAssociated = n.IsAssociated(),
                                     TargetType = n.GetTargetType()
                                 })
                                 .Where(n => (n.IsAssociated || n.IsOwned))
                                 .ToList();

            //visited.Add(entityType);

            if (navs.Any())
            {
                // there are children. call recursively.
                foreach (var nav in navs)
                {
                    if (nav.IsOwned)
                        GetIncludePaths(nav.TargetType, currentPath + "." + nav.Navigation.Name, ref paths);
                    else
                        paths.Add((currentPath + "." + nav.Navigation.Name).TrimStart('.'));
                }
            }
            else if (currentPath != null)
            {
                // no more children. path ends here.
                paths.Add(currentPath.TrimStart('.'));
            }
        }
        
        public virtual TEntity Load<TEntity>(params object[] keyValues)
            where TEntity : class
        {
            IEntityServices<TEntity> entityServices = _entityServicesFactory.GetEntityServices<TEntity>();
            Expression<Func<TEntity, bool>> keyFilter = entityServices.CreateFindByKeyExpression(keyValues);

            IQueryable<TEntity> query = GetBaseQuery<TEntity>().AsNoTracking();
            query = (IQueryable<TEntity>)_eventManager.OnRootLoading(query, _dbContext).Queryable;

            TEntity entity = query.SingleOrDefault(keyFilter);
            if (entity != null)
                _eventManager.OnRootLoaded(entity, _dbContext);

            return entity;
        }

        public virtual async Task<TEntity> LoadAsync<TEntity>(params object[] keyValues)
            where TEntity : class
        {
            IEntityServices<TEntity> entityServices = _entityServicesFactory.GetEntityServices<TEntity>();
            Expression<Func<TEntity, bool>> keyFilter = entityServices.CreateFindByKeyExpression(keyValues);

            IQueryable<TEntity> query = GetBaseQuery<TEntity>().AsNoTracking();
            query = (IQueryable<TEntity>)_eventManager.OnRootLoading(query, _dbContext).Queryable;

            TEntity entity = await query.SingleOrDefaultAsync(keyFilter);
            if (entity != null)
                _eventManager.OnRootLoaded(entity, _dbContext);

            return entity;
        }
        
        public virtual List<TResult> Load<TEntity, TResult>(Func<IQueryable<TEntity>, IQueryable<TResult>> queryConfig)
            where TEntity : class
            where TResult : class
        {
            IQueryable<TEntity> query = GetBaseQuery<TEntity>().AsNoTracking();
            IQueryable<TResult> configQuery = queryConfig?.Invoke(query);

            configQuery = (IQueryable<TResult>)_eventManager.OnRootLoading(configQuery, _dbContext).Queryable;

            List<TResult> entities = configQuery.ToList();
            for (int i = 0; i < entities.Count; i++)
                entities[i] = (TResult)_eventManager.OnRootLoaded(entities[i], _dbContext).Root;

            return entities;
        }

        public virtual async Task<List<TResult>> LoadAsync<TEntity, TResult>(Func<IQueryable<TEntity>, IQueryable<TResult>> queryConfig)
            where TEntity : class
            where TResult : class
        {
            IQueryable<TEntity> query = GetBaseQuery<TEntity>().AsNoTracking();
            IQueryable<TResult> configQuery = queryConfig?.Invoke(query);

            configQuery = (IQueryable<TResult>)_eventManager.OnRootLoading(configQuery, _dbContext).Queryable;

            List<TResult> entities = await configQuery.ToListAsync();
            for (int i = 0; i < entities.Count; i++)
                entities[i] = (TResult)_eventManager.OnRootLoaded(entities[i], _dbContext).Root;

            return entities;
        }
        
        public virtual List<TEntity> Load<TEntity>(Func<IQueryable<TEntity>, IQueryable<TEntity>> queryConfig)
            where TEntity : class
        {
            return Load<TEntity, TEntity>(queryConfig);
        }

        public virtual async Task<List<TEntity>> LoadAsync<TEntity>(Func<IQueryable<TEntity>, IQueryable<TEntity>> queryConfig)
            where TEntity : class
        {
            return await LoadAsync<TEntity, TEntity>(queryConfig);
        }
        
        public TEntity LoadPersisted<TEntity>(TEntity entity)
            where TEntity : class
        {
            IEntityServices<TEntity> entityServices = _entityServicesFactory.GetEntityServices<TEntity>();
            return (LoadPersisted<TEntity>(new[] { entityServices.GetKeyValue(entity) })).FirstOrDefault();
        }

        public async Task<TEntity> LoadPersistedAsync<TEntity>(TEntity entity)
            where TEntity : class
        {
            IEntityServices<TEntity> entityServices = _entityServicesFactory.GetEntityServices<TEntity>();
            return (await LoadPersistedAsync<TEntity>(new[] { entityServices.GetKeyValue(entity) })).FirstOrDefault();
        }
        
        public IList<TEntity> LoadPersisted<TEntity>(KeyValue[] keys)
            where TEntity : class
        {
            IEntityServices<TEntity> entityServices = _entityServicesFactory.GetEntityServices<TEntity>();
            Expression<Func<TEntity, bool>> keyFilter = entityServices.CreateFilterByKeysExpression(keys);

            IQueryable<TEntity> query = GetBaseQuery<TEntity>().AsTracking().Where(keyFilter);
            query = (IQueryable<TEntity>)_eventManager.OnRootLoading(query, _dbContext).Queryable;

            IList<TEntity> persisted = query.ToList();
            if (persisted != null)
            {
                for (int i = 0; i < persisted.Count; i++)
                {
                    persisted[i] = (TEntity)_eventManager.OnRootLoaded(persisted[i], _dbContext).Root;
                }
            }

            return persisted;
        }

        public async Task<IList<TEntity>> LoadPersistedAsync<TEntity>(KeyValue[] keys)
            where TEntity : class
        {
            IEntityServices<TEntity> entityServices = _entityServicesFactory.GetEntityServices<TEntity>();
            Expression<Func<TEntity, bool>> keyFilter = entityServices.CreateFilterByKeysExpression(keys);

            IQueryable<TEntity> query = GetBaseQuery<TEntity>().AsTracking().Where(keyFilter);
            query = (IQueryable<TEntity>)_eventManager.OnRootLoading(query, _dbContext).Queryable;

            IList<TEntity> persisted = await query.ToListAsync();
            if (persisted != null)
            {
                for(int i = 0; i <persisted.Count; i++)
                {
                    persisted[i] = (TEntity)_eventManager.OnRootLoaded(persisted[i], _dbContext).Root;
                }
            }

            return persisted;
        }
    }
}