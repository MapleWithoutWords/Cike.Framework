namespace ProjectName.Application;

[DependsOn([
    typeof(ProjectNameDomainModule),
    typeof(ProjectApplicationContractsModule),
    typeof(CikeCqrsModule),
    typeof(CikeEventBusLocalModule),
    ])]
public class ProjectNameApplicationModule : CikeModule
{

}
