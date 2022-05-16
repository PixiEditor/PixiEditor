using System;
using System.ComponentModel;
using PixiEditor.ChangeableDocument.Actions.Properties;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditorPrototype.Models;

namespace PixiEditorPrototype.ViewModels;

internal abstract class StructureMemberViewModel : INotifyPropertyChanged
{
    protected IReadOnlyStructureMember member;

    public event PropertyChangedEventHandler? PropertyChanged;
    protected DocumentViewModel Document { get; }
    protected DocumentHelpers Helpers { get; }

    public string Name
    {
        get => member.Name;
        set => Helpers.ActionAccumulator.AddFinishedActions(new SetStructureMemberName_Action(value, member.GuidValue));
    }

    public bool IsVisible
    {
        get => member.IsVisible;
        set => Helpers.ActionAccumulator.AddFinishedActions(new SetStructureMemberVisibility_Action(value, member.GuidValue));
    }

    public BlendMode BlendMode
    {
        get => member.BlendMode;
        set => Helpers.ActionAccumulator.AddFinishedActions(new SetStructureMemberBlendMode_Action(value, member.GuidValue));
    }

    public bool ClipToMemberBelowEnabled
    {
        get => member.ClipToMemberBelow;
        set => Helpers.ActionAccumulator.AddFinishedActions(new SetStructureMemberClipToMemberBelow_Action(value, member.GuidValue));
    }

    public bool IsSelected { get; set; }
    public bool ShouldDrawOnMask { get; set; }

    public float Opacity
    {
        get => member.Opacity;
    }

    public Guid GuidValue
    {
        get => member.GuidValue;
    }

    public bool HasMask
    {
        get => member.ReadOnlyMask is not null;
    }

    public RelayCommand MoveUpCommand { get; }
    public RelayCommand MoveDownCommand { get; }

    public RelayCommand UpdateOpacityCommand { get; }

    public RelayCommand EndOpacityUpdateCommand { get; }

    public void RaisePropertyChanged(string name)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public StructureMemberViewModel(DocumentViewModel doc, DocumentHelpers helpers, IReadOnlyStructureMember member)
    {
        this.member = member;
        Document = doc;
        Helpers = helpers;
        MoveUpCommand = new(_ => Helpers.StructureHelper.MoveStructureMember(GuidValue, false));
        MoveDownCommand = new(_ => Helpers.StructureHelper.MoveStructureMember(GuidValue, true));
        UpdateOpacityCommand = new(UpdateOpacity);
        EndOpacityUpdateCommand = new(EndOpacityUpdate);
    }

    private void EndOpacityUpdate(object? opacity)
    {
        Helpers.ActionAccumulator.AddFinishedActions(new EndOpacityChange_Action());
    }

    private void UpdateOpacity(object? opacity)
    {
        if (opacity is not double value)
            throw new ArgumentException("The passed value isn't a double");
        Helpers.ActionAccumulator.AddActions(new OpacityChange_Action(GuidValue, (float)value));
    }
}
