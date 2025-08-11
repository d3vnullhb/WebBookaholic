using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Bookaholic.Models;
using System.Collections.Generic;

namespace Bookaholic.Services
{
    public interface IVnPayService
    {
        string CreatePaymentUrl(PaymentInformationModel model, HttpContext context);
        PaymentResponseModel PaymentExecute(IQueryCollection collections);
    }

    public class VnPayService : IVnPayService
    {
        private readonly IConfiguration _config;

        public VnPayService(IConfiguration config)
        {
            _config = config;
        }

        public string CreatePaymentUrl(PaymentInformationModel model, HttpContext context)
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(_config["TimeZoneId"]);
            var timeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);

            var tick = DateTime.Now.Ticks.ToString();
            var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";

            var vnpayData = new SortedDictionary<string, string>
            {
                { "vnp_Version", _config["Vnpay:Version"] },
                { "vnp_Command", _config["Vnpay:Command"] },
                { "vnp_TmnCode", _config["Vnpay:TmnCode"] },
                { "vnp_Amount", ((int)(model.Amount * 100)).ToString() },
                { "vnp_CreateDate", timeNow.ToString("yyyyMMddHHmmss") },
                { "vnp_CurrCode", _config["Vnpay:CurrCode"] },
                { "vnp_IpAddr", ipAddress },
                { "vnp_Locale", _config["Vnpay:Locale"] },
                { "vnp_OrderInfo", $"Order-{tick} | {model.Name} | {model.OrderDescription}" }, 
                { "vnp_OrderType", model.OrderType },
                { "vnp_ReturnUrl", _config["Vnpay:PaymentCallBack:ReturnUrl"] }, 
                { "vnp_TxnRef", tick }
            };

            string signData = string.Join("&", vnpayData
                .OrderBy(x => x.Key)
                .Select(kvp => $"{WebUtility.UrlEncode(kvp.Key)}={WebUtility.UrlEncode(kvp.Value)}"));

            string secureHash = HmacSHA512(_config["Vnpay:HashSecret"], signData);
            string paymentUrl = $"{_config["Vnpay:BaseUrl"]}?{signData}&vnp_SecureHash={secureHash}";

            return paymentUrl;
        }

        public PaymentResponseModel PaymentExecute(IQueryCollection query)
        {
            var vnpData = query
                .Where(kvp => kvp.Key.StartsWith("vnp_"))
                .ToDictionary(k => k.Key, v => v.Value.ToString());

            string vnpSecureHash = vnpData["vnp_SecureHash"];
            vnpData.Remove("vnp_SecureHash");
            vnpData.Remove("vnp_SecureHashType");

            string signData = string.Join("&", vnpData
                .OrderBy(k => k.Key)
                .Select(kvp => $"{WebUtility.UrlEncode(kvp.Key)}={WebUtility.UrlEncode(kvp.Value)}"));

            string computedHash = HmacSHA512(_config["Vnpay:HashSecret"], signData);

            bool success = string.Equals(vnpSecureHash, computedHash, StringComparison.InvariantCultureIgnoreCase);
            string responseCode = vnpData.GetValueOrDefault("vnp_ResponseCode", "");

            return new PaymentResponseModel
            {
                Success = success && responseCode == "00",
                PaymentMethod = "VNPAY",
                OrderDescription = vnpData.GetValueOrDefault("vnp_OrderInfo", ""),
                OrderId = vnpData.GetValueOrDefault("vnp_TxnRef", ""),
                PaymentId = vnpData.GetValueOrDefault("vnp_TransactionNo", ""),
                TransactionId = vnpData.GetValueOrDefault("vnp_TransactionNo", ""),
                Token = vnpSecureHash,
                VnPayResponseCode = responseCode
            };
        }

        private string HmacSHA512(string key, string input)
        {
            using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(key));
            byte[] hashValue = hmac.ComputeHash(Encoding.UTF8.GetBytes(input));
            return string.Concat(hashValue.Select(b => b.ToString("x2")));
        }
    }
}
