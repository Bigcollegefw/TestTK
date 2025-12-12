// using System;
// using System.Collections;
// using System.Text;
// using UnityEngine;
// using UnityEngine.Networking;
// using TKFramework.Auth;

// namespace TKFramework
// {
//     /// <summary>
//     /// IAP订单API管理器 - 负责创建订单和验证支付
//     /// </summary>
//     public class IAPOrderAPI : MonoBehaviour
//     {
//         #region Singleton

//         private static IAPOrderAPI _instance;
//         public static IAPOrderAPI Instance
//         {
//             get
//             {
//                 if (_instance == null)
//                 {
//                     GameObject go = new GameObject("IAPOrderAPI");
//                     _instance = go.AddComponent<IAPOrderAPI>();
//                     DontDestroyOnLoad(go);
//                 }
//                 return _instance;
//             }
//         }

//         #endregion

//         #region 配置常量

//         private static string BASE_URL => CustomGlobalConfig.apiBaseUrl;
//         private static string CREATE_ORDER_URL => BASE_URL + "/order/create";
//         private static string VERIFY_PAYMENT_URL => BASE_URL + "/pay/googlePay";
//         private const int REQUEST_TIMEOUT = 15; // 请求超时时间(秒)

//         #endregion

//         #region Unity生命周期

//         private void Awake()
//         {
//             if (_instance != null && _instance != this)
//             {
//                 Destroy(gameObject);
//                 return;
//             }

//             _instance = this;
//             DontDestroyOnLoad(gameObject);
//         }

//         #endregion

//         #region 创建订单接口

//         /// <summary>
//         /// 创建订单
//         /// </summary>
//         /// <param name="goodsId">商品ID</param>
//         /// <param name="onComplete">完成回调 (成功, 订单数据, 错误信息)</param>
//         public void CreateOrder(string goodsId, Action<bool, OrderResponse, string> onComplete)
//         {
//             if (string.IsNullOrEmpty(goodsId))
//             {
//                 Debug.LogError("[IAPOrderAPI] goodsId不能为空");
//                 onComplete?.Invoke(false, null, "商品ID不能为空");
//                 return;
//             }

//             // 使用AuthManager执行需要认证的请求
//             AuthManager.Instance.ExecuteAuthenticatedRequest<OrderResponse>(
//                 (authToken, result) => CreateOrderCoroutine(goodsId, authToken, result),
//                 (success, data, error) =>
//                 {
//                     if (success)
//                     {
//                         onComplete?.Invoke(true, data, null);
//                     }
//                     else
//                     {
//                         onComplete?.Invoke(false, null, error);
//                     }
//                 }
//             );
//         }

//         private IEnumerator CreateOrderCoroutine(string goodsId, string authToken, AuthManager.RequestResult<OrderResponse> result)
//         {
//             string url = $"{CREATE_ORDER_URL}?goodsId={UnityWebRequest.EscapeURL(goodsId)}";

//             Debug.Log($"[IAPOrderAPI] 创建订单请求 - URL: {url}");

//             using (UnityWebRequest request = UnityWebRequest.Get(url))
//             {
//                 request.timeout = REQUEST_TIMEOUT;
//                 request.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
//                 request.SetRequestHeader("Authorization", authToken);

//                 yield return request.SendWebRequest();

//                 result.responseCode = (int)request.responseCode;

//                 if (request.result == UnityWebRequest.Result.Success)
//                 {
//                     try
//                     {
//                         string responseText = request.downloadHandler.text;
//                         Debug.Log($"[IAPOrderAPI] 创建订单成功 - 响应: {responseText}");

//                         // 解析响应
//                         var response = JsonUtility.FromJson<OrderResponse>(responseText);

//                         // 服务器返回code:200表示成功
//                         if (response != null && response.code == 200 && response.data != null)
//                         {
//                             Debug.Log($"[IAPOrderAPI] ✅ 订单创建成功 - 订单ID: {response.data.id}");
//                             result.success = true;
//                             result.data = response;
//                         }
//                         else
//                         {
//                             string errorMsg = response?.message ?? "未知错误";
//                             Debug.LogError($"[IAPOrderAPI] ❌ 创建订单失败 - 错误: {errorMsg}");
//                             result.success = false;
//                             result.error = errorMsg;
//                         }
//                     }
//                     catch (Exception e)
//                     {
//                         Debug.LogError($"[IAPOrderAPI] ❌ 解析订单响应失败: {e.Message}\n{e.StackTrace}");
//                         result.success = false;
//                         result.error = $"解析响应失败: {e.Message}";
//                     }
//                 }
//                 else
//                 {
//                     string errorMsg = $"网络错误: {request.error}";
//                     Debug.LogError($"[IAPOrderAPI] ❌ 创建订单请求失败 - {errorMsg}");
//                     result.success = false;
//                     result.error = errorMsg;
//                 }
//             }
//         }

//         #endregion

//         #region 验证支付接口

//         /// <summary>
//         /// 验证Google支付
//         /// </summary>
//         /// <param name="orderId">订单ID</param>
//         /// <param name="googleToken">Google购买凭证(receipt)</param>
//         /// <param name="onComplete">完成回调 (成功, 订单数据, 错误信息)</param>
//         public void VerifyGooglePayment(string orderId, string googleToken, Action<bool, OrderResponse, string> onComplete)
//         {
//             if (string.IsNullOrEmpty(orderId))
//             {
//                 Debug.LogError("[IAPOrderAPI] orderId不能为空");
//                 onComplete?.Invoke(false, null, "订单ID不能为空");
//                 return;
//             }

