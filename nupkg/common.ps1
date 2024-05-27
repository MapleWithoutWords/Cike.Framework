# Paths
$packFolder = (Get-Item -Path "./" -Verbose).FullName
$rootFolder = Join-Path $packFolder "../"

function Write-Info   
{
	param(
        [Parameter(Mandatory = $true)]
        [string]
        $text
    )

	Write-Host $text -ForegroundColor Black -BackgroundColor Green

	try 
	{
	   $host.UI.RawUI.WindowTitle = $text
	}		
	catch 
	{
		#Changing window title is not suppoerted!
	}
}

function Write-Error   
{
	param(
        [Parameter(Mandatory = $true)]
        [string]
        $text
    )

	Write-Host $text -ForegroundColor Red -BackgroundColor Black 
}

function Seperator   
{
	Write-Host ("_" * 100)  -ForegroundColor gray 
}

function Get-Current-Version { 
	$commonPropsFilePath = resolve-path "../Directory.Build.props"
	$commonPropsXmlCurrent = [xml](Get-Content $commonPropsFilePath ) 
	$currentVersion = $commonPropsXmlCurrent.Project.PropertyGroup.Version.Trim()
	return $currentVersion
}

function Get-Current-Branch {
	return git branch --show-current
}	   

function Read-File {
	param(
        [Parameter(Mandatory = $true)]
        [string]
        $filePath
    )
		
	$pathExists = Test-Path -Path $filePath -PathType Leaf
	if ($pathExists)
	{
		return Get-Content $filePath		
	}
	else{
		Write-Error  "$filePath path does not exist!"
	}
}

# List of solutions
$solutions = (
    "./"
    # "modules/account",
    # "modules/audit-logging",
    # "modules/background-jobs",
    # "modules/basic-theme",
    # "modules/blogging",
    # "modules/client-simulation",
    # "modules/docs",
    # "modules/feature-management",
    # "modules/identity",
    # "modules/identityserver",
    # "modules/openiddict",
    # "modules/permission-management",
    # "modules/setting-management",
    # "modules/tenant-management",
    # "modules/users",
    # "modules/virtual-file-explorer",
    # "modules/blob-storing-database",
    # "modules/cms-kit"
)

# List of projects
$projects = (

    # framework
    "src/Cike.AspNetCore.MinimalAPIs",
    "src/Cike.Auth",
    "src/Cike.Core",
    "src/Cike.Cqrs",
    "src/Cike.Data",
    "src/Cike.Data.EFCore",
    "src/Cike.Data.EFCore.MySql",
    "src/Cike.Data.EFCore.SqlServer",
    "src/Cike.Domain",
    "src/Cike.EventBus",
    "src/Cike.EventBus.Local",
    "src/Cike.Uow"
)
