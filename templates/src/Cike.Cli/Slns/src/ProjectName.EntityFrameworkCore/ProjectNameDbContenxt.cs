namespace ProjectName.EntityFrameworkCore;

public class ProjectNameDbContenxt : CikeDbContext<ProjectNameDbContenxt>
{
    public ProjectNameDbContenxt(DbContextOptions<ProjectNameDbContenxt> options, IServiceProvider serviceProvider) : base(options, serviceProvider)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}
