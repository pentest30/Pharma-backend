﻿namespace GHPCommerce.WebApi.ConfigurationOptions
{
    public class CORS
    {
        public bool AllowAnyOrigin { get; set; }

        public string[] AllowedOrigins { get; set; }
    }
}
