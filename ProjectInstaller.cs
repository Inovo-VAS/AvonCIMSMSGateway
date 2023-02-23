// Decompiled with JetBrains decompiler
// Type: CIMSMSGateway.ProjectInstaller
// Assembly: CIMSMSGateway, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: CEEB88B5-4E13-4F96-8B61-FA72482A0091
// Assembly location: D:\projects\inovo\clients\Avon\CIMSMSGateway.exe

using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

namespace CIMSMSGateway
{
  [RunInstaller(true)]
  public class ProjectInstaller : Installer
  {
    private IContainer components = (IContainer) null;
    private ServiceProcessInstaller serviceProcessInstaller1;
    private ServiceInstaller serviceInstaller1;

    public ProjectInstaller() => this.InitializeComponent();

    protected override void Dispose(bool disposing)
    {
      if (disposing && this.components != null)
        this.components.Dispose();
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      this.serviceProcessInstaller1 = new ServiceProcessInstaller();
      this.serviceInstaller1 = new ServiceInstaller();
      this.serviceProcessInstaller1.Account = ServiceAccount.LocalSystem;
      this.serviceProcessInstaller1.Password = (string) null;
      this.serviceProcessInstaller1.Username = (string) null;
      this.serviceInstaller1.Description = "CIM Messaging Gateway";
      this.serviceInstaller1.DisplayName = "CIMMessagingGateway";
      this.serviceInstaller1.ServiceName = "CIMMessagingGateway";
      this.Installers.AddRange(new Installer[2]
      {
        (Installer) this.serviceProcessInstaller1,
        (Installer) this.serviceInstaller1
      });
    }
  }
}
