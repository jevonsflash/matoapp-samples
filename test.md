# abp-mp-auth

第三方身份验证在Abp中称之为外部身份验证(ExternalAuthentication), 区别于Abp的外部身份授权(ExternalAuth)，这里Auth的全称应为Authorization，即授权。

首先应该弄清楚这两者的区别，之前的使用 Abp.Zero 搭建第三方登录模块 系列文章中描述的业务，外部身份授权(ExternalAuth)

还记得我们实现的WeChatAuthProvider吗？它继承于ExternalAuthProviderApi这个抽象类，实现的微信授权功能。所以微信登录这个动作，实际是在授权(Authorization)已有的微信账号，访问服务端资源，而身份验证(Authentication)步骤，已在其他端完成了（手机微信扫码），在服务端获取已验证好身份的第三方账户并生成Token则可以抽象的认为是授权(Authorization)行为。


从Abp接口设计上，也能看得出来两者的差别。

外部身份验证(ExternalAuthentication)关注的是校验，实现TryAuthenticateAsync并返回是否成功，而CreateUserAsync和UpdateUserAsync仅是校验流程里的一部分，不实现它并不影响身份验证结果，外部授权源的接口定义如下，

```
public interface IExternalAuthenticationSource<TTenant, TUser> where TTenant : AbpTenant<TUser> where TUser : AbpUserBase
{
    ...

    Task<bool> TryAuthenticateAsync(string userNameOrEmailAddress, string plainPassword, TTenant tenant);

    Task<TUser> CreateUserAsync(string userNameOrEmailAddress, TTenant tenant);

    Task UpdateUserAsync(TUser user, TTenant tenant);
}
```


外部授权（ExternalAuth）这一步关注的业务是拿到外部账号，如微信的OpenId，所以IExternalAuthManager重点则是GetUserInfo，而IsValidUser并没有在默认实现中使用到
```
public interface IExternalAuthManager
{
    Task<bool> IsValidUser(string provider, string providerKey, string providerAccessCode);

    Task<ExternalAuthUserInfo> GetUserInfo(string provider, string accessCode);
}
```

然而这些是从LoginManager原本实现看出的，我们可以重写这个类原本的方法，加入电话号码的处理逻辑。

在搞清楚这两个接口后，相信你会对Abp用户系统的理解更加深刻

短信获取验证码来校验，是比较常用的第三方身份验证方式，今天来做一个手机号码免密登录，并且具有绑定/解绑手机号功能的小案例

## 用户验证码校验模块

首先定义DomainService接口，我们将实现手机验证码的发送、验证码校验、解绑手机号、绑定手机号

这4个功能，并且定义用途以校验行为合法性，和用它来区分短信模板

```
public interface ICaptchaManager
{
    Task BindAsync(string token);
    Task UnbindAsync(string token);
    Task SendCaptchaAsync(long userId, string phoneNumber, string purpose);
    Task<bool> VerifyCaptchaAsync(string token, string purpose = "IDENTITY_VERIFICATION");
}
```

```
public const string LOGIN = "LOGIN";

public const string IDENTITY_VERIFICATION = "IDENTITY_VERIFICATION";

public const string BIND_PHONENUMBER = "BIND_PHONENUMBER";

public const string UNBIND_PHONENUMBER = "UNBIND_PHONENUMBER";
```

定义一个验证码Token缓存类，以及对应的缓存条目类，用于承载验证码的校验内容

```
public class SmsCaptchaTokenCache : MemoryCacheBase<SmsCaptchaTokenCacheItem>, ISingletonDependency
{
    public SmsCaptchaTokenCache() : base(nameof(SmsCaptchaTokenCache))
    {
    }
}
```
缓存条目将存储电话号码，用户Id（非登录用途）以及用途

