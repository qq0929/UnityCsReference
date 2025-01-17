// Unity C# reference source
// Copyright (c) Unity Technologies. For terms of use, see
// https://unity3d.com/legal/licenses/Unity_Reference_Only_License

using System;
using System.Linq;
using UnityEngine.Rendering;
using UnityEngine;
using UnityEngineInternal;
using Object = UnityEngine.Object;

namespace UnityEditor
{
    internal class LightingWindowBakeSettings
    {
        // light modes
        private SavedBool       m_ShowRealtimeLightsSettings;
        private SavedBool       m_ShowMixedLightsSettings;
        private SavedBool       m_ShowGeneralLightmapSettings;
        private bool            m_LightingSettingsReadOnlyMode;

        SerializedObject m_LightingSettings;

        //realtime GI
        SerializedProperty m_EnableRealtimeGI;
        SerializedProperty m_RealtimeResolution;
        SerializedProperty m_RealtimeEnvironmentLighting;

        //baked
        SerializedProperty m_EnabledBakedGI;
        SerializedProperty m_MixedBakeMode;
        SerializedProperty m_AlbedoBoost;
        SerializedProperty m_LightmapParameters;
        SerializedProperty m_LightmapDirectionalMode;
        SerializedProperty m_BakeResolution;
        SerializedProperty m_Padding;
        SerializedProperty m_AmbientOcclusion;
        SerializedProperty m_AOMaxDistance;
        SerializedProperty m_CompAOExponent;
        SerializedProperty m_CompAOExponentDirect;
        SerializedProperty m_TextureCompression;
        SerializedProperty m_FinalGather;
        SerializedProperty m_FinalGatherRayCount;
        SerializedProperty m_FinalGatherFiltering;
        SerializedProperty m_LightmapMaxSize;
        SerializedProperty m_BakeBackend;

        SerializedProperty m_PVRSampling; // TODO(PVR): make non-fixed sampling modes work.
        SerializedProperty m_PVRSampleCount;
        SerializedProperty m_PVRDirectSampleCount;
        SerializedProperty m_PVRBounces;
        SerializedProperty m_PVRCulling;
        SerializedProperty m_PVRFilteringMode;
        SerializedProperty m_PVRFilterTypeDirect;
        SerializedProperty m_PVRFilterTypeIndirect;
        SerializedProperty m_PVRFilterTypeAO;
        SerializedProperty m_PVRDenoiserTypeDirect;
        SerializedProperty m_PVRDenoiserTypeIndirect;
        SerializedProperty m_PVRDenoiserTypeAO;
        SerializedProperty m_PVRFilteringGaussRadiusDirect;
        SerializedProperty m_PVRFilteringGaussRadiusIndirect;
        SerializedProperty m_PVRFilteringGaussRadiusAO;
        SerializedProperty m_PVRFilteringAtrousPositionSigmaDirect;
        SerializedProperty m_PVRFilteringAtrousPositionSigmaIndirect;
        SerializedProperty m_PVRFilteringAtrousPositionSigmaAO;
        SerializedProperty m_PVREnvironmentMIS;
        SerializedProperty m_PVREnvironmentSampleCount;
        SerializedProperty m_LightProbeSampleCountMultiplier;

        SerializedProperty m_BounceScale;
        SerializedProperty m_ExportTrainingData;
        SerializedProperty m_TrainingDataDestination;
        SerializedProperty m_ForceWhiteAlbedo;
        SerializedProperty m_ForceUpdates;
        SerializedProperty m_FilterMode;

        SerializedObject lightingSettings
        {
            get
            {
                // if we set a new scene as the active scene, we need to make sure to respond to those changes
                if (m_LightingSettings == null || m_LightingSettings.targetObject == null || m_LightingSettings.targetObject != Lightmapping.lightingSettingsInternal)
                {
                    var targetObject = Lightmapping.lightingSettingsInternal;
                    m_LightingSettingsReadOnlyMode = false;

                    if (targetObject == null)
                    {
                        targetObject = Lightmapping.lightingSettingsDefaults;
                        m_LightingSettingsReadOnlyMode = true;
                    }

                    SerializedObject lso = m_LightingSettings = new SerializedObject(targetObject);

                    if (lso != null)
                    {
                        //realtime GI
                        m_RealtimeResolution = lso.FindProperty("m_RealtimeResolution");
                        m_EnableRealtimeGI = lso.FindProperty("m_EnableRealtimeLightmaps");
                        m_RealtimeEnvironmentLighting = lso.FindProperty("m_RealtimeEnvironmentLighting");

                        //baked
                        m_EnabledBakedGI = lso.FindProperty("m_EnableBakedLightmaps");
                        m_BakeBackend = lso.FindProperty("m_BakeBackend");
                        m_MixedBakeMode = lso.FindProperty("m_MixedBakeMode");
                        m_AlbedoBoost = lso.FindProperty("m_AlbedoBoost");
                        m_LightmapMaxSize = lso.FindProperty("m_LightmapMaxSize");
                        m_LightmapParameters = lso.FindProperty("m_LightmapParameters");
                        m_LightmapDirectionalMode = lso.FindProperty("m_LightmapsBakeMode");
                        m_BakeResolution = lso.FindProperty("m_BakeResolution");
                        m_Padding = lso.FindProperty("m_Padding");
                        m_AmbientOcclusion = lso.FindProperty("m_AO");
                        m_AOMaxDistance = lso.FindProperty("m_AOMaxDistance");
                        m_CompAOExponent = lso.FindProperty("m_CompAOExponent");
                        m_CompAOExponentDirect = lso.FindProperty("m_CompAOExponentDirect");
                        m_TextureCompression = lso.FindProperty("m_TextureCompression");
                        m_FinalGather = lso.FindProperty("m_FinalGather");
                        m_FinalGatherRayCount = lso.FindProperty("m_FinalGatherRayCount");
                        m_FinalGatherFiltering = lso.FindProperty("m_FinalGatherFiltering");

                        m_PVRSampling = lso.FindProperty("m_PVRSampling");
                        m_PVRSampleCount = lso.FindProperty("m_PVRSampleCount");
                        m_PVRDirectSampleCount = lso.FindProperty("m_PVRDirectSampleCount");
                        m_PVRBounces = lso.FindProperty("m_PVRBounces");
                        m_PVRCulling = lso.FindProperty("m_PVRCulling");
                        m_PVRFilteringMode = lso.FindProperty("m_PVRFilteringMode");
                        m_PVRFilterTypeDirect = lso.FindProperty("m_PVRFilterTypeDirect");
                        m_PVRFilterTypeIndirect = lso.FindProperty("m_PVRFilterTypeIndirect");
                        m_PVRFilterTypeAO = lso.FindProperty("m_PVRFilterTypeAO");
                        m_PVRDenoiserTypeDirect = lso.FindProperty("m_PVRDenoiserTypeDirect");
                        m_PVRDenoiserTypeIndirect = lso.FindProperty("m_PVRDenoiserTypeIndirect");
                        m_PVRDenoiserTypeAO = lso.FindProperty("m_PVRDenoiserTypeAO");
                        m_PVRFilteringGaussRadiusDirect = lso.FindProperty("m_PVRFilteringGaussRadiusDirect");
                        m_PVRFilteringGaussRadiusIndirect = lso.FindProperty("m_PVRFilteringGaussRadiusIndirect");
                        m_PVRFilteringGaussRadiusAO = lso.FindProperty("m_PVRFilteringGaussRadiusAO");
                        m_PVRFilteringAtrousPositionSigmaDirect = lso.FindProperty("m_PVRFilteringAtrousPositionSigmaDirect");
                        m_PVRFilteringAtrousPositionSigmaIndirect = lso.FindProperty("m_PVRFilteringAtrousPositionSigmaIndirect");
                        m_PVRFilteringAtrousPositionSigmaAO = lso.FindProperty("m_PVRFilteringAtrousPositionSigmaAO");
                        m_PVREnvironmentMIS = lso.FindProperty("m_PVREnvironmentMIS");
                        m_PVREnvironmentSampleCount = lso.FindProperty("m_PVREnvironmentSampleCount");
                        m_LightProbeSampleCountMultiplier = lso.FindProperty("m_LightProbeSampleCountMultiplier");

                        //dev debug properties
                        m_ExportTrainingData = lso.FindProperty("m_ExportTrainingData");
                        m_TrainingDataDestination = lso.FindProperty("m_TrainingDataDestination");
                        m_ForceWhiteAlbedo = lso.FindProperty("m_ForceWhiteAlbedo");
                        m_ForceUpdates = lso.FindProperty("m_ForceUpdates");
                        m_FilterMode = lso.FindProperty("m_FilterMode");
                        m_BounceScale = lso.FindProperty("m_BounceScale");
                    }
                }

                return m_LightingSettings;
            }
        }

