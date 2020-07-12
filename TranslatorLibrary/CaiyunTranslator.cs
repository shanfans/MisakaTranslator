﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Net.Http;

namespace TranslatorLibrary
{
    public class CaiyunTranslator : ITranslator
    {
        public string caiyunToken;//彩云小译 令牌
        private string errorInfo;//错误信息

        public string GetLastError()
        {
            return errorInfo;
        }

        public string Translate(string sourceText, string desLang, string srcLang)
        {
            if (sourceText == "" || desLang == "" || srcLang == "")
            {
                errorInfo = "Param Missing";
                return null;
            }

            if (desLang == "jp")
                desLang = "ja";
            if (srcLang == "jp")
                srcLang = "ja";

            // 原文
            string q = sourceText;
            string retString;

            string trans_type = srcLang + "2" + desLang;

            string url = "https://api.interpreter.caiyunai.com/v1/translator";
            //json参数
            string jsonParam = "{\"source\": [\"" + q + "\"], \"trans_type\": \"" + trans_type + "\", \"request_id\": \"demo\", \"detect\": true}";

            var hc = CommonFunction.GetHttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("X-Authorization", "token " + caiyunToken);
            request.Headers.Add("ContentType", "application/json;charset=UTF-8");
            request.Content = new StringContent(jsonParam);
            try
            {
                retString = hc.SendAsync(request).GetAwaiter().GetResult().Content.ReadAsStringAsync().GetAwaiter().GetResult();
            }
            catch (WebException ex)
            {
                errorInfo = ex.Message;
                return null;
            }
            finally
            {
                request.Dispose();
            }

            CaiyunTransResult oinfo;
            try
            {
                oinfo = JsonConvert.DeserializeObject<CaiyunTransResult>(retString);
            }
            catch {
                errorInfo = "JsonConvert Error";
                return null;
            }

            if (oinfo != null && oinfo.target.Count >= 1)
            {
                //得到翻译结果
                string r = "";
                for (int i = 0;i < oinfo.target.Count;i++) {
                    r += Regex.Unescape(oinfo.target[i]);
                }

                return r;
            }
            else
            {
                errorInfo = "ErrorInfo:" + oinfo.message;
                return null;
            }
            
        }

        public void TranslatorInit(string param1, string param2 = "")
        {
            caiyunToken = param1;
        }


        /// <summary>
        /// 彩云小译API申请地址
        /// </summary>
        /// <returns></returns>
        public static string GetUrl_allpyAPI()
        {
            return "https://dashboard.caiyunapp.com/user/sign_in/";
        }

        /// <summary>
        /// 彩云小译API额度查询地址
        /// </summary>
        /// <returns></returns>
        public static string GetUrl_bill()
        {
            return "https://dashboard.caiyunapp.com/";
        }

        /// <summary>
        /// 彩云小译API文档地址（错误代码）
        /// </summary>
        /// <returns></returns>
        public static string GetUrl_Doc()
        {
            return "https://fanyi.caiyunapp.com/#/api";
        }
    }


    class CaiyunTransResult
    {
        public string message { get; set; }
        public double confidence { get; set; }
        public int rc { get; set; }
        public List<string> target { get; set; }
    }


}
