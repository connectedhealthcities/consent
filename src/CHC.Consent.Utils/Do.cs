namespace CHC.Consent.Utils
{
    public static class Do
    {    
        public static void Nothing() {}
        
        public static void Nothing<T>(T _) {}

        public static void Nothing<TArg1, TArg2>(TArg1 arg1, TArg2 arg2) { }
    }
}