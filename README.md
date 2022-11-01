# abp-mp-auth

![glimpse](https://raw.githubusercontent.com/jevonsflash/matoapp-samples/master/assets/glimpse.png)


搭建一套集成手机号免密登录验证与号码绑定功能的用户系统


## 开始

1. 在电脑浏览器打开 https://www.matoapp.net:3009/
2. 使用你的手机号码接收验证码

## 技术博客 

这是一篇系列博文，我将使用Abp.Zero搭建一套集成手机号免密登录验证与号码绑定功能的用户系统：

* [Abp.Zero 手机号免密登录验证与号码绑定功能的实现（一）：验证码模块](https://blog.csdn.net/jevonsflash/article/details/127538020)
* [Abp.Zero 手机号免密登录验证与号码绑定功能的实现（二）：改造Abp默认实现](https://blog.csdn.net/jevonsflash/article/details/127576427)
* [Abp.Zero 手机号免密登录验证与号码绑定功能的实现（三）：网页端开发](https://blog.csdn.net/jevonsflash/article/details/127576964)

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

地址：https://www.matoapp.net:3009/
