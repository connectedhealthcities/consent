A `PermissionEntry` defines a `Security Principal`'s `Permission`s for a `Securables`. The absence of a `PermissionEntry` for a `Securable` means that a `Securable Principal` is no able the access that `Securable`.

`Studies` are the source of `Permissions` for new `Consent` and `Person` objects

Currently `Permission` on a `Person` implies this `Permission` on all their `Identity` objects.

**THE SYSTEM DOES NOT PROVIDE GRANUALAR PERMISSIONS ON `Identity`**

As we rely on an external authentication system:

  * New installs require a **system** administrator to authenticate to the system at install time
  * All other permissions are provided via invitation tokens - a token must be presented by an authenticated users before it is enacted
  * **UNIMPLEMENTED** tokens make need different formats for different transports (e.g. link in email, code in letter )
  * Tokens may have different expiry dates - local tokens may expire in minutes or hours, whereas tokens sent via the post may need to be valid for weeks, months, or even years.
  
**NOTE** In the case where a group of people may share `Permission`s, and that group of people has members who may change, it is very much advised to use `Role`s rather than manage these `Permission`s on an individual level. 
  
