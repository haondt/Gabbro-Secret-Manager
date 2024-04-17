namespace Gabbro_Secret_Manager.Domain.Models
{
    public class ViewSecret
    {
        public static ViewSecret FromSecret(DecryptedSecret secret)
        {
            return new ViewSecret
            {
                Comments = secret.Comments,
                Id = secret.Id,
                Name = secret.Name,
                Value = secret.Value,
                Tags = secret.Tags,
            };
        }
        public required Guid Id { get; set; }
        public required  string Name { get; set; }
        public required string Value { get; set; }
        public string Comments { get; set; } = "";
        public required HashSet<string> Tags { get; set; } = [];
    }
}
