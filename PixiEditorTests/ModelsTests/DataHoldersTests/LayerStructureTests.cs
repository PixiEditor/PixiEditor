using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Layers;
using System;
using Xunit;

namespace PixiEditorTests.ModelsTests.DataHoldersTests
{
    public class LayerStructureTests
    {
        [Fact]
        public void TestThatAddNewGroupAddsNewGroup()
        {
            Document doc = new Document(1, 1);
            doc.Layers.Add(new("_testLayer", 1, 1));
            var testLayer = doc.Layers[^1];
            doc.LayerStructure.AddNewGroup("test", testLayer.GuidValue);

            Assert.Single(doc.LayerStructure.Groups);
            Assert.Equal(testLayer.GuidValue, doc.LayerStructure.Groups[0].StartLayerGuid);
            Assert.Equal(testLayer.GuidValue, doc.LayerStructure.Groups[0].EndLayerGuid);
        }

        [Fact]
        public void TestThatAddNewGroupAddsNewGroupAsASubgroup()
        {
            Document doc = new Document(1, 1);
            doc.Layers.Add(new("_testLayer", 1, 1));
            var testLayer = doc.Layers[^1];
            doc.LayerStructure.AddNewGroup("test", testLayer.GuidValue);
            doc.LayerStructure.AddNewGroup("test1", testLayer.GuidValue);

            Assert.Single(doc.LayerStructure.Groups);
            Assert.Single(doc.LayerStructure.Groups[0].Subgroups);
            Assert.Equal(testLayer.GuidValue, doc.LayerStructure.Groups[0].StartLayerGuid);
            Assert.Equal(testLayer.GuidValue, doc.LayerStructure.Groups[0].EndLayerGuid);
            Assert.Equal(testLayer.GuidValue, doc.LayerStructure.Groups[0].Subgroups[0].StartLayerGuid);
            Assert.Equal(testLayer.GuidValue, doc.LayerStructure.Groups[0].Subgroups[0].EndLayerGuid);
        }

        [Fact]
        public void TestThatMoveGroupMovesSwapsLayerPlacesWithOtherGroup()
        {
            Document doc = new Document(1, 1);
            doc.Layers.Add(new Layer("_testLayer", 1, 1));
            doc.Layers.Add(new Layer("_testLayer1", 1, 1));
            var testLayer = doc.Layers[0];
            var testLayer1 = doc.Layers[^1];
            doc.LayerStructure.AddNewGroup("test", testLayer.GuidValue);
            doc.LayerStructure.AddNewGroup("test1", testLayer1.GuidValue);

            Assert.Equal(0, doc.Layers.IndexOf(testLayer));
            Assert.Equal(1, doc.Layers.IndexOf(testLayer1));

            doc.LayerStructure.MoveGroup(doc.LayerStructure.Groups[0].GroupGuid, 1);

            Assert.Equal(1, doc.Layers.IndexOf(testLayer));
            Assert.Equal(0, doc.Layers.IndexOf(testLayer1));
        }

        [Fact]
        public void TestThatIsChildOfDetectsNestedGroupCorrectly()
        {
            LayerStructure ls = new LayerStructure(new Document(1, 1));
            Layer testLayer = new Layer("tst", 1, 1);
            ls.Groups.Add(new GuidStructureItem("group 1", testLayer.GuidValue));
            ls.Groups[0].Subgroups.Add(new GuidStructureItem("group 1 nested", testLayer.GuidValue));

            Assert.True(ls.IsChildOf(ls.Groups[0].Subgroups[0], ls.Groups[0]));
            Assert.False(ls.IsChildOf(ls.Groups[0], ls.Groups[0].Subgroups[0]));
        }

