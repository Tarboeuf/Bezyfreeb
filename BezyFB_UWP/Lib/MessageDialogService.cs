using CommonPortableLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace BezyFB_UWP.Lib
{
    public class MessageDialogService : IMessageDialogService
    {
        
        public async Task AfficherMessage(string message)
        {
            var md = new MessageDialog(message);
            await md.ShowAsync();
        }

        public async Task<DialogResult> ShowYesNoDialog(string content)
        {

            var dialog = new MessageDialog(content, "Question");

            dialog.Commands.Add(new Windows.UI.Popups.UICommand("Yes") { Id = DialogResult.Yes });
            dialog.Commands.Add(new Windows.UI.Popups.UICommand("No") { Id = DialogResult.No });

            //if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily != "Windows.Mobile")
            //{
            //    // Adding a 3rd command will crash the app when running on Mobile !!!
            //    dialog.Commands.Add(new Windows.UI.Popups.UICommand("Maybe later") { Id = 2 });
            //}

            dialog.DefaultCommandIndex = 0;
            dialog.CancelCommandIndex = 1;

            var result = await dialog.ShowAsync();
            return (DialogResult)result.Id;
        }
    }
}
