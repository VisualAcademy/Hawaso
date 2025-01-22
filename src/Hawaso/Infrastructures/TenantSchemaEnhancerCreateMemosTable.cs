namespace Hawaso.Infrastructures;

public class TenantSchemaEnhancerCreateMemosTable
{
    private string _masterConnectionString;

    public TenantSchemaEnhancerCreateMemosTable(string masterConnectionString)
    {
        _masterConnectionString = masterConnectionString;
    }

    public void EnhanceAllTenantDatabases()
    {
        List<string> tenantConnectionStrings = GetTenantConnectionStrings();

        foreach (string connStr in tenantConnectionStrings)
        {
            CreateMemosTableIfNotExists(connStr);
        }
    }

    private List<string> GetTenantConnectionStrings()
    {
        List<string> result = new List<string>();

        using (SqlConnection connection = new SqlConnection(_masterConnectionString))
        {
            connection.Open();

            SqlCommand cmd = new SqlCommand("SELECT ConnectionString FROM dbo.Tenants", connection);
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    result.Add(reader["ConnectionString"].ToString());
                }
            }

            connection.Close();
        }

        return result;
    }

    private void CreateMemosTableIfNotExists(string connectionString)
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            connection.Open();

            SqlCommand cmdCheck = new SqlCommand(@"
                    SELECT COUNT(*) 
                    FROM INFORMATION_SCHEMA.TABLES 
                    WHERE TABLE_SCHEMA = 'dbo' 
                    AND TABLE_NAME = 'Memos'", connection);

            int tableCount = (int)cmdCheck.ExecuteScalar();

            if (tableCount == 0)
            {
                SqlCommand cmdCreateTable = new SqlCommand(@"
                        CREATE TABLE [dbo].[Memos](
                            Id BIGINT IDENTITY(1,1) PRIMARY KEY,
                            ParentId BigInt Null,
                            ParentKey NVarChar(255) Null,
                            CreatedBy NVarChar(255) Null,
                            Created DATETIMEOFFSET Default(GetDate()) Null,
                            ModifiedBy NVarChar(255) Null,
                            Modified DATETIMEOFFSET Null,
                            Name NVarChar(255) Not Null,
                            PostDate DateTime Default GetDate() Not Null,
                            PostIp NVarChar(15) Null,
                            Title NVarChar(150) Not Null,
                            Content NText Not Null,
                            Category NVarChar(20) Default('Free') Null,
                            Email NVarChar(100) Null,
                            Password NVarChar(255) Null,
                            ReadCount Int Default 0,
                            Encoding NVarChar(20) Not Null,
                            Homepage NVarChar(100) Null,
                            ModifyDate DateTime Null,
                            ModifyIp NVarChar(15) Null,
                            CommentCount Int Default 0,
                            IsPinned Bit Default 0 Null,
                            FileName NVarChar(255) Null,
                            FileSize Int Default 0,
                            DownCount Int Default 0,
                            Ref Int Not Null,
                            Step Int Not Null Default 0,
                            RefOrder Int Not Null Default 0,
                            AnswerNum Int Not Null Default 0,
                            ParentNum Int Not Null Default 0,
                            Num Int Null,
                            UserId Int Null,
                            CategoryId Int Null Default 0,
                            BoardId Int Null Default 0,
                            ApplicationId Int Null Default 0
                        )", connection);

                cmdCreateTable.ExecuteNonQuery();
            }

            connection.Close();
        }
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
