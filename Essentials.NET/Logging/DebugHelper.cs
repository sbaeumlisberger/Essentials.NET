#define DEBUG // preverse the Debug.Write call on release build

using System.Diagnostics;

namespace Essentials.NET.Logging;

internal static class DebugHelper
{
    public static void Write(string message)
    {
        Debug.Write(message);
    }
}