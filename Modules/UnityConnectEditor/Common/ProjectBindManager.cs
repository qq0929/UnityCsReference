// Unity C# reference source
// Copyright (c) Unity Technologies. For terms of use, see
// https://unity3d.com/legal/licenses/Unity_Reference_Only_License


using UnityEditorInternal;
using Button = UnityEngine.UIElements.Button;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;
using System;
using System.Text;
using System.Collections.Generic;

namespace UnityEditor.Connect
{
    /// <summary>
    /// A common system to handle Project Bind configuration.
    /// </summary>
    internal class ProjectBindManager
    {
        const string k_ProjectBindTemplatePath = "UXML/ServicesWindow/ProjectBind.uxml";
        const string k_ProjectBindCommonStyleSheetPath = "StyleSheets/ServicesWindow/ProjectBindCommon.uss";
        const string k_ProjectBindDarkStyleSheetPath = "StyleSheets/ServicesWindow/ProjectBindDark.uss";
        const string k_ProjectBindLightStyleSheetPath = "StyleSheets/ServicesWindow/ProjectBindLight.uss";
        const string k_SelectOrganizationText = "Select organization";
        const string k_SelectProjectText = "Select project";
        const long k_HttpStatusCodeUnprocessableEntity = 422;
        const string k_Yes = "Yes";
        const string k_No = "No";

        static readonly List<string> k_AnyRoleFilter;
        static readonly List<string> k_AtLeastManagerFilter;

        const string k_LinkProjectWindowTitle = "Link Project";
        const string k_DialogConfirmationMessage = "Are you sure you want to link to the project '{0}' in organization '{1}'?";
        const string k_CouldNotCreateProjectMessage = "Could not create project.";
        const string k_CouldNotObtainProjectMessage = "Could not obtain projects.";
        const string k_CouldNotObtainOrganizationsMessage = "Could not obtain organizations.";
        const string k_ProjectLinkSuccessMessage = "Project was linked successfully.";

        const string k_ProjectBindContainerName = "ProjectBindContainer";
        const string k_CreateProjectIdBlockName = "CreateProjectIdBlock";
        const string k_ReuseProjectIdBlockName = "ReuseProjectIdBlock";
        const string k_OrganizationFieldName = "OrganizationField";
        const string k_CreateProjectIdBtnName = "CreateProjectIdBtn";
        const string k_ReuseProjectIdLinkBtnName = "ReuseProjectIdLinkBtn";
        const string k_CreateProjectIdLinkBtnName = "CreateProjectIdLinkBtn";
        const string k_ProjectIdFieldName = "ProjectIdField";
        const string k_LinkBtnName = "LinkBtn";
        const string k_CancelBtnName = "CancelBtn";

        const string k_RootDataKey = "rootKey";
        const string k_BindContainerDataKey = "bindContainerKey";

        const string k_RoleOwner = "owner";
        const string k_RoleManager = "manager";
        const string k_RoleUser = "user";

        const string k_JsonProjectsNodeName = "projects";
        const string k_JsonArchivedNodeName = "archived";
        const string k_JsonOrgIdNodeName = "org_id";
        const string k_JsonNameNodeName = "name";
        const string k_JsonGuidNodeName = "guid";
        const string k_JsonOrgsNodeName = "orgs";
        const string k_JsonRoleNodeName = "role";
        const string k_JsonOrgNameNodeName = "org_name";

        Dictionary<string, ProjectInfoData> m_ProjectInfoByName;
        VisualElement m_CreateProjectIdBlock;
        VisualElement m_ReuseProjectIdBlock;
        string m_LastCreateBlockOrganization;
        string m_LastReuseBlockOrganization;
        string m_LastReuseBlockProject;
        UnityWebRequest m_CurrentRequest;
        int m_CreateIteration;

        public CreateButtonCallback createButtonCallback { private get; set; }
        public LinkButtonCallback linkButtonCallback { private get; set; }
        public ExceptionCallback exceptionCallback { private get; set; }

        public VisualElement projectBindContainer { get; private set; }

