using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Flh.Data
{

    public interface IRepository<TEntity>
    {
        TEntity Add(TEntity newEntity);
        IEnumerable<TEntity> AddRange(IEnumerable<TEntity> newEntitys);
        TEntity Delete(TEntity deleteEntity);
        IEnumerable<TEntity> DeleteRange(IEnumerable<TEntity> deleteEntitys);
        void SaveChanges();
        IQueryable<TEntity> Entities { get; }
        IQueryable<TEntity> EntitiesWith(params string[] includes);
        IQueryable<TEntity> EntitiesWith<TProperty>(params Expression<Func<TEntity, TProperty>>[] includes);
        int Delete(Expression<Func<TEntity, bool>> filter);
        int Update(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TEntity>> update);
    }
}
