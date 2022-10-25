<template>
  <div id="app">
    <!-- <img alt="Vue logo" src="./assets/logo.png" />
    <HelloWorld msg="Welcome to Your Vue.js App" /> -->
    <div style="height: 450px">
      <div v-if="wechatMiniappLoginStatus == 'ACCESSED'">
        <el-result
          icon="info"
          title="已扫码"
          subTitle="请在小程序上根据提示进行操作"
        >
        </el-result>
      </div>

      <div v-else-if="wechatMiniappLoginStatus == 'AUTHORIZED'">
        <el-result
          icon="success"
          title="已授权"
          :subTitle="loading ? '请稍候..' : '正在使用微信账号登录系统'"
        >
        </el-result>
      </div>
      <div v-else class="center">
        <img
          :src="`${prefix}/MiniProgram/GetACode?scene=${getToken()}&page=${miniappPage}&mode=content`"
        />
      </div>
    </div>
    <div class="center">
      <el-button
        v-if="wechatMiniappLoginToken != null"
        type="primary"
        size="medium"
        @click="wechatMiniappLoginToken = null"
        >刷新</el-button
      >
    </div>
    <div class="center">
      <span>{{ userInfo }}</span>
    </div>
  </div>
</template>

<script lang='ts'>
import { request } from "./ajaxRequire";
import Cookies from "js-cookie";

const tokenKey = "main_token";
const setToken = (token: string) => Cookies.set(tokenKey, token);

export default {
  name: "App",
  // components: {HelloWorld},
  data: () => {
    return {
      loading: false,
      timerId: -1,
      wechatMiniappLoginToken: null,
      wechatMiniappLoginStatus: "WAIT",
      // miniappPage: "",
      // 小程序未发布时无法跳转至页面，报错41030，https://developers.weixin.qq.com/community/develop/doc/00066c970b4c90f8fd6cbe31e5b400?source=indexmixflow
      miniappPage: "pages/login/index",
      prefix: "https://api.matoapp.net:3002/api/services/app",
      userInfo: "",
      loginExternalForms: {
        WeChat: {
          authProvider: "WeChatAuthProvider",
          providerKey: "default",
          providerAccessCode: "",
        },
      },
    };
  },
  created: function () {
    this.start();
  },
  methods: {
    successMessage(value = "执行成功") {
      this.$notify({
        title: "成功",
        message: value,
        type: "success",
      });
    },

    errorMessage(value = "执行错误") {
      this.$notify.error({
        title: "错误",
        message: value,
      });
    },

    getToken() {
      if (this.wechatMiniappLoginToken == null) {
        var date = new Date();
        var token = `${(Math.random() * 100000000)
          .toFixed(0)
          .toString()
          .padEnd(8, "0")}`;
        this.wechatMiniappLoginToken = token;
      }
      return this.wechatMiniappLoginToken;
    },

    start() {
      clearInterval(this.timerId);
      this.timerId = setInterval(async () => {
        if (!this.loading) {
          this.loading = true;

          await request(
            `${this.prefix}/MiniProgram/GetToken?token=${this.wechatMiniappLoginToken}`,
            "get",
            null
          )
            .then(async (re) => {
              if (re.data.result == null) {
                this.wechatMiniappLoginStatus = "EXPIRED";
                this.wechatMiniappLoginToken = null;
                this.loading = false;
              } else {
                var result = re.data.result;
                this.wechatMiniappLoginStatus = result.status;
                if (
                  this.wechatMiniappLoginStatus == "AUTHORIZED" &&
                  result.providerAccessCode != null
                ) {
                  await this.handleWxLogin(result.providerAccessCode)
                    .then(() => {
                      this.wechatMiniappLoginToken = null;
                      this.loading = false;
                    })
                    .catch((e) => {
                      this.wechatMiniappLoginToken = null;
                      this.loading = false;
                      clearInterval(this.timerId);
                    });
                } else {
                  this.loading = false;
                }
              }
            })
            .catch((e) => {
              this.loading = false;
            });
        }
      }, 1000);
    },

    async afterLoginSuccess(userinfo) {
      clearInterval(this.timerId);
      this.successMessage("登录成功");
      this.userInfo = userinfo;
    },

    async externalLogin(userInfo: {
      authProvider: string;
      providerKey: string;
      providerAccessCode: string;
    }) {
      let authProvider = userInfo.authProvider;
      let providerKey = userInfo.providerKey;
      let providerAccessCode = userInfo.providerAccessCode;

      await request(
        `https://api.matoapp.net:3002/api/TokenAuth/ExternalAuthenticate`,
        "post",
        {
          authProvider,
          providerKey,
          providerAccessCode,
        }
      ).then(async (res) => {
        var data = res.data.result;
        setToken(data.accessToken);
      });
    },

    async handleExternalLogin(authProvider) {
      // (this.$refs.baseForm as any).validate(async (valid) => {
      //   if (valid == null) {
      var currentForms = this.loginExternalForms[authProvider];

      this.loading = true;
      return await this.externalLogin(currentForms).then(async (re) => {
        return await request(
          `${this.prefix}/User/GetCurrentUser`,
          "get",
          null
        ).then(async (re) => {
          var result = re.data.result as any;
          return await this.afterLoginSuccess(result);
        });
      });
    },

    async handleWxLogin(providerAccessCode) {
      this.loginExternalForms.WeChat.providerAccessCode = providerAccessCode;
      return await this.handleExternalLogin("WeChat");
    },
  },
};
</script>
<style scoped>
.center {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
}
</style>