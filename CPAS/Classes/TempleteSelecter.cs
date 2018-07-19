using CPAS.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace CPAS.Classes
{
    class TemplateSelecter : DataTemplateSelector
    {
        public DataTemplate UI_Data { get; set; }
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {

            FrameworkElement element = container as FrameworkElement;
            if (item == null)
                return null;
           if ((int)item==1)  
                return (DataTemplate)element.TryFindResource("RoiPanel");
            else
                return (DataTemplate)element.TryFindResource("ModelPanel");

        }
    }
}
