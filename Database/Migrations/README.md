# Database Migration Instructions

## Migration: 20260110_add_action_sorting_and_priority.sql

### What This Migration Does

1. **Adds `priority` column** to `actions_action` table
   - Type: `integer`
   - Default: `3` (Normal priority)
   - Values: 1=Critical, 2=High, 3=Normal, 4=Low
   - Creates index `idx_actions_action_priority` for performance

2. **Creates new stored procedure** `get_actions_v3_sorting`
   - Same functionality as `get_actions_v3` plus sorting capabilities
   - Adds `priority` to return table
   - Adds `_sortby` parameter (text, default: 'duedate')
   - Adds `_sortdirection` parameter (text, default: 'asc')
   - Supports sorting by: id, name, duedate, startdate, modifiedat, areaname, username, lastcommentdate, priority
   - Backwards compatible with old parameter names

3. **Keeps existing** `get_actions_v3` function unchanged for compatibility

### How to Run

#### Option 1: Using psql command line
```bash
psql -U your_username -d your_database -f Database/Migrations/20260110_add_action_sorting_and_priority.sql
```

#### Option 2: Using pgAdmin or database client
1. Open pgAdmin or your preferred PostgreSQL client
2. Connect to your database
3. Open Query Tool
4. Load the file: `Database/Migrations/20260110_add_action_sorting_and_priority.sql`
5. Execute the script

#### Option 3: Copy and paste
Copy the entire contents of the migration file and execute in your database client.

### Post-Migration Steps

1. **Grant permissions** to your application user:
   ```sql
   GRANT EXECUTE ON FUNCTION public.get_actions_v3_sorting TO your_app_user;
   ```

2. **Verify the migration**:
   ```sql
   -- Check priority column
   SELECT column_name, data_type, column_default
   FROM information_schema.columns
   WHERE table_name = 'actions_action' AND column_name = 'priority';

   -- Check stored procedure
   SELECT routine_name, routine_type
   FROM information_schema.routines
   WHERE routine_name = 'get_actions_v3_sorting' AND routine_schema = 'public';
   ```

3. **Test the sorting**:
   ```sql
   -- Test ascending priority sort
   SELECT id, description, priority, due_date
   FROM get_actions_v3_sorting(
       _companyid := 1,
       _sortby := 'priority',
       _sortdirection := 'asc',
       _limit := 10
   );

   -- Test descending due date sort
   SELECT id, description, priority, due_date
   FROM get_actions_v3_sorting(
       _companyid := 1,
       _sortby := 'duedate',
       _sortdirection := 'desc',
       _limit := 10
   );
   ```

### Supported Sort Parameters

| Frontend Value | Database Value | Description |
|----------------|----------------|-------------|
| duedate | duedate | Due date (default) |
| startdate | startdate | Created date |
| modifiedat | modifiedat, modificationdate | Last modified date |
| priority | priority | Action priority (1-4) |
| areaname | areaname, area | Assigned area name |
| username | username, user | Assigned user name |
| lastcommentdate | lastcommentdate, recentchat | Last comment timestamp |
| id | id | Action ID |
| name | name | Action description |

### Rollback (if needed)

```sql
-- Remove stored procedure
DROP FUNCTION IF EXISTS public.get_actions_v3_sorting;

-- Remove priority column (WARNING: data loss)
ALTER TABLE public.actions_action DROP COLUMN IF EXISTS priority;

-- Remove index
DROP INDEX IF EXISTS public.idx_actions_action_priority;
```

### Notes

- Migration is **idempotent** - safe to run multiple times
- Existing `get_actions_v3` function remains unchanged
- Default priority for existing actions is `3` (Normal)
- The script includes verification checks and will output success/failure messages
- NULLS are handled with `NULLS LAST` to ensure consistent sorting

### Support

For issues or questions, check the commit history or contact the development team.
