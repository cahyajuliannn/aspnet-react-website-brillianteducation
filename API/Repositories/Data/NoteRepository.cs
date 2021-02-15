using API.Context;
using API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Repositories
{
    public class NoteRepository : GeneralRepository<Note, MyContext>
    {
        MyContext _myContext;
        public NoteRepository(MyContext myContext) : base(myContext)
        {
            _myContext = myContext;
        }
    }
}
