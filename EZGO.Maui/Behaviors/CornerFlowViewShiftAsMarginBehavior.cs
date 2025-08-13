using System;
using EZGO.Maui.Behaviors;
using EZGO.Maui.Core.Classes;
using PanCardView;

namespace EZGO.Maui.Behaviors
{
    public class CornerFlowViewShiftAsMarginBehavior : BehaviorBase<CoverFlowView>
    {
        /// <summary>
        /// The margin applied between after <see cref="CoverFlowView"/>'s child item
        /// </summary>
        public int Margin { get; set; }

        /// <summary>
        /// Width of a single <see cref="CoverFlowView"/>'s child item
        /// </summary>
        public int ChildWidth => (int)DeviceSettings.DeviceFormat.FullWidth;

        protected override void OnAttachedTo(CoverFlowView bindable)
        {
            base.OnAttachedTo(bindable);
            // A little hack for the CornerFlowView control. See the formula in the link below
            // https://github.com/AndreiMisiukevich/CardView/blob/1b139508638dfb5479055b8ec6fbb17c3890876a/PanCardView/Processors/CoverFlowProcessor.cs#L184
            // Set the percentage value to 1.0 to get rid of GetSize() part
            AssociatedObject.PositionShiftPercentage = 1.0;
            // Shift the position by the full width of the child element and plus the additional margin
            // Minus sign is so that we get a positive translation value (see the formula in the link above)
            AssociatedObject.PositionShiftValue = -(Margin + ChildWidth);
        }

        public CornerFlowViewShiftAsMarginBehavior()
        {
        }
    }
}

