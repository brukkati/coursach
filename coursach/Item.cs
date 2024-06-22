using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace coursach
{
    public class Item
    {
        [Key]
        public int id { get; set; }
        public string Name { get; set; }
        public string Price { get; set; }
        public string Description { get; set; } 
    }
}