        static bool PlayerHasSM20Support()
        {
            var apis = PlayerSettings.GetGraphicsAPIs(EditorUserBuildSettings.activeBuildTarget);
            bool hasSM20Api = apis.Contains(UnityEngine.Rendering.GraphicsDeviceType.OpenGLES2);
            return hasSM20Api;
        }

        public void OnEnable()
        {
            m_ShowGeneralLightmapSettings = new SavedBool("LightingWindow.ShowGeneralLightmapSettings", true);
            m_ShowRealtimeLightsSettings = new SavedBool("LightingWindow.ShowRealtimeLightsSettings", true);
            m_ShowMixedLightsSettings = new SavedBool("LightingWindow.ShowMixedLightsSettings", true);
        }

        public void OnDisable()
        {
            if (m_LightingSettings != null)
                m_LightingSettings.Dispose();
        }

        static void DrawResolutionField(SerializedProperty resolution, GUIContent label)
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(resolution, label);

            GUILayout.Label(" texels per unit", Styles.LabelStyle);
            GUILayout.EndHorizontal();
        }

        static void DrawFilterSettingField(SerializedProperty gaussSetting,
            SerializedProperty atrousSetting,
            GUIContent gaussLabel,
            GUIContent atrousLabel,
            LightingSettings.FilterType type)
        {
            if (type == LightingSettings.FilterType.None)
                return;

            GUILayout.BeginHorizontal();

            if (type == LightingSettings.FilterType.Gaussian)
            {
                EditorGUILayout.IntSlider(gaussSetting, 0, 5, gaussLabel);
                GUILayout.Label(" texels", Styles.LabelStyle);
            }
            else if (type == LightingSettings.FilterType.ATrous)
            {
                EditorGUILayout.Slider(atrousSetting, 0.0f, 2.0f, atrousLabel);
                GUILayout.Label(" sigma", Styles.LabelStyle);
            }

            GUILayout.EndHorizontal();
        }

        static private bool isBuiltIn(SerializedProperty prop)
        {
            if (prop.objectReferenceValue != null)
            {
                var parameters = prop.objectReferenceValue as LightmapParameters;
                return (parameters.hideFlags == HideFlags.NotEditable);
            }

            return true;
        }

        static private bool LightmapParametersGUI(SerializedProperty prop, GUIContent content)
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUIInternal.AssetPopup<LightmapParameters>(prop, content, "giparams", "Default-Medium");

            string label = "Edit...";

            if (isBuiltIn(prop))
                label = "View";

            bool editClicked = false;

            if (prop.objectReferenceValue == null)
            {
                using (new EditorGUI.DisabledScope(true))
                {
                    if (GUILayout.Button(label, EditorStyles.miniButton, GUILayout.ExpandWidth(false)))
                    {
                        Selection.activeObject = null;
                        editClicked = true;
                    }
                }
            }
            else
            {
                if (GUILayout.Button(label, EditorStyles.miniButton, GUILayout.ExpandWidth(false)))
                {
                    Selection.activeObject = prop.objectReferenceValue;
                    editClicked = true;
                }
            }

            EditorGUILayout.EndHorizontal();

