using ChangeableDocument;
using ChangeableDocument.Actions.Structure;
using PixiEditorPrototype.ViewModels;
using System;
using System.Collections.Generic;

namespace PixiEditorPrototype.Models
{
    internal class DocumentStructureHelper
    {
        private DocumentViewModel doc;
        public DocumentStructureHelper(DocumentViewModel doc)
        {
            this.doc = doc;
        }

        public void CreateNewStructureMember(StructureMemberType type)
        {
            if (doc.SelectedStructureMember == null)
            {
                //put member on top
                doc.ActionAccumulator.AddAction(new CreateStructureMember_Action(doc.StructureRoot.GuidValue, 0, type));
                return;
            }
            if (doc.SelectedStructureMember is FolderViewModel folder)
            {
                //put member inside folder on top
                doc.ActionAccumulator.AddAction(new CreateStructureMember_Action(folder.GuidValue, 0, type));
                return;
            }
            if (doc.SelectedStructureMember is LayerViewModel layer)
            {
                //put member above the layer
                var path = FindPath(layer.GuidValue);
                if (path.Count < 2)
                    throw new Exception("Couldn't find a path to the selected member");
                var parent = (FolderViewModel)path[1];
                doc.ActionAccumulator.AddAction(new CreateStructureMember_Action(parent.GuidValue, parent.Children.IndexOf(layer), type));
                return;
            }
            throw new Exception("Unknown member type: " + type.ToString());
        }

        public StructureMemberViewModel FindOrThrow(Guid guid) => Find(guid) ?? throw new Exception("Could not find member with guid " + guid.ToString());
        public StructureMemberViewModel? Find(Guid guid)
        {
            var list = FindPath(guid);
            return list.Count > 0 ? list[0] : null;
        }

        public (StructureMemberViewModel, FolderViewModel) FindChildAndParentOrThrow(Guid childGuid)
        {
            var path = FindPath(childGuid);
            if (path.Count < 2)
                throw new Exception("Couldn't find child and parent");
            return (path[0], (FolderViewModel)path[1]);
        }
        public List<StructureMemberViewModel> FindPath(Guid guid)
        {
            var list = new List<StructureMemberViewModel>();
            if (FillPath(doc.StructureRoot, guid, list))
                list.Add(doc.StructureRoot);
            return list;
        }

        private bool FillPath(FolderViewModel folder, Guid guid, List<StructureMemberViewModel> toFill)
        {
            if (folder.GuidValue == guid)
            {
                return true;
            }
            foreach (var member in folder.Children)
            {
                if (member is LayerViewModel childLayer && childLayer.GuidValue == guid)
                {
                    toFill.Add(member);
                    return true;
                }
                if (member is FolderViewModel childFolder)
                {
                    if (FillPath(childFolder, guid, toFill))
                    {
                        toFill.Add(childFolder);
                        return true;
                    }
                }
            }
            return false;
        }

        public void MoveStructureMember(Guid guid, bool toSmallerIndex)
        {
            var path = FindPath(guid);
            if (path.Count < 2)
                throw new Exception("Couldn't find the member to be moved");
            if (path.Count == 2)
            {
                int curIndex = doc.StructureRoot.Children.IndexOf(path[0]);
                if (curIndex == 0 && toSmallerIndex || curIndex == doc.StructureRoot.Children.Count - 1 && !toSmallerIndex)
                    return;
                doc.ActionAccumulator.AddAction(new MoveStructureMember_Action(guid, doc.StructureRoot.GuidValue, toSmallerIndex ? curIndex - 1 : curIndex + 1));
                return;
            }
            var folder = (FolderViewModel)path[1];
            int index = folder.Children.IndexOf(path[0]);
            if (toSmallerIndex && index > 0 || !toSmallerIndex && index < folder.Children.Count - 1)
            {
                doc.ActionAccumulator.AddAction(new MoveStructureMember_Action(guid, path[1].GuidValue, toSmallerIndex ? index - 1 : index + 1));
            }
            else
            {
                int parentIndex = ((FolderViewModel)path[2]).Children.IndexOf(folder);
                doc.ActionAccumulator.AddAction(new MoveStructureMember_Action(guid, path[2].GuidValue, toSmallerIndex ? parentIndex : parentIndex + 1));
            }
        }
    }
}
