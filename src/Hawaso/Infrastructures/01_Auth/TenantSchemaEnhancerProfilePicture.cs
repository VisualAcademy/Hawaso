using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System;

namespace Azunt.Web.Infrastructures.Auth;

/// <summary>
/// AspNetUsers.ProfilePicture 컬럼만 추가하는 전용 클래스
/// </summary>
public class TenantSchemaEnhancerProfilePicture
{
    private readonly string _connectionString;
    private readonly ILogger<TenantSchemaEnhancerProfilePicture> _logger;

    public TenantSchemaEnhancerProfilePicture(
        string connectionString,
        ILogger<TenantSchemaEnhancerProfilePicture> logger)
    {
        _connectionString = connectionString;
        _logger = logger;
    }

    /// <summary>
    /// ProfilePicture 컬럼 추가
    /// </summary>
    public void EnsureProfilePictureColumn()
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            // 컬럼 존재 여부 체크
            using var checkCmd = new SqlCommand(@"
                SELECT COUNT(*)
                FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_NAME = 'AspNetUsers'
                  AND COLUMN_NAME = 'ProfilePicture';
            ", connection);

            var exists = (int)checkCmd.ExecuteScalar();

            if (exists == 0)
            {
                using var alterCmd = new SqlCommand(@"
                    ALTER TABLE [dbo].[AspNetUsers]
                    ADD [ProfilePicture] VARBINARY(MAX) NULL;
                ", connection);

                alterCmd.ExecuteNonQuery();

                _logger.LogInformation("AspNetUsers.ProfilePicture column added.");
            }
            else
            {
                _logger.LogInformation("AspNetUsers.ProfilePicture already exists.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to ensure ProfilePicture column.");
            throw;
        }
    }
}