using PixiEditor.ChangeableDocument.Actions.Properties;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditorPrototype.Models;
using System;
using System.ComponentModel;

namespace PixiEditorPrototype.ViewModels
{
    internal abstract class StructureMemberViewModel : INotifyPropertyChanged
    {
        private IReadOnlyStructureMember member;
        public event PropertyChangedEventHandler? PropertyChanged;
        public DocumentViewModel Document { get; }
        private DocumentHelpers Helpers { get; }

        public string Name
        {
            get => member.Name;
            set => Helpers.ActionAccumulator.AddAction(new SetStructureMemberName_Action(value, member.GuidValue));
        }

        public bool IsVisible
        {
            get => member.IsVisible;
            set => Helpers.ActionAccumulator.AddAction(new SetStructureMemberVisibility_Action(value, member.GuidValue));
        }

        public bool IsSelected { get; set; }

        public float Opacity
        {
            get => member.Opacity;
        }

        public Guid GuidValue
        {
            get => member.GuidValue;
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
            Helpers.ActionAccumulator.AddAction(new EndOpacityChange_Action());
        }

        private void UpdateOpacity(object? opacity)
        {
            if (opacity is not double value)
                throw new ArgumentException("The passed value isn't a double");
            Helpers.ActionAccumulator.AddAction(new OpacityChange_Action(GuidValue, (float)value));
        }
    }
}
