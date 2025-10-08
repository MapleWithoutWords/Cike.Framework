namespace ProjectName.EntityFrameworkCore;

[DependsOn([
    typeof(ProjectNameDomainModule),
    typeof(CikeDataEFCoreMySqlModule),
    ])]
public class ProjectNameEntityFrameworkCoreModule : CikeModule
{

}
