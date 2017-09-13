#Set-ExecutionPolicy Unrestricted -Force
#. { iwr -useb http://boxstarter.org/bootstrapper.ps1 } | iex; get-boxstarter -Force

#refreshenv
#$env:Path = [System.Environment]::GetEnvironmentVariable("Path","Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path","User")
#$env:PSModulePath =  [System.Environment]::GetEnvironmentVariable("PSModulePath","User") + ";" + [System.Environment]::GetEnvironmentVariable("PSModulePath","Machine")

#Import-Module Boxstarter.Chocolatey

function Get-ScriptDirectory
{
  
  $Invocation = (Get-Variable MyInvocation -Scope 1).Value
  Split-Path $Invocation.MyCommand.Path
}


function TryToAutomaticallyTriggerStage2($d)
{
    $atLoginTrigger = New-JobTrigger -AtLogOn -User $env:USERNAME
    Register-ScheduledJob -Trigger $atLoginTrigger -Name "Bootstrap-Stage-2" -FilePath "$d\Bootstrap-2.ps1"
}

function DownloadAndInstallWmt5_1
{
    iwr http://download.microsoft.com/download/6/F/5/6F5FF66C-6775-42B0-86C4-47D41F2DA187/Win8.1AndW2K12R2-KB3191564-x64.msu -OutFile KB3191564.msu
    Start-Process -FilePath wusa -ArgumentList "$PWD/KB3191564.msu /quiet /forcerestart" -Wait
}

$scriptDirectory = Get-ScriptDirectory
TryToAutomaticallyTriggerStage2 $scriptDirectory
DownloadAndInstallWmt5_1

