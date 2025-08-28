namespace EZGO.Maui.Core.Models.Feed
{
    public class FeedDataModel
    {
        public int MainFeedId { get; set; }
        public int FactoryUpdatesFeedId { get; set; }
        public List<FeedMessageItemModel> MainFeedMessages { get; set; } = new();
        public List<FeedMessageItemModel> FactoryUpdatesMessages { get; set; } = new();
    }
}
