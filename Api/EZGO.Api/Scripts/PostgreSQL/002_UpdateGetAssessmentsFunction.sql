-- DROP FUNCTION public.get_assessments(int4, bool, bool, int4, int4, int4, int4, int4, int4, timestamp, timestamp, timestamp, int4, _int4, _int4, int4, varchar, int4, int4, bool);

CREATE OR REPLACE FUNCTION public.get_assessments(
    _companyid integer,
    _sortbymodifiedat boolean DEFAULT NULL::boolean,
    _iscompleted boolean DEFAULT NULL::boolean,
    _assessmenttype integer DEFAULT NULL::integer,
    _completedforid integer DEFAULT NULL::integer,
    _assessorid integer DEFAULT NULL::integer,
    _role integer DEFAULT NULL::integer,
    _areaid integer DEFAULT 0,
    _templateid integer DEFAULT NULL::integer,
    _timestamp timestamp without time zone DEFAULT NULL::timestamp without time zone,
    _starttimestamp timestamp without time zone DEFAULT NULL::timestamp without time zone,
    _endtimestamp timestamp without time zone DEFAULT NULL::timestamp without time zone,
    _timespanindays integer DEFAULT 0,
    _tagids integer[] DEFAULT NULL::integer[],
    _assessorids integer[] DEFAULT NULL::integer[],
    _userid integer DEFAULT 0,
    _filtertext character varying DEFAULT NULL::character varying,
    _limit integer DEFAULT 0,
    _offset integer DEFAULT 0,
    _sortbycompletedat boolean DEFAULT NULL::boolean  -- NEW PARAMETER: Sort by completion date DESC when true
)
 RETURNS TABLE(id integer, signatures character varying, completed_for_id integer, completed_at timestamp without time zone, completedfor character varying, completedfor_picture character varying, assessor_id integer, assessor character varying, assessor_picture character varying, is_completed boolean, assessment_template_id integer, company_id integer, area_id integer, role integer, assessment_type integer, name character varying, description text, media text, signature_type integer, signature_required boolean, created_at timestamp without time zone, modified_at timestamp without time zone, created_by_id integer, modified_by_id integer, created_by character varying, modified_by character varying, total_score integer, nr_of_skillinstructions integer, version character varying, start_at timestamp without time zone, end_at timestamp without time zone, assessors text)
 LANGUAGE plpgsql
 STABLE
AS $function$BEGIN
	RETURN QUERY
SELECT
	A.id,
	A.signatures,
	A.completed_for_id,
	A.completed_at::timestamp,
	CONCAT(PUF.first_name, ' ', PUF.last_name)::varchar AS completedfor,
	PUF.picture AS completedfor_picture,
	A.assessor_id,
	CONCAT(PUA.first_name, ' ', PUA.last_name)::varchar AS assessor,
	PUA.picture AS assessor_picture,
	A.is_completed,
	AST.id AS assessment_template_id,
	AST.company_id,
	AST.area_id,
	AST.role,
	AST.assessment_type,
	AST.name,
	AST.description,
	AST.media,
	AST.signature_type,
	AST.signature_required,
	A.created_at,
	A.modified_at,
	A.created_by_id,
	A.modified_by_id,
	CONCAT(PUC.first_name, ' ', PUC.last_name)::varchar AS created_by,
	CONCAT(PUM.first_name, ' ', PUM.last_name)::varchar AS modified_by,
	A.total_score,
	(SELECT (COUNT(ASI.id))::int4 FROM assessment_skillinstructions ASI WHERE ASI.assessment_id = A.id AND ASI.company_id = _companyid AND ASI.is_active = true) AS nr_of_skillinstructions,
	A."version",
	(
      SELECT MIN(ASI.start_at)::timestamp
      FROM assessment_skillinstructions ASI
      WHERE ASI.assessment_id = A.id
        AND ASI.company_id   = _companyid
        AND ASI.is_active    = true
    ) AS start_at,
	(
		SELECT MAX(COALESCE(ASII2.scored_at, ASII2.completed_at))
		FROM assessment_skillinstruction_items ASII2
		WHERE
			NOT EXISTS
			(
				SELECT *
				FROM assessment_skillinstruction_items ASII3
				WHERE ASII3.is_completed = false
				  AND ASII3.assessment_id = A.id
				  AND ASII3.is_active = true
				  AND ASII3.company_id = A.company_id
			)
			AND ASII2.is_completed = true
			AND ASII2.assessment_id = A.id
			AND ASII2.is_active = true
			AND ASII2.company_id = A.company_id
	)::timestamp AS end_at,

	COALESCE((
	      SELECT json_agg(
	               json_build_object(
	                 'Id',      uu.id,
	                 'Name',    TRIM(BOTH FROM COALESCE(
	                               NULLIF(CONCAT(uu.first_name,' ',uu.last_name),' '),
	                               NULLIF(uu.username,''),
	                               'User '||uu.id::text
	                             )),
	                 'Picture', NULLIF(uu.picture,'')
	               )
	               ORDER BY uu.id
	             )
	      FROM (
	        SELECT DISTINCT u.id, u.first_name, u.last_name, u.username, u.picture
	          FROM assessment_skillinstruction_items asii
	          JOIN profiles_user u
	            ON u.id = asii.assessor_id
	           AND u.company_id = _companyid
	         WHERE asii.company_id   = _companyid
	           AND asii.assessment_id= A.id
	           AND asii.assessor_id IS NOT NULL
	      ) uu
	    )::text, '[]') AS assessors
