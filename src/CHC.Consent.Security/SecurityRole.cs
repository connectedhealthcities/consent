namespace CHC.Consent.Security
{
    public class SecurityRole
    {
        public string Name { get; }
        protected SecurityRole(string name)
        {
            Name = name;
        }
        public SecurityRole StudyAdministrator = new SecurityRole("study_administrator");
    }
}