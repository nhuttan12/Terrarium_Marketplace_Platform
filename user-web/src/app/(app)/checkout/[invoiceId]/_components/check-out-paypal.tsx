'use client';

import { PayPalScriptProvider, PayPalButtons } from "@paypal/react-paypal-js";
import { useState } from "react";
import { useRouter } from "next/navigation";

interface Props {
    invoiceId: string; // Truyền mã hóa đơn (Guid) từ trang cha vào đây
}

export default function PayPalCheckoutButton({ invoiceId }: Props) {
	const [message, setMessage] = useState('');
	const router = useRouter();
	// Cấu hình khởi tạo PayPal
	const initialOptions = {
		clientId: process.env.NEXT_PUBLIC_PAYPAL_CLIENT_ID || '',
		currency: 'USD',
		intent: 'capture',
	};

	return (
		<div className='w-full max-w-md mx-auto mt-6'>
			<PayPalScriptProvider options={initialOptions}>
				<PayPalButtons
					style={{ layout: 'vertical', shape: 'rect', color: 'blue' }}
					// BƯỚC 1: Gọi .NET Backend để khởi tạo giao dịch
					createOrder={async () => {
						try {
							setMessage('');
							const response = await fetch(
								'https://localhost:7087/api/payment/create',
								{
									method: 'POST',
									headers: { 'Content-Type': 'application/json' },
									credentials: 'include', // Gửi HttpOnly Cookie (X-Access-Token) kèm request
									body: JSON.stringify({ invoiceId: invoiceId }),
								},
							);

							const data = await response.json();

							if (!response.ok) {
								throw new Error(data.message || 'Không thể tạo đơn hàng PayPal.');
							}

							// Trả về orderId do API .NET (PayPalService) tạo ra
							return data.orderId;
						} catch (error: any) {
							console.error('Lỗi Create Order:', error);
							setMessage(error.message);
							throw error;
						}
					}}
					// BƯỚC 2: PayPal gọi hàm này khi khách đã login popup và ấn Approve (Đồng ý)
					onApprove={async (data, actions) => {
						try {
							setMessage('Đang xử lý thanh toán, vui lòng đợi...');

							// data.orderID lúc này chính là paypalOrderId
							const response = await fetch(
								`https://localhost:7087/api/payment/capture/${data.orderID}`,
								{
									method: 'POST',
									headers: { 'Content-Type': 'application/json' },
									credentials: 'include', // Gửi HttpOnly Cookie (X-Access-Token) kèm request
								},
							);

							const result = await response.json();

							if (!response.ok) {
								throw new Error(
									result.message || 'Lỗi khi chốt thanh toán với hệ thống.',
								);
							}

							// Thành công (Database đã update Paid)
							setMessage('Thanh toán thành công! Cảm ơn bạn.');

							// Tùy chọn: Chuyển hướng người dùng về trang cảm ơn
							// router.push(`/checkout/success?invoiceId=${invoiceId}`);
						} catch (error: any) {
							console.error('Lỗi Capture Order:', error);
							setMessage('Thanh toán thất bại: ' + error.message);
						}
					}}
					// Xử lý khi khách hàng đóng cửa sổ Popup ngang chừng
					onCancel={() => {
						setMessage('Bạn đã hủy phiên thanh toán.');
					}}
					// Xử lý khi có lỗi nội tại từ script của PayPal
					onError={(err) => {
						console.error('PayPal Script Error:', err);
						setMessage('Lỗi kết nối cổng thanh toán. Vui lòng thử lại sau.');
					}}
				/>
			</PayPalScriptProvider>

			{/* Hiển thị thông báo trạng thái */}
			{message && (
				<div className='mt-4 p-3 text-center rounded bg-gray-50 text-gray-800 text-sm font-medium'>
					{message}
				</div>
			)}
		</div>
	);
}