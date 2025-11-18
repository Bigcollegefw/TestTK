using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

public class IAPManager : MonoBehaviour, IDetailedStoreListener
{
    private static IAPManager _instance;
    public static IAPManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("IAPManager");
                _instance = go.AddComponent<IAPManager>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    /// 可以获取已经配置的商品列表，包含商品价格等信息，发起购买请求，连接游戏逻辑与商店服务
    private IStoreController storeController;
    //提供跨平台适配能力,扩展基础内购功能，支持各商店的独特特性（如订阅管理、退款处理等）。
    private IExtensionProvider extensionProvider;
    private bool isInitialized = false;


    #region 商品ID定义 - 请根据您在商店中配置的商品ID修改
    // 消耗型商品（金币、礼包等）
    public const string package1 = "com.dp.moodtopia.puzzle.io.package1";
    #endregion

    #region 事件回调

    /// <summary>
    /// 购买成功事件 - 参数：商品ID
    /// </summary>
    public event Action<string> PurchaseSuccessEvent;

    /// <summary>
    /// 购买失败事件 - 参数：商品ID, 错误信息
    /// </summary>
    public event Action<string, string> PurchaseFailedEvent;

    /// <summary>
    /// 初始化成功事件
    /// </summary>
    public event Action InitializeSuccessEvent;

    /// <summary>
    /// 初始化失败事件 - 参数：错误信息
    /// </summary>
    public event Action<string> InitializeFailedEvent;

    #endregion


    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    void OnDestroy()
    {
        // 清理单例引用
        if (_instance == this)
        {
            _instance = null;
        }
    }

