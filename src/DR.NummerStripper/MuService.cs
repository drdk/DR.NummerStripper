using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DR.Common.RESTClient;

namespace DR.NummerStripper
{
    public class MuService
    {
        private readonly IJsonClient _jsonClient;
        public MuService()
        {
            _jsonClient = new JsonClient(true) { BaseURL = "https://www.dr.dk/mu-online/api/1.4" };
         }

        public ProgramCard GetByProductionNumber(string prdNbr)
        {
            try
            {
                return _jsonClient.Get<ProgramCard>($"/programcard/?productionnumber={prdNbr}");
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return null;
            }
        }


        public class ProgramCard
        {
            public string Title { get; set; }
            public string ProductionNumber { get; set; }
            public string Urn { get; set; }
            public string Slug { get; set; }
            public string SeriesSlug { get; set; }
            public string SeasonSlug { get; set; }
            public Channel ChannelType { get; set; }
            public Uri PrimaryImageUri  { get; set; }
            private Uri _presentationUri;

            public Uri PresentationUri
            {
                get
                {
                    if (_presentationUri == null)
                        _presentationUri = new Uri($"https://www.dr.dk/tv/se/{SeriesSlug??"-"}/{SeasonSlug??"-"}/{Slug}");
                    return _presentationUri;
                }
                set => _presentationUri = value;
            }

            public string Description { get; set; }
            public DateTime? PrimaryBroadcastStartTime { get; set; }
        }

        public enum Channel
        {
            UNKNOWN =0,
            TV,
            RADIO
        }
    }
}
