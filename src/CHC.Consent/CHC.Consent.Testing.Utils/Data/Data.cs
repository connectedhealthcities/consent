using System.IO;
using System.Reflection;
using System.Text;

namespace CHC.Consent.Testing.Utils.Data
{
    public class Data
    {
        public static readonly string PersonSpecificationJson =
            new StreamReader(
                    Assembly.GetExecutingAssembly().GetManifestResourceStream(
                        typeof(Data),
                        "PersonSpecification.json"),
                    Encoding.UTF8)
                .ReadToEnd();
    }
}