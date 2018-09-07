﻿using UnityEngine;
using System.Collections.Generic;

namespace BugsnagUnity
{
  class UnityMetadata
  {
    internal static Dictionary<string, string> Data => new Dictionary<string, string> {
      { "unityVersion", Application.unityVersion },
      { "unityException", "false" }, // this is overridden when we make a notify call from the c# layer
      { "platform", Application.platform.ToString() },
      { "osLanguage", Application.systemLanguage.ToString() },
      { "version", Application.version },
      { "companyName", Application.companyName },
      { "productName", Application.productName },
    };
  }
}
