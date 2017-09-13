[CmdletBinding()]
param(
  [Parameter()]$OutputPath = [IO.Path]::Combine($PSScriptRoot, 'CHC'),
  [Parameter()]$ConfigData,
  [parameter()][System.Management.Automation.PSCredential]$SqlInstallCredentials = (Get-Credential -Message 'Install Sql As...')
)

function Create-CredentialsFromPlainText {
    
    param(
        [Parameter(Mandatory=$true)][string]$username, 
        [Parameter(Mandatory=$true)][string]$plainTextPassword)

    process {
        $secpasswd = ConvertTo-SecureString "$plainTextPassword" -AsPlainText -Force
        return New-Object System.Management.Automation.PSCredential("$username", $secpasswd)
    }
}

<#
Set-WebConfigurationProperty -pspath 'MACHINE/WEBROOT/APPHOST'  -filter "system.webServer/proxy" -name "enabled" -value "True"
Set-WebConfigurationProperty -pspath 'MACHINE/WEBROOT/APPHOST'  -filter "system.webServer/proxy" -name "preserveHostHeader" -value "True"

#>

Configuration CHC {
    
    param(
        [Parameter(Mandatory=$true)]
        [System.Management.Automation.PSCredential]$sqlInstaller
    )
    
    Import-DscResource -ModuleName 'PSDesiredStateConfiguration', 'xPSDesiredStateConfiguration', 'xSqlServer', 'xWebAdministration', 'xWindowsUpdate', 'cChoco', 'xCertificate'

    Node $AllNodes.Where({$_.Roles -contains 'Web'}).NodeName {

        Script Install_Net_4.6.1
        {
            SetScript = {
                $SourceURI = "https://download.microsoft.com/download/E/4/1/E4173890-A24A-4936-9FC9-AF930FE3FA40/NDP461-KB3102436-x86-x64-AllOS-ENU.exe"
                $FileName = $SourceURI.Split('/')[-1]
                $BinPath = Join-Path $env:SystemRoot -ChildPath "Temp\$FileName"

                if (!(Test-Path $BinPath))
                {
                    Invoke-Webrequest -Uri $SourceURI -OutFile $BinPath
                }

                write-verbose "Installing .Net 4.6.1 from $BinPath"
                write-verbose "Executing $binpath /q /norestart"
                Sleep 5
                Start-Process -FilePath $BinPath -ArgumentList "/q /norestart" -Wait -NoNewWindow            
                Sleep 5
                Write-Verbose "Setting DSCMachineStatus to reboot server after DSC run is completed"
                $global:DSCMachineStatus = 1
            }

            TestScript = {
                [int]$NetBuildVersion = 394271

                if (Get-ItemProperty -Path 'HKLM:\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full' | %{$_ -match 'Release'})
                {
                    [int]$CurrentRelease = (Get-ItemProperty -Path 'HKLM:\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full').Release
                    if ($CurrentRelease -lt $NetBuildVersion)
                    {
                        Write-Verbose "Current .Net build version is less than 4.6.1 ($CurrentRelease)"
                        return $false
                    }
                    else
                    {
                        Write-Verbose "Current .Net build version is the same as or higher than 4.6.1 ($CurrentRelease)"
                        return $true
                    }
                }
                else
                {
                    Write-Verbose ".Net build version not recognised"
                    return $false
                }
            }

            GetScript = {
                if (Get-ItemProperty -Path 'HKLM:\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full' | %{$_ -match 'Release'})
                {
                    $NetBuildVersion =  (Get-ItemProperty -Path 'HKLM:\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full').Release
                    return $NetBuildVersion
                }
                else
                {
                    Write-Verbose ".Net build version not recognised"
                    return ".Net 4.6.1 not found"
                }
            }
        }

        WindowsFeature IIS {
            Name = 'Web-WebServer'
            Ensure = 'Present'
        }

        xWebAppPool ChcWebAppAppPool {
            Name = 'ChcWeb'
            Ensure = 'Present'
            DependsOn = '[WindowsFeature]IIS'
            enableConfigurationOverride = $true
            managedRuntimeVersion = 'v4.0'
            autoStart = $true
            identityType = 'ApplicationPoolIdentity'
        }

        $chcPfxLocalPath = 'Resources\iis\chc-web.pfx'
        $chcPfxPassword = 'potato'

        $chcPfxPath = 'C:\DSCFiles\iis\chc-web.pfx'
        $chcPfx = [System.Convert]::ToBase64String((Get-Content -Encoding Byte $chcPfxLocalPath))
        $chcPfxThumbprint = (New-Object System.Security.Cryptography.X509Certificates.X509Certificate2($chcPfxLocalPath, $chcPfxPassword)).Thumbprint
        
        
        Script chcPfx {
            GetScript = {
                @{ Result = Get-Item -Path $using:chcPfxPath -ErrorAction Ignore }
            }
            TestScript = {
                (Get-Item -Path $using:chcPfxPath -ErrorAction Ignore) -ne $null
            }
            SetScript = {
                New-Item (Split-Path $using:chcPfxPath) -ItemType 'Directory'
                [System.IO.File]::WriteAllBytes($using:chcPfxPath, [System.Convert]::FromBase64String($using:chcPfx))
            }
        }

        xPfxImport ChcCertificate {
            Thumbprint = $chcPfxThumbprint
            Path = $chcPfxPath
            Location = 'LocalMachine'
            Store = 'WebHosting'
            Credential = Create-CredentialsFromPlainText -username 'Ignored' -plainTextPassword $chcPfxPassword
            
            DependsOn = '[WindowsFeature]IIS','[Script]chcPfx'
        }

        xWebsite ChcWebsite {
            Name = "ChcWeb"
            Ensure = 'Present'
            BindingInfo = @( 
                MSFT_xWebBindingInformation  {
                    Protocol = 'http'
                    HostName = 'chc-web'
                }
                MSFT_xWebBindingInformation {
                    Protocol = 'https'
                    CertificateThumbprint = $chcPfxThumbprint
                    CertificateStoreName = 'WebHosting'
                    SslFlags = 0 
                }
            )
            ApplicationPool = 'ChcWeb'
            PhysicalPath = 'C:\CHC_WEB\App'
            DependsOn = "[xWebAppPool]ChcWebAppAppPool","[xWebsite]DisableDefaultWebsite", '[xPfxImport]ChcCertificate'
        }

        xWebsite DisableDefaultWebsite {
            Name = "Default Web Site"
            Ensure = 'Absent'
        }
    }

    Node $AllNodes.Where({$_.Roles -contains 'IdentityProvider'}).NodeName {
        cChocoInstaller InstallChoco {
            InstallDir = 'C:\choco\'
        }
        cChocoPackageInstaller InstallJDK {
            Name = "JDK8"
            Params = "'source=false installdir=c:\\java8'"
            Ensure = 'Present'
            DependsOn = '[cChocoInstaller]InstallChoco'
        }


        $keycloakZipPath = 'C:\DSCFiles\Keycloak\keycloak.zip'
        $keycloakInstallPath = 'C:\keycloak\'
        $keycloakZipUri = 'https://downloads.jboss.org/keycloak/3.2.1.Final/keycloak-3.2.1.Final.zip'

        $keycloakBasePath = Join-Path $keycloakInstallPath 'keycloak-3.2.1.Final'
        $keycloakConfigurationPath = Join-Path $keycloakBasePath 'standalone\configuration'
            
        Script DownloadKeycloak {
            
            GetScript = {
                @{ Result = (Get-Item -Path $using:keycloakZipPath -ErrorAction Ignore) -ne $null }
            }

            TestScript = {
                (Get-Item -Path $using:keycloakZipPath -ErrorAction Ignore) -ne $null
            }

            SetScript = {
                $wc = New-Object System.Net.WebClient
                $wc.DownloadFile($using:keycloakZipUri, $using:keycloakZipPath)
            }
        }

        Archive UnzipKeyCloak {
            DependsOn = '[Script]DownloadKeycloak'
            Ensure = 'Present'

            Path = $keycloakZipPath
            Destination = $keycloakInstallPath
        }

        #

        $bytes = Get-Content -Encoding Byte 'Resources\keycloak\keycloak.jks'
        $contents = [System.Convert]::ToBase64String($bytes)
        $destinationPath = (Join-Path $keycloakConfigurationPath 'keycloak.jks')

        Script KeycloakKeyStore {
            DependsOn = '[Archive]UnzipKeyCloak'

            GetScript = {
                $file = Get-Item $using:destinationPath -ErrorAction Ignore
                @{
                    Result = $file -ne $null
                }
            }

            TestScript = {
                Write-Verbose "Testing for $using:destinationPath"
                (Get-Item $using:destinationPath -ErrorAction Ignore) -ne $null
            }

            SetScript = {
                Write-Verbose "Writing $using:destinationPath"
                New-Item (Split-Path $using:destinationPath) -ItemType 'Directory'
                [System.IO.File]::WriteAllBytes($using:destinationPath, [System.Convert]::FromBase64String($using:contents))
            }         
        }

        $keycloakCertFile = 'C:\DSCFiles\Keycloak\keycloak.cer'

        File KeycloakCertification {
            DependsOn = '[Script]KeycloakKeyStore'
            Ensure = 'Present'
            DestinationPath = $keycloakCertFile
            Contents = Get-Content 'Resources\keycloak\keycloak.cer' -Raw 
        }

        xCertificateImport KeycloakCertificateImport {
            DependsOn = '[File]KeycloakCertification'
            Thumbprint = 'fdb6098d44388860fd7ab525aea1f5f21d3d64b0'
            Path = $keycloakCertFile
            Location = 'LocalMachine'
            Store = 'Root'
        }

        File KeycloakConfiguration {
            DependsOn = '[Archive]UnzipKeyCloak'
            Ensure = 'Present'
            DestinationPath = (Join-Path $keycloakConfigurationPath 'standalone.xml')
            Contents = Get-Content 'Resources\keycloak\standalone.xml' -Raw
        }

        $keycloakRealmPath = 'C:\DSCFiles\Keycloak\chc-consent-realm.json'
        
        File KeycloakRealm {
            DependsOn = '[Archive]UnzipKeyCloak'
            Ensure = 'Present'
            DestinationPath = (Join-Path $keycloakConfigurationPath 'chc-consent-realm.json')
            Contents = Get-Content 'Resources\keycloak\chc-consent-realm.json' -Raw
        }

        Script Keycloak {
            DependsOn = '[Archive]UnzipKeyCloak','[cChocoPackageInstaller]InstallJDK','[Script]KeycloakKeyStore','[File]KeycloakCertification','[File]KeycloakRealm'
            
            GetScript = {
                @{ Result = Get-WmiObject win32_process -Filter "name like '%java.exe'" | where CommandLine -like "*keycloak*" | select ProcessID }
            }

            TestScript = {
                @( Get-WmiObject win32_process -Filter "name like '%java.exe'" | where CommandLine -like "*keycloak*" | select ProcessID ).Length -ne 0
            }

            SetScript = {
                $keycloakBasePath = $using:keycloakBasePath
                Start-Process -FilePath (Join-Path $keycloakBasePath 'bin\standalone.bat') -WorkingDirectory $keycloakBasePath `
                    -ArgumentList  " ""-Dkeycloak.import=$keycloakRealmPath"" " `
                    -RedirectStandardError (Join-Path $keycloakBasePath 'std-err.log') `
                    -RedirectStandardOutput (Join-Path $keycloakBasePath 'std-out.log')
            }
        }


        Package IisUrlRewrite {
            Ensure = 'Present'
            Path = 'http://download.microsoft.com/download/D/D/E/DDE57C26-C62C-4C59-A1BB-31D58B36ADA2/rewrite_amd64_en-US.msi'
            Name = 'IIS URL Rewrite Module 2'
            ProductId = '38D32370-3A31-40E9-91D0-D236F47E3C4A'
        }

        Package IisArr {
            DependsOn = '[Package]IisUrlRewrite'
            Ensure = 'Present'
            Path = 'http://download.microsoft.com/download/E/9/8/E9849D6A-020E-47E4-9FD0-A023E99B54EB/requestRouter_amd64.msi'
            Name = 'Microsoft Application Request Routing 3.0'
            ProductId = '279B4CB0-A213-4F94-B224-19D6F5C59942'
        }

        Script IisEnableProxy {
            DependsOn = "[Package]IisArr"
            GetScript = {
                @{
                    Result = (Get-WebConfigurationProperty -pspath 'MACHINE/WEBROOT/APPHOST'  -filter "system.webServer/proxy" -name "enabled").Value
                }
            }
            TestScript = {
                (Get-WebConfigurationProperty -pspath 'MACHINE/WEBROOT/APPHOST'  -filter "system.webServer/proxy" -name "enabled") -eq $true
            }

            SetScript = {
                Set-WebConfigurationProperty -pspath 'MACHINE/WEBROOT/APPHOST'  -filter "system.webServer/proxy" -name "enabled" -value "True"
            }
        }

        #Set-WebConfigurationProperty -pspath 'MACHINE/WEBROOT/APPHOST'  -filter "system.webServer/proxy" -name "preserveHostHeader" -value "True"
        Script IisEnableProxyPreserveHostHeader {
            DependsOn = "[Script]IisEnableProxy"
            GetScript = {
                @{
                    Result = (Get-WebConfigurationProperty -pspath 'MACHINE/WEBROOT/APPHOST'  -filter "system.webServer/proxy" -name "preserveHostHeader").Value
                }
            }
            TestScript = {
                (Get-WebConfigurationProperty -pspath 'MACHINE/WEBROOT/APPHOST'  -filter "system.webServer/proxy" -name "preserveHostHeader") -eq $true
            }

            SetScript = {
                Set-WebConfigurationProperty -pspath 'MACHINE/WEBROOT/APPHOST'  -filter "system.webServer/proxy" -name "preserveHostHeader" -value "True"
            }
        }


    }

    Node $AllNodes.Where({$_.Roles -contains 'SqlServer'}).NodeName {
        
        Write-Host "Configuration $Node"
        
        [PSCredential] $sqlSa = Create-CredentialsFromPlainText -username "sa" -plainTextPassword $Node.SqlConfig.SAPwd

        Write-Host "sa password is $Node.SqlConfig.SAPwd"
        
        WindowsFeature DotNet35 {
            Ensure = 'Present'
            Name = 'Net-Framework-Core'
        }

        xSQLServerSetup SqlSetup {
            InstanceName = $Node.SqlConfig.InstanceName
            Features = $Node.SqlConfig.Features
        #
            PsDscRunAsCredential = $sqlInstaller
            SourcePath = $Node.SqlConfig.SourcePath
        #
            SecurityMode = 'SQL'
            SAPwd = $sqlSa

            #SQLSysAdminAccounts = "$env:USERDOMAIN\Administrators"

            DependsOn = "[WindowsFeature]DotNet35"
        }

        xSQLServerNetwork SqlNetwork {
            InstanceName = $Node.SqlConfig.InstanceName
            ProtocolName = 'tcp'
            TcpDynamicPorts = ''
            TcpPort = '1433'
            RestartService = $true
            DependsOn = "[xSQLServerSetup]SqlSetup"
        }
    }

    <# This runs windows update....
    Node $AllNodes.NodeName {
        xWindowsUpdateAgent ApplyWindowsFixes {
            UpdateNow = $true
            IsSingleInstance = 'Yes'
            Source = "MicrosoftUpdate"
            Category = "Security"
            Notifications    = 'ScheduledInstallation'
        }
    }
    #>
    
}

CHC -OutputPath $OutputPath -ConfigurationData $ConfigData -sqlInstaller $SqlInstallCredentials