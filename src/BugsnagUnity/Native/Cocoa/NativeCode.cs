﻿using System;
using System.Runtime.InteropServices;

namespace BugsnagUnity
{
  [StructLayout(LayoutKind.Sequential)]
  struct NativeUser
  {
    public IntPtr Id;
  }

  partial class NativeCode
  {
    const string StartBugsnagWithConfigurationMethod = "bugsnag_startBugsnagWithConfiguration";
    const string SetMetadataMethod = "bugsnag_setMetadata";
    const string RetrieveDeviceDataMethod = "bugsnag_retrieveDeviceData";
    const string RetrieveAppDataMethod = "bugsnag_retrieveAppData";
    const string PopulateUserMethod = "bugsnag_populateUser";
    const string CreateBreadcrumbsMethod = "bugsnag_createBreadcrumbs";
    const string AddBreadcrumbMethod = "bugsnag_addBreadcrumb";
    const string RetrieveBreadcrumbsMethod = "bugsnag_retrieveBreadcrumbs";

    [DllImport(Import, EntryPoint = StartBugsnagWithConfigurationMethod)]
    internal static extern void StartBugsnagWithConfiguration(IntPtr configuration);

    [DllImport(Import, EntryPoint = StartBugsnagWithConfigurationMethod)]
    internal static extern void AddMetadata(IntPtr configuration, string tab, string[] metadata, int metadataCount);

    [DllImport(Import, EntryPoint = RetrieveAppDataMethod)]
    internal static extern void RetrieveAppData(IntPtr instance, Action<IntPtr, string, string> populate);

    [DllImport(Import, EntryPoint = RetrieveDeviceDataMethod)]
    internal static extern void RetrieveDeviceData(IntPtr instance, Action<IntPtr, string, string> populate);

    [DllImport(Import, EntryPoint = PopulateUserMethod)]
    internal static extern void PopulateUser(ref NativeUser user);

    [DllImport(Import, EntryPoint = "bugsnag_createConfiguration")]
    internal static extern IntPtr CreateConfiguration(string apiKey);

    [DllImport(Import, EntryPoint = "bugsnag_getApiKey")]
    internal static extern IntPtr GetApiKey(IntPtr configuration);

    [DllImport(Import, EntryPoint = "bugsnag_setReleaseStage")]
    internal static extern void SetReleaseStage(IntPtr configuration, string releaseStage);

    [DllImport(Import, EntryPoint = "bugsnag_getReleaseStage")]
    internal static extern IntPtr GetReleaseStage(IntPtr configuration);

    [DllImport(Import, EntryPoint = "bugsnag_setContext")]
    internal static extern void SetContext(IntPtr configuration, string context);

    [DllImport(Import, EntryPoint = "bugsnag_getContext")]
    internal static extern IntPtr GetContext(IntPtr configuration);

    [DllImport(Import, EntryPoint = "bugsnag_setAppVersion")]
    internal static extern void SetAppVersion(IntPtr configuration, string appVersion);

    [DllImport(Import, EntryPoint = "bugsnag_getAppVersion")]
    internal static extern IntPtr GetAppVersion(IntPtr configuration);

    [DllImport(Import, EntryPoint = "bugsnag_setNotifyUrl")]
    internal static extern void SetNotifyEndpoint(IntPtr configuration, string endpoint);

    [DllImport(Import, EntryPoint = "bugsnag_getNotifyUrl")]
    internal static extern IntPtr GetNotifyEndpoint(IntPtr configuration);

    internal delegate void NotifyReleaseStageCallback(IntPtr instance, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)]string[] releaseStages, long count);

    [DllImport(Import, EntryPoint = "bugsnag_getNotifyReleaseStages")]
    internal static extern void GetNotifyReleaseStages(IntPtr configuration, IntPtr instance, NotifyReleaseStageCallback callback);

    [DllImport(Import, EntryPoint = "bugsnag_setNotifyReleaseStages")]
    internal static extern void SetNotifyReleaseStages(IntPtr configuration, string[] releaseStages, int count);

    [DllImport(Import, EntryPoint = CreateBreadcrumbsMethod)]
    internal static extern IntPtr CreateBreadcrumbs(IntPtr configuration);

    [DllImport(Import, EntryPoint = AddBreadcrumbMethod)]
    internal static extern void NativeAddBreadcrumb(IntPtr breadcrumbs, string name, string type, string[] metadata, int metadataCount);

    internal delegate void BreadcrumbInformation(IntPtr instance, string name, string timestamp, string type, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 5)]string[] keys, long keysSize, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 7)]string[] values, long valuesSize);

    [DllImport(Import, EntryPoint = RetrieveBreadcrumbsMethod)]
    internal static extern void NativeRetrieveBreadcrumbs(IntPtr breadcrumbs, IntPtr instance, BreadcrumbInformation visitor);
  }
}