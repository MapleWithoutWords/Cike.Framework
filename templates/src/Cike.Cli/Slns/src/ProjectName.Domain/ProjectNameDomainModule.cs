namespace ProjectName.Domain;

[DependsOn([
    typeof(ProjectNameDomainSharedModule),
    typeof(CikeCachingModule),
    typeof(CikeDomainModule),
    ])]
public class ProjectNameDomainModule : CikeModule
{

}