        internal struct ProjectBindState
        {
            public bool bound;
            public string projectName;
        }

        static ProjectBindManager()
        {
            k_AnyRoleFilter = new List<string>(new[] { k_RoleOwner, k_RoleManager, k_RoleUser });
            k_AtLeastManagerFilter = new List<string>(new[] { k_RoleOwner, k_RoleManager });
        }

        /// <summary>
        /// Configures a new Project Bind manager to this within an existing EditorWindow
        /// </summary>
        /// <param name="rootVisualElement">visual element where the project bind content must be added</param>
        public ProjectBindManager(VisualElement rootVisualElement)
        {
            InitializeProjectBindManager(rootVisualElement);
        }

        void InitializeProjectBindManager(VisualElement rootVisualElement)
        {
            m_LastCreateBlockOrganization = L10n.Tr(k_SelectOrganizationText);
            m_LastReuseBlockOrganization = L10n.Tr(k_SelectOrganizationText);
            m_LastReuseBlockProject = L10n.Tr(k_SelectProjectText);
            rootVisualElement.AddStyleSheetPath(k_ProjectBindCommonStyleSheetPath);
            rootVisualElement.viewDataKey = k_RootDataKey;
            rootVisualElement.AddStyleSheetPath(EditorGUIUtility.isProSkin ? k_ProjectBindDarkStyleSheetPath : k_ProjectBindLightStyleSheetPath);
            var projectBindTemplate = EditorGUIUtility.Load(k_ProjectBindTemplatePath) as VisualTreeAsset;
            rootVisualElement.Add(projectBindTemplate.CloneTree().contentContainer);
            projectBindContainer = rootVisualElement.Q(k_ProjectBindContainerName);
            projectBindContainer.viewDataKey = k_BindContainerDataKey;

            m_CreateProjectIdBlock = projectBindContainer.Q(k_CreateProjectIdBlockName);
            m_ReuseProjectIdBlock = projectBindContainer.Q(k_ReuseProjectIdBlockName);
            m_LastCreateBlockOrganization = string.Empty;
            m_LastReuseBlockOrganization = string.Empty;

            SetupCreateProjectIdBlock();
            SetupReuseProjectIdBlock();
        }

        void SetupCreateProjectIdBlock()
        {
            var popupField = BuildPopupField(m_CreateProjectIdBlock, k_OrganizationFieldName);
            popupField.RegisterValueChangedCallback(delegate(ChangeEvent<string> evt)
            {
                if (evt.newValue != m_LastCreateBlockOrganization)
                {
                    m_LastCreateBlockOrganization = evt.newValue;
                    m_CreateProjectIdBlock.Q(k_CreateProjectIdBtnName).SetEnabled(m_LastCreateBlockOrganization != L10n.Tr(k_SelectOrganizationText));
                }
            });
            popupField.choices.Add(L10n.Tr(k_SelectOrganizationText));
            popupField.value = L10n.Tr(k_SelectOrganizationText);
            popupField.SetEnabled(false);
            LoadCreateOrganizationField(popupField);

            var reuseProjectIdClickable = new Clickable(() =>
            {
                m_CreateProjectIdBlock.style.display = DisplayStyle.None;
                m_ReuseProjectIdBlock.style.display = DisplayStyle.Flex;
            });
            m_CreateProjectIdBlock.Q(k_ReuseProjectIdLinkBtnName).AddManipulator(reuseProjectIdClickable);

            m_CreateProjectIdBlock.Q<Button>(k_CancelBtnName).style.display = DisplayStyle.None;

            var createProjectIdBtn = m_CreateProjectIdBlock.Q<Button>(k_CreateProjectIdBtnName);
            createProjectIdBtn.SetEnabled(false);
            createProjectIdBtn.clicked += () =>
            {
                if (m_LastCreateBlockOrganization != L10n.Tr(k_SelectOrganizationText))
                {
                    m_CreateIteration = 0;
                    RequestCreateOperation();
                }
            };
        }

