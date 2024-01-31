CREATE TABLE [dbo].[Incidents] (
    [ID]                     INT            IDENTITY (1, 1) NOT NULL Primary Key,

    [CaseNumber]             NVARCHAR (50)  NULL,

    [DailyLogID]             INT            NULL,

    DailyLogNumber          NVarChar(Max)   Null, 

    [OpenedDate]             SMALLDATETIME  NULL,
    [Occurred]               SMALLDATETIME  NULL,
    [Closed]                 SMALLDATETIME  NULL,

    [PropertyID]             INT            NULL,
    [Property]               NVARCHAR (50)  NULL,

    ReportTypeId            INT             NULL,
    
    ReportSpecificId        INT             NULL,

    [Specific]               NVARCHAR (50)  NULL,
    [SpecificID]             INT            NULL,

    [CaseStatusID]           INT            NULL,

    [LocationID]             INT            NULL,
    [Location]               NVARCHAR (50)  NULL,

    [SublocationID]          INT            NULL,
    [Sublocation]            NVARCHAR (50)  NULL,

    [DepartmentID]           INT            NULL,
    [Department]             NVARCHAR (50)  NULL,
    
    

    [Summary]                NVARCHAR (255) NULL,
    [Details]                NVARCHAR (MAX) NULL,
    [Resolution]             NVARCHAR (255) NULL,
    [Reference]              NVARCHAR (255) NULL,
    [SecondaryOperatorID]    INT            NULL,
    [SecondaryOperator]      NVARCHAR (50)  NULL,

    [Custodial]              BIT            NULL,
    [UseForce]               BIT            NULL,
    [Medical]                BIT            NULL,
    [RiskManagement]         BIT            NULL,
    [Active]                 BIT            NULL,
    [Priority]               NVARCHAR (10)  NULL,

    AgentName                NVarChar(Max) NULL,
    SupervisorName                NVarChar(Max) NULL,
    ManagerName                NVarChar(Max) NULL,
    DirectorName                NVarChar(Max) NULL,

    AgentSignature                NVarChar(Max) NULL,
    SupervisorSignature                NVarChar(Max) NULL,
    ManagerSignature                NVarChar(Max) NULL,
    DirectorSignature                NVarChar(Max) NULL,

    AgentTime               DateTime        NULL,
    SupervisorTime               DateTime        NULL,
    ManagerTime               DateTime        NULL,
    DirectorTime               DateTime        NULL,

    [CaseTypeID]             INT            NULL,
    [InvestigatorID]         INT            NULL,
    [ShiftID]                INT            NULL,
    [ImmediateSupervisorID]  INT            NULL,
    [GamingClassID]          INT            NULL,
    [SurveillanceNotified]   INT            NULL,
    [SurveillanceObserverID] INT            NULL,
    [InitialContact]         NVARCHAR (50)  NULL,
    [InspectorSig]           NVARCHAR (50)  NULL,
    [DeputyDirectorSig]      NVARCHAR (50)  NULL,
    [DirectorSig]            NVARCHAR (50)  NULL,
    [SupervisorSig]          NVARCHAR (50)  NULL,
    [CompletionDate]         SMALLDATETIME  NULL,
    [TGAForwardDate]         SMALLDATETIME  NULL,
    [TGOReturnDate]          SMALLDATETIME  NULL,
    [Citation]               NVARCHAR (255) NULL,
    [ViolationNature]        NVARCHAR (255) NULL,
    [Variance]               MONEY          NULL,
    [Employee]               BIT            NULL,
    [ManagerID]              INT            NULL,
    [TapeIdentification]     NVARCHAR (50)  NULL,
    [ActionTaken]            NVARCHAR (50)  NULL,
    [SuspectPhoto]           NVARCHAR (50)  NULL,
    [ExclusionInfo]          NVARCHAR (255) NULL,
    [Notification]           NVARCHAR (255) NULL,
    [PoliceContacted]        BIT            NULL,
    [PoliceContact]          NVARCHAR (255) NULL,
    [InvestigatorSigTS]      SMALLDATETIME  NULL,
    [SupervisorSigTS]        SMALLDATETIME  NULL,
    [DeputyDirectorSigTS]    SMALLDATETIME  NULL,
    [DirectorSigTS]          SMALLDATETIME  NULL,
    [TGOResponse]            NVARCHAR (255) NULL,
    [CreatedBy]              NVARCHAR (50)  NULL,
    [ClosedBy]               NVARCHAR (50)  NULL,


    [ModifiedBy]             NVARCHAR (50)  NULL,
    [ModifiedDate]           SMALLDATETIME  NULL,
    [DispatchCallID]         INT            NULL,
    [AuditID]                INT            NULL,
    [SavingsOrLosses]        BIT            NULL,
    [DirectorOnly]           BIT            NULL,
    [CaseType]               NVARCHAR (50)  NULL,
    [Status]                 NVARCHAR (50)  NULL,
    [Agent]                  NVARCHAR (50)  NULL,
    [AgentSigFile]           NVARCHAR (50)  NULL,
    [SupervisorSigFile]      NVARCHAR (50)  NULL,
    [ManagerSigFile]         NVARCHAR (50)  NULL,
    [DirectorSigFile]        NVARCHAR (50)  NULL,
    [AgentImage]             IMAGE          NULL,
    [SupervisorImage]        IMAGE          NULL,
    [ManagerImage]           IMAGE          NULL,
    [DirectorImage]          IMAGE          NULL,

    RemarksTitle1            NVarChar(Max)  NULL,
    RemarksMemos1            NVarChar(Max)  NULL,
    RemarksTitle2            NVarChar(Max)  NULL,
    RemarksMemos2            NVarChar(Max)  NULL,
    RemarksTitle3            NVarChar(Max)  NULL,
    RemarksMemos3            NVarChar(Max)  NULL,
    RemarksTitle4            NVarChar(Max)  NULL,
    RemarksMemos4            NVarChar(Max)  NULL,
    RemarksTitle5            NVarChar(Max)  NULL,
    RemarksMemos5            NVarChar(Max)  NULL,



);
