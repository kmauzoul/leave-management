using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace leave_management.Contracts
{
    //Base repository that does CRUD
    public interface IRepositoryBase<T> where T : class
    {
        //Return all from Database
        Task<ICollection<T>> FindAll();

        //Get class by it's Id
        Task<T> FindById(int id);


        Task<bool> Exists(int id);
       
        Task<bool> Create(T entity);

        Task<bool> Save();

        Task<bool> Update(T entity);

        Task<bool> Delete(T entity);
    }

    public interface IGenericRepository<T> where T: class
    {
        
        Task<IList<T>> FindAll(
            Expression<Func<T, bool>> expression = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            List<string> includes = null);

        //Get class by it's Id
        Task<T> Find(
            Expression<Func<T, bool>> expression,
            List<string> includes = null);

        Task<bool> Exists(Expression<Func<T, bool>> expression = null);
        Task Create(T entity);
        void Update(T entity);

        void Delete(T entity);
    }
}
