using Microsoft.EntityFrameworkCore;

namespace Gityard.Application;

public partial class GityardContext : DbContext
{
    public GityardContext(DbContextOptions<GityardContext> options)
        : base(options)
    {
    }
}
