// Decompiled with JetBrains decompiler
// Type: CIMSMSGateway.CIMDB
// Assembly: CIMSMSGateway, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: CEEB88B5-4E13-4F96-8B61-FA72482A0091
// Assembly location: D:\projects\inovo\clients\Avon\CIMSMSGateway.exe

using SMSGatewayAPI;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading;

namespace CIMSMSGateway
{
  internal class CIMDB
  {
    private SqlClientFactory sqlClientFactory = SqlClientFactory.Instance;
    private string connectionString = "";
    private static string sender = "CIM";
    private bool started = true;
    private Logging onLogging = (Logging) null;

    public string Sender => CIMDB.sender;

    public CIMDB(string connectionstring) => this.connectionString = connectionstring;

    public void SendSMSS(string provider, SendingSMS sendingSMS)
    {
      DbConnection dbConnection1 = this.NewConnection();
      DbConnection dbConnection2 = this.NewConnection();
      DbConnection dbConnection3;
      try
      {
        DbCommand command1 = dbConnection1.CreateCommand();
        DbCommand command2 = dbConnection2.CreateCommand();
        if (this.started)
        {
          this.started = false;
          command2.CommandText = "update dbo.AppSendOutSMS Set State='Ready' where State='Sending' ";
          command2.ExecuteNonQuery();
          this.Log("INFO", "SendSMSS()", "initiated reading sms's from db");
        }
        command1.CommandText = "select id,Message,CellphoneNumber,State,coalesce((select top 1 1 from dbo.AppOptOutSMS where AppOptOutSMS.CellphoneNumber=AppSendOutSMS.CellPhoneNumber),0) OptOut from (select TOP 500 * from dbo.AppSendOutSMS appsms where COALESCE((SELECT Top 1 1 FROM [CIMV33].[dbo].[ConfigSmsGateways] WHERE GETDATE() BETWEEN DATEADD(MINUTE,OkToSendTimeWindowStart,CAST(CAST(GETDATE() AS DATE) AS DATETIME)) AND DATEADD(MINUTE,OkToSendTimeWindowEnd,CAST(CAST(GETDATE() AS DATE) AS DATETIME))),0)=1 AND State='Ready') AppSendOutSMS where State='Ready' order by LoadTime";
        command2.CommandText = "EXECUTE COLLECTIONS.spProd_UpdateSMSSendState @MESSAGEID,@PROVIDER,@STATE,@STATEDESCRIPTION";
        DbDataReader dbDataReader = command1.ExecuteReader();
        object[] values = new object[dbDataReader.FieldCount];
        bool flag = false;
        long num = 0;
        while (Thread.CurrentThread.ThreadState == ThreadState.Running && dbDataReader.Read())
        {
          if (!flag)
          {
            flag = true;
            this.Log("INFO", "SendSMSS()", "start queuing sms's from db");
          }
          dbDataReader.GetValues(values);
          if (sendingSMS != null)
          {
            ++num;
            if (values[3] != null && values[3].ToString().Equals("Ready"))
            {
              DbParameterCollection parameters = command2.Parameters;
              if (parameters.Count > 0)
                parameters.Clear();
              DbParameter parameter1 = command2.CreateParameter();
              parameter1.ParameterName = "@MESSAGEID";
              parameter1.Value = (object) values[0].ToString();
              parameters.Add((object) parameter1);
              DbParameter parameter2 = command2.CreateParameter();
              parameter2.ParameterName = "@PROVIDER";
              parameter2.Value = (object) provider;
              parameters.Add((object) parameter2);
              DbParameter parameter3 = command2.CreateParameter();
              parameter3.ParameterName = "@STATE";
              parameter3.Value = values[4].ToString().Equals("1") ? (object) "OptOut" : (object) "Sending";
              parameters.Add((object) parameter3);
              DbParameter parameter4 = command2.CreateParameter();
              parameter4.ParameterName = "@STATEDESCRIPTION";
              parameter4.Value = values[4].ToString().Equals("1") ? (object) "OptOut" : (object) "Sending";
              parameters.Add((object) parameter4);
              command2.ExecuteNonQuery();
            }
            if ((values[4].ToString().Equals("1") ? "OptOut" : "Sending").Equals("Sending"))
              sendingSMS(values[2].ToString(), values[1].ToString(), values[0].ToString(), CIMDB.sender, values[4].ToString().Equals("1"));
          }
        }
        if (flag)
          this.Log("INFO", "SendSMSS()", "done queuing (" + (object) num + ") sms's from db");
        dbDataReader.Close();
        command1.Dispose();
        if (dbConnection2 != null)
        {
          dbConnection2.Close();
          dbConnection2.Dispose();
          dbConnection2 = (DbConnection) null;
        }
        if (dbConnection1 == null)
          return;
        dbConnection1.Close();
        dbConnection1.Dispose();
        dbConnection3 = (DbConnection) null;
      }
      catch (Exception ex)
      {
        if (dbConnection2 != null)
        {
          dbConnection2.Close();
          dbConnection2.Dispose();
        }
        if (dbConnection1 != null)
        {
          dbConnection1.Close();
          dbConnection1.Dispose();
          dbConnection3 = (DbConnection) null;
        }
        this.Log("ERROR", "SendSMSS()", "failed reading sms's from db:" + ex.Message, ex);
      }
    }

