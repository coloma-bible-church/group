namespace Group.Twilio.Twilio
{
    using System;
    using System.Collections.Generic;
    using Common.Uris;
    using global::Twilio.AspNet.Common;

    [Serializable]
    public class MediaSmsRequest : SmsRequest
    {
        public int? NumMedia { get; set; }

        public string? MediaUrl0 { get; set; }

        public string? MediaUrl1 { get; set; }

        public string? MediaUrl2 { get; set; }

        public string? MediaUrl3 { get; set; }

        public string? MediaUrl4 { get; set; }

        public string? MediaUrl5 { get; set; }

        public string? MediaUrl6 { get; set; }

        public string? MediaUrl7 { get; set; }

        public string? MediaUrl8 { get; set; }

        public string? MediaUrl9 { get; set; }

        public IEnumerable<string> MediaUrls
        {
            get
            {
                if (UriChecker.IsValidAndSecure(MediaUrl0))
                    yield return MediaUrl0!;
                if (UriChecker.IsValidAndSecure(MediaUrl1))
                    yield return MediaUrl1!;
                if (UriChecker.IsValidAndSecure(MediaUrl2))
                    yield return MediaUrl2!;
                if (UriChecker.IsValidAndSecure(MediaUrl3))
                    yield return MediaUrl3!;
                if (UriChecker.IsValidAndSecure(MediaUrl4))
                    yield return MediaUrl4!;
                if (UriChecker.IsValidAndSecure(MediaUrl5))
                    yield return MediaUrl5!;
                if (UriChecker.IsValidAndSecure(MediaUrl6))
                    yield return MediaUrl6!;
                if (UriChecker.IsValidAndSecure(MediaUrl7))
                    yield return MediaUrl7!;
                if (UriChecker.IsValidAndSecure(MediaUrl8))
                    yield return MediaUrl8!;
                if (UriChecker.IsValidAndSecure(MediaUrl9))
                    yield return MediaUrl9!;
            }
        }
    }
}