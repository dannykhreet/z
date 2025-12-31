-- =============================================
-- Skills Matrix Legend Stored Procedures for PostgreSQL
-- All database operations are performed via stored procedures
-- Version: 1.0
-- =============================================

-- =============================================
-- GET: Get legend configuration by company ID
-- =============================================
CREATE OR REPLACE FUNCTION get_skill_matrix_legend_configuration(
    _company_id INTEGER
)
RETURNS TABLE (
    id INTEGER,
    company_id INTEGER,
    version INTEGER,
    created_at TIMESTAMP WITH TIME ZONE,
    updated_at TIMESTAMP WITH TIME ZONE,
    created_by INTEGER,
    updated_by INTEGER
) AS $$
BEGIN
    RETURN QUERY
    SELECT
        c.id,
        c.company_id,
        c.version,
        c.created_at,
        c.updated_at,
        c.created_by,
        c.updated_by
    FROM skill_matrix_legend_configuration c
    WHERE c.company_id = _company_id;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- GET: Get legend items by configuration ID and skill type
-- =============================================
CREATE OR REPLACE FUNCTION get_skill_matrix_legend_items(
    _configuration_id INTEGER,
    _skill_type VARCHAR(20)
)
RETURNS TABLE (
    id INTEGER,
    configuration_id INTEGER,
    skill_level_id INTEGER,
    skill_type VARCHAR(20),
    label VARCHAR(255),
    description VARCHAR(500),
    icon_color VARCHAR(7),
    background_color VARCHAR(7),
    sort_order INTEGER,
    score_value INTEGER,
    icon_class VARCHAR(50),
    is_default BOOLEAN,
    created_at TIMESTAMP WITH TIME ZONE,
    updated_at TIMESTAMP WITH TIME ZONE
) AS $$
BEGIN
    RETURN QUERY
    SELECT
        i.id,
        i.configuration_id,
        i.skill_level_id,
        i.skill_type,
        i.label,
        i.description,
        i.icon_color,
        i.background_color,
        i.sort_order,
        i.score_value,
        i.icon_class,
        i.is_default,
        i.created_at,
        i.updated_at
    FROM skill_matrix_legend_item i
    WHERE i.configuration_id = _configuration_id
      AND i.skill_type = _skill_type
    ORDER BY i.sort_order;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- CREATE: Insert new legend configuration
-- =============================================
CREATE OR REPLACE FUNCTION insert_skill_matrix_legend_configuration(
    _company_id INTEGER,
    _version INTEGER,
    _created_by INTEGER
)
RETURNS INTEGER AS $$
DECLARE
    v_id INTEGER;
BEGIN
    INSERT INTO skill_matrix_legend_configuration (
        company_id, version, created_at, created_by
    )
    VALUES (
        _company_id, _version, NOW(), _created_by
    )
    RETURNING id INTO v_id;

    RETURN v_id;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- CREATE: Insert legend item
-- =============================================
CREATE OR REPLACE FUNCTION insert_skill_matrix_legend_item(
    _configuration_id INTEGER,
    _skill_level_id INTEGER,
    _skill_type VARCHAR(20),
    _label VARCHAR(255),
    _description VARCHAR(500),
    _icon_color VARCHAR(7),
    _background_color VARCHAR(7),
    _sort_order INTEGER,
    _score_value INTEGER,
    _icon_class VARCHAR(50),
    _is_default BOOLEAN
)
RETURNS INTEGER AS $$
DECLARE
    v_id INTEGER;
BEGIN
    INSERT INTO skill_matrix_legend_item (
        configuration_id, skill_level_id, skill_type, label, description,
        icon_color, background_color, sort_order, score_value, icon_class,
        is_default, created_at
    )
    VALUES (
        _configuration_id, _skill_level_id, _skill_type, _label, _description,
        _icon_color, _background_color, _sort_order, _score_value, _icon_class,
        _is_default, NOW()
    )
    RETURNING id INTO v_id;

    RETURN v_id;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- UPDATE: Update legend configuration
