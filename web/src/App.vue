<template>
  <div id="app">
    <!-- <img alt="Vue logo" src="./assets/logo.png" />
    <HelloWorld msg="Welcome to Your Vue.js App" /> -->
    <el-row>
      <div class="title-container center">
        <h3 class="title">欢迎登录</h3>
      </div>
    </el-row>
    <el-row v-if="token == null">
      <el-tabs
        tab-position="top"
        style="display: inline"
        v-model="activeName"
        :stretch="true"
      >
        <el-tab-pane label="账号密码登录" name="account">
          <el-form
            ref="loginForm"
            :model="loginForm"
            class="login-form"
            autocomplete="on"
            label-position="left"
          >
            <el-form-item class="item" prop="username">
              <el-input
                ref="username"
                v-model="loginForm.username"
                :placeholder="'请输入用户名/手机号/邮箱'"
                name="username"
                type="text"
                tabindex="1"
                autocomplete="on"
              >
                <span slot="prepend" class="svg-container">
                  <i class="el-icon-user" />
                </span>
              </el-input>
            </el-form-item>

            <el-tooltip
              v-model="capsTooltip"
              content="大写锁定已打开"
              placement="right"
              manual
            >
              <el-form-item class="item" prop="password">
                <el-input
                  :key="passwordType"
                  ref="password"
                  v-model="loginForm.password"
                  :type="passwordType"
                  :placeholder="'请输入密码'"
                  name="password"
                  tabindex="2"
                  autocomplete="on"
                  @keyup.native="checkCapslock"
                  @blur="capsTooltip = false"
                  @keyup.enter.native="handleLogin"
                >
                  <span slot="prepend" class="svg-container">
                    <i class="el-icon-lock" />
                  </span>
                  <span slot="append" class="show-pwd" @click="showPwd">
                    <i
                      :class="
                        passwordType == 'password'
                          ? 'el-icon-view'
                          : 'el-icon-minus'
                      "
                    />
                  </span>
                </el-input>
              </el-form-item>
            </el-tooltip>

            <el-row type="flex" class="row-bg" justify="center">
              <el-col :span="20">
                <el-button
                  :loading="loading"
                  type="primary"
                  style="width: 100%"
                  @click.native.prevent="handleLogin"
                >
                  登录
                </el-button>
              </el-col>
            </el-row>
          </el-form>
        </el-tab-pane>

        <el-tab-pane label="手机免密登录" name="phoneNumber">
          <el-form
            ref="loginForm"
            :model="loginForm"
            class="login-form"
            autocomplete="on"
            label-position="left"
          >
            <el-form-item class="item" prop="username">
              <el-input
                ref="username"
                v-model="loginForm.username"
                :placeholder="'请输入手机号'"
                type="text"
                tabindex="1"
                autocomplete="on"
              >
                <template slot="prepend">+86</template>
              </el-input>
            </el-form-item>

            <el-form-item class="item" prop="password">
              <el-input
                ref="password"
                v-model="loginForm.password"
                :type="passwordType"
                :placeholder="'发送验证码后键入验证码'"
                tabindex="2"
                autocomplete="on"
                @blur="capsTooltip = false"
                @keyup.enter.native="handleLogin"
              >
                <el-button
                  slot="append"
                  :disabled="smsSendCd > 0"
                  @click="sendVerificationCode(loginForm.username, 'LOGIN')"
                  >{{
                    smsSendCd == 0 ? "发送验证码" : smsSendCd + "后重试"
                  }}</el-button
                >
              </el-input>
            </el-form-item>

            <el-row type="flex" class="row-bg" justify="center">
              <el-col :span="20">
                <el-button
                  :disabled="currentVerifyingType == null"
                  :loading="loading"
                  type="primary"
                  style="width: 100%"
                  @click.native.prevent="handleLogin"
                >
                  免密登录
                </el-button>
              </el-col>
            </el-row>
          </el-form>
        </el-tab-pane>
      </el-tabs>
    </el-row>
    <el-row v-else>
      <el-button
        :loading="loading"
        type="danger"
        style="width: 100%"
        @click.native.prevent="logout"
      >
        退出登录
      </el-button>
    </el-row>
    <el-row class="center" v-if="userInfo != null">
      <span>{{ userInfo }}</span>

      <el-form
        ref="userInfo"
        :model="userInfo"
        autocomplete="on"
        label-position="left"
      >
        <el-form-item :span="12" label="当前手机号" :required="true">
          <el-input v-model="userInfo.phoneNumber"> </el-input
        ></el-form-item>

        <el-button
          v-if="!userInfo.isPhoneNumberConfirmed"
          size="mini"
          type="primary"
          :disabled="smsSendCd > 0"
          @click="
            sendVerificationCode(userInfo.phoneNumber, 'BIND_PHONENUMBER')
          "
          >{{ smsSendCd == 0 ? "验证手机号" : smsSendCd + "后重试" }}</el-button
        >
        <el-button
          v-else
          size="mini"
          type="danger"
          :disabled="smsSendCd > 0"
          @click="
            sendVerificationCode(userInfo.phoneNumber, 'UNBIND_PHONENUMBER')
          "
          >{{ smsSendCd == 0 ? "解绑手机号" : smsSendCd + "后重试" }}</el-button
        >

        <el-form-item
          v-if="currentVerifyingType != null"
          :span="12"
          label="验证码"
          :required="true"
        >
          <el-input v-model="verifyNumber">
            <el-button
              slot="append"
              type="primary"
              size="mini"
              @click="verify"
            >
              完成验证</el-button
            >
          </el-input></el-form-item
        >
      </el-form>
    </el-row>
  </div>
