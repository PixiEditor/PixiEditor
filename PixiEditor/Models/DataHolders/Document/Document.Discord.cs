using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixiEditor.Models.DataHolders
{
    public partial class Document
    {
        private readonly DateTime openedUtc = DateTime.UtcNow;

        public DateTime OpenedUTC
        {
            get => openedUtc;
        }
    }
}