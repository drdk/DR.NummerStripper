using DR.Common.RESTClient;
using DR.NummerStripper.Annotations;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Net;
using System.Runtime.Caching;
using System.Runtime.CompilerServices;

namespace DR.NummerStripper.MU
{
    public class ProductionService : INotifyPropertyChanged
    {
        private readonly IJsonClient _jsonClient;
        private readonly ObjectCache _cache;
        public ProductionService()
        {
            _cache = MemoryCache.Default;
            _jsonClient = new JsonClient(true)
            {
                BaseURL = "https://www.dr.dk/mu-online/api/1.4"
            };
        }

        private class Result : IResult
        {
            public string ProductionNumber { get; set; }
            public ProgramCard ProgramCard { get; set; }
            public Image Image { get; set; }
        }

        private IResult _current;
        public IResult Current
        {
            get => _current;
            private set
            {
                _current = value;
                OnPropertyChanged(nameof(Current));
            }
        }

        public void Cache(string prdNbr)
        {
            lock (_cache)
            {
                // load into cache.
                var temp = GetByProductionNumber(prdNbr);
                var tempImg = GetImage(temp?.PrimaryImageUri);
            }
        }

        public void Load(string prdNbr)
        {
            lock (_cache)
            {
                var pc = GetByProductionNumber(prdNbr);
                Current = new Result
                {
                    ProductionNumber = prdNbr,
                    ProgramCard = pc,
                    Image = GetImage(pc?.PrimaryImageUri)
                };
            }
        }

        private Image GetImage(Uri uri)
        {
            if (uri == null)
            {
                return null;
            }
            var ub = new UriBuilder(uri);
            ub.Query += "width=160&height=90";
            var key = ub.ToString();

            if (_cache.Contains(key))
            {
                return (Image)_cache.Get(key);
            }

            try
            {
                using (var wc = new WebClient())
                {
                    var stream = wc.OpenRead(ub.Uri);
                    if (stream != null)
                    {
                        var res = Image.FromStream(stream);
                        _cache.Add(key, res, DateTimeOffset.UtcNow.AddHours(1));
                        return res;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"{key} not found or error. ({e.Message})");

            }
            return null;
        }

        private ProgramCard GetByProductionNumber(string prdNbr)
        {
            if (_cache.Contains(prdNbr))
            {
                Debug.WriteLine($"{prdNbr} in cache.");
                return (ProgramCard)_cache.Get(prdNbr);
            }
            try
            {
                var res = _jsonClient.Get<ProgramCard>($"/programcard/?productionnumber={prdNbr}");
                Debug.WriteLine($"{prdNbr} found in mu-online");
                _cache.Add(prdNbr, res, DateTimeOffset.Now.AddMinutes(15));
                return res;
            }
            catch (Exception e)
            {
                Debug.WriteLine($"{prdNbr} not found or error. ({e.Message})");
                return null;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }

}
