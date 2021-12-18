using PixiEditor.Helpers.Extensions;
using PixiEditor.Models.Layers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using PixiEditor.Models.DataHolders;

namespace PixiEditor.Helpers.Converters
{
    public class LayerStructureToGroupsConverter
        : SingleInstanceMultiValueConverter<LayerStructureToGroupsConverter>
    {
        public override object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is not LayerStructure structure)
            {
                return Binding.DoNothing;
            }

            return GetSubGroups(structure.Groups);
        }

        private ObservableCollection<GuidStructureItem> GetSubGroups(IEnumerable<GuidStructureItem> groups)
        {
            WpfObservableRangeCollection<GuidStructureItem> finalGroups = new WpfObservableRangeCollection<GuidStructureItem>();
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
                if (subGroup.Subgroups.Count > 0)
                {
                    groups.AddRange(GetSubGroups(subGroup));
                }
            }

            return groups.Distinct();
        }
    }
}
