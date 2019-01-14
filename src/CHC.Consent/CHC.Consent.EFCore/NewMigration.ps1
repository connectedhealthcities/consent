param(
    [Parameter(Mandatory=$False)]
    $command = "migrations",
    [Parameter(Mandatory=$False)]
    $action = "add",
    [Parameter(Mandatory=$False,Position=0,ValueFromRemainingArguments=$True)]
    [Object[]] $args
);

#echo dotnet ef $command $action -c ConsentContext -s ..\CHC.Consent.Api $args

&'dotnet' ef $command $action -c ConsentContext -s ..\CHC.Consent.Api $args 
