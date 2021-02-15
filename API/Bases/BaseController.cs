using API.Repositories.Interface;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Bases
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaseController<TEntity, TRepository> : ControllerBase
        where TEntity:class
        where TRepository:IRepository<TEntity>
    
    {
        private IRepository<TEntity> _repository;
        public BaseController(TRepository repository)
        {
            this._repository = repository;
        }
        [HttpGet]
        public async Task<IEnumerable<TEntity>> GetAll() => await _repository.GetAll();

        [HttpGet("{Id}")]
        public async Task<ActionResult<TEntity>> GetById(int Id) => await _repository.GetById(Id);

        [HttpPost]
        public async Task<ActionResult<TEntity>> Post (TEntity entity)
        {
            var data = await _repository.Create(entity);
            if (data != 0)
            {
                return Ok("Data has been saved");
            }
            return BadRequest("Failed to save data");
        }
        [HttpDelete("{Id}")]
        public async Task<ActionResult<int>> Delete(int Id)
        {
            var deleted = await _repository.Delete(Id);
            if (deleted.Equals(null))
            {
                return NotFound("Data not found");
            }
            return deleted;
        }
    }
}
