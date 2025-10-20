-- 특정 Employee의 과거 주소(other_addresses) 펼치기
SELECT e.ID AS EmployeeID, a.*
FROM dbo.Employees AS e
CROSS APPLY OPENJSON(e.FormData, '$.other_addresses')
WITH
(
    source                  NVARCHAR(255)   '$.source',
    address                 NVARCHAR(MAX)   '$.address',
    city                    NVARCHAR(255)   '$.city',
    county                  NVARCHAR(255)   '$.county',
    state                   NVARCHAR(50)    '$.state',
    country                 NVARCHAR(255)   '$.country',
    postal_code             NVARCHAR(50)    '$.postal_code',

    start_date              NVARCHAR(50)    '$.start_date',
    end_date                NVARCHAR(50)    '$.end_date',

    landlord                NVARCHAR(255)   '$.landlord',
    landlord_address        NVARCHAR(MAX)   '$.landlord_address',
    landlord_city           NVARCHAR(255)   '$.landlord_city',
    landlord_state          NVARCHAR(50)    '$.landlord_state',
    landlord_county         NVARCHAR(255)   '$.landlord_county',
    landlord_postal_code    NVARCHAR(50)    '$.landlord_postal_code',

    phone                   NVARCHAR(50)    '$.phone',
    own_rent                NVARCHAR(50)    '$.own_rent',
    residence               NVARCHAR(50)    '$.residence',              -- enum: own/rent/other
    residence_description   NVARCHAR(255)   '$.residence_description',
    current_address         bit             '$.current_address'         -- JSON bool → bit
) AS a
WHERE e.ID = 170003;  -- 필요 시 파라미터로