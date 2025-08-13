using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using EZGO.Api.Models.Enumerations;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Classes.ValidationRules;
using EZGO.Maui.Core.Utils;

namespace EZGO.Maui.Core.Models.Feed
{
    public class AddFeedItemModel : NotifyPropertyChanged
    {
        private int NumberOfMediaElements;
        public ObservableCollection<MediaItem> MediaItems { get; set; }

        public ValidatableObject<string> Title { get; set; } = new ValidatableObject<string>();
        public ValidatableObject<string> Description { get; set; } = new ValidatableObject<string>();
        public bool IsSticky { get; set; } = false;
        public bool IsHighlighted { get; set; } = false;
        public FeedTypeEnum FeedType { get; set; }
        public FeedItemTypeEnum FeedItemType { get; set; }
        public int? ParentId { get; set; }
        public int FeedId { get; set; }
        public int Id { get; set; }
        public bool IsFactoryUpdate => FeedType == FeedTypeEnum.FactoryUpdates;

        public AddFeedItemModel(FeedItemTypeEnum feedItemType, FeedTypeEnum feedType, int? ParentId = null)
        {
            FeedItemType = feedItemType;
            FeedType = feedType;

            if (ParentId == null && FeedType == FeedTypeEnum.MainFeed)
                NumberOfMediaElements = 5;
            else
                NumberOfMediaElements = 1;

            MediaItems = new ObservableCollection<MediaItem>(Enumerable.Range(1, NumberOfMediaElements).Select(x => MediaItem.Empty()));
            InitializeValidationRules();
        }

        private void InitializeValidationRules()
        {
            Title.Validations.Add(new IsNullOrEmptyValidationRule<string>("Title should not be empty"));
            Title.Validations.Add(new IsNullOrWhiteSpaceValidationRule<string>("Title should not be empty"));
            Title.Validations.Add(new IsLenghtToLongValidationRule<string>($"Title is longer than {Constants.FeedTitleMaxLength} characters", Constants.FeedTitleMaxLength));
            Description.Validations.Add(new IsNullOrEmptyValidationRule<string>("Description should not be empty"));
            Description.Validations.Add(new IsNullOrWhiteSpaceValidationRule<string>("Description should not be empty"));
        }
    }
}
