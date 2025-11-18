using UnityEngine;
using UnityEngine.UI;

public class IAPTestUI : MonoBehaviour
{
    private void Start()
    {
        // 自动创建测试UI
        CreateTestUI();

        // 注册事件监听
        RegisterIAPEvents();
    }

    // 注册 IAP 事件
    private void RegisterIAPEvents()
    {
        IAPManager.Instance.PurchaseSuccessEvent += OnPurchaseSuccess;
        IAPManager.Instance.PurchaseFailedEvent += OnPurchaseFailed;
        IAPManager.Instance.InitializeSuccessEvent += OnInitializeSuccess;
        IAPManager.Instance.InitializeFailedEvent += OnInitializeFailed;
    }

    private void OoDestroy()
    {
        CancelInvoke();
    }


    // 创建测试UI
    private void CreateTestUI()
    {
        GameObject scrollView = new GameObject("IAP Test ScrollView");
        scrollView.transform.SetParent(transform, false);

        RectTransform scrollRect = scrollView.AddComponent<RectTransform>();
        scrollRect.anchorMin = new Vector2(0, 0);
        scrollRect.anchorMax = new Vector2(1, 1);
        scrollRect.offsetMin = new Vector2(20, 20);
        scrollRect.offsetMax = new Vector2(-20, -20);
    
        ScrollRect scroll = scrollView.AddComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.vertical = true;

        // 创建内容容器
        GameObject content = new GameObject("Content");
        content.transform.SetParent(scrollView.transform, false);

        RectTransform contentRect = content.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);

        VerticalLayoutGroup layout = content.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 10;
        layout.padding = new RectOffset(10, 10, 10, 10);
        layout.childControlHeight = false;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;    

        ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scroll.content = contentRect;

        CreateTitle(content.transform, "IAP 测试界面");
        CreateTitle(content.transform, $"平台: {IAPManager.Instance.GetCurrentStoreName()}", 30);
    
    // 创建礼包购买按钮
        CreateSectionTitle(content.transform, "=== 礼包商品 ===");
        CreatePurchaseButton(content.transform, "礼包1", IAPManager.package1);
    
    // 创建功能按钮
        CreateSectionTitle(content.transform, "=== 其他功能 ===");
        CreateFunctionButton(content.transform, "恢复购买", () =>
        {
            IAPManager.Instance.RestorePurchases();
        });

