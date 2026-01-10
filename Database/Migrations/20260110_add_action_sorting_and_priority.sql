-- ============================================
-- Migration: Add Action Sorting and Priority
-- Date: 2026-01-10
-- Description:
--   1. Adds priority column to actions_action table
--   2. Updates existing get_actions_v3 stored procedure to add sorting capabilities
-- ============================================

-- ============================================
-- STEP 1: Add priority column to actions_action table
-- ============================================
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = 'public'
          AND table_name = 'actions_action'
          AND column_name = 'priority'
    ) THEN
        -- Add priority column with default value 3 (Normal)
        ALTER TABLE public.actions_action
        ADD COLUMN priority integer DEFAULT 3;

        -- Add comment describing the priority values
        COMMENT ON COLUMN public.actions_action.priority
        IS 'Action priority: 1=Critical, 2=High/Important, 3=Normal, 4=Low';

        -- Create index for efficient sorting
        CREATE INDEX idx_actions_action_priority ON public.actions_action(priority);

        RAISE NOTICE 'Priority column added to actions_action table with default value 3 (Normal)';
    ELSE
        RAISE NOTICE 'Priority column already exists in actions_action table';
    END IF;
END $$;

-- ============================================
-- STEP 2: Update get_actions_v3 stored procedure
-- ============================================
-- CHANGES MADE TO get_actions_v3:
--   1. Added _sortby parameter (text, default: 'duedate') - determines which column to sort by
--   2. Added _sortdirection parameter (text, default: 'asc') - determines sort direction (asc/desc)
--   3. Added 'priority' column to RETURNS TABLE definition
--   4. Added AA.priority to SELECT statement
--   5. Replaced fixed ORDER BY clause with dynamic sorting logic supporting:
--      - id: Sort by action ID
--      - name/description: Sort by action description
--      - duedate: Sort by due date (default)
--      - startdate: Sort by created_at date
--      - modifiedat/modificationdate: Sort by modified_at timestamp
--      - areaname/area: Sort by assigned area name
--      - username/user: Sort by assigned user name
--      - lastcommentdate/recentchat: Sort by most recent comment date
--      - priority: Sort by priority level (1=Critical, 2=High, 3=Normal, 4=Low)
--   6. Backwards compatible with old parameter names (recentchat→lastcommentdate, user→username, etc.)
--   7. All NULL values sorted to end (NULLS LAST) for consistent behavior
--   8. Maintains secondary sort by is_resolved, due_date, modified_at for consistent ordering
-- ============================================

