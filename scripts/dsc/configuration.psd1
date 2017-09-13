@{
    AllNodes = @(
        @{
            NodeName = "*"
            PsDscAllowPlainTextPassword = $true
        },
        @{
            NodeName = "CHC-WEB"
            Roles = "Web","IdentityProvider","Middleware"
        }
        @{
            #Name of DB Server
            NodeName = "CHC-DB"
            Roles = "SqlServer"
            SqlConfig = @{
                #Probably want to change these for your environment - 
                #The DSC is fussy about which letters it passed through to the setup
                SAPwd = '7<6524_2HHD-Sls'
                #where is the setup.exe located?
                SourcePath   = 'G:\SQL\' 
                
                
                #default instance
                InstanceName = 'MSSQLServer' 
                #core engine - minimal
                Features     = 'SQLEngine'

            }
        }
    );
}