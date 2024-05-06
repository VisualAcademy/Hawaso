CREATE TABLE [dbo].[Audit](
	[ID] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,		-- 기본 키, 자동 증가
	[Title] [nvarchar](max) NULL,						-- 감사 제목
	[Business] [nvarchar](max) NULL,					-- 관련 사업 단위
	[Status] [nvarchar](max) NULL,						-- 현재 상태
	[CreatedDate] [datetime2](7) NULL,					-- 생성 날짜
	[EndDate] [datetime2](7) NULL,						-- 종료 날짜
	[FollowupDate] [datetime2](7) NULL,					-- 추적 감사의 마감일
	[Progress] [nvarchar](max) NULL,					-- 진행 상황
	[Percentage] [nvarchar](max) NULL,					-- 완료 비율
	[Objective] [nvarchar](max) NULL,					-- 감사 목표
	[ProcedureScope] [nvarchar](max) NULL,				-- 절차 및 범위
	[WPGenerated] [bit] NULL,							-- 작업지시서가 생성되었는지 여부
	[ChecklistID] [int] NULL,							-- 체크리스트 ID
	[Active] [bit] NULL,								-- 활성 상태 여부
	[NeedFollowup] [bit] NULL,							-- 추적 감사 필요 여부
	[BusinessID] [int] NULL,							-- 사업 단위 ID
	[CreatedBy] [int] NULL,								-- 생성자 ID
	[ArchievedBy] [int] NULL,							-- 보관 처리한 사용자 ID
	[DepartmentID] [int] NULL,							-- 부서 ID
	[AssignedTo] [nvarchar](50) NULL,					-- 담당자
	[EstDate] [datetime2](7) NULL,						-- 예정 날짜
	[EstHour] [int] NULL,								-- 예정 시간
	[ActualHour] [int] NULL,							-- 실제 소요 시간
	[Description] [nvarchar](max) NULL,					-- 설명
	[StartDate] [datetime2](7) NULL					-- 시작 날짜
)
Go
