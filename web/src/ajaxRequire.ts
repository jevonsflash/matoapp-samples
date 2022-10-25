import Cookies from "js-cookie";
import axios, {  CancelTokenSource } from 'axios'
//发送网络请求
const tokenKey = "main_token";
const getToken = () => Cookies.get(tokenKey);

export const request = async (url: string, methods, data: any, onProgress?: (e) => void, cancelToken?: CancelTokenSource) => {
    let token = null
    let timeout = 3000;
    if (cancelToken) {
        token = cancelToken.token
        timeout = 0;
    }

    const service = axios.create()
    service.interceptors.request.use(
        (config) => {
            const token = getToken();
            // Add X-Access-Token header to every request, you can add other custom headers here
            if (token) {
                config.headers['X-XSRF-TOKEN'] = token
                config.headers['Authorization'] = 'Bearer ' + token
            }
            return config
        },
        (error) => {
            Promise.reject(error)
        }
    )

    const re = await service.request({
        url: url,
        method: methods,
        data: data,
        cancelToken: token,
        timeout: timeout,
        onUploadProgress: function (progressEvent) { //原生获取上传进度的事件
            if (progressEvent.lengthComputable) {
                if (onProgress) {
                    onProgress(progressEvent);
                }
            }
        },
    })
    return re as any;
}

///获得取消令牌
export const getCancelToken = () => {
    const source = axios.CancelToken.source();
    return source;
}