</template>

<script lang='ts'>
import { request } from "./ajaxRequire";
import Cookies from "js-cookie";

const tokenKey = "main_token";
const setToken = (token: string) => Cookies.set(tokenKey, token);
const getToken = () => Cookies.get(tokenKey);
export default {
  name: "App",
  // components: {HelloWorld},
  data: () => {
    return {
      loading: false,
      // host: "https://api.matoapp.net:3002/",
      host: "http://localhost:44311/",
      prefix: "api/services/app",
      userInfo: null,                   //用户对象
      currentVerifyingType: null,       //当前发送验证码的用途
      smsSendCd: 0,                     //发送验证码的冷却时间，默认60s
      activeName: "account",           
      loginForm: {                      //登录对象
        username: "",
        password: "",
      },
      passwordType: "password",
      capsTooltip: false,
      token: null,                      //登录凭证Token
      verifyNumber: null,               //填写的验证码
      timer: null,
    };
  },
  created: async function () {
    this.token = getToken();
    if (this.token != null) {
      await this.getCurrentUser();
    }
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

    showPwd() {
      if (this.passwordType === "password") {
        this.passwordType = "";
      } else {
        this.passwordType = "password";
      }
    },

    checkCapslock(e: KeyboardEvent) {
      const { key } = e;
      this.capsTooltip =
        key != null && key.length == 1 && key >= "A" && key <= "Z";
    },

    async handleLogin() {
      this.loading = true;

      let userNameOrEmailAddress = this.loginForm.username;
      let password = this.loginForm.password;
      userNameOrEmailAddress = userNameOrEmailAddress.trim();
      await request(`${this.host}api/TokenAuth/Authenticate`, "post", {
        userNameOrEmailAddress,
        password,
      })
        .catch((re) => {
          var res = re.response.data;
          this.errorMessage(res.error.message);
        })
        .then(async (res) => {
          var data = res.data.result;
          setToken(data.accessToken);
          await this.getCurrentUser();
        })
        .finally(() => {
          setTimeout(() => {
            this.loading = false;
          }, 1.5 * 1000);
        });
    },
    async getCurrentUser() {
      await request(
        `${this.host}${this.prefix}/User/GetCurrentUser`,
        "get",
        null
      )
        .catch((re) => {
          var res = re.response.data;
          this.errorMessage(res.error.message);
        })
        .then(async (re) => {
          var result = re.data.result as any;
          this.userInfo = result;
          this.token = getToken();
          clearInterval(this.timer);

          this.smsSendCd = 0;
          this.currentVerifyingType = null;

          this.successMessage("登录成功");
        });
    },

    async sendVerificationCode(phoneNumber, type) {
      this.currentVerifyingType = type;
      this.smsSendCd = 60;
      this.timer = setInterval(() => {
        this.smsSendCd--;
        if (this.smsSendCd <= 0) {
          clearInterval(this.timer);
        }
      }, 1000);
      await request(`${this.host}${this.prefix}/Captcha/Send`, "post", {
        userId: this.userInfo == null ? null : this.userInfo.id,
        phoneNumber: phoneNumber,
        type: type,
      })
        .catch((re) => {
          var res = re.response.data;
          this.errorMessage(res.error.message);
        })
        .then((re) => {
          var res = re.data.result;
          this.successMessage("发送验证码成功");
        });
    },

    logout() {
      setToken(null);
      this.token = null;
      this.userInfo = null;
    },

    async verify() {
      var action = null;
      if (this.currentVerifyingType == "BIND_PHONENUMBER") {
        action = "Bind";
      } else if (this.currentVerifyingType == "UNBIND_PHONENUMBER") {
        action = "Unbind";
      } else {
        action = "Verify";
      }
      await request(`${this.host}${this.prefix}/Captcha/${action}`, "post", {
        token: this.verifyNumber,
      })
        .catch((re) => {
          var res = re.response.data;
          this.errorMessage(res.error.message);
        })
        .then((re) => {
          var res = re.data;
          if (res.success) {
            this.successMessage("绑定成功");
            window.location.reload()   
           }
        });
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