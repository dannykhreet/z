using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Models.Areas;
using EZGO.Maui.Core.ViewModels;
using Syncfusion.Maui.Buttons;
using Syncfusion.Maui.TreeView;
using Syncfusion.TreeView.Engine;

namespace EZGO.Maui.Controls.WorkAreas;

public partial class AreasGridControl : ContentView
{
    private int columnsPerRow = 3;
    private int minimumRowAmount = 2;

    public AreasGridControl()
    {
        InitializeComponent();
        //Placeholder.Margin = GridMargin;
        AreaScrollView.Margin = GridMargin;
        AreaGrid.RowSpacing = GridRowSpacing;
        InitializePlaceholder();
    }

    #region Events

    private void ExpandArea_OnClicked(object sender, EventArgs e)
    {
        var workAreaViewModel = (WorkAreaViewModel)BindingContext;
        if (workAreaViewModel.IsBusy)
            return;

        SfButton button = (SfButton)sender;
        BasicWorkAreaModel workArea = (BasicWorkAreaModel)button.BindingContext;
        workArea.IsRootExpanded = !workArea.IsRootExpanded;
        foreach (View areaGridChild in AreaGrid.Children)
        {
            BasicWorkAreaModel workarea = (BasicWorkAreaModel)areaGridChild.BindingContext;
            if (workArea.Id != workarea.Id)
                workarea.IsRootExpanded = false;
        }

        InitializeAreaGrid();
    }

    private void TapGestureRecognizer_OnTapped(object sender, EventArgs e)
    {
        Image imageIcon = sender as Image ?? new Image();
        TreeViewNode workArea = imageIcon.BindingContext as TreeViewNode ?? new TreeViewNode();
        SfTreeView treeView = GetTreeView(imageIcon);

        if (treeView != null)
        {
            if (workArea.IsExpanded)
                treeView.CollapseNode(workArea);
            else
                treeView.ExpandNode(workArea);
        }
    }

    #endregion

    #region Methods

    private static SfTreeView GetTreeView(Element element)
    {
        //TODO Find out why we don't have access to the TreeView normal way
        var treeView = element.Parent.Parent.Parent.Parent.Parent.Parent.Parent as SfTreeView;

        return treeView ?? new SfTreeView();
    }

    private void InitializePlaceholder()
    {
        //Placeholder.Children.Clear();

        //for (int i = 0; i < minimumRowAmount; i++)
        //{
        //    for (int j = 0; j < columnsPerRow; j++)
        //    {
        //        var grid = new Grid
        //        {
        //            Opacity = 0.1,
        //            BackgroundColor = Colors.White
        //        };
        //        Grid.SetRow(grid, i);
        //        Grid.SetColumn(grid, j);

        //        Placeholder.Children.Add(grid);
        //    }
        //}
    }

    private void InitializeAreaGrid()
    {
        int rowNumber = 0;
        int columnNumber = 0;

        void IncrementRow()
        {
            rowNumber++;
            columnNumber = 0;
        }

        double CalculateItemSize(double size, double spacing, int itemsCount) => (size - (spacing * (itemsCount - 1))) / itemsCount;

        double scrollViewHeight = AreaScrollView.Height;
        double scrollViewWidth = AreaScrollView.Width;

        if (scrollViewHeight <= 0 || scrollViewWidth <= 0) return;

        double rowHeight = CalculateItemSize(scrollViewHeight, AreaGrid.RowSpacing, minimumRowAmount);
        double columnWidth = CalculateItemSize(scrollViewWidth, AreaGrid.ColumnSpacing, columnsPerRow);

        int expandedColumn = -1;
        int expandedRow = -1;

        if (AreaGrid.Children == null || !AreaGrid.Children.Any()) return;

        int rowCount = (AreaGrid.Children.Count + columnsPerRow - 1) / columnsPerRow;

        if (rowCount < minimumRowAmount)
            rowCount = minimumRowAmount;

        AreaGrid.RowDefinitions.Clear();
        AreaGrid.ColumnDefinitions.Clear();

        for (int rowIndex = 0; rowIndex < rowCount; rowIndex++)
        {
            AreaGrid.RowDefinitions.Add(new RowDefinition { Height = rowHeight });
        }

        for (int columnIndex = 0; columnIndex < columnsPerRow; columnIndex++)
        {
            AreaGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = columnWidth });
        }

        foreach (View areaGridChild in AreaGrid.Children)
        {
            if (columnNumber == expandedColumn && rowNumber == expandedRow + 1)
            {
                columnNumber++;

                if (columnNumber > columnsPerRow - 1)
                    IncrementRow();
            }

            Grid.SetRow(areaGridChild, rowNumber);
            Grid.SetColumn(areaGridChild, columnNumber);

            if (areaGridChild.BindingContext is BasicWorkAreaModel workArea && workArea.IsRootExpanded)
            {
                Grid.SetRowSpan(areaGridChild, 2);
                areaGridChild.HeightRequest = scrollViewHeight;
                expandedColumn = columnNumber;
                expandedRow = rowNumber;
            }
            else
            {
                Grid.SetRowSpan(areaGridChild, 1);
                areaGridChild.HeightRequest = rowHeight;
                areaGridChild.WidthRequest = columnWidth;
            }

            if (columnNumber >= columnsPerRow - 1)
                IncrementRow();
            else
                columnNumber++;
        }
    }

    private double sWidth;
    private double sHeight;

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);

        if (sWidth != width || sHeight != height)
        {
            if (sWidth < 0 || sHeight < 0)
            {
                sWidth = DeviceSettings.ScreenWidth;
                sHeight = DeviceSettings.ScreenHeight;
            }
            else
            {
                sWidth = width;
                sHeight = height;
            }

            if (sWidth > sHeight)
            {
                columnsPerRow = 3;
                minimumRowAmount = 2;
            }
            else
            {
                columnsPerRow = 2;
                minimumRowAmount = 3;
            }

            InitializePlaceholder();
            InitializeAreaGrid();
        }

    }

    #endregion
}