```
public class SmsCaptchaTokenCacheItem 
{
    public string PhoneNumber { get; set; }

    public long UserId { get; set; }

    public string Purpose { get; set; }
}
```
阿里云和腾讯云提供了短信服务Sms，是国内比较常见的短信服务提供商，不需要自己写了，网上有大把的封装好的库，这里使用[AbpBoilerplate.Sms](https://github.com/MatoApps/Sms)作为短信服务库。


建立短信验证码的领域服务类SmsCaptchaManager，并注入短信服务ISmsService
```
public class SmsCaptchaManager : DomainService, ICaptchaManager
{
    private readonly ISmsService SmsService;
    private readonly UserManager _userManager;
    private readonly SmsCaptchaTokenCache captchaTokenCache;


    public static TimeSpan TokenCacheDuration = TimeSpan.FromMinutes(5);

    public SmsCaptchaManager(ISmsService SmsService,
        UserManager userManager,
        SmsCaptchaTokenCache captchaTokenCache
        )
    {
        this.SmsService=SmsService;
        _userManager=userManager;
        this.captchaTokenCache=captchaTokenCache;

    }
}
```

新建SendCaptchaAsync方法，作为短信发送和缓存Token方法，CommonHelp中的GetRandomCaptchaNumber()用于生成随机6位验证码，发送完毕后，将此验证码作为缓存条目的Key值存入

```
public async Task SendCaptchaAsync(long userId, string phoneNumber, string purpose)
{
    var captcha = CommonHelper.GetRandomCaptchaNumber();
    var model = new SendSmsRequest();
    model.PhoneNumbers= phoneNumber;
    model.SignName="MatoApp";
    model.TemplateCode= purpose switch
    {
        CaptchaPurpose.BIND_PHONENUMBER => "SMS_255330989",
        CaptchaPurpose.UNBIND_PHONENUMBER => "SMS_255330923",
        CaptchaPurpose.LOGIN => "SMS_255330901",
        CaptchaPurpose.IDENTITY_VERIFICATION => "SMS_255330974"
    };
    model.TemplateParam= JsonConvert.SerializeObject(new { code = captcha });

    var result = await SmsService.SendSmsAsync(model);

    if (string.IsNullOrEmpty(result.BizId) && result.Code!="OK")
    {
        throw new UserFriendlyException("验证码发送失败，错误信息:"+result.Message);
    }

    await captchaTokenCache.SetAsync(captcha, new SmsCaptchaTokenCacheItem()
    {
        PhoneNumber=phoneNumber,
        UserId=userId,
        Purpose=purpose
    }, absoluteExpireTime: DateTimeOffset.Now.Add(TokenCacheDuration));
}
```



## 改造Abp默认实现

改造User类，重写PhoneNumber使得电话号码为必填项，和中国大陆手机号11位长度

```
public new const int MaxPhoneNumberLength = 11;

[Required]
[StringLength(MaxPhoneNumberLength)]
public override string PhoneNumber { get; set; }


```

改造UserStore类，扩展通过PhoneNumber查找用户的方法
```

public async Task<User> FindByNameOrPhoneNumberAsync(string userNameOrPhoneNumber)
{

    return await UserRepository.FirstOrDefaultAsync(
        user => user.NormalizedUserName == userNameOrPhoneNumber || user.PhoneNumber == userNameOrPhoneNumber
    );
}

[UnitOfWork]
public async Task<User> FindByNameOrPhoneNumberAsync(int? tenantId, string userNameOrPhoneNumber)
{
    using (_unitOfWorkManager.Current.SetTenantId(tenantId))
    {
        return await FindByNameOrPhoneNumberAsync(userNameOrPhoneNumber);
    }
}
```


改造UserManager类，添加检测重复电话号码的方法CheckDuplicateUsernameOrPhoneNumber
```

public async Task<IdentityResult> CheckDuplicateUsernameOrPhoneNumber(long? expectedUserId, string userName, string phone)
{
    var user = await FindByNameAsync(userName);
    if (user != null && user.Id != expectedUserId)
    {
        throw new UserFriendlyException(string.Format(L("Identity.DuplicateUserName"), userName));
    }

    user = await FindByNameOrPhoneNumberAsync(GetCurrentTenantId(), phone);
    if (user != null && user.Id != expectedUserId)
    {
        throw new UserFriendlyException("电话号码重复", phone);
    }

    return IdentityResult.Success;
}

```

重写对用户的Create和Update，使其先检测是否重复电话号码。

```
//override

public override async Task<IdentityResult> CreateAsync(User user)
{
    var result = await CheckDuplicateUsernameOrPhoneNumber(user.Id, user.UserName, user.PhoneNumber);
    if (!result.Succeeded)
    {
        return result;
    }


    return await base.CreateAsync(user);
}

public override async Task<IdentityResult> UpdateAsync(User user)
{
    var result = await CheckDuplicateUsernameOrPhoneNumber(user.Id, user.UserName, user.PhoneNumber);
    if (!result.Succeeded)
    {
        return result;
    }

    return await base.UpdateAsync(user);
}
```

改造LogInManager类，分别重写LoginAsyncInternal，TryLoginFromExternalAuthenticationSourcesAsync两个方法，在用Email找不到用户之后，添加用手机号码查找用户的逻辑，添加的代码如下：
```
...
if (user == null)
{
    user = await userManager.FindByNameOrPhoneNumberAsync(tenantId, combinationName);
}
```






新建电话号码验证源类PhoneNumberExternalAuthenticationSource，并实现验证码校验逻辑，具体的代码
```
public class PhoneNumberExternalAuthenticationSource : DefaultExternalAuthenticationSource<Tenant, User>, ITransientDependency
{
    private readonly CaptchaManager captchaManager;

    public PhoneNumberExternalAuthenticationSource(CaptchaManager captchaManager)
    {
        this.captchaManager=captchaManager;
    }
    /// <inheritdoc/>
    public override string Name { get; } = "SMS验证码登录";

    /// <inheritdoc/>
    public override async Task<bool> TryAuthenticateAsync(string phoneNumber, string token, Tenant tenant)
    {
        //for test
        //return true;
        var currentItem = await captchaManager.GetToken(token);
        if (currentItem==null || currentItem.PhoneNumber!=phoneNumber || currentItem.Purpose!=CaptchaPurpose.LOGIN)
        {
            return false;
        }
        await captchaManager.RemoveToken(token);
        return true;
    }

    /// <inheritdoc/>
    public override Task<User> CreateUserAsync(string userNameOrEmailAddress, Tenant tenant)
    {
        var seed = Guid.NewGuid().ToString("N").Substring(0, 7);
        var surname = "手";
        var name = "机用户"+seed;
        var userName = PinyinUtil.PinYin(surname+name);

            var result = new User()
            {
                Surname = surname,
                Name = name,
                UserName =  userName,
                IsPhoneNumberConfirmed = true,
                IsActive=true,
                TenantId = tenant?.Id,
                PhoneNumber = userNameOrEmailAddress,
                Settings = null,
                IsEmailConfirmed = true,
                EmailAddress=$"{userName}@abc.com"
            };
        return Task.FromResult(result);

    }

    /// <inheritdoc/>
    public override Task UpdateUserAsync(User user, Tenant tenant)
    {
        return Task.FromResult(0);
    }

}
```