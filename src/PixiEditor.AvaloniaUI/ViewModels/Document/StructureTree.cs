using System.Collections.ObjectModel;
using PixiEditor.AvaloniaUI.Models.Handlers;

namespace PixiEditor.AvaloniaUI.ViewModels.Document;

internal class StructureTree
{
    public ObservableCollection<IStructureMemberHandler> Members { get; } = new();
   
    private Dictionary<IStructureMemberHandler, ObservableCollection<IStructureMemberHandler>> _memberMap = new();

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
        nodeGraphViewModel.OutputNode.TraverseBackwards((node, previous) =>
        {
            if (node is IStructureMemberHandler structureMemberHandler)
            {
                membersMet.Add(structureMemberHandler);
            }
            
            if (previous is IFolderHandler folder)
            {
                lastRoot = folder.Children;
                relativeFolderIndex = 0;
            }
            
            if (node is IFolderHandler handler)
            {
                UpdateMember(handler, relativeFolderIndex, lastRoot);
            }
            if (node is ILayerHandler layerHandler)
            {
                UpdateMember(layerHandler, relativeFolderIndex, lastRoot);
                relativeFolderIndex++;
            }
            
            return true;
        });
        
        List<IStructureMemberHandler> toRemove = new();
        
        foreach (var member in _memberMap)
        {
            if (!membersMet.Contains(member.Key))
            {
                toRemove.Add(member.Key);
                member.Value.Remove(member.Key);
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
            root.Insert(relativeIndex, member);
            _memberMap.Add(member, root);
            return;
        }
        else
        {
            ObservableCollection<IStructureMemberHandler> oldRoot = _memberMap[member];
            if (oldRoot != root)
            {
                oldRoot.Remove(member);
                root.Insert(relativeIndex, member);
                _memberMap[member] = root;
            }
        }
            
        bool existsAtIndex = root.Count > relativeIndex && root[relativeIndex] == member;
        if (!existsAtIndex)
        {
            root.Move(root.IndexOf(member), relativeIndex);
        }
    }
}
