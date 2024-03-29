﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using DR.NummerStripper.MU;
using DR.NummerStripper.Properties;
using WK.Libraries.SharpClipboardNS;

namespace DR.NummerStripper
{
    internal class TrayIconContext : ApplicationContext
    {
        private readonly NotifyIcon _trayIcon;
        private readonly Assembly _assembly = typeof(TrayIconContext).GetTypeInfo().Assembly;
        
        private readonly List<string> _history = new List<string>();

        private const int MaxItems = 8;
        private readonly MenuItem _startCurrent;
        private readonly MenuItem _whatsOnModeToggle;
        private bool WhatsOnMode => _whatsOnModeToggle.Checked;
        private readonly MenuItem[] _baseItems;
        private readonly SharpClipboard _clipboard;
        private readonly KeyboardHook _startHook;
        private readonly KeyboardHook _whatsOnHook;

        private ProductionForm _productionForm = null;
        private readonly ProductionService _productionService;

        private string _lastText = string.Empty;


        public TrayIconContext()
        {
            _productionService = new ProductionService();
            
            _startCurrent = new MenuItem("Prøv at  &Starte nuværende indhold (Ctrl+Alt+Shift+S)", StartCurrent);
            _whatsOnModeToggle = new MenuItem("WhatsOn Mode (Ctrl+Alt+Shift+W)", ToggleWhatsOnMode);

            if (!Clipboard.ContainsText() || !SafeToStart(Clipboard.GetText()))
            {
                _startCurrent.Enabled = false;
            }
            _baseItems = new[]
            {
                new MenuItem("-"),
                _startCurrent,
                _whatsOnModeToggle,
                new MenuItem("&Om", About),
                new MenuItem("&Afslut", Exit),
            };

            var icon = new Icon(_assembly.GetManifestResourceStream("DR.NummerStripper.Icon.ico") ??
                                               throw new InvalidOperationException());
            _trayIcon = new NotifyIcon()
            {
                Icon = icon,
                ContextMenu = new ContextMenu(_baseItems),
                Visible = true,
                Text = "DR Udklipsholderhjælper",
            };

            _trayIcon.Click += Click;
            _trayIcon.BalloonTipClicked += StartCurrent;
            _clipboard = new SharpClipboard { ObservableFormats = { Texts = true, Images = false, Files = false, Others = false }};
            _clipboard.ClipboardChanged += ClipboardChanged;
            _clipboard.StartMonitoring();
            _startHook = new KeyboardHook();
            _startHook.RegisterHotKey(ModifierKeys.Control | ModifierKeys.Alt | ModifierKeys.Shift, Keys.S);
            _startHook.KeyPressed += StartCurrent;
            _whatsOnHook = new KeyboardHook();
            _whatsOnHook.RegisterHotKey(ModifierKeys.Control | ModifierKeys.Alt | ModifierKeys.Shift, Keys.W);
            _whatsOnHook.KeyPressed += ToggleWhatsOnMode;
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
        private string GetTextFromHistory(MenuItem menuItem) =>
            _history.AsEnumerable().Reverse().Skip(
                int.Parse(menuItem.Text.Substring(1, 1)) - 1).First();


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
                        text.IsProductionNumber());
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return false;
            }
        }

        private void ToggleWhatsOnMode(object sender, EventArgs e)
        {
            _whatsOnModeToggle.Checked = !WhatsOnMode;

            if (!Clipboard.ContainsText()) return;

            var text = Clipboard.GetText();

            if (!text.IsProductionNumber()) return;

            text = WhatsOnMode ? text.ToWhatsOnProductionNumber() : text.ToCleanProductionNumber();

            ClipboardUtil.TrySetText(text);
        }


        void Exit(object sender, EventArgs e)
        {
            // Hide tray icon, otherwise it will remain shown until user mouses over it
            _trayIcon.Visible = false;
            _clipboard?.Dispose();
            _startHook?.Dispose();
            _whatsOnHook?.Dispose();
            _productionForm?.Dispose();
            
            Application.Exit();
        }

        void About(object sender, EventArgs e)
        {
            var aboutText = string.Empty;
            var aboutCaption = string.Empty;

            if (Attribute.IsDefined(_assembly, typeof(AssemblyDescriptionAttribute)))
            {
                var a = (AssemblyDescriptionAttribute) Attribute.GetCustomAttribute(_assembly,
                    typeof(AssemblyDescriptionAttribute));

                aboutText = a.Description;
            }

            aboutText += $"{Environment.NewLine}Version: {_assembly.GetName().Version}";

            if (Attribute.IsDefined(_assembly, typeof(AssemblyTitleAttribute)))
            {
                var a = (AssemblyTitleAttribute)Attribute.GetCustomAttribute(_assembly,
                    typeof(AssemblyTitleAttribute));

                aboutCaption = a.Title;
            }

            MessageBox.Show(aboutText, aboutCaption, MessageBoxButtons.OK);
        }

        void Start(object sender, EventArgs e)
        {
            if (!(sender is MenuItem menuItem)) return;

            Start(GetTextFromHistory(menuItem));
            Copy(sender,e);
        }

        void StartOrUpdateProductionForm(string prdNbr)
        {
            _productionService.Load(prdNbr.ToCleanProductionNumber());
            if (_productionForm == null || _productionForm.IsDisposed)
            {
                _productionForm = new ProductionForm(_productionService);
            }
            _productionForm.Show();
            _productionForm.BringToFront();
            _productionForm.Activate();
        }

        void Start(string text)
        {
            if (SafeToStart(text))
            {
                if (text.IsProductionNumber())
                {
                    StartOrUpdateProductionForm(text);
                }
                else
                {
                    var process = Process.Start(text);
                    Debug.WriteLine($"{text} ({process?.Id})");
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
            if (!(sender is MenuItem menuItem)) return;
            var newText = GetTextFromHistory(menuItem);
            if (newText != Clipboard.GetText())
            {
                ClipboardUtil.TrySetText(newText);
            }
        }

        void Click(object sender, EventArgs e)
        {
            if (e is MouseEventArgs me && (me.Button & MouseButtons.Left) != 0)
            {
                StartCurrent(sender, e);
            }
        }



        private void UpdateIcon()
        {
            if (Clipboard.ContainsText())
            {
                var text = Clipboard.GetText();
                if (_lastText == text) return;
                _lastText = text = Clipboard.GetText();

                var safe = SafeToStart(text);

                _startCurrent.Enabled = safe;

                if (text.IsProductionNumber())
                {
                    _productionService.Cache(text.ToCleanProductionNumber());
                    if (Settings.Default.ShowBallonTips)
                        _trayIcon.ShowBalloonTip(2000, "Produktionsnummer", text, ToolTipIcon.Info);
                    _trayIcon.Icon = text.IsWhatsOnProductionNumber() ?
                        IconFactory.MakeOne('W', Brushes.DeepSkyBlue):
                        IconFactory.MakeOne('P', Brushes.Red);
                    if (_productionForm != null && !_productionForm.IsDisposed)
                    {
                        StartOrUpdateProductionForm(text);
                    }
                    return;
                } 
                if (safe)
                {
                    if (Settings.Default.ShowBallonTips)
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
            Debug.WriteLine(e.ContentType.ToString());
            // Is the content copied of text type?
            if (e.ContentType == SharpClipboard.ContentTypes.Text)
            {
                var text = (string) e.Content;
                Debug.WriteLine(text);
                if (text.IsProductionNumber())
                {
                    var newText = WhatsOnMode ? text.ToWhatsOnProductionNumber() : text.ToCleanProductionNumber();
                    if (newText != text) { ClipboardUtil.TrySetText(newText); }
                }
                else if (text.IsEscapedUncPath())
                {
                    ClipboardUtil.TrySetText(text.UnescapedUncPath());
                }

                text = Clipboard.GetText();
                UpdateHistory(text);
            }
            UpdateIcon();
        }
    }
}
