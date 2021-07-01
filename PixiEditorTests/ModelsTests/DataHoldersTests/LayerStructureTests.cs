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

            doc.LayerStructure.MoveGroup(doc.LayerStructure.Groups[0].GroupGuid, 1);

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

        [Fact]
        public void TestThatGroupContainsOnlyLayerDetectsOnlySingleLayerCorrectly()
        {
            var doc = new Document(0, 0);
            doc.Layers.Add(new Layer("layer"));
            var guid = doc.Layers[0].LayerGuid;
            doc.LayerStructure.AddNewGroup("layer group", guid);
            Assert.True(LayerStructure.GroupContainsOnlyLayer(guid, doc.LayerStructure.Groups[0]));
        }

        [Fact]
        public void TestThatGroupContainsOnlyLayerDetectsOnlySingleLayerThatIsNested()
        {
            var doc = new Document(0, 0);
            doc.Layers.Add(new Layer("layer"));
            var guid = doc.Layers[0].LayerGuid;
            doc.LayerStructure.AddNewGroup("layer group", guid);
            doc.LayerStructure.AddNewGroup("layer group nested", guid);
            Assert.False(LayerStructure.GroupContainsOnlyLayer(guid, doc.LayerStructure.Groups[0]));
            Assert.True(LayerStructure.GroupContainsOnlyLayer(guid, doc.LayerStructure.Groups[0].Subgroups[0]));
        }

        [Fact]
        public void TestThatCloneReturnsSameLayerStructure()
        {
            Document doc = new(0, 0);
            doc.Layers.Add(new("Test"));
            doc.Layers.Add(new("Test2"));
            LayerStructure structure = new(doc);
            structure.AddNewGroup("Test group", doc.Layers[0].LayerGuid);

            var clone = structure.CloneGroups();

            Assert.Equal(structure.Groups.Count, clone.Count);
            Assert.Single(clone);
            Assert.Equal(structure.Groups[0].GroupGuid, clone[0].GroupGuid);
        }

        [Fact]
        public void TestThatGetGroupByGuidReturnsNullForNonExistingGroup()
        {
            Document doc = new(0, 0);
            doc.Layers.Add(new("Test"));

            Assert.Null(doc.LayerStructure.GetGroupByGuid(null));
            Assert.Null(doc.LayerStructure.GetGroupByGuid(Guid.NewGuid()));
        }

        [Fact]
        public void TestThatGetGroupByGuidReturnsGroupCorrectly()
        {
            Document doc = new(0, 0);
            doc.Layers.Add(new("Test"));
            var group = doc.LayerStructure.AddNewGroup("Test group", doc.Layers[0].LayerGuid);

            Assert.Equal(group.GroupGuid, doc.LayerStructure.GetGroupByGuid(group.GroupGuid).GroupGuid);
        }

        [Fact]
        public void TestThatPreMoveReassignBoundsMakesNestedGroupEmptyAndRemovesItAndParent()
        {
            Document doc = new(0, 0);
            doc.Layers.Add(new("Test"));
            doc.LayerStructure.AddNewGroup("Test group", doc.Layers[0].LayerGuid);
            var group1 = doc.LayerStructure.AddNewGroup("Test group nested", doc.Layers[0].LayerGuid);

            doc.LayerStructure.PreMoveReassignBounds(new GroupData(group1.GroupGuid), doc.Layers[0].LayerGuid);

            Assert.Empty(doc.LayerStructure.Groups);
        }

        [Fact]
        public void TestThatPostMoveReassignBoundsAssignsNewLayerToGroup()
        {
            Document doc = new(0, 0);
            doc.Layers.Add(new("Test"));
            doc.LayerStructure.AddNewGroup("Test group", doc.Layers[0].LayerGuid);
            var group1 = doc.LayerStructure.AddNewGroup("Test group nested", doc.Layers[0].LayerGuid);

            doc.Layers.Add(new("Test 1"));

            var firstLayer = doc.Layers[0];
            var layer = doc.Layers[^1];

            doc.LayerStructure.PostMoveReassignBounds(new GroupData(group1.GroupGuid), layer.LayerGuid);

            Assert.Single(doc.LayerStructure.Groups);
            Assert.Single(doc.LayerStructure.Groups[0].Subgroups);
            Assert.Equal(layer.LayerGuid, doc.LayerStructure.Groups[0].Subgroups[0].EndLayerGuid);
            Assert.Equal(firstLayer.LayerGuid, doc.LayerStructure.Groups[0].Subgroups[0].StartLayerGuid);
            Assert.Equal(layer.LayerGuid, doc.LayerStructure.Groups[0].EndLayerGuid);
            Assert.Equal(firstLayer.LayerGuid, doc.LayerStructure.Groups[0].StartLayerGuid);
        }

        [Fact]
        public void TestThatAssignParentAssignsParent()
        {
            Document doc = new(0, 0);
            doc.Layers.Add(new Layer("Test"));

            var firstLayer = doc.Layers[0];

            doc.LayerStructure.AddNewGroup("Test group", doc.Layers[0].LayerGuid);

            doc.Layers.Add(new Layer("Test 1"));

            var layer = doc.Layers[^1];

            doc.LayerStructure.AssignParent(doc.Layers[^1].LayerGuid, doc.LayerStructure.Groups[0].GroupGuid);

            Assert.Equal(layer.LayerGuid, doc.LayerStructure.Groups[0].EndLayerGuid);
            Assert.Equal(firstLayer.LayerGuid, doc.LayerStructure.Groups[0].StartLayerGuid);
        }

        [Fact]
        public void TestThatAssignParentDeAssignsParentOnNull()
        {
            Document doc = new(0, 0);
            doc.Layers.Add(new Layer("Test"));

            var firstLayer = doc.Layers[0];

            doc.LayerStructure.AddNewGroup("Test group", doc.Layers[0].LayerGuid);

            doc.Layers.Add(new Layer("Test 1"));

            var layer = doc.Layers[^1];

            doc.LayerStructure.AssignParent(layer.LayerGuid,  doc.LayerStructure.Groups[0].GroupGuid);
            doc.LayerStructure.AssignParent(layer.LayerGuid, null);

            Assert.Equal(firstLayer.LayerGuid, doc.LayerStructure.Groups[0].EndLayerGuid);
            Assert.Equal(firstLayer.LayerGuid, doc.LayerStructure.Groups[0].StartLayerGuid);
        }

        [Fact]
        public void TestThatGetGroupLayersReturnsAllLayersInGroup()
        {
            Document doc = new(0, 0);
            doc.Layers.Add(new Layer("Test"));
            doc.Layers.Add(new Layer("Test 1"));
            doc.Layers.Add(new Layer("Test 2"));
            doc.Layers.Add(new Layer("Test 3"));
            doc.LayerStructure.AddNewGroup("Test group", doc.Layers[0].LayerGuid);

            doc.LayerStructure.AssignParent(doc.Layers[1].LayerGuid, doc.LayerStructure.Groups[0].GroupGuid);
            doc.LayerStructure.AssignParent(doc.Layers[2].LayerGuid, doc.LayerStructure.Groups[0].GroupGuid);
            doc.LayerStructure.AddNewGroup("Test group", doc.Layers[2].LayerGuid);

            var layersInGroup = doc.LayerStructure.GetGroupLayers(doc.LayerStructure.Groups[0]);

            Assert.Equal(3, layersInGroup.Count);
            Assert.Contains(doc.Layers[0], layersInGroup);
            Assert.Contains(doc.Layers[1], layersInGroup);
            Assert.Contains(doc.Layers[2], layersInGroup);
        }
    }
}