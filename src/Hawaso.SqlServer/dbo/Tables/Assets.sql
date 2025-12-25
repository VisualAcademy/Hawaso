CREATE TABLE dbo.Assets
(
    AssetId            BIGINT IDENTITY(1,1) PRIMARY KEY,
    
    -- 기본 정보
    Name               NVARCHAR(200)       NOT NULL,          -- 물건 이름
    Category           NVARCHAR(100)       NULL,              -- 의류 / 가방 / 전자기기 / 가구 등
    Location           NVARCHAR(200)       NULL,              -- 집/방/옷장/서랍/창고 등
    Tags               NVARCHAR(400)       NULL,              -- 쉼표 구분 태그 (예: 필수,미니멀,팔예정)

    -- 금액/구매 정보
    PurchasePrice      DECIMAL(18,2)       NULL,
    PurchaseDate       DATETIMEOFFSET(7)   NULL,              -- 구매 일시 (타임존 포함)

    -- 상태 관리
    Status             NVARCHAR(50)        NULL,              -- 사용중 / 보관중 / 판매예정 / 기부예정 / 폐기예정 등
    IsEssential        BIT                 NOT NULL DEFAULT(0),
    IsActive           BIT                 NOT NULL DEFAULT(1),

    -- 기록/관리용
    Notes              NVARCHAR(MAX)       NULL,

    CreatedAt          DATETIMEOFFSET(7)   NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt          DATETIMEOFFSET(7)   NULL
);
