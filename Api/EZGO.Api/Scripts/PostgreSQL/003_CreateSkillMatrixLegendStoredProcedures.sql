-- =============================================
-- Skills Matrix Legend Stored Procedures for PostgreSQL
-- All database operations are performed via stored procedures
-- Version: 1.0
-- =============================================

-- =============================================
-- GET: Get legend configuration by company ID
-- =============================================
CREATE OR REPLACE FUNCTION sp_get_skill_matrix_legend_configuration(
    p_company_id INTEGER
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
    WHERE c.company_id = p_company_id;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- GET: Get legend items by configuration ID
-- =============================================
CREATE OR REPLACE FUNCTION sp_get_skill_matrix_legend_items(
    p_configuration_id INTEGER
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
    WHERE i.configuration_id = p_configuration_id
    ORDER BY i.skill_type, i.sort_order;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- CREATE: Insert new legend configuration
-- =============================================
CREATE OR REPLACE FUNCTION sp_insert_skill_matrix_legend_configuration(
    p_company_id INTEGER,
    p_version INTEGER,
    p_created_by INTEGER
)
RETURNS INTEGER AS $$
DECLARE
    v_id INTEGER;
BEGIN
    INSERT INTO skill_matrix_legend_configuration (
        company_id, version, created_at, created_by
    )
    VALUES (
        p_company_id, p_version, NOW(), p_created_by
    )
    RETURNING id INTO v_id;

    RETURN v_id;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- CREATE: Insert legend item
-- =============================================
CREATE OR REPLACE FUNCTION sp_insert_skill_matrix_legend_item(
    p_configuration_id INTEGER,
    p_skill_level_id INTEGER,
    p_skill_type VARCHAR(20),
    p_label VARCHAR(255),
    p_description VARCHAR(500),
    p_icon_color VARCHAR(7),
    p_background_color VARCHAR(7),
    p_sort_order INTEGER,
    p_score_value INTEGER,
    p_icon_class VARCHAR(50),
    p_is_default BOOLEAN
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
        p_configuration_id, p_skill_level_id, p_skill_type, p_label, p_description,
        p_icon_color, p_background_color, p_sort_order, p_score_value, p_icon_class,
        p_is_default, NOW()
    )
    RETURNING id INTO v_id;

    RETURN v_id;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- UPDATE: Update legend configuration
-- =============================================
CREATE OR REPLACE FUNCTION sp_update_skill_matrix_legend_configuration(
    p_company_id INTEGER,
    p_version INTEGER,
    p_updated_by INTEGER
)
RETURNS BOOLEAN AS $$
BEGIN
    UPDATE skill_matrix_legend_configuration
    SET
        version = p_version,
        updated_at = NOW(),
        updated_by = p_updated_by
    WHERE company_id = p_company_id;

    RETURN FOUND;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- UPDATE: Update single legend item
-- =============================================
CREATE OR REPLACE FUNCTION sp_update_skill_matrix_legend_item(
    p_company_id INTEGER,
    p_skill_level_id INTEGER,
    p_skill_type VARCHAR(20),
    p_label VARCHAR(255),
    p_description VARCHAR(500),
    p_icon_color VARCHAR(7),
    p_background_color VARCHAR(7),
    p_sort_order INTEGER,
    p_score_value INTEGER,
    p_icon_class VARCHAR(50)
)
RETURNS BOOLEAN AS $$
BEGIN
    UPDATE skill_matrix_legend_item i
    SET
        label = p_label,
        description = p_description,
        icon_color = p_icon_color,
        background_color = p_background_color,
        sort_order = p_sort_order,
        score_value = p_score_value,
        icon_class = p_icon_class,
        is_default = FALSE,
        updated_at = NOW()
    FROM skill_matrix_legend_configuration c
    WHERE i.configuration_id = c.id
      AND c.company_id = p_company_id
      AND i.skill_level_id = p_skill_level_id
      AND i.skill_type = p_skill_type;

    -- Also update configuration version
    UPDATE skill_matrix_legend_configuration
    SET version = version + 1, updated_at = NOW()
    WHERE company_id = p_company_id;

    RETURN FOUND;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- DELETE: Delete legend items by company ID
-- =============================================
CREATE OR REPLACE FUNCTION sp_delete_skill_matrix_legend_items_by_company(
    p_company_id INTEGER
)
RETURNS INTEGER AS $$
DECLARE
    v_deleted_count INTEGER;
BEGIN
    DELETE FROM skill_matrix_legend_item
    WHERE configuration_id = (
        SELECT id FROM skill_matrix_legend_configuration WHERE company_id = p_company_id
    );

    GET DIAGNOSTICS v_deleted_count = ROW_COUNT;
    RETURN v_deleted_count;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- DELETE: Delete legend configuration by company ID
-- =============================================
CREATE OR REPLACE FUNCTION sp_delete_skill_matrix_legend_configuration(
    p_company_id INTEGER
)
RETURNS BOOLEAN AS $$
BEGIN
    -- First delete items (handled by cascade, but explicit for clarity)
    PERFORM sp_delete_skill_matrix_legend_items_by_company(p_company_id);

    -- Then delete configuration
    DELETE FROM skill_matrix_legend_configuration
    WHERE company_id = p_company_id;

    RETURN FOUND;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- CHECK: Check if configuration exists
-- =============================================
CREATE OR REPLACE FUNCTION sp_exists_skill_matrix_legend_configuration(
    p_company_id INTEGER
)
RETURNS BOOLEAN AS $$
DECLARE
    v_exists BOOLEAN;
BEGIN
    SELECT EXISTS(
        SELECT 1 FROM skill_matrix_legend_configuration WHERE company_id = p_company_id
    ) INTO v_exists;

    RETURN v_exists;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- GET: Get configuration version
-- =============================================
CREATE OR REPLACE FUNCTION sp_get_skill_matrix_legend_version(
    p_company_id INTEGER
)
RETURNS INTEGER AS $$
DECLARE
    v_version INTEGER;
BEGIN
    SELECT COALESCE(version, 0)
    INTO v_version
    FROM skill_matrix_legend_configuration
    WHERE company_id = p_company_id;

    RETURN COALESCE(v_version, 0);
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- GET: Get configuration ID by company ID
-- =============================================
CREATE OR REPLACE FUNCTION sp_get_skill_matrix_legend_configuration_id(
    p_company_id INTEGER
)
RETURNS INTEGER AS $$
DECLARE
    v_id INTEGER;
BEGIN
    SELECT id INTO v_id
    FROM skill_matrix_legend_configuration
    WHERE company_id = p_company_id;

    RETURN v_id;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- COMPOSITE: Get full configuration with items
-- Returns configuration and all items in one call
-- =============================================
CREATE OR REPLACE FUNCTION sp_get_skill_matrix_legend_full(
    p_company_id INTEGER,
    OUT config_id INTEGER,
    OUT config_company_id INTEGER,
    OUT config_version INTEGER,
    OUT config_created_at TIMESTAMP WITH TIME ZONE,
    OUT config_updated_at TIMESTAMP WITH TIME ZONE,
    OUT config_created_by INTEGER,
    OUT config_updated_by INTEGER,
    OUT items_json JSONB
)
AS $$
BEGIN
    -- Get configuration
    SELECT
        c.id, c.company_id, c.version, c.created_at, c.updated_at, c.created_by, c.updated_by
    INTO
        config_id, config_company_id, config_version, config_created_at, config_updated_at, config_created_by, config_updated_by
    FROM skill_matrix_legend_configuration c
    WHERE c.company_id = p_company_id;

    -- Get items as JSON array
    IF config_id IS NOT NULL THEN
        SELECT COALESCE(jsonb_agg(
            jsonb_build_object(
                'id', i.id,
                'configurationId', i.configuration_id,
                'skillLevelId', i.skill_level_id,
                'skillType', i.skill_type,
                'label', i.label,
                'description', i.description,
                'iconColor', i.icon_color,
                'backgroundColor', i.background_color,
                'order', i.sort_order,
                'scoreValue', i.score_value,
                'iconClass', i.icon_class,
                'isDefault', i.is_default,
                'createdAt', i.created_at,
                'updatedAt', i.updated_at
            ) ORDER BY i.skill_type, i.sort_order
        ), '[]'::jsonb)
        INTO items_json
        FROM skill_matrix_legend_item i
        WHERE i.configuration_id = config_id;
    ELSE
        items_json := '[]'::jsonb;
    END IF;
END;
$$ LANGUAGE plpgsql;

-- Add comments
COMMENT ON FUNCTION sp_get_skill_matrix_legend_configuration(INTEGER) IS 'Gets legend configuration by company ID';
COMMENT ON FUNCTION sp_get_skill_matrix_legend_items(INTEGER) IS 'Gets legend items by configuration ID';
COMMENT ON FUNCTION sp_insert_skill_matrix_legend_configuration(INTEGER, INTEGER, INTEGER) IS 'Inserts new legend configuration';
COMMENT ON FUNCTION sp_insert_skill_matrix_legend_item(INTEGER, INTEGER, VARCHAR, VARCHAR, VARCHAR, VARCHAR, VARCHAR, INTEGER, INTEGER, VARCHAR, BOOLEAN) IS 'Inserts new legend item';
COMMENT ON FUNCTION sp_update_skill_matrix_legend_configuration(INTEGER, INTEGER, INTEGER) IS 'Updates legend configuration';
COMMENT ON FUNCTION sp_update_skill_matrix_legend_item(INTEGER, INTEGER, VARCHAR, VARCHAR, VARCHAR, VARCHAR, VARCHAR, INTEGER, INTEGER, VARCHAR) IS 'Updates single legend item';
COMMENT ON FUNCTION sp_delete_skill_matrix_legend_items_by_company(INTEGER) IS 'Deletes all legend items for a company';
COMMENT ON FUNCTION sp_delete_skill_matrix_legend_configuration(INTEGER) IS 'Deletes legend configuration and items for a company';
COMMENT ON FUNCTION sp_exists_skill_matrix_legend_configuration(INTEGER) IS 'Checks if configuration exists for company';
COMMENT ON FUNCTION sp_get_skill_matrix_legend_version(INTEGER) IS 'Gets current version of configuration';
COMMENT ON FUNCTION sp_get_skill_matrix_legend_configuration_id(INTEGER) IS 'Gets configuration ID by company ID';
COMMENT ON FUNCTION sp_get_skill_matrix_legend_full(INTEGER, OUT INTEGER, OUT INTEGER, OUT INTEGER, OUT TIMESTAMP WITH TIME ZONE, OUT TIMESTAMP WITH TIME ZONE, OUT INTEGER, OUT INTEGER, OUT JSONB) IS 'Gets full configuration with items as JSON';
