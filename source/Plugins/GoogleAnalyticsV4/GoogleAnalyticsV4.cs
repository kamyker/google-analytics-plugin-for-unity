/*
  Copyright 2014 Google Inc. All rights reserved.

  Licensed under the Apache License, Version 2.0 (the "License");
  you may not use this file except in compliance with the License.
  You may obtain a copy of the License at

      http://www.apache.org/licenses/LICENSE-2.0

  Unless required by applicable law or agreed to in writing, software
  distributed under the License is distributed on an "AS IS" BASIS,
  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
  See the License for the specific language governing permissions and
  limitations under the License.
*/

using UnityEngine;

/*
  GoogleAnalyticsV4 is an interface for developers to send hits to Google
  Analytics.
  The class will delegate the hits to the appropriate helper class depending on
  the platform being built for - Android, iOS or Measurement Protocol for all
  others.

  Each method has a simple form with the hit parameters, or developers can
  pass a builder to the same method name in order to add custom metrics or
  custom dimensions to the hit.
*/
public class GoogleAnalyticsV4 : MonoBehaviour
{
	private string uncaughtExceptionStackTrace = null;
	private bool initialized = false;

	public enum DebugMode
	{
		ERROR,
		WARNING,
		INFO,
		VERBOSE
	};

	[Tooltip("The tracking code to be used for platforms other than Android and iOS. Example value: UA-XXXX-Y.")]
	public string trackingCode;

	[HideInInspector]
	[Tooltip("The application name. This value should be modified in the " +
		 "Unity Player Settings.")]
	public string productName;

	[HideInInspector]
	[Tooltip("The application identifier. Example value: com.company.app.")]
	public string bundleIdentifier;

	[HideInInspector]
	[Tooltip("The application version. Example value: 1.2")]
	public string bundleVersion;

	[Range(0, 3600),
		Tooltip("The dispatch period in seconds. Only required for Android and iOS.")]
	public int dispatchPeriod = 5;

	[Range(0, 100),
		Tooltip("The sample rate to use. Only required for Android and iOS.")]
	public int sampleFrequency = 100;

	[Tooltip("The log level. Default is WARNING.")]
	public DebugMode logLevel = DebugMode.WARNING;

	[Tooltip("If checked, the IP address of the sender will be anonymized.")]
	public bool anonymizeIP = false;

	[Tooltip("Automatically report uncaught exceptions.")]
	public bool UncaughtExceptionReporting = false;

	[Tooltip("Automatically send a launch event when the game starts up.")]
	public bool sendLaunchEvent = false;

	[Tooltip("If checked, hits will not be dispatched. Use for testing.")]
	public bool dryRun = false;

	// TODO: Create conditional textbox attribute
	[Tooltip("The amount of time in seconds your application can stay in" +
		 "the background before the session is ended. Default is 30 minutes" +
		 " (1800 seconds). A value of -1 will disable session management.")]
	public int sessionTimeout = 1800;

	[Tooltip("If you enable this collection, ensure that you review and adhere to the Google Analytics " +
		 "policies for SDKs and advertising features. Click the button below to view them in your browser." +
		 " https://support.google.com/analytics/answer/2700409")]
	public bool enableAdId = false;

	public static GoogleAnalyticsV4 instance = null;

	[HideInInspector]
	public readonly static string currencySymbol = "USD";
	public readonly static string EVENT_HIT = "createEvent";
	public readonly static string APP_VIEW = "createAppView";
	public readonly static string SET = "set";
	public readonly static string SET_ALL = "setAll";
	public readonly static string SEND = "send";
	public readonly static string ITEM_HIT = "createItem";
	public readonly static string TRANSACTION_HIT = "createTransaction";
	public readonly static string SOCIAL_HIT = "createSocial";
	public readonly static string TIMING_HIT = "createTiming";
	public readonly static string EXCEPTION_HIT = "createException";

	private GoogleAnalyticsMPV3 mpTracker = new GoogleAnalyticsMPV3();

	void Awake()
	{
		InitializeTracker();
		if (sendLaunchEvent)
			LogEvent("Google Analytics", "Auto Instrumentation", "Game Launch", 0);

		if (UncaughtExceptionReporting)
		{
			Application.logMessageReceived += HandleException;
			if (belowThreshold(logLevel, DebugMode.VERBOSE))
			{
				Debug.Log("Enabling uncaught exception reporting.");
			}
		}
	}

