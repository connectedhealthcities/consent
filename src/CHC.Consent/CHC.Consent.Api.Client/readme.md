# Auto-generated API Client

> see https://aka.ms/autorest

``` PowerShell

npm install -g autorest
autorest --input-file=???

```

if you're running the development server

``` PowerShell

autorest --input-file=http://localhost:5000/swagger/v1/swagger.json

```

``` yaml
output-folder: .\generated
message-format: json
clear-output-folder: true
license-header: >-
    (C) 2018 CHC 
    License: TBC
directive:
    - where: $.definitions
      transform: >
        for(x in $){
            const newName = x.replace(/[\.-](\w)/g, o => o[1].toUpperCase());
            if(newName === x) continue; 
            $[x]['x-ms-discriminator-value'] = x;
            
            $[newName] = $[x];
            delete $[x];
        }
    - where: $.paths[*][*].consumes
      transform: $.push('application/xml')
output-artifact:
 - swagger-document.json        
csharp:
    namespace: CHC.Consent.Api.Client
    add-credentials: true
            
```