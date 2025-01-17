// Unity C# reference source
// Copyright (c) Unity Technologies. For terms of use, see
// https://unity3d.com/legal/licenses/Unity_Reference_Only_License

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Connect;

namespace UnityEditor.PackageManager.UI
{
    internal sealed class AssetStoreRestAPI
    {
        static IAssetStoreRestAPI s_Instance = null;
        public static IAssetStoreRestAPI instance => s_Instance ?? AssetStoreRestAPIInternal.instance;

        internal class AssetStoreRestAPIInternal : IAssetStoreRestAPI
        {
            private static AssetStoreRestAPIInternal s_Instance;
            public static AssetStoreRestAPIInternal instance => s_Instance ?? (s_Instance = new AssetStoreRestAPIInternal());

            private readonly string m_Host;
            private const string k_PurchasesUri = "/-/api/purchases";
            private const string k_TaggingsUri = "/-/api/taggings";
            private const string k_ProductInfoUri = "/-/api/product";
            private const string k_UpdateInfoUri = "/-/api/legacy-package-update-info";
            private const string k_DownloadInfoUri = "/-/api/legacy-package-download-info";

            private AssetStoreRestAPIInternal()
            {
                m_Host = UnityConnect.instance.GetConfigurationURL(CloudConfigUrl.CloudPackagesApi);
            }

            public void GetPurchases(string query, Action<Dictionary<string, object>> doneCallbackAction, Action<Error> errorCallbackAction)
            {
                var httpRequest = ApplicationUtil.instance.GetASyncHTTPClient($"{m_Host}{k_PurchasesUri}{query ?? string.Empty}");
                HandleHttpRequest(httpRequest, doneCallbackAction, errorCallbackAction);
            }

            public void GetTaggings(Action<Dictionary<string, object>> doneCallbackAction, Action<Error> errorCallbackAction)
            {
                var httpRequest = ApplicationUtil.instance.GetASyncHTTPClient($"{m_Host}{k_TaggingsUri}");
                HandleHttpRequest(httpRequest, doneCallbackAction, errorCallbackAction);
            }

            public void GetProductDetail(long productID, Action<Dictionary<string, object>> doneCallbackAction)
            {
                var httpRequest = ApplicationUtil.instance.GetASyncHTTPClient($"{m_Host}{k_ProductInfoUri}/{productID}");
                var etag = AssetStoreCache.instance.GetLastETag(productID);
                httpRequest.header["If-None-Match"] = etag.Replace("\"", "\\\"");

                HandleHttpRequest(httpRequest,
                    result =>
                    {
                        if (httpRequest.responseHeader.ContainsKey("ETag"))
                            etag = httpRequest.responseHeader["ETag"];
                        AssetStoreCache.instance.SetLastETag(productID, etag);

                        doneCallbackAction?.Invoke(result);
                    },
                    error =>
                    {
                        var ret = new Dictionary<string, object> { ["errorMessage"] = error.message };
                        doneCallbackAction?.Invoke(ret);
                    });
            }

            public void GetDownloadDetail(long productID, Action<AssetStoreDownloadInfo> doneCallbackAction)
            {
                var httpRequest = ApplicationUtil.instance.GetASyncHTTPClient($"{m_Host}{k_DownloadInfoUri}/{productID}");
                HandleHttpRequest(httpRequest,
                    result =>
                    {
                        var downloadInfo = AssetStoreDownloadInfo.ParseDownloadInfo(result);
                        doneCallbackAction?.Invoke(downloadInfo);
                    },
                    error =>
                    {
                        var downloadInfo = new AssetStoreDownloadInfo
                        {
                            isValid = false,
                            errorMessage = error.message
                        };
                        doneCallbackAction?.Invoke(downloadInfo);
                    });
            }

            public void GetProductUpdateDetail(IEnumerable<AssetStoreLocalInfo> localInfos, Action<Dictionary<string, object>> doneCallbackAction)
            {
                if (localInfos?.Any() != true)
                {
                    doneCallbackAction?.Invoke(new Dictionary<string, object>());
                    return;
                }

                var localInfosJsonData = Json.Serialize(localInfos.Select(info => info?.ToDictionary() ?? new Dictionary<string, string>()).ToList());
                var httpRequest = ApplicationUtil.instance.GetASyncHTTPClient($"{m_Host}{k_UpdateInfoUri}", "POST");

                HandleHttpRequest(httpRequest,
                    result =>
                    {
                        var ret = result["result"] as Dictionary<string, object>;
                        doneCallbackAction?.Invoke(ret);
                    },
                    error =>
                    {
                        var ret = new Dictionary<string, object> { ["errorMessage"] = error.message };
                        doneCallbackAction?.Invoke(ret);
                    });
            }

            private void HandleHttpRequest(IAsyncHTTPClient httpRequest, Action<Dictionary<string, object>> doneCallbackAction, Action<Error> errorCallbackAction)
            {
                AssetStoreOAuth.instance.FetchUserInfo(
                    userInfo =>
                    {
                        httpRequest.header["Content-Type"] = "application/json";
                        httpRequest.header["Authorization"] = "Bearer " + userInfo.accessToken;
                        httpRequest.doneCallback = httpClient =>
                        {
                            var parsedResult = AssetStoreUtils.ParseResponseAsDictionary(httpRequest, errorMessage =>
                            {
                                errorCallbackAction?.Invoke(new Error(NativeErrorCode.Unknown, errorMessage));
                            });
                            if (parsedResult != null)
                                doneCallbackAction?.Invoke(parsedResult);
                        };
                        httpRequest.Begin();
                    },
                    errorCallbackAction);
            }
        }
    }
}
