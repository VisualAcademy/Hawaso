CREATE TABLE [dbo].[PurgesArchives]
(
    [Num] BigINT NOT NULL PRIMARY KEY Identity(1, 1),
    PurgeDate DateTime Default(GetDate()), 


    -- 나머지는 Employees

)
Go
