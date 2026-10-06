import axios, {
	type AxiosError,
	type AxiosInstance,
	type AxiosResponse,
	type InternalAxiosRequestConfig,
} from 'axios';
import { type ResponseApi } from '@/types/common/ResponseApi';
import * as https from 'node:https';

const API_BASE_URL: string = process.env.NEXT_PUBLIC_API_URL || 'https://localhost:7087/api';
const isDevMode: boolean = process.env.NODE_ENV === 'development';

const httpsAgent: https.Agent = new https.Agent({
	rejectUnauthorized: !isDevMode,
});

export const apiClient: AxiosInstance = axios.create({
	baseURL: API_BASE_URL,
	timeout: 10000,
	withCredentials: true,
	httpsAgent: httpsAgent,
});

apiClient.interceptors.request.use(
	(config: InternalAxiosRequestConfig): InternalAxiosRequestConfig => {
		// 1. Tự động xử lý Content-Type linh hoạt
		// Nếu payload là FormData (ví dụ: upload file), Axios sẽ tự động set 'multipart/form-data' và gắn boundary.
		if (config.data && !(config.data instanceof FormData)) {
			config.headers.set('Content-Type', 'application/json');
		}

		// 2. Chống tấn công CSRF cho các phương thức Mutation
		const method: string = config.method?.toUpperCase() || '';
		if (['POST', 'PUT', 'PATCH', 'DELETE'].includes(method)) {
		}
		console.log('=== CHI TIẾT REQUEST GỬI ĐI ===');
		console.log(`[${config.method?.toUpperCase()}] ${config.baseURL}${config.url}`);
		console.log('Headers:', config.headers);
		return config;
	},
	(error: AxiosError): Promise<never> => {
		return Promise.reject(error);
	},
);

apiClient.interceptors.response.use(
	(response: AxiosResponse): AxiosResponse => {
		return response;
	},
	async (error: AxiosError): Promise<never> => {
		const status: number = error.response?.status || 500;

		if (status === 401 && typeof window !== 'undefined') {
			window.location.href = '/login';
		}

		if (error.response && error.response.data) {
			return Promise.reject(error);
		}

		error.response = {
			data: {
				isSuccess: false,
				data: null,
				error: {
					code: `CLIENT_HTTP_${status}`,
					message: 'Không thể kết nối đến máy chủ .NET',
					errorType: 'NetworkError',
				},
			} as ResponseApi<null>,
			status: status,
			statusText: 'Network Error',
			headers: {},
			config: error.config!,
		};

		return Promise.reject(error);
	},
);

export default apiClient;