    internal void Close()
    {
    }

    internal void ReceivedSMSS(string provider, List<SMSReceived> sMSReceiveds)
    {
      if (sMSReceiveds == null || sMSReceiveds.Count <= 0)
        return;
      DbConnection dbConnection = (DbConnection) null;
      DbCommand dbCommand = (DbCommand) null;
      bool flag1 = false;
      bool flag2 = false;
      try
      {
        dbConnection = this.NewConnection();
        flag1 = true;
        try
        {
          dbCommand = dbConnection.CreateCommand();
          dbCommand.CommandText = "EXECUTE COLLECTIONS.spProd_CimReceiveSMS @CellphoneNumber,@Provider,@Message,@MessageID";
          flag2 = true;
        }
        catch
        {
        }
      }
      catch
      {
      }
      this.Log("INFO", "SendSMSS()", "start capturing sms replies (" + (object) sMSReceiveds.Count + ") to db");
      long num = 0;
      foreach (SMSReceived sMsReceived in sMSReceiveds)
      {
        ++num;
        DbParameterCollection parameters = dbCommand.Parameters;
        if (parameters.Count > 0)
          parameters.Clear();
        DbParameter parameter1 = dbCommand.CreateParameter();
        parameter1.ParameterName = "@MessageID";
        parameter1.Value = sMsReceived.RefNumber == null ? (object) "" : (object) sMsReceived.RefNumber;
        parameters.Add((object) parameter1);
        DbParameter parameter2 = dbCommand.CreateParameter();
        parameter2.ParameterName = "@Provider";
        parameter2.Value = (object) provider;
        parameters.Add((object) parameter2);
        DbParameter parameter3 = dbCommand.CreateParameter();
        parameter3.ParameterName = "@Message";
        parameter3.Value = sMsReceived.Message == null ? (object) "" : (object) sMsReceived.Message;
        parameters.Add((object) parameter3);
        DbParameter parameter4 = dbCommand.CreateParameter();
        parameter4.ParameterName = "@CellphoneNumber";
        parameter4.Value = (object) sMsReceived.Number;
        parameters.Add((object) parameter4);
        dbCommand.ExecuteNonQuery();
      }
      if (flag2)
      {
        if (dbCommand != null)
        {
          try
          {
            dbCommand.Dispose();
          }
          catch
          {
          }
        }
      }
      if (flag1)
      {
        if (dbConnection != null)
        {
          try
          {
            dbConnection.Close();
            dbConnection.Dispose();
          }
          catch
          {
          }
        }
      }
      this.Log("INFO", "SendSMSS()", "done capturing sms replies (" + (object) num + ") to db");
    }

