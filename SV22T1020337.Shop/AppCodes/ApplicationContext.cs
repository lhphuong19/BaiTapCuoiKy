using Newtonsoft.Json;

namespace SV22T1020337.Shop
{
    /// <summary>
    /// Lớp cung cấp các tiện ích liên quan đến ngữ cảnh của ứng dụng web
    /// </summary>
    public static class ApplicationContext
    {
        private static IHttpContextAccessor? _httpContextAccessor;
        private static IWebHostEnvironment?  _webHostEnvironment;
        private static IConfiguration?       _configuration;

        public static void Configure(IHttpContextAccessor httpContextAccessor,
                                     IWebHostEnvironment  webHostEnvironment,
                                     IConfiguration       configuration)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _webHostEnvironment  = webHostEnvironment  ?? throw new ArgumentNullException(nameof(webHostEnvironment));
            _configuration       = configuration       ?? throw new ArgumentNullException(nameof(configuration));
        }

        public static HttpContext?          HttpContext      => _httpContextAccessor?.HttpContext;
        public static IWebHostEnvironment?  WebHostEnviroment => _webHostEnvironment;
        public static IConfiguration?       Configuration    => _configuration;

        public static string BaseURL        => $"{HttpContext?.Request.Scheme}://{HttpContext?.Request.Host}/";
        public static string WWWRootPath    => _webHostEnvironment?.WebRootPath    ?? string.Empty;
        public static string ApplicationRootPath => _webHostEnvironment?.ContentRootPath ?? string.Empty;

        public static int    PageSize  => Convert.ToInt32(GetConfigValue("PageSize"));
        public static string AppName   => GetConfigValue("AppName");

        public static string GetConfigValue(string name) => _configuration?[name] ?? "";

        public static T GetConfigSection<T>(string name) where T : new()
        {
            var value = new T();
            _configuration?.GetSection(name).Bind(value);
            return value;
        }

        public static void SetSessionData(string key, object value)
        {
            try
            {
                string json = JsonConvert.SerializeObject(value);
                if (!string.IsNullOrEmpty(json))
                    _httpContextAccessor?.HttpContext?.Session.SetString(key, json);
            }
            catch { }
        }

        public static T? GetSessionData<T>(string key) where T : class
        {
            try
            {
                string json = _httpContextAccessor?.HttpContext?.Session.GetString(key) ?? "";
                return string.IsNullOrEmpty(json) ? null : JsonConvert.DeserializeObject<T>(json);
            }
            catch { return null; }
        }
    }
}
