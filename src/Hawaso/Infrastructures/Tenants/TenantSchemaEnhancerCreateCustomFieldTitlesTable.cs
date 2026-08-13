using Microsoft.Data.SqlClient;
using System.Collections.Generic;

namespace Hawaso.Infrastructures.Tenants
{
    public class TenantSchemaEnhancerCreateCustomFieldTitlesTable
    {
        private readonly string _masterConnectionString;

        public TenantSchemaEnhancerCreateCustomFieldTitlesTable(string masterConnectionString)
        {
            _masterConnectionString = masterConnectionString;
        }

        public void EnhanceAllTenantDatabases()
        {
            List<string> tenantConnectionStrings = GetTenantConnectionStrings();

            foreach (string connectionString in tenantConnectionStrings)
            {
                CreateTableIfNotExists(connectionString);
                InsertDefaultValuesIfEmpty(connectionString);
            }
        }

        private List<string> GetTenantConnectionStrings()
        {
            List<string> result = new();

            using var connection = new SqlConnection(_masterConnectionString);
            connection.Open();

            const string sql = """
                SELECT ConnectionString
                FROM dbo.Tenants
                WHERE ConnectionString IS NOT NULL
                """;

            using var command = new SqlCommand(sql, connection);
            using var reader = command.ExecuteReader();

            int connectionStringOrdinal = reader.GetOrdinal("ConnectionString");

            while (reader.Read())
            {
                if (reader.IsDBNull(connectionStringOrdinal))
                {
                    continue;
                }

                string connectionString = reader.GetString(connectionStringOrdinal);

                if (!string.IsNullOrWhiteSpace(connectionString))
                {
                    result.Add(connectionString);
                }
            }

            return result;
        }

        private void CreateTableIfNotExists(string connectionString)
        {
            using var connection = new SqlConnection(connectionString);
            connection.Open();

            const string checkSql = """
                SELECT COUNT(*)
                FROM INFORMATION_SCHEMA.TABLES
                WHERE TABLE_SCHEMA = 'dbo'
                    AND TABLE_NAME = 'CustomFieldTitles'
                """;

            using var checkCommand = new SqlCommand(checkSql, connection);

            int count = Convert.ToInt32(checkCommand.ExecuteScalar());

            if (count > 0)
            {
                return;
            }

            const string createSql = """
                CREATE TABLE [dbo].[CustomFieldTitles]
                (
                    [ID] bigint IDENTITY(1,1) NOT NULL PRIMARY KEY,
                    [Type] nvarchar(50) NOT NULL,
                    [Field] nvarchar(max) NOT NULL,
                    [Title] nvarchar(max) NULL,
                    [Visible] bit NOT NULL DEFAULT(0),
                    [Searchable] bit NOT NULL DEFAULT(0)
                )
                ON [PRIMARY]
                TEXTIMAGE_ON [PRIMARY]
                """;

            using var createCommand = new SqlCommand(createSql, connection);
            createCommand.ExecuteNonQuery();
        }

        private void InsertDefaultValuesIfEmpty(string connectionString)
        {
            using var connection = new SqlConnection(connectionString);
            connection.Open();

            const string countSql = """
                SELECT COUNT(*)
                FROM dbo.CustomFieldTitles
                """;

            using var countCommand = new SqlCommand(countSql, connection);

            int rowCount = Convert.ToInt32(countCommand.ExecuteScalar());

            if (rowCount > 0)
            {
                return;
            }

            const string insertSql = """
                INSERT INTO dbo.CustomFieldTitles
                (
                    [Type],
                    [Field],
                    [Title],
                    [Visible],
                    [Searchable]
                )
                VALUES
                    ('EmployeeProfile', 'Custom1', NULL, 0, 0),
                    ('EmployeeProfile', 'Custom2', NULL, 0, 0),
                    ('EmployeeProfile', 'Custom3', NULL, 0, 0),
                    ('EmployeeProfile', 'Custom4', NULL, 0, 0),
                    ('EmployeeProfile', 'Custom5', NULL, 0, 0),
                    ('EmployeeProfile', 'Custom6', NULL, 0, 0),
                    ('StateLicense', 'Custom1', NULL, 0, 0),
                    ('StateLicense', 'Custom2', NULL, 0, 0),
                    ('StateLicense', 'Custom3', NULL, 0, 0),
                    ('StateLicense', 'Custom4', NULL, 0, 0),
                    ('StateLicense', 'Custom5', NULL, 0, 0),
                    ('StateLicense', 'Custom6', NULL, 0, 0)
                """;

            using var insertCommand = new SqlCommand(insertSql, connection);
            insertCommand.ExecuteNonQuery();
        }
    }
}