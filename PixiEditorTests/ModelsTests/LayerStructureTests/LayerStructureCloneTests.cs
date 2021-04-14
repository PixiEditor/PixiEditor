using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Layers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

            var clone = structure.Clone();

            Assert.Equal(doc, clone.Owner);
            Assert.Single(clone.Groups);
            Assert.Equal(structure.Groups[0].GroupGuid, clone.Groups[0].GroupGuid);
        }
    }
}