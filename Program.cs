// Decompiled with JetBrains decompiler
// Type: CIMSMSGateway.Program
// Assembly: CIMSMSGateway, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: CEEB88B5-4E13-4F96-8B61-FA72482A0091
// Assembly location: D:\projects\inovo\clients\Avon\CIMSMSGateway.exe

using System.ServiceProcess;

namespace CIMSMSGateway
{
  internal static class Program
  {
    public static void Main(string[] args)
    {
      if (args != null && args.Length == 1 && args[0].Length > 1 && (args[0][0] == '-' || args[0][0] == '/'))
      {
        string lower = args[0].Substring(1).ToLower();
        if (!(lower == "install") && !(lower == "i"))
        {
          if (!(lower == "uninstall") && !(lower == "u"))
          {
            if (!(lower == "console") && !(lower == "c"))
              return;
            CIMSMSGateway.StartupCIMSMSGateway();
          }
          else
            SelfInstaller.UninstallMe();
        }
        else
          SelfInstaller.InstallMe();
      }
      else
        ServiceBase.Run(new ServiceBase[1]
        {
          (ServiceBase) new CIMSMSGateway()
        });
    }
  }
}
