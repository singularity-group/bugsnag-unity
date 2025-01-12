using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using BugsnagUnity.Payload;
using UnityEngine;
using System.Threading;
using System.Text;

namespace BugsnagUnity
{
    class NativeInterface
    {
        private IntPtr BugsnagNativeInterface;
        private IntPtr BugsnagUnityClass;
        // Cache of classes used:
        private IntPtr LastRunInfoClass;
        private IntPtr BreadcrumbClass;
        private IntPtr BreadcrumbTypeClass;
        private IntPtr CollectionClass;
        private IntPtr IteratorClass;
        private IntPtr ListClass;
        private IntPtr MapClass;
        private IntPtr DateClass;
        private IntPtr DateUtilsClass;
        private IntPtr MapEntryClass;
        private IntPtr SetClass;
        private IntPtr StringClass;
        private IntPtr SessionClass;
        private IntPtr ClientClass;

        // Cache of methods used:
        private IntPtr BreadcrumbGetMessage;
        private IntPtr BreadcrumbGetMetadata;
        private IntPtr BreadcrumbGetTimestamp;
        private IntPtr BreadcrumbGetType;
        private IntPtr ClassIsArray;
        private IntPtr CollectionIterator;
        private IntPtr IteratorHasNext;
        private IntPtr IteratorNext;
        private IntPtr MapEntryGetKey;
        private IntPtr MapEntryGetValue;
        private IntPtr MapEntrySet;
        private IntPtr ObjectGetClass;
        private IntPtr ObjectToString;
        private IntPtr ToIso8601;
        private IntPtr AddFeatureFlagMethod;
        private IntPtr ClearFeatureFlagMethod;
        private IntPtr ClearFeatureFlagsMethod;






        private bool CanRunOnBackgroundThread;

        private static bool Unity2019OrNewer;
        private Thread MainThread;

        private class OnSessionCallback : AndroidJavaProxy
        {

            private Configuration _config;

            public OnSessionCallback(Configuration config) : base("com.bugsnag.android.OnSessionCallback")
            {
                _config = config;
            }
            public bool onSession(AndroidJavaObject session)
            {

                var wrapper = new NativeSession(session);
                foreach (var callback in _config.GetOnSessionCallbacks())
                {
                    try
                    {
                        if (!callback.Invoke(wrapper))
                        {
                            return false;
                        }
                    }
                    catch {
                        // If the callback causes an exception, ignore it and execute the next one
                    }

                }

                return true;
            }
        }

        private class OnSendErrorCallback : AndroidJavaProxy
        {
            private Configuration _config;

            public OnSendErrorCallback(Configuration config) : base("com.bugsnag.android.OnSendCallback")
            {
                _config = config;
            }
            public bool onSend(AndroidJavaObject @event)
            {
                var wrapper = new NativeEvent(@event);
                foreach (var callback in _config.GetOnSendErrorCallbacks())
                {
                    try
                    {
                        if (!callback.Invoke(wrapper))
                        {
                            return false;
                        }
                    }
                    catch {
                        // If the callback causes an exception, ignore it and execute the next one
                    }
                }
                return true;
            }
        }

