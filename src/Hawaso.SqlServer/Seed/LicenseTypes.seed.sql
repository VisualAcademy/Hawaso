-- =============================================================
-- Seed: LicenseTypes
-- =============================================================
PRINT N'Seeding data: dbo.LicenseTypes';

;WITH S AS (
    SELECT *
    FROM (VALUES
        (N'Gaming',     1, NULL, NULL),
        (N'Non-Gaming', 1, NULL, NULL),
        (N'Principal',  2, NULL, NULL),
        (N'Employee',   2, NULL, NULL)
    ) V([Type],[ApplicantType],[Description],[BgRequired])
)
MERGE dbo.LicenseTypes AS T
USING S
ON (T.[Type] = S.[Type] AND ISNULL(T.[ApplicantType],0) = ISNULL(S.[ApplicantType],0))
WHEN MATCHED AND T.Active <> 1
    THEN UPDATE SET 
         T.Active = 1,
         -- 혹시 기존에 NULL인 BgRequired가 있으면 0으로 정리
         T.BgRequired = ISNULL(T.BgRequired, 0)
WHEN NOT MATCHED BY TARGET
    THEN INSERT (Active, CreatedAt, CreatedBy, [Type], [Description], [ApplicantType], [BgRequired])
         VALUES (
             1,
             SYSDATETIMEOFFSET(),
             N'Seed',
             S.[Type],
             S.[Description],
             S.[ApplicantType],
             ISNULL(S.[BgRequired], 0)  -- ← 여기서 NULL이면 0(false)로 강제
         );

PRINT N'✔ LicenseTypes seed complete.';
