using apiplate.DataBase;
using apiplate.Models;
using apiplate.Interfaces;
using apiplate.Repository;
using apiplate.Resources;
using apiplate.Utils.URI;
using AutoMapper;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using apiplate.Resources.Wrappers.Filters;
using System;
using Microsoft.AspNetCore.JsonPatch;
using System.Linq;
using System.Text;

namespace apiplate.Services
{
    public class BaseService<TModel, TResource, TRequest> : IBaseService<TModel, TResource, TRequest>
     where TModel : BaseModel where TResource : BaseResource
    {
        protected readonly IRepository<TModel> _repository;
        protected readonly IMapper _mapper;
        private readonly IRepository<Admin> _adminsRepository;
        public BaseService(IMapper mapper, IUriService uriSerivce, IRepository<TModel> repository, IRepository<Admin> adminsRepository)
        {
            _repository = repository;
            _mapper = mapper;
            _adminsRepository = adminsRepository;
        }

        public async virtual Task CreateActivity(int userId, int rowId, string action)
        {
            var tableTitle = typeof(TModel).Name;
            Activity activity = new Activity(userId, tableTitle, rowId, action, DateTime.Now);
            var user = await _adminsRepository.SingleAsync(userId);
            user.Activities.Add(activity);
            await _adminsRepository.complete();
        }

        public async virtual Task<TResource> CreateAsync(TRequest newItem, int userId)
        {
            try
            {
                var mappedItem = _mapper.Map<TRequest, TModel>(newItem);
                mappedItem.CreatedAt = DateTime.Now;
                mappedItem.LastUpdate = DateTime.Now;
                await _repository.CreateAsync(mappedItem);
                var result = _mapper.Map<TModel, TResource>(mappedItem);
                await _repository.complete();
                return result;
            }

            catch (System.Exception e)
            {

                throw e;
            }

        }

        public async virtual Task DeleteAsync(int id)
        {
            try
            {
                await _repository.DeleteAsync(id);
                await _repository.complete();

            }
            catch (System.Exception e)
            {

                throw e;
            }

        }

        public async virtual Task<FileContentResult> ExportToCSV()
        {
            // list of properties
            Type type = typeof(TModel);
            var title = type.Name;
            var properties = type.GetProperties().Where(c => c.PropertyType.IsPrimitive ||
                     c.PropertyType == typeof(String)
                     || c.PropertyType == typeof(DateTime)).ToList();
            var propertiesNames = properties.Select(c => c.Name);
            var data = new StringBuilder();

            string header = "";
            //create header 
            int index = 0;
            foreach (var name in propertiesNames)
            {
                header += name;
                if (index < propertiesNames.Count() - 1)
                    header += ",";
                index++;
            }
            data.AppendLine(header);
            var list = await _repository.ListAsync();
            var filteredProps = properties.Where(c => c.PropertyType.IsPrimitive ||
                     c.PropertyType == typeof(String)
                     || c.PropertyType == typeof(DateTime)).ToList();
            foreach (var item in list)
            {
                string value = "";
                int i = 0;
                foreach (var prop in filteredProps)
                {

                    var propValue = prop.GetValue(item)?.ToString();
                    propValue = propValue?.Replace(",", "");
                    value += propValue;
                    if (i < filteredProps.Count() - 1)
                        value += ",";
                    i++;

                }
                data.AppendLine(value);
            }
            var result = new FileContentResult(Encoding.UTF8.GetBytes(data.ToString()),
            "text/csv")
            {
                FileDownloadName = $"{title}.csv",
            };
            return result;
        }

        public async virtual Task<int> GetTotalRecords() => await _repository.GetTotalRecords();

        public async virtual Task<IList<TResource>> ListAsync(PaginationFilter filter, IList<Func<TModel, bool>> conditions, string search = "", string orderBy = "LastUpdate", bool ascending = true)
        {
            if (search == null) search = "";
            var validFilter = (filter == null) ?
            new PaginationFilter()
            : new PaginationFilter(filter.PageIndex, filter.PageSize);
            var list = await _repository.ListAsync();
            if (conditions != default)
            {
                foreach (var condition in conditions)
                {
                    list = list.Where(condition).ToList();
                }
            }
            list = list
            .Where(c => (GetSearchPropValue(c) == null)
            ? true
            : GetSearchPropValue(c).ToLower().Contains(search.ToLower())).ToList();
            list = list
            .Skip((validFilter.PageIndex - 1) * validFilter.PageSize)
            .Take(validFilter.PageSize).ToList();
            var result = _mapper.Map<IList<TModel>, IList<TResource>>(list);
            result = OrderBy(result, orderBy, ascending);
            return result;
        }
        protected List<TResource> OrderBy(IList<TResource> list, string prop, Boolean ascending)
        {
            //Get ordering Prop
            var type = typeof(TResource);
            var orderedList = list.OrderBy(c => c.LastUpdate).ToList();
            var orderProp = type.GetProperties().SingleOrDefault(c => c.Name.ToLower() == prop.ToLower());
            if (orderProp == null)
                throw new Exception("ordering property isn't available");
            if (ascending)
                orderedList = list.OrderBy(c => orderProp.GetValue(c, null)).ToList();
            else
                orderedList = list.OrderByDescending(c => orderProp.GetValue(c, null)).ToList();
            return orderedList;

        }
        public async virtual Task<TResource> SingleAsync(int id)
        {
            var result = await _repository.SingleAsync(id);
            var mappedResult = _mapper.Map<TModel, TResource>(result);
            return mappedResult;
        }

        public virtual async Task<TResource> UpdateAsync(int id, TRequest newItem)
        {
            var mappedItem = _mapper.Map<TRequest, TModel>(newItem);
            var result = await _repository.UpdateAsync(id, mappedItem);
            await _repository.complete();
            var mappedResult = _mapper.Map<TModel, TResource>(result);
            return mappedResult;
        }


        public async virtual Task<TResource> UpdateAsync(int id, JsonPatchDocument<TModel> newItem)
        {
            try
            {
                var result = await _repository.UpdateAsync(id, newItem);
                await _repository.complete();
                var mappedResult = _mapper.Map<TModel, TResource>(result);
                return mappedResult;
            }
            catch (System.Exception e)
            {

                throw e;
            }
        }
        protected virtual string GetSearchPropValue(TModel obj)
        {
            var type = typeof(TModel);
            var searchProp = type.GetProperties().SingleOrDefault(c => c.Name.ToLower() == "title");
            var propValue = searchProp?.GetValue(obj).ToString();
            return propValue;
        }

    }
}