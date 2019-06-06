using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using WK.Libraries.SharpClipboardNS;

namespace DR.NummerStripper
{
    internal class TrayIconContext : ApplicationContext
    {
        private readonly NotifyIcon _trayIcon;
        private Assembly _assembly => typeof(TrayIconContext).GetTypeInfo().Assembly;
        
        private List<string> _history = new List<string>();

        private const int MaxItems = 8;
        private MenuItem _startCurrent;
        private MenuItem[] _baseItems;
        private SharpClipboard _clipboard;
        private readonly KeyboardHook _startHook;

        public TrayIconContext(string[] args)
        {
            _startCurrent = new MenuItem("Prøv at  &Starte nuværende indhold (Ctrl+Alt+Shift+S)", StartCurrent);
            if (!Clipboard.ContainsText() || !IsStartable(Clipboard.GetText()))
            {
                _startCurrent.Enabled = false;
            }
            _baseItems = new[]
            {
                new MenuItem("-"),
                _startCurrent,
                new MenuItem("&Om", About),
                new MenuItem("&Afslut", Exit),
            };

            _trayIcon = new NotifyIcon()
            {
                Icon = new System.Drawing.Icon(_assembly.GetManifestResourceStream("DR.NummerStripper.Icon.ico") ?? throw new InvalidOperationException()),
                ContextMenu = new ContextMenu(_baseItems),
                Visible = true,
                Text = "NumberStripper",
            };
            
            _clipboard = new SharpClipboard();
            _clipboard.ClipboardChanged += ClipboardChanged;
            _startHook = new KeyboardHook();
            _startHook.RegisterHotKey(ModifierKeys.Control | ModifierKeys.Alt | ModifierKeys.Shift, Keys.S);
            _startHook.KeyPressed += StartCurrent;
        }

        private void UpdateHistory(string text)
        {
            lock (_history)
            {
                if (_history.Contains(text))
                    return;

                _history.Add(text);

                while (_history.Count > MaxItems)
                {
                    _history.RemoveAt(0);
                }

                var c = 1;
                _trayIcon.ContextMenu = new ContextMenu(
                    _history.AsEnumerable().Reverse().Select(x => IsStartable(x) ? 
                        new MenuItem($"&{c++}: {x}", Start) : new MenuItem($"&{c++}: {x}", Copy)).Concat(_baseItems).ToArray());
            }
        }

        private bool IsStartable(string text)
        {
            if (File.Exists(text))
            {
                return true;
            }

            if (Directory.Exists(text))
            {
                return true;
            }

            if (Uri.IsWellFormedUriString(text, UriKind.Absolute))
            {
                return true;
            }

            return false;
        }

        void Exit(object sender, EventArgs e)
        {
            // Hide tray icon, otherwise it will remain shown until user mouses over it
            _trayIcon.Visible = false;
            _clipboard?.Dispose();
            _startHook?.Dispose();
            Application.Exit();
        }

        void About(object sender, EventArgs e)
        {
            var aboutText = string.Empty;
            var aboutCaption = string.Empty;

            if (Attribute.IsDefined(_assembly, typeof(AssemblyDescriptionAttribute)))
            {
                var a = (AssemblyDescriptionAttribute)Attribute.GetCustomAttribute(_assembly, typeof(AssemblyDescriptionAttribute));
                aboutText = a.Description;
            }

            if (Attribute.IsDefined(_assembly, typeof(AssemblyTitleAttribute)))
            {
                var a = (AssemblyTitleAttribute)Attribute.GetCustomAttribute(_assembly, typeof(AssemblyTitleAttribute));
                aboutCaption = a.Title;
            }

            MessageBox.Show(aboutText, aboutCaption, MessageBoxButtons.OK);
        }

        void Start(object sender, EventArgs e)
        {
            if (sender is MenuItem menuItem)
            {
                var process = System.Diagnostics.Process.Start(menuItem.Text.Substring(4));
            }
        }

        void StartCurrent(object sender, EventArgs e)
        {
            if (!Clipboard.ContainsText()) return;

            var text = Clipboard.GetText();

            if (IsStartable(text))
            {
                var process = System.Diagnostics.Process.Start(text);
            }
        }


        void Copy(object sender, EventArgs e)
        {
            if (sender is MenuItem menuItem)
            {
                Clipboard.SetText(menuItem.Text.Substring(4));
            }
        }

        private Regex EscapedUncPath = new Regex(@"^\\{4}", RegexOptions.Compiled);
        private Regex WhatsOnPrdNmr = new Regex(@"^[01]-\d{3}-\d{2}-\d{4}-\d$", RegexOptions.Compiled);
        
        private void ClipboardChanged(Object sender, SharpClipboard.ClipboardChangedEventArgs e)
        {
            // Is the content copied of text type?
            if (e.ContentType == SharpClipboard.ContentTypes.Text)
            {
                var text = _clipboard.ClipboardText;
                if (WhatsOnPrdNmr.IsMatch(text))
                {
                    Clipboard.SetText(text.Replace("-",String.Empty));
                }
                else if (EscapedUncPath.IsMatch(text))
                {
                    Clipboard.SetText(text.Replace(@"\\",@"\"));
                }

                text = Clipboard.GetText();
                UpdateHistory(text);
                _startCurrent.Enabled = IsStartable(text);
            }
        }
    }
}
