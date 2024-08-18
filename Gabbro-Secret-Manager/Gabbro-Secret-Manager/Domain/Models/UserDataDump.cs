using Gabbro_Secret_Manager.Core;

namespace Gabbro_Secret_Manager.Domain.Models
{
    public class UserDataDump
    {
        public List<DumpSecret> Secrets { get; set; }

        public static UserDataDump Create(List<DumpSecret> dumpSecrets)
        {
            return new UserDataDump
            {
                Secrets = dumpSecrets
            };
        }

        public static UserDataDump Create(IEnumerable<DecryptedSecret> secrets)
        {
            return new UserDataDump
            {
                Secrets = secrets.Select(DumpSecret.Create).ToList()
            };
        }
    }

    public class DumpSecret
    {
        public required string Key { get; set; }
        public required string Value { get; set; }
        public required string Comments { get; set; }
        public required HashSet<string> Tags { get; set; }

        public static DumpSecret Create(DecryptedSecret secret)
        {
            return new DumpSecret
            {
                Key = secret.Name,
                Value = secret.Value,
                Comments = secret.Comments,
                Tags = secret.Tags
            };
        }
    }
}
