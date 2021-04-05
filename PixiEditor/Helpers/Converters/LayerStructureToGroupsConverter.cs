using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using PixiEditor.Helpers.Extensions;
using PixiEditor.Models.Layers;

namespace PixiEditor.Helpers.Converters
{
    public class LayerStructureToGroupsConverter : IMultiValueConverter
    {
        private ObservableCollection<GuidStructureItem> GetSubGroups(IEnumerable<GuidStructureItem> groups)
        {
            ObservableCollection<GuidStructureItem> finalGroups = new ObservableCollection<GuidStructureItem>();
            foreach (var group in groups)
            {
                finalGroups.AddRange(GetSubGroups(group));
            }

            return finalGroups;
        }

        private IEnumerable<GuidStructureItem> GetSubGroups(GuidStructureItem group)
        {
            List<GuidStructureItem> groups = new List<GuidStructureItem>() { group };

            foreach (var subGroup in group.Subgroups)
            {
                groups.Add(subGroup);
                if(subGroup.Subgroups.Count > 0)
                {
                    groups.AddRange(GetSubGroups(subGroup));
                }
            }

            return groups.Distinct();
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is LayerStructure structure)
            {
                return GetSubGroups(structure.Groups);
            }

            return Binding.DoNothing;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}