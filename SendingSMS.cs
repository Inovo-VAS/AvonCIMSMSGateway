// Decompiled with JetBrains decompiler
// Type: CIMSMSGateway.SendingSMS
// Assembly: CIMSMSGateway, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: CEEB88B5-4E13-4F96-8B61-FA72482A0091
// Assembly location: D:\projects\inovo\clients\Avon\CIMSMSGateway.exe

namespace CIMSMSGateway
{
  public delegate void SendingSMS(
    string number,
    string message,
    string refnumber,
    string smsapp,
    bool stop);
}