FROM assessments A
	INNER JOIN assessment_templates AST ON AST.id = A.assessment_template_id
	INNER JOIN profiles_user PUC ON PUC.id = A.created_by_id AND PUC.company_id = _companyid
	INNER JOIN profiles_user PUM ON PUM.id = A.modified_by_id AND PUM.company_id = _companyid
	LEFT JOIN profiles_user PUF ON PUF.id = A.completed_for_id AND PUF.company_id = _companyid
	LEFT JOIN profiles_user PUA ON PUA.id = A.assessor_id AND PUA.company_id = _companyid
WHERE AST.company_id = _companyid  AND A.is_active = true AND A.company_id = _companyid
	AND (EXISTS (SELECT areas.id FROM get_area_nodes_from_root_to_leaf(_companyid, _areaid) areas WHERE areas.id = AST.area_id) OR _areaid= 0)
	AND (EXISTS (SELECT allowedtemplates.id FROM get_user_allowed_assessmenttemplateids(_companyid, _userid) allowedtemplates WHERE allowedtemplates.id = AST.id) OR _userid = 0)
	AND (AST.assessment_type = _assessmenttype OR _assessmenttype IS NULL)
	AND (AST.role = _role OR _role IS NULL)
	AND (A.completed_at::timestamp < get_timestamp_without_companytimezone(_companyid ,_timestamp) OR _timestamp IS NULL)
	AND (A.completed_at >= get_timestamp_without_companytimezone(_companyid ,_starttimestamp) OR _starttimestamp IS NULL)
	AND (A.completed_at <= get_timestamp_without_companytimezone(_companyid ,_endtimestamp) OR _endtimestamp IS NULL)
	AND (
	  CASE
	    WHEN _timespanindays IS NULL OR _timespanindays = 0 THEN TRUE
	    ELSE A.completed_at > (NOW()::date - (_timespanindays || 'days')::interval)
	  END
	)
	AND (A.is_completed = _iscompleted OR _iscompleted IS NULL)
	AND (A.completed_for_id = _completedforid OR _completedforid IS NULL)
	AND (A.assessor_id = _assessorid OR _assessorid IS NULL)
	AND (A.assessment_template_id = _templateid OR _templateid IS NULL)
	--filter on tags start
	AND (_tagids IS NULL
		OR (a."id" IN (
			SELECT DISTINCT TTR.assessment_id
			FROM tags_tag_relation TTR
			WHERE TTR.assessment_id IS NOT NULL AND TTR.tag_id = ANY(_tagids)
		) OR a."id" IN (
			SELECT DISTINCT ASI.assessment_id
			FROM assessment_skillinstructions ASI
				INNER JOIN assessment_template_skillinstructions ATSI ON ASI.assessment_template_skillinstruction_id = ATSI."id"
				INNER JOIN workinstruction_templates WIT ON ATSI.workinstruction_template_id = WIT."id"
				INNER JOIN tags_tag_relation TTR ON ATSI.workinstruction_template_id = TTR.workinstruction_template_id
			WHERE ASI.is_active = TRUE AND TTR.tag_id = ANY(_tagids)
		)
	)) --filter on tags end
	--filter on assessors start
	AND (_assessorids IS NULL
		OR A."id" IN (
			SELECT DISTINCT A.id
	          FROM assessment_skillinstruction_items asii
	          JOIN profiles_user u
	            ON u.id = asii.assessor_id
	           AND u.company_id = _companyid
	         WHERE asii.company_id = _companyid
       		   AND asii.assessment_id = A.id
       		   AND asii.assessor_id IS NOT NULL
 	 		   AND asii.assessor_id = ANY(_assessorids)
			)
		OR (A.assessor_id IS NOT NULL
		AND A.assessor_id = ANY(_assessorids))
		) --filter on assessors end
	AND (_filtertext IS NULL OR POSITION(LOWER(_filtertext) IN LOWER(CONCAT(AST."id", ' ', AST.name))) > 0)
ORDER BY
    -- Primary sort: by completed_at DESC when _sortbycompletedat is true (for completed assessments)
    -- NULLS LAST ensures assessments without completion date appear at the end
    CASE WHEN _sortbycompletedat IS NOT NULL AND _sortbycompletedat THEN A.completed_at END DESC NULLS LAST,
    -- Secondary sort: by modified_at DESC when _sortbymodifiedat is true
    CASE WHEN _sortbymodifiedat IS NOT NULL AND _sortbymodifiedat THEN A.modified_at END DESC,
    -- Fallback sorts for consistent ordering
    A.completed_at DESC,
    AST.name ASC,
    AST.id
LIMIT CASE WHEN (_limit > 0) THEN _limit END
OFFSET CASE WHEN (_offset > 0) THEN _offset END
;END$function$
;
