using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Repositories.Interface
{
    public interface IRepository<T> where T: class
    {
        Task<List<T>> GetAll();
        Task<T> GetById(int Id);
        Task<int> Update(T entity);
        Task<int> Create(T entity);
        Task<int> Delete(int Id);

    }
}
