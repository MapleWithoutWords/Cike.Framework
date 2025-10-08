namespace ProjectName.Application.Contracts;

[DependsOn(
    typeof(ProjectNameDomainSharedModule),
    typeof(CikeContractsModule)
)]
public class ProjectApplicationContractsModule : CikeModule
{

}