        void SetupReuseProjectIdBlock()
        {
            var organizationPopupField = BuildPopupField(m_ReuseProjectIdBlock, k_OrganizationFieldName);
            var projectIdPopupField = BuildPopupField(m_ReuseProjectIdBlock, k_ProjectIdFieldName);

            organizationPopupField.RegisterValueChangedCallback(delegate(ChangeEvent<string> evt)
            {
                if (evt.newValue != m_LastReuseBlockOrganization)
                {
                    m_LastReuseBlockOrganization = evt.newValue;
                    if (m_LastReuseBlockOrganization == L10n.Tr(k_SelectOrganizationText))
                    {
                        var projectIdField = m_ReuseProjectIdBlock.Q<PopupField<string>>(k_ProjectIdFieldName);
                        projectIdField.choices = new List<string>() { L10n.Tr(k_SelectProjectText) };
                        projectIdField.SetEnabled(false);
                        projectIdField.value = L10n.Tr(k_SelectProjectText);
                        m_ReuseProjectIdBlock.Q<Button>(k_LinkBtnName).SetEnabled(false);
                    }
                    else
                    {
                        var projectIdField = m_ReuseProjectIdBlock.Q<PopupField<string>>(k_ProjectIdFieldName);
                        LoadProjectField(organizationPopupField.value, projectIdField);
                    }
                }
            });
            organizationPopupField.choices.Add(L10n.Tr(k_SelectOrganizationText));
            organizationPopupField.value = L10n.Tr(k_SelectOrganizationText);
            organizationPopupField.SetEnabled(false);
            LoadReuseOrganizationField(organizationPopupField, projectIdPopupField);

            projectIdPopupField.RegisterValueChangedCallback(delegate(ChangeEvent<string> evt)
            {
                if (evt.newValue != m_LastReuseBlockOrganization)
                {
                    m_LastReuseBlockProject = evt.newValue;
                    if (m_LastReuseBlockOrganization == L10n.Tr(k_SelectOrganizationText))
                    {
                        return;
                    }
                    m_ReuseProjectIdBlock.Q<Button>(k_LinkBtnName).SetEnabled(m_LastReuseBlockProject != L10n.Tr(k_SelectProjectText));
                }
            });
            projectIdPopupField.choices.Add(L10n.Tr(k_SelectProjectText));
            projectIdPopupField.value = L10n.Tr(k_SelectProjectText);

            m_ReuseProjectIdBlock.style.display = DisplayStyle.None;

            var createProjectIdClickable = new Clickable(() =>
            {
                m_CreateProjectIdBlock.style.display = DisplayStyle.Flex;
                m_ReuseProjectIdBlock.style.display = DisplayStyle.None;
            });
            m_ReuseProjectIdBlock.Q(k_CreateProjectIdLinkBtnName).AddManipulator(createProjectIdClickable);

            m_ReuseProjectIdBlock.Q<Button>(k_CancelBtnName).style.display = DisplayStyle.None;

            var linkBtn = m_ReuseProjectIdBlock.Q<Button>(k_LinkBtnName);
            linkBtn.SetEnabled(false);
            linkBtn.clicked += () =>
            {
                if (L10n.Tr(k_SelectProjectText) != m_LastReuseBlockProject)
                {
                    var abort = false;
                    var projectInfo = m_ProjectInfoByName[m_LastReuseBlockProject];
                    if (EditorUtility.DisplayDialog(L10n.Tr(k_LinkProjectWindowTitle),
                        string.Format(L10n.Tr(k_DialogConfirmationMessage), projectInfo.name, projectInfo.organizationId),
                        L10n.Tr(k_Yes), L10n.Tr(k_No)))
                    {
                        try
                        {
                            UnityConnect.instance.BindProject(projectInfo.guid, projectInfo.name, projectInfo.organizationId);
                            EditorAnalytics.SendProjectServiceBindingEvent(new ProjectBindManager.ProjectBindState() { bound = true, projectName = projectInfo.name });
                            NotificationManager.instance.Publish(Notification.Topic.ProjectBind, Notification.Severity.Info, L10n.Tr(k_ProjectLinkSuccessMessage));
                        }
                        catch (Exception ex)
                        {
                            if (exceptionCallback != null)
                            {
                                exceptionCallback.Invoke(ex);
                                abort = true;
                            }
                            else
                            {
                                //If there is no exception callback, we have to at least log it
                                Debug.LogException(ex);
                            }
                        }
                        if (!abort)
                        {
                            linkButtonCallback?.Invoke(projectInfo);
                        }
                    }
                }
            };
        }

