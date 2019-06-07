﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
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
        private ProductionForm _productionForm = null;


        public TrayIconContext(string[] args)
        {
            _startCurrent = new MenuItem("Prøv at  &Starte nuværende indhold (Ctrl+Alt+Shift+S)", StartCurrent);
            if (!Clipboard.ContainsText() || !SafeToStart(Clipboard.GetText()))
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

            var icon = new System.Drawing.Icon(_assembly.GetManifestResourceStream("DR.NummerStripper.Icon.ico") ??
                                               throw new InvalidOperationException());
            _trayIcon = new NotifyIcon()
            {
                Icon = icon,
                ContextMenu = new ContextMenu(_baseItems),
                Visible = true,
                Text = "NummerStripper",
            };

            _trayIcon.Click += Click;
            _trayIcon.BalloonTipClicked += StartCurrent;
            _clipboard = new SharpClipboard();
            _clipboard.ClipboardChanged += ClipboardChanged;
            _startHook = new KeyboardHook();
            _startHook.RegisterHotKey(ModifierKeys.Control | ModifierKeys.Alt | ModifierKeys.Shift, Keys.S);
            _startHook.KeyPressed += StartCurrent;
        }

        private void UpdateHistory(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;
            
            lock (_history)
            {
                if (_history.Contains(text)) return;

                _history.Add(text);

                while (_history.Count > MaxItems) _history.RemoveAt(0);

                var c = 1;
                _trayIcon.ContextMenu = new ContextMenu(
                    _history.AsEnumerable().Reverse().Select(x => SafeToStart(x) ? 
                        new MenuItem($"&{c++}: {Crop(x)}", Start) : new MenuItem($"&{c++}: {Crop(x)}", Copy)).Concat(_baseItems).ToArray());
            }
        }

        private string Crop(string text) => 
            text.Length > 96 ? $"{text.Substring(0, 96)}..." : text;

        private bool SafeToStart(string text)
        {
            try
            {
                return !string.IsNullOrWhiteSpace(text) &&
                       (File.Exists(text) ||
                        Directory.Exists(text) ||
                        Uri.IsWellFormedUriString(text, UriKind.Absolute) || 
                        PrdNmr.IsMatch(text));
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return false;
            }
        }


        void Exit(object sender, EventArgs e)
        {
            // Hide tray icon, otherwise it will remain shown until user mouses over it
            _trayIcon.Visible = false;
            _clipboard?.Dispose();
            _startHook?.Dispose();
            _productionForm?.Dispose();
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

        private string GetTextFromHistory(MenuItem menuItem) =>
            _history.AsEnumerable().Reverse().Skip(
                int.Parse(menuItem.Text.Substring(1,1)) - 1 ).First();

        void Start(object sender, EventArgs e)
        {
            if (!(sender is MenuItem menuItem)) return;

            Start(GetTextFromHistory(menuItem));
            Copy(sender,e);
        }

        void StartOrUpdateProductionForm(string prdNbr)
        {
            if (_productionForm == null || _productionForm.IsDisposed)
            {
                _productionForm = new ProductionForm(prdNbr);
            }
            else
            {
                _productionForm.PrdNbr = prdNbr;
            }
            _productionForm.Show();
            _productionForm.BringToFront();
            _productionForm.Activate();
        }

        void Start(string text)
        {
            if (SafeToStart(text))
            {
                if (PrdNmr.IsMatch(text))
                {
                    StartOrUpdateProductionForm(text);
                }
                else
                {
                    var process = Process.Start(text);
                }
            }
        }

        void StartCurrent(object sender, EventArgs e)
        {
            if (!Clipboard.ContainsText()) return;

            var text = Clipboard.GetText();
            Start(text);
        }


        void Copy(object sender, EventArgs e)
        {
            if (sender is MenuItem menuItem)
            {
                Clipboard.SetText(GetTextFromHistory(menuItem));
            }
        }

        void Click(object sender, EventArgs e)
        {
            var me = (e as MouseEventArgs);
            if ((me.Button & MouseButtons.Left) != 0)
            {
                StartCurrent(sender, e);
            }
        }

        private Regex EscapedUncPath = new Regex(@"^\\{4}", RegexOptions.Compiled);
        private Regex WhatsOnPrdNmr = new Regex(@"^[01]-\d{3}-\d{2}-\d{4}-\d$", RegexOptions.Compiled);
        private Regex PrdNmr = new Regex(@"[01]\d{9}", RegexOptions.Compiled);
        private string _lastText = string.Empty;

        private void UpdateIcon()
        {
            if (Clipboard.ContainsText())
            {
                var text = Clipboard.GetText();
                if (_lastText == text) return;
                _lastText = text = Clipboard.GetText();

                var safe = SafeToStart(text);

                _startCurrent.Enabled = safe;

                if (PrdNmr.IsMatch(text))
                {
                    _trayIcon.ShowBalloonTip(2000, "Produktionsnummer", text, ToolTipIcon.Info);
                    _trayIcon.Icon = IconFactory.MakeOne('P', Brushes.Red);
                    if (_productionForm != null && !_productionForm.IsDisposed)
                    {
                        StartOrUpdateProductionForm(text);
                    }
                    return;
                }
                if (safe)
                {
                    _trayIcon.ShowBalloonTip(2000, "Startbar", text, ToolTipIcon.Info);
                    _trayIcon.Icon = IconFactory.MakeOne('S', Brushes.GreenYellow);
                    return;
                }

            }
            else
            {
                _startCurrent.Enabled = false;
            }
            _trayIcon.Icon = IconFactory.MakeOne('X', Brushes.AntiqueWhite);
        }

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
                UpdateIcon();
            }
        }
    }
}
