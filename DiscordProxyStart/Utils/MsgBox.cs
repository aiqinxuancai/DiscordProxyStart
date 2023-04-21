using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;


namespace DiscordProxyStart.Utils
{

    public enum MsgBoxButtons
    {
        OK = 0,
        OKCancel = 1,
        AbortRetryIgnore = 2,
        YesNoCancel = 3,
        YesNo = 4,
        RetryCancel = 5
    }

    public enum MsgBoxDialogResult
    {
        None = 0,
        OK = 1,
        Cancel = 2,
        Yes = 6,
        No = 7
    }

    internal class MsgBox
    {
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);




        public static MsgBoxDialogResult Show(string caption, string message, MsgBoxButtons buttons)
        {
            uint type = 0;
            switch (buttons)
            {
                case MsgBoxButtons.OK:
                    type = 0x00000000; // MB_OK
                    break;
                case MsgBoxButtons.OKCancel:
                    type = 0x00000001; // MB_OKCANCEL
                    break;
                case MsgBoxButtons.AbortRetryIgnore:
                    type = 0x00000002; // MB_ABORTRETRYIGNORE
                    break;
                case MsgBoxButtons.YesNoCancel:
                    type = 0x00000003; // MB_YESNOCANCEL
                    break;
                case MsgBoxButtons.YesNo:
                    type = 0x00000004; // MB_YESNO
                    break;
                case MsgBoxButtons.RetryCancel:
                    type = 0x00000005; // MB_RETRYCANCEL
                    break;
            }

            int result = MessageBox(IntPtr.Zero, message, caption, type);
            switch (result)
            {
                case 1: // IDOK
                    return MsgBoxDialogResult.OK;
                case 2: // IDCANCEL
                    return MsgBoxDialogResult.Cancel;
                case 6: // IDYES
                    return MsgBoxDialogResult.Yes;
                case 7: // IDNO
                    return MsgBoxDialogResult.No;
                default:
                    return MsgBoxDialogResult.None;
            }
        }
    }

}
