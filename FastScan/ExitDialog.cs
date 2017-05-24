using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace FastScan
{
    public class ExitDialog
    {
        string _title = "Back pressed";
        string _content = "Exit application?";

        public ExitDialog()
        {

        }
        public ExitDialog(string con, string titl)
        {
            _title = titl;
            _content = con;
        }
        public async System.Threading.Tasks.Task<bool> exitDialog()
        {
            Class1.doLog("exitDialog started");

            bool bDoExit = true;
            UICommand yesCommand = new UICommand("Yes", cmd => { bDoExit = true; });
            UICommand noCommand = new UICommand("No", cmd => { bDoExit = false; });

            Windows.UI.Popups.MessageDialog dlg = new Windows.UI.Popups.MessageDialog(_content, _title);
            dlg.Options = MessageDialogOptions.None;
            dlg.Commands.Add(yesCommand);

            if (noCommand != null)
            {
                dlg.Commands.Add(noCommand);
                dlg.CancelCommandIndex = (uint)dlg.Commands.Count - 1;
            }
            dlg.DefaultCommandIndex = 0;
            dlg.CancelCommandIndex = 1;
            var command = await dlg.ShowAsync(); //already sets bDoExit
            if (command == yesCommand) {
                bDoExit = true;
            }
            if (command == noCommand) {
                bDoExit = false;
            }

            return bDoExit;

        }
    }
}