        void RequestCreateOperation()
        {
            var payload = $"{{\"name\":\"{Application.productName + GetProjectNameSuffix()}\", \"active\":true}}";
            var uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(payload));
            m_CurrentRequest = new UnityWebRequest(
                ServicesConfiguration.instance.GetOrganizationProjectsApiUrl(m_LastCreateBlockOrganization),
                UnityWebRequest.kHttpVerbPOST)
            { downloadHandler = new DownloadHandlerBuffer(), uploadHandler = uploadHandler};
            m_CurrentRequest.SetRequestHeader("AUTHORIZATION", $"Bearer {UnityConnect.instance.GetUserInfo().accessToken}");
            m_CurrentRequest.SetRequestHeader("Content-Type", "application/json;charset=UTF-8");
            var operation = m_CurrentRequest.SendWebRequest();
            operation.completed += CreateOperationOnCompleted;
        }

        string GetProjectNameSuffix()
        {
            return m_CreateIteration > 0 ? $" ({m_CreateIteration})" : string.Empty;
        }

        void CreateOperationOnCompleted(AsyncOperation obj)
        {
            if (m_CurrentRequest == null)
            {
                //If we lost our m_CurrentRequest request reference, we can't risk doing anything.
                return;
            }

            if (m_CurrentRequest.result != UnityWebRequest.Result.ProtocolError)
            {
                var jsonParser = new JSONParser(m_CurrentRequest.downloadHandler.text);
                var json = jsonParser.Parse();
                var abort = false;
                try
                {
                    var projectInfo = ExtractProjectInfoFromJson(json);
                    try
                    {
                        UnityConnect.instance.BindProject(projectInfo.guid, projectInfo.name, projectInfo.organizationId);
                        EditorAnalytics.SendProjectServiceBindingEvent(new ProjectBindManager.ProjectBindState() { bound = true, projectName = projectInfo.name });
                        NotificationManager.instance.Publish(Notification.Topic.ProjectBind, Notification.Severity.Info, L10n.Tr(k_ProjectLinkSuccessMessage));
                    }
                    catch (Exception ex)
                    {
                        if (exceptionCallback != null)
                        {
                            exceptionCallback.Invoke(ex);
                            abort = true;
                        }
                        else
                        {
                            //If there is no exception callback, we have to at least log it
                            Debug.LogException(ex);
                        }
                    }
                    if (!abort)
                    {
                        createButtonCallback?.Invoke(projectInfo);
                    }
                }
                finally
                {
                    m_CurrentRequest?.Dispose();
                    m_CurrentRequest = null;
                }
            }
            else if (m_CurrentRequest.responseCode == k_HttpStatusCodeUnprocessableEntity)
            {
                m_CurrentRequest?.Dispose();
                m_CurrentRequest = null;
                m_CreateIteration++;
                RequestCreateOperation();
            }
            else
            {
                try
                {
                    var ex = new UnityConnectWebRequestException(L10n.Tr(k_CouldNotCreateProjectMessage))
                    {
                        error = m_CurrentRequest.error,
                        method = m_CurrentRequest.method,
                        timeout = m_CurrentRequest.timeout,
                        url = m_CurrentRequest.url,
                        responseHeaders = m_CurrentRequest.GetResponseHeaders(),
                        responseCode = m_CurrentRequest.responseCode,
                        isHttpError = (m_CurrentRequest.result == UnityWebRequest.Result.ProtocolError),
                        isNetworkError = (m_CurrentRequest.result == UnityWebRequest.Result.ConnectionError),
                    };
                    if (exceptionCallback != null)
                    {
                        exceptionCallback.Invoke(ex);
                    }
                    else
                    {
                        //If there is no exception callback, we have to at least log it
                        Debug.LogException(ex);
                    }
                }
                finally
                {
                    m_CurrentRequest?.Dispose();
                    m_CurrentRequest = null;
                }
            }
        }