            return editClicked;
        }

        void RealtimeLightingGUI()
        {
            // ambient GI - realtime / baked
            bool realtimeGISupported = SupportedRenderingFeatures.IsLightmapBakeTypeSupported(LightmapBakeType.Realtime);

            if (!realtimeGISupported)
                return;

            m_ShowRealtimeLightsSettings.value = EditorGUILayout.FoldoutTitlebar(m_ShowRealtimeLightsSettings.value, Styles.RealtimeLightsLabel, true);

            if (m_ShowRealtimeLightsSettings.value)
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.PropertyField(m_EnableRealtimeGI, Styles.UseRealtimeGI);

                if (m_EnableRealtimeGI.boolValue && PlayerHasSM20Support())
                {
                    EditorGUILayout.HelpBox(Styles.NoRealtimeGIInSM2AndGLES2.text, MessageType.Warning);
                }

                EditorGUI.indentLevel++;

                bool bakedGISupported = SupportedRenderingFeatures.IsLightmapBakeTypeSupported(LightmapBakeType.Baked);
                bool bakedGI = Lightmapping.GetLightingSettingsOrDefaultsFallback().bakedGI;
                bool realtimeGI = Lightmapping.GetLightingSettingsOrDefaultsFallback().realtimeGI;

                if (bakedGI && realtimeGI)
                {
                    // if the user has selected the only state that is supported, then gray it out
                    using (new EditorGUI.DisabledScope(m_RealtimeEnvironmentLighting.boolValue && !bakedGISupported))
                    {
                        EditorGUILayout.PropertyField(m_RealtimeEnvironmentLighting, Styles.RealtimeEnvironmentLighting);
                    }

                    // if they have selected a state that isnt supported, show dialog, and still make the box editable
                    if (!m_RealtimeEnvironmentLighting.boolValue && !bakedGISupported)
                    {
                        EditorGUILayout.HelpBox("The following mode is not supported and will fallback on Realtime", MessageType.Warning);
                    }
                }
                // Show "Realtime" on if baked GI is disabled (but we don't wanna show the box if the whole mode is not supported.)
                else
                {
                    using (new EditorGUI.DisabledScope(true))
                    {
                        EditorGUILayout.Toggle(Styles.RealtimeEnvironmentLighting, realtimeGI);
                    }
                }

                EditorGUI.indentLevel -= 2;
                EditorGUILayout.Space();
            }
        }

        void OnMixedModeSelected(object userData)
        {
            m_MixedBakeMode.intValue = (int)userData;
        }

        void MixedLightingGUI()
        {
            if (!SupportedRenderingFeatures.IsLightmapBakeTypeSupported(LightmapBakeType.Baked))
                return;

            m_ShowMixedLightsSettings.value = EditorGUILayout.FoldoutTitlebar(m_ShowMixedLightsSettings.value, Styles.MixedLightsLabel, true);

            if (m_ShowMixedLightsSettings.value)
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.PropertyField(m_EnabledBakedGI, Styles.EnableBaked);

                if (!m_EnabledBakedGI.boolValue)
                {
                    EditorGUILayout.HelpBox(Styles.BakedGIDisabledInfo.text, MessageType.Info);
                }

                using (new EditorGUI.DisabledScope(!m_EnabledBakedGI.boolValue))
                {
                    bool mixedGISupported = SupportedRenderingFeatures.IsLightmapBakeTypeSupported(LightmapBakeType.Mixed);

                    using (new EditorGUI.DisabledScope(!mixedGISupported))
                    {
                        var rect = EditorGUILayout.GetControlRect();
                        EditorGUI.BeginProperty(rect, Styles.MixedLightMode, m_MixedBakeMode);
                        rect = EditorGUI.PrefixLabel(rect, Styles.MixedLightMode);

                        int index = Math.Max(0, Array.IndexOf(Styles.MixedModeValues, m_MixedBakeMode.intValue));

                        if (EditorGUI.DropdownButton(rect, Styles.MixedModeStrings[index], FocusType.Passive))
                        {
                            var menu = new GenericMenu();

                            for (int i = 0; i < Styles.MixedModeValues.Length; i++)
                            {
                                int value = Styles.MixedModeValues[i];
                                bool selected = (value == m_MixedBakeMode.intValue);

                                if (!SupportedRenderingFeatures.IsMixedLightingModeSupported((MixedLightingMode)value))
                                    menu.AddDisabledItem(Styles.MixedModeStrings[i], selected);
                                else
                                    menu.AddItem(Styles.MixedModeStrings[i], selected, OnMixedModeSelected, value);
                            }
                            menu.DropDown(rect);
                        }
                        EditorGUI.EndProperty();

                        if (mixedGISupported)
                        {
                            if (!SupportedRenderingFeatures.IsMixedLightingModeSupported((MixedLightingMode)m_MixedBakeMode.intValue))
                            {
                                string fallbackMode = Styles.MixedModeStrings[(int)SupportedRenderingFeatures.FallbackMixedLightingMode()].text;
                                EditorGUILayout.HelpBox(Styles.MixedModeNotSupportedWarning.text + fallbackMode, MessageType.Warning);
                            }
                            else if (m_EnabledBakedGI.boolValue)
                            {
                                EditorGUILayout.HelpBox(Styles.HelpStringsMixed[m_MixedBakeMode.intValue].text, MessageType.Info);
                            }
                        }
                    }
                }
                EditorGUI.indentLevel--;
                EditorGUILayout.Space();
            }
        }

        public void DeveloperBuildSettingsGUI()
        {
            if (!Unsupported.IsDeveloperMode())
                return;

            Lightmapping.concurrentJobsType = (Lightmapping.ConcurrentJobsType)EditorGUILayout.IntPopup(Styles.ConcurrentJobs, (int)Lightmapping.concurrentJobsType, Styles.ConcurrentJobsTypeStrings, Styles.ConcurrentJobsTypeValues);
            EditorGUILayout.PropertyField(m_ForceWhiteAlbedo, Styles.ForceWhiteAlbedo);
            EditorGUILayout.PropertyField(m_ForceUpdates, Styles.ForceUpdates);

            if (m_BakeBackend.intValue != (int)LightingSettings.Lightmapper.Enlighten)
            {
                EditorGUILayout.PropertyField(m_ExportTrainingData, Styles.ExportTrainingData);

                if (m_ExportTrainingData.boolValue)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(m_TrainingDataDestination, Styles.TrainingDataDestination);
                    EditorGUI.indentLevel--;
                }
            }

            EditorGUILayout.PropertyField(m_FilterMode, Styles.FilterMode);

            EditorGUILayout.Slider(m_BounceScale, 0.0f, 10.0f, Styles.BounceScale);

            if (GUILayout.Button("Clear disk cache", GUILayout.Width(Styles.ButtonWidth)))
            {
                Lightmapping.Clear();
                Lightmapping.ClearDiskCache();
            }

            if (GUILayout.Button("Print state to console", GUILayout.Width(Styles.ButtonWidth)))
            {
                Lightmapping.PrintStateToConsole();
            }

            if (GUILayout.Button("Reset albedo/emissive", GUILayout.Width(Styles.ButtonWidth)))
                GIDebugVisualisation.ResetRuntimeInputTextures();

            if (GUILayout.Button("Reset environment", GUILayout.Width(Styles.ButtonWidth)))
                DynamicGI.UpdateEnvironment();
        }

        private void ClampFilterType(SerializedProperty filter)
        {
            if (m_BakeBackend.intValue == (int)LightingSettings.Lightmapper.ProgressiveGPU)
            {
                // Force unsupported A-Trous filter back to Gaussian.
                if (filter.intValue == (int)LightingSettings.FilterType.ATrous)
                    filter.intValue = (int)LightingSettings.FilterType.Gaussian;
            }
        }

        void OnDirectDenoiserSelected(object userData)
        {
            m_PVRDenoiserTypeDirect.intValue = (int)userData;
        }

        void OnIndirectDenoiserSelected(object userData)
        {
            m_PVRDenoiserTypeIndirect.intValue = (int)userData;
        }

        void OnAODenoiserSelected(object userData)
        {
            m_PVRDenoiserTypeAO.intValue = (int)userData;
        }

        public enum DenoiserTarget
        {
            Direct = 0,
            Indirect = 1,
            AO = 2
        }
        void DrawDenoiserTypeDropdown(SerializedProperty prop, GUIContent label, DenoiserTarget target)
        {
            var rect = EditorGUILayout.GetControlRect();
            EditorGUI.BeginProperty(rect, label, prop);
            rect = EditorGUI.PrefixLabel(rect, label);

            int index = Math.Max(0, Array.IndexOf(Styles.DenoiserTypeValues, prop.intValue));

            if (EditorGUI.DropdownButton(rect, Styles.DenoiserTypeStrings[index], FocusType.Passive))
            {
                bool radeonDenoiserSupported = Lightmapping.IsRadeonDenoiserSupported();
                bool openImageDenoiserSupported = Lightmapping.IsOpenImageDenoiserSupported();
                bool optixDenoiserSupported = Lightmapping.IsOptixDenoiserSupported();
                var menu = new GenericMenu();
                for (int i = 0; i < Styles.DenoiserTypeValues.Length; i++)
                {
                    int value = Styles.DenoiserTypeValues[i];
                    bool optixDenoiserItem = (value == (int)LightingSettings.DenoiserType.Optix);
                    bool openImageDenoiserItem = (value == (int)LightingSettings.DenoiserType.OpenImage);
                    bool radeonDenoiserItem = (value == (int)LightingSettings.DenoiserType.RadeonPro);
                    bool selected = (value == prop.intValue);

                    if (!optixDenoiserSupported && optixDenoiserItem)
                        menu.AddDisabledItem(Styles.DenoiserTypeStrings[i], selected);
                    else if (!openImageDenoiserSupported && openImageDenoiserItem)
                        menu.AddDisabledItem(Styles.DenoiserTypeStrings[i], selected);
                    else if (!radeonDenoiserSupported && radeonDenoiserItem)
                        menu.AddDisabledItem(Styles.DenoiserTypeStrings[i], selected);
                    else
                    {
                        if (target == DenoiserTarget.Direct)
                            menu.AddItem(Styles.DenoiserTypeStrings[i], selected, OnDirectDenoiserSelected, value);
                        else if (target == DenoiserTarget.Indirect)
                            menu.AddItem(Styles.DenoiserTypeStrings[i], selected, OnIndirectDenoiserSelected, value);
                        else if (target == DenoiserTarget.AO)
                            menu.AddItem(Styles.DenoiserTypeStrings[i], selected, OnAODenoiserSelected, value);
                    }
                }
                menu.DropDown(rect);
            }
            EditorGUI.EndProperty();
        }

        bool DenoiserSupported(LightingSettings.DenoiserType denoiserType)
        {
            if (denoiserType == LightingSettings.DenoiserType.Optix && !Lightmapping.IsOptixDenoiserSupported())
                return false;
            if (denoiserType == LightingSettings.DenoiserType.OpenImage && !Lightmapping.IsOpenImageDenoiserSupported())
                return false;
            if (denoiserType == LightingSettings.DenoiserType.RadeonPro && !Lightmapping.IsRadeonDenoiserSupported())
                return false;

            return true;
        }

        void OnBakeBackedSelected(object userData)
        {
            m_BakeBackend.intValue = (int)userData;
        }

        void BakeBackendGUI()
        {
            var rect = EditorGUILayout.GetControlRect();
            EditorGUI.BeginProperty(rect, Styles.BakeBackend, m_BakeBackend);
            EditorGUI.BeginChangeCheck();
            rect = EditorGUI.PrefixLabel(rect, Styles.BakeBackend);

            int index = Math.Max(0, Array.IndexOf(Styles.BakeBackendValues, m_BakeBackend.intValue));

            if (EditorGUI.DropdownButton(rect, Styles.BakeBackendStrings[index], FocusType.Passive))
            {
                var menu = new GenericMenu();

                for (int i = 0; i < Styles.BakeBackendValues.Length; i++)
                {
                    int value = Styles.BakeBackendValues[i];
                    bool selected = (value == m_BakeBackend.intValue);

                    if (!SupportedRenderingFeatures.IsLightmapperSupported(value))
                        menu.AddDisabledItem(Styles.BakeBackendStrings[i], selected);
                    else
                        menu.AddItem(Styles.BakeBackendStrings[i], selected, OnBakeBackedSelected, value);
                }
                menu.DropDown(rect);
            }
            if (EditorGUI.EndChangeCheck())
                InspectorWindow.RepaintAllInspectors(); // We need to repaint other inspectors that might need to update based on the selected backend.

            EditorGUI.EndProperty();

            if (!SupportedRenderingFeatures.IsLightmapperSupported(m_BakeBackend.intValue))
            {
                string fallbackLightmapper = Styles.BakeBackendStrings[SupportedRenderingFeatures.FallbackLightmapper()].text;
                EditorGUILayout.HelpBox(Styles.LightmapperNotSupportedWarning.text + fallbackLightmapper, MessageType.Warning);
            }
        }

        void GeneralLightmapSettingsGUI()
        {
            bool bakedGISupported = SupportedRenderingFeatures.IsLightmapBakeTypeSupported(LightmapBakeType.Baked);
            bool realtimeGISupported = SupportedRenderingFeatures.IsLightmapBakeTypeSupported(LightmapBakeType.Realtime);
            bool lightmapperSupported = SupportedRenderingFeatures.IsLightmapperSupported(m_BakeBackend.intValue);

            if (!bakedGISupported && !realtimeGISupported)
                return;

            m_ShowGeneralLightmapSettings.value = EditorGUILayout.FoldoutTitlebar(m_ShowGeneralLightmapSettings.value, Styles.GeneralLightmapLabel, true);

            if (m_ShowGeneralLightmapSettings.value)
            {
                EditorGUI.indentLevel++;
                using (new EditorGUI.DisabledScope(!m_EnabledBakedGI.boolValue && !m_EnableRealtimeGI.boolValue))
                {
                    if (bakedGISupported)
                    {
                        using (new EditorGUI.DisabledScope(!m_EnabledBakedGI.boolValue))
                        {
                            BakeBackendGUI();


                            if (lightmapperSupported)
                            {
                                if (m_BakeBackend.intValue == (int)LightingSettings.Lightmapper.Enlighten)
                                {
                                    EditorGUI.indentLevel++;

                                    EditorGUILayout.PropertyField(m_FinalGather, Styles.FinalGather);
                                    if (m_FinalGather.boolValue)
                                    {
                                        EditorGUI.indentLevel++;
                                        EditorGUILayout.PropertyField(m_FinalGatherRayCount, Styles.FinalGatherRayCount);
                                        EditorGUILayout.PropertyField(m_FinalGatherFiltering, Styles.FinalGatherFiltering);
                                        EditorGUI.indentLevel--;
                                    }

                                    EditorGUI.indentLevel--;
                                }

                                if (m_BakeBackend.intValue != (int)LightingSettings.Lightmapper.Enlighten)
                                {
                                    EditorGUI.indentLevel++;

                                    EditorGUILayout.PropertyField(m_PVRCulling, Styles.PVRCulling);

                                    bool enableMIS = (m_PVREnvironmentMIS.intValue & 1) != 0;
                                    if (EditorGUILayout.Toggle(Styles.PVREnvironmentMIS, enableMIS))
                                        m_PVREnvironmentMIS.intValue |= 1;
                                    else
                                        m_PVREnvironmentMIS.intValue &= ~1;

                                    // Sampling type
                                    //EditorGUILayout.PropertyField(m_PvrSampling, Styles.m_PVRSampling); // TODO(PVR): make non-fixed sampling modes work.

                                    if (m_PVRSampling.intValue != (int)LightingSettings.Sampling.Auto)
                                    {
                                        // Update those constants also in LightmapBake.cpp UpdateSamples() and LightmapBake.h.
                                        // NOTE: sample count needs to be a power of two as we are using Sobol sequence.
                                        const int kMinDirectSamples = 1;
                                        const int kMinEnvironmentSamples = 8;
                                        const int kMinSamples = 8;
                                        //We are counting the samples number on int32 hence the limitation here
                                        const int kMaxSamples = int.MaxValue;

                                        // Sample count
                                        // TODO(PVR): make non-fixed sampling modes work.
                                        //EditorGUI.indentLevel++;
                                        //if (LightingSettings.giPathTracerSampling == LightingSettings.PathTracerSampling.PathTracerSamplingAdaptive)
                                        //  EditorGUILayout.PropertyField(m_PVRSampleCount, Styles.PVRSampleCountAdaptive);
                                        //else

                                        EditorGUILayout.PropertyField(m_PVRDirectSampleCount, Styles.PVRDirectSampleCount);
                                        EditorGUILayout.PropertyField(m_PVRSampleCount, Styles.PVRIndirectSampleCount);

                                        if (m_PVRSampleCount.intValue < kMinSamples ||
                                            m_PVRSampleCount.intValue > kMaxSamples)
                                        {
                                            m_PVRSampleCount.intValue = Math.Max(Math.Min(m_PVRSampleCount.intValue, kMaxSamples), kMinSamples);
                                        }

                                        if (m_PVRDirectSampleCount.intValue < kMinDirectSamples ||
                                            m_PVRDirectSampleCount.intValue > kMaxSamples)
                                        {
                                            m_PVRDirectSampleCount.intValue = Math.Max(Math.Min(m_PVRDirectSampleCount.intValue, kMaxSamples), kMinDirectSamples);
                                        }

                                        EditorGUILayout.PropertyField(m_PVREnvironmentSampleCount, Styles.PVREnvironmentSampleCount);

                                        if (m_PVREnvironmentSampleCount.intValue < kMinEnvironmentSamples || m_PVREnvironmentSampleCount.intValue > kMaxSamples)
                                        {
                                            m_PVREnvironmentSampleCount.intValue = Math.Max(Math.Min(m_PVREnvironmentSampleCount.intValue, kMaxSamples), kMinEnvironmentSamples);
                                        }

                                        using (new EditorGUI.DisabledScope(EditorSettings.useLegacyProbeSampleCount))
                                        {
                                            EditorGUILayout.PropertyField(m_LightProbeSampleCountMultiplier, Styles.ProbeSampleCountMultiplier);
                                            int directSampleCount = m_PVRDirectSampleCount.intValue;
                                            int indirectSampleCount = m_PVRSampleCount.intValue;
                                            int environmentSampleCount = m_PVREnvironmentSampleCount.intValue;
                                            int maxSampleCount = Math.Max(directSampleCount, Math.Max(indirectSampleCount, environmentSampleCount));
                                            float maxMultiplier = (float)kMaxSamples / (float)maxSampleCount;
                                            if (m_LightProbeSampleCountMultiplier.floatValue > maxMultiplier)
                                            {
                                                m_LightProbeSampleCountMultiplier.floatValue = Math.Min(m_LightProbeSampleCountMultiplier.floatValue, maxMultiplier);
                                                if (m_LightProbeSampleCountMultiplier.floatValue > 2.0f)
                                                    m_LightProbeSampleCountMultiplier.floatValue = (float)Math.Floor((double)m_LightProbeSampleCountMultiplier.floatValue);
                                            }
                                            float minMultiplier = (float)kMinSamples / (float)maxSampleCount;
                                            if (m_LightProbeSampleCountMultiplier.floatValue < minMultiplier)
                                            {
                                                m_LightProbeSampleCountMultiplier.floatValue = Math.Max(m_LightProbeSampleCountMultiplier.floatValue, minMultiplier);
                                            }
                                        }

                                        // TODO(PVR): make non-fixed sampling modes work.
                                        //EditorGUI.indentLevel--;
                                    }

                                    EditorGUILayout.IntPopup(m_PVRBounces, Styles.BouncesStrings, Styles.BouncesValues, Styles.PVRBounces);

                                    // Filtering
                                    EditorGUILayout.PropertyField(m_PVRFilteringMode, Styles.PVRFilteringMode);

                                    if (m_PVRFilteringMode.intValue == (int)LightingSettings.FilterMode.Advanced)
                                    {
                                        // Check if the platform doesn't support denoising.
                                        bool usingGPULightmapper = m_BakeBackend.intValue == (int)LightingSettings.Lightmapper.ProgressiveGPU;
                                        bool anyDenoisingSupported = (Lightmapping.IsOptixDenoiserSupported() || Lightmapping.IsOpenImageDenoiserSupported() || Lightmapping.IsRadeonDenoiserSupported());
                                        bool aoDenoisingSupported = DenoiserSupported((LightingSettings.DenoiserType)m_PVRDenoiserTypeAO.intValue);
                                        bool directDenoisingSupported = DenoiserSupported((LightingSettings.DenoiserType)m_PVRDenoiserTypeDirect.intValue);
                                        bool indirectDenoisingSupported = DenoiserSupported((LightingSettings.DenoiserType)m_PVRDenoiserTypeIndirect.intValue);

                                        EditorGUI.indentLevel++;
                                        using (new EditorGUI.DisabledScope(!anyDenoisingSupported))
                                        {
                                            DrawDenoiserTypeDropdown(m_PVRDenoiserTypeDirect, directDenoisingSupported ? Styles.PVRDenoiserTypeDirect : Styles.DenoisingWarningDirect, DenoiserTarget.Direct);
                                        }
                                        ClampFilterType(m_PVRFilterTypeDirect);
                                        if (m_BakeBackend.intValue == (int)LightingSettings.Lightmapper.ProgressiveGPU)
                                            EditorGUILayout.IntPopup(m_PVRFilterTypeDirect, Styles.GPUFilterOptions, Styles.GPUFilterInts, Styles.PVRFilterTypeDirect);
                                        else
                                            EditorGUILayout.PropertyField(m_PVRFilterTypeDirect, Styles.PVRFilterTypeDirect);

                                        EditorGUI.indentLevel++;
                                        DrawFilterSettingField(m_PVRFilteringGaussRadiusDirect,
                                            m_PVRFilteringAtrousPositionSigmaDirect,
                                            Styles.PVRFilteringGaussRadiusDirect,
                                            Styles.PVRFilteringAtrousPositionSigmaDirect,
                                            (LightingSettings.FilterType)m_PVRFilterTypeDirect.intValue);
                                        EditorGUI.indentLevel--;

                                        EditorGUILayout.Space();

                                        using (new EditorGUI.DisabledScope(!anyDenoisingSupported))
                                        {
                                            DrawDenoiserTypeDropdown(m_PVRDenoiserTypeIndirect, indirectDenoisingSupported ? Styles.PVRDenoiserTypeIndirect : Styles.DenoisingWarningIndirect, DenoiserTarget.Indirect);
                                        }
                                        if (m_BakeBackend.intValue == (int)LightingSettings.Lightmapper.ProgressiveGPU)
                                            EditorGUILayout.IntPopup(m_PVRFilterTypeIndirect, Styles.GPUFilterOptions, Styles.GPUFilterInts, Styles.PVRFilterTypeIndirect);
                                        else
                                            EditorGUILayout.PropertyField(m_PVRFilterTypeIndirect, Styles.PVRFilterTypeIndirect);
                                        ClampFilterType(m_PVRFilterTypeIndirect);

                                        EditorGUI.indentLevel++;
                                        DrawFilterSettingField(m_PVRFilteringGaussRadiusIndirect,
                                            m_PVRFilteringAtrousPositionSigmaIndirect,
                                            Styles.PVRFilteringGaussRadiusIndirect,
                                            Styles.PVRFilteringAtrousPositionSigmaIndirect,
                                            (LightingSettings.FilterType)m_PVRFilterTypeIndirect.intValue);
                                        EditorGUI.indentLevel--;

                                        using (new EditorGUI.DisabledScope(!m_AmbientOcclusion.boolValue))
                                        {
                                            EditorGUILayout.Space();
                                            using (new EditorGUI.DisabledScope(!anyDenoisingSupported))
                                            {
                                                DrawDenoiserTypeDropdown(m_PVRDenoiserTypeAO, aoDenoisingSupported ? Styles.PVRDenoiserTypeAO : Styles.DenoisingWarningAO, DenoiserTarget.AO);
                                            }
                                            if (m_BakeBackend.intValue == (int)LightingSettings.Lightmapper.ProgressiveGPU)
                                                EditorGUILayout.IntPopup(m_PVRFilterTypeAO, Styles.GPUFilterOptions, Styles.GPUFilterInts, Styles.PVRFilterTypeAO);
                                            else
                                                EditorGUILayout.PropertyField(m_PVRFilterTypeAO, Styles.PVRFilterTypeAO);
                                            ClampFilterType(m_PVRFilterTypeAO);

                                            EditorGUI.indentLevel++;
                                            DrawFilterSettingField(m_PVRFilteringGaussRadiusAO,
                                                m_PVRFilteringAtrousPositionSigmaAO,
                                                Styles.PVRFilteringGaussRadiusAO, Styles.PVRFilteringAtrousPositionSigmaAO,
                                                (LightingSettings.FilterType)m_PVRFilterTypeAO.intValue);
                                            EditorGUI.indentLevel--;
                                        }
                                        // Show warning if A-Trous filtering is selected and the platform doesn't support it.
                                        if (usingGPULightmapper && (m_PVRFilterTypeDirect.intValue == (int)LightingSettings.FilterType.ATrous || m_PVRFilterTypeIndirect.intValue == (int)LightingSettings.FilterType.ATrous || (m_AmbientOcclusion.boolValue && m_PVRFilterTypeAO.intValue == (int)LightingSettings.FilterType.ATrous)))
                                            EditorGUILayout.HelpBox(Styles.ProgressiveGPUWarning.text, MessageType.Warning);

                                        EditorGUI.indentLevel--;
                                    }

                                    EditorGUI.indentLevel--;
                                }
                            }
                        }
                    }

                    // We only want to show the Indirect Resolution in a disabled state if the user is using PLM and has the ability to turn on Realtime GI.
                    if (realtimeGISupported || (bakedGISupported && (m_BakeBackend.intValue == (int)LightingSettings.Lightmapper.Enlighten) && lightmapperSupported))
                    {
                        using (new EditorGUI.DisabledScope((m_BakeBackend.intValue != (int)LightingSettings.Lightmapper.Enlighten) && !m_EnableRealtimeGI.boolValue))
                        {
                            DrawResolutionField(m_RealtimeResolution, Styles.IndirectResolution);
                        }
                    }

                    if (bakedGISupported)
                    {
                        using (new EditorGUI.DisabledScope(!m_EnabledBakedGI.boolValue))
                        {
                            DrawResolutionField(m_BakeResolution, Styles.LightmapResolution);

                            GUILayout.BeginHorizontal();
                            EditorGUILayout.PropertyField(m_Padding, Styles.Padding);
                            GUILayout.Label(" texels", Styles.LabelStyle);
                            GUILayout.EndHorizontal();

                            EditorGUILayout.IntPopup(m_LightmapMaxSize, Styles.LightmapMaxSizeStrings, Styles.LightmapMaxSizeValues, Styles.LightmapMaxSize);

                            EditorGUILayout.PropertyField(m_TextureCompression, Styles.TextureCompression);

                            EditorGUILayout.PropertyField(m_AmbientOcclusion, Styles.AmbientOcclusion);
                            if (m_AmbientOcclusion.boolValue)
                            {
                                EditorGUI.indentLevel++;
                                EditorGUILayout.PropertyField(m_AOMaxDistance, Styles.AOMaxDistance);
                                if (m_AOMaxDistance.floatValue < 0.0f)
                                    m_AOMaxDistance.floatValue = 0.0f;
                                EditorGUILayout.Slider(m_CompAOExponent, 0.0f, 10.0f, Styles.AmbientOcclusionContribution);
                                EditorGUILayout.Slider(m_CompAOExponentDirect, 0.0f, 10.0f, Styles.AmbientOcclusionContributionDirect);

                                EditorGUI.indentLevel--;
                            }
                        }
                    }

                    bool directionalSupported = SupportedRenderingFeatures.IsLightmapsModeSupported(LightmapsMode.CombinedDirectional);

                    if (directionalSupported || (m_LightmapDirectionalMode.intValue == (int)LightmapsMode.CombinedDirectional))
                    {
                        EditorGUILayout.IntPopup(m_LightmapDirectionalMode, Styles.LightmapDirectionalModeStrings, Styles.LightmapDirectionalModeValues, Styles.LightmapDirectionalMode);

                        if (!directionalSupported)
                        {
                            EditorGUILayout.HelpBox(Styles.DirectionalNotSupportedWarning.text, MessageType.Warning);
                        }
                    }
                    else
                    {
                        using (new EditorGUI.DisabledScope(true))
                        {
                            EditorGUILayout.IntPopup(Styles.LightmapDirectionalMode, 0, Styles.LightmapDirectionalModeStrings, Styles.LightmapDirectionalModeValues);
                        }
                    }

                    // albedo boost, push the albedo value towards one in order to get more bounce
                    EditorGUILayout.Slider(m_AlbedoBoost, 1.0f, 10.0f, Styles.AlbedoBoost);

                    if (LightmapParametersGUI(m_LightmapParameters, Styles.DefaultLightmapParameters))
                    {
                        EditorWindow.FocusWindowIfItsOpen<InspectorWindow>();
                    }
                }

                EditorGUI.indentLevel--;
                EditorGUILayout.Space();
            }
        }

        public void OnGUI()
        {
            lightingSettings.UpdateIfRequiredOrScript();

            using (new EditorGUI.DisabledScope(m_LightingSettingsReadOnlyMode))
            {
                RealtimeLightingGUI();
                MixedLightingGUI();
                GeneralLightmapSettingsGUI();
            }

            lightingSettings.ApplyModifiedProperties();
        }

        static class Styles
        {
            public static readonly int[] BakeBackendValues = { (int)LightingSettings.Lightmapper.Enlighten, (int)LightingSettings.Lightmapper.ProgressiveCPU, (int)LightingSettings.Lightmapper.ProgressiveGPU };
            public static readonly GUIContent[] BakeBackendStrings =
            {
                EditorGUIUtility.TrTextContent("Enlighten (Deprecated)"),
                EditorGUIUtility.TrTextContent("Progressive CPU"),
                EditorGUIUtility.TrTextContent("Progressive GPU (Preview)"),
            };

            public static readonly int[] LightmapDirectionalModeValues = { (int)LightmapsMode.NonDirectional, (int)LightmapsMode.CombinedDirectional };
            public static readonly GUIContent[] LightmapDirectionalModeStrings =
            {
                EditorGUIUtility.TrTextContent("Non-Directional"),
                EditorGUIUtility.TrTextContent("Directional"),
            };

            public static readonly int[] LightmapMaxSizeValues = { 32, 64, 128, 256, 512, 1024, 2048, 4096 };
            public static readonly GUIContent[] LightmapMaxSizeStrings = Array.ConvertAll(LightmapMaxSizeValues, (x) => new GUIContent(x.ToString()));

            public static readonly int[] ConcurrentJobsTypeValues = { (int)Lightmapping.ConcurrentJobsType.Min, (int)Lightmapping.ConcurrentJobsType.Low, (int)Lightmapping.ConcurrentJobsType.High };
            public static readonly GUIContent[] ConcurrentJobsTypeStrings =
            {
                EditorGUIUtility.TrTextContent("Min"),
                EditorGUIUtility.TrTextContent("Low"),
                EditorGUIUtility.TrTextContent("High")
            };

            // must match LightmapMixedBakeMode
            public static readonly int[] MixedModeValues = { 0, 1, 2 };
            public static readonly GUIContent[] MixedModeStrings =
            {
                EditorGUIUtility.TrTextContent("Baked Indirect"),
                EditorGUIUtility.TrTextContent("Subtractive"),
                EditorGUIUtility.TrTextContent("Shadowmask")
            };

            // must match PVRDenoiserType
            public static readonly int[] DenoiserTypeValues = { (int)LightingSettings.DenoiserType.Optix, (int)LightingSettings.DenoiserType.OpenImage, (int)LightingSettings.DenoiserType.RadeonPro, (int)LightingSettings.DenoiserType.None };
            public static readonly GUIContent[] DenoiserTypeStrings =
            {
                EditorGUIUtility.TrTextContent("Optix"),
                EditorGUIUtility.TrTextContent("OpenImageDenoise"),
                EditorGUIUtility.TrTextContent("Radeon Pro"),
                EditorGUIUtility.TrTextContent("None")
            };

            public static readonly int[] BouncesValues = { 0, 1, 2, 3, 4 };
            public static readonly GUIContent[] BouncesStrings =
            {
                EditorGUIUtility.TrTextContent("None"),
                EditorGUIUtility.TextContent("1"),
                EditorGUIUtility.TextContent("2"),
                EditorGUIUtility.TextContent("3"),
                EditorGUIUtility.TextContent("4")
            };

            public static readonly GUIContent[] HelpStringsMixed =
            {
                EditorGUIUtility.TrTextContent("Mixed lights provide realtime direct lighting while indirect light is baked into lightmaps and light probes."),
                EditorGUIUtility.TrTextContent("Mixed lights provide baked direct and indirect lighting for static objects. Dynamic objects receive realtime direct lighting and cast shadows on static objects using the main directional light in the scene."),
                EditorGUIUtility.TrTextContent("Mixed lights provide realtime direct lighting. Indirect lighting gets baked into lightmaps and light probes. Shadowmasks and light probes occlusion get generated for baked shadows. The Shadowmask Mode used at run time can be set in the Quality Settings panel.")
            };

            // TODO(RadeonRays): Used for hiding A-trous filtering option until it is implemented.
            public static readonly GUIContent[] GPUFilterOptions = new[] { EditorGUIUtility.TrTextContent("Gaussian"), EditorGUIUtility.TrTextContent("None") };
            public static readonly int[] GPUFilterInts = new[] { (int)LightingSettings.FilterType.Gaussian, (int)LightingSettings.FilterType.None };

            public static readonly GUIContent LightmapperNotSupportedWarning = EditorGUIUtility.TrTextContent("The Lightmapper is not supported by the current render pipeline. Fallback is ");
            public static readonly GUIContent MixedModeNotSupportedWarning = EditorGUIUtility.TrTextContent("The Mixed mode is not supported by the current render pipeline. Fallback mode is ");
            public static readonly GUIContent DirectionalNotSupportedWarning = EditorGUIUtility.TrTextContent("Directional Mode is not supported. Fallback will be Non-Directional.");

            public static readonly GUIContent EnableBaked = EditorGUIUtility.TrTextContent("Baked Global Illumination", "Controls whether Mixed and Baked lights will use baked Global Illumination. If enabled, Mixed lights are baked using the specified Lighting Mode and Baked lights will be completely baked and not adjustable at runtime.");
            public static readonly GUIContent BounceScale = EditorGUIUtility.TrTextContent("Bounce Scale", "Multiplier for indirect lighting. Use with care.");
            public static readonly GUIContent UpdateThreshold = EditorGUIUtility.TrTextContent("Update Threshold", "Threshold for updating realtime GI. A lower value causes more frequent updates (default 1.0).");
            public static readonly GUIContent AlbedoBoost = EditorGUIUtility.TrTextContent("Albedo Boost", "Controls the amount of light bounced between surfaces by intensifying the albedo of materials in the scene. Increasing this draws the albedo value towards white for indirect light computation. The default value is physically accurate.");
            public static readonly GUIContent LightmapDirectionalMode = EditorGUIUtility.TrTextContent("Directional Mode", "Controls whether baked and realtime lightmaps will store directional lighting information from the lighting environment. Options are Directional and Non-Directional.");
            public static readonly GUIContent DefaultLightmapParameters = EditorGUIUtility.TrTextContent("Lightmap Parameters", "Allows the adjustment of advanced parameters that affect the process of generating a lightmap for an object using global illumination.");
            public static readonly GUIContent RealtimeLightsLabel = EditorGUIUtility.TrTextContent("Realtime Lighting", "Precompute Realtime indirect lighting for realtime lights and static objects. In this mode realtime lights, ambient lighting, materials of static objects (including emission) will generate indirect lighting at runtime. Only static objects are blocking and bouncing light, dynamic objects receive indirect lighting via light probes.");
            public static readonly GUIContent RealtimeEnvironmentLighting = EditorGUIUtility.TrTextContent("Realtime Environment Lighting", "Specifies the Global Illumination mode that should be used for handling ambient light in the Scene. This property is not editable unless both Realtime Global Illumination and Baked Global Illumination are enabled for the scene.");
            public static readonly GUIContent MixedLightsLabel = EditorGUIUtility.TrTextContent("Mixed Lighting", "Bake Global Illumination for mixed lights and static objects. May bake both direct and/or indirect lighting based on settings. Only static objects are blocking and bouncing light, dynamic objects receive baked lighting via light probes.");
            public static readonly GUIContent GeneralLightmapLabel = EditorGUIUtility.TrTextContent("Lightmapping Settings", "Settings that apply to both Global Illumination modes (Precomputed Realtime and Baked).");
            public static readonly GUIContent NoRealtimeGIInSM2AndGLES2 = EditorGUIUtility.TrTextContent("Realtime Global Illumination is not supported on SM2.0 hardware nor when using GLES2.0.");
            public static readonly GUIContent ConcurrentJobs = EditorGUIUtility.TrTextContent("Concurrent Jobs", "The amount of simultaneously scheduled jobs.");
            public static readonly GUIContent ForceWhiteAlbedo = EditorGUIUtility.TrTextContent("Force White Albedo", "Force white albedo during lighting calculations.");
            public static readonly GUIContent ForceUpdates = EditorGUIUtility.TrTextContent("Force Updates", "Force continuous updates of runtime indirect lighting calculations.");
            public static readonly GUIContent FilterMode = EditorGUIUtility.TrTextContent("Filter Mode");
            public static readonly GUIContent ExportTrainingData = EditorGUIUtility.TrTextContent("Export Training Data", "Exports unfiltered textures, normals, positions.");
            public static readonly GUIContent TrainingDataDestination = EditorGUIUtility.TrTextContent("Destination", "Destination for the training data, for example 'mysetup/30samples'. Will still be located at the first level in the project folder. ");
            public static readonly GUIStyle   LabelStyle = EditorStyles.wordWrappedMiniLabel;
            public static readonly GUIContent IndirectResolution = EditorGUIUtility.TrTextContent("Indirect Resolution", "Sets the resolution in texels that are used per unit for objects being lit by indirect lighting. The larger the value, the more significant the impact will be on the time it takes to bake the lighting.");
            public static readonly GUIContent LightmapResolution = EditorGUIUtility.TrTextContent("Lightmap Resolution", "Sets the resolution in texels that are used per unit for objects being lit by baked global illumination. Larger values will result in increased time to calculate the baked lighting.");
            public static readonly GUIContent Padding = EditorGUIUtility.TrTextContent("Lightmap Padding", "Sets the separation in texels between shapes in the baked lightmap.");
            public static readonly GUIContent LightmapMaxSize = EditorGUIUtility.TrTextContent("Max Lightmap Size", "Sets the max size of the full lightmap Texture in pixels. Values are squared, so a setting of 1024 can produce a 1024x1024 pixel sized lightmap.");
            public static readonly GUIContent TextureCompression = EditorGUIUtility.TrTextContent("Compress Lightmaps", "Controls whether the baked lightmap is compressed or not. When enabled, baked lightmaps are compressed to reduce required storage space but some artifacting may be present due to compression.");
            public static readonly GUIContent AmbientOcclusion = EditorGUIUtility.TrTextContent("Ambient Occlusion", "Specifies whether to include ambient occlusion or not in the baked lightmap result. Enabling this results in simulating the soft shadows that occur in cracks and crevices of objects when light is reflected onto them.");
            public static readonly GUIContent AmbientOcclusionContribution = EditorGUIUtility.TrTextContent("Indirect Contribution", "Adjusts the contrast of ambient occlusion applied to indirect lighting. The larger the value, the more contrast is applied to the ambient occlusion for indirect lighting.");
            public static readonly GUIContent AmbientOcclusionContributionDirect = EditorGUIUtility.TrTextContent("Direct Contribution", "Adjusts the contrast of ambient occlusion applied to the direct lighting. The larger the value is, the more contrast is applied to the ambient occlusion for direct lighting. This effect is not physically accurate.");
            public static readonly GUIContent AOMaxDistance = EditorGUIUtility.TrTextContent("Max Distance", "Controls how far rays are cast in order to determine if an object is occluded or not. A larger value produces longer rays and contributes more shadows to the lightmap, while a smaller value produces shorter rays that contribute shadows only when objects are very close to one another. A value of 0 casts an infinitely long ray that has no maximum distance.");
            public static readonly GUIContent FinalGather = EditorGUIUtility.TrTextContent("Final Gather", "Specifies whether the final light bounce of the global illumination calculation is calculated at the same resolution as the baked lightmap. When enabled, visual quality is improved at the cost of additional time required to bake the lighting.");
            public static readonly GUIContent FinalGatherRayCount = EditorGUIUtility.TrTextContent("Ray Count", "Controls the number of rays emitted for every final gather point.");
            public static readonly GUIContent FinalGatherFiltering = EditorGUIUtility.TrTextContent("Denoising", "Controls whether a denoising filter is applied to the final gather output.");
            public static readonly GUIContent MixedLightMode = EditorGUIUtility.TrTextContent("Lighting Mode", "Specifies which Scene lighting mode will be used for all Mixed lights in the Scene. Options are Baked Indirect, Shadowmask and Subtractive.");
            public static readonly GUIContent UseRealtimeGI = EditorGUIUtility.TrTextContent("Realtime Global Illumination (Deprecated)", "Enlighten is entering deprecation. Please ensure that your project will not require support for Enlighten beyond the deprecation date.");
            public static readonly GUIContent BakedGIDisabledInfo = EditorGUIUtility.TrTextContent("All Baked and Mixed lights in the Scene are currently being overridden to Realtime light modes. Enable Baked Global Illumination to allow the use of Baked and Mixed light modes.");
            public static readonly GUIContent BakeBackend = EditorGUIUtility.TrTextContent("Lightmapper", "Specifies which baking system will be used to generate baked lightmaps.");
            //public static readonly GUIContent PVRSampling = EditorGUIUtility.TrTextContent("Sampling", "How to sample the lightmaps. Auto and adaptive automatically test for convergence. Auto uses a maximum of 16K samples. Adaptive uses a configurable maximum number of samples. Fixed always uses the set number of samples and does not test for convergence.");
            //public static readonly GUIContent PVRDirectSampleCountAdaptive = EditorGUIUtility.TrTextContent("Max Direct Samples", "Maximum number of samples to use for direct lighting.");
            public static readonly GUIContent PVRDirectSampleCount = EditorGUIUtility.TrTextContent("Direct Samples", "Controls the number of samples the lightmapper will use for direct lighting calculations. Increasing this value may improve the quality of lightmaps but increases the time required for baking to complete.");
            //public static readonly GUIContent PVRSampleCountAdaptive = EditorGUIUtility.TrTextContent("Max Indirect Samples", "Maximum number of samples to use for indirect lighting.");
            public static readonly GUIContent PVRIndirectSampleCount = EditorGUIUtility.TrTextContent("Indirect Samples", "Controls the number of samples the lightmapper will use for indirect lighting calculations. Increasing this value may improve the quality of lightmaps but increases the time required for baking to complete.");
            public static readonly GUIContent PVRBounces = EditorGUIUtility.TrTextContent("Bounces", "Controls the maximum number of bounces the lightmapper will compute for indirect light.");
            public static readonly GUIContent DenoisingWarningDirect = EditorGUIUtility.TrTextContent("Direct Denoiser", "Your hardware doesn't support denoising. To see minimum requirements, read the documentation.");
            public static readonly GUIContent DenoisingWarningIndirect = EditorGUIUtility.TrTextContent("Indirect Denoiser", "Your hardware doesn't support denoising. To see minimum requirements, read the documentation.");
            public static readonly GUIContent DenoisingWarningAO = EditorGUIUtility.TrTextContent("Ambient Occlusion Denoiser", "Your hardware doesn't support denoising. To see minimum requirements, read the documentation.");
            public static readonly GUIContent PVRDenoiserTypeDirect = EditorGUIUtility.TrTextContent("Direct Denoiser", "Specifies the type of denoiser used to reduce noise for direct lights.");
            public static readonly GUIContent PVRDenoiserTypeIndirect = EditorGUIUtility.TrTextContent("Indirect Denoiser", "Specifies the type of denoiser used to reduce noise for indirect lights.");
            public static readonly GUIContent PVRDenoiserTypeAO = EditorGUIUtility.TrTextContent("Ambient Occlusion Denoiser", "Specifies the type of denoiser used to reduce noise for ambient occlusion.");
            public static readonly GUIContent PVRFilteringMode = EditorGUIUtility.TrTextContent("Filtering", "Specifies the method to reduce noise in baked lightmaps.");
            public static readonly GUIContent PVRFilterTypeDirect = EditorGUIUtility.TrTextContent("Direct Filter", "Specifies the filter kernel applied to the direct light stored in the lightmap. Gaussian blurs the lightmap with some loss of detail. A-Trous reduces noise based on a threshold while maintaining edge detail.");
            public static readonly GUIContent PVRFilterTypeIndirect = EditorGUIUtility.TrTextContent("Indirect Filter", "Specifies the filter kernel applied to the indirect light stored in the lightmap. Gaussian blurs the lightmap with some loss of detail. A-Trous reduces noise based on a threshold while maintaining edge detail.");
            public static readonly GUIContent PVRFilterTypeAO = EditorGUIUtility.TrTextContent("Ambient Occlusion Filter", "Specifies the filter kernel applied to the ambient occlusion stored in the lightmap. Gaussian blurs the lightmap with some loss of detail. A-Trous reduces noise based on a threshold while maintaining edge detail.");
            public static readonly GUIContent PVRFilteringGaussRadiusDirect = EditorGUIUtility.TrTextContent("Radius", "Controls the radius of the filter for direct light stored in the lightmap. A higher value gives a stronger blur and less noise.");
            public static readonly GUIContent PVRFilteringGaussRadiusIndirect = EditorGUIUtility.TrTextContent("Radius", "Controls the radius of the filter for indirect light stored in the lightmap. A higher value gives a stronger blur and less noise.");
            public static readonly GUIContent PVRFilteringGaussRadiusAO = EditorGUIUtility.TrTextContent("Radius", "Controls the radius of the filter for ambient occlusion stored in the lightmap. A higher value gives a stronger blur and less noise.");
            public static readonly GUIContent PVRFilteringAtrousPositionSigmaDirect = EditorGUIUtility.TrTextContent("Sigma", "Controls the threshold of the filter for direct light stored in the lightmap. A higher value increases the threshold, which reduces noise in the direct layer of the lightmap. Too high of a value can cause a loss of detail in the lightmap.");
            public static readonly GUIContent PVRFilteringAtrousPositionSigmaIndirect = EditorGUIUtility.TrTextContent("Sigma", "Controls the threshold of the filter for indirect light stored in the lightmap. A higher value increases the threshold, which reduces noise in the direct layer of the lightmap. Too high of a value can cause a loss of detail in the lightmap.");
            public static readonly GUIContent PVRFilteringAtrousPositionSigmaAO = EditorGUIUtility.TrTextContent("Sigma", "Controls the threshold of the filter for ambient occlusion stored in the lightmap. A higher value increases the threshold, which reduces noise in the direct layer of the lightmap. Too high of a value can cause a loss of detail in the lightmap.");
            public static readonly GUIContent PVRCulling = EditorGUIUtility.TrTextContent("Progressive Updates", "Specifies whether the lightmapper should prioritize baking what is visible in the scene view. When disabled, lightmaps are only composited once fully converged which can improve baking performance.");
            public static readonly GUIContent PVREnvironmentMIS = EditorGUIUtility.TrTextContent("Multiple Importance Sampling", "Specifies whether to use multiple importance sampling for sampling the environment. This will generally lead to faster convergence when generating lightmaps but can lead to noisier results in certain low frequency environments.");
            public static readonly GUIContent PVREnvironmentSampleCount = EditorGUIUtility.TrTextContent("Environment Samples", "Controls the number of samples the lightmapper will use for environment lighting calculations. Increasing this value may improve the quality of lightmaps but increases the time required for baking to complete.");
            public static readonly GUIContent ProbeSampleCountMultiplier = EditorGUIUtility.TrTextContent("Light Probe Sample Multiplier", "Controls how many samples are used for Light Probes as a multiplier of the general sample counts above. Higher values improve the quality of Light Probes, but also take longer to bake. Enable the Light Probe sample count multiplier in the Editor tab under Project Settings.");
            public static readonly GUIContent ProgressiveGPUWarning = EditorGUIUtility.TrTextContent("A-Trous filtering is not implemented in the Progressive GPU lightmapper yet. Use the CPU lightmapper instead if you need this functionality.");

            public static readonly float ButtonWidth = 160;
        }
    }
} // namespace
