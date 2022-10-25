<template>
  <div class="content">
    <image class="logo" src="../../static/logo.png"></image>
    <div class="text-area">
      <text class="title">{{ title }}</text>
    </div>
    <div class="main-area">
      <button
        @click="handleWxLogin"
        :disabled="isInvalid || loading"
        class="sub-btn"
      >
        授权登录
      </button>

      <button @click="cancelWxLogin" :disabled="loading" class="sub-btn">
        取消
      </button>
      <div style="text">
        <text v-show="isInvalid">此二维码已失效，请重新扫码</text>
      </div>
      <div><text v-show="loading">正在请求数据，请稍候</text></div>
    </div>
  </div>
</template>

<script lang='ts'>
import { getCancelToken, request } from "./ajaxRequire";

export default {
  data() {
    return {
      title: "欢迎使用扫码登录",
      loading: false,
      isInvalid: false,

      loginExternalForms: {
        WeChat: {
          token: "",
          providerAccessCode: "",
        },
      },
      prefix: "https://api.matoapp.net:3002/api/services/app",
    };
  },
  onLoad(option) {
    this.loginExternalForms.WeChat.token = option.scene;
    this.start();
  },
  methods: {
    successMessage(value = "执行成功") {
      uni.showToast({
        title: value,
        icon: "success",
        duration: 1.5 * 1000,
      });
    },
    errorMessage(value = "执行错误") {
      uni.showToast({
        title: value,
        icon: "error",
        duration: 1.5 * 1000,
      });
    },
    async start() {
      var currentForms = this.loginExternalForms["WeChat"];
      this.loading = true;
      await request(`${this.prefix}/MiniProgram/Access`, "post", currentForms)
        .then((re) => {
          this.successMessage("您已扫描二维码，请点击确认登录以完成");
        })
        .catch((c) => {
          var err = c.response?.data?.error?.message;
          if (err != null) {
            if (err == "WechatMiniProgramLoginInvalidToken") {
              this.isInvalid = true;
            } else {
              this.errorMessage(c.err);
            }
          }
        })
        .finally(() => {
          setTimeout(() => {
            this.loading = false;
          }, 1.5 * 1000);
        });
    },
    async handleWxLogin() {
      const that = this;
      uni.login({
        provider: "weixin",
        success: (loginRes) => {
          that.loginExternalForms.WeChat.providerAccessCode = loginRes.code;
          that.handleExternalLogin("WeChat");
        },
      });
    },

    cancelWxLogin() {
      uni.redirectTo({
        url: "/pages/index/index",
      });
    },

    async handleExternalLogin(authProvider) {
      var currentForms = this.loginExternalForms[authProvider];
      this.loading = true;
      await request(
        `${this.prefix}/MiniProgram/Authenticate`,
        "post",
        currentForms
      )
        .then((re) => {
          uni.redirectTo({
            url: "/pages/index/index",
          });
        })
        .catch((c) => {
          var err = c.response?.data?.error?.message;
          if (err != null) {
            if (err == "WechatMiniProgramLoginInvalidToken") {
              this.isInvalid = true;
            } else {
              this.errorMessage(c.err);
            }
          }
          setTimeout(() => {
            this.loading = false;
          }, 1.5 * 1000);
        });
    },
  },
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

.main-area {
  margin-top: 150px;
  width: 100%;
}
.sub-btn {
  margin: 10px 15px;
  line-height: 50px;
}
.text-area {
  display: flex;
  justify-content: center;
}

.title {
  font-size: 36rpx;
  color: #8f8f94;
}
</style>
