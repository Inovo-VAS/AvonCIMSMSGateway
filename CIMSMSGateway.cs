// Decompiled with JetBrains decompiler
// Type: CIMSMSGateway.CIMSMSGateway
// Assembly: CIMSMSGateway, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: CEEB88B5-4E13-4F96-8B61-FA72482A0091
// Assembly location: D:\projects\inovo\clients\Avon\CIMSMSGateway.exe

using log4net;
using SMSGatewayAPI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.ServiceProcess;

namespace CIMSMSGateway
{
  public class CIMSMSGateway : ServiceBase
  {
    private static SMSGateway SMSGateway = (SMSGateway) null;
    private static CIMDB CIMDB = (CIMDB) null;
    private static DateTime startSendingStamp = DateTime.Now.AddSeconds(10.0);
    private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    private IContainer components = (IContainer) null;

    public CIMSMSGateway() => this.InitializeComponent();

    protected override void OnStart(string[] args) => CIMSMSGateway.StartupCIMSMSGateway();

    protected override void OnStop() => CIMSMSGateway.StopCIMSMSGateway();

    public static void StartupCIMSMSGateway()
    {
      if (CIMSMSGateway.CIMDB != null)
        return;
      CIMSMSGateway.CIMDB = new CIMDB("Server=134.65.204.106\\PRESENCE;Initial Catalog=CIMV33;User Id=InovoCIMUsr;Password=InovoCIMUsr");
      CIMSMSGateway.CIMDB.OnLogging = new Logging(CIMSMSGateway.CIMDBLog);
      CIMSMSGateway.CIMDB.ConnectGateway(new ConnectingGateway(CIMSMSGateway.ConnectGateway));
    }

    private static void ConnectGateway(string provider, params string[] settings)
    {
      if (CIMSMSGateway.SMSGateway == null)
      {
        CIMSMSGateway.SMSGateway = new SMSGateway(provider.ToUpper(), settings);
        CIMSMSGateway.SMSGateway.ResponseApps = new string[1]
        {
          "CIM"
        };
        CIMSMSGateway.SMSGateway.OnStartSendingSMSS = new StartSendingSMSs(CIMSMSGateway.StartSendingSMSS);
        CIMSMSGateway.SMSGateway.OnSMSsSent = new SentSMSs(CIMSMSGateway.SentSMSS);
        CIMSMSGateway.SMSGateway.EnableSending = true;
        CIMSMSGateway.SMSGateway.EnableReceiving = true;
        CIMSMSGateway.SMSGateway.OnReceivedMultipleSMSS = new ReceivedMultipleSMSS(CIMSMSGateway.ReceivedSMSS);
        CIMSMSGateway.SMSGateway.OnLogging = new SMSGatewayAPI.Logging(CIMSMSGateway.SMSGatewayLog);
      }
      CIMSMSGateway.SMSGateway.StartGateway();
    }

    private static void SMSGatewayLog(string level, string section, string message, Exception e) => CIMSMSGateway.Log("SMSGateway", level, section, message, e);

    private static void CIMDBLog(string level, string section, string message, Exception e) => CIMSMSGateway.Log("CIMDB", level, section, message, e);

    private static void ReceivedSMSS(SMSGateway gateway, List<SMSReceived> sMSReceiveds)
    {
      if (CIMSMSGateway.CIMDB == null)
        return;
      CIMSMSGateway.CIMDB.ReceivedSMSS(gateway.Provider, sMSReceiveds);
    }

    private static void SentSMSS(
      SMSGateway gateway,
      long totalprocessed,
      long totalsent,
      long totalfailed,
      long totalstopped,
      List<SMSRequest> smsssent,
      List<SMSRequest> smssfaild,
      List<SMSRequest> smssstopped,
      List<string> smserrors)
    {
      if (CIMSMSGateway.CIMDB == null)
        return;
      CIMSMSGateway.CIMDB.SentSMSS(totalprocessed, totalsent, totalfailed, totalstopped, smsssent, smssfaild, smssstopped);
    }

    private static void StartSendingSMSS(SMSGateway smsGateway)
    {
      if (CIMSMSGateway.CIMDB == null || !(CIMSMSGateway.startSendingStamp < DateTime.Now))
        return;
      CIMSMSGateway.CIMDB.SendSMSS(smsGateway.Provider, (SendingSMS) ((number, message, refnumber, smsapp, stop) =>
      {
        if (CIMSMSGateway.SMSGateway == null)
          return;
        CIMSMSGateway.SMSGateway.SendMessage(number, message, refnumber, smsapp, stop);
      }));
      CIMSMSGateway.startSendingStamp = DateTime.Now.AddSeconds(10.0);
    }

    public static void StopCIMSMSGateway()
    {
      if (CIMSMSGateway.SMSGateway != null)
      {
        CIMSMSGateway.SMSGateway.StopGateway();
        CIMSMSGateway.SMSGateway = (SMSGateway) null;
      }
      if (CIMSMSGateway.CIMDB == null)
        return;
      CIMSMSGateway.CIMDB.Close();
      CIMSMSGateway.CIMDB = (CIMDB) null;
    }

    public static void Log(
      string component,
      string level,
      string section,
      string message,
      Exception e)
    {
      if ((level = level.ToUpper()).Equals("ERROR") || e != null)
      {
        if (e == null)
          CIMSMSGateway.log.Fatal((object) ("[" + component + "]" + section + " - " + message), e);
        else
          CIMSMSGateway.log.Error((object) ("[" + component + "]" + section + " - " + message));
      }
      else if (level.Equals("INFO"))
        CIMSMSGateway.log.Info((object) ("[" + component + "]" + section + " - " + message));
      else if (level.Equals("WARNING"))
        CIMSMSGateway.log.Warn((object) ("[" + component + "]" + section + " - " + message));
      else
        CIMSMSGateway.log.Debug((object) ("[" + component + "]" + section + " - " + message));
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && this.components != null)
        this.components.Dispose();
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      this.CanShutdown = true;
      this.ServiceName = "CIMMessagingGateway";
    }
  }
}
