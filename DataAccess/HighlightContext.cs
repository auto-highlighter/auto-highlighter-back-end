using auto_highlighter_back_end.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace auto_highlighter_back_end.DataAccess
{
    public class HighlightContext : DbContext
    {
        public HighlightContext(DbContextOptions options) : base(options) { }
        public DbSet<HighlightEntity> HighlightEntity { get; set; }
    }
}
