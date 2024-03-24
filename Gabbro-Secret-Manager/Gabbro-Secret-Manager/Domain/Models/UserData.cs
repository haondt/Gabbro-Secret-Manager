namespace Gabbro_Secret_Manager.Domain.Models
{
    public class UserData
    {
        public required string UserKey { get; set; }
        public required EncryptionKeySettings EncryptionKeySettings { get; set; }
    }
}
