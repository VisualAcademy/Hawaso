CREATE TABLE AllowedIpRanges (
    Id INT PRIMARY KEY IDENTITY(1,1),       -- 자동 증가하는 기본 키
    StartIpRange VARCHAR(15),               -- IP 범위 시작 주소
    EndIpRange VARCHAR(15),                 -- IP 범위 끝 주소
    Description NVarChar(Max),              -- IP 범위에 대한 설명
    CreateDate DATETIME Default(GetDate()), -- 범위가 추가된 날짜
    TenantId BIGINT                         -- 테넌트 ID
);
