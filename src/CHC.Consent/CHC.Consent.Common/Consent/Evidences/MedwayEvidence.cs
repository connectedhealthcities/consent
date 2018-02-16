namespace CHC.Consent.Common.Consent.Evidences
{
    [Evidence("medway.evidence.bib4all.bradfordhospitals.nhs.uk")]
    public class MedwayEvidence : Evidence
    {
        public string CompetentStatus { get; set; }
        public string ConsentGivenBy { get; set; }
        public string ConsentTakenBy { get; set; }

        protected bool Equals(MedwayEvidence other)
        {
            return string.Equals(CompetentStatus, other.CompetentStatus) && string.Equals(ConsentGivenBy, other.ConsentGivenBy) && string.Equals(ConsentTakenBy, other.ConsentTakenBy);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((MedwayEvidence) obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (CompetentStatus != null ? CompetentStatus.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ConsentGivenBy != null ? ConsentGivenBy.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ConsentTakenBy != null ? ConsentTakenBy.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}