    //async 只是给 Start() 加了 “不卡线程的能力”，没改变它 “Unity 自动调用” 的本质。
    async void Start()// 1. 标记这是异步方法，允许用 await
    {
        try
        {
            // 执行到这里，遇到 await，触发异步操作
            await InitializeUnityGamingServices();

            // 验证 Unity Services 状态后再初始化 IAP

            if (UnityServices.State == ServicesInitializationState.Initialized)
            {
                Debug.Log("[IAP] ✅ Unity Services 已就绪，开始初始化 IAP...");
                InitializePurchasing();
            }
            else
            {
                Debug.LogError($"[IAP] ❌ Unity Services 未正确初始化，状态: {UnityServices.State}");
                Debug.LogError("[IAP] IAP 初始化已取消，请检查网络连接和Unity Cloud配置");
                InitializeFailedEvent?.Invoke($"Unity Services 初始化失败: {UnityServices.State}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[IAP] ❌ 启动过程出错: {e.Message}\n{e.StackTrace}");
            InitializeFailedEvent?.Invoke($"启动异常: {e.Message}");
        }
    }

    // UnityGamingService是Unity整合的“游戏后台服务集合”，
    // 内购需要确认 “当前设备是否有权限访问商店”“购买记录是否能同步到云端”，
    // 这些都需要通过 UGS 的 “统一授权通道” 来完成
    private async Task InitializeUnityGamingServices()
    {
        try
        {
            Debug.Log("[IAP] 开始初始化 Unity Gaming Services...");
            // 检查 UnityServices 是否可用
            if (UnityServices.State == ServicesInitializationState.Uninitialized)
            {
                // 根据构建配置自动选择环境
                // development: 开发测试环境（Unity编辑器、开发构建）
                // production: 生产环境（正式发布版本）
#if DEVELOPMENT_BUILD || UNITY_EDITOR
                string environmentName = "development";
#else
                string environmentName = "production";
#endif
                Debug.Log($"[IAP] 使用环境: {environmentName}");

                var options = new InitializationOptions().SetEnvironmentName(environmentName);
                await UnityServices.InitializeAsync(options);
                Debug.Log($"[IAP] ✅ Unity Gaming Services 初始化成功，状态: {UnityServices.State}");
            }
            else if (UnityServices.State == ServicesInitializationState.Initialized)
            {
                Debug.Log($"[IAP] Unity Gaming Services 已经初始化，当前状态: {UnityServices.State}");
            }
            else
            {
                Debug.LogWarning($"[IAP] Unity Gaming Services 状态异常: {UnityServices.State}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[IAP] ❌ Unity Gaming Services 初始化失败: {e.Message}");
            Debug.LogError($"[IAP] 堆栈: {e.StackTrace}");

            // 抛出异常，阻止后续的 IAP 初始化
            // 关键：打印所有 inner exception（子服务失败的具体原因）
            Exception innerException = e.InnerException;
            int innerLevel = 1;
            while (innerException != null)
            {
                Debug.LogError($"[IAP] 内部异常（{innerLevel}层）: {innerException.Message}");
                Debug.LogError($"[IAP] 内部堆栈（{innerLevel}层）: {innerException.StackTrace}");
                innerException = innerException.InnerException;
                innerLevel++;
            }
            throw;
        }
    }

    /// <summary>
    /// 初始化 IAP 系统
    /// </summary>
    public void InitializePurchasing()
    {
        if (isInitialized)
        {
            Debug.Log("[IAP] 已经初始化完成");
            return;
        }

        // 关键检查：确保 Unity Gaming Services 已初始化
        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            Debug.LogError("[IAP] ❌ Unity Gaming Services 未初始化，无法启动 IAP");
            Debug.LogError($"[IAP] 当前状态: {UnityServices.State}");
            Debug.LogError("[IAP] 请确保已在 Unity Editor 中连接 Unity Cloud (Window → Services)");
            InitializeFailedEvent?.Invoke($"Unity Services 未就绪: {UnityServices.State}");
            return;
        }

        Debug.Log("[IAP] 开始初始化 Unity IAP...");

        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

        // ===== 添加消耗型商品 =====
        builder.AddProduct(package1, ProductType.Consumable);
        // 开始异步初始化
        // 调用 Unity IAP 的初始化方法，传入两个参数：
        // 1. this：当前 IAPManager 实例（因为 IAPManager 实现了 IDetailedStoreListener 接口，能接收初始化结果回调）
        // 2. builder：前面创建的商品配置器（商品清单）
        UnityPurchasing.Initialize(this, builder);

        //UnityPurchasing.Initialize(this, builder) 执行成功后，
        // 会走到 IAPManager 实现的 OnInitialized 接口方法

    }
    
    // 购买指定商品
    public void BuyProduct(string productId)
    {
        if (!isInitialized)
        {
            Debug.LogError($"[IAP] IAP 未初始化，无法购买商品: {productId}");
            PurchaseFailedEvent?.Invoke(productId, "IAP 未初始化");
            return;
        }
        Product product = storeController.products.WithID(productId);
        if (product != null && product.availableToPurchase)
        {
            Debug.Log($"[IAP] 开始购买商品: {product.definition.id}");
            Debug.Log($"[IAP] 价格: {product.metadata.localizedPriceString}");
            storeController.InitiatePurchase(product);
        }
        else
        {
            string error = product == null ? "商品不存在" : "商品不可购买";
            Debug.LogError($"[IAP] 无法购买商品 {productId}: {error}");
            PurchaseFailedEvent?.Invoke(productId, error);
        }
    }

    /// <summary>
    /// 恢复购买（用于非消耗型商品和订阅）
    /// </summary>
    public void RestorePurchases()
    {
        if (!isInitialized)
        {
            Debug.LogError("[IAP] IAP 未初始化，无法恢复购买");
            return;
        }

#if UNITY_IOS
        // iOS 需要显式调用恢复购买
        Debug.Log("[IAP] iOS: 开始恢复购买...");
        var apple = extensionProvider.GetExtension<IAppleExtensions>();
        apple.RestoreTransactions((result) =>
        {
            if (result)
            {
                Debug.Log("[IAP] iOS: 恢复购买成功");
            }
            else
            {
                Debug.Log("[IAP] iOS: 恢复购买失败或没有可恢复的商品");
            }
        });
#elif UNITY_ANDROID
        // Android Google Play 会自动恢复购买
        Debug.Log("[IAP] Android: Google Play 会自动恢复购买");

        // 遍历所有已购买的商品
        int restoredCount = 0;
        foreach (var product in storeController.products.all)
        {
            if (product.hasReceipt && product.definition.type != ProductType.Consumable)
            {
                Debug.Log($"[IAP] 已购买商品: {product.definition.id}");
                restoredCount++;
            }
        }
        Debug.Log($"[IAP] 共恢复 {restoredCount} 个非消耗型商品");
#endif
    }

    /// <summary>
    /// 获取商品信息
    /// </summary>
    public Product GetProduct(string productId)
    {
        if (isInitialized)
        {
            return storeController.products.WithID(productId);
        }
        return null;
    }

    /// <summary>
    /// 获取商品价格（本地化显示）
    /// </summary>
    public string GetProductPrice(string productId)
    {
        if (isInitialized)
        {
            Product product = storeController.products.WithID(productId);
            if (product != null && product.metadata != null)
            {
                return product.metadata.localizedPriceString;
            }
        }
        return "N/A";
    }

    /// <summary>
    /// 获取商品标题（本地化显示）
    /// </summary>
    public string GetProductTitle(string productId)
    {
        if (isInitialized)
        {
            Product product = storeController.products.WithID(productId);
            if (product != null && product.metadata != null)
            {
                return product.metadata.localizedTitle;
            }
        }
        return "";
    }

    /// <summary>
    /// 检查 IAP 是否已初始化
    /// </summary>
    public bool IsInitialized()
    {
        return isInitialized;
    }
    
    /// <summary>
    /// 获取当前平台名称
    /// </summary>
    public string GetCurrentStoreName()
    {
#if UNITY_ANDROID
        return "Google Play";
#elif UNITY_IOS
        return "Apple App Store";
#else
        return "Unknown";
#endif
    }


    #region IDetailedStoreListener 接口实现
    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        Debug.Log($"[IAP] ✅ Unity IAP 初始化成功！当前平台: {GetCurrentStoreName()}");
        storeController = controller;
        extensionProvider = extensions;
        isInitialized = true;

        // 打印所有可用商品信息
        Debug.Log($"[IAP] 可用商品数量: {controller.products.all.Length}");
        foreach (var product in controller.products.all)
        {
            if (product.availableToPurchase)
            {
                Debug.Log($"[IAP] 商品: {product.definition.id}");
                Debug.Log($"      标题: {product.metadata.localizedTitle}");
                Debug.Log($"      价格: {product.metadata.localizedPriceString}");
                Debug.Log($"      类型: {product.definition.type}");
            }
        }

        // 平台特定的初始化
        InitializePlatformSpecific();

        // 检查并处理待处理的购买（补单逻辑）
        CheckPendingPurchases();

        InitializeSuccessEvent?.Invoke();
    }

