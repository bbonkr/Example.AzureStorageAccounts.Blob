namespace Example.App.Configurations;

public class AzureStorageAccountConfiguration
{
    public const string Name = "AzureStorageAccount";

    public string AccountKey { get; set; } = string.Empty;

    public string AccountName { get; set; } = string.Empty;

    public string ConnectionString { get; set; } = string.Empty;
}
