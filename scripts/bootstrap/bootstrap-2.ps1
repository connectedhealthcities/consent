#Install PowerShellGet and NuGet (requirement)
Install-PackageProvider -Name NuGet -MinimumVersion 2.8.5.201 -Force
Install-Module -Name PowerShellGet -Force

#Stop prompts for PSGallery modules
Set-PackageSource -Name PSGallery -Trusted


#Required PS modules for DSC
Install-Module -Name xPSDesiredStateConfiguration, xWebAdministration, xSQLServer, xWindowsUpdate, xCertificate

#PSRemoting - allow access from the required machines!
Enable-PSRemoting
#Change this if the machine names are different
Set-Item wsman:\localhost\Client\TrustedHosts "CHC-WEB,CHC-DB" -Force

#Change the Local Configuration Manager settings
.\lcm.ps1
 Set-DscLocalConfigurationManager -Path .\LCMConfig