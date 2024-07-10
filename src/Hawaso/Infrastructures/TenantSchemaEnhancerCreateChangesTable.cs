namespace Portals.Infrastructures;

public class TenantSchemaEnhancerCreateChangesTable
{
    private readonly string _connectionString;

    public TenantSchemaEnhancerCreateChangesTable(string connectionString)
    {
        _connectionString = connectionString;
    }

    public void CreateChangesTable()
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            SqlCommand cmdCheck = new SqlCommand(@"
                    SELECT COUNT(*) 
                    FROM INFORMATION_SCHEMA.TABLES 
                    WHERE TABLE_SCHEMA = 'dbo' 
                    AND TABLE_NAME = 'Changes'", connection);

            int tableCount = (int)cmdCheck.ExecuteScalar();

            if (tableCount == 0)
            {
                SqlCommand cmdCreateTable = new SqlCommand(@"
                        CREATE TABLE [dbo].[Changes](
                            [Id] INT IDENTITY(1,1) PRIMARY KEY,
                            [Email] NVARCHAR(255) NULL,
                            [UserName] NVARCHAR(255) NULL,
                            [PhoneNumber] NVARCHAR(50) NULL,
                            [Address] NVARCHAR(255) NULL,
                            [MobilePhone] NVARCHAR(50) NULL,
                            [FirstName] NVARCHAR(255) NULL,
                            [LastName] NVARCHAR(255) NULL,
                            [Age] INT NULL,
                            [IsComplete] BIT NULL,
                            [IsActive] BIT NULL,
                            [CreatedAt] DATETIME DEFAULT GETDATE() NULL
                        )", connection);

                cmdCreateTable.ExecuteNonQuery();
            }

            connection.Close();
        }
    }
}