        CreateFunctionButton(content.transform, "刷新商品信息", () =>
        {
            ShowAllProducts();
        });
    
    }

    // 创建标题
    private void CreateTitle(Transform parent, string title, int fontSize = 40)
    {
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(parent, false);

        RectTransform rect = titleObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0, fontSize + 20);

        Text text = titleObj.AddComponent<Text>();
        text.text = title;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
    }

    private void CreateSectionTitle(Transform parent, string title)
    {
        GameObject titleObj = new GameObject("SectionTitle");
        titleObj.transform.SetParent(parent, false);

        RectTransform rect = titleObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0, 40);

        Text text = titleObj.AddComponent<Text>();
        text.text = title;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 28;
        text.alignment = TextAnchor.MiddleLeft;
        text.color = new Color(1f, 0.9f, 0.4f);
        text.fontStyle = FontStyle.Bold;
    }

    /// <summary>
    /// 创建购买按钮
    /// </summary>
    private void CreatePurchaseButton(Transform parent, string displayName, string productId)
    {
        GameObject buttonObj = new GameObject($"Btn_{displayName}");
        buttonObj.transform.SetParent(parent, false);

        RectTransform rect = buttonObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0, 80);

        Image image = buttonObj.AddComponent<Image>();
        image.color = new Color(0.2f, 0.6f, 0.8f);

        Button button = buttonObj.AddComponent<Button>();
        button.targetGraphic = image;

        // 添加按钮文本
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);

        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        Text text = textObj.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 24;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;

        // 设置按钮文本内容（显示价格）
        UpdateButtonText(text, displayName, productId);

        // 添加点击事件
        button.onClick.AddListener(() =>
        {
            Debug.Log($"[测试] 点击购买: {displayName} ({productId})");
            IAPManager.Instance.BuyProduct(productId);
        });

        // 添加悬停效果
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.2f, 0.6f, 0.8f);
        colors.highlightedColor = new Color(0.3f, 0.7f, 0.9f);
        colors.pressedColor = new Color(0.1f, 0.5f, 0.7f);
        colors.selectedColor = new Color(0.3f, 0.7f, 0.9f);
        button.colors = colors;
    }

    /// <summary>
    /// 创建功能按钮
    /// </summary>
    private void CreateFunctionButton(Transform parent, string buttonText, System.Action onClick)
    {
        GameObject buttonObj = new GameObject($"Btn_{buttonText}");
        buttonObj.transform.SetParent(parent, false);

        RectTransform rect = buttonObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0, 80);

        Image image = buttonObj.AddComponent<Image>();
        image.color = new Color(0.4f, 0.7f, 0.4f);

        Button button = buttonObj.AddComponent<Button>();
        button.targetGraphic = image;

        // 添加按钮文本
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);

        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        Text text = textObj.AddComponent<Text>();
        text.text = buttonText;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 24;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;

        // 添加点击事件
        button.onClick.AddListener(() => onClick?.Invoke());
    }

    /// <summary>
    /// 更新按钮文本（显示商品信息）
    /// </summary>
    private void UpdateButtonText(Text text, string displayName, string productId)
    {
        if (IAPManager.Instance.IsInitialized())
        {
            string price = IAPManager.Instance.GetProductPrice(productId);
            string title = IAPManager.Instance.GetProductTitle(productId);

            if (!string.IsNullOrEmpty(title))
            {
                text.text = $"{displayName}\n{title}\n{price}";
            }
            else
            {
                text.text = $"{displayName}\n{productId}\n加载中...";
            }
        }
        else
        {
            text.text = $"{displayName}\n初始化中...";
        }
    }

    /// <summary>
    /// 显示所有商品信息
    /// </summary>
    private void ShowAllProducts()
    {
        Debug.Log("========== 所有商品信息 ==========");

        string[] allProducts = new string[]
        {
            IAPManager.package1
        };

        foreach (string productId in allProducts)
        {
            var product = IAPManager.Instance.GetProduct(productId);
            if (product != null)
            {
                Debug.Log($"商品: {productId}");
                Debug.Log($"  标题: {product.metadata.localizedTitle}");
                Debug.Log($"  价格: {product.metadata.localizedPriceString}");
                Debug.Log($"  描述: {product.metadata.localizedDescription}");
                Debug.Log($"  可购买: {product.availableToPurchase}");
                Debug.Log("---");
            }
        }
    }

    #region IAP 事件回调

    private void OnInitializeSuccess()
    {
        Debug.Log("✅ [测试UI] IAP 初始化成功！");
        ShowNotification("IAP 初始化成功", Color.green);

        // 刷新所有按钮显示
        RefreshAllButtons();
    }

    private void OnInitializeFailed(string error)
    {
        Debug.LogError($"❌ [测试UI] IAP 初始化失败: {error}");
        ShowNotification($"IAP 初始化失败: {error}", Color.red);
    }

    private void OnPurchaseSuccess(string productId)
    {
        Debug.Log($"✅ [测试UI] 购买成功: {productId}");
        ShowNotification($"购买成功: {productId}", Color.green);
    }

    private void OnPurchaseFailed(string productId, string reason)
    {
        Debug.LogError($"❌ [测试UI] 购买失败: {productId} - {reason}");
        ShowNotification($"购买失败: {reason}", Color.red);
    }

    #endregion

    /// <summary>
    /// 显示通知消息
    /// </summary>
    private void ShowNotification(string message, Color color)
    {
        // 简单的控制台输出，您可以扩展为屏幕提示
        Debug.Log($"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{message}</color>");
    }


    /// <summary>
    /// 刷新所有按钮显示
    /// </summary>
    private void RefreshAllButtons()
    {
        // 延迟刷新，等待商品信息加载
        Invoke(nameof(RefreshButtonsDelayed), 0.5f);
    }

    private void RefreshButtonsDelayed()
    {
        // 重新创建UI以刷新价格
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        CreateTestUI();
    }
}