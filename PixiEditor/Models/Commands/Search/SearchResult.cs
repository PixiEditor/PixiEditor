using GalaSoft.MvvmLight;
using PixiEditor.Helpers;
using PixiEditor.Models.DataHolders;
using System.Text.RegularExpressions;
using System.Windows.Documents;
using System.Windows.Media;

namespace PixiEditor.Models.Commands.Search
{
    public abstract class SearchResult : ObservableObject
    {
        public string SearchTerm { get; init; }

        public virtual Inline[] TextBlockContent => GetInlines().ToArray();

        public Match Match { get; init; }

        public abstract string Text { get; }

        public virtual string Description { get; }

        public abstract bool CanExecute { get; }

        public abstract ImageSource Icon { get; }

        public abstract void Execute();

        public virtual KeyCombination Shortcut { get; }

        public RelayCommand ExecuteCommand { get; }

        public SearchResult()
        {
            ExecuteCommand = new(_ => Execute(), _ => CanExecute);
        }

        private IEnumerable<Inline> GetInlines()
        {
            if (Match == null)
            {
                yield return new Run(Text);
                yield break;
            }

            foreach (Group group in Match.Groups.Values.Skip(1))
            {
                var run = new Run(group.Value);

                if (group.Value.Equals(SearchTerm, StringComparison.OrdinalIgnoreCase))
                {
                    yield return new Bold(run);
                }
                else
                {
                    yield return run;
                }
            }
        }
    }
}
