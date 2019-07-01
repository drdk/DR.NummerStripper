using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using DR.NummerStripper.Annotations;

namespace DR.NummerStripper
{
    // taken from https://github.com/gitextensions/gitextensions/pull/5746/files#diff-9cf55cacfb6ee2048f22ba8e4f552257R6
    public static class ClipboardUtil
    {
        public static bool TrySetText([NotNull] string text, [CallerMemberName] string memberName = "")
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            try
            {
                Debug.WriteLine($"{memberName} ClipboardUtil.TrySetText to {text}");
                Clipboard.SetDataObject(
                    text,
                    copy: false,
                    retryTimes: 5,
                    retryDelay: 100);

                return true;
            }
            catch (ExternalException e)
            {
                Debug.WriteLine(e.Message);
                // The clipboard is being used by another process
                return false;
            }
        }
    }
}
