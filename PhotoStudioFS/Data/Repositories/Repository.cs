using Microsoft.EntityFrameworkCore;
using PhotoStudioFS.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PhotoStudioFS.Data.Repositories
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        protected readonly DbContext context;
        private DbSet<TEntity> _entities;

        public Repository(DbContext _context)
        {
            context = _context;
            _entities = context.Set<TEntity>();
        }

        public async Task Add(TEntity entity)
        {
            context.Set<TEntity>().Add(entity);
            await _entities.AddAsync(entity);
        }

        public async Task AddRange(IEnumerable<TEntity> entities)
        {
            await _entities.AddRangeAsync(entities);
        }

        public async Task<IEnumerable<TEntity>> Find(Expression<Func<TEntity, bool>> predicate)
        {
            return await _entities.AsNoTracking().Where(predicate).ToListAsync();
        }

        public async Task<TEntity> Get(int id)
        {
            //return context.Set<TEntity>().Find(id);
            return await _entities.FindAsync(id);
        }

        public async Task<IEnumerable<TEntity>> GetAll()
        {
            return await _entities.AsNoTracking().ToListAsync();
        }

        public void Remove(TEntity entity)
        {
            _entities.Remove(entity);
        }

        public void RemoveRange(IEnumerable<TEntity> entities)
        {
            _entities.RemoveRange(entities);
        }

        public async Task<TEntity> SingleOrDefault(Expression<Func<TEntity, bool>> predicate)
        {
            return await _entities.SingleOrDefaultAsync(predicate);
        }

        public void Update(TEntity entity)
        {
            _entities.Update(entity);
        }
    }
}