    internal void SentSMSS(
      long totalprocessed,
      long totalsent,
      long totalfailed,
      long totalstopped,
      List<SMSRequest> smsssent,
      List<SMSRequest> smssfaild,
      List<SMSRequest> smssstopped)
    {
      if (totalprocessed <= 0L)
        return;
      DbConnection dbConnection = (DbConnection) null;
      DbCommand dbCommand = (DbCommand) null;
      bool flag1 = false;
      bool flag2 = false;
      try
      {
        dbConnection = this.NewConnection();
        flag1 = true;
        try
        {
          dbCommand = dbConnection.CreateCommand();
          dbCommand.CommandText = "EXECUTE COLLECTIONS.spProd_UpdateSMSSendState @MESSAGEID,@PROVIDER,@STATE,@STATEDESCRIPTION";
          flag2 = true;
        }
        catch
        {
        }
      }
      catch
      {
      }
      if (totalsent > 0L)
      {
        this.Log("INFO", "SentSMSS()", "start logging sms's (" + (object) totalsent + ") sent to db");
        long num = 0;
        foreach (SMSRequest smsRequest in smsssent)
        {
          ++num;
          if (flag1)
          {
            if (flag2)
            {
              try
              {
                DbParameterCollection parameters = dbCommand.Parameters;
                if (parameters.Count > 0)
                  parameters.Clear();
                DbParameter parameter1 = dbCommand.CreateParameter();
                parameter1.ParameterName = "@MESSAGEID";
                parameter1.Value = (object) smsRequest.RefNumber;
                parameters.Add((object) parameter1);
                DbParameter parameter2 = dbCommand.CreateParameter();
                parameter2.ParameterName = "@PROVIDER";
                parameter2.Value = (object) smsRequest.Provider;
                parameters.Add((object) parameter2);
                DbParameter parameter3 = dbCommand.CreateParameter();
                parameter3.ParameterName = "@STATE";
                parameter3.Value = (object) smsRequest.Status;
                parameters.Add((object) parameter3);
                DbParameter parameter4 = dbCommand.CreateParameter();
                parameter4.ParameterName = "@STATEDESCRIPTION";
                parameter4.Value = (object) smsRequest.StatusDescription;
                parameters.Add((object) parameter4);
                dbCommand.ExecuteNonQuery();
              }
              catch
              {
                if (dbCommand != null)
                {
                  try
                  {
                    dbCommand.Dispose();
                  }
                  catch
                  {
                  }
                  dbCommand = (DbCommand) null;
                }
                flag2 = false;
                break;
              }
            }
            else
              break;
          }
          else
            break;
        }
        this.Log("INFO", "SentSMSS()", "stop logging sms's (" + (object) num + ") sent to db");
      }
      if (totalfailed > 0L)
      {
        this.Log("INFO", "SentSMSS()", "start logging sms's (" + (object) totalfailed + ") failed to db");
        long num = 0;
        foreach (SMSRequest smsRequest in smssfaild)
        {
          ++num;
          if (flag1)
          {
            if (flag2)
            {
              try
              {
                DbParameterCollection parameters = dbCommand.Parameters;
                if (parameters.Count > 0)
                  parameters.Clear();
                DbParameter parameter1 = dbCommand.CreateParameter();
                parameter1.ParameterName = "@MESSAGEID";
                parameter1.Value = (object) smsRequest.RefNumber;
                parameters.Add((object) parameter1);
                DbParameter parameter2 = dbCommand.CreateParameter();
                parameter2.ParameterName = "@PROVIDER";
                parameter2.Value = (object) smsRequest.Provider;
                parameters.Add((object) parameter2);
                DbParameter parameter3 = dbCommand.CreateParameter();
                parameter3.ParameterName = "@STATE";
                parameter3.Value = (object) smsRequest.Status;
                parameters.Add((object) parameter3);
                DbParameter parameter4 = dbCommand.CreateParameter();
                parameter4.ParameterName = "@STATEDESCRIPTION";
                parameter4.Value = (object) smsRequest.StatusDescription;
                parameters.Add((object) parameter4);
                dbCommand.ExecuteNonQuery();
              }
              catch
              {
                if (dbCommand != null)
                {
                  try
                  {
                    dbCommand.Dispose();
                  }
                  catch
                  {
                  }
                  dbCommand = (DbCommand) null;
                }
                flag2 = false;
                break;
              }
            }
            else
              break;
          }
          else
            break;
        }
        this.Log("INFO", "SentSMSS()", "stop logging sms's (" + (object) num + ") failed to db");
      }
      if (totalstopped > 0L)
      {
        this.Log("INFO", "SentSMSS()", "start logging sms's (" + (object) totalstopped + ") stopped to db");
        long num = 0;
        foreach (SMSRequest smsRequest in smssstopped)
        {
          ++num;
          if (flag1)
          {
            if (flag2)
            {
              try
              {
                DbParameterCollection parameters = dbCommand.Parameters;
                if (parameters.Count > 0)
                  parameters.Clear();
                DbParameter parameter1 = dbCommand.CreateParameter();
                parameter1.ParameterName = "@MESSAGEID";
                parameter1.Value = (object) smsRequest.RefNumber;
                parameters.Add((object) parameter1);
                DbParameter parameter2 = dbCommand.CreateParameter();
                parameter2.ParameterName = "@PROVIDER";
                parameter2.Value = (object) smsRequest.Provider;
                parameters.Add((object) parameter2);
                DbParameter parameter3 = dbCommand.CreateParameter();
                parameter3.ParameterName = "@STATE";
                parameter3.Value = (object) "Stop";
                parameters.Add((object) parameter3);
                DbParameter parameter4 = dbCommand.CreateParameter();
                parameter4.ParameterName = "@STATEDESCRIPTION";
                parameter4.Value = (object) smsRequest.StatusDescription;
                parameters.Add((object) parameter4);
                dbCommand.ExecuteNonQuery();
              }
              catch
              {
                if (dbCommand != null)
                {
                  try
                  {
                    dbCommand.Dispose();
                  }
                  catch
                  {
                  }
                  dbCommand = (DbCommand) null;
                }
                flag2 = false;
                break;
              }
            }
            else
              break;
          }
          else
            break;
        }
        this.Log("INFO", "SentSMSS()", "stop logging sms's (" + (object) num + ") stopped to db");
      }
      if (flag2)
      {
        if (dbCommand != null)
        {
          try
          {
            dbCommand.Dispose();
          }
          catch
          {
          }
        }
      }
      if (flag1)
      {
        if (dbConnection != null)
        {
          try
          {
            dbConnection.Close();
            dbConnection.Dispose();
          }
          catch
          {
          }
        }
      }
    }

