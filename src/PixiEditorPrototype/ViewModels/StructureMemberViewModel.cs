using ChangeableDocument.Actions;
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
            get => member.Name;
            set => Document.ActionAccumulator.AddAction(new SetStructureMemberName_Action(value, member.GuidValue));
        }

        public bool IsVisible
        {
            get => member.IsVisible;
            set => Document.ActionAccumulator.AddAction(new SetStructureMemberVisibility_Action(value, member.GuidValue));
        }

        public float Opacity
        {
            get => member.Opacity;
            set => SetOpacity(value);
        }

        public Guid GuidValue
        {
            get => member.GuidValue;
        }

        public RelayCommand MoveUpCommand { get; }
        public RelayCommand MoveDownCommand { get; }

        public RelayCommand UpdateOpacityCommand { get; }

        public RelayCommand EndOpacityUpdateCommand { get; }

        public RelayCommand SetOpacityCommand { get; }

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
            SetOpacityCommand = new((value) => { if (value != null) SetOpacity((float)(double)value); });
        }

        private void EndOpacityUpdate(object? opacity)
        {
            Document.ActionAccumulator.AddAction(new EndOpacityChange_Action());
        }

        private void UpdateOpacity(object? opacity)
        {
            if (opacity != null)
                Document.ActionAccumulator.AddAction(new OpacityChange_Action(GuidValue, (float)(double)opacity));
        }

        private void SetOpacity(float value)
        {
            //Document.ActionAccumulator.AddAction(new OpacityChange_Action(GuidValue, value));
            //Document.ActionAccumulator.AddAction(new EndOpacityChange_Action());
        }
    }
}
