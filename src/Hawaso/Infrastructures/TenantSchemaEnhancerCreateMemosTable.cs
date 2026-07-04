#nullable enable

using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;

namespace Hawaso.Infrastructures;

public class TenantSchemaEnhancerCreateMemosTable
{
    private readonly string _masterConnectionString;

    public TenantSchemaEnhancerCreateMemosTable(string masterConnectionString)
    {
        if (string.IsNullOrWhiteSpace(masterConnectionString))
        {
            throw new ArgumentException("Master connection string cannot be null or whitespace.", nameof(masterConnectionString));
        }

        _masterConnectionString = masterConnectionString;
    }

    public void EnhanceAllTenantDatabases()
    {
        List<string> tenantConnectionStrings = GetTenantConnectionStrings();

        foreach (string connectionString in tenantConnectionStrings)
        {
            CreateMemosTableIfNotExists(connectionString);
        }
    }

    private List<string> GetTenantConnectionStrings()
    {
        List<string> result = new();

        using SqlConnection connection = new(_masterConnectionString);
        connection.Open();

        using SqlCommand command = new(
            "SELECT ConnectionString FROM dbo.Tenants WHERE ConnectionString IS NOT NULL",
            connection);

        using SqlDataReader reader = command.ExecuteReader();

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

    private void CreateMemosTableIfNotExists(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        using SqlConnection connection = new(connectionString);
        connection.Open();

        using SqlCommand commandCheck = new(@"
SELECT COUNT(*)
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_SCHEMA = 'dbo'
AND TABLE_NAME = 'Memos';", connection);

        int tableCount = Convert.ToInt32(commandCheck.ExecuteScalar() ?? 0);

        if (tableCount > 0)
        {
            return;
        }

        using SqlCommand commandCreateTable = new(@"
CREATE TABLE [dbo].[Memos](
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    ParentId BIGINT NULL,
    ParentKey NVARCHAR(255) NULL,
    CreatedBy NVARCHAR(255) NULL,
    Created DATETIMEOFFSET DEFAULT(GETDATE()) NULL,
    ModifiedBy NVARCHAR(255) NULL,
    Modified DATETIMEOFFSET NULL,
    Name NVARCHAR(255) NOT NULL,
    PostDate DATETIME DEFAULT GETDATE() NOT NULL,
    PostIp NVARCHAR(15) NULL,
    Title NVARCHAR(150) NOT NULL,
    Content NTEXT NOT NULL,
    Category NVARCHAR(20) DEFAULT('Free') NULL,
    Email NVARCHAR(100) NULL,
    Password NVARCHAR(255) NULL,
    ReadCount INT DEFAULT 0,
    Encoding NVARCHAR(20) NOT NULL,
    Homepage NVARCHAR(100) NULL,
    ModifyDate DATETIME NULL,
    ModifyIp NVARCHAR(15) NULL,
    CommentCount INT DEFAULT 0,
    IsPinned BIT DEFAULT 0 NULL,
    FileName NVARCHAR(255) NULL,
    FileSize INT DEFAULT 0,
    DownCount INT DEFAULT 0,
    Ref INT NOT NULL,
    Step INT NOT NULL DEFAULT 0,
    RefOrder INT NOT NULL DEFAULT 0,
    AnswerNum INT NOT NULL DEFAULT 0,
    ParentNum INT NOT NULL DEFAULT 0,
    Num INT NULL,
    UserId INT NULL,
    CategoryId INT NULL DEFAULT 0,
    BoardId INT NULL DEFAULT 0,
    ApplicationId INT NULL DEFAULT 0
);", connection);

        commandCreateTable.ExecuteNonQuery();
    }
}

//// 테넌트 데이터베이스에 Memos 테이블 생성
//using (var scope = app.ApplicationServices.CreateScope())
//{
//    var services = scope.ServiceProvider;
//    string masterConnectionString = Configuration.GetConnectionString("DefaultConnection");
//    var schemaEnhancerMemos = new TenantSchemaEnhancerCreateMemosTable(masterConnectionString);
//    schemaEnhancerMemos.EnhanceAllTenantDatabases();
//}