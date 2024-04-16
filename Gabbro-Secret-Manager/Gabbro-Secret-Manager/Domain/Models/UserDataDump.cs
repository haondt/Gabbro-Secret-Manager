using Gabbro_Secret_Manager.Core;

namespace Gabbro_Secret_Manager.Domain.Models
{
    public class UserDataDump
    {
        public List<DumpSecret> Secrets { get; }

        public UserDataDump(List<DumpSecret> dumpSecrets)
        {
            Secrets = dumpSecrets;
        }

        public UserDataDump(IEnumerable<DecryptedSecret> secrets)
        {
            Secrets = secrets.Select(s => new DumpSecret(s)).ToList();
        }
    }

    public class DumpSecret
    {
        public string Key { get; }
        public string Value { get; }
        public string Comments { get; }
        public HashSet<string> Tags { get; }

        public DumpSecret(
            string key,
            string value,
            string comments,
            HashSet<string> tags)
        {
            Key = key;
            Value = value;
            Comments = comments;
            Tags = tags;
        }

        public DumpSecret(DecryptedSecret secret)
        {
            Key = secret.Name;
            Value = secret.Value;
            Comments = secret.Comments;
            Tags = secret.Tags;
        }
    }
}
