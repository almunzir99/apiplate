using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using apiplate.Models;
using Microsoft.AspNetCore.JsonPatch;

namespace apiplate.Repository
{
    public interface IRepository<TModel> 
    where TModel:BaseModel
    {
        Task<IList<TModel>> ListAsync();
        Task<TModel> SingleAsync(int id);
        Task<TModel> SingleAsync(Expression<Func<TModel, bool>> predicate);
        Task<TModel> CreateAsync(TModel newItem);
        Task<TModel> UpdateAsync(int id, TModel newItem);
        Task<TModel> UpdateAsync(int id, JsonPatchDocument<TModel> newItem);

        Task DeleteAsync(int id);
        Task<int> GetTotalRecords();
        IQueryable<TModel> IncludeableDbSet {get; set;}
        Task<int> complete();

    }
}