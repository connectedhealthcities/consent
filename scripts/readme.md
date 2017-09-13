These scripts should (*help?*) setup the development environment.

They all assume you're running an elevated  PowerShell session (i.e. with Administrator rights)

The idea is that there are two nodes:

	* `CHC-WEB` which will host the web and middleware
	* `CHC-DB` which will host the the SQL Server

You will need two **patched** `Windows 2012 R2` Servers ready to go. It's easiest if they're already called `CHC-WEB` and `CHC-DB` and can resolve each other by these DNS names.

The `bootstrap` directory contains scripts to help get these machines ready to go.

  1. `bootstrap\bootstrap.ps1` - will install _Windows Management Framework (WMF) 5.1_ - this process requires a restart
  2. `bootstrap\bootstrap-2.ps1` will install _PowerShellGet_ from [PowerShellGallery](https://www.powershellgallery.com/) and the required _Desired State Configuration (DSC)_ Modules. It will also configure _Local Configuration Manager (LCM)_ to reboot the machine if necessary.

SQL Specific
----

You will need `SQL Server 2008 R2` Installation media.

If you are downloading [Expres Edition](https://www.microsoft.com/en-us/download/details.aspx?id=30438) you will need to extract the SFX - by running a command like `SQLEXPR_x64_ENU.exe /x:D:\SQL`.

If you are using an ISO, please make sure it is mounted.

JDK
---

The DSC uses Chocolatey to install Oracle JDK. If you don't care for Oracle's licensing, then the ZULU distribution of OpenJDK should work fine too.


Keycloak (IdP)
---

This is put behind an IIS Reverse proxy (Rewrite and ARR) at /auth - the connection between IIS and Keycloak is https with a self-signed certificate.

**The setup will use the internal database, so watch out if you nuke the server as all the users will disappear.**
**WARNING - The self-signed certificate is installed in the Trusted CA Root on the CHC-WEB machine**

Here are some resources that have been useful setting this up.

  * [Https Setup (and how to create a different key!)](http://www.keycloak.org/docs/3.2/server_installation/topics/network/https.html)
  * [Putting it behind a load balancer/reverse proxy](https://www.keycloak.org/docs/3.2/server_installation/topics/clustering/load-balancer.html)
  * [how to make IIS ARR forward the Host Header](https://stackoverflow.com/a/7180527/26479) 

There's also _Redhat Single Signon_ if you'd prefer a vendor supported version.

Desired State Configuration (DSC)
---

in the dsc folder

  * Review `configuration.psd1`
  * run `.\chc.ps1 -ConfigData .\configuration.psd1` 
    * you will be prompted for the credentials to use to **install** sql server - please use the full name of the user (like `CHC-DB\Administrator`) - else it's likely to fail
  * run `Start-DSCConfiguration -Wait -Verbose -Force .\CHC`

 
Helpful scripts
---

New PFX file (this prints the thumbprint as the last line of the output)

        $mypwd = ConvertTo-SecureString -String 'potato' -AsPlainText -Force
        New-SelfSignedCertificate -DnsName 'chc-web', 'localhost' -CertStoreLocation Cert:\CurrentUser\My | %{ Export-PfxCertificate -Cert $_ -FilePath $env:TEMP\test.pfx -Password $mypwd; $_.Thumbprint }


