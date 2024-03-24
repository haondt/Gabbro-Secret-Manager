namespace Gabbro_Secret_Manager.Domain.Models
{
    public struct EncryptionKeySettings
    {
        public required int Iterations { get; set; }
        public required string Salt { get; set; }
    }
}