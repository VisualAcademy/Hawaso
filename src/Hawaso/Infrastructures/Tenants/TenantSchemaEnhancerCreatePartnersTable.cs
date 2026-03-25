using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;

namespace Hawaso.Infrastructures.Tenants;

public class TenantSchemaEnhancerCreatePartnersTable(string masterConnectionString)
{
    // 모든 테넌트 데이터베이스를 향상시키는 메서드
    public void EnhanceAllTenantDatabases()
    {
        List<(string ConnectionString, bool IsMultiPortalEnabled)> tenantDetails = GetTenantDetails();

        foreach (var tenant in tenantDetails)
        {
            // Partners 테이블이 없으면 생성합니다.
            CreatePartnersTableIfNotExists(tenant.ConnectionString);

            // IsMultiPortalEnabled가 true인 경우 기본 파트너를 추가합니다.
            if (tenant.IsMultiPortalEnabled)
            {
                AddDefaultPartnerIfNotExists(tenant.ConnectionString);
            }
        }
    }

    // 모든 테넌트의 연결 문자열과 IsMultiPortalEnabled 값을 가져오는 메서드
    private List<(string ConnectionString, bool IsMultiPortalEnabled)> GetTenantDetails()
    {
        List<(string ConnectionString, bool IsMultiPortalEnabled)> result = new();

        using (SqlConnection connection = new SqlConnection(masterConnectionString))
        {
            connection.Open();

            using SqlCommand cmd = new SqlCommand(
                "SELECT ConnectionString, IsMultiPortalEnabled FROM dbo.Tenants",
                connection);

            using SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                object connectionStringValue = reader["ConnectionString"];
                object isMultiPortalEnabledValue = reader["IsMultiPortalEnabled"];

                if (connectionStringValue == DBNull.Value)
                {
                    continue;
                }

                string connectionString = Convert.ToString(connectionStringValue) ?? string.Empty;

                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    continue;
                }

                bool isMultiPortalEnabled =
                    isMultiPortalEnabledValue != DBNull.Value &&
                    Convert.ToBoolean(isMultiPortalEnabledValue);

                result.Add((connectionString, isMultiPortalEnabled));
            }
        }

        return result;
    }

    // 특정 테넌트 데이터베이스에 Partners 테이블이 없으면 생성하는 메서드
    private void CreatePartnersTableIfNotExists(string connectionString)
    {
        // 문자열의 두 개의 백슬래시를 하나의 백슬래시로 변경
        connectionString = connectionString.Replace("\\\\", "\\");

        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            connection.Open();

            using SqlCommand cmdCheck = new SqlCommand(@"
                SELECT COUNT(*) 
                FROM INFORMATION_SCHEMA.TABLES 
                WHERE TABLE_SCHEMA = 'dbo' 
                  AND TABLE_NAME = 'Partners'", connection);

            int tableCount = Convert.ToInt32(cmdCheck.ExecuteScalar());

            if (tableCount == 0)
            {
                using SqlCommand cmdCreateTable = new SqlCommand(@"
                    CREATE TABLE [dbo].[Partners](
                        [ID] bigint IDENTITY(1,1) NOT NULL PRIMARY KEY CLUSTERED,
                        [BaseAPIAddress] nvarchar(max) NULL,
                        [Name] nvarchar(max) NULL,
                        [Password] nvarchar(max) NULL,
                        [PrimaryEmail] nvarchar(max) NULL
                    )", connection);

                cmdCreateTable.ExecuteNonQuery();
            }
        }
    }

    // 특정 테넌트 데이터베이스에 기본 파트너가 없으면 추가하는 메서드
    private void AddDefaultPartnerIfNotExists(string connectionString)
    {
        // 문자열의 두 개의 백슬래시를 하나의 백슬래시로 변경
        connectionString = connectionString.Replace("\\\\", "\\");

        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            connection.Open();

            using SqlCommand cmdCheck = new SqlCommand(@"
                SELECT COUNT(*) 
                FROM dbo.Partners 
                WHERE Name = 'VisualAcademy'", connection);

            int partnerCount = Convert.ToInt32(cmdCheck.ExecuteScalar());

            if (partnerCount == 0)
            {
                // 다음 코드는 교육용 암호를 사용한 것입니다. 실제 프로덕션 환경에서는 사용하지 마세요.
                using SqlCommand cmdInsert = new SqlCommand(@"
                    INSERT INTO dbo.Partners (BaseAPIAddress, Name, Password, PrimaryEmail) 
                    VALUES ('https://portal.visualacademy.com', 'VisualAcademy', 'Pa$$w0rd', 'visualacademy@visualacademy.com')", connection);

                cmdInsert.ExecuteNonQuery();
            }
        }
    }
}