using API.Bases;
using API.Context;
using API.Repositories.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Repositories
{
    public class GeneralRepository<TEntity, TContext> : IRepository<TEntity>
        where TEntity:class, BaseModel
        where TContext:MyContext
    {
        private MyContext _myContext;
        public GeneralRepository(MyContext context)
        {
            _myContext = context;
        }
        public async Task<int> Create(TEntity entity)
        {
            await _myContext.Set<TEntity>().AddAsync(entity);
            var CreateItem = await _myContext.SaveChangesAsync();
            return CreateItem;
        }

        public async Task<int> Delete(int Id)
        {
            var item = await GetById(Id);
            if (item == null)
            {
                return 0;
            }
            _myContext.Entry(item).State = EntityState.Deleted;
            var delete = await _myContext.SaveChangesAsync();
            return delete;
        }

        public virtual async Task<List<TEntity>> GetAll()
        {
            var GetAll = await _myContext.Set<TEntity>().Where(Q => Q.Id > 0).ToListAsync();
            if (!GetAll.Count().Equals(0))
            {
                return GetAll;
            }
            return null;
        }

        public async Task<TEntity> GetById(int Id)
        {
            var item = await _myContext.Set<TEntity>().FindAsync(Id);
            if (item == null)
            {
                return null;
            }
            return item;
        }

        public async Task<int> Update(TEntity entity)
        {
            _myContext.Entry(entity).State = EntityState.Modified;
            var edit = await _myContext.SaveChangesAsync();
            return edit;
        }
    }
}