    private void InitializePlatformSpecific()
    {
#if UNITY_IOS
        // iOS 特定设置,
        // 通过 extensionProvider.GetExtension<接口名>() 就能拿到这些平台专属的 “工具”，进而配置特有逻辑。
        var appleExtensions = extensionProvider.GetExtension<IAppleExtensions>();
        
        // 注册延迟购买监听（适用于"询问购买"功能）
        // iOS 有一个特有场景 ——“家长控制”（或 “询问购买” 功能）：
        // 当未成年人点击购买时，系统会弹出 “需要家长批准” 的提示，
        // 此时购买不会立即完成，而是进入 “延迟状态”（等待家长在另一设备上批准）。
        appleExtensions.RegisterPurchaseDeferredListener(OnPurchaseDeferred);
        //通过 RegisterPurchaseDeferredListener 注册一个 “延迟购买的回调方法
        // （OnPurchaseDeferred）”，当 iOS 触发 “延迟购买” 时，会自动调用 
        // OnPurchaseDeferred 方法 —— 会打印日志（Debug.Log("[IAP] iOS: 
        // 购买已延迟（等待批准）: {productId}")），方便开发者知道 
        // “当前购买处于等待状态”，后续也可以基于这个回调做 UI 提示）

        Debug.Log("[IAP] iOS: Apple Extensions 已初始化");
#elif UNITY_ANDROID
        // Android 特定设置
        var googleExtensions = extensionProvider.GetExtension<IGooglePlayStoreExtensions>();

        // Google Play Extensions 已初始化
        // 注意：Unity IAP 4.12.2 已经自动处理延迟支付和订阅变更
        // 无需手动设置监听器，系统会自动处理


        Debug.Log("[IAP] Android: Google Play Extensions 已初始化");
        Debug.Log("[IAP] Android: 自动支持延迟支付和订阅功能");
#endif
    }


