CREATE TABLE Pages (
    Id INT IDENTITY(1,1) PRIMARY KEY, -- 기본 키 (Primary Key)
    TenantName NVARCHAR(MAX) NOT NULL DEFAULT 'Hawaso', -- 테넌트명 (예: Hawaso, Tenant1 등)
    PageName VARCHAR(50) NOT NULL DEFAULT 'Contact', -- 페이지명 (예: Contact, About 등)
    Title NVARCHAR(200) NOT NULL, -- 페이지 제목
    Content NVARCHAR(MAX) NOT NULL, -- 페이지의 본문 (HTML 또는 Plain Text)
    LastUpdated DATETIME NOT NULL DEFAULT GETDATE() -- 마지막 수정 날짜 (기본값: 현재 시간)
);