using DienTu.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DienTu.Utility
{
    public static class SD
    {
        public const string DeafaultFoodImage = "default_food.png";

        public const string ManagerUser = "Manager";
        public const string CustomerUser = "Customer";

        public const string ssShoppingCartCount = "ssCartCount";
		public const string ssCouponCode = "ssCouponCode";


		public const string StatusSubmitted = "Chờ Xác nhận";
		public const string StatusInProcess = "Đang chuẩn bị";
		public const string StatusShip = "Đang giao";
		public const string StatusCompleted = "Hoàn thành";
		public const string StatusCancelled = "Huỷ";


		public const string PaymentStatusPending = "Chưa thanh toán";
		public const string PaymentStatusApproved = "Đã thanh toán";
		public const string PaymentStatusRejected = "Đã huỷ";



		public static string ConvertToRawHtml(string source)
		{
			char[] array = new char[source.Length];
			int arrayIndex = 0;
			bool inside = false;

			for (int i = 0; i < source.Length; i++)
			{
				char let = source[i];
				if (let == '<')
				{
					inside = true;
					continue;
				}
				if (let == '>')
				{
					inside = false;
					continue;
				}
				if (!inside)
				{
					array[arrayIndex] = let;
					arrayIndex++;
				}
			}
			return new string(array, 0, arrayIndex);
		}

		public static double DiscountedPrice(Coupon couponFromDb, double OriginalOrderTotal)
        {
			if(couponFromDb==null)
            {
				return OriginalOrderTotal;
            }
			else
            {
				if(couponFromDb.MinimumAmount> OriginalOrderTotal)
                {
					return OriginalOrderTotal;
                }
				else
                {
					if(Convert.ToInt32(couponFromDb.CouponType)==(int)Coupon.ECouponType.Dollar)
                    {
						//tien mat
						return Math.Round(OriginalOrderTotal-couponFromDb.Discount,2);
                    }						
					
					if (Convert.ToInt32(couponFromDb.CouponType) == (int)Coupon.ECouponType.Percent)
					{   
						//10-->100
						//phan tram
						return Math.Round(OriginalOrderTotal * ((100 - couponFromDb.Discount) / 100), 2);
					}
                }
            }
			return OriginalOrderTotal;
        }
	}
}