-- =============================================
CREATE OR REPLACE FUNCTION update_skill_matrix_legend_configuration(
    _company_id INTEGER,
    _version INTEGER,
    _updated_by INTEGER
)
RETURNS BOOLEAN AS $$
BEGIN
    UPDATE skill_matrix_legend_configuration
    SET
        version = _version,
        updated_at = NOW(),
        updated_by = _updated_by
    WHERE company_id = _company_id;

    RETURN FOUND;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- UPDATE: Update single legend item
-- =============================================
CREATE OR REPLACE FUNCTION update_skill_matrix_legend_item(
    _company_id INTEGER,
    _skill_level_id INTEGER,
    _skill_type VARCHAR(20),
    _label VARCHAR(255),
    _description VARCHAR(500),
    _icon_color VARCHAR(7),
    _background_color VARCHAR(7),
    _sort_order INTEGER,
    _score_value INTEGER,
    _icon_class VARCHAR(50)
)
RETURNS BOOLEAN AS $$
BEGIN
    UPDATE skill_matrix_legend_item i
    SET
        label = _label,
        description = _description,
        icon_color = _icon_color,
        background_color = _background_color,
        sort_order = _sort_order,
        score_value = _score_value,
        icon_class = _icon_class,
        is_default = FALSE,
        updated_at = NOW()
    FROM skill_matrix_legend_configuration c
    WHERE i.configuration_id = c.id
      AND c.company_id = _company_id
      AND i.skill_level_id = _skill_level_id
      AND i.skill_type = _skill_type;

    -- Also update configuration version
    UPDATE skill_matrix_legend_configuration
    SET version = version + 1, updated_at = NOW()
    WHERE company_id = _company_id;

    RETURN FOUND;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- DELETE: Delete legend items by configuration ID
-- =============================================
CREATE OR REPLACE FUNCTION delete_skill_matrix_legend_items(
    _configuration_id INTEGER
)
RETURNS INTEGER AS $$
DECLARE
    v_deleted_count INTEGER;
BEGIN
    DELETE FROM skill_matrix_legend_item
    WHERE configuration_id = _configuration_id;

    GET DIAGNOSTICS v_deleted_count = ROW_COUNT;
    RETURN v_deleted_count;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- DELETE: Delete legend configuration by company ID
-- =============================================
CREATE OR REPLACE FUNCTION delete_skill_matrix_legend_configuration(
    _company_id INTEGER
)
RETURNS BOOLEAN AS $$
DECLARE
    v_config_id INTEGER;
BEGIN
    -- Get configuration ID
    SELECT id INTO v_config_id
    FROM skill_matrix_legend_configuration
    WHERE company_id = _company_id;

    IF v_config_id IS NOT NULL THEN
        -- Delete items first
        PERFORM delete_skill_matrix_legend_items(v_config_id);
    END IF;

    -- Then delete configuration
    DELETE FROM skill_matrix_legend_configuration
    WHERE company_id = _company_id;

    RETURN FOUND;
END;
$$ LANGUAGE plpgsql;

-- Add comments
COMMENT ON FUNCTION get_skill_matrix_legend_configuration(INTEGER) IS 'Gets legend configuration by company ID';
COMMENT ON FUNCTION get_skill_matrix_legend_items(INTEGER, VARCHAR) IS 'Gets legend items by configuration ID and skill type';
COMMENT ON FUNCTION insert_skill_matrix_legend_configuration(INTEGER, INTEGER, INTEGER) IS 'Inserts new legend configuration';
COMMENT ON FUNCTION insert_skill_matrix_legend_item(INTEGER, INTEGER, VARCHAR, VARCHAR, VARCHAR, VARCHAR, VARCHAR, INTEGER, INTEGER, VARCHAR, BOOLEAN) IS 'Inserts new legend item';
COMMENT ON FUNCTION update_skill_matrix_legend_configuration(INTEGER, INTEGER, INTEGER) IS 'Updates legend configuration';
COMMENT ON FUNCTION update_skill_matrix_legend_item(INTEGER, INTEGER, VARCHAR, VARCHAR, VARCHAR, VARCHAR, VARCHAR, INTEGER, INTEGER, VARCHAR) IS 'Updates single legend item';
COMMENT ON FUNCTION delete_skill_matrix_legend_items(INTEGER) IS 'Deletes all legend items for a configuration';
COMMENT ON FUNCTION delete_skill_matrix_legend_configuration(INTEGER) IS 'Deletes legend configuration and items for a company';
