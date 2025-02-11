using Microsoft.Data.SqlClient;

namespace Hawaso.Web.Components.Pages.VendorPages.Models
{
    public class VendorPermanentDeleteSchemaEnhancer
    {
        private readonly string _connectionString;

        public VendorPermanentDeleteSchemaEnhancer(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void EnsureVendorsTableExists()
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // Vendors 테이블 존재 여부 확인
                SqlCommand cmdCheck = new SqlCommand(@"
                        SELECT COUNT(*) 
                        FROM INFORMATION_SCHEMA.TABLES 
                        WHERE TABLE_SCHEMA = 'dbo' 
                        AND TABLE_NAME = 'Vendors'", connection);

                int tableCount = (int)cmdCheck.ExecuteScalar();

                // Vendors 테이블이 없으면 생성
                if (tableCount == 0)
                {
                    SqlCommand cmdCreateTable = new SqlCommand(@"
                            CREATE TABLE [dbo].[Vendors](
                                [ID] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                                [Active] BIT NOT NULL DEFAULT 1,
                                [Name] NVARCHAR(MAX) NOT NULL,
                                [Alias] NVARCHAR(MAX) NULL,
                                [LicenseNumber] NVARCHAR(35) NULL,
                                [LicenseDate] DATETIME2(7) NULL,
                                [LicenseRenewalDate] DATETIME2(7) NULL,
                                [LicenseExpirationDate] DATETIME2(7) NULL
                            )", connection);

                    cmdCreateTable.ExecuteNonQuery();
                }

                connection.Close();
            }
        }
    }
}
