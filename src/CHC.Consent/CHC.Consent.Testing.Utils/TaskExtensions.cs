using System.Threading.Tasks;

namespace CHC.Consent.Testing.Utils
{
    public static class TaskExtensions
    {
        public static void RunSync(this Task task) => task.GetAwaiter().GetResult();
        public static T RunSync<T>(this Task<T> task) => task.GetAwaiter().GetResult();
    }
}