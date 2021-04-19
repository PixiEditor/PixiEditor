using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Layers;
using Xunit;

namespace PixiEditorTests.ModelsTests.LayerStructureTests
{
    public class LayerStructureCloneTests
    {
        public void TestThatCloneReturnsSameLayerStructure()
        {
            Document doc = new(0, 0);
            doc.Layers.Add(new("Test"));
            doc.Layers.Add(new("Test2"));
            LayerStructure structure = new(doc);
            structure.AddNewGroup("Test group", doc.Layers[0].LayerGuid);

            var clone = structure.CloneGroups();

            Assert.Equal(structure.Groups, clone);
            Assert.Single(clone);
            Assert.Equal(structure.Groups[0].GroupGuid, clone[0].GroupGuid);
        }
    }
}