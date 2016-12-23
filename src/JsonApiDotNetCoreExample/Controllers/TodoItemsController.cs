using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Data;
using JsonApiDotNetCoreExample.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExample.Controllers
{
    [Route("api/[controller]")]
    public class TodoItemsController : JsonApiController<TodoItem>
    {
        public TodoItemsController(
            ILoggerFactory loggerFactory,
            AppDbContext context, IJsonApiContext jsonApiContext) 
            : base(loggerFactory, context, jsonApiContext)
        { }
    }
}
