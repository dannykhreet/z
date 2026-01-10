# Database Migration Instructions

## Migration: 20260110_add_action_sorting_and_priority.sql

### What This Migration Does

1. Adds `priority` column to `actions_action` table (integer, default 3)
2. Updates `get_actions_v3` stored procedure:
   - Adds `_sortby` and `_sortdirection` parameters
   - Adds `priority` to return table
   - Updates ORDER BY for dynamic sorting

### Supported Sort Columns

- id, name, duedate (default), startdate, modifiedat, areaname, username, lastcommentdate, priority
- Backwards compatible aliases: description→name, modificationdate→modifiedat, area→areaname, user→username, recentchat→lastcommentdate

### How to Run

```bash
psql -U your_username -d your_database -f Database/Migrations/20260110_add_action_sorting_and_priority.sql
```

Or execute the script in pgAdmin/your database client.

### Post-Migration

```sql
-- Grant permissions
GRANT EXECUTE ON FUNCTION public.get_actions_v3 TO your_app_user;

-- Test sorting
SELECT id, description, priority, due_date
FROM get_actions_v3(
    _companyid := 1,
    _sortby := 'priority',
    _sortdirection := 'asc',
    _limit := 10
);
```

### Rollback

```sql
ALTER TABLE public.actions_action DROP COLUMN IF EXISTS priority;
DROP INDEX IF EXISTS public.idx_actions_action_priority;
```

**Note:** You'll need to restore the original `get_actions_v3` from backup.

### Notes

- Migration is idempotent (safe to run multiple times)
- Existing calls work without changes (fully backwards compatible)
- Priority values: 1=Critical, 2=High, 3=Normal (default), 4=Low
