import axios from 'uni-axios-ts'
//发送网络请求
export const request = async (url: string, methods, data: any, onProgress?: (e) => void, cancelToken?) => {
    let token = null
    let timeout = 3000;
    if (cancelToken) {
        token = cancelToken.token
        timeout = 0;
    }
    const service = axios.create()


    service.interceptors.request.use(
        (config) => {
            config.header['Content-Type'] = "application/json"
            return config
        },
        (error) => {
            Promise.reject(error)
        }
    )


    const re = await service.request({
        headers: { 'Content-Type': 'multipart/form-data' },
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
