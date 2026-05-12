--BEGIN TRY
--    BEGIN TRANSACTION;

--    ------------------------------------------------------------
--    -- 0. 안전 확인: Custom1 값이 LicenseNumber(NVARCHAR(35))보다 긴지 확인
--    --    결과가 있으면 LicenseNumber로 이동 시 잘릴 수 있으므로 중단
--    ------------------------------------------------------------
--    IF EXISTS (
--        SELECT 1
--        FROM dbo.Employees
--        WHERE LEN(Custom1) > 35
--    )
--    BEGIN
--        THROW 50001, 'Custom1 contains values longer than 35 characters. LicenseNumber is NVARCHAR(35), so data may be truncated.', 1;
--    END;

--    ------------------------------------------------------------
--    -- 1. Employees 테이블 백업
--    --    이미 같은 이름의 백업 테이블이 있으면 중단
--    ------------------------------------------------------------
--    IF OBJECT_ID('dbo.Employees20260512', 'U') IS NOT NULL
--    BEGIN
--        THROW 50002, 'Backup table dbo.Employees20260512 already exists.', 1;
--    END;

--    SELECT *
--    INTO dbo.Employees20260512
--    FROM dbo.Employees;

--    ------------------------------------------------------------
--    -- 2. 백업용 컬럼 추가
--    --    StateCertificationNumber는 업무상 신규 컬럼이 아니라
--    --    기존 LicenseNumber 백업/검증용 컬럼
--    ------------------------------------------------------------
--    IF COL_LENGTH('dbo.Employees', 'StateCertificationNumber') IS NULL
--    BEGIN
--        ALTER TABLE dbo.Employees
--        ADD StateCertificationNumber NVARCHAR(35) NULL;
--    END;

--    ------------------------------------------------------------
--    -- 3. 기존 LicenseNumber 값을 StateCertificationNumber에 백업
--    ------------------------------------------------------------
--    UPDATE dbo.Employees
--    SET StateCertificationNumber = LicenseNumber;

--    ------------------------------------------------------------
--    -- 4. LicenseNumber와 Custom1 값 SWAP
--    --
--    --    기존 LicenseNumber -> Custom1
--    --    기존 Custom1       -> LicenseNumber
--    --
--    --    결과:
--    --    StateCertificationNumber = 기존 LicenseNumber
--    --    Custom1                  = 기존 LicenseNumber
--    --    LicenseNumber            = 기존 Custom1
--    ------------------------------------------------------------
--    UPDATE dbo.Employees
--    SET
--        LicenseNumber = CONVERT(NVARCHAR(35), Custom1),
--        Custom1 = StateCertificationNumber;

--    COMMIT TRANSACTION;
--END TRY
--BEGIN CATCH
--    IF @@TRANCOUNT > 0
--        ROLLBACK TRANSACTION;

--    THROW;
--END CATCH;



--SELECT TOP (100)
--    E.ID,

--    B.LicenseNumber AS Backup_OldLicenseNumber,
--    B.Custom1       AS Backup_OldCustom1,

--    E.StateCertificationNumber AS Current_StateCertificationNumber,
--    E.Custom1                  AS Current_Custom1,
--    E.LicenseNumber            AS Current_LicenseNumber,

--    CASE
--        WHEN ISNULL(E.StateCertificationNumber, '') = ISNULL(B.LicenseNumber, '')
--         AND ISNULL(E.Custom1, '') = ISNULL(B.LicenseNumber, '')
--         AND ISNULL(E.LicenseNumber, '') = ISNULL(CONVERT(NVARCHAR(35), B.Custom1), '')
--            THEN 'OK'
--        ELSE 'Mismatch'
--    END AS SwapCheck
--FROM dbo.Employees AS E
--INNER JOIN dbo.Employees20260512 AS B
--    ON E.ID = B.ID
--ORDER BY E.ID DESC;
