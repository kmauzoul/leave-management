using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace leave_management.Contracts
{
    //Base repository that does CRUD
    public interface IRepositoryBase<T> where T : class
    {
        //Return all from Database
        ICollection<T> FindAll();

        //Get class by it's Id
        T FindById(int id);

       
        bool Create(T entity);

        bool Save();

        bool Update(T entity);

        bool Delete(T entity);
    }
}
