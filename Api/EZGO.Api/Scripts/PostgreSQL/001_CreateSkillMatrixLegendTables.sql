-- Matrix Legend Table and Stored Procedures
-- Migration: Rename skill_matrix_legend_item to matrix_legend_item

-- ============================================================================
-- CLEANUP: Drop old objects if they exist
-- ============================================================================

-- Drop old stored procedures
DROP FUNCTION IF EXISTS public.get_skill_matrix_legend_items(int4, varchar);
DROP FUNCTION IF EXISTS public.check_skill_matrix_legend_exists(int4);
DROP FUNCTION IF EXISTS public.insert_skill_matrix_legend_item(int4, int4, varchar, varchar, varchar, varchar, varchar, int4, int4, varchar, bool, int4);
DROP FUNCTION IF EXISTS public.update_skill_matrix_legend_item(int4, int4, varchar, varchar, varchar, varchar, varchar, int4, int4, varchar, int4);

-- Drop old table (this will also drop associated indexes and constraints)
DROP TABLE IF EXISTS skill_matrix_legend_item CASCADE;

-- ============================================================================
-- CREATE TABLE: matrix_legend_item
-- ============================================================================

CREATE TABLE IF NOT EXISTS matrix_legend_item (
    id                  SERIAL PRIMARY KEY,
    company_id          INT NOT NULL,
    skill_level_id      INT NOT NULL,
    skill_type          VARCHAR(20) NOT NULL,
    label               VARCHAR(255),
    description         VARCHAR(500),
    icon_color          VARCHAR(7),
    background_color    VARCHAR(7),
    sort_order          INT NOT NULL,
    score_value         INT,
    icon_class          VARCHAR(50),
    is_default          BOOLEAN NOT NULL DEFAULT FALSE,
    created_at          TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at          TIMESTAMPTZ,
    created_by          INT,
    updated_by          INT,

    -- Foreign key constraints
    CONSTRAINT fk_matrix_legend_item_company
        FOREIGN KEY (company_id)
        REFERENCES companies_company(id)
        ON DELETE CASCADE,

    CONSTRAINT fk_matrix_legend_item_created_by
        FOREIGN KEY (created_by)
        REFERENCES profiles_user(id)
        ON DELETE SET NULL,

    CONSTRAINT fk_matrix_legend_item_updated_by
        FOREIGN KEY (updated_by)
        REFERENCES profiles_user(id)
        ON DELETE SET NULL,

    -- Unique constraint for company/skill_level_id/skill_type combination
    CONSTRAINT uq_matrix_legend_item_company_level_type
        UNIQUE (company_id, skill_level_id, skill_type)
);

-- ============================================================================
-- INDEXES
-- ============================================================================

CREATE INDEX IF NOT EXISTS idx_matrix_legend_item_company_id
    ON matrix_legend_item (company_id);

CREATE INDEX IF NOT EXISTS idx_matrix_legend_item_company_skill_type
    ON matrix_legend_item (company_id, skill_type);

CREATE INDEX IF NOT EXISTS idx_matrix_legend_item_sort_order
    ON matrix_legend_item (company_id, skill_type, sort_order);

-- ============================================================================
-- STORED PROCEDURES
-- ============================================================================

-- Get legend items by company and skill type
CREATE OR REPLACE FUNCTION public.get_matrix_legend_items(
    _company_id INT,
    _skill_type VARCHAR(20)
)
RETURNS TABLE (
    id                  INT,
    company_id          INT,
    skill_level_id      INT,
    skill_type          VARCHAR(20),
    label               VARCHAR(255),
    description         VARCHAR(500),
    icon_color          VARCHAR(7),
    background_color    VARCHAR(7),
    sort_order          INT,
    score_value         INT,
    icon_class          VARCHAR(50),
    is_default          BOOLEAN,
    created_at          TIMESTAMPTZ,
    updated_at          TIMESTAMPTZ,
    created_by          INT,
    updated_by          INT
)
LANGUAGE plpgsql
STABLE
AS $$
BEGIN
    RETURN QUERY
    SELECT
        i.id,
        i.company_id,
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
        i.updated_at,
        i.created_by,
        i.updated_by
    FROM matrix_legend_item i
    WHERE i.company_id = _company_id
      AND i.skill_type = _skill_type
    ORDER BY i.sort_order;
END;
$$;

-- Check if legend exists for company
CREATE OR REPLACE FUNCTION public.check_matrix_legend_exists(
    _company_id INT
)
RETURNS BOOLEAN
LANGUAGE plpgsql
STABLE
AS $$
BEGIN
    RETURN EXISTS (
        SELECT 1
        FROM matrix_legend_item
        WHERE company_id = _company_id
    );
END;
$$;

-- Insert new legend item
CREATE OR REPLACE FUNCTION public.insert_matrix_legend_item(
    _company_id         INT,
    _skill_level_id     INT,
    _skill_type         VARCHAR(20),
    _label              VARCHAR(255),
    _description        VARCHAR(500),
    _icon_color         VARCHAR(7),
    _background_color   VARCHAR(7),
    _sort_order         INT,
    _score_value        INT,
    _icon_class         VARCHAR(50),
    _is_default         BOOLEAN,
    _created_by         INT
)
RETURNS INT
LANGUAGE plpgsql
AS $$
DECLARE
    v_id INT;
BEGIN
    INSERT INTO matrix_legend_item (
        company_id,
        skill_level_id,
        skill_type,
        label,
        description,
        icon_color,
        background_color,
        sort_order,
        score_value,
        icon_class,
        is_default,
        created_at,
        created_by
    )
    VALUES (
        _company_id,
        _skill_level_id,
        _skill_type,
        _label,
        _description,
        _icon_color,
        _background_color,
        _sort_order,
        _score_value,
        _icon_class,
        _is_default,
        NOW(),
        _created_by
    )
    RETURNING id INTO v_id;

    RETURN v_id;
END;
$$;

-- Update existing legend item
CREATE OR REPLACE FUNCTION public.update_matrix_legend_item(
    _company_id         INT,
    _skill_level_id     INT,
    _skill_type         VARCHAR(20),
    _label              VARCHAR(255),
    _description        VARCHAR(500),
    _icon_color         VARCHAR(7),
    _background_color   VARCHAR(7),
    _sort_order         INT,
    _score_value        INT,
    _icon_class         VARCHAR(50),
    _updated_by         INT
)
RETURNS BOOLEAN
LANGUAGE plpgsql
AS $$
BEGIN
    UPDATE matrix_legend_item
    SET
        label = _label,
        description = _description,
        icon_color = _icon_color,
        background_color = _background_color,
        sort_order = _sort_order,
        score_value = _score_value,
        icon_class = _icon_class,
        is_default = FALSE,
        updated_at = NOW(),
        updated_by = _updated_by
    WHERE company_id = _company_id
      AND skill_level_id = _skill_level_id
      AND skill_type = _skill_type;

    RETURN FOUND;
END;
$$;
