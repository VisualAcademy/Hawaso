CREATE TABLE [dbo].[Employees]
(
    [Id] INT NOT NULL PRIMARY KEY,

    [Address] [nvarchar](500), -- 주소

    [Ethnicity] [nvarchar](50), -- 민족성(Ethnicity), 인종(Race) 

    PostalCode [nvarchar](35), -- 우편번호

    LicenseRenewalDate [datetime2](7), -- 면허 갱신 날짜 
)
Go
