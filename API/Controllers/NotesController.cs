using API.Bases;
using API.Context;
using API.Models;
using API.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotesController : BaseController<Note, NoteRepository>
    {
        private readonly NoteRepository _noteRepository;
        private readonly MyContext _myContext;


        public NotesController(NoteRepository noteRepository, MyContext myContext ): base(noteRepository)
        {
            this._noteRepository = noteRepository;
            this._myContext = myContext;
        }

        [HttpPut("{Id}")]
        public async Task<ActionResult<int>> Update(int Id, Note entity)
        {
            var update = await _noteRepository.GetById(Id);
            if(update != null)
            {
                update.Value = entity.Value;
                await _noteRepository.Update(update);
                return Ok("Data has been updated");
            }
            return BadRequest("Failed to update data. Please try again.");
        }
    }
}
