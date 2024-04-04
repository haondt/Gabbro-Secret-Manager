using Gabbro_Secret_Manager.Core;

namespace Gabbro_Secret_Manager.Domain.Models
{
    public class UserDataDump
    {
        public required List<DumpSecret> Secrets { get; set; }
    }

    public class DumpSecret
    {
        public required string Key { get; set; }
        public required string Value { get; set; }
        public required HashSet<string> Tags { get; set; }
    }
}
