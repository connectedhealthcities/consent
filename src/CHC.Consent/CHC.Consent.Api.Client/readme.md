# Auto-generated API Client

> see https://aka.ms/autorest

``` PowerShell

npm install -g autorest
autorest --input-file=???

```

``` yaml
output-folder: .\generated
message-format: json
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
            
```