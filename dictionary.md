Person
--
A collection of identities

Identity
--
A fact about a person.

Rules Identities are determined by Identity Kind.

Identity Kind
--
Meta data about an Identity. 

Including: 
* an external identifier; 
* Whether an Identity is:
  * **atomic**/**simple** or **composite**
  * **mutable** or **immutable**
  * Whether a Person may have a **single** or **multiple** Identities for this Identity Kind

Subject Identifier
--
A shareable identifier that identifies Consent from a Person to share data with a Study.

We expect a Subject Identifier to be identified by a subset of a Person's Identity.

We expect different Studies to have different rules about generating and formatting Subject Identifiers.

Match
--
Rules determining how to find a Person by Identity values.

Initially, we are focusing on finding a person by **atomic** Identities.