    // 检查并处理待处理的购买（补单逻辑）
    private void CheckPendingPurchases()
    {
        Debug.Log("[IAP] 开始检查待处理的购买...");
        int pendingCount = 0;
        foreach (var product in storeController.products.all)
        {
            // 只处理有收据的消耗型商品（非消耗型商品会自动恢复）
            if (product.hasReceipt && product.definition.type == ProductType.Consumable)
            {
                Debug.Log($"[IAP] 发现待处理购买: {product.definition.id}");
                Debug.Log($"[IAP] 交易ID: {product.transactionID}");

                // 直接处理购买（不需要重新创建PurchaseEventArgs）
                // Unity IAP会在启动时自动触发未完成的购买回调
                // 这里只是记录日志，实际补单由Unity IAP自动处理


                pendingCount++;
            }
        }
        if (pendingCount > 0)
        {
            Debug.Log($"[IAP] ⚠️ 发现 {pendingCount} 个待处理购买");
            Debug.Log($"[IAP] Unity IAP 将自动重新触发 ProcessPurchase 回调");
        }
        else
        {
            Debug.Log("[IAP] ✅ 没有待处理的购买");
        }
    }

#if UNITY_IOS
    /// <summary>
    /// iOS 延迟购买回调（家长控制等情况）
    /// </summary>
    private void OnPurchaseDeferred(Product product)
    {
        Debug.Log($"[IAP] iOS: 购买已延迟（等待批准）: {product.definition.id}");
    }
#endif

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        OnInitializeFailed(error, "详细信息不可用（旧版回调）");
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
         Debug.LogError($"[IAP] ❌ Unity IAP 初始化失败: {error}");
        Debug.LogError($"[IAP] 错误消息: {message}");

        // 根据错误类型给出具体建议

        switch (error)
        {
            case InitializationFailureReason.PurchasingUnavailable:
                Debug.LogError("[IAP] 购买功能不可用。请检查：");
                Debug.LogError("[IAP] 1. 应用是否已上传到Google Play Console（至少内部测试）");
                Debug.LogError("[IAP] 2. 设备是否安装了Google Play商店");
                Debug.LogError("[IAP] 3. 应用签名是否与Google Play Console一致");
                Debug.LogError("[IAP] 4. Package Name是否正确: " + Application.identifier);
                break;
            case InitializationFailureReason.NoProductsAvailable:
                Debug.LogError("[IAP] 没有可用商品。请检查：");
                Debug.LogError("[IAP] 1. Google Play Console中是否已创建商品");
                Debug.LogError("[IAP] 2. 商品是否已激活");
                Debug.LogError("[IAP] 3. 商品ID是否与代码中一致");
                break;
            case InitializationFailureReason.AppNotKnown:
                Debug.LogError("[IAP] 应用未知。请检查：");
                Debug.LogError("[IAP] 1. Package Name是否正确: " + Application.identifier);
                Debug.LogError("[IAP] 2. 应用是否已上传到Google Play Console");
                Debug.LogError("[IAP] 3. 应用版本是否已发布到测试轨道");
                break;
        }


