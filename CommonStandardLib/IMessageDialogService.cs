using System.Threading.Tasks;

namespace CommonStandardLib
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
