using Come.CollectiveOAuth.Models;
using Come.CollectiveOAuth.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;

namespace Come.AspNetCore.Sample.Controllers
{
    public class OAuth2Controller : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration configuration;
        public OAuth2Controller(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            this.configuration = configuration;
        }

        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 构建授权Url方法
        /// </summary>
        /// <param name="authSource"></param>
        /// <returns>RedirectUrl</returns>
        public IActionResult Authorization(string authSource)
        {
            AuthRequestFactory authRequest = new AuthRequestFactory(configuration);
            var request = authRequest.GetRequest(authSource);
            var authorize = request.authorize(AuthStateUtils.createState());
            Console.WriteLine(authorize);
            return Redirect(authorize);
        }

        /// <summary>
        /// 授权回调方法
        /// </summary>
        /// <param name="authSource"></param>
        /// <param name="authCallback"></param>
        /// <returns></returns>
        public IActionResult Callback(string authSource, AuthCallback authCallback)
        {
            AuthRequestFactory authRequest = new AuthRequestFactory(configuration);
            var request = authRequest.GetRequest(authSource);
            var authResponse = request.login(authCallback);
            return Content(JsonConvert.SerializeObject(authResponse));
        }

        /// <summary>
        /// 钉钉callback
        /// </summary>
        /// <param name="authSource"></param>
        /// <param name="authCallback"></param>
        /// <returns></returns>
        public IActionResult DingTalkCallback(AuthCallback authCallback)
        {
            AuthRequestFactory authRequest = new AuthRequestFactory(configuration);
            var request = authRequest.GetRequest("DINGTALK_SCAN");
            var authResponse = request.login(authCallback);
            return Content(JsonConvert.SerializeObject(authResponse));
        }
    }
}
