using API.Bases;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace API.Models
{
    public class Note : BaseModel
    {
        [Key]
        public int Id { get; set; }
        public string Value { get; set; }
    }
}