        [Fact]
        public void TestThatIsChildOfDetectsNestedLayersCorrectly()
        {
            var doc = new Document(1, 1);
            doc.Layers.Add(new Layer("tst", 1, 1));
            Guid testLayerGuid = doc.Layers[0].GuidValue;
            LayerStructure ls = new LayerStructure(doc);
            ls.AddNewGroup("Test group", testLayerGuid);
            ls.AddNewGroup("Test group nested", testLayerGuid);

            Assert.True(ls.IsChildOf(testLayerGuid, ls.Groups[0]));
            Assert.True(ls.IsChildOf(testLayerGuid, ls.Groups[0].Subgroups[0]));
        }

        [Fact]
        public void TestThatGroupContainsOnlyLayerDetectsOnlySingleLayerCorrectly()
        {
            var doc = new Document(1, 1);
            doc.Layers.Add(new Layer("layer", 1, 1));
            var guid = doc.Layers[0].GuidValue;
            doc.LayerStructure.AddNewGroup("layer group", guid);
            Assert.True(LayerStructure.GroupContainsOnlyLayer(guid, doc.LayerStructure.Groups[0]));
        }

        [Fact]
        public void TestThatGroupContainsOnlyLayerDetectsOnlySingleLayerThatIsNested()
        {
            var doc = new Document(1, 1);
            doc.Layers.Add(new Layer("layer", 1, 1));
            var guid = doc.Layers[0].GuidValue;
            doc.LayerStructure.AddNewGroup("layer group", guid);
            doc.LayerStructure.AddNewGroup("layer group nested", guid);
            Assert.False(LayerStructure.GroupContainsOnlyLayer(guid, doc.LayerStructure.Groups[0]));
            Assert.True(LayerStructure.GroupContainsOnlyLayer(guid, doc.LayerStructure.Groups[0].Subgroups[0]));
        }

        [Fact]
        public void TestThatCloneReturnsSameLayerStructure()
        {
            Document doc = new(1, 1);
            doc.Layers.Add(new("Test", 1, 1));
            doc.Layers.Add(new("Test2", 1, 1));
            LayerStructure structure = new(doc);
            structure.AddNewGroup("Test group", doc.Layers[0].GuidValue);

            var clone = structure.CloneGroups();

            Assert.Equal(structure.Groups.Count, clone.Count);
            Assert.Single(clone);
            Assert.Equal(structure.Groups[0].GroupGuid, clone[0].GroupGuid);
        }

        [Fact]
        public void TestThatGetGroupByGuidReturnsNullForNonExistingGroup()
        {
            Document doc = new(1, 1);
            doc.Layers.Add(new("Test", 1, 1));

            Assert.Null(doc.LayerStructure.GetGroupByGuid(null));
            Assert.Null(doc.LayerStructure.GetGroupByGuid(Guid.NewGuid()));
        }

        [Fact]
        public void TestThatGetGroupByGuidReturnsGroupCorrectly()
        {
            Document doc = new(1, 1);
            doc.Layers.Add(new("Test", 1, 1));
            var group = doc.LayerStructure.AddNewGroup("Test group", doc.Layers[0].GuidValue);

            Assert.Equal(group.GroupGuid, doc.LayerStructure.GetGroupByGuid(group.GroupGuid).GroupGuid);
        }

        [Fact]
        public void TestThatPreMoveReassignBoundsMakesNestedGroupEmptyAndRemovesItAndParent()
        {
            Document doc = new(1, 1);
            doc.Layers.Add(new("Test", 1, 1));
            doc.LayerStructure.AddNewGroup("Test group", doc.Layers[0].GuidValue);
            var group1 = doc.LayerStructure.AddNewGroup("Test group nested", doc.Layers[0].GuidValue);

            doc.LayerStructure.PreMoveReassignBounds(new GroupData(group1.GroupGuid), doc.Layers[0].GuidValue);

            Assert.Empty(doc.LayerStructure.Groups);
        }

