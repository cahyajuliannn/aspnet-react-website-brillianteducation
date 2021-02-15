using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Bases
{
    public interface BaseModel
    {
        int Id { get; set; }
        string Value { get; set; }
    }
}
