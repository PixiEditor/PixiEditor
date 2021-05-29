using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixiEditor.Helpers
{
    public class FileFilterBuilder
    {
        private List<(string, string[])> filters = new List<(string, string[])>();

        public void AddFilter(string name, params string[] fileExtensions)
        {
            if (fileExtensions != null && !fileExtensions.Any())
            {
                return;
            }

            filters.Add((name, fileExtensions));
        }

        public void AddFilter(string name, IEnumerable<string> fileExtensions)
        {
            AddFilter(name, fileExtensions.ToArray());
        }

        public string Build(bool includeAny, string anyName = "Any")
        {
            if (filters.Count == 0)
            {
                return string.Empty;
            }

            StringBuilder builder = new StringBuilder();

            if (includeAny)
            {
                builder.Append(BuildFilter(anyName, filters.SelectMany(x => x.Item2).ToArray()));
                builder.Append('|');
            }

            builder.Append(BuildFilter(filters[0].Item1, filters[0].Item2));

            foreach (var tuple in filters.Skip(1))
            {
                builder.Append('|');
                builder.Append(BuildFilter(tuple.Item1, tuple.Item2));
            }

            return builder.ToString();
        }

        public override string ToString() => Build(false);

        private static StringBuilder BuildFilter(string name, string[] extensions)
        {
            StringBuilder builder = new StringBuilder();

            builder.Append(name);

            builder.Append('|');

            builder.Append(BuildExtension(extensions));

            return builder;
        }

        private static StringBuilder BuildExtension(string[] extensions)
        {
            StringBuilder builder = new StringBuilder();

            foreach (string extension in extensions)
            {
                builder.Append('*');

                if (!extension.StartsWith('.'))
                {
                    builder.Append('.');
                }

                builder.Append(extension);
                builder.Append("; ");
            }

            return builder;
        }
    }
}
