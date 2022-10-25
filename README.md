# abp-mp-auth

![glimpse](https://raw.githubusercontent.com/jevonsflash/abp-mp-auth/master/assets/glimpse.gif)


搭建一套基于微信小程序登录的网站第三方登录模块


## 开始

1. 请在电脑浏览器打开 https://www.matoapp.net:3003/
2. 使用微信扫描Web端的小程序码
3. 等待微信小程序跳转至授权页面
4. 点击授权登录按钮，完成登录
5. 结果请在Web端查看



## 技术博客 

本系列博文将从原理分析、设计到代码编写，搭建一套基于微信小程序登录的网站第三方登录模块：

[使用 Abp.Zero 搭建第三方登录模块（一）：原理篇](https://blog.csdn.net/jevonsflash/article/details/125432269).

[使用 Abp.Zero 搭建第三方登录模块（二）：服务端开发](https://blog.csdn.net/jevonsflash/article/details/125441074).

[使用 Abp.Zero 搭建第三方登录模块（三）：网页端开发](https://blog.csdn.net/jevonsflash/article/details/125870551).

[使用 Abp.Zero 搭建第三方登录模块（四）：微信小程序开发](https://blog.csdn.net/jevonsflash/article/details/125872614).

## 本地运行

### api:

后端程序，使用.Net 6项目框架搭建，运行前请确保安装.net 6 运行时https://dotnet.microsoft.com/en-us/download/dotnet/6.0

```
cd api
```
```
dotnet run
```

### web:

前端程序

```
cd web
```
```
yarn install
```
```
yarn serve
```
## 在线演示

地址：https://www.matoapp.net:3003/