    private DbConnection NewConnection()
    {
      DbConnection connection = this.sqlClientFactory.CreateConnection();
      connection.ConnectionString = this.connectionString;
      connection.Open();
      return connection;
    }

    public bool ConnectGateway(ConnectingGateway connectingGateway)
    {
      DbConnection dbConnection = (DbConnection) null;
      DbCommand dbCommand = (DbCommand) null;
      bool flag = false;
      try
      {
        dbConnection = this.NewConnection();
        dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText = "SELECT top 1 GatewayService,[Username],[Password],[OkToSendTimeWindowStart],[OkToSendTimeWindowEnd],[MaxSendPerRun] FROM [CIMV33].[dbo].[ConfigSmsGateways] where GatewayService like 'stouf'";
        DbDataReader dbDataReader = dbCommand.ExecuteReader();
        if (dbDataReader.Read())
        {
          object[] values = new object[dbDataReader.FieldCount];
          dbDataReader.GetValues(values);
          connectingGateway(values[0].ToString(), new string[3]
          {
            "username=" + values[1].ToString(),
            "password=" + values[2].ToString(),
            "sender=" + CIMDB.sender
          });
          flag = true;
        }
        dbDataReader.Close();
      }
      catch (Exception ex)
      {
        if (dbCommand != null)
        {
          try
          {
            dbCommand.Dispose();
          }
          catch
          {
          }
          dbCommand = (DbCommand) null;
        }
        if (dbConnection != null)
        {
          try
          {
            dbConnection.Close();
          }
          catch
          {
          }
          dbConnection = (DbConnection) null;
        }
      }
      if (dbCommand != null)
      {
        try
        {
          dbCommand.Dispose();
        }
        catch
        {
        }
      }
      if (dbConnection != null)
      {
        try
        {
          dbConnection.Close();
        }
        catch
        {
        }
      }
      return flag;
    }

    public Logging OnLogging
    {
      get => this.onLogging;
      set => this.onLogging = value;
    }

    private void Log(string level, string section, string message, Exception e = null)
    {
      if (this.onLogging == null)
        return;
      this.onLogging(level, section, message, e);
    }
  }
}
