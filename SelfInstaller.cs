// Decompiled with JetBrains decompiler
// Type: CIMSMSGateway.SelfInstaller
// Assembly: CIMSMSGateway, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: CEEB88B5-4E13-4F96-8B61-FA72482A0091
// Assembly location: D:\projects\inovo\clients\Avon\CIMSMSGateway.exe

using System;
using System.Configuration.Install;
using System.Reflection;

namespace CIMSMSGateway
{
  public static class SelfInstaller
  {
    private static readonly string _exePath = Assembly.GetExecutingAssembly().Location;

    public static bool InstallMe()
    {
      try
      {
        ManagedInstallerClass.InstallHelper(new string[1]
        {
          SelfInstaller._exePath
        });
      }
      catch (Exception ex)
      {
        return false;
      }
      return true;
    }

    public static bool UninstallMe()
    {
      try
      {
        ManagedInstallerClass.InstallHelper(new string[2]
        {
          "/u",
          SelfInstaller._exePath
        });
      }
      catch
      {
        return false;
      }
      return true;
    }
  }
}
