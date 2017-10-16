# Consent
The story of building a system to manage consent

---

# Where to start?

Born In Bradford For All (BiB4All)
* Cohort study with the potential to run for "100 years" 
* Has Ethics Committee approval for 3 years
* Consent for Mothers (on a pregnancy-by-pregnancy basis) and Children
* Ethics Committee approval to record/manage consent electronically
* **Most importantly** - Happy for their study to be Guinea Pig for CHC Consent system

---

# What is Consent? - Questions

* What does it mean to provide consent?
* What does that consent cover?
* What happens if consent is revoked?

---

# What is Consent? - Answers

NOT OUR PROBLEM
====
    
---

# What is Consent? - The idiot's view

* A date when Consent was provided
* Evidence collected at the time Consent was provided
* The same for when Consent is withdrawn
    
---

# Who Can Provide (and manage) Consent?

* Mothers
* Children
* Grand Parents
* Legal Guardians
* Social workers
* The subject themselves

    
**This list changes throughout the subject's lifetime**

Management of changes is outside scope of system

---

# Identity

* We need to **identify** people to tell if they have provided consent
* (BiB4All) The method of identifying a child's consent is **different** to identifying a mother's consent

# External Data (BiB4All) 

* Merges data from external sources (Local Council, Education, etc)
* This data does not contain the same identifiers 
* **Need to find people by arbitrary identities**

# Identity - Subject Identifiers

* Storing identity separately from the data reduces risk of disclosure
    * Provide identity to the system
    * System provides **Subject Identifier**
    * **Subject Identifier** stored with data and can be used to query for consent

---

# Identity - Questions

* How do **YOU** identify subjects?
* Does identity change over time?
* Can a subject/patient/person have more than one value for the same kind of identity?
* How do you handle historical values? (Address changes for example?)
* How do you match identities with external systems?

---

# Security

<!-- Authenticate via external identity provider -->

* Simple when you're managing one study
* Ridiculously Complex when there's more than one study

---

# Security - Questions

* How do we reduce the risk of disclosure of information?
* How do we reduce the risk of duplicating data?
* How do we ensure that data is kept up-to-date but not unexpectedly changed?

---

# CHC Consent - Scope

* Store and retrieval of consent for study subject
* Provide a way for subjects to see how their data is being used
* Provide a way for that consent to be managed 

**What are we missing?**

---

