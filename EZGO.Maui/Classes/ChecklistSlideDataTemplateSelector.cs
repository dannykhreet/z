using System;
using EZGO.Maui.Core.Models.Tasks;

namespace EZGO.Maui.Classes
{
    public class ChecklistSlideDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate CarouselItemTemplate { get; set; }

        public DataTemplate SignButtonTemplate { get; set; }

        public DataTemplate ChecklistPropertyTemplate { get; set; }

        public DataTemplate SignSlideItemTemplate { get; set; }

        /// <param name="item">The data for which to return a template.</param>
        /// <param name="container">An optional container object in which the developer may have opted to store <see cref="T:Xamarin.Forms.DataTemplateSelector" /> objects.</param>
        /// <summary>The developer overrides this method to return a valid data template for the specified <paramref name="item" />. This method is called by <see cref="M:Xamarin.Forms.DataTemplateSelector.SelectTemplate(System.Object,Xamarin.Forms.BindableObject)" />.</summary>
        /// <returns>A developer-defined <see cref="T:Xamarin.Forms.DataTemplate" /> that can be used to display <paramref name="item" />.</returns>
        /// <remarks>
        ///     <para>This method causes <see cref="M:Xamarin.Forms.DataTemplateSelector.SelectTemplate(System.Object,Xamarin.Forms.BindableObject)" /> to throw an exception if it returns an instance of <see cref="T:Xamarin.Forms.DataTemplateSelector" />.</para>
        /// </remarks>
        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            DataTemplate result;

            BasicTaskTemplateModel taskTemplate = item as BasicTaskTemplateModel;

            if (taskTemplate.IsSignButton)
                result = SignButtonTemplate;
            else if (taskTemplate.IsPropertyButton)
                result = ChecklistPropertyTemplate;
            else if (taskTemplate.Id == -1 && taskTemplate.StageTemplateId != null)
                result = SignSlideItemTemplate;
            else
                result = CarouselItemTemplate;

            return result;
        }
    }
}

