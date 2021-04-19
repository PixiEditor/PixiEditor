using System;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Layers;
using Xunit;

namespace PixiEditorTests.ModelsTests.DataHoldersTests
{
    public class LayerStructureTests
    {
        [Fact]
        public void TestThatAddNewGroupAddsNewGroup()
        {
            Document doc = new Document(1, 1);
            doc.Layers.Add(new("_testLayer"));
            var testLayer = doc.Layers[^1];
            doc.LayerStructure.AddNewGroup("test", testLayer.LayerGuid);

            Assert.Single(doc.LayerStructure.Groups);
            Assert.Equal(testLayer.LayerGuid, doc.LayerStructure.Groups[0].StartLayerGuid);
            Assert.Equal(testLayer.LayerGuid, doc.LayerStructure.Groups[0].EndLayerGuid);
        }

        [Fact]
        public void TestThatAddNewGroupAddsNewGroupAsASubgroup()
        {
            Document doc = new Document(1, 1);
            doc.Layers.Add(new("_testLayer"));
            var testLayer = doc.Layers[^1];
            doc.LayerStructure.AddNewGroup("test", testLayer.LayerGuid);
            doc.LayerStructure.AddNewGroup("test1", testLayer.LayerGuid);

            Assert.Single(doc.LayerStructure.Groups);
            Assert.Single(doc.LayerStructure.Groups[0].Subgroups);
            Assert.Equal(testLayer.LayerGuid, doc.LayerStructure.Groups[0].StartLayerGuid);
            Assert.Equal(testLayer.LayerGuid, doc.LayerStructure.Groups[0].EndLayerGuid);
            Assert.Equal(testLayer.LayerGuid, doc.LayerStructure.Groups[0].Subgroups[0].StartLayerGuid);
            Assert.Equal(testLayer.LayerGuid, doc.LayerStructure.Groups[0].Subgroups[0].EndLayerGuid);
        }

        [Fact]
        public void TestThatMoveGroupMovesSwapsLayerPlacesWithOtherGroup()
        {
            Document doc = new Document(1, 1);
            doc.Layers.Add(new Layer("_testLayer"));
            doc.Layers.Add(new Layer("_testLayer1"));
            var testLayer = doc.Layers[0];
            var testLayer1 = doc.Layers[^1];
            doc.LayerStructure.AddNewGroup("test", testLayer.LayerGuid);
            doc.LayerStructure.AddNewGroup("test1", testLayer1.LayerGuid);

            Assert.Equal(0, doc.Layers.IndexOf(testLayer));
            Assert.Equal(1, doc.Layers.IndexOf(testLayer1));

            doc.LayerStructure.MoveGroup(doc.LayerStructure.Groups[0].GroupGuid, null, 1);

            Assert.Equal(1, doc.Layers.IndexOf(testLayer));
            Assert.Equal(0, doc.Layers.IndexOf(testLayer1));
        }

        [Fact]
        public void TestThatIsChildOfDetectsNestedGroupCorrectly()
        {
            LayerStructure ls = new LayerStructure(new Document(0, 0));
            Layer testLayer = new Layer("tst");
            ls.Groups.Add(new GuidStructureItem("group 1", testLayer.LayerGuid));
            ls.Groups[0].Subgroups.Add(new GuidStructureItem("group 1 nested", testLayer.LayerGuid));

            Assert.True(ls.IsChildOf(ls.Groups[0].Subgroups[0], ls.Groups[0]));
            Assert.False(ls.IsChildOf(ls.Groups[0], ls.Groups[0].Subgroups[0]));
        }

        [Fact]
        public void TestThatIsChildOfDetectsNestedLayersCorrectly()
        {
            var doc = new Document(0, 0);
            doc.Layers.Add(new Layer("tst"));
            Guid testLayerGuid = doc.Layers[0].LayerGuid;
            LayerStructure ls = new LayerStructure(doc);
            ls.AddNewGroup("Test group", testLayerGuid);
            ls.AddNewGroup("Test group nested", testLayerGuid);

            Assert.True(ls.IsChildOf(testLayerGuid, ls.Groups[0]));
            Assert.True(ls.IsChildOf(testLayerGuid, ls.Groups[0].Subgroups[0]));
        }
    }
}