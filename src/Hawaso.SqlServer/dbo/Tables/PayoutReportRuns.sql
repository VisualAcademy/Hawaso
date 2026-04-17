-- 주간/월간 등 Payout 리포트 생성 이력을 저장하는 테이블
-- (엑셀 생성 및 이메일 발송 기록 관리 용도)

CREATE TABLE [dbo].[PayoutReportRuns]
(
    [Id] BIGINT IDENTITY(1,1) NOT NULL,           -- 내부 PK
    [TenantId] NVARCHAR(128) NOT NULL,            -- 테넌트 구분

    [ReportType] NVARCHAR(32) NOT NULL,           -- 리포트 유형 (Weekly / Monthly / Custom)
    [RangeStartUtc] DATETIME2 NOT NULL,           -- 조회 시작 기간 (UTC)
    [RangeEndUtc] DATETIME2 NOT NULL,             -- 조회 종료 기간 (UTC)

    [TotalCount] INT NOT NULL,                    -- 포함된 Payout 건수
    [TotalAmount] DECIMAL(18,2) NOT NULL,         -- 총 지급 금액
    [Currency] NVARCHAR(8) NULL,                  -- 통화 (단일 통화 기준, 필요 시 확장 가능)

    [GeneratedFilePath] NVARCHAR(500) NULL,       -- 생성된 파일 경로 (Blob / FileSystem 등)
    [EmailTo] NVARCHAR(500) NULL,                 -- 발송 대상 이메일
    [EmailSentUtc] DATETIME2 NULL,                -- 이메일 발송 완료 시각

    [CreatedUtc] DATETIME2 NOT NULL 
        CONSTRAINT [DF_PayoutReportRuns_CreatedUtc] DEFAULT (SYSUTCDATETIME()), -- 리포트 생성 시각

    CONSTRAINT [PK_PayoutReportRuns] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

-- 특정 테넌트 + 기간별 리포트 조회 최적화
CREATE INDEX [IX_PayoutReportRuns_TenantId_RangeStartUtc_RangeEndUtc]
ON [dbo].[PayoutReportRuns] ([TenantId], [RangeStartUtc], [RangeEndUtc]);
GO