	void Update()
	{
		if (!string.IsNullOrEmpty(uncaughtExceptionStackTrace))
		{
			LogException(uncaughtExceptionStackTrace, true);
			uncaughtExceptionStackTrace = null;
		}
	}

	private void HandleException(string condition, string stackTrace, LogType type)
	{
		if (type == LogType.Exception)
		{
			uncaughtExceptionStackTrace = condition + "\n" + stackTrace
				 + StackTraceUtility.ExtractStackTrace();
		}
	}

	// TODO: Error checking on initialization parameters
	private void InitializeTracker()
	{
		if (!initialized)
		{
			instance = this;

			DontDestroyOnLoad(instance);

			// automatically set app parameters from player settings if they are left empty
			if (string.IsNullOrEmpty(productName))
				productName = Application.productName;

			if (string.IsNullOrEmpty(bundleIdentifier))
				bundleIdentifier = Application.identifier;

			if (string.IsNullOrEmpty(bundleVersion))
				bundleVersion = Application.version;

			Debug.Log("Initializing Google Analytics 0.2.");
			mpTracker.SetTrackingCode(trackingCode);
			mpTracker.SetBundleIdentifier(bundleIdentifier);
			mpTracker.SetAppName(productName);
			mpTracker.SetAppVersion(bundleVersion);
			mpTracker.SetLogLevelValue(logLevel);
			mpTracker.SetAnonymizeIP(anonymizeIP);
			mpTracker.SetDryRun(dryRun);
			mpTracker.InitializeTracker();

			initialized = true;
			SetOnTracker(Fields.DEVELOPER_ID, "GbOCSs");
		}
	}

	public void SetAppLevelOptOut(bool optOut)
	{
		InitializeTracker();
		mpTracker.SetOptOut(optOut);
	}

	public void SetUserIDOverride(string userID)
	{
		SetOnTracker(Fields.USER_ID, userID);
	}

	public void ClearUserIDOverride()
	{
		InitializeTracker();
		mpTracker.ClearUserIDOverride();
	}

	public void StartSession()
	{
		InitializeTracker();
		mpTracker.StartSession();
	}

	public void StopSession()
	{
		InitializeTracker();
		mpTracker.StopSession();
	}

	// Use values from Fields for the fieldName parameter ie. Fields.SCREEN_NAME
	public void SetOnTracker(Field fieldName, object value)
	{
		InitializeTracker();
		mpTracker.SetTrackerVal(fieldName, value);
	}

	public void LogScreen(string title)
	{
		AppViewHitBuilder builder = new AppViewHitBuilder().SetScreenName(title);
		LogScreen(builder);
	}

	public void LogScreen(AppViewHitBuilder builder)
	{
		InitializeTracker();
		if (builder.Validate() == null)
			return;

		if (belowThreshold(logLevel, DebugMode.VERBOSE))
			Debug.Log("Logging screen.");

		mpTracker.LogScreen(builder);
	}

	public void LogEvent(string eventCategory, string eventAction,
		 string eventLabel, long value)
	{
		EventHitBuilder builder = new EventHitBuilder()
			 .SetEventCategory(eventCategory)
			 .SetEventAction(eventAction)
			 .SetEventLabel(eventLabel)
			 .SetEventValue(value);

		LogEvent(builder);
	}

	public void LogEvent(EventHitBuilder builder)
	{
		InitializeTracker();

		if (builder.Validate() == null)
			return;

		if (belowThreshold(logLevel, DebugMode.VERBOSE))
			Debug.Log("Logging event.");

		mpTracker.LogEvent(builder);
	}

	public void LogTransaction(string transID, string affiliation,
		 double revenue, double tax, double shipping)
	{
		LogTransaction(transID, affiliation, revenue, tax, shipping, "");
	}

	public void LogTransaction(string transID, string affiliation,
		 double revenue, double tax, double shipping, string currencyCode)
	{
		TransactionHitBuilder builder = new TransactionHitBuilder()
			 .SetTransactionID(transID)
			 .SetAffiliation(affiliation)
			 .SetRevenue(revenue)
			 .SetTax(tax)
			 .SetShipping(shipping)
			 .SetCurrencyCode(currencyCode);

		LogTransaction(builder);
	}

