﻿CREATE TABLE [dbo].[DailyLogs] (
    [ID]                INT             IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [ParentKey]         NVARCHAR(255)   NULL,  
    [LogNumber]         NVARCHAR(50)    NULL,
    [Occurred]          SMALLDATETIME   NULL,
    [Ended]             SMALLDATETIME   NULL,
    [PropertyID]        INT             NULL,
    [LocationID]        INT             NULL,
    [SublocationID]     INT             NULL,
    [DepartmentID]      INT             NULL,
    [TopicID]          INT             NULL,
    [Action]            NVARCHAR(MAX)   NULL,
    [Synopsis]          NVARCHAR(MAX)   NULL,
    [StatusID]          INT             NULL,
    [CreatedBy]         NVARCHAR(255)   NULL,
    [CreatedDate]       DATETIMEOFFSET  NULL,
    [ModifiedBy]        NVARCHAR(255)   NULL,
    [ModifiedDate]      DATETIMEOFFSET  NULL,
    [Active]            BIT             NULL,
    [Reference]         NVARCHAR(MAX)   NULL,
    [Topic]             NVARCHAR(MAX)   NULL,
    [SecondaryOperator] NVARCHAR(255)   NULL,
    [LogDetails]        NVARCHAR(MAX)   NULL,
    [DispatchCallID]    INT             NULL,
    [InvestigatorID]    INT             NULL,
    [Amount]            MONEY           NULL,
    [Attachment]        NVARCHAR(50)    NULL,
    [Status]            NVARCHAR(50)    NULL,
    [Property]          NVARCHAR(50)    NULL,
    [Location]          NVARCHAR(50)    NULL,
    [Sublocation]       NVARCHAR(50)    NULL,
    [Agent]             NVARCHAR(50)    NULL,
    [Adjusted]          BIT             NULL,
    [Sgc]              BIT             NULL,
    [Surveillance]      BIT             NULL,
    [Security]          BIT             NULL,

    DivisionId BIGINT NULL DEFAULT 0, 
    DivisionName NVARCHAR(255) NULL, 

);
