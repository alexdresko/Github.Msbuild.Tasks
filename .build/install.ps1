param($installPath, $toolsPath, $package, $project)

Import-Module (Join-Path $toolsPath "MSBuild.psm1")


function Copy-MSBuildTasks($project) {
	$solutionDir = Get-SolutionDir
	$tasksToolsPath = (Join-Path $solutionDir ".build")

	if(!(Test-Path $tasksToolsPath)) {
		mkdir $tasksToolsPath | Out-Null
	}

	Write-Host "Copying Github.Msbuild.Tasks files to $tasksToolsPath"
	Copy-Item "$toolsPath\*" $tasksToolsPath -Force | Out-Null

	Write-Host "Don't forget to commit the .build folder"
	return "$tasksToolsPath"
}

function Add-Solution-Folder($buildPath) {
	# Get the open solution.
	$solution = Get-Interface $dte.Solution ([EnvDTE80.Solution2])

	# Create the solution folder.
	$buildFolder = $solution.Projects | Where {$_.ProjectName -eq ".build"}
	if (!$buildFolder) {
		$buildFolder = $solution.AddSolutionFolder(".build")
	}


	# Add files to solution folder
	$projectItems = Get-Interface $buildFolder.ProjectItems ([EnvDTE.ProjectItems])

	$files = Get-ChildItem $buildPath

	for ($i=0; $i -lt $files.Count; $i++) {
		$outfile = $files[$i].FullName
		$projectItems.AddFromFile($outfile)
	}
}


function Main 
{
	$taskPath = Copy-MSBuildTasks $project
	Add-Solution-Folder $taskPath
}

Main