CREATE OR REPLACE FUNCTION public.get_actions_v3(
    _companyid integer,
    _filtertext character varying DEFAULT NULL::character varying,
    _isresolved boolean DEFAULT NULL::boolean,
    _isoverdue boolean DEFAULT NULL::boolean,
    _isunresolved boolean DEFAULT NULL::boolean,
    _hasunviewedcomments boolean DEFAULT NULL::boolean,
    _userid integer DEFAULT NULL::integer,
    _assignedareaid integer DEFAULT NULL::integer,
    _assignedareaids integer[] DEFAULT NULL::integer[],
    _assignedtomeuserid integer DEFAULT NULL::integer,
    _assigneduserid integer DEFAULT NULL::integer,
    _assigneduserids integer[] DEFAULT NULL::integer[],
    _timestamp timestamp without time zone DEFAULT NULL::timestamp without time zone,
    _createdfrom timestamp without time zone DEFAULT NULL::timestamp without time zone,
    _createdto timestamp without time zone DEFAULT NULL::timestamp without time zone,
    _overduefrom timestamp without time zone DEFAULT NULL::timestamp without time zone,
    _overdueto timestamp without time zone DEFAULT NULL::timestamp without time zone,
    _resolvedfrom timestamp without time zone DEFAULT NULL::timestamp without time zone,
    _resolvedto timestamp without time zone DEFAULT NULL::timestamp without time zone,
    _createdbyid integer DEFAULT NULL::integer,
    _createdbyorassignedto integer DEFAULT NULL::integer,
    _taskid integer DEFAULT NULL::integer,
    _tasktemplateid integer DEFAULT NULL::integer,
    _checklistid integer DEFAULT NULL::integer,
    _checklisttemplateid integer DEFAULT NULL::integer,
    _auditid integer DEFAULT NULL::integer,
    _audittemplateid integer DEFAULT NULL::integer,
    _parentareaid integer DEFAULT NULL::integer,
    _resolvedcutoffdate timestamp without time zone DEFAULT NULL::timestamp without time zone,
    _tagids integer[] DEFAULT NULL::integer[],
    _limit integer DEFAULT 0,
    _offset integer DEFAULT 0,
    _sortby text DEFAULT 'duedate'::text,
    _sortdirection text DEFAULT 'asc'::text
)
RETURNS TABLE(
    comment text,
    company_id integer,
    created_at timestamp with time zone,
    created_by_id integer,
    description text,
    due_date date,
    id integer,
    image_0 character varying,
    image_1 character varying,
    image_2 character varying,
    image_3 character varying,
    image_4 character varying,
    image_5 character varying,
    is_resolved boolean,
    modified_at timestamp with time zone,
    resolved_at timestamp with time zone,
    task_id integer,
    task_template_id integer,
    video_0 character varying,
    video_1 character varying,
    video_2 character varying,
    video_3 character varying,
    video_4 character varying,
    video_5 character varying,
    video_thumbnail_0 character varying,
    video_thumbnail_1 character varying,
    video_thumbnail_2 character varying,
    video_thumbnail_3 character varying,
    video_thumbnail_4 character varying,
    video_thumbnail_5 character varying,
    commentnr integer,
    unviewedcommentnr integer,
    createdby text,
    lastcommentdate timestamp with time zone,
    priority integer
)
LANGUAGE plpgsql
STABLE
AS $function$
BEGIN
    RETURN QUERY
    SELECT
        AA.comment,
        AA.company_id,
        AA.created_at,
        AA.created_by_id,
        AA.description,
        AA.due_date,
        AA.id,
        AA.image_0,
        AA.image_1,
        AA.image_2,
        AA.image_3,
        AA.image_4,
        AA.image_5,
        AA.is_resolved,
        AA.modified_at,
        AA.resolved_at,
        AA.task_id,
        AA.task_template_id,
        AA.video_0,
        AA.video_1,
        AA.video_2,
        AA.video_3,
        AA.video_4,
        AA.video_5,
        AA.video_thumbnail_0,
        AA.video_thumbnail_1,
        AA.video_thumbnail_2,
        AA.video_thumbnail_3,
        AA.video_thumbnail_4,
        AA.video_thumbnail_5,
        (SELECT Count(AAC.id)::int4 FROM actions_actioncomment AAC WHERE AAC.action_id = AA.id AND AAC.is_active = true) AS commentnr,
        (SELECT (T.comment_count - T.comment_viewed_count)::int4 FROM
            (SELECT COUNT(AAC.id) AS comment_count,COUNT(AACV.comment_id) AS comment_viewed_count FROM actions_actioncomment AAC
                LEFT JOIN actions_action_assigned_users AAU ON AAU.action_id = AA."id" AND AAU.user_id = _userid
                LEFT JOIN actions_actioncommentviewed AACV ON AACV.comment_id = AAC."id" AND AACV.user_id = _userid
                WHERE AAC.action_id = AA."id" AND AAC.user_id <> _userid AND AA.is_active = true AND AAC.is_active = true) AS T) AS unviewedcommentnr,
        CONCAT(PU.first_name, ' ', PU.last_name) AS createdby,
        (SELECT AAC.modified_at::timestamptz FROM actions_actioncomment AAC WHERE AAC.action_id = AA.id ORDER BY modified_at DESC LIMIT 1) AS lastcommentdate,
        AA.priority
    FROM
        actions_action AA
        INNER JOIN profiles_user PU ON PU.id = AA.created_by_id AND PU.company_id = _companyid
        LEFT JOIN actions_action_assigned_areas AAA ON AAA.action_id = AA.id AND (AAA.area_id = _assignedareaid)
        LEFT JOIN actions_action_assigned_users AAU ON AAU.action_id = AA.id AND (AAU.user_id = _assigneduserid)
        LEFT JOIN actions_action_assigned_users AAUCA ON AAUCA.action_id = AA.id AND (AAUCA.user_id = _createdbyorassignedto)
    WHERE
        AA.company_id = _companyid AND AA.is_active = true

        -- filter text
        AND (_filtertext IS NULL OR POSITION(LOWER(_filtertext) IN LOWER(CONCAT(AA."id", ' ', AA.comment, ' ', AA.description))) > 0)

        -- filter isresolved
        -- filter isoverdue
        -- filter isunresolved
        AND ((_isresolved IS NULL AND _isoverdue IS NULL AND _isunresolved IS NULL)
        OR (_isresolved IS NOT NULL AND AA.is_resolved = _isresolved)
        OR (_isoverdue IS NOT NULL AND now()::date > AA.due_date AND AA.is_resolved = false)
        OR (_isunresolved IS NOT NULL AND now()::date <= AA.due_date AND AA.is_resolved = false))

        -- filter assignedareaid
        AND (_assignedareaid IS NULL OR AAA.area_id = _assignedareaid)
        -- filter assignedareaids
        AND (_assignedareaids IS NULL
            OR (AA."id" IN (
                SELECT DISTINCT AAAF.action_id
                FROM actions_action_assigned_areas AAAF
                WHERE AAAF.area_id = ANY(_assignedareaids)
            )
        ))
        -- filter 'assigned to me'
        AND (_assignedtomeuserid IS NULL
            OR AA."id" IN (SELECT AAA.action_id FROM actions_action_assigned_users AAA WHERE AAA.user_id = _assignedtomeuserid)
            OR (AA."id" IN (
                SELECT DISTINCT AAAF.action_id
                FROM actions_action_assigned_areas AAAF
                WHERE AAAF.area_id IN (SELECT area_id FROM get_allowedareaids_by_user(_companyid, _assignedtomeuserid))
                )
            )
        )
        -- filter assigneduserid
        AND (_assigneduserid IS NULL OR AAU.user_id = _assigneduserid)
        -- filter assigneduserids
        AND (_assigneduserids IS NULL
            OR (AA."id" IN (
                SELECT DISTINCT AAUF.action_id
                FROM actions_action_assigned_users AAUF
                WHERE AAUF.user_id = ANY(_assigneduserids)
            )
        ))

        -- filter timestamp
        AND (_timestamp IS NULL OR AA.created_at < _timestamp)

        -- filter createdfrom and createdto
        AND ((_createdfrom IS NULL OR _createdto IS NULL) OR (DATE(AA.created_at) >= _createdfrom AND DATE(AA.created_at) <= _createdto))

        -- filter overduefrom and overdueto
        AND ((_overduefrom IS NULL OR _overdueto IS NULL) OR (DATE(AA.due_date) >= _overduefrom AND DATE(AA.due_date) <= _overdueto))

        -- filter resolvedfrom and resolvedto
        AND ((_resolvedfrom IS NULL OR _resolvedto IS NULL) OR (AA.resolved_at IS NOT NULL AND DATE(AA.resolved_at) >= _resolvedfrom AND DATE(AA.resolved_at) <= _resolvedto))

        -- resolved actions cut off date
        AND (_resolvedcutoffdate IS NULL OR AA.is_resolved = FALSE OR AA.resolved_at::date >= _resolvedcutoffdate)

        -- filter createdbyid
        AND (_createdbyid IS NULL OR AA.created_by_id = _createdbyid)

        -- filter createdbyorassignedto
        AND (_createdbyorassignedto IS NULL OR
                AA.created_by_id = _createdbyorassignedto
                OR AAUCA.user_id = _createdbyorassignedto
                OR (AA."id" IN (
                    SELECT DISTINCT AAAF.action_id
                    FROM actions_action_assigned_areas AAAF
                    WHERE AAAF.area_id IN (SELECT area_id FROM get_allowedareaids_by_user(_companyid, _createdbyorassignedto))
                    )
                )
            )

        -- filter taskid
        -- filter tasktemplateid
        AND CASE WHEN (_taskid IS NOT NULL AND _tasktemplateid IS NOT NULL) THEN
            AA.task_id = _taskid OR AA.task_template_id = _tasktemplateid
        ELSE
            (_taskid IS NULL OR AA.task_id = _taskid)
            AND (_tasktemplateid IS NULL OR AA.task_template_id = _tasktemplateid)
        END

        -- filter checklistid
        AND (_checklistid IS NULL
            OR AA."task_id" IN (
                SELECT DISTINCT TT."id"
                FROM checklists_checklist CC
                INNER JOIN checklists_checklist_tasks CCT ON CCT.checklist_id = _checklistid
                INNER JOIN tasks_task TT ON TT.id = CCT.task_id
            ))--filter on checklistid end

        --filter checklisttemplateid
        AND (_checklisttemplateid IS NULL
            OR AA."task_template_id" IN (
                SELECT DISTINCT TTT."id"
                FROM checklists_checklisttemplate CCT
                INNER JOIN checklists_checklisttemplate_tasks CCTT ON CCTT.checklisttemplate_id = _checklisttemplateid
                INNER JOIN tasks_tasktemplate TTT ON TTT.id = CCTT.tasktemplate_id
            ))

        -- filter auditid
        AND (_auditid IS NULL
            OR AA."task_id" IN (
                SELECT DISTINCT TT."id"
                FROM audits_audit AA
                INNER JOIN audits_audit_tasks AAT ON AAT.audit_id = _auditid
                INNER JOIN tasks_task TT ON TT.id = AAT.task_id
            ))--filter on auditid end

        --filter audittemplateid
        AND (_audittemplateid IS NULL
            OR AA."task_template_id" IN (
                SELECT DISTINCT TTT."id"
                FROM audits_audittemplate AAT
                INNER JOIN audits_audittemplate_tasks AATT ON AATT.audittemplate_id = _audittemplateid
                INNER JOIN tasks_tasktemplate TTT ON TTT.id = AATT.tasktemplate_id
            ))

        --filter parentareaid
        AND (_parentareaid IS NULL
            OR ((SELECT DISTINCT AAA1.area_id FROM actions_action_area AAA1 WHERE AAA1.action_id = AA."id" LIMIT 1) IN (SELECT DISTINCT areas.id FROM get_area_nodes_from_root_to_leaf(_companyid, _parentareaid) areas))
            )

        --filter unviewed comments user id
        AND (_hasunviewedcomments IS NULL OR
            (SELECT (T.comment_count - T.comment_viewed_count)::int4 FROM
                (SELECT COUNT(AAC.id) AS comment_count, COUNT(AACV.comment_id) AS comment_viewed_count FROM actions_actioncomment AAC
                LEFT JOIN actions_action_assigned_users AAU ON AAU.action_id = AA."id" AND AAU.user_id = _userid
                LEFT JOIN actions_actioncommentviewed AACV ON AACV.comment_id = AAC."id" AND AACV.user_id = _userid
                WHERE AAC.action_id = AA."id" AND AAC.user_id <> _userid AND AA.is_active = true AND AAC.is_active = true) AS T
            ) > 0
        )

        -- filter tagids
        AND (_tagids IS NULL --filter on tags start
            OR (AA."id" IN (
                SELECT DISTINCT TTR.action_id
                FROM tags_tag_relation TTR
                WHERE TTR.action_id IS NOT NULL AND TTR.tag_id = ANY(_tagids)
            )
        )) --filter on tags end
    ORDER BY
        -- Priority: 1=Critical, 2=High, 3=Normal, 4=Low (lower number = higher priority)
        CASE WHEN LOWER(_sortby) = 'priority' AND LOWER(_sortdirection) = 'desc' THEN AA.priority END DESC NULLS LAST,
        CASE WHEN LOWER(_sortby) = 'priority' AND LOWER(_sortdirection) = 'asc' THEN AA.priority END ASC NULLS LAST,

        -- ID sorting
        CASE WHEN LOWER(_sortby) = 'id' AND LOWER(_sortdirection) = 'desc' THEN AA.id END DESC NULLS LAST,
        CASE WHEN LOWER(_sortby) = 'id' AND LOWER(_sortdirection) = 'asc' THEN AA.id END ASC NULLS LAST,

        -- Name/Description sorting
        CASE WHEN LOWER(_sortby) IN ('name', 'description') AND LOWER(_sortdirection) = 'desc' THEN AA.description END DESC NULLS LAST,
        CASE WHEN LOWER(_sortby) IN ('name', 'description') AND LOWER(_sortdirection) = 'asc' THEN AA.description END ASC NULLS LAST,

        -- Start Date (created_at) sorting
        CASE WHEN LOWER(_sortby) = 'startdate' AND LOWER(_sortdirection) = 'desc' THEN AA.created_at END DESC NULLS LAST,
        CASE WHEN LOWER(_sortby) = 'startdate' AND LOWER(_sortdirection) = 'asc' THEN AA.created_at END ASC NULLS LAST,

        -- Modified At sorting (support both 'modifiedat' and 'modificationdate')
        CASE WHEN LOWER(_sortby) IN ('modifiedat', 'modificationdate') AND LOWER(_sortdirection) = 'desc' THEN AA.modified_at END DESC NULLS LAST,
        CASE WHEN LOWER(_sortby) IN ('modifiedat', 'modificationdate') AND LOWER(_sortdirection) = 'asc' THEN AA.modified_at END ASC NULLS LAST,

        -- Area Name sorting (support both 'areaname' and 'area')
        CASE WHEN LOWER(_sortby) IN ('areaname', 'area') AND LOWER(_sortdirection) = 'desc' THEN
            (SELECT AR.name FROM actions_action_assigned_areas AAAA
             INNER JOIN areas_area AR ON AR.id = AAAA.area_id
             WHERE AAAA.action_id = AA.id LIMIT 1) END DESC NULLS LAST,
        CASE WHEN LOWER(_sortby) IN ('areaname', 'area') AND LOWER(_sortdirection) = 'asc' THEN
            (SELECT AR.name FROM actions_action_assigned_areas AAAA
             INNER JOIN areas_area AR ON AR.id = AAAA.area_id
             WHERE AAAA.action_id = AA.id LIMIT 1) END ASC NULLS LAST,

        -- User Name sorting (support both 'username' and 'user')
        CASE WHEN LOWER(_sortby) IN ('username', 'user') AND LOWER(_sortdirection) = 'desc' THEN
            (SELECT CONCAT(PUF.first_name, ' ', PUF.last_name) FROM actions_action_assigned_users AAAU
             INNER JOIN profiles_user PUF ON PUF.id = AAAU.user_id
             WHERE AAAU.action_id = AA.id LIMIT 1) END DESC NULLS LAST,
        CASE WHEN LOWER(_sortby) IN ('username', 'user') AND LOWER(_sortdirection) = 'asc' THEN
            (SELECT CONCAT(PUF.first_name, ' ', PUF.last_name) FROM actions_action_assigned_users AAAU
             INNER JOIN profiles_user PUF ON PUF.id = AAAU.user_id
             WHERE AAAU.action_id = AA.id LIMIT 1) END ASC NULLS LAST,

        -- Last Comment Date sorting (support both 'lastcommentdate' and 'recentchat')
        CASE WHEN LOWER(_sortby) IN ('lastcommentdate', 'recentchat') AND LOWER(_sortdirection) = 'desc' THEN
            (SELECT AAC.modified_at FROM actions_actioncomment AAC
             WHERE AAC.action_id = AA.id ORDER BY modified_at DESC LIMIT 1) END DESC NULLS LAST,
        CASE WHEN LOWER(_sortby) IN ('lastcommentdate', 'recentchat') AND LOWER(_sortdirection) = 'asc' THEN
            (SELECT AAC.modified_at FROM actions_actioncomment AAC
             WHERE AAC.action_id = AA.id ORDER BY modified_at DESC LIMIT 1) END ASC NULLS LAST,

        -- Due Date sorting (default when _sortby is NULL or 'duedate')
        CASE WHEN (LOWER(_sortby) = 'duedate' OR _sortby IS NULL) AND LOWER(_sortdirection) = 'desc' THEN AA.due_date END DESC NULLS LAST,
        CASE WHEN (LOWER(_sortby) = 'duedate' OR _sortby IS NULL) AND LOWER(_sortdirection) = 'asc' THEN AA.due_date END ASC NULLS LAST,

        -- Secondary sort for consistent ordering
        AA.is_resolved ASC,
        AA.due_date ASC NULLS LAST,
        AA.modified_at DESC

    LIMIT CASE WHEN (_limit > 0) THEN _limit END
    OFFSET CASE WHEN (_offset > 0) THEN _offset END;