        [Fact]
        public void TestThatPostMoveReassignBoundsAssignsNewLayerToGroup()
        {
            Document doc = new(1, 1);
            doc.Layers.Add(new("Test", 1, 1));
            doc.LayerStructure.AddNewGroup("Test group", doc.Layers[0].GuidValue);
            var group1 = doc.LayerStructure.AddNewGroup("Test group nested", doc.Layers[0].GuidValue);

            doc.Layers.Add(new("Test 1", 1, 1));

            var firstLayer = doc.Layers[0];
            var layer = doc.Layers[^1];

            doc.LayerStructure.PostMoveReassignBounds(new GroupData(group1.GroupGuid), layer.GuidValue);

            Assert.Single(doc.LayerStructure.Groups);
            Assert.Single(doc.LayerStructure.Groups[0].Subgroups);
            Assert.Equal(layer.GuidValue, doc.LayerStructure.Groups[0].Subgroups[0].EndLayerGuid);
            Assert.Equal(firstLayer.GuidValue, doc.LayerStructure.Groups[0].Subgroups[0].StartLayerGuid);
            Assert.Equal(layer.GuidValue, doc.LayerStructure.Groups[0].EndLayerGuid);
            Assert.Equal(firstLayer.GuidValue, doc.LayerStructure.Groups[0].StartLayerGuid);
        }

        [Fact]
        public void TestThatAssignParentAssignsParent()
        {
            Document doc = new(1, 1);
            doc.Layers.Add(new Layer("Test", 1, 1));

            var firstLayer = doc.Layers[0];

            doc.LayerStructure.AddNewGroup("Test group", doc.Layers[0].GuidValue);

            doc.Layers.Add(new Layer("Test 1", 1, 1));

            var layer = doc.Layers[^1];

            doc.LayerStructure.AssignParent(doc.Layers[^1].GuidValue, doc.LayerStructure.Groups[0].GroupGuid);

            Assert.Equal(layer.GuidValue, doc.LayerStructure.Groups[0].EndLayerGuid);
            Assert.Equal(firstLayer.GuidValue, doc.LayerStructure.Groups[0].StartLayerGuid);
        }

        [Fact]
        public void TestThatAssignParentDeAssignsParentOnNull()
        {
            Document doc = new(1, 1);
            doc.Layers.Add(new Layer("Test", 1, 1));

            var firstLayer = doc.Layers[0];

            doc.LayerStructure.AddNewGroup("Test group", doc.Layers[0].GuidValue);

            doc.Layers.Add(new Layer("Test 1", 1, 1));

            var layer = doc.Layers[^1];

            doc.LayerStructure.AssignParent(layer.GuidValue, doc.LayerStructure.Groups[0].GroupGuid);
            doc.LayerStructure.AssignParent(layer.GuidValue, null);

            Assert.Equal(firstLayer.GuidValue, doc.LayerStructure.Groups[0].EndLayerGuid);
            Assert.Equal(firstLayer.GuidValue, doc.LayerStructure.Groups[0].StartLayerGuid);
        }

        [Fact]
        public void TestThatGetGroupLayersReturnsAllLayersInGroup()
        {
            Document doc = new(1, 1);
            doc.Layers.Add(new Layer("Test", 1, 1));
            doc.Layers.Add(new Layer("Test 1", 1, 1));
            doc.Layers.Add(new Layer("Test 2", 1, 1));
            doc.Layers.Add(new Layer("Test 3", 1, 1));
            doc.LayerStructure.AddNewGroup("Test group", doc.Layers[0].GuidValue);

            doc.LayerStructure.AssignParent(doc.Layers[1].GuidValue, doc.LayerStructure.Groups[0].GroupGuid);
            doc.LayerStructure.AssignParent(doc.Layers[2].GuidValue, doc.LayerStructure.Groups[0].GroupGuid);
            doc.LayerStructure.AddNewGroup("Test group", doc.Layers[2].GuidValue);

            var layersInGroup = doc.LayerStructure.GetGroupLayers(doc.LayerStructure.Groups[0]);

            Assert.Equal(3, layersInGroup.Count);
            Assert.Contains(doc.Layers[0], layersInGroup);
            Assert.Contains(doc.Layers[1], layersInGroup);
            Assert.Contains(doc.Layers[2], layersInGroup);
        }
    }
}
