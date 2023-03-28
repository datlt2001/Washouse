﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using Washouse.Common.Utils;
using Washouse.Model.RequestModels;
using Washouse.Model.ResponseModels;
using Washouse.Service.Interface;
using Washouse.Web.Models;
using static Google.Apis.Requests.BatchRequest;

namespace Washouse.Web.Controllers
{
    [Route("api/payments")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        public IPaymentService _paymentService;
        private readonly VNPaySettings vnPaySettings;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PaymentController(IPaymentService paymentService, IOptions<VNPaySettings> _vnpaySettings, IHttpContextAccessor httpContextAccessor)
        {
            _paymentService = paymentService;
            vnPaySettings = _vnpaySettings.Value;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet]
        public IActionResult GetPayment()
        {
            string url = vnPaySettings.VNP_Url;
            string returnUrl = vnPaySettings.VNP_ReturnUrl;
            string tmnCode = vnPaySettings.VNP_TmnCode;
            string hashSecret = vnPaySettings.VNP_HashSecret;

            PayLib pay = new PayLib();
            pay.AddRequestData("vnp_Version", "2.1.0"); //Phiên bản api mà merchant kết nối. Phiên bản hiện tại là 2.1.0
            pay.AddRequestData("vnp_Command", "pay"); //Mã API sử dụng, mã cho giao dịch thanh toán là 'pay'
            pay.AddRequestData("vnp_TmnCode", tmnCode); //Mã website của merchant trên hệ thống của VNPAY (khi đăng ký tài khoản sẽ có trong mail VNPAY gửi về)
            pay.AddRequestData("vnp_Amount", "1000000"); //số tiền cần thanh toán, công thức: số tiền * 100 - ví dụ 10.000 (mười nghìn đồng) --> 1000000
            pay.AddRequestData("vnp_BankCode", ""); //Mã Ngân hàng thanh toán (tham khảo: https://sandbox.vnpayment.vn/apis/danh-sach-ngan-hang/), có thể để trống, người dùng có thể chọn trên cổng thanh toán VNPAY
            pay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss")); //ngày thanh toán theo định dạng yyyyMMddHHmmss
            pay.AddRequestData("vnp_CurrCode", "VND"); //Đơn vị tiền tệ sử dụng thanh toán. Hiện tại chỉ hỗ trợ VND
            pay.AddRequestData("vnp_IpAddr", PayUtils.GetIpAddress(HttpContext)); //Địa chỉ IP của khách hàng thực hiện giao dịch
            pay.AddRequestData("vnp_Locale", "vn"); //Ngôn ngữ giao diện hiển thị - Tiếng Việt (vn), Tiếng Anh (en)
            pay.AddRequestData("vnp_OrderInfo", "Pay order"); //Thông tin mô tả nội dung thanh toán
            pay.AddRequestData("vnp_OrderType", "other"); //topup: Nạp tiền điện thoại - billpayment: Thanh toán hóa đơn - fashion: Thời trang - other: Thanh toán trực tuyến
            pay.AddRequestData("vnp_ReturnUrl", returnUrl); //URL thông báo kết quả giao dịch khi Khách hàng kết thúc thanh toán
            pay.AddRequestData("vnp_TxnRef", DateTime.Now.Ticks.ToString()); //mã hóa đơn

            string paymentUrl = pay.CreateRequestUrl(url, hashSecret);

            return Ok(new ResponseModel
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Updated",
                Data = paymentUrl
            });
        }

        [HttpGet("GetPaymentConfirm")]
        public IActionResult GetPaymentConfirm()
        {
            
            //if (Request.QueryString.Count > 0)
            //{
            string hashSecret = vnPaySettings.VNP_HashSecret; //Chuỗi bí mật            
            var vnpayData = Request.Query;
            PayLib pay = new PayLib();

            //lấy toàn bộ dữ liệu được trả về
            foreach (string s in vnpayData.Keys)
            {
                if (!string.IsNullOrEmpty(s) && s.StartsWith("vnp_"))
                {
                    pay.AddResponseData(s, vnpayData[s]);
                }
            }

            long orderId = Convert.ToInt64(pay.GetResponseData("vnp_TxnRef")); //mã hóa đơn
            long vnpayTranId = Convert.ToInt64(pay.GetResponseData("vnp_TransactionNo")); //mã giao dịch tại hệ thống VNPAY
            string vnp_ResponseCode = pay.GetResponseData("vnp_ResponseCode"); //response code: 00 - thành công, khác 00 - xem thêm https://sandbox.vnpayment.vn/apis/docs/bang-ma-loi/
            string vnp_SecureHash = Request.Query["vnp_SecureHash"]; //hash của dữ liệu trả về

            bool checkSignature = pay.ValidateSignature(vnp_SecureHash, hashSecret); //check chữ ký đúng hay không?
            string response = null;
            if (checkSignature)
            {
                if (vnp_ResponseCode == "00")
                {
                    //Thanh toán thành công
                    response = "Thanh toán thành công hóa đơn " + orderId + " | Mã giao dịch: " + vnpayTranId;
                }
                else
                {
                    //Thanh toán không thành công. Mã lỗi: vnp_ResponseCode
                    response = "Có lỗi xảy ra trong quá trình xử lý hóa đơn " + orderId + " | Mã giao dịch: " + vnpayTranId + " | Mã lỗi: " + vnp_ResponseCode;
                }
            }
            else
            {
                response = "Có lỗi xảy ra trong quá trình xử lý";
            }
            //}

            return Ok(new ResponseModel
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Success",
                Data = response
            });
        }

        [HttpGet("callback")]
        public IActionResult ReturnUrl([FromQuery] VNPayTransactionResult result)
        {
            // Get the query parameters from the request
            var queryString = Request.Query;

            

            // Return a response to VNPay
            return Ok(new ResponseModel
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Transaction completed successfully.",
                Data = result
            });
        }

    }
}