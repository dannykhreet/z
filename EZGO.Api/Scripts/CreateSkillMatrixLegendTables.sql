-- =============================================
-- Skills Matrix Legend Configuration Tables
-- Creates tables for storing customizable legend configurations
-- =============================================

-- Create the configuration table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SkillMatrixLegendConfiguration]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[SkillMatrixLegendConfiguration] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [CompanyId] INT NOT NULL,
        [Version] INT NOT NULL DEFAULT 1,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NULL,
        [CreatedBy] INT NULL,
        [UpdatedBy] INT NULL,
        CONSTRAINT [PK_SkillMatrixLegendConfiguration] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [UQ_SkillMatrixLegendConfiguration_CompanyId] UNIQUE NONCLUSTERED ([CompanyId] ASC)
    );

    PRINT 'Created table: SkillMatrixLegendConfiguration';
END
GO

-- Create index on CompanyId for faster lookups
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_SkillMatrixLegendConfiguration_CompanyId')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_SkillMatrixLegendConfiguration_CompanyId]
    ON [dbo].[SkillMatrixLegendConfiguration] ([CompanyId] ASC);

    PRINT 'Created index: IX_SkillMatrixLegendConfiguration_CompanyId';
END
GO

-- Create the legend item table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SkillMatrixLegendItem]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[SkillMatrixLegendItem] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [ConfigurationId] INT NOT NULL,
        [SkillLevelId] INT NOT NULL,
        [SkillType] NVARCHAR(20) NOT NULL,
        [Label] NVARCHAR(255) NOT NULL,
        [Description] NVARCHAR(500) NULL,
        [IconColor] NVARCHAR(7) NOT NULL,
        [BackgroundColor] NVARCHAR(7) NOT NULL,
        [Order] INT NOT NULL,
        [ScoreValue] INT NULL,
        [IconClass] NVARCHAR(50) NULL,
        [IsDefault] BIT NOT NULL DEFAULT 0,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NULL,
        CONSTRAINT [PK_SkillMatrixLegendItem] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_SkillMatrixLegendItem_Configuration] FOREIGN KEY ([ConfigurationId])
            REFERENCES [dbo].[SkillMatrixLegendConfiguration] ([Id])
            ON DELETE CASCADE,
        CONSTRAINT [CK_SkillMatrixLegendItem_SkillType] CHECK ([SkillType] IN ('mandatory', 'operational')),
        CONSTRAINT [CK_SkillMatrixLegendItem_ScoreValue] CHECK ([ScoreValue] IS NULL OR ([ScoreValue] >= 1 AND [ScoreValue] <= 5)),
        CONSTRAINT [CK_SkillMatrixLegendItem_IconColor] CHECK ([IconColor] LIKE '#[0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f]'),
        CONSTRAINT [CK_SkillMatrixLegendItem_BackgroundColor] CHECK ([BackgroundColor] LIKE '#[0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f]')
    );

    PRINT 'Created table: SkillMatrixLegendItem';
END
GO

-- Create indexes for legend items
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_SkillMatrixLegendItem_ConfigurationId')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_SkillMatrixLegendItem_ConfigurationId]
    ON [dbo].[SkillMatrixLegendItem] ([ConfigurationId] ASC);

    PRINT 'Created index: IX_SkillMatrixLegendItem_ConfigurationId';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_SkillMatrixLegendItem_SkillType_Order')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_SkillMatrixLegendItem_SkillType_Order]
    ON [dbo].[SkillMatrixLegendItem] ([ConfigurationId] ASC, [SkillType] ASC, [Order] ASC);

    PRINT 'Created index: IX_SkillMatrixLegendItem_SkillType_Order';
END
GO

-- Create unique constraint to prevent duplicate skill levels per configuration and type
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'UQ_SkillMatrixLegendItem_ConfigSkillLevel')
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [UQ_SkillMatrixLegendItem_ConfigSkillLevel]
    ON [dbo].[SkillMatrixLegendItem] ([ConfigurationId] ASC, [SkillType] ASC, [SkillLevelId] ASC);

    PRINT 'Created unique index: UQ_SkillMatrixLegendItem_ConfigSkillLevel';
END
GO

PRINT 'Skills Matrix Legend tables created successfully';
GO
