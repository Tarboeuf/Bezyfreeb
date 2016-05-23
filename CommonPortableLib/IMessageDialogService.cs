using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonPortableLib
{
    public interface IMessageDialogService
    {
        Task AfficherMessage(string v);
        Task<DialogResult> ShowYesNoDialog(string v);
    }

    public enum DialogResult
    {
        Yes,
        No,
    }

    public enum DialogOption
    {
        YesNo,
        Ok,
    }
}
