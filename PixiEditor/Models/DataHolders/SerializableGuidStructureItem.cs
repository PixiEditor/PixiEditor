using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixiEditor.Models.DataHolders
{
    [Serializable]
    public class SerializableGuidStructureItem
    {
        public Guid GroupGuid { get; set; }

        public string Name { get; set; }

        public Guid StartLayerGuid { get; set; }

        public Guid EndLayerGuid { get; set; }

        public SerializableGuidStructureItem[] Subgroups { get; set; }

        public SerializableGuidStructureItem Parent { get; set; }

        public SerializableGuidStructureItem(Guid groupGuid, string name, Guid startLayerGuid, Guid endLayerGuid, SerializableGuidStructureItem[] subgroups, SerializableGuidStructureItem parent)
        {
            GroupGuid = groupGuid;
            Name = name;
            StartLayerGuid = startLayerGuid;
            EndLayerGuid = endLayerGuid;
            Subgroups = subgroups;
            Parent = parent;
        }
    }
}