END$function$;

-- ============================================
-- STEP 3: Add comments and documentation
-- ============================================
COMMENT ON FUNCTION public.get_actions_v3 IS 'Retrieves actions with comprehensive filtering and sorting capabilities. Supports sorting by: id, name, duedate, startdate, modifiedat, areaname, username, lastcommentdate, priority. Backwards compatible with old sort parameter names (recentchat, user, modificationdate, area). Added priority column and dynamic sorting in v3.1.';

-- ============================================
-- STEP 4: Verification queries
-- ============================================
DO $$
DECLARE
    priority_exists boolean;
    procedure_exists boolean;
BEGIN
    -- Check if priority column exists
    SELECT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = 'public'
          AND table_name = 'actions_action'
          AND column_name = 'priority'
    ) INTO priority_exists;

    -- Check if get_actions_v3 stored procedure exists
    SELECT EXISTS (
        SELECT 1 FROM information_schema.routines
        WHERE routine_schema = 'public'
          AND routine_name = 'get_actions_v3'
    ) INTO procedure_exists;

    -- Report results
    IF priority_exists THEN
        RAISE NOTICE '✓ Priority column exists in actions_action table';
    ELSE
        RAISE WARNING '✗ Priority column NOT found in actions_action table';
    END IF;

    IF procedure_exists THEN
        RAISE NOTICE '✓ Stored procedure get_actions_v3 updated successfully';
    ELSE
        RAISE WARNING '✗ Stored procedure get_actions_v3 NOT found';
    END IF;

    RAISE NOTICE '============================================';
    RAISE NOTICE 'Migration completed';
    RAISE NOTICE '============================================';
END $$;

-- ============================================
-- STEP 5: Grant permissions (update with your user)
-- ============================================
-- GRANT EXECUTE ON FUNCTION public.get_actions_v3 TO your_app_user;

-- ============================================
-- END OF MIGRATION
-- ============================================
