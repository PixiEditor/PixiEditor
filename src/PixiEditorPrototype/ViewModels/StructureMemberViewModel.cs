using ChangeableDocument.Actions.Properties;
using ChangeableDocument.Changeables.Interfaces;
using System;
using System.ComponentModel;

namespace PixiEditorPrototype.ViewModels
{
    internal abstract class StructureMemberViewModel : INotifyPropertyChanged
    {
        private IReadOnlyStructureMember member;
        public event PropertyChangedEventHandler? PropertyChanged;
        public DocumentViewModel Document { get; }

        public string Name
        {
            get => member.ReadOnlyName;
            set => Document.ActionAccumulator.AddAction(new SetStructureMemberName_Action(value, member.ReadOnlyGuidValue));
        }

        public bool IsVisible
        {
            get => member.ReadOnlyIsVisible;
            set => Document.ActionAccumulator.AddAction(new SetStructureMemberVisibility_Action(value, member.ReadOnlyGuidValue));
        }

        public float Opacity
        {
            get => member.ReadOnlyOpacity;
        }

        public Guid GuidValue
        {
            get => member.ReadOnlyGuidValue;
        }

        public RelayCommand MoveUpCommand { get; }
        public RelayCommand MoveDownCommand { get; }

        public RelayCommand UpdateOpacityCommand { get; }

        public RelayCommand EndOpacityUpdateCommand { get; }

        public void RaisePropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public StructureMemberViewModel(DocumentViewModel doc, IReadOnlyStructureMember member)
        {
            this.member = member;
            Document = doc;
            MoveUpCommand = new(_ => Document.StructureHelper.MoveStructureMember(GuidValue, true));
            MoveDownCommand = new(_ => Document.StructureHelper.MoveStructureMember(GuidValue, false));
            UpdateOpacityCommand = new(UpdateOpacity);
            EndOpacityUpdateCommand = new(EndOpacityUpdate);
        }

        private void EndOpacityUpdate(object? opacity)
        {
            Document.ActionAccumulator.AddAction(new EndOpacityChange_Action());
        }

        private void UpdateOpacity(object? opacity)
        {
            if (opacity == null || opacity is not double value)
                throw new Exception("Can't update opacity");
            Document.ActionAccumulator.AddAction(new OpacityChange_Action(GuidValue, (float)value));
        }
    }
}
