using Microsoft.AspNetCore.Mvc;
using System;
using System.Text.Json;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace CheckedReceipt.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class ReceiptController : ControllerBase
    {
        /// <summary>
        /// ReceiptPrize 取近2期發票結果等相關訊息
        /// </summary>

        [HttpGet]
        public ActionResult<string> GetReceiptPrize()
        {
            ReturnModel ReturnModel = new ReturnModel();
            try
            {
                List<ReceiptPrize_Model> DataList = new List<ReceiptPrize_Model>();
                //最新獎號
                string strUrl = @"https://invoice.etax.nat.gov.tw/"; //財政部統一發票開獎網頁
                string strHtml = HttpOpera.GetHttp(strUrl);
                ReceiptPrize_Model receiptModel = new Receipt(strHtml).AnalysisPrize();
                DataList.Add(receiptModel);
                //前期獎號
                strUrl = @"https://invoice.etax.nat.gov.tw/lastNumber.html";
                strHtml = HttpOpera.GetHttp(strUrl);
                receiptModel = new Receipt(strHtml).AnalysisPrize();
                DataList.Add(receiptModel);

                ReturnModel.Status = true;
                ReturnModel.Data = DataList;
            }
            catch (Exception ex)
            {
                ReturnModel.Status = false;
                ReturnModel.ErrorMessage = ex.Message;
            }

            return Ok(ReturnModel);
        }
        
    }

    #region 模型
    public class ReturnModel
    {
        public string CurrentTime
        {
            get
            {
                return DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            }
        }
        public object Data { set; get; }
        public bool Status { set; get; }
        public string ErrorMessage { set; get; }

    }
    public class ReceiptPrize_Model
    {
        public int ID { set; get; }
        public int ReceiptYear { set; get; }
        public string ReceiptMonth { set; get; }
        public string SpecialNumber { set; get; }
        public int SpecialDigit{ set; get; }
        public int SpecialPrize { set; get; }
        public string GrandNumber { set; get; }
        public int GrandDigit { set; get; }
        public int GrandPrize { set; get; }
        public string FirstNumber_1 { set; get; }
        public string FirstNumber_2 { set; get; }
        public string FirstNumber_3 { set; get; }
        public string ExtraNumber_1 { set; get; }
        public string ExtraNumber_2 { set; get; }
        public int FirstDigit { set; get; }
        public int FirstPrize { set; get; }
        public int SecondDigit { set; get; }
        public int SecondPrize { set; get; }
        public int ThirdDigit { set; get; }
        public int ThirdPrize { set; get; }
        public int FourthDigit { set; get; }
        public int FourthPrize { set; get; }
        public int FifthDigit { set; get; }
        public int FifthPrize { set; get; }
        public int SixthDigit { set; get; }
        public int SixthPrize { set; get; }
        public int ExtraDigit { set; get; }
        public int ExtraPrize { set; get; }
        public string GetPrizeDate { set; get; }
    }
    #endregion

    #region HTTP訪問
    public static class HttpOpera
    {
        /// <summary>
        /// 訪問網頁，Get類型
        /// </summary>
        /// <param name="Url">網頁網址</param>
        /// <returns></returns>
        public static string GetHttp(string Url)
        {
            string strHTML = "";
            Uri uri = new Uri(Url);
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(uri);
            try
            {
                myReq.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/42.0.2311.135 Safari/537.36 Edge/12.10240";
                myReq.Accept = "*";
                myReq.KeepAlive = true;
                myReq.Headers.Add("Accept-Language", "zh-CN");
                System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                HttpWebResponse result = (HttpWebResponse)myReq.GetResponse();
                if (result.StatusCode == HttpStatusCode.OK)
                {
                    Stream receviceStream = result.GetResponseStream();
                    StreamReader readerOfStream = new StreamReader(receviceStream, System.Text.Encoding.GetEncoding("utf-8"));
                    strHTML = readerOfStream.ReadToEnd();
                    readerOfStream.Close();
                    receviceStream.Close();
                    result.Close();
                    myReq.Abort();

                }
                return strHTML;
            }
            catch (Exception ex)
            {
                return strHTML;
            }
            finally
            {
                if (myReq != null)
                {
                    myReq.Abort();
                }
                myReq = null;
            }
        }

        /// <summary>
        /// 訪問網頁，POST類型
        /// </summary>
        /// <param name="Url">網頁網址</param>
        /// <param name="Postdata">傳遞參數</param>
        /// <returns></returns>
        public static string PostHttp(string Url, string Postdata)
        {
            string strHTML = "";
            Uri uri = new Uri(Url);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            try
            {
                request.ContentType = "application/x-www-form-urlencoded";
                request.Method = "POST";
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/42.0.2311.135 Safari/537.36 Edge/12.10240";
                request.Accept = "*/*";
                request.KeepAlive = true;

                byte[] bs = Encoding.ASCII.GetBytes(Postdata);
                request.ContentLength = bs.Length;
                using (Stream reqStream = request.GetRequestStream())
                {
                    reqStream.Write(bs, 0, bs.Length);
                }
                using (WebResponse response = request.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        strHTML = reader.ReadToEnd();
                    }
                }
                return strHTML;

            }
            catch (Exception ex)
            {
                return "";
            }
            finally
            {
                if (request != null)
                {
                    request.Abort();
                }
            }
        }

    }
    #endregion

    #region 統一發票操作
    public class Receipt
    {
        #region 爬蟲部分
        private string StrData { set; get; }
        /// <summary>
        /// 先提供網頁Html
        /// </summary>
        /// <param name="strData"></param>
        public Receipt(string strData)
        {
            this.StrData = strData;
        }
        /// <summary>
        /// 分析 統一發票 開獎號碼 ，返回Model
        /// </summary>
        /// <returns></returns>
        public ReceiptPrize_Model AnalysisPrize()
        {
            ReceiptPrize_Model model = new ReceiptPrize_Model();
            string[] listMonth = AnalysisMonth().Split(",");
            model.ReceiptYear = Convert.ToInt16(listMonth[0]);
            model.ReceiptMonth = listMonth[1];
            MatchCollection NumberList;
            string strHtml = GetRegString(StrData, @"<table[^$]*?</table");
            int i = 0;
            foreach (Match match in GetMatchCollection("<tr[^$]*?</tr", GetRegString(strHtml, @"<tbody[^$]*?</tfoot")))
            {
                switch (i)
                {
                    case 0:
                        //特別獎 獎號 獎金
                        model.SpecialNumber = GetRegString(GetRegString(match.ToString(), @"<span[^$]*?</span"), @"[0-9]+");
                        NumberList = GetMatchCollection(@"[0-9]+", GetRegString(match.ToString(), @"<p class=""mb-0"">[^$]*?</p>").Replace(@"<p class=""mb-0"">", "").Replace(@"<p class=""mb-0"">", "").Replace(",", ""));
                        model.SpecialDigit = Convert.ToInt16(NumberList[0].ToString());
                        model.SpecialPrize = Convert.ToInt16(NumberList[1].ToString()) * 10000;

                        break;
                    case 1:
                        //特獎 獎號 獎金
                        model.GrandNumber = GetRegString(GetRegString(match.ToString(), @"<span[^$]*?</span"), @">[^$]*?<").Replace(">", "").Replace("<", "");
                        NumberList = GetMatchCollection(@"[0-9]+", GetRegString(match.ToString(), @"<p class=""mb-0"">[^$]*?</p>").Replace(@"<p class=""mb-0"">", "").Replace(@"<p class=""mb-0"">", "").Replace(",", ""));
                        model.GrandDigit = Convert.ToInt16(NumberList[0].ToString()); 
                        model.GrandPrize = Convert.ToInt16(NumberList[1].ToString()) * 10000;
                        break;
                    case 2:
                        //頭獎 獎號*3 獎金
                        NumberList = GetMatchCollection(@"<span[^$]*?</span>", match.ToString());
                        model.FirstNumber_1 = GetRegString(NumberList[0].ToString(), @"[0-9]+") + GetRegString(NumberList[1].ToString(), @"[0-9]+");
                        model.FirstNumber_2 = GetRegString(NumberList[2].ToString(), @"[0-9]+") + GetRegString(NumberList[3].ToString(), @"[0-9]+");
                        model.FirstNumber_3 = GetRegString(NumberList[4].ToString(), @"[0-9]+") + GetRegString(NumberList[5].ToString(), @"[0-9]+");
                        NumberList = GetMatchCollection(@"[0-9]+", GetRegString(match.ToString(), @"<p class=""mb-0"">[^$]*?</p>").Replace(@"<p class=""mb-0"">", "").Replace(@"<p class=""mb-0"">", "").Replace(",", ""));
                        model.FirstDigit = Convert.ToInt16(NumberList[0].ToString());
                        model.FirstPrize = Convert.ToInt16(NumberList[1].ToString()) * 10000;
                        break;
                    case 3:
                        //二獎 獎金
                        NumberList = GetMatchCollection(@"[0-9]+", GetRegString(match.ToString(), @"<p class=""mb-0"">[^$]*?</p>").Replace(@"<p class=""mb-0"">", "").Replace(@"<p class=""mb-0"">", "").Replace(",", ""));
                        model.SecondDigit = Convert.ToInt16(NumberList[0].ToString());
                        model.SecondPrize = Convert.ToInt16(NumberList[2].ToString()) * 10000;
                        break;
                    case 4:
                        //三獎 獎金
                        NumberList = GetMatchCollection(@"[0-9]+", GetRegString(match.ToString(), @"<p class=""mb-0"">[^$]*?</p>").Replace(@"<p class=""mb-0"">", "").Replace(@"<p class=""mb-0"">", "").Replace(",", ""));
                        model.ThirdDigit = Convert.ToInt16(NumberList[0].ToString());
                        model.ThirdPrize = Convert.ToInt16(NumberList[2].ToString()) * 10000;
                        break;
                    case 5:
                        //四獎 獎金
                        NumberList = GetMatchCollection(@"[0-9]+", GetRegString(match.ToString(), @"<p class=""mb-0"">[^$]*?</p>").Replace(@"<p class=""mb-0"">", "").Replace(@"<p class=""mb-0"">", "").Replace(",", ""));
                        model.FourthDigit = Convert.ToInt16(NumberList[0].ToString());
                        model.FourthPrize = Convert.ToInt16(NumberList[2].ToString()) * 1000;
                        break;
                    case 6:
                        //五獎 獎金
                        NumberList = GetMatchCollection(@"[0-9]+", GetRegString(match.ToString(), @"<p class=""mb-0"">[^$]*?</p>").Replace(@"<p class=""mb-0"">", "").Replace(@"<p class=""mb-0"">", "").Replace(",", ""));
                        model.FifthDigit = Convert.ToInt16(NumberList[0].ToString());
                        model.FifthPrize = Convert.ToInt16(NumberList[2].ToString()) * 1000;
                        break;
                    case 7:
                        //六獎 獎金
                        NumberList = GetMatchCollection(@"[0-9]+", GetRegString(match.ToString(), @"<p class=""mb-0"">[^$]*?</p>").Replace(@"<p class=""mb-0"">", "").Replace(@"<p class=""mb-0"">", "").Replace(",", ""));
                        model.SixthDigit = Convert.ToInt16(NumberList[0].ToString());
                        model.SixthPrize = Convert.ToInt16(NumberList[2].ToString()) * 100;
                        break;
                    case 8:
                        //獎金提領日期
                        NumberList = GetMatchCollection(@"[0-9]+", GetRegString(match.ToString(), @""">[^$]*?<").Replace(@"<p class=""mb-0"">", "").Replace(@"<p class=""mb-0"">", "").Replace(",", ""));
                        model.GetPrizeDate = NumberList[0].ToString() + NumberList[1].ToString() + NumberList[2].ToString() + "-" + NumberList[3].ToString() + NumberList[4].ToString() + NumberList[5].ToString();
                        break;
                }
                i++;
            }
            return model;
        }
        /// <summary>
        /// 解析 開獎年月份
        /// </summary>
        /// <returns>
        /// 民國年,月份-月分: string: 101,03-04
        /// </returns>
        public string AnalysisMonth()
        {
            string strMonth = GetRegString(StrData, @"<li><a class=""etw-on""[^$]*?</a");
            strMonth = GetRegString(strMonth, @"title=""[^$]*?"">").Replace(@"title=""", "").Replace(@"月中獎號碼單"">", "").Replace("年", ",");


            string asd = @"<span class=""font-weight-bold"">16525</span><span
                        class=""font-weight-bold etw-color-red"">386</span>";
            string asdd = GetRegString(asd, @"\d");

            return strMonth;
        }

        /// <summary>
        /// 利用正則擷取字串
        /// </summary>
        /// <param name="strRegRule">正則條件</param>
        /// <param name="strData">輸入字串</param>
        /// <returns>MatchCollection</returns>
        private static string GetRegString(string strData, string strRegRule)
        {
            return new Regex(strRegRule).Match(strData).ToString();
        }

        /// <summary>
        /// 利用正則擷取字串，回傳陣列
        /// </summary>
        /// <param name="strRegRule">正則條件</param>
        /// <param name="strData">輸入字串</param>
        /// <returns>MatchCollection</returns>
        protected static MatchCollection GetMatchCollection(string strRegRule, string strData)
        {
            Regex reg = new Regex(strRegRule, RegexOptions.IgnoreCase);
            return reg.Matches(strData);
        }
        #endregion

    }
    #endregion
}