//             if (string.IsNullOrEmpty(googleToken))
//             {
//                 Debug.LogError("[IAPOrderAPI] googleToken不能为空");
//                 onComplete?.Invoke(false, null, "购买凭证不能为空");
//                 return;
//             }

//             // 使用AuthManager执行需要认证的请求
//             AuthManager.Instance.ExecuteAuthenticatedRequest<OrderResponse>(
//                 (authToken, result) => VerifyPaymentCoroutine(orderId, googleToken, authToken, result),
//                 (success, data, error) =>
//                 {
//                     if (success)
//                     {
//                         onComplete?.Invoke(true, data, null);
//                     }
//                     else
//                     {
//                         onComplete?.Invoke(false, null, error);
//                     }
//                 }
//             );
//         }

//         private IEnumerator VerifyPaymentCoroutine(string orderId, string googleToken, string authToken, AuthManager.RequestResult<OrderResponse> result)
//         {
//             Debug.Log($"[IAPOrderAPI] 验证支付请求 - 订单ID: {orderId}");

//             // 从SDK公参获取deviceId
//             string deviceId = "";
//             var commonParams = SDKCommonParams.Cached;
//             if (commonParams != null)
//             {
//                 deviceId = commonParams.deviceId ?? "";
//                 Debug.Log($"[IAPOrderAPI] 从SDK公参获取deviceId: {deviceId}");
//             }
//             else
//             {
//                 Debug.LogWarning("[IAPOrderAPI] SDK公参未就绪，deviceId为空");
//             }

//             // 构建请求体
//             var payDto = new PayDto
//             {
//                 orderId = orderId,
//                 googleToken = googleToken,
//                 deviceId = deviceId
//             };

//             string jsonBody = JsonUtility.ToJson(payDto);
//             Debug.Log($"[IAPOrderAPI] 请求体JSON: {jsonBody}");

//             using (UnityWebRequest request = new UnityWebRequest(VERIFY_PAYMENT_URL, "POST"))
//             {
//                 byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
//                 request.uploadHandler = new UploadHandlerRaw(bodyRaw);
//                 request.downloadHandler = new DownloadHandlerBuffer();
//                 request.timeout = REQUEST_TIMEOUT;
//                 request.SetRequestHeader("Content-Type", "application/json");
//                 request.SetRequestHeader("Authorization", authToken);

//                 yield return request.SendWebRequest();

//                 result.responseCode = (int)request.responseCode;

//                 if (request.result == UnityWebRequest.Result.Success)
//                 {
//                     try
//                     {
//                         string responseText = request.downloadHandler.text;
//                         Debug.Log($"[IAPOrderAPI] 验证支付成功 - 响应: {responseText}");

//                         // 解析响应
//                         var response = JsonUtility.FromJson<OrderResponse>(responseText);

//                         // 服务器返回code:200表示成功
//                         if (response != null && response.code == 200 && response.data != null)
//                         {
//                             Debug.Log($"[IAPOrderAPI] ✅ 支付验证成功 - 订单ID: {response.data.id}, 支付状态: {response.data.payStatus}");
//                             result.success = true;
//                             result.data = response;
//                         }
//                         else
//                         {
//                             string errorMsg = response?.message ?? "未知错误";
//                             Debug.LogError($"[IAPOrderAPI] ❌ 验证支付失败 - 错误: {errorMsg}");
//                             result.success = false;
//                             result.error = errorMsg;
//                         }
//                     }
//                     catch (Exception e)
//                     {
//                         Debug.LogError($"[IAPOrderAPI] ❌ 解析验证响应失败: {e.Message}\n{e.StackTrace}");
//                         result.success = false;
//                         result.error = $"解析响应失败: {e.Message}";
//                     }
//                 }
//                 else
//                 {
//                     string errorMsg = $"网络错误: {request.error}";
//                     Debug.LogError($"[IAPOrderAPI] ❌ 验证支付请求失败 - {errorMsg}");
//                     result.success = false;
//                     result.error = errorMsg;
//                 }
//             }
//         }

//         #endregion

//         #region 数据模型

//         /// <summary>
//         /// 支付请求DTO
//         /// </summary>
//         [Serializable]
//         public class PayDto
//         {
//             public string orderId;
//             public string googleToken;
//             public string deviceId; // 设备ID，从SDK公参中获取
//         }

//         /// <summary>
//         /// API响应
//         /// </summary>
//         [Serializable]
//         public class OrderResponse
//         {
//             public int code;
//             public string message;
//             public MjOrder data;
//         }

//         /// <summary>
//         /// 订单数据
//         /// </summary>
//         [Serializable]
//         public class MjOrder
//         {
//             public string id;                // 订单ID
//             public string goodsId;           // 商品ID
//             public string userId;            // 用户ID
//             public int payStatus;            // 支付状态 0:待支付 1:已支付 2:已取消 3:已退款
//             public int shippingStatus;       // 发货状态 0:未发货 1:已发货
//             public int gameFirstPay;         // 是否游戏首充 0:否 1:是
//             public int goodsFirstPay;        // 是否商品首充 0:否 1:是
//             public string createTime;        // 创建时间
//         }

//         #endregion
//     }
// }
