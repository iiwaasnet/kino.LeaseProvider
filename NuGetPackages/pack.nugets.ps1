$MsBuild = "C:\Program Files (x86)\MSBuild\14.0\Bin\msbuild.exe"

$SolutionFile = Get-ChildItem ..\src -Recurse | Where-Object {$_.Extension -eq ".sln"}
nuget restore $SolutionFile.FullName

foreach ($NugetSpec in Get-ChildItem ..\src -Recurse | Where-Object {$_.Extension -eq ".nuspec"})
{
	$ProjectFile = Get-ChildItem $NugetSpec.DirectoryName | Where-Object {$_.Extension -eq ".csproj"}

	nuget pack $ProjectFile.FullName -BasePath $NugetSpec.DirectoryName -Build -Prop Configuration=Release -Prop FilePath=$MsBuild -NonInteractive -IncludeReferencedProjects
}