﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sales_Model.Constants
{
    public class Message
    {
        //Xử lý message tài khoản
        public const string ErrorMsg = "Có lỗi xảy ra";
        public const string NotAuthorize = "Bạn không có quyền này";
        public const string AccountNotFound = "Không tìm thấy tài khoản này";
        public const string AccountLoginAgain = "Bạn vui lòng đăng nhập lại để thực hiện chức năng này";
        public const string AccountLogoutSuccess = "Đăng xuất thành công";
        public const string AccountLogLogin = "Login";
        public const string AccountLogDelete = "Xóa tài khoản";
        public const string AccountLogLogout = "Logout";
        public const string AccountLogChange = "Thay đổi thông tin tài khoản";
        public const string AccountLogPassword = "Thay đổi mật khẩu";

        //Xử lý message sản phẩm
        public const string ProductLogChange = "Thay đổi thông tin sản phẩm";
        public const string ProductLogDelete = "Xóa sản phẩm";
        public const string ProductLogAdd = "Thêm mới sản phẩm";
        public const string ProductNotExist = "Sản phẩm này không tồn tại";
        public const string ProductNotFound = "Không tìm thấy sản phẩm này";
        public const string ProductPriceNotFound = "Không tìm thấy giá sản phẩm này";

        //Xử lý message order
        public const string OrderLogChange = "Thay đổi thông tin đơn hàng";
        //public const string ProductLogDelete = "Xóa sản phẩm";
        public const string OrderLogAdd = "Đơn hàng mới được tạo";
        public const string OrderNotExist = "Đơn hàng này không tồn tại";
        public const string OrderNotFound = "Không tìm thấy đơn hàng này";
        public const string OrderItemAdd = "Thêm vào giỏ hàng thành công";
        public const string QuantityInvalid = "Số lượng không hợp lệ";
        public const string QuantityNotEnough = "Số lượng sản phẩm không đủ";
    }
}