        void LoadProjectField(string organizationName, PopupField<string> projectIdField)
        {
            var getProjectsRequest = new UnityWebRequest(ServicesConfiguration.instance.GetOrganizationProjectsApiUrl(organizationName),
                UnityWebRequest.kHttpVerbGET) { downloadHandler = new DownloadHandlerBuffer() };
            getProjectsRequest.SetRequestHeader("AUTHORIZATION", $"Bearer {UnityConnect.instance.GetUserInfo().accessToken}");
            var operation = getProjectsRequest.SendWebRequest();
            operation.completed += op =>
            {
                try
                {
                    if (getProjectsRequest.result != UnityWebRequest.Result.ProtocolError)
                    {
                        var jsonParser = new JSONParser(getProjectsRequest.downloadHandler.text);
                        var json = jsonParser.Parse();
                        try
                        {
                            m_ProjectInfoByName = new Dictionary<string, ProjectInfoData>();

                            var jsonProjects = json.AsDict()[k_JsonProjectsNodeName].AsList();
                            foreach (var jsonProject in jsonProjects)
                            {
                                if (!jsonProject.AsDict()[k_JsonArchivedNodeName].AsBool())
                                {
                                    var projectInfo = ExtractProjectInfoFromJson(jsonProject);
                                    m_ProjectInfoByName.Add(projectInfo.name, projectInfo);
                                }
                            }

                            var projectNames = new List<string> { L10n.Tr(k_SelectProjectText) };
                            var sortedProjectNames = new List<string>(m_ProjectInfoByName.Keys);
                            sortedProjectNames.Sort();
                            projectNames.AddRange(sortedProjectNames);
                            projectIdField.choices = projectNames;
                            projectIdField.SetEnabled(true);
                        }
                        catch (Exception ex)
                        {
                            if (exceptionCallback != null)
                            {
                                exceptionCallback.Invoke(ex);
                            }
                            else
                            {
                                //If there is no exception callback, we have to at least log it
                                Debug.LogException(ex);
                            }
                        }
                    }
                    else
                    {
                        var ex = new UnityConnectWebRequestException(L10n.Tr(k_CouldNotObtainProjectMessage))
                        {
                            error = getProjectsRequest.error,
                            method = getProjectsRequest.method,
                            timeout = getProjectsRequest.timeout,
                            url = getProjectsRequest.url,
                            responseHeaders = getProjectsRequest.GetResponseHeaders(),
                            responseCode = getProjectsRequest.responseCode,
                            isHttpError = (getProjectsRequest.result == UnityWebRequest.Result.ProtocolError),
                            isNetworkError = (getProjectsRequest.result == UnityWebRequest.Result.ConnectionError),
                        };
                        if (exceptionCallback != null)
                        {
                            exceptionCallback.Invoke(ex);
                        }
                        else
                        {
                            //If there is no exception callback, we have to at least log it
                            Debug.LogException(ex);
                        }
                    }
                }
                finally
                {
                    getProjectsRequest.Dispose();
                }
            };
        }

        static ProjectInfoData ExtractProjectInfoFromJson(JSONValue jsonProject)
        {
            return new ProjectInfoData(
                jsonProject.AsDict()[k_JsonOrgIdNodeName].AsString(),
                jsonProject.AsDict()[k_JsonNameNodeName].AsString(),
                jsonProject.AsDict()[k_JsonGuidNodeName].AsString());
        }

