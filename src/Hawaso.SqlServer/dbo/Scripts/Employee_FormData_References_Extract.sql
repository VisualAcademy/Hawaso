  -- 특정 Employee의 레퍼런스 펼치기
SELECT e.ID AS EmployeeID, r.*
FROM dbo.Employees e
CROSS APPLY OPENJSON(e.FormData, '$.references')
WITH
(
    first_name NVARCHAR(255) '$.first_name',
    last_name  NVARCHAR(255) '$.last_name',
    email      NVARCHAR(255) '$.email',
    home_phone NVARCHAR(50)  '$.home_phone',
    work_phone NVARCHAR(50)  '$.work_phone',
    mobile_phone NVARCHAR(50) '$.mobile_phone',
    address NVARCHAR(MAX)    '$.address',
    city    NVARCHAR(255)    '$.city',
    state   NVARCHAR(50)     '$.state',
    postal_code NVARCHAR(50) '$.postal_code',
    occupation NVARCHAR(255) '$.occupation',
    year_met NVARCHAR(50)    '$.year_met',
    years_known NVARCHAR(50) '$.years_known'
) AS r
WHERE e.ID = 170003;