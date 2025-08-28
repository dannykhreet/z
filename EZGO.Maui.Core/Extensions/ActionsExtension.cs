using EZGO.Maui.Core.Models.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EZGO.Maui.Core.Extensions
{
    public static class ActionsExtension
    {
        public static List<ActionsModel> SortActions(this IEnumerable<ActionsModel> actions)
        {
            List<ActionsModel> sortedActions = new List<ActionsModel>();

            IOrderedEnumerable<ActionsModel> actionsWithUnreadMessages = actions.Where(action => (action.UnviewedCommentNr) > 0)
                .OrderByDescending(action => action.Comments.Max(comment => comment.ModifiedAt));

            IOrderedEnumerable<ActionsModel> uncompletedActions = actions.Where(action => (action.UnviewedCommentNr) == 0 && !(action.IsResolved ?? false))
                .OrderBy(action => action.DueDate);

            IOrderedEnumerable<ActionsModel> completedActions = actions.Where(action => (action.UnviewedCommentNr) == 0 && (action.IsResolved ?? false))
                .OrderByDescending(action => action.DueDate);

            if (actionsWithUnreadMessages.Any())
                sortedActions.AddRange(actionsWithUnreadMessages);

            if (uncompletedActions.Any())
                sortedActions.AddRange(uncompletedActions);

            if (completedActions.Any())
                sortedActions.AddRange(completedActions);

            return sortedActions;
        }

        public static List<BasicActionsModel> SortActions(this IEnumerable<BasicActionsModel> actions)
        {
            List<BasicActionsModel> sortedActions = new List<BasicActionsModel>();

            List<BasicActionsModel> actionsWithUnreadMessages = actions.Where(action => (action.UnviewedCommentNr) > 0&&action.FilterStatus!=Enumerations.ActionStatusEnum.Solved)
                .OrderByDescending(action => action.Comments.Max(comment => comment.ModifiedAt)).ToList();

            List<BasicActionsModel> uncompletedActions = actions.Where(action => (action.UnviewedCommentNr) == 0 && !(action.IsResolved))
                .OrderBy(action => action.DueDate).ToList();

            List<BasicActionsModel> completedActions = actions.Where(action => action.IsResolved)
                .OrderByDescending(action => action.DueDate).ToList();

            sortedActions.AddRange(actionsWithUnreadMessages);

            sortedActions.AddRange(uncompletedActions);

            sortedActions.AddRange(completedActions);

            return sortedActions;
        }
    }
}