        /// <summary>
        /// To create a new project, the current user must be the owner or a manager of an organization
        /// </summary>
        /// <param name="organizationField"></param>
        /// <param name="projectIdField"></param>
        void LoadCreateOrganizationField(PopupField<string> organizationField, PopupField<string> projectIdField = null)
        {
            var getOrganizationsRequest = new UnityWebRequest(ServicesConfiguration.instance.GetCurrentUserApiUrl() + "?include=orgs",
                UnityWebRequest.kHttpVerbGET) { downloadHandler = new DownloadHandlerBuffer() };
            getOrganizationsRequest.SetRequestHeader("AUTHORIZATION", $"Bearer {UnityConnect.instance.GetUserInfo().accessToken}");
            var operation = getOrganizationsRequest.SendWebRequest();
            operation.completed += op =>
            {
                try
                {
                    if (getOrganizationsRequest.result != UnityWebRequest.Result.ProtocolError)
                    {
                        var jsonParser = new JSONParser(getOrganizationsRequest.downloadHandler.text);
                        var json = jsonParser.Parse();
                        try
                        {
                            var sortedOrganizationNames = new List<string>();
                            foreach (var rawOrg in json.AsDict()[k_JsonOrgsNodeName].AsList())
                            {
                                var org = rawOrg.AsDict();
                                if (k_AtLeastManagerFilter.Contains(org[k_JsonRoleNodeName].AsString()))
                                {
                                    sortedOrganizationNames.Add(org[k_JsonNameNodeName].AsString());
                                }
                            }
                            sortedOrganizationNames.Sort();
                            var popUpChoices = new List<string> { L10n.Tr(k_SelectOrganizationText) };
                            organizationField.SetEnabled(true);
                            popUpChoices.AddRange(sortedOrganizationNames);
                            organizationField.choices = popUpChoices;
                            organizationField.SetValueWithoutNotify(organizationField.choices[0]);
                            if (projectIdField != null)
                            {
                                projectIdField.choices = new List<string> { L10n.Tr(k_SelectProjectText) };
                                projectIdField.SetValueWithoutNotify(L10n.Tr(k_SelectProjectText));
                                projectIdField.SetEnabled(false);
                            }
                        }
                        catch (Exception ex)
                        {
                            if (exceptionCallback != null)
                            {
                                exceptionCallback.Invoke(ex);
                            }
                            else
                            {
                                //If there is no exception callback, we have to at least log it
                                Debug.LogException(ex);
                            }
                        }
                    }
                    else
                    {
                        var ex = new UnityConnectWebRequestException(L10n.Tr(k_CouldNotObtainOrganizationsMessage))
                        {
                            error = getOrganizationsRequest.error,
                            method = getOrganizationsRequest.method,
                            timeout = getOrganizationsRequest.timeout,
                            url = getOrganizationsRequest.url,
                            responseHeaders = getOrganizationsRequest.GetResponseHeaders(),
                            responseCode = getOrganizationsRequest.responseCode,
                            isHttpError = (getOrganizationsRequest.result == UnityWebRequest.Result.ProtocolError),
                            isNetworkError = (getOrganizationsRequest.result == UnityWebRequest.Result.ConnectionError),
                        };
                        if (exceptionCallback != null)
                        {
                            exceptionCallback.Invoke(ex);
                        }
                        else
                        {
                            //If there is no exception callback, we have to at least log it
                            Debug.LogException(ex);
                        }
                    }
                }
                finally
                {
                    getOrganizationsRequest.Dispose();
                }
            };
        }

