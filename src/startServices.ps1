try
{
	Write-Host "=====Start Fitmeplan.IdentityServer======"
	$serviceDirectory = [string]::Format("{0}\Web\{1}", ${env:SolutionPath}, "Fitmeplan.IdentityServer")
	Start-Process -FilePath "dotnet" -ArgumentList "exec", "$serviceDirectory\bin\Debug\netcoreapp3.1\Fitmeplan.IdentityServer.dll" -WorkingDirectory "$serviceDirectory" -PassThru
	
	Write-Host "=====Start Fitmeplan.Api======"
	$serviceDirectory = [string]::Format("{0}\Web\{1}", ${env:SolutionPath}, "Fitmeplan.Api")
	Start-Process -FilePath "dotnet " -ArgumentList "exec", "$serviceDirectory\bin\Debug\netcoreapp3.1\Fitmeplan.Api.dll" -WorkingDirectory "$serviceDirectory" -PassThru
	
	# $serviceDirectory = [string]::Format("{0}\{1}", ${env:SolutionPath}, "..\..\Fitmeplan.Client\src\Fitmeplan.Client\")
	# Write-Host $serviceDirectory
	# if(Test-Path $serviceDirectory) {
		# Start-Process -FilePath "dotnet" -ArgumentList "exec", "$serviceDirectory\bin\Debug\netcoreapp3.1\Fitmeplan.Client.App.dll" -WorkingDirectory "$serviceDirectory" -PassThru
	# }

	foreach ($item in Get-ChildItem -Directory -Recurse -Filter "*.Service" | ?{ $_.PSIsContainer })
	{
		$message = [string]::Format("Starting {0}...", $item.Name)
		Write-Host "================================================================================"
		Write-Host $message
		Write-Host "================================================================================"

		$serviceDirectory = [string]::Format("{0}\Services\{1}", ${env:SolutionPath}, $item.Name)
		Start-Process -FilePath "dotnet" -ArgumentList "exec", "$serviceDirectory\bin\Debug\netcoreapp3.1\$($item.Name).dll" -WorkingDirectory "$serviceDirectory" -PassThru -WindowStyle hidden
		Write-Host "DONE!"
	}
}
catch [System.Exception]
{
    $errType = $_.Exception.GetType().FullName
    $message = $_.Exception.Message
    Write-Host "ERROR: '$errType' '$message'" -ForegroundColor Red
    exit 1
}