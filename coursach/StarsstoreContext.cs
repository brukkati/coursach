using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace coursach
{
    class StarsstoreContext: DbContext
    {
        public StarsstoreContext() : base("DbConnection") { }
        public DbSet<Item> Items { get; set; }
    }
}
