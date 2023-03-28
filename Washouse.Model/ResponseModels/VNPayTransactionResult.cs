using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Washouse.Model.ResponseModels
{
    public class VNPayTransactionResult
    {
        public string vnp_ResponseCode { get; set; }//response code: 00 - thành công, khác 00 - xem thêm https://sandbox.vnpayment.vn/apis/docs/bang-ma-loi/
        public string vnp_TxnRef { get; set; }//mã hóa đơn
        public string vnp_Amount { get; set; }//số tiền
        public string vnp_BankCode { get; set; }
        // Add any other necessary properties
        public string vnp_TransactionNo { get; set; }//mã giao dịch tại hệ thống VNPAY
        public string vnp_OrderInfo { get; set; }//Thông tin mô tả nội dung thanh toán

        public string vnp_TransactionStatus { get; set; }//Mã phản hồi kết quả thanh toán. Tình trạng của giao dịch tại Cổng thanh toán VNPAY.00: Giao dịch thanh toán được thực hiện thành công tại VNPAYKhác 00: Giao dịch không thành công tại VNPAY Tham khảo thêm tại bảng mã lỗi
    }
}