        /// <summary>
        /// To attach a project an existing project, we must collect all orgs the current user is a member of.
        /// In addition the current user may be a guest of a specific project, in which case we must also look at
        /// all projects to find organizations.
        /// </summary>
        /// <param name="organizationField"></param>
        /// <param name="projectIdField"></param>
        void LoadReuseOrganizationField(PopupField<string> organizationField, PopupField<string> projectIdField = null)
        {
            var getOrganizationsRequest = new UnityWebRequest(ServicesConfiguration.instance.GetCurrentUserApiUrl() + "?include=orgs,projects",
                UnityWebRequest.kHttpVerbGET) { downloadHandler = new DownloadHandlerBuffer() };
            getOrganizationsRequest.SetRequestHeader("AUTHORIZATION", $"Bearer {UnityConnect.instance.GetUserInfo().accessToken}");
            var operation = getOrganizationsRequest.SendWebRequest();
            operation.completed += op =>
            {
                try
                {
                    if (getOrganizationsRequest.result != UnityWebRequest.Result.ProtocolError)
                    {
                        var jsonParser = new JSONParser(getOrganizationsRequest.downloadHandler.text);
                        var json = jsonParser.Parse();
                        try
                        {
                            var sortedOrganizationNames = new List<string>();

                            foreach (var rawOrg in json.AsDict()[k_JsonOrgsNodeName].AsList())
                            {
                                var org = rawOrg.AsDict();
                                if (k_AnyRoleFilter.Contains(org[k_JsonRoleNodeName].AsString()))
                                {
                                    sortedOrganizationNames.Add(org[k_JsonNameNodeName].AsString());
                                }
                            }

                            foreach (var rawProject in json.AsDict()[k_JsonProjectsNodeName].AsList())
                            {
                                var project = rawProject.AsDict();
                                if (!project[k_JsonArchivedNodeName].AsBool()
                                    && !sortedOrganizationNames.Contains(project[k_JsonOrgNameNodeName].AsString()))
                                {
                                    sortedOrganizationNames.Add(project[k_JsonOrgNameNodeName].AsString());
                                }
                            }

                            sortedOrganizationNames.Sort();
                            var popUpChoices = new List<string> { L10n.Tr(k_SelectOrganizationText) };
                            organizationField.SetEnabled(true);
                            popUpChoices.AddRange(sortedOrganizationNames);
                            organizationField.choices = popUpChoices;
                            organizationField.SetValueWithoutNotify(organizationField.choices[0]);
                            if (projectIdField != null)
                            {
                                projectIdField.choices = new List<string> { L10n.Tr(k_SelectProjectText) };
                                projectIdField.SetValueWithoutNotify(L10n.Tr(k_SelectProjectText));
                                projectIdField.SetEnabled(false);
                            }
                        }
                        catch (Exception ex)
                        {
                            if (exceptionCallback != null)
                            {
                                exceptionCallback.Invoke(ex);
                            }
                            else
                            {
                                //If there is no exception callback, we have to at least log it
                                Debug.LogException(ex);
                            }
                        }
                    }
                    else
                    {
                        var ex = new UnityConnectWebRequestException(L10n.Tr(k_CouldNotObtainOrganizationsMessage))
                        {
                            error = getOrganizationsRequest.error,
                            method = getOrganizationsRequest.method,
                            timeout = getOrganizationsRequest.timeout,
                            url = getOrganizationsRequest.url,
                            responseHeaders = getOrganizationsRequest.GetResponseHeaders(),
                            responseCode = getOrganizationsRequest.responseCode,
                            isHttpError = (getOrganizationsRequest.result == UnityWebRequest.Result.ProtocolError),
                            isNetworkError = (getOrganizationsRequest.result == UnityWebRequest.Result.ConnectionError),
                        };
                        if (exceptionCallback != null)
                        {
                            exceptionCallback.Invoke(ex);
                        }
                        else
                        {
                            //If there is no exception callback, we have to at least log it
                            Debug.LogException(ex);
                        }
                    }
                }
                finally
                {
                    getOrganizationsRequest.Dispose();
                }
            };
        }

        static PopupField<string> BuildPopupField(VisualElement block, string anchorName)
        {
            var anchor = block.Q(anchorName);
            var anchorParent = anchor.parent;
            var anchorIndex = anchorParent.IndexOf(anchor);
            var popupField = new PopupField<string> { name = anchor.name };
            anchorParent.RemoveAt(anchorIndex);
            anchorParent.Insert(anchorIndex, popupField);
            return popupField;
        }

        public delegate void CreateButtonCallback(ProjectInfoData projectInfoData);

        public delegate void LinkButtonCallback(ProjectInfoData projectInfoData);

        public delegate void ExceptionCallback(Exception exception);
    }

    internal class ProjectInfoData
    {
        public string organizationId { get; }
        public string name { get; }
        public string guid { get; }

        public ProjectInfoData(string organizationId, string name, string guid)
        {
            this.guid = guid;
            this.name = name;
            this.organizationId = organizationId;
        }
    }
}
