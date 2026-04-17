-- Stripe에서 조회한 Payout 데이터를 캐시/이력 저장하기 위한 테이블
-- (Stripe가 원본 데이터이며, 본 테이블은 조회 성능 및 리포트 생성을 위한 캐시 역할)

CREATE TABLE [dbo].[StripePayoutSnapshots]
(
    [Id] BIGINT IDENTITY(1,1) NOT NULL,           -- 내부 PK
    [TenantId] NVARCHAR(128) NOT NULL,            -- 테넌트 구분 (Potawatomi 등)

    [StripePayoutId] NVARCHAR(100) NOT NULL,      -- Stripe Payout 고유 ID (po_xxx)
    [Amount] DECIMAL(18,2) NOT NULL,              -- 지급 금액 (Stripe는 cent 단위 → 변환 후 저장)
    [Currency] NVARCHAR(8) NOT NULL,              -- 통화 (USD 등)

    [Status] NVARCHAR(40) NOT NULL,               -- 상태 (pending / in_transit / paid / failed / canceled)
    [ArrivalDateUtc] DATETIME2 NULL,              -- 실제 계좌 입금 예정/완료 날짜
    [CreatedStripeUtc] DATETIME2 NOT NULL,        -- Stripe에서 생성된 시각 (UTC)

    [Description] NVARCHAR(500) NULL,             -- 설명 (있을 경우)
    [Method] NVARCHAR(40) NULL,                   -- 지급 방식 (standard / instant)
    [SourceType] NVARCHAR(40) NULL,               -- 자금 출처 유형 (card / bank_account 등)
    [StatementDescriptor] NVARCHAR(100) NULL,     -- 명세서 표시 문자열

    [FailureCode] NVARCHAR(100) NULL,             -- 실패 코드 (실패 시)
    [FailureMessage] NVARCHAR(1000) NULL,         -- 실패 메시지

    [RawJson] NVARCHAR(MAX) NULL,                 -- Stripe 원본 JSON (디버깅/감사용)
    [SyncedUtc] DATETIME2 NOT NULL 
        CONSTRAINT [DF_StripePayoutSnapshots_SyncedUtc] DEFAULT (SYSUTCDATETIME()), -- 마지막 동기화 시각

    CONSTRAINT [PK_StripePayoutSnapshots] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

-- 동일 테넌트에서 동일 Payout 중복 저장 방지
CREATE UNIQUE INDEX [UX_StripePayoutSnapshots_TenantId_StripePayoutId]
ON [dbo].[StripePayoutSnapshots] ([TenantId], [StripePayoutId]);
GO

-- 최근 생성된 Payout 조회 최적화
CREATE INDEX [IX_StripePayoutSnapshots_TenantId_CreatedStripeUtc]
ON [dbo].[StripePayoutSnapshots] ([TenantId], [CreatedStripeUtc] DESC);
GO

-- 입금 기준 조회(리포트) 최적화
CREATE INDEX [IX_StripePayoutSnapshots_TenantId_ArrivalDateUtc]
ON [dbo].[StripePayoutSnapshots] ([TenantId], [ArrivalDateUtc] DESC);
GO

-- 상태별 필터링 (paid / failed 등) 최적화
CREATE INDEX [IX_StripePayoutSnapshots_TenantId_Status]
ON [dbo].[StripePayoutSnapshots] ([TenantId], [Status]);
GO