-- =============================================
-- Skills Matrix Legend Configuration Tables for PostgreSQL
-- Creates tables for storing customizable legend configurations
-- Version: 1.0
-- =============================================

-- Create the configuration table
CREATE TABLE IF NOT EXISTS skill_matrix_legend_configuration (
    id SERIAL PRIMARY KEY,
    company_id INTEGER NOT NULL,
    version INTEGER NOT NULL DEFAULT 1,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE NULL,
    created_by INTEGER NULL,
    updated_by INTEGER NULL,
    CONSTRAINT uq_skill_matrix_legend_config_company_id UNIQUE (company_id)
);

-- Create index on company_id for faster lookups
CREATE INDEX IF NOT EXISTS ix_skill_matrix_legend_config_company_id
ON skill_matrix_legend_configuration (company_id);

-- Add comment to table
COMMENT ON TABLE skill_matrix_legend_configuration IS 'Stores customizable Skills Matrix Legend configurations per company';
COMMENT ON COLUMN skill_matrix_legend_configuration.company_id IS 'Reference to the company this configuration belongs to';
COMMENT ON COLUMN skill_matrix_legend_configuration.version IS 'Version number that increments on each update for tracking changes';

-- Create the legend item table
CREATE TABLE IF NOT EXISTS skill_matrix_legend_item (
    id SERIAL PRIMARY KEY,
    configuration_id INTEGER NOT NULL,
    skill_level_id INTEGER NOT NULL,
    skill_type VARCHAR(20) NOT NULL,
    label VARCHAR(255) NOT NULL,
    description VARCHAR(500) NULL,
    icon_color VARCHAR(7) NOT NULL,
    background_color VARCHAR(7) NOT NULL,
    sort_order INTEGER NOT NULL,
    score_value INTEGER NULL,
    icon_class VARCHAR(50) NULL,
    is_default BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE NULL,
    CONSTRAINT fk_skill_matrix_legend_item_config
        FOREIGN KEY (configuration_id)
        REFERENCES skill_matrix_legend_configuration (id)
        ON DELETE CASCADE,
    CONSTRAINT ck_skill_matrix_legend_item_skill_type
        CHECK (skill_type IN ('mandatory', 'operational')),
    CONSTRAINT ck_skill_matrix_legend_item_score_value
        CHECK (score_value IS NULL OR (score_value >= 1 AND score_value <= 5)),
    CONSTRAINT ck_skill_matrix_legend_item_icon_color
        CHECK (icon_color ~ '^#[0-9A-Fa-f]{6}$'),
    CONSTRAINT ck_skill_matrix_legend_item_background_color
        CHECK (background_color ~ '^#[0-9A-Fa-f]{6}$')
);

-- Create indexes for legend items
CREATE INDEX IF NOT EXISTS ix_skill_matrix_legend_item_config_id
ON skill_matrix_legend_item (configuration_id);

CREATE INDEX IF NOT EXISTS ix_skill_matrix_legend_item_skill_type_order
ON skill_matrix_legend_item (configuration_id, skill_type, sort_order);

-- Create unique constraint to prevent duplicate skill levels per configuration and type
CREATE UNIQUE INDEX IF NOT EXISTS uq_skill_matrix_legend_item_config_skill_level
ON skill_matrix_legend_item (configuration_id, skill_type, skill_level_id);

-- Add comments to item table
COMMENT ON TABLE skill_matrix_legend_item IS 'Stores individual legend items for Skills Matrix configurations';
COMMENT ON COLUMN skill_matrix_legend_item.skill_level_id IS 'Skill level identifier (1-5 for operational, 1-3 for mandatory)';
COMMENT ON COLUMN skill_matrix_legend_item.skill_type IS 'Type of skill: mandatory or operational';
COMMENT ON COLUMN skill_matrix_legend_item.label IS 'Display label for this skill level';
COMMENT ON COLUMN skill_matrix_legend_item.description IS 'Description text for this skill level';
COMMENT ON COLUMN skill_matrix_legend_item.icon_color IS 'HEX color code for the icon/number (e.g., #FF8800)';
COMMENT ON COLUMN skill_matrix_legend_item.background_color IS 'HEX color code for the background (e.g., #FFFFFF)';
COMMENT ON COLUMN skill_matrix_legend_item.sort_order IS 'Display order (ascending)';
COMMENT ON COLUMN skill_matrix_legend_item.score_value IS 'Score value for operational skills (1-5)';
COMMENT ON COLUMN skill_matrix_legend_item.icon_class IS 'CSS class for the icon (e.g., thumbsup, thumbsdown, warning)';
COMMENT ON COLUMN skill_matrix_legend_item.is_default IS 'Whether this is a default system-provided legend item';
