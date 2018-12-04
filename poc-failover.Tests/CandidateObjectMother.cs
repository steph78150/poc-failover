namespace poc_failover.Tests
{
    public static class CandidateObjectMother 
    {
        public static CandidateMessage JfKennedy()
        {
            return new CandidateMessage 
            {
                Term = 1963,
                Candidate = "jf_kennedy"
            };
        }

        public static CandidateMessage BillClinton() 
        {
            return new CandidateMessage 
            {
                Term = 1992,
                Candidate = "bill_clinton"
            };
        }

        public static CandidateMessage BarackObama() 
        {
            return new CandidateMessage 
            {
                Term = 2012,
                Candidate = "barack_obama"
            };
        }

        public static CandidateMessage MittRomney() 
        {
             return new CandidateMessage 
            {
                Term = 2012,
                Candidate = "mitt_romney"
            };
        }
    }
}
