<template>
  <div class="content">
    <image class="logo" src="../../static/logo.png"></image>
    <div class="text-area">
      <text class="title">{{ title }}</text>
    </div>
    <div>
      <div class="main-area">
        <text class="sub-title">使用帮助</text>
      </div>
      <div class="main-area">
        <ul>
          <li>
            请在电脑浏览器打开:<br/>https://www.matoapp.net:3003/
          </li>
          <li>使用微信扫描Web端的小程序码</li>
          <li>等待微信小程序跳转至授权页面</li>
          <li>点击授权登录按钮，完成登录</li>
        </ul>
      </div>
      <div class="main-area sub-title">
        <text>详情请查看系列博客⌈使用 Abp.Zero 搭建第三方登录模块⌋</text>
      </div>
      <div class="main-area">
        <text>https://blog.csdn.net/jevonsflash/</text>
      </div>
    </div>
  </div>
</template>

<script lang='ts'>
export default {
  data() {
    return {
      title: "abp mp auth sample",
    };
  }
};
</script>

<style>
.content {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
}

.logo {
  height: 200rpx;
  width: 200rpx;
  margin-top: 200rpx;
  margin-left: auto;
  margin-right: auto;
  margin-bottom: 50rpx;
}

.text-area {
  display: flex;
  justify-content: center;
}
.main-area {
  margin: 10px;
}
.sub-title {
  font-weight: bold;
  font-size: 36rpx;
  margin-top: 5px;
  color: #000000;
}
.title {
  font-size: 36rpx;
  color: #8f8f94;
}
</style>
