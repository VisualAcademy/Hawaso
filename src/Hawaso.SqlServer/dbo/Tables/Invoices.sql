CREATE TABLE [dbo].[Invoices]
(
    [Id]           BIGINT IDENTITY(1,1) NOT NULL, -- 인보이스 고유 식별자 (자동 증가 PK)
        
    [TenantId]     NVARCHAR(MAX) NOT NULL,       -- 멀티테넌트 구분용 테넌트 ID

    [ClientId]     BIGINT NULL,                  -- 고객(클라이언트) 내부 식별자 (FK 가능)

    [TenantName]   NVARCHAR(128) NOT NULL,       -- 테넌트 표시 이름
    [TenantKey]    NVARCHAR(128) NOT NULL,       -- 테넌트 고유 키 (시스템 식별용)
    [Email]        NVARCHAR(256) NOT NULL,       -- 청구 대상 이메일 원본 값
    [EmailNormalized] NVARCHAR(256) NOT NULL,    -- 검색/비교용 정규화 이메일
    [FirstName]    NVARCHAR(100) NULL,           -- 고객 이름
    [MiddleName]   NVARCHAR(100) NULL,           -- 고객 중간 이름
    [LastName]     NVARCHAR(100) NULL,           -- 고객 성
    [ClientName]   NVARCHAR(200) NULL,           -- 고객 전체 이름 (표시용)
    [ClientType]   NVARCHAR(64)  NULL,           -- 고객 유형 (예: Individual, Company 등)
    [InvoiceNumber] NVARCHAR(32) NULL,           -- 인보이스 번호 (외부 표시용)

    [IssueDateUtc] DATETIME2 NOT NULL,           -- 인보이스 발행일 (UTC 기준)
    [DueDateUtc]   DATETIME2 NULL,               -- 납부 기한 (UTC 기준)

    [Currency]     NVARCHAR(8) NOT NULL,         -- 통화 코드 (예: USD, KRW)

    [ApplyTax]     BIT NOT NULL,                 -- 세금 적용 여부
    [TaxRate]      DECIMAL(18,2) NOT NULL,       -- 세율 (%)
    [Subtotal]     DECIMAL(18,2) NOT NULL,       -- 공급가액 (세전 금액)
    [Tax]          DECIMAL(18,2) NOT NULL,       -- 세금 금액
    [Total]        DECIMAL(18,2) NOT NULL,       -- 총 금액 (공급가액 + 세금)

    [Status]       INT NOT NULL,                 -- 인보이스 상태 코드 (예: Draft, Sent, Paid 등)

    [PdfPath]      NVARCHAR(MAX) NULL,           -- 생성된 PDF 파일 경로

    [CreatedUtc]   DATETIME2 NOT NULL,           -- 생성일 (UTC)
    [UpdatedUtc]   DATETIME2 NOT NULL,           -- 최종 수정일 (UTC)
    [EmailSentUtc] DATETIME2 NULL,               -- 이메일 발송일 (UTC)

    [IsDeleted]    BIT NOT NULL,                 -- 소프트 삭제 여부
    [DeletedUtc]   DATETIME2 NULL,               -- 삭제 일시 (UTC)

    [PaidDateUtc]  DATETIME2 NULL,               -- 실제 결제 완료 일시 (UTC)

    [VendorId] BIGINT NULL, -- 소속 Vendor 식별자
    [VendorName]   NVARCHAR(200) NULL,           -- 소속 Vendor 표시 이름

    [FeeName] NVARCHAR(100) NULL,
    [Fee] DECIMAL(18,2) NOT NULL DEFAULT (0),

    CONSTRAINT [PK_Invoices] PRIMARY KEY CLUSTERED ([Id] ASC) -- 기본키
);
GO
