using System.Collections.Generic;
using System.Linq;
using Come.CollectiveOAuth.Cache;
using Come.CollectiveOAuth.Config;
using Come.CollectiveOAuth.Enums;
using Come.CollectiveOAuth.Request;
using Come.CollectiveOAuth.Utils;
using Microsoft.Extensions.Configuration;

namespace Come.AspNetCore.Sample
{
    public class AuthRequestFactory
    {
        private IConfiguration configuration;
        public AuthRequestFactory() { }

        public AuthRequestFactory(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        #region 从appsettings.json中获取默认配置（可以改造成从数据库中读取）
        public Dictionary<string, ClientConfig> _clientConfigs =new Dictionary<string, ClientConfig>();

        public Dictionary<string, ClientConfig> ClientConfigs
        {
            get
            {
                if (_clientConfigs == null || _clientConfigs.Count <= 0)
                {
                    var _defaultPrefix = "OAuthConfig:";
 
                    var defaultAuthList = typeof(DefaultAuthSourceEnum).ToList().Select(a => a.Name.ToUpper()).ToList();
                    foreach (var authSource in defaultAuthList)
                    {
                        //var clientConfig = new ClientConfig
                        //{
                        //    clientId = configuration[$"{_defaultPrefix}{authSource}_ClientId"],
                        //    clientSecret = configuration.GetValue<string>($"{_defaultPrefix}{authSource}_ClientSecret"),
                        //    redirectUri = configuration.GetValue<string>($"{_defaultPrefix}{authSource}_RedirectUri"),
                        //    alipayPublicKey = configuration.GetValue<string>($"{_defaultPrefix}{authSource}_AlipayPublicKey"),
                        //    unionId = configuration.GetValue<string>($"{_defaultPrefix}{authSource}_UnionId"),
                        //    stackOverflowKey = configuration.GetValue<string>($"{_defaultPrefix}{authSource}_StackOverflowKey"),
                        //    agentId = configuration.GetValue<string>($"{_defaultPrefix}{authSource}_AgentId"),
                        //    scope = configuration.GetValue<string>($"{_defaultPrefix}{authSource}_Scope")
                        //};
                        var clientConfig = configuration.GetSection($"{_defaultPrefix}{authSource}").Get<ClientConfig>();

                        _clientConfigs.Add(authSource, clientConfig);
                    }
                }
                return _clientConfigs;
            }
        }


        public ClientConfig GetClientConfig(string authSource)
        {
            if (authSource.IsNullOrWhiteSpace())
            {
                return null;
            }

            if (!ClientConfigs.ContainsKey(authSource))
            {
                return null;
            }
            else
            {
                return ClientConfigs[authSource];
            }
        }
        #endregion

        /// <summary>
        /// 返回AuthRequest对象
        /// </summary>
        /// <param name="authSource"></param>
        /// <returns></returns>
        public IAuthRequest GetRequest(string authSource)
        {
            // 获取 CollectiveOAuth 中已存在的
            IAuthRequest authRequest = GetDefaultRequest(authSource);
            return authRequest;
        }

        /// <summary>
        /// 获取默认的 Request
        /// </summary>
        /// <param name="authSource"></param>
        /// <returns>{@link AuthRequest}</returns>
        private IAuthRequest GetDefaultRequest(string authSource)
        {
            ClientConfig clientConfig = GetClientConfig(authSource);
            IAuthStateCache authStateCache = new DefaultAuthStateCache();

            DefaultAuthSourceEnum authSourceEnum = GlobalAuthUtil.enumFromString<DefaultAuthSourceEnum>(authSource);

            return authSourceEnum switch
            {
                DefaultAuthSourceEnum.WECHAT_MP => new WeChatMpAuthRequest(clientConfig, authStateCache),
                DefaultAuthSourceEnum.WECHAT_OPEN => new WeChatOpenAuthRequest(clientConfig, authStateCache),
                DefaultAuthSourceEnum.WECHAT_ENTERPRISE => new WeChatEnterpriseAuthRequest(clientConfig, authStateCache),
                DefaultAuthSourceEnum.WECHAT_ENTERPRISE_SCAN => new WeChatEnterpriseScanAuthRequest(clientConfig, authStateCache),
                DefaultAuthSourceEnum.ALIPAY_MP => new AlipayMpAuthRequest(clientConfig, authStateCache),
                DefaultAuthSourceEnum.GITEE => new GiteeAuthRequest(clientConfig, authStateCache),
                DefaultAuthSourceEnum.GITHUB => new GithubAuthRequest(clientConfig, authStateCache),
                DefaultAuthSourceEnum.BAIDU => new BaiduAuthRequest(clientConfig, authStateCache),
                DefaultAuthSourceEnum.XIAOMI => new XiaoMiAuthRequest(clientConfig, authStateCache),
                //case DefaultAuthSourceEnum.DINGTALK_SCAN:
                //    return new DingTalkScanAuthRequest(clientConfig, authStateCache);
                DefaultAuthSourceEnum.OSCHINA => new OschinaAuthRequest(clientConfig, authStateCache),
                DefaultAuthSourceEnum.CODING => new CodingAuthRequest(clientConfig, authStateCache),
                DefaultAuthSourceEnum.LINKEDIN => new LinkedInAuthRequest(clientConfig, authStateCache),
                DefaultAuthSourceEnum.WEIBO => new WeiboAuthRequest(clientConfig, authStateCache),
                DefaultAuthSourceEnum.QQ => new QQAuthRequest(clientConfig, authStateCache),
                DefaultAuthSourceEnum.DOUYIN => new DouyinAuthRequest(clientConfig, authStateCache),
                DefaultAuthSourceEnum.GOOGLE => new GoogleAuthRequest(clientConfig, authStateCache),
                DefaultAuthSourceEnum.FACEBOOK => new FackbookAuthRequest(clientConfig, authStateCache),
                DefaultAuthSourceEnum.MICROSOFT => new MicrosoftAuthRequest(clientConfig, authStateCache),
                DefaultAuthSourceEnum.TOUTIAO => new ToutiaoAuthRequest(clientConfig, authStateCache),
                DefaultAuthSourceEnum.TEAMBITION => new TeambitionAuthRequest(clientConfig, authStateCache),
                DefaultAuthSourceEnum.RENREN => new RenrenAuthRequest(clientConfig, authStateCache),
                DefaultAuthSourceEnum.PINTEREST => new PinterestAuthRequest(clientConfig, authStateCache),
                DefaultAuthSourceEnum.STACK_OVERFLOW => new StackOverflowAuthRequest(clientConfig, authStateCache),
                DefaultAuthSourceEnum.HUAWEI => new HuaweiAuthRequest(clientConfig, authStateCache),
                DefaultAuthSourceEnum.KUJIALE => new KujialeAuthRequest(clientConfig, authStateCache),
                DefaultAuthSourceEnum.GITLAB => new GitlabAuthRequest(clientConfig, authStateCache),
                DefaultAuthSourceEnum.MEITUAN => new MeituanAuthRequest(clientConfig, authStateCache),
                DefaultAuthSourceEnum.ELEME => new ElemeAuthRequest(clientConfig, authStateCache),
                _ => null,
            };
        }
    }
}