using System.Text;
using JetBrains.Annotations;

namespace Reusable.IOnymous
{
    [PublicAPI]
    public interface IEmailSubject
    {
        string Value { get; }

        Encoding Encoding { get; }
    }

    public class EmailSubject : IEmailSubject
    {
        public string Value { get; set; }

        public Encoding Encoding { get; set; } = Encoding.UTF8;
    }
}