	public void LogTransaction(TransactionHitBuilder builder)
	{
		InitializeTracker();
		if (builder.Validate() == null)
			return;

		if (belowThreshold(logLevel, DebugMode.VERBOSE))
			Debug.Log("Logging transaction.");

		mpTracker.LogTransaction(builder);
	}

	public void LogItem(string transID, string name, string sku,
		 string category, double price, long quantity)
	{
		LogItem(transID, name, sku, category, price, quantity, null);
	}

	public void LogItem(string transID, string name, string sku,
		 string category, double price, long quantity, string currencyCode)
	{
		ItemHitBuilder builder = new ItemHitBuilder()
			 .SetTransactionID(transID)
			 .SetName(name)
			 .SetSKU(sku)
			 .SetCategory(category)
			 .SetPrice(price)
			 .SetQuantity(quantity)
			 .SetCurrencyCode(currencyCode);

		LogItem(builder);
	}

	public void LogItem(ItemHitBuilder builder)
	{
		InitializeTracker();
		if (builder.Validate() == null)
			return;

		if (belowThreshold(logLevel, DebugMode.VERBOSE))
			Debug.Log("Logging item.");

		mpTracker.LogItem(builder);
	}

	public void LogException(string exceptionDescription, bool isFatal)
	{
		ExceptionHitBuilder builder = new ExceptionHitBuilder()
			 .SetExceptionDescription(exceptionDescription)
			 .SetFatal(isFatal);

		LogException(builder);
	}

	public void LogException(ExceptionHitBuilder builder)
	{
		InitializeTracker();
		if (builder.Validate() == null)
			return;

		if (belowThreshold(logLevel, DebugMode.VERBOSE))
			Debug.Log("Logging exception.");

		mpTracker.LogException(builder);
	}

	public void LogSocial(string socialNetwork, string socialAction,
		 string socialTarget)
	{
		SocialHitBuilder builder = new SocialHitBuilder()
			 .SetSocialNetwork(socialNetwork)
			 .SetSocialAction(socialAction)
			 .SetSocialTarget(socialTarget);

		LogSocial(builder);
	}

	public void LogSocial(SocialHitBuilder builder)
	{
		InitializeTracker();
		if (builder.Validate() == null)
			return;

		if (belowThreshold(logLevel, DebugMode.VERBOSE))
			Debug.Log("Logging social.");

		mpTracker.LogSocial(builder);
	}

	public void LogTiming(string timingCategory, long timingInterval,
		 string timingName, string timingLabel)
	{
		TimingHitBuilder builder = new TimingHitBuilder()
			 .SetTimingCategory(timingCategory)
			 .SetTimingInterval(timingInterval)
			 .SetTimingName(timingName)
			 .SetTimingLabel(timingLabel);

		LogTiming(builder);
	}

	public void LogTiming(TimingHitBuilder builder)
	{
		InitializeTracker();
		if (builder.Validate() == null)
			return;

		if (belowThreshold(logLevel, DebugMode.VERBOSE))
			Debug.Log("Logging timing.");

		mpTracker.LogTiming(builder);
	}

	public void Dispose()
	{
		initialized = false;
	}

	public static bool belowThreshold(DebugMode userLogLevel, DebugMode comparelogLevel)
	{
		if (comparelogLevel == userLogLevel)
		{
			return true;
		}
		else if (userLogLevel == GoogleAnalyticsV4.DebugMode.ERROR)
		{
			return false;
		}
		else if (userLogLevel == GoogleAnalyticsV4.DebugMode.VERBOSE)
		{
			return true;
		}
		else if (userLogLevel == GoogleAnalyticsV4.DebugMode.WARNING &&
		(comparelogLevel == GoogleAnalyticsV4.DebugMode.INFO ||
		comparelogLevel == GoogleAnalyticsV4.DebugMode.VERBOSE))
		{
			return false;
		}
		else if (userLogLevel == GoogleAnalyticsV4.DebugMode.INFO &&
		(comparelogLevel == GoogleAnalyticsV4.DebugMode.VERBOSE))
		{
			return false;
		}
		return true;
	}
}
