using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using apiplate.Models;
using apiplate.Interfaces;
using apiplate.Resources;
using apiplate.Resources.Wrappers.Filters;
using apiplate.Extensions;
using apiplate.Helpers;
using apiplate.Utils.URI;
using apiplate.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.JsonPatch;
using apiplate.Attributes.Permissions;

namespace apiplate.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public abstract class BaseController<TModel, TResource, TRequest, TService> : ControllerBase, IBaseController<TModel, TResource, TRequest, TService>
where TModel : BaseModel where TResource : BaseResource where TService : IBaseService<TModel, TResource, TRequest>
    {
        protected readonly TService _service;
        protected readonly IUriService _uriSerivce;
        public abstract string PermissionTitle { get; }

        public BaseController(TService service, IUriService uriSerivce)
        {
            _service = service;
            _uriSerivce = uriSerivce;

        }
        [Permission(true, PermissionTypes.CREATE)]
        [HttpPost]
        public virtual async Task<IActionResult> PostAsync([FromBody] TRequest body)
        {
            try
            {
                int _currentUserId = int.Parse(HttpContext.User.GetClaimValue("id"));
                var result = await _service.CreateAsync(body, _currentUserId);
                var response = new Response<TResource>(data: result);
                //create Activity
                if (!IsAnonymous("PostAsync"))
                    await _service.CreateActivity(_currentUserId, result.Id.Value, "Create");
                return Ok(response);

            }
            catch (System.Exception e)
            {

                var response = new Response<TResource>(success: false, errors: new List<string>() { e.Message });
                return BadRequest(response);
            }
        }
        [Permission(true, PermissionTypes.DETELE)]
        [HttpDelete("{id}")]
        public virtual async Task<IActionResult> DeleteAsync(int id)
        {

            try
            {
                await _service.DeleteAsync(id);
                var response = new Response<TResource>(message: "Item Deleted Successfully");
                //create Activity
                int _currentUserId = int.Parse(HttpContext.User.GetClaimValue("id"));
                await _service.CreateActivity(_currentUserId, id, "Delete");
                return Ok(response);

            }
            catch (System.Exception e)
            {
                var response = new Response<TResource>(success: false, errors: new List<string>() { e.Message });
                return BadRequest(response);
            }
        }
        [Permission(true, PermissionTypes.READ)]
        [HttpGet]
        public virtual async Task<IActionResult> GetAsync([FromQuery] PaginationFilter filter = null, [FromQuery] string title = "", [FromQuery] string orderBy = "LastUpdate", Boolean ascending = true)
        {
            var validFilter = (filter == null)
           ? new PaginationFilter()
           : new PaginationFilter(pageIndex: filter.PageIndex, pageSize: filter.PageSize);
            int _currentUserId = int.Parse(HttpContext.User.GetClaimValue("id"));
            var result = await _service.ListAsync(filter, new List<Func<TModel, bool>>(), title, orderBy, ascending);
            var totalRecords = await _service.GetTotalRecords();
            return Ok(PaginationHelper.CreatePagedResponse<TResource>(result,
            validFilter, _uriSerivce, totalRecords, Request.Path.Value));
        }
        [Permission(true, PermissionTypes.READ)]
        [HttpGet("{id}")]
        public virtual async Task<IActionResult> SingleAsync(int id)
        {
            try
            {
                var result = await _service.SingleAsync(id);
                var response = new Response<TResource>(data: result);
                return Ok(response);

            }
            catch (System.Exception e)
            {

                var response = new Response<TResource>(success: false, errors: new List<string>() { e.Message });
                return BadRequest(response);
            }
        }
        [Permission(true, PermissionTypes.UPDATE)]
        [HttpPut("{id}")]
        public virtual async Task<IActionResult> PutAsync(int id, [FromBody] TRequest body)
        {
            try
            {
                var result = await _service.UpdateAsync(id, body);
                var response = new Response<TResource>(data: result);
                //create Activity
                int _currentUserId = int.Parse(HttpContext.User.GetClaimValue("id"));
                await _service.CreateActivity(_currentUserId, id, "Update");
                return Ok(response);

            }
            catch (System.Exception e)
            {
                var response = new Response<TResource>(success: false, errors: new List<string>() { e.Message });
                return BadRequest(response);
            }
        }
        [Permission(true, PermissionTypes.UPDATE)]
        [HttpPatch("{id}")]
        public virtual async Task<IActionResult> PatchAsync(int id, [FromBody] JsonPatchDocument<TModel> body)
        {
            try
            {
                var result = await _service.UpdateAsync(id, body);
                var response = new Response<TResource>(data: result);
                return Ok(response);

            }
            catch (System.Exception e)
            {
                var response = new Response<TResource>(success: false, errors: new List<string>() { e.Message });
                return BadRequest(response);
            }
        }
        [HttpGet("export/csv")]
        public virtual async Task<IActionResult> ExportToCSV()
        {
            var result = await this._service.ExportToCSV();
            return result;

        }
        private bool IsAnonymous(string name)
        {
            var type = this.GetType();
            var targetMethod = type.GetMethods().FirstOrDefault(c => c.Name == name);
            var isAnonymous = targetMethod.GetCustomAttributes(true).SingleOrDefault(c => c.GetType().Name == "AllowAnonymousAttribute");
            return isAnonymous == null ? false : true;

        }
        protected int GetCurrentUserId()
        {
            int _currentUserId = int.Parse(HttpContext.User.GetClaimValue("id"));
            return _currentUserId;
        }
        protected string GetCurrentUserType()
        {
            string type = HttpContext.User.GetClaimValue("http://schemas.microsoft.com/ws/2008/06/identity/claims/role");
            return type;
        }

    }
}