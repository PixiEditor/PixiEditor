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

        public Guid GuidValue
        {
            get => member.GuidValue;
        }

        public RelayCommand MoveUpCommand { get; }
        public RelayCommand MoveDownCommand { get; }

        public void RaisePropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public StructureMemberViewModel(DocumentViewModel doc, IReadOnlyStructureMember member)
        {
            this.member = member;
            Document = doc;
            MoveUpCommand = new(_ => Document.StructureHelper.MoveStructureMember(GuidValue, true), _ => true);
            MoveDownCommand = new(_ => Document.StructureHelper.MoveStructureMember(GuidValue, false), _ => true);
        }
    }
}
