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

	[Tooltip("The tracking code to be used for platforms other than Android and iOS. Example value: UA-XXXX-Y.")]
	public string trackingCode;

	//[Range(0, 3600),
	//	Tooltip("The dispatch period in seconds. Only required for Android and iOS.")]
	//public int dispatchPeriod = 5;

	//[Range(0, 100),
	//	Tooltip("The sample rate to use. Only required for Android and iOS.")]
	//public int sampleFrequency = 100;

	[Tooltip("The log level. Default is WARNING."), SerializeField]
	private DebugMode logLevel = DebugMode.WARNING;

	[Tooltip("If checked, the IP address of the sender will be anonymized.")]
	public bool AnonymizeIP = false;

	[Tooltip("Automatically report uncaught exceptions.")]
	public bool UncaughtExceptionReporting = false;

	[Tooltip("Automatically send a launch event when the game starts up.")]
	public bool SendStartSessionEvent = false;

	[Tooltip("If checked, hits will not be dispatched. Use for testing.")]
	public bool DryRun = false;

	// TODO: add to mp
	//[Tooltip("The amount of time in seconds your application can stay in" +
	//	 "the background before the session is ended. Default is 30 minutes" +
	//	 " (1800 seconds). A value of -1 will disable session management.")]
	//public int sessionTimeout = 1800;


	public static GoogleAnalyticsV4 instance = null;

	private GoogleAnalyticsMPV3 mpTracker = new GoogleAnalyticsMPV3();

	void Awake()
	{
		InitializeTracker();
		if (SendStartSessionEvent)
			StartSession();

		if (UncaughtExceptionReporting)
		{
			Application.logMessageReceived += HandleException;
			if (IsLogLevelEnough(DebugMode.VERBOSE))
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

			Debug.Log("Initializing Google Analytics 0.2.");
			mpTracker.SetTrackingCode(trackingCode);
			mpTracker.SetBundleIdentifier(Application.identifier);
			mpTracker.SetAppName(Application.productName);
			mpTracker.SetAppVersion(Application.version);
			mpTracker.SetLogLevelValue(logLevel);
			mpTracker.SetAnonymizeIP(AnonymizeIP);
			mpTracker.SetDryRun(DryRun);
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
		LogEvent("Google Analytics", "Auto Instrumentation", "Session started", 0);
	}

	private void OnDestroy()
	{
		StopSession();
	}

	public void StopSession()
	{
		InitializeTracker();
		mpTracker.StopSession();
		LogEvent("Google Analytics", "Auto Instrumentation", "Session ended", 0);
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

		if (IsLogLevelEnough(DebugMode.VERBOSE))
			Debug.Log("Logging screen.");

		mpTracker.LogScreen(builder);
	}

	public void LogPageView(string pageLocation, string pageTitle = "")
	{
		PageViewHitBuilder builder = new PageViewHitBuilder(pageLocation, pageTitle);
		LogPageView(builder);
	}

	public void LogPageView(PageViewHitBuilder builder)
	{
		InitializeTracker();
		if (builder.Validate() == null)
			return;

		if (IsLogLevelEnough(DebugMode.VERBOSE))
			Debug.Log("Logging pageview.");

		mpTracker.LogPageView(builder);
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

		if (IsLogLevelEnough(DebugMode.VERBOSE))
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

		if (IsLogLevelEnough(DebugMode.VERBOSE))
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

		if (IsLogLevelEnough(DebugMode.VERBOSE))
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

		if (IsLogLevelEnough(DebugMode.VERBOSE))
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

		if (IsLogLevelEnough(DebugMode.VERBOSE))
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

		if (IsLogLevelEnough(DebugMode.VERBOSE))
			Debug.Log("Logging timing.");

		mpTracker.LogTiming(builder);
	}

	public void Dispose()
	{
		initialized = false;
	}

	public static bool IsLogLevelEnough(DebugMode comparelogLevel)
	{
		return instance.logLevel.IsBelow(comparelogLevel);
	}
}
