using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sales_Model.Constants
{
    public class Message
    {
        public const string ErrorMsg = "Có lỗi xảy ra";
        public const string ProductNotExist = "Sản phẩm này không tồn tại";
        public const string ProductNotFound = "Không tìm thấy sản phẩm này";
        public const string NotAuthorize = "Bạn không có quyền này";
        public const string AccountNotFound= "Không tìm thấy tài khoản này";
        public const string AccountLoginAgain= "Bạn vui lòng đăng nhập lại để thực hiện chức năng này";
        public const string AccountLogoutSuccess= "Đăng xuất thành công";


        public const string AccountLogLogin = "Login";
        public const string AccountLogDelete = "Xóa tài khoản";
        public const string AccountLogLogout = "Logout";
        public const string AccountLogChange = "Thay đổi thông tin tài khoản";

        public const string ProductLogChange = "Thay đổi thông tin sản phẩm";
        public const string ProductLogDelete = "Xóa sản phẩm";
        public const string ProductLogAdd = "Thêm mới sản phẩm";

    }
}
