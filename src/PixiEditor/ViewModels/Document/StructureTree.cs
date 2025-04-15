using System.Collections.ObjectModel;
using PixiEditor.Models.Handlers;

namespace PixiEditor.ViewModels.Document;

internal class StructureTree
{
    public ObservableCollection<IStructureMemberHandler> Members { get; } = new();
   
    private Dictionary<INodeHandler, ObservableCollection<IStructureMemberHandler>> _memberMap = new();

    public void Update(NodeGraphViewModel nodeGraphViewModel)
    {
        if (nodeGraphViewModel.OutputNode == null)
        {
            Members.Clear();
            _memberMap.Clear();
            return;
        }
        
        int relativeFolderIndex = 0;
        List<IStructureMemberHandler> membersMet = new();
        ObservableCollection<IStructureMemberHandler> lastRoot = Members;
        nodeGraphViewModel.OutputNode.TraverseBackwards((node, previous, property) =>
        {
            if (previous != null && _memberMap.TryGetValue(previous, out var value))
            {
                if (lastRoot != value)
                {
                    lastRoot = value;
                    relativeFolderIndex = lastRoot.Count;
                }
            }
            
            if(node is IStructureMemberHandler structureMember)
            {
                membersMet.Add(structureMember);
            }
            
            if (previous is IFolderHandler folder)
            {
                if (property.PropertyName == "Content")
                {
                    lastRoot = folder.Children;
                    relativeFolderIndex = 0;
                }
            }
            
            if (node is IFolderHandler handler)
            {
                UpdateMember(handler, relativeFolderIndex, lastRoot);
                relativeFolderIndex++;
            }
            if (node is ILayerHandler layerHandler)
            {
                UpdateMember(layerHandler, relativeFolderIndex, lastRoot);
                relativeFolderIndex++;
            }
            
            _memberMap.TryAdd(node, lastRoot);
            
            return true;
        });
        
        List<IStructureMemberHandler> toRemove = new();
        
        foreach (var member in _memberMap)
        {
            if (!membersMet.Contains(member.Key))
            {
                if(member.Key is not IStructureMemberHandler structureMemberHandler) continue;
                toRemove.Add(structureMemberHandler);
                member.Value.Remove(structureMemberHandler);
            }
        }
        
        foreach (var member in toRemove)
        {
            _memberMap.Remove(member);
        }
    }
    
    private void UpdateMember(IStructureMemberHandler member, int relativeIndex, ObservableCollection<IStructureMemberHandler> root)
    {
        bool existsInMembers = _memberMap.ContainsKey(member);
        if(!existsInMembers)
        {
            int clampIndex = Math.Clamp(relativeIndex, 0, root.Count);
            root.Insert(clampIndex, member);
            _memberMap.Add(member, root);
            return;
        }
        else
        {
            ObservableCollection<IStructureMemberHandler> oldRoot = _memberMap[member];
            if (oldRoot != root)
            {
                oldRoot.Remove(member);
                int clampIndex = Math.Clamp(relativeIndex, 0, root.Count);
                root.Insert(clampIndex, member);
                _memberMap[member] = root;
            }
        }
            
        bool existsAtIndex = root.Count > relativeIndex && root[relativeIndex] == member;
        if (!existsAtIndex) //TODO: this is inefficient, causes a lot of reordering, make the algorithm better
        {
            int clampedIndex = Math.Clamp(relativeIndex, 0, root.Count - 1);
            root.Move(root.IndexOf(member), clampedIndex);
        }
    }
}
