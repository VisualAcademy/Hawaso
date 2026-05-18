namespace Hawaso.Infrastructures.Tenants;

using System.Data;
using Microsoft.Data.SqlClient;

public class TenantSchemaEnhancerAddColumnApplications(string masterConnectionString)
{
    public void EnhanceAllTenantDatabases()
    {
        List<string> tenantConnectionStrings = GetTenantConnectionStrings();

        foreach (string connectionString in tenantConnectionStrings)
        {
            EnsureNewColumns(connectionString);
        }
    }

    private List<string> GetTenantConnectionStrings()
    {
        List<string> result = new();

        using SqlConnection connection = new(masterConnectionString);
        connection.Open();

        using SqlCommand command = new(
            "SELECT ConnectionString FROM dbo.Tenants WHERE ConnectionString IS NOT NULL",
            connection);

        using SqlDataReader reader = command.ExecuteReader();

        while (reader.Read())
        {
            object value = reader["ConnectionString"];

            if (value == DBNull.Value)
            {
                continue;
            }

            string? connectionString = value.ToString();

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                continue;
            }

            result.Add(connectionString);
        }

        return result;
    }

    private static void EnsureNewColumns(string connectionString)
    {
        using SqlConnection connection = new(connectionString);
        connection.Open();

        List<(string Name, string Type)> columnsToAdd =
        [
            ("PortalName", "nvarchar(max) DEFAULT('Hawaso') NULL"),
            ("Language", "nvarchar(255) DEFAULT('en-US') NULL"),
        ];

        foreach ((string name, string type) in columnsToAdd)
        {
            using SqlCommand checkCommand = new($@"
SELECT COUNT(*)
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = 'dbo'
  AND TABLE_NAME = 'Applications'
  AND COLUMN_NAME = @ColumnName",
                connection);

            checkCommand.Parameters.AddWithValue("@ColumnName", name);

            int columnCount = Convert.ToInt32(checkCommand.ExecuteScalar());

            if (columnCount > 0)
            {
                continue;
            }

            using SqlCommand addColumnCommand = new($@"
ALTER TABLE [dbo].[Applications]
ADD [{name}] {type}",
                connection);

            addColumnCommand.ExecuteNonQuery();
        }
    }
}