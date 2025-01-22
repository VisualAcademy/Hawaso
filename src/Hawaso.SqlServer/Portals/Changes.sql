CREATE TABLE Changes (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Email NVARCHAR(255) NULL,
    UserName NVARCHAR(255) NULL,
    PhoneNumber NVARCHAR(50) NULL, -- Identity
    Address NVARCHAR(255) NULL, -- Identity

    SecondaryPhone NVARCHAR(255) NULL, -- Mobile Phone
    MobilePhone NVARCHAR(50) NULL,
    FirstName NVARCHAR(255) NULL,
    MiddleName NVARCHAR(255) NULL,
    LastName NVARCHAR(255) NULL,
    Age INT NULL,
    IsComplete BIT NULL,
    IsActive BIT NULL,
    CreatedAt DATETIME DEFAULT GETDATE() NULL,
    TenantName NVARCHAR(255) NULL,
    SSN NVARCHAR(255) NULL,
    CriminalHistory NVARCHAR(Max) NULL,


    PrimaryPhone NVARCHAR(255) NULL, -- Home Phone
    PhysicalAddress NVARCHAR(255) NULL,         -- 물리적 주소
    MailingAddress NVARCHAR(255) NULL,          -- 우편 주소

    NewEmail NVARCHAR(255) NULL,                -- 새로운 이메일
    BadgeName NVARCHAR(255) Null,
    ReasonForChange NVARCHAR(Max) Null,         -- 변경 이유

    MaritalStatus NVARCHAR(50) NULL,            -- 결혼 상태
    SpousesName NVARCHAR(100) NULL,             -- 배우자 이름

    RoommateName1 NVarChar(255) Null,
    RoommateName2 NVarChar(255) Null,

    RelationshipDisclosureName NVARCHAR(100) NULL, -- 관계 공개: 이름
    RelationshipDisclosurePosition NVARCHAR(100) NULL, -- 관계 공개: 직위
    RelationshipDisclosure NVARCHAR(MAX) NULL,  -- 관계 공개: 설명
    AdditionalEmploymentBusinessName NVARCHAR(255) NULL, -- 추가 고용: 회사 이름
    AdditionalEmploymentStartDate DATE NULL,    -- 추가 고용: 시작 날짜
    AdditionalEmploymentEndDate DATE NULL,      -- 추가 고용: 종료 날짜
    AdditionalEmploymentLocation NVARCHAR(255) NULL -- 추가 고용: 위치
);