        InitializeFailedEvent?.Invoke($"{error}: {message}");
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
    {
        throw new System.NotImplementedException();
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        throw new System.NotImplementedException();
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        string productId = args.purchasedProduct.definition.id;
        Debug.Log($"[IAP] ✅ 购买成功: {productId}");
        Debug.Log($"[IAP] 交易ID: {args.purchasedProduct.transactionID}");

        // 验证收据（可选，建议在服务器端验证）
        bool validPurchase = ValidateReceipt(args.purchasedProduct);

        if (!validPurchase)
        {
            Debug.LogWarning($"[IAP] ⚠️ 收据验证失败，可能是伪造购买: {productId}");
            // 生产环境中应该拒绝此购买
            // return PurchaseProcessingResult.Complete;
        }

        // 处理购买
        bool processed = HandlePurchase(productId);

        if (processed)
        {
            PurchaseSuccessEvent?.Invoke(productId);
            return PurchaseProcessingResult.Complete;
        }
        else
        {
            // 如果处理失败，返回 Pending，下次启动时会再次调用
            Debug.LogWarning($"[IAP] ⚠️ 购买处理失败，标记为 Pending: {productId}");
            return PurchaseProcessingResult.Pending;
        }
    }


    private bool ValidateReceipt(Product product)
    {
        bool validPurchase = true;

        // TODO: 配置完 Google Play License Key 后，取消下面的注释以启用收据验证
        // 当前为了避免编译错误，暂时注释掉收据验证代码
        // 生成 Tangle 文件后（Obfuscate License Keys），取消注释即可启用

        /* 启用收据验证的步骤：
            * 1. 在 Unity Editor 中打开：Window → Unity IAP → Receipt Validation Obfuscator
            * 2. 在 Receipt Obfuscator 部分输入 Google Play License Key
            * 3. 点击 "Obfuscate License Keys" 按钮
            * 4. 等待生成 GooglePlayTangle.cs 和 AppleTangle.cs 文件
            * 5. 取消下面的注释即可启用收据验证
            */

        /* 
#if UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE_OSX
        try
        {
            var validator = new CrossPlatformValidator(
                GooglePlayTangle.Data(),
                AppleTangle.Data(),
                Application.identifier
            );

            var result = validator.Validate(product.receipt);

            Debug.Log("[IAP] 收据验证通过");

            foreach (IPurchaseReceipt productReceipt in result)
            {
                Debug.Log($"[IAP] 收据详情: {productReceipt.productID}");
                Debug.Log($"[IAP] 购买日期: {productReceipt.purchaseDate}");
            }
        }
        catch (IAPSecurityException ex)
        {
            Debug.LogError($"[IAP] 收据验证失败: {ex.Message}");
            validPurchase = false;
        }
#else
        Debug.LogWarning("[IAP] 收据验证仅在 Android/iOS 平台可用");
#endif
        */

        // 暂时跳过验证（开发阶段）
        Debug.LogWarning("[IAP] 收据验证已禁用。配置 License Key 后可以启用。");

        return validPurchase;
    }

    /// <summary>
    /// 处理购买 - 发放商品给玩家
    /// </summary>
    private bool HandlePurchase(string productId)
    {
        try
        {
            // ===== 根据商品ID发放对应的奖励 =====
            switch (productId)
            {
                case package1:
                    Debug.Log("[IAP] 发放 礼包1");
                    // TODO: 调用您的游戏逻辑
                    // 例如: MainData.Instance.AddCoins(600);
                    return true;
                default:
                    Debug.LogWarning($"[IAP] ⚠️ 未处理的商品: {productId}");
                    return false;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[IAP] ❌ 处理购买时发生错误: {e.Message}\n{e.StackTrace}");
            return false;
        }
    }

    #endregion

}
