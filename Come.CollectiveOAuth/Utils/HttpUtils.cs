using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading;

namespace Come.CollectiveOAuth.Utils
{
    public class HttpUtils
    {
        /// <summary>
        /// 模拟Form表单post请求
        /// </summary>
        /// <param name="postUrl"></param>
        /// <param name="postData"></param>
        /// <param name="header"></param>
        /// <param name="charset"></param>
        /// <returns></returns>
        public static string RequestFormPost(string postUrl, string postData = null, Dictionary<string, object> header = null, string charset = null)
        {
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12; //加上这一句
            if (string.IsNullOrWhiteSpace(charset))
                charset = "UTF-8";

            if (string.IsNullOrWhiteSpace(postData))
                postData = "";

            Stream outstream = null;
            Stream instream = null;
            StreamReader sr = null;
            HttpWebResponse response = null;
            HttpWebRequest request = null;
            Encoding encoding = System.Text.Encoding.GetEncoding(charset);

            byte[] data = encoding.GetBytes(postData);
            // 准备请求...
            try
            {
                // 设置参数
                request = WebRequest.Create(postUrl) as HttpWebRequest;
                CookieContainer cookieContainer = new CookieContainer();
                request.CookieContainer = cookieContainer;
                request.AllowAutoRedirect = true;
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded;charset=" + charset.ToLower();
                request.ContentLength = data.Length;

                ComeSetRequestHeader(request, header);

                outstream = request.GetRequestStream();
                outstream.Write(data, 0, data.Length);
                outstream.Close();
                //发送请求并获取相应回应数据
                response = request.GetResponse() as HttpWebResponse;
                //直到request.GetResponse()程序才开始向目标网页发送Post请求
                instream = response.GetResponseStream();
                sr = new StreamReader(instream, encoding);
                //返回结果网页（html）代码
                string content = sr.ReadToEnd();
                string err = string.Empty;
                return content;
            }
            catch (Exception ex)
            {
                string err = ex.Message;
                return string.Empty;
            }
        }


        /// <summary>
        /// 普通post请求
        /// </summary>
        /// <param name="postUrl"></param>
        /// <param name="postData"></param>
        /// <param name="header"></param>
        /// <param name="charset"></param>
        /// <returns></returns>
        public static string RequestPost(string postUrl, string postData = null, Dictionary<string, object> headers = null, string charset = null)
        {
            //System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12; //加上这一句

            try
            {
                var contentType = "application/json";
                postData ??= "";
                using HttpClient client = new HttpClient();
                client.Timeout = new TimeSpan(0, 0, 30);
                if (headers != null)
                {
                    foreach (var header in headers)
                        client.DefaultRequestHeaders.Add(header.Key, Convert.ToString(header.Value));
                }

                using (HttpContent httpContent = new StringContent(postData, Encoding.UTF8))
                {
                    if (contentType != null)
                        httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);

                    HttpResponseMessage response = client.PostAsync(postUrl, httpContent).Result;
                    return response.Content.ReadAsStringAsync().Result;
                }
            }
            catch (Exception ex)
            {
                string err = ex.Message;
                Console.WriteLine(postUrl);
                Console.WriteLine(err);
                return string.Empty;
            }
        }

        /// <summary>
        /// 有一些请求比较特殊，需要标记Accept为application/json
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string RequestJsonGet(string url, Dictionary<string, object> headers = null)
        {
            //StringBuilder builder = new StringBuilder();
            //builder.Append(url);
            //HttpWebRequest request = (HttpWebRequest)WebRequest.Create(builder.ToString());
            //request.UserAgent = "Foo";
            //request.Accept = "application/json";
            //ComeSetRequestHeader(request, header);
            //return ComeRequestGet(request);
            using HttpClient client = new HttpClient();
            if (headers != null)
            {
                foreach (var header in headers)
                    client.DefaultRequestHeaders.Add(header.Key, Convert.ToString(header.Value));
            }
            else
            {
                client.DefaultRequestHeaders.Add("ContentType", "application/json");
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
            }
            try
            {
                HttpResponseMessage response = client.GetAsync(url).Result;
                return response.Content.ReadAsStringAsync().Result;
            }
            catch (Exception ex)
            {
                //TODO 打印日志
                Console.WriteLine($"[Http请求出错]{url}|{ex.Message}");
            }
            return "";
        }

        /// <summary>
        /// 发送Get请求
        /// </summary>
        /// <param name="url">地址</param>
        /// <param name="dic">请求参数定义</param>
        public static string RequestGet(string url, Dictionary<string, object> header = null)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(url);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(builder.ToString());
            request.ContentType = "application/json;charset=utf-8;";
            ComeSetRequestHeader(request, header);
            return ComeRequestGet(request);
        }


        /// <summary>
        /// 通用的设置RequestHeader方法
        /// </summary>
        /// <param name="request"></param>
        /// <param name="header"></param>
        public static void ComeSetRequestHeader(HttpWebRequest request, Dictionary<string, object> header = null)
        {
            if (header != null && header.Count > 0)
            {
                foreach (var item in header)
                {
                    switch (item.Key.ToUpper())
                    {
                        case "HOST":
                            request.Host = Convert.ToString(item.Value);
                            break;
                        case "CONTENT-TYPE":
                            request.ContentType = Convert.ToString(item.Value);
                            break;
                        case "CONNECTION":
                            SetSpecialHeaderValue(request.Headers, item.Key, Convert.ToString(item.Value));
                            break;
                        default:
                            request.Headers.Add(item.Key, Convert.ToString(item.Value));
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// 设置特殊的header
        /// </summary>
        /// <param name="header"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public static void SetSpecialHeaderValue(WebHeaderCollection header, string name, string value)
        {
            var property = typeof(WebHeaderCollection).GetProperty("InnerCollection",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (property != null)
            {
                var collection = property.GetValue(header, null) as NameValueCollection;
                collection[name] = value;
            }
        }


        /// <summary>
        /// 通用的get请求
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static string ComeRequestGet(HttpWebRequest request)
        {
            string result = "";
            request.AllowAutoRedirect = true;
            request.Method = "GET";
            request.CookieContainer = new CookieContainer();
            request.Credentials = CredentialCache.DefaultCredentials;
            HttpWebResponse resp = (HttpWebResponse)request.GetResponse();
            Stream stream = resp.GetResponseStream();
            try
            {
                //获取内容
                using (StreamReader reader = new StreamReader(stream))
                {
                    result = reader.ReadToEnd();
                }
            }
            finally
            {
                stream.Close();
            }
            return result;
        }

        /// <summary>
        /// 通用的Post请求
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static string ComeRequestPost(HttpWebRequest request)
        {
            string result = "";
            request.AllowAutoRedirect = true;
            request.Method = "POST";
            request.CookieContainer = new CookieContainer();
            request.Credentials = CredentialCache.DefaultCredentials;
            HttpWebResponse resp = (HttpWebResponse)request.GetResponse();
            Stream stream = resp.GetResponseStream();
            try
            {
                //获取内容
                using (StreamReader reader = new StreamReader(stream))
                {
                    result = reader.ReadToEnd();
                }
            }
            finally
            {
                stream.Close();
            }
            return result;
        }

    }
}