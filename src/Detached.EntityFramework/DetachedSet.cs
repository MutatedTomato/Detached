using Detached.EntityFramework.Events;
using Detached.EntityFramework.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Detached.EntityFramework
{
    public class DetachedSet<TEntity> : IDetachedSet<TEntity>, IDetachedSet
        where TEntity : class
    {
        #region Fields

        IDetachedContextServices _detachedServices;
        IEventManager _eventManager;
        DbContext _dbContext;

        #endregion

        #region Ctor.

        public DetachedSet(IDetachedContextServices detachedServices, IEventManager eventManager)
        {
            _detachedServices = detachedServices;
            _dbContext = detachedServices.DetachedContext.DbContext;
            _eventManager = eventManager;
        }

        #endregion

        public Type EntityType
        {
            get
            {
                return typeof(TEntity);
            }
        }

        public IQueryable<TEntity> GetBaseQuery()
        {
            return _detachedServices.LoadServices.GetBaseQuery<TEntity>();
        }

        public TEntity Load(params object[] key)
        {
            return _detachedServices.LoadServices.Load<TEntity>(key);
        }

        public async Task<TEntity> LoadAsync(params object[] key)
        {
            return await _detachedServices.LoadServices.LoadAsync<TEntity>(key);
        }

        public List<TEntity> Load(Func<IQueryable<TEntity>, IQueryable<TEntity>> queryConfig)
        {
            return _detachedServices.LoadServices.Load<TEntity>(queryConfig);
        }

        public async Task<List<TEntity>> LoadAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>> queryConfig)
        {
            return await _detachedServices.LoadServices.LoadAsync<TEntity>(queryConfig);
        }
        
        public List<TResult> Load<TResult>(Func<IQueryable<TEntity>, IQueryable<TResult>> queryConfig)
            where TResult : class
        {
            return _detachedServices.LoadServices.Load<TEntity, TResult>(queryConfig);
        }

        public async Task<List<TResult>> LoadAsync<TResult>(Func<IQueryable<TEntity>, IQueryable<TResult>> queryConfig)
            where TResult : class
        {
            return await _detachedServices.LoadServices.LoadAsync<TEntity, TResult>(queryConfig);
        }

        public virtual TEntity Update(TEntity root)
        {
            // temporally disabled autodetect changes
            bool autoDetectChanges = _dbContext.ChangeTracker.AutoDetectChangesEnabled;
            _dbContext.ChangeTracker.AutoDetectChangesEnabled = false;

            TEntity persisted = _detachedServices.LoadServices.LoadPersisted<TEntity>(root);
            if (persisted == null) // add new entity.
            {
                persisted = (TEntity)_detachedServices.UpdateServices.Add(root).Entity;
            }
            else
            {
                persisted = (TEntity)_eventManager.OnRootLoaded(persisted, _dbContext).Root; // entity to merge has been loaded.
                persisted = (TEntity)_detachedServices.UpdateServices.Merge(root, persisted).Entity; // merge existing entity.
                // call root loaded again for the modified entity.
                persisted = (TEntity)_eventManager.OnRootLoaded(persisted, _dbContext).Root;
            }
            // re-enable autodetect changes.
            _dbContext.ChangeTracker.AutoDetectChangesEnabled = autoDetectChanges;

            return persisted;
        }

        public virtual async Task<TEntity> UpdateAsync(TEntity root)
        {
            // temporally disabled autodetect changes
            bool autoDetectChanges = _dbContext.ChangeTracker.AutoDetectChangesEnabled;
            _dbContext.ChangeTracker.AutoDetectChangesEnabled = false;

            TEntity persisted = await _detachedServices.LoadServices.LoadPersistedAsync<TEntity>(root);
            if (persisted == null) // add new entity.
            {
                persisted = (TEntity)_detachedServices.UpdateServices.Add(root).Entity;
            }
            else
            {
                persisted = (TEntity)_eventManager.OnRootLoaded(persisted, _dbContext).Root; // entity to merge has been loaded.
                persisted = (TEntity)_detachedServices.UpdateServices.Merge(root, persisted).Entity; // merge existing entity.
                // call root loaded again for the modified entity.
                persisted = (TEntity)_eventManager.OnRootLoaded(persisted, _dbContext).Root;
            }
            // re-enable autodetect changes.
            _dbContext.ChangeTracker.AutoDetectChangesEnabled = autoDetectChanges;

            return persisted;
        }

        public virtual void Delete(params object[] keyValues)
        {
            Delete(new KeyValue(keyValues));
        }

        public virtual async Task DeleteAsync(params object[] keyValues)
        {
            await DeleteAsync(new KeyValue(keyValues));
        }
        
        public virtual void Delete(params KeyValue[] keys)
        {
            // temporally disable autodetect changes.
            bool autoDetectChanges = _dbContext.ChangeTracker.AutoDetectChangesEnabled;
            _dbContext.ChangeTracker.AutoDetectChangesEnabled = false;

            IList<TEntity> persisted = _detachedServices.LoadServices.LoadPersisted<TEntity>(keys);
            if (persisted != null)
            {
                foreach (TEntity entity in persisted)
                {
                    _detachedServices.UpdateServices.Delete(entity);
                }
            }
            // re-enable autodetect changes.
            _dbContext.ChangeTracker.AutoDetectChangesEnabled = true;
        }

        public virtual async Task DeleteAsync(params KeyValue[] keys)
        {
            // temporally disable autodetect changes.
            bool autoDetectChanges = _dbContext.ChangeTracker.AutoDetectChangesEnabled;
            _dbContext.ChangeTracker.AutoDetectChangesEnabled = false;

            IList<TEntity> persisted = await _detachedServices.LoadServices.LoadPersistedAsync<TEntity>(keys);
            if (persisted != null)
            {
                foreach (TEntity entity in persisted)
                {
                    _detachedServices.UpdateServices.Delete(entity);
                }
            }
            // re-enable autodetect changes.
            _dbContext.ChangeTracker.AutoDetectChangesEnabled = true;
        }

        IQueryable IDetachedSet.GetBaseQuery()
        {
            return GetBaseQuery();
        }
        
        object IDetachedSet.Load(params object[] key)
        {
            return Load(key);
        }

        async Task<object> IDetachedSet.LoadAsync(params object[] key)
        {
            return await LoadAsync(key);
        }

        public object Update(object root)
        {
            return Update((TEntity)root);
        }

        public async Task<object> UpdateAsync(object root)
        {
            return await UpdateAsync((TEntity)root);
        }
    }
}