        public NativeInterface(Configuration cfg)
        {
            AndroidJavaObject config = CreateNativeConfig(cfg);
            Unity2019OrNewer = IsUnity2019OrNewer();
            MainThread = Thread.CurrentThread;
            using (AndroidJavaClass system = new AndroidJavaClass("java.lang.System"))
            {
                string arch = system.CallStatic<string>("getProperty", "os.arch");
                CanRunOnBackgroundThread = (arch != "x86" && arch != "i686" && arch != "x86_64");
            }
            using (AndroidJavaClass unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (AndroidJavaObject activity = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity"))
            using (AndroidJavaObject context = activity.Call<AndroidJavaObject>("getApplicationContext"))
            using (AndroidJavaObject client = new AndroidJavaObject("com.bugsnag.android.Client", context, config))
            {
                // lookup the NativeInterface class and set the client to the local object.
                // all subsequent communication should go through the NativeInterface.
                IntPtr nativeInterfaceRef = AndroidJNI.FindClass("com/bugsnag/android/NativeInterface");
                BugsnagNativeInterface = AndroidJNI.NewGlobalRef(nativeInterfaceRef);
                AndroidJNI.DeleteLocalRef(nativeInterfaceRef);

                IntPtr setClient = AndroidJNI.GetStaticMethodID(BugsnagNativeInterface, "setClient", "(Lcom/bugsnag/android/Client;)V");

                object[] args = new object[] { client };
                jvalue[] jargs = AndroidJNIHelper.CreateJNIArgArray(args);
                AndroidJNI.CallStaticVoidMethod(BugsnagNativeInterface, setClient, jargs);
                AndroidJNIHelper.DeleteJNIArgArray(args, jargs);

                // Cache JNI refs which will be used to load report data later in the
                // app lifecycle to avoid repeated lookups
                IntPtr unityRef = AndroidJNI.FindClass("com/bugsnag/android/unity/BugsnagUnity");
                BugsnagUnityClass = AndroidJNI.NewGlobalRef(unityRef);
                AndroidJNI.DeleteLocalRef(unityRef);

                IntPtr crumbRef = AndroidJNI.FindClass("com/bugsnag/android/Breadcrumb");
                BreadcrumbClass = AndroidJNI.NewGlobalRef(crumbRef);
                AndroidJNI.DeleteLocalRef(crumbRef);

                IntPtr lastRunInfoRef = AndroidJNI.FindClass("com/bugsnag/android/LastRunInfo");
                LastRunInfoClass = AndroidJNI.NewGlobalRef(lastRunInfoRef);
                AndroidJNI.DeleteLocalRef(lastRunInfoRef);

                IntPtr crumbTypeRef = AndroidJNI.FindClass("com/bugsnag/android/BreadcrumbType");
                BreadcrumbTypeClass = AndroidJNI.NewGlobalRef(crumbTypeRef);
                AndroidJNI.DeleteLocalRef(crumbTypeRef);

                IntPtr collectionRef = AndroidJNI.FindClass("java/util/Collection");
                CollectionClass = AndroidJNI.NewGlobalRef(collectionRef);
                AndroidJNI.DeleteLocalRef(collectionRef);

                IntPtr iterRef = AndroidJNI.FindClass("java/util/Iterator");
                IteratorClass = AndroidJNI.NewGlobalRef(iterRef);
                AndroidJNI.DeleteLocalRef(iterRef);

                IntPtr listRef = AndroidJNI.FindClass("java/util/List");
                ListClass = AndroidJNI.NewGlobalRef(listRef);
                AndroidJNI.DeleteLocalRef(listRef);

                IntPtr mapRef = AndroidJNI.FindClass("java/util/Map");
                MapClass = AndroidJNI.NewGlobalRef(mapRef);
                AndroidJNI.DeleteLocalRef(mapRef);

                IntPtr dateRef = AndroidJNI.FindClass("java/util/Date");
                DateClass = AndroidJNI.NewGlobalRef(dateRef);
                AndroidJNI.DeleteLocalRef(dateRef);

                IntPtr dateUtilsRef = AndroidJNI.FindClass("com/bugsnag/android/internal/DateUtils");
                DateUtilsClass = AndroidJNI.NewGlobalRef(dateUtilsRef);

                IntPtr entryRef = AndroidJNI.FindClass("java/util/Map$Entry");
                MapEntryClass = AndroidJNI.NewGlobalRef(entryRef);
                AndroidJNI.DeleteLocalRef(entryRef);

                IntPtr setRef = AndroidJNI.FindClass("java/util/Set");
                SetClass = AndroidJNI.NewGlobalRef(setRef);
                AndroidJNI.DeleteLocalRef(setRef);

                IntPtr stringRef = AndroidJNI.FindClass("java/lang/String");
                StringClass = AndroidJNI.NewGlobalRef(stringRef);
                AndroidJNI.DeleteLocalRef(stringRef);

                IntPtr sessionRef = AndroidJNI.FindClass("com/bugsnag/android/Session");
                SessionClass = AndroidJNI.NewGlobalRef(sessionRef);
                AndroidJNI.DeleteLocalRef(sessionRef);

                IntPtr clientRef = AndroidJNI.FindClass("com/bugsnag/android/Client");
                ClientClass = AndroidJNI.NewGlobalRef(clientRef);
                AndroidJNI.DeleteLocalRef(clientRef);

                BreadcrumbGetMetadata = AndroidJNI.GetMethodID(BreadcrumbClass, "getMetadata", "()Ljava/util/Map;");
                BreadcrumbGetType = AndroidJNI.GetMethodID(BreadcrumbClass, "getType", "()Lcom/bugsnag/android/BreadcrumbType;");
                BreadcrumbGetTimestamp = AndroidJNI.GetMethodID(BreadcrumbClass, "getStringTimestamp", "()Ljava/lang/String;");
                BreadcrumbGetMessage = AndroidJNI.GetMethodID(BreadcrumbClass, "getMessage", "()Ljava/lang/String;");
                CollectionIterator = AndroidJNI.GetMethodID(CollectionClass, "iterator", "()Ljava/util/Iterator;");
                IteratorHasNext = AndroidJNI.GetMethodID(IteratorClass, "hasNext", "()Z");
                IteratorNext = AndroidJNI.GetMethodID(IteratorClass, "next", "()Ljava/lang/Object;");
                MapEntryGetKey = AndroidJNI.GetMethodID(MapEntryClass, "getKey", "()Ljava/lang/Object;");
                MapEntryGetValue = AndroidJNI.GetMethodID(MapEntryClass, "getValue", "()Ljava/lang/Object;");
                MapEntrySet = AndroidJNI.GetMethodID(MapClass, "entrySet", "()Ljava/util/Set;");
                AddFeatureFlagMethod = AndroidJNI.GetMethodID(ClientClass, "addFeatureFlag", "(Ljava/lang/String;Ljava/lang/String;)V");
                ClearFeatureFlagMethod = AndroidJNI.GetMethodID(ClientClass, "clearFeatureFlag", "(Ljava/lang/String;)V");
                ClearFeatureFlagsMethod = AndroidJNI.GetMethodID(ClientClass, "clearFeatureFlags", "()V");

                IntPtr objectRef = AndroidJNI.FindClass("java/lang/Object");
                ObjectToString = AndroidJNI.GetMethodID(objectRef, "toString", "()Ljava/lang/String;");
                ObjectGetClass = AndroidJNI.GetMethodID(objectRef, "getClass", "()Ljava/lang/Class;");
                AndroidJNI.DeleteLocalRef(objectRef);

                IntPtr classRef = AndroidJNI.FindClass("java/lang/Class");
                ClassIsArray = AndroidJNI.GetMethodID(classRef, "isArray", "()Z");
                AndroidJNI.DeleteLocalRef(classRef);

                ToIso8601 = AndroidJNI.GetStaticMethodID(DateUtilsClass, "toIso8601", "(Ljava/util/Date;)Ljava/lang/String;");
                AndroidJNI.DeleteLocalRef(dateUtilsRef);

                // the bugsnag-android notifier uses Activity lifecycle tracking to
                // determine if the application is in the foreground. As the unity
                // activity has already started at this point we need to tell the
                // notifier about the activity and the fact that it has started.
                using (AndroidJavaObject sessionTracker = client.Get<AndroidJavaObject>("sessionTracker"))
                using (AndroidJavaObject activityClass = activity.Call<AndroidJavaObject>("getClass"))
                {
                    string activityName = null;
                    using (AndroidJavaObject activityNameObject = activityClass.Call<AndroidJavaObject>("getSimpleName"))
                    {
                        if (activityNameObject != null)
                        {
                            activityName = AndroidJNI.GetStringUTFChars(activityNameObject.GetRawObject());
                        }
                    }
                    sessionTracker.Call("updateForegroundTracker", activityName, true, 0L);
                }

                ConfigureNotifierInfo(client);
            }
        }

        /**
         * Transforms an IConfiguration C# object into a Java Configuration object.
         */
        AndroidJavaObject CreateNativeConfig(Configuration config)
        {
            var obj = new AndroidJavaObject("com.bugsnag.android.Configuration", config.ApiKey);
            // configure automatic tracking of errors/sessions
            using (AndroidJavaObject errorTypes = new AndroidJavaObject("com.bugsnag.android.ErrorTypes"))
            {
                errorTypes.Call("setAnrs", config.EnabledErrorTypes.ANRs);
                errorTypes.Call("setNdkCrashes", config.EnabledErrorTypes.Crashes);
                errorTypes.Call("setUnhandledExceptions", config.EnabledErrorTypes.Crashes);
                obj.Call("setEnabledErrorTypes", errorTypes);
            }

            obj.Call("setAutoTrackSessions", config.AutoTrackSessions);
            obj.Call("setAutoDetectErrors", config.AutoDetectErrors);
            obj.Call("setAppVersion", config.AppVersion);
            obj.Call("setContext", config.Context);
            obj.Call("setMaxBreadcrumbs", config.MaximumBreadcrumbs);
            obj.Call("setMaxPersistedEvents", config.MaxPersistedEvents);
            obj.Call("setPersistUser", config.PersistUser);
            obj.Call("setLaunchDurationMillis", config.LaunchDurationMillis);
            obj.Call("setSendLaunchCrashesSynchronously", config.SendLaunchCrashesSynchronously);

            if (config.GetUser() != null)
            {
                var user = config.GetUser();
                obj.Call("setUser", user.Id, user.Email, user.Name);
            }

            //Register for callbacks
            obj.Call("addOnSession", new OnSessionCallback(config));
            obj.Call("addOnSend", new OnSendErrorCallback(config));


            // set endpoints
            var notify = config.Endpoints.Notify.ToString();
            var sessions = config.Endpoints.Session.ToString();
            using (AndroidJavaObject endpointConfig = new AndroidJavaObject("com.bugsnag.android.EndpointConfiguration", notify, sessions))
            {
                obj.Call("setEndpoints", endpointConfig);
            }

            //android layer expects a nonnull java Integer not just an int, so we check if it has actually been set to a valid value
            if (config.VersionCode > -1)
            {
                var javaInteger = new AndroidJavaObject("java.lang.Integer", config.VersionCode);
                obj.Call("setVersionCode", javaInteger);
            }

            //Null or empty check necessary because android will set the app.type to empty if that or null is passed as default
            if (!string.IsNullOrEmpty(config.AppType))
            {
                obj.Call("setAppType", config.AppType);
            }

            // set EnabledBreadcrumbTypes
            if (config.EnabledBreadcrumbTypes != null)
            {

                using (AndroidJavaObject enabledBreadcrumbs = new AndroidJavaObject("java.util.HashSet"))
                {
                    AndroidJavaClass androidBreadcrumbEnumClass = new AndroidJavaClass("com.bugsnag.android.BreadcrumbType");
                    for (int i = 0; i < config.EnabledBreadcrumbTypes.Length; i++)
                    {
                        var stringValue = Enum.GetName(typeof(BreadcrumbType), config.EnabledBreadcrumbTypes[i]).ToUpper();
                        using (AndroidJavaObject crumbType = androidBreadcrumbEnumClass.CallStatic<AndroidJavaObject>("valueOf", stringValue))
                        {
                            enabledBreadcrumbs.Call<Boolean>("add", crumbType);
                        }
                    }
                    obj.Call("setEnabledBreadcrumbTypes", enabledBreadcrumbs);
                }
            }

            // set feature flags
            if (config.FeatureFlags != null && config.FeatureFlags.Count > 0)
            {
                foreach (var flag in config.FeatureFlags)
                {
                    obj.Call("addFeatureFlag",flag.Name,flag.Variant);
                }
            }

            // set sendThreads
            AndroidJavaClass androidThreadSendPolicyClass = new AndroidJavaClass("com.bugsnag.android.ThreadSendPolicy");
            using (AndroidJavaObject policy = androidThreadSendPolicyClass.CallStatic<AndroidJavaObject>("valueOf", GetAndroidFormatThreadSendName(config.SendThreads)))
            {
                obj.Call("setSendThreads", policy);
            }

            // set release stages
            obj.Call("setReleaseStage", config.ReleaseStage);

            if (config.EnabledReleaseStages != null && config.EnabledReleaseStages.Length > 0)
            {
                obj.Call("setEnabledReleaseStages", GetAndroidStringSetFromArray(config.EnabledReleaseStages));
            }

            // set DiscardedClasses
            if (config.DiscardClasses != null && config.DiscardClasses.Length > 0)
            {
                obj.Call("setDiscardClasses", GetAndroidStringSetFromArray(config.DiscardClasses));
            }

            // set ProjectPackages
            if (config.ProjectPackages != null && config.ProjectPackages.Length > 0)
            {
                obj.Call("setProjectPackages", GetAndroidStringSetFromArray(config.ProjectPackages));
            }

            // set redacted keys
            if (config.RedactedKeys != null && config.RedactedKeys.Length > 0)
            {
                obj.Call("setRedactedKeys", GetAndroidStringSetFromArray(config.RedactedKeys));
            }

            // add unity event callback
            var BugsnagUnity = new AndroidJavaClass("com.bugsnag.android.unity.BugsnagUnity");
            obj.Call("addOnError", BugsnagUnity.CallStatic<AndroidJavaObject>("getNativeCallback", new object[] { }));

            // set persistence directory
            if (!string.IsNullOrEmpty(config.PersistenceDirectory))
            {
                AndroidJavaObject androidFile = new AndroidJavaObject("java.io.File", config.PersistenceDirectory);
                obj.Call("setPersistenceDirectory", androidFile);
            }

            return obj;
        }

        private string GetAndroidFormatThreadSendName(ThreadSendPolicy threadSendPolicy)
        {
            switch (threadSendPolicy)
            {
                case ThreadSendPolicy.Always:
                    return "ALWAYS";
                case ThreadSendPolicy.UnhandledOnly:
                    return "UNHANDLED_ONLY";
                default:
                    return "NEVER";
            }
        }

        private AndroidJavaObject GetAndroidStringSetFromArray(string[] array)
        {
            AndroidJavaObject set = new AndroidJavaObject("java.util.HashSet");
            foreach (var item in array)
            {
                set.Call<Boolean>("add", item);
            }
            return set;
        }

        private void ConfigureNotifierInfo(AndroidJavaObject client)
        {
            using (AndroidJavaObject notifier = client.Get<AndroidJavaObject>("notifier"))
            {

                AndroidJavaObject androidNotifier = new AndroidJavaObject("com.bugsnag.android.Notifier");
                androidNotifier.Call("setUrl", androidNotifier.Get<string>("url"));
                androidNotifier.Call("setName", androidNotifier.Get<string>("name"));
                androidNotifier.Call("setVersion", androidNotifier.Get<string>("version"));
                AndroidJavaObject list = new AndroidJavaObject("java.util.ArrayList");
                list.Call<Boolean>("add", androidNotifier);
                notifier.Call("setDependencies", list);

                notifier.Call("setUrl", NotifierInfo.NotifierUrl);
                notifier.Call("setName", "Unity Bugsnag Notifier");
                notifier.Call("setVersion", NotifierInfo.NotifierVersion);
            }
        }

        /**
         * Pushes a local JNI frame with 128 capacity. This avoids the reference table
         * being exceeded, which can happen on some lower-end Android devices in extreme conditions
         * (e.g. Nexus 7 running Android 6). This is likely due to AndroidJavaObject
         * not deleting local references immediately.
         *
         * If this call is unsuccessful it indicates the device is low on memory so the caller should no-op.
         * https://docs.unity3d.com/ScriptReference/AndroidJNI.PopLocalFrame.html
         */
        private bool PushLocalFrame()
        {
            if (AndroidJNI.PushLocalFrame(128) != 0)
            {
                AndroidJNI.ExceptionClear(); // clear pending OutOfMemoryError.
                return false;
            }
            return true;
        }

        /**
         * Pops the local JNI frame, freeing any references in the table.
         * https://docs.unity3d.com/ScriptReference/AndroidJNI.PopLocalFrame.html
         */
        private void PopLocalFrame()
        {
            AndroidJNI.PopLocalFrame(System.IntPtr.Zero);
        }

        public void SetAutoDetectErrors(bool newValue)
        {
            CallNativeVoidMethod("setAutoNotify", "(Z)V", new object[] { newValue });
        }

        public void SetContext(string newValue)
        {
            CallNativeVoidMethod("setContext", "(Ljava/lang/String;)V", new object[] { newValue });
        }

        public void SetUser(User user)
        {
            var method = "setUser";
            var description = "(Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;)V";
            if (user == null)
            {
                CallNativeVoidMethod(method, description, new object[] { null, null, null });
            }
            else
            {
                CallNativeVoidMethod(method, description,
                    new object[] { user.Id, user.Email, user.Name });
            }
        }

        public void StartSession()
        {
            CallNativeVoidMethod("startSession", "()V", new object[] { });
        }

        public bool ResumeSession()
        {
            return CallNativeBoolMethod("resumeSession", "()Z", new object[] { });
        }

        public void PauseSession()
        {
            CallNativeVoidMethod("pauseSession", "()V", new object[] { });
        }

        public void UpdateSession(Session session)
        {
            if (session != null)
            {
                // The ancient version of the runtime used doesn't have an equivalent to GetUnixTime()
                var startedAt = (long)(session.StartedAt - new DateTime(1970, 1, 1, 0, 0, 0, 0))?.TotalMilliseconds;
                CallNativeVoidMethod("registerSession", "(JLjava/lang/String;II)V", new object[]{
                    startedAt, session.Id.ToString(), session.UnhandledCount(),
                    session.HandledCount()
                });
            }
        }

        public Session GetCurrentSession()
        {
            var javaSession = CallNativeObjectMethodRef("getCurrentSession", "()Lcom/bugsnag/android/Session;", new object[] { });

            var id = AndroidJNI.CallStringMethod(javaSession, AndroidJNIHelper.GetMethodID(SessionClass, "getId"), new jvalue[] { });

            if (id == null)
            {
                return null;
            }

            var javaStartedAt = AndroidJNI.CallObjectMethod(javaSession, AndroidJNIHelper.GetMethodID(SessionClass, "getStartedAt"), new jvalue[] { });
            var unhandledCount = AndroidJNI.CallIntMethod(javaSession, AndroidJNIHelper.GetMethodID(SessionClass, "getUnhandledCount"), new jvalue[] { });
            var handledCount = AndroidJNI.CallIntMethod(javaSession, AndroidJNIHelper.GetMethodID(SessionClass, "getHandledCount"), new jvalue[] { });
            var timeLong = AndroidJNI.CallLongMethod(javaStartedAt, AndroidJNIHelper.GetMethodID(DateClass, "getTime"), new jvalue[] { });
            var unityDateTime = new DateTime(1970, 1, 1).AddMilliseconds(timeLong);

            return new Session(id, unityDateTime, unhandledCount, handledCount);

        }

        public void MarkLaunchCompleted()
        {
            CallNativeVoidMethod("markLaunchCompleted", "()V", new object[] { });
        }

        public Dictionary<string, object> GetApp()
        {
            return GetJavaMapData("getApp");
        }

        public Dictionary<string, object> GetDevice()
        {
            return GetJavaMapData("getDevice");
        }

        public Dictionary<string, object> GetMetadata()
        {
            return GetJavaMapData("getMetadata");
        }

        public Dictionary<string, object> GetUser()
        {
            return GetJavaMapData("getUser");
        }

        public void ClearMetadata(string tab)
        {
            if (tab == null)
            {
                return;
            }
            CallNativeVoidMethod("clearMetadata", "(Ljava/lang/String;Ljava/lang/String;)V", new object[] { tab, null });
        }

        public void ClearMetadata(string tab, string key)
        {
            if (tab == null)
            {
                return;
            }
            CallNativeVoidMethod("clearMetadata", "(Ljava/lang/String;Ljava/lang/String;)V", new object[] { tab, key });
        }

        public void AddMetadata(string tab, string key, string value)
        {
            if (tab == null || key == null)
            {
                return;
            }
            CallNativeVoidMethod("addMetadata", "(Ljava/lang/String;Ljava/lang/String;Ljava/lang/Object;)V",
                new object[] { tab, key, value });
        }

        public void LeaveBreadcrumb(string name, string type, IDictionary<string, object> metadata)
        {
            if (!CanRunJNI())
            {
                return;
            }
            bool isAttached = bsg_unity_isJNIAttached();
            if (!isAttached)
            {
                AndroidJNI.AttachCurrentThread();
            }
            if (PushLocalFrame())
            {
                using (AndroidJavaObject map = BuildJavaMapDisposable(metadata))
                {
                    CallNativeVoidMethod("leaveBreadcrumb", "(Ljava/lang/String;Ljava/lang/String;Ljava/util/Map;)V",
                        new object[] { name, type, map });
                }
                PopLocalFrame();
            }
            if (!isAttached)
            {
                AndroidJNI.DetachCurrentThread();
            }
        }

        public List<Breadcrumb> GetBreadcrumbs()
        {
            List<Breadcrumb> breadcrumbs = new List<Breadcrumb>();
            if (!CanRunJNI())
            {
                return breadcrumbs;
            }
            bool isAttached = bsg_unity_isJNIAttached();
            if (!isAttached)
            {
                AndroidJNI.AttachCurrentThread();
            }

            IntPtr javaBreadcrumbs = CallNativeObjectMethodRef("getBreadcrumbs", "()Ljava/util/List;", new object[] { });
            IntPtr iterator = AndroidJNI.CallObjectMethod(javaBreadcrumbs, CollectionIterator, new jvalue[] { });
            AndroidJNI.DeleteLocalRef(javaBreadcrumbs);

            while (AndroidJNI.CallBooleanMethod(iterator, IteratorHasNext, new jvalue[] { }))
            {
                IntPtr crumb = AndroidJNI.CallObjectMethod(iterator, IteratorNext, new jvalue[] { });
                breadcrumbs.Add(ConvertToBreadcrumb(crumb));
                AndroidJNI.DeleteLocalRef(crumb);
            }
            AndroidJNI.DeleteLocalRef(iterator);

            if (!isAttached)
            {
                AndroidJNI.DetachCurrentThread();
            }

            return breadcrumbs;
        }

        private Dictionary<string, object> GetJavaMapData(string methodName)
        {
            if (!CanRunJNI())
            {
                return new Dictionary<string, object>();
            }
            bool isAttached = bsg_unity_isJNIAttached();
            if (!isAttached)
            {
                AndroidJNI.AttachCurrentThread();
            }

            IntPtr map = CallNativeObjectMethodRef(methodName, "()Ljava/util/Map;", new object[] { });
            Dictionary<string, object> value = DictionaryFromJavaMap(map);
            AndroidJNI.DeleteLocalRef(map);

            if (!isAttached)
            {
                AndroidJNI.DetachCurrentThread();
            }

            return value;
        }

        // Manually converts any C# strings in the arguments, replacing invalid chars with the replacement char..
        // If we don't do this, C# will coerce them using NewStringUTF, which crashes on invalid UTF-8.
        // Arg lists processed this way must be released using ReleaseConvertedStringArgs.
        private object[] ConvertStringArgsToNative(object[] args)
        {
            object[] itemsAsJavaObjects = new object[args.Length];
            for (int i = 0; i < args.Length; i++)
            {
                var obj = args[i];

                if (obj is string)
                {
                    itemsAsJavaObjects[i] = BuildJavaStringDisposable(obj as string);
                }
                else
                {
                    itemsAsJavaObjects[i] = obj;
                }
            }
            return itemsAsJavaObjects;
        }

        // Release any strings in a processed argument list.
        // @param originalArgs: The original C# args.
        // @param convertedArgs: The args list returned by ConvertStringArgsToNative.
        private void ReleaseConvertedStringArgs(object[] originalArgs, object[] convertedArgs)
        {
            for (int i = 0; i < originalArgs.Length; i++)
            {
                if (originalArgs[i] is string)
                {
                    (convertedArgs[i] as AndroidJavaObject).Dispose();
                }
            }
        }

        private void CallNativeVoidMethod(string methodName, string methodSig, object[] args)
        {
            if (!CanRunJNI())
            {
                return;
            }
            bool isAttached = bsg_unity_isJNIAttached();
            if (!isAttached)
            {
                AndroidJNI.AttachCurrentThread();
            }

            object[] convertedArgs = ConvertStringArgsToNative(args);
            jvalue[] jargs = AndroidJNIHelper.CreateJNIArgArray(convertedArgs);
            IntPtr methodID = AndroidJNI.GetStaticMethodID(BugsnagNativeInterface, methodName, methodSig);
            AndroidJNI.CallStaticVoidMethod(BugsnagNativeInterface, methodID, jargs);
            AndroidJNIHelper.DeleteJNIArgArray(convertedArgs, jargs);
            ReleaseConvertedStringArgs(args, convertedArgs);

            if (!isAttached)
            {
                AndroidJNI.DetachCurrentThread();
            }
        }

        private IntPtr CallNativeObjectMethodRef(string methodName, string methodSig, object[] args)
        {
            if (!CanRunJNI())
            {
                return IntPtr.Zero;
            }
            bool isAttached = bsg_unity_isJNIAttached();
            if (!isAttached)
            {
                AndroidJNI.AttachCurrentThread();
            }

            object[] convertedArgs = ConvertStringArgsToNative(args);
            jvalue[] jargs = AndroidJNIHelper.CreateJNIArgArray(convertedArgs);
            IntPtr methodID = AndroidJNI.GetStaticMethodID(BugsnagNativeInterface, methodName, methodSig);
            IntPtr nativeValue = AndroidJNI.CallStaticObjectMethod(BugsnagNativeInterface, methodID, jargs);
            AndroidJNIHelper.DeleteJNIArgArray(args, jargs);
            ReleaseConvertedStringArgs(args, convertedArgs);

            if (!isAttached)
            {
                AndroidJNI.DetachCurrentThread();
            }
            return nativeValue;
        }

        private IntPtr ConvertToStringJNIArrayRef(string[] items)
        {
            if (items == null || items.Length == 0)
            {
                return IntPtr.Zero;
            }

            AndroidJavaObject[] itemsAsJavaObjects = new AndroidJavaObject[items.Length];
            for (int i = 0; i < items.Length; i++)
            {
                itemsAsJavaObjects[i] = BuildJavaStringDisposable(items[i]);
            }

            AndroidJavaObject first = itemsAsJavaObjects[0];
            IntPtr rawArray = AndroidJNI.NewObjectArray(items.Length, StringClass, first.GetRawObject());
            first.Dispose();

            for (int i = 1; i < items.Length; i++)
            {
                AndroidJNI.SetObjectArrayElement(rawArray, i, itemsAsJavaObjects[i].GetRawObject());
                itemsAsJavaObjects[i].Dispose();
            }

            return rawArray;
        }

        private string CallNativeStringMethod(string methodName, string methodSig, object[] args)
        {
            if (!CanRunJNI())
            {
                return "";
            }
            bool isAttached = bsg_unity_isJNIAttached();
            if (!isAttached)
            {
                AndroidJNI.AttachCurrentThread();
            }

            object[] convertedArgs = ConvertStringArgsToNative(args);
            jvalue[] jargs = AndroidJNIHelper.CreateJNIArgArray(convertedArgs);
            IntPtr methodID = AndroidJNI.GetStaticMethodID(BugsnagNativeInterface, methodName, methodSig);
            IntPtr nativeValue = AndroidJNI.CallStaticObjectMethod(BugsnagNativeInterface, methodID, jargs);
            AndroidJNIHelper.DeleteJNIArgArray(args, jargs);
            ReleaseConvertedStringArgs(args, convertedArgs);

            string value = null;
            if (nativeValue != null && nativeValue != IntPtr.Zero)
            {
                value = AndroidJNI.GetStringUTFChars(nativeValue);
            }
            AndroidJNI.DeleteLocalRef(nativeValue);

            if (!isAttached)
            {
                AndroidJNI.DetachCurrentThread();
            }
            return value;
        }

        private bool CallNativeBoolMethod(string methodName, string methodSig, object[] args)
        {
            if (!CanRunJNI())
            {
                return false;
            }
            bool isAttached = bsg_unity_isJNIAttached();
            if (!isAttached)
            {
                AndroidJNI.AttachCurrentThread();
            }

            object[] convertedArgs = ConvertStringArgsToNative(args);
            jvalue[] jargs = AndroidJNIHelper.CreateJNIArgArray(convertedArgs);
            IntPtr methodID = AndroidJNI.GetStaticMethodID(BugsnagNativeInterface, methodName, methodSig);
            bool nativeValue = AndroidJNI.CallStaticBooleanMethod(BugsnagNativeInterface, methodID, jargs);
            AndroidJNIHelper.DeleteJNIArgArray(args, jargs);
            ReleaseConvertedStringArgs(args, convertedArgs);
            if (!isAttached)
            {
                AndroidJNI.DetachCurrentThread();
            }
            return nativeValue;
        }

        [DllImport("bugsnag-unity")]
        private static extern bool bsg_unity_isJNIAttached();

        private Breadcrumb ConvertToBreadcrumb(IntPtr javaBreadcrumb)
        {
            var metadata = new Dictionary<string, object>();

            IntPtr javaMetadata = AndroidJNI.CallObjectMethod(javaBreadcrumb, BreadcrumbGetMetadata, new jvalue[] { });
            IntPtr entries = AndroidJNI.CallObjectMethod(javaMetadata, MapEntrySet, new jvalue[] { });
            AndroidJNI.DeleteLocalRef(javaMetadata);

            IntPtr iterator = AndroidJNI.CallObjectMethod(entries, CollectionIterator, new jvalue[] { });
            AndroidJNI.DeleteLocalRef(entries);

            while (AndroidJNI.CallBooleanMethod(iterator, IteratorHasNext, new jvalue[] { }))
            {
                IntPtr entry = AndroidJNI.CallObjectMethod(iterator, IteratorNext, new jvalue[] { });
                IntPtr key = AndroidJNI.CallObjectMethod(entry, MapEntryGetKey, new jvalue[] { });
                IntPtr value = AndroidJNI.CallObjectMethod(entry, MapEntryGetValue, new jvalue[] { });
                AndroidJNI.DeleteLocalRef(entry);

                if (key != IntPtr.Zero && value != IntPtr.Zero)
                {
                    var obj = AndroidJNI.CallStringMethod(value, ObjectToString, new jvalue[] { });
                    metadata.Add(AndroidJNI.GetStringUTFChars(key), obj);
                }
                AndroidJNI.DeleteLocalRef(key);
                AndroidJNI.DeleteLocalRef(value);
            }
            AndroidJNI.DeleteLocalRef(iterator);

            IntPtr type = AndroidJNI.CallObjectMethod(javaBreadcrumb, BreadcrumbGetType, new jvalue[] { });
            string typeName = AndroidJNI.CallStringMethod(type, ObjectToString, new jvalue[] { });
            AndroidJNI.DeleteLocalRef(type);

            string message = "<empty>";
            IntPtr messageObj = AndroidJNI.CallObjectMethod(javaBreadcrumb, BreadcrumbGetMessage, new jvalue[] { });
            if (messageObj != IntPtr.Zero)
            {
                message = AndroidJNI.GetStringUTFChars(messageObj);
            }
            AndroidJNI.DeleteLocalRef(messageObj);

            string timestamp = AndroidJNI.CallStringMethod(javaBreadcrumb, BreadcrumbGetTimestamp, new jvalue[] { });
            return new Breadcrumb(message, timestamp, typeName, metadata);
        }

        internal static AndroidJavaObject BuildJavaMapDisposable(IDictionary<string, object> src)
        {
            AndroidJavaObject map = new AndroidJavaObject("java.util.HashMap");
            if (src != null)
            {
                foreach (var entry in src)
                {
                    using (AndroidJavaObject key = BuildJavaStringDisposable(entry.Key))
                    using (AndroidJavaObject value = BuildJavaStringDisposable(entry.Value.ToString()))
                    {
                        map.Call<AndroidJavaObject>("put", key, value);
                    }
                }
            }
            return map;
        }

        internal static AndroidJavaObject BuildJavaStringDisposable(string input)
        {
            if (input == null)
            {
                return null;
            }

            try
            {
                // The default encoding on Android is UTF-8
                using (AndroidJavaClass CharsetClass = new AndroidJavaClass("java.nio.charset.Charset"))
                using (AndroidJavaObject Charset = CharsetClass.CallStatic<AndroidJavaObject>("defaultCharset"))
                {
                    byte[] Bytes = Encoding.UTF8.GetBytes(input);

                    if (Unity2019OrNewer)
                    { // should succeed on Unity 2019.1 and above
                        sbyte[] SBytes = new sbyte[Bytes.Length];
                        Buffer.BlockCopy(Bytes, 0, SBytes, 0, Bytes.Length);
                        return new AndroidJavaObject("java.lang.String", SBytes, Charset);
                    }
                    else
                    { // use legacy API on older versions
                        return new AndroidJavaObject("java.lang.String", Bytes, Charset);
                    }
                }
            }
            catch (EncoderFallbackException _)
            {
                // The input string could not be encoded as UTF-8
                return new AndroidJavaObject("java.lang.String");
            }
        }

        internal static bool IsUnity2019OrNewer()
        {
            using (AndroidJavaClass CharsetClass = new AndroidJavaClass("java.nio.charset.Charset"))
            using (AndroidJavaObject Charset = CharsetClass.CallStatic<AndroidJavaObject>("defaultCharset"))
            {
                try
                { // should succeed on Unity 2019.1 and above
                    using (AndroidJavaObject obj = new AndroidJavaObject("java.lang.String", new sbyte[0], Charset))
                    {
                        return true;
                    }
                }
                catch (System.Exception _)
                { // use legacy API on older versions
                    return false;
                }
            }
        }

        private bool CanRunJNI()
        {
            return CanRunOnBackgroundThread || object.ReferenceEquals(Thread.CurrentThread, MainThread);
        }

        private Dictionary<string, object> DictionaryFromJavaMap(IntPtr source)
        {
            var dict = new Dictionary<string, object>();

            IntPtr entries = AndroidJNI.CallObjectMethod(source, MapEntrySet, new jvalue[] { });
            IntPtr iterator = AndroidJNI.CallObjectMethod(entries, CollectionIterator, new jvalue[] { });
            AndroidJNI.DeleteLocalRef(entries);

            while (AndroidJNI.CallBooleanMethod(iterator, IteratorHasNext, new jvalue[] { }))
            {
                IntPtr entry = AndroidJNI.CallObjectMethod(iterator, IteratorNext, new jvalue[] { });
                string key = AndroidJNI.CallStringMethod(entry, MapEntryGetKey, new jvalue[] { });
                IntPtr value = AndroidJNI.CallObjectMethod(entry, MapEntryGetValue, new jvalue[] { });
                AndroidJNI.DeleteLocalRef(entry);

                if (value != null && value != IntPtr.Zero)
                {
                    IntPtr valueClass = AndroidJNI.CallObjectMethod(value, ObjectGetClass, new jvalue[] { });
                    if (AndroidJNI.CallBooleanMethod(valueClass, ClassIsArray, new jvalue[] { }))
                    {
                        string[] values = AndroidJNIHelper.ConvertFromJNIArray<string[]>(value);
                        dict.AddToPayload(key, values);
                    }
                    else if (AndroidJNI.IsInstanceOf(value, MapClass))
                    {
                        dict.AddToPayload(key, DictionaryFromJavaMap(value));
                    }
                    else if (AndroidJNI.IsInstanceOf(value, DateClass))
                    {
                        jvalue[] args = new jvalue[1];
                        args[0].l = value;
                        var time = AndroidJNI.CallStaticStringMethod(DateUtilsClass, ToIso8601, args);
                        dict.AddToPayload(key, time);
                    }
                    else
                    {
                        // FUTURE(dm): check if Integer, Long, Double, or Float before calling toString
                        dict.AddToPayload(key, AndroidJNI.CallStringMethod(value, ObjectToString, new jvalue[] { }));
                    }
                    AndroidJNI.DeleteLocalRef(valueClass);
                }
                AndroidJNI.DeleteLocalRef(value);
            }
            AndroidJNI.DeleteLocalRef(iterator);

            return dict;
        }

        public LastRunInfo GetlastRunInfo()
        {
            var javaLastRunInfo = CallNativeObjectMethodRef("getLastRunInfo", "()Lcom/bugsnag/android/LastRunInfo;", new object[] { });
            var crashed = AndroidJNI.GetBooleanField(javaLastRunInfo, AndroidJNIHelper.GetFieldID(LastRunInfoClass, "crashed"));
            var consecutiveLaunchCrashes = AndroidJNI.GetIntField(javaLastRunInfo, AndroidJNIHelper.GetFieldID(LastRunInfoClass, "consecutiveLaunchCrashes"));
            var crashedDuringLaunch = AndroidJNI.GetBooleanField(javaLastRunInfo, AndroidJNIHelper.GetFieldID(LastRunInfoClass, "crashedDuringLaunch"));
            var lastRunInfo = new LastRunInfo
            {
                ConsecutiveLaunchCrashes = consecutiveLaunchCrashes,
                Crashed = crashed,
                CrashedDuringLaunch = crashedDuringLaunch
            };
            return lastRunInfo;
        }

        private IntPtr GetClientRef()
        {
            return CallNativeObjectMethodRef("getClient", "()Lcom/bugsnag/android/Client;", new object[] { });
        }

        public void AddFeatureFlag(string name, string variant)
        {
            object[] args = new object[] { name, variant };
            jvalue[] jargs = AndroidJNIHelper.CreateJNIArgArray(args);
            AndroidJNI.CallVoidMethod(GetClientRef(), AddFeatureFlagMethod, jargs);
        }

        public void ClearFeatureFlag(string name)
        {
            object[] args = new object[] { name };
            jvalue[] jargs = AndroidJNIHelper.CreateJNIArgArray(args);
            AndroidJNI.CallVoidMethod(GetClientRef(), ClearFeatureFlagMethod, jargs);
        }

        public void ClearFeatureFlags()
        {
            AndroidJNI.CallVoidMethod(GetClientRef(), ClearFeatureFlagsMethod, null);
        }

    }
}
