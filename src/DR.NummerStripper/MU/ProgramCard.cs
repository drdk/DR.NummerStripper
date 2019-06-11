using System;

namespace DR.NummerStripper.MU
{
    public class ProgramCard
    {
        public string Title { get; set; }
        public string ProductionNumber { get; set; }
        public string Urn { get; set; }
        public string Slug { get; set; }
        public string SeriesSlug { get; set; }
        public string SeasonSlug { get; set; }
        public Channel ChannelType { get; set; }
        public Uri PrimaryImageUri { get; set; }
        public Uri PresentationUri { get; set; }
        public string PrimaryChannel { get; set; }
        public string PrimaryChannelSlug { get; set; }
        public string Description { get; set; }
        public DateTime? PrimaryBroadcastStartTime { get; set; }
    }
}