using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using apiplate.DataBase;
using apiplate.Models;
using apiplate.Extensions;
using apiplate.Resources;
using apiplate.Resources.Wrappers.Filters;
using apiplate.Utils.URI;
using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections;

namespace apiplate.Repository
{
    public class BaseRepository<TModel> : IRepository<TModel>
     where TModel : BaseModel
    {
        protected readonly ApiplateDbContext _context;
        protected readonly DbSet<TModel> _dbSet;

        private IQueryable<TModel> includeableDbSet;
        public IQueryable<TModel> IncludeableDbSet
        {
            get { return includeableDbSet; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value", "value shouldn't be null");
                includeableDbSet = value;
            }
        }

        protected IList<Expression<Func<TModel, object>>> propsToLoad;
        protected readonly IMapper _mapper;
        public BaseRepository(ApiplateDbContext context, IUriService uriSerivce, IMapper mapper)
        {

            _context = context;
            _dbSet = _context.Set<TModel>();
            includeableDbSet = _context.Set<TModel>();
            _mapper = mapper;
        }

        public virtual async Task<TModel> CreateAsync(TModel item)
        {
            try
            {
                await _dbSet.AddAsync(item);
                return item;
            }
            catch (System.Exception e)
            {

                throw e;
            }


        }

        public virtual async Task DeleteAsync(int id)
        {
            try
            {
                var target = await includeableDbSet.SingleOrDefaultAsync(c => c.Id == id);
                if (target == null)
                    throw new System.Exception("The target Item doesn't Exist");
                _dbSet.Remove(target);
            }
            catch (System.Exception e)
            {

                throw e;
            }


        }
        public virtual async Task<IList<TModel>> ListAsync()
        {

            var list = await includeableDbSet.ToListAsync();
            return list;

        }


        public virtual async Task<int> GetTotalRecords()
        {
            return await _dbSet.CountAsync();
        }
        public async Task<IList<TModel>> SearchAsync(Func<TModel, bool> predicate)
        {
            var result = await includeableDbSet.ToListAsync();
            result = result.Where(predicate).ToList();
            return result;
        }

        public virtual async Task<TModel> SingleAsync(int id)
        {
            var result = await includeableDbSet.SingleOrDefaultAsync(c => c.Id == id);
            if (result == null)
                throw new Exception("item is not found");
            return result;
        }
        public async Task<TModel> SingleAsync(Expression<Func<TModel, bool>> predicate)
        {
            var result = await includeableDbSet.SingleOrDefaultAsync(predicate);
            if (result == null)
                throw new Exception("item is not found");
            return result;
        }

        public virtual async Task<TModel> UpdateAsync(int id, TModel newItem)
        {
            try
            {
                var result = await includeableDbSet.SingleOrDefaultAsync(c => c.Id == id);
                if (result == null)
                    throw new Exception("item is not found");
                // update values using reflection
                ManualMap<TModel, TModel>(newItem, result,propsToExclude: new string[]{"Id","CreatedAt"});
                result.LastUpdate = DateTime.Now;
                return result;
            }
            catch (System.Exception e)
            {

                throw e;
            }
        }
        public virtual async Task<TModel> UpdateAsync(int id, JsonPatchDocument<TModel> newItem)
        {
            try
            {
                var result = await includeableDbSet.SingleOrDefaultAsync(c => c.Id == id);
                if (result == null)
                    throw new Exception("item is not found");
                newItem.ApplyTo(result);
                result.LastUpdate = DateTime.Now;
                return result;
            }
            catch (System.Exception e)
            {

                throw e;
            }
        }

        public async Task<int> complete()
        {
            try
            {
                return await _context.SaveChangesAsync();
            }
            catch (DbUpdateException exception)
            {
                throw new System.Exception(exception.Decode());
            }
        }
        public TDest ManualMap<TSource, TDest>(TSource source, TDest dest,IList<Func<TSource, bool>> conditions = null,string[] propsToExclude = null )
        {
            propsToExclude = propsToExclude ?? Array.Empty<string>();
            var sourceProps = source.GetType().GetProperties();
            var destProps = dest.GetType().GetProperties();

            foreach (var prop in destProps)
            {
                var propName = prop.Name;
                var propValue = prop.GetValue(dest);
                foreach (var sourceProp in sourceProps)
                {
                    var sourcePropName = sourceProp.Name;
                    var sourcePropValue = sourceProp.GetValue(source);
                    if (propName == sourcePropName 
                    && prop.PropertyType == sourceProp.PropertyType 
                    && sourcePropValue != default && propsToExclude.Contains(propName) == false)
                    {
                        if(conditions != null)
                        foreach (var condition in conditions)
                        {
                            var result = condition.Invoke(source);
                            if(result == false)
                            continue;
                        }
                        if (prop.PropertyType.IsPrimitive
                           || prop.PropertyType == typeof(Decimal)
                           || prop.PropertyType == typeof(String) || prop.PropertyType == typeof(DateTime))
                        {

                            prop.SetValue(dest,sourcePropValue);
                        }
                        else if (typeof(IEnumerable).IsAssignableFrom(source.GetType()))
                        {
                            var listPropValue = propValue as IEnumerable;
                            var listSourcePropValue = sourcePropValue as IEnumerable;

                            foreach (var value in listPropValue)
                            {
                                foreach (var sourceValue in listSourcePropValue)
                                {
                                    var manualMapMethodInfo = this.GetType().GetMethod("ManualMap");
                                    manualMapMethodInfo.MakeGenericMethod(sourceProp.PropertyType, prop.PropertyType)
                                    .Invoke(this, new[] { sourceValue, value,null,null});
                                }

                            }
                        }
                        else
                        {
                            var manualMapMethodInfo = this.GetType().GetMethod("ManualMap");
                            manualMapMethodInfo.MakeGenericMethod(sourceProp.PropertyType, prop.PropertyType)
                            .Invoke(this, new[] { sourcePropValue, propValue,null,null});
                        }
                    }
                }
            }
            return dest;
        }
    }


}