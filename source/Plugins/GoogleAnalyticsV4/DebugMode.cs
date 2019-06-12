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

public enum DebugMode
{
	ERROR = 0,
	WARNING = 1,
	INFO = 2,
	VERBOSE = 3
};

public static class DebugModeExtensions
{
	public static bool IsBelow(this DebugMode mode, DebugMode modeToCompare)
	{
		return (int)mode >= (int)modeToCompare;
	}
}

