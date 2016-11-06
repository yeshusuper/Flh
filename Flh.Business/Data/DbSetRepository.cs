using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using EntityFramework.Extensions;

namespace Flh.Business.Data
{
    internal class DbSetRepository<TContext, TEntity> : Flh.Data.IRepository<TEntity>
        where TContext : DbContext, IObjectContextAdapter
        where TEntity : class
    {
        public readonly TContext Context;

        public DbSetRepository(TContext context)
        {
            this.Context = context;
        }

        public TEntity Add(TEntity newEntity)
        {
            return DbSet.Add(newEntity);
        }

        public IEnumerable<TEntity> AddRange(IEnumerable<TEntity> newEntitys)
        {
            return DbSet.AddRange(newEntitys);
        }

        public TEntity Delete(TEntity deleteEntity)
        {
            return DbSet.Remove(deleteEntity);
        }

        public IEnumerable<TEntity> DeleteRange(IEnumerable<TEntity> deleteEntitys)
        {
            return DbSet.RemoveRange(deleteEntitys);
        }

        public IQueryable<TEntity> Entities
        {
            get { return DbSet; }
        }

        public IQueryable<TEntity> EntitiesWith(params string[] includes)
        {
            if (includes == null || includes.Length == 0) return Entities;
            DbQuery<TEntity> entities = DbSet;
            for (int i = 0; i < includes.Length; i++)
            {
                entities = entities.Include(includes[i]);
            }
            return entities;
        }

        public IQueryable<TEntity> EntitiesWith<TProperty>(params System.Linq.Expressions.Expression<Func<TEntity, TProperty>>[] includes)
        {
            if (includes == null || includes.Length == 0) return Entities;
            IQueryable<TEntity> entities = DbSet;
            for (int i = 0; i < includes.Length; i++)
            {
                entities = entities.Include(includes[i]);
            }
            return entities;
        }

        public void SaveChanges()
        {
            try
            {
                Context.SaveChanges();
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                var sb = new StringBuilder();
                foreach (var item in ex.EntityValidationErrors)
                {
                    foreach (var error in item.ValidationErrors)
                    {
                        sb.AppendLine(error.PropertyName + ":" + error.ErrorMessage);
                    }
                }
                Log.LoggerResolver.Current.Fail(sb.ToString());
                throw ex;
            }
        }

        private DbSet<TEntity> DbSet { get { return Context.Set<TEntity>(); } }

        public int Delete(Expression<Func<TEntity, bool>> filter)
        {
            var result = DbSet.Where(filter).Delete();
            var caches = Context.ObjectContext.ObjectStateManager.GetObjectStateEntries(EntityState.Unchanged | EntityState.Added | EntityState.Modified);
            var entitys = caches.Where(cache => cache.Entity is TEntity).Select(cache => cache.Entity as TEntity)
                            .Where(filter.Compile());
            foreach (var entity in entitys)
            {
                Context.ObjectContext.Detach(entity);
            }
            return result;
        }

        public int Update(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TEntity>> update)
        {
            var result = DbSet.Where(filter).Update(update);
            var caches = Context.ObjectContext.ObjectStateManager.GetObjectStateEntries(EntityState.Unchanged);
            var entitys = caches.Where(cache => cache.Entity is TEntity).Select(cache => cache.Entity as TEntity)
                            .Where(filter.Compile());
            Context.ObjectContext.Refresh(System.Data.Entity.Core.Objects.RefreshMode.StoreWins, entitys);
            return result;
        }

        ~DbSetRepository()
        {
            if (Context != null)
            {
                Context.Dispose();
            }
        }
    }
}
