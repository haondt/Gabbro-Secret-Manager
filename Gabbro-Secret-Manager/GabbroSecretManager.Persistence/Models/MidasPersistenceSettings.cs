namespace GabbroSecretManager.Persistence.Models
{
    public record GabbroSecretManagerPersistenceSettings
    {
        public required string DatabasePath { get; init; }
        public bool UseConnectionPooling { get; init; } = true;
        public string AccountsTableName { get; init; } = "accounts";
    }
}
