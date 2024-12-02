using AEBLL.Contract;
using AEBLL.Implement;
using AEModels.Models;
using Microsoft.Win32;
using OPCAE.OPC;
using OPCAE.OPCAE;
using OPCAE.OPCAE.Contract;
using OPCAE.OPCAE.NET;
using OPCAEModel.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace OPCAEClient
{
    public partial class Form1 : Form
    {
        int resultCode;
        string msg_0 = string.Empty;
        OpcEventServer opcEventServer = new OpcEventServer();
        List<ALARMEVENTINFO> alarmEventInfoList =new  List<ALARMEVENTINFO>();
        IAlarmEventInfoManage _alarmEventInfoManage = new AlarmEventInfoManage();
        IMinInfoManage _minInfoManage = new MinInfoManage();
        OpcEventServer[] opcEventServerArr = null;
        List<List<string>> opcServerInfoList = new List<List<string>>();
        int[] hostcode = null;
        IDayStatisticsInfoManage _dayStatisticsInfoManage = new DayStatisticsInfoManage();
        IStatisticsInfoManage _statisticsInfoManage = new StatisticsInfoManage();
        public Form1()
        {
            InitializeComponent();
            //AutoStart();
        }
        public async void init() 
        {
            //获取所有OPCServerInfo
            List<List<string>> opcServerInfoList = getOPCServerInfoList();
            //进行连接
            getAEData(opcServerInfoList);
        }
        /// <summary>
        /// 获取OPCServer信息
        /// </summary>
        /// <returns></returns>
        public List<List<string>> getOPCServerInfoList() 
        {
            //获取server信息路径
            string serverPath= "C:\\OPCServerInfo.csv";
            //string serverPath = ConfigurationManager.AppSettings["serverAddr"];
            // 读取csv文件中内容
            //使用for循环 读取多个server信息
            FileStream fs = new FileStream(serverPath, FileMode.Open, FileAccess.Read,FileShare.ReadWrite);
            StreamReader sr = new StreamReader(fs, Encoding.Default);
            string[] lines;
            string data;
            int lineNum = 0;
            
            while ((data = sr.ReadLine()) != null)
            {
                lines = data.Split(',');
                if (lineNum > 0 && lines.Length > 2)
                {
                    if ((!string.IsNullOrEmpty(lines[1])) && (!string.IsNullOrEmpty(lines[2])))
                    {
                        List<string> opcServerInfo = new List<string>();
                        opcServerInfo.Add(lines[1]);
                        opcServerInfo.Add(lines[2]);
                        opcServerInfo.Add(lines[3]);
                        opcServerInfoList.Add(opcServerInfo);
                    }
                }
                lineNum++;
            }
            return opcServerInfoList;
        }
        /// <summary>
        /// 服务连接并读数据
        /// </summary>
        /// <param name="opcServerInfoList"></param>
        public void getAEData(List<List<string>> opcServerInfoList) 
        {
            string msg = string.Empty;
            if (opcServerInfoList != null && opcServerInfoList.Count > 0)
            {
                int opcServerCount = opcServerInfoList.Count;
                hostcode = new int[opcServerCount];
                opcEventServerArr = new OpcEventServer[opcServerCount];
                //有数据时进行服务连接
                //await Task.Run(() =>
                //{
                    for (int k = 0; k < opcServerCount; k++)
                    {
                        int num = k;
                        if (!string.IsNullOrEmpty(opcServerInfoList[num][2]))
                        {
                            if (opcServerInfoList[num][2].Contains("霍尼"))
                            {
                                #region
                                //开启线程
                                opcEventServerArr[num] = new OpcEventServer();
                                HoneyHandler(opcEventServerArr[num], opcServerInfoList[num][0], opcServerInfoList[num][1]);
                                #endregion
                            }
                            else
                            {
                                #region
                                opcEventServerArr[num] = new OpcEventServer();
                                SUPCONHandler(opcEventServerArr[num], opcServerInfoList[num][0], opcServerInfoList[num][1]);
                                #endregion
                            }
                        }
                    }
               // });
                
            }
        }
        //开启线程 实现OPC连接 霍尼
        private async Task HoneyHandler(OpcEventServer opcEventServer_0, string name, string ip)
        {
            string msg = "";
            OpcEventServer opcEventServerHoney = new OpcEventServer();
            opcEventServerHoney = opcEventServer_0;
            int hostcodeHoney = opcEventServer_0.Connect(name, ip);
            if (HRESULTS.Failed(hostcodeHoney))
            {
                msg = "不能连接到服务器，错误码 0x" + hostcodeHoney.ToString("X");
                if (this.InvokeRequired)
                {
                    // 使用Invoke安全地在UI线程上调用
                    this.Invoke(new Action(() =>
                    {
                        // 更新UI的代码，例如：
                        this.label1.Text += name + "/" + ip + "：" + msg + "\r\n";
                    }));
                }
                else
                {
                    // 如果已经在UI线程上，直接更新UI
                    this.label1.Text += name + "/" + ip + "：" + msg + "\r\n";
                }
            }
            else
            {
                //连接成功
                EventServerStatus eventServerStatus;
                int eventStatusCode = opcEventServerHoney.GetStatus(out eventServerStatus);
                if (HRESULTS.Failed(eventStatusCode))
                {
                    msg = opcEventServerHoney.GetErrorString(eventStatusCode);
                }
                else
                {
                    try
                    {
                        //实时报警
                        getAlarmEventInfo(opcEventServerHoney);
                        //陈旧报警
                        getOldAlarmEventInfo(opcEventServerHoney);
                        msg = "连接成功";
                    }
                    catch (Exception e)
                    {
                        msg = e.ToString();
                    }

                    if (this.InvokeRequired)
                    {
                        // 使用Invoke安全地在UI线程上调用
                        this.Invoke(new Action(() =>
                        {
                            // 更新UI的代码，例如：
                            this.label1.Text += name + "/" + ip + "：" + msg + "\r\n";
                        }));
                    }
                    else
                    {
                        // 如果已经在UI线程上，直接更新UI
                        this.label1.Text += name + "/" + ip + "：" + msg + "\r\n";
                    }
                }
            }
        }

        //开启线程 实现OPC连接 中控
        private async Task SUPCONHandler(OpcEventServer opcEventServer_0, string name, string ip)
        {
            string msg = "";
            OpcEventServer opcEventServerSUPCON = new OpcEventServer();
            opcEventServerSUPCON = opcEventServer_0;
            int hostcodeSUPCON = opcEventServer_0.Connect(name, ip);
            if (HRESULTS.Failed(hostcodeSUPCON))
            {
                msg = "不能连接到服务器，错误码 0x" + hostcodeSUPCON.ToString("X");
                if (this.InvokeRequired)
                {
                    // 使用Invoke安全地在UI线程上调用
                    this.Invoke(new Action(() =>
                    {
                        // 更新UI的代码，例如：
                        this.label1.Text += name + "/" + ip + "：" + msg + "\r\n";
                    }));
                }
                else
                {
                    // 如果已经在UI线程上，直接更新UI
                    this.label1.Text += name + "/" + ip + "：" + msg + "\r\n";
                }
            }
            else
            {
                //连接成功
                EventServerStatus eventServerStatus;
                int eventStatusCode = opcEventServerSUPCON.GetStatus(out eventServerStatus);
                if (HRESULTS.Failed(eventStatusCode))
                {
                    msg = opcEventServerSUPCON.GetErrorString(eventStatusCode);
                }
                else
                {
                    try
                    {
                        //实时报警
                        getAlarmEventInfo_SUPCON(opcEventServerSUPCON);
                        //陈旧报警
                        getOldAlarmEventInfo_SUPCON(opcEventServerSUPCON);
                        msg = "连接成功";
                    }
                    catch (Exception e)
                    {
                        msg = e.ToString();
                    }
                    if (this.InvokeRequired)
                    {
                        // 使用Invoke安全地在UI线程上调用
                        this.Invoke(new Action(() =>
                        {
                            // 更新UI的代码，例如：
                            this.label1.Text += name + "/" + ip + "：" + msg + "\r\n";
                        }));
                    }
                    else
                    {
                        // 如果已经在UI线程上，直接更新UI
                        this.label1.Text += name + "/" + ip + "：" + msg + "\r\n";
                    }
                }
            }
        }

        /// <summary>
        /// 霍尼 陈旧报警获取
        /// </summary>
        /// <param name="opcEventServer_0"></param>
        /// <returns></returns>
        private void getOldAlarmEventInfo(OpcEventServer opcEventServer_0)
        {
            OnAEeventHandler onAEeventHandler_old = new OnAEeventHandler(myEventHandler_old);
            //eventSubscriptionMgt_old = new EventSubscriptionMgt(onAEeventHandler_old);
            EventSubscriptionMgt oldEventSubscriptionMgt_Honey= new EventSubscriptionMgt(onAEeventHandler_old);
            int clientSubscription_old = 1236;
            int bufferTime = 600000;
            int maxSize = 0;
            int revisedBufferTime_old;
            int revisedMaxSize_old;
            int resultCode_0_old = oldEventSubscriptionMgt_Honey.Create(opcEventServer_0, true, bufferTime, maxSize, clientSubscription_old, out revisedBufferTime_old, out revisedMaxSize_old);

            //int attr_eventCategorie_in = 16385; int[] attrIDs_in=new int[] {1069,8,10,1065,1021,1022,1026,1027,1028,1029,1045,1048,1049,1050,1051,1054,1058,1066,1067,
            //    1068,1071,1072,1073,1080,1082,10750,5002,12001,12003,12004,12005,12008,12009,13};
            int attr_eventCategorie_in_old = 16385; int[] attrIDs_in_old = new int[] { 1069 };
            int attr_code_old = oldEventSubscriptionMgt_Honey.SelectReturnedAttributes(attr_eventCategorie_in_old, attrIDs_in_old);

            int get_attr_eventCategorie_in_old = 16385; int[] attrIDs_out_old;
            int get_attr_code_old = oldEventSubscriptionMgt_Honey.GetReturnedAttributes(get_attr_eventCategorie_in_old, out attrIDs_out_old);
            //eventSubscriptionMgt.SetFilter(7,new int[] { 16385},0,1000,new string[] { "ASSET_1" },new string[] { "FIC10002"});
            oldEventSubscriptionMgt_Honey.Refresh();
            if (HRESULTS.Failed(resultCode_0_old))
            {
                resultCode = resultCode_0_old;
                base.DialogResult = DialogResult.Abort;
                base.Close();
            }
            else
            {
                //method0();
                //method1();
                // msg = "连接服务器成功";
            }
        }
        /// <summary>
        /// 霍尼实时报警读取
        /// </summary>
        /// <param name="opcEventServer_0"></param>
        /// <returns></returns>
        private void getAlarmEventInfo(OpcEventServer opcEventServer_0)
        {
            OnAEeventHandler onAEeventHandler = new OnAEeventHandler(myEventHandler);
            EventSubscriptionMgt eventSubscriptionMgt_Honey= new EventSubscriptionMgt(onAEeventHandler);
            //eventSubscriptionMgt = new EventSubscriptionMgt(onAEeventHandler);
            int clientSubscription = 1235;
            int revisedBufferTime;
            int revisedMaxSize;
            int resultCode_0 = eventSubscriptionMgt_Honey.Create(opcEventServer_0, true, 0x3e8, 1, clientSubscription, out revisedBufferTime, out revisedMaxSize);

            //int attr_eventCategorie_in = 16385; int[] attrIDs_in=new int[] {1069,8,10,1065,1021,1022,1026,1027,1028,1029,1045,1048,1049,1050,1051,1054,1058,1066,1067,
            //    1068,1071,1072,1073,1080,1082,10750,5002,12001,12003,12004,12005,12008,12009,13};
            int attr_eventCategorie_in = 16385; int[] attrIDs_in = new int[] { 1069 };
            int attr_code = eventSubscriptionMgt_Honey.SelectReturnedAttributes(attr_eventCategorie_in, attrIDs_in);

            int get_attr_eventCategorie_in = 16385; int[] attrIDs_out;
            int get_attr_code = eventSubscriptionMgt_Honey.GetReturnedAttributes(get_attr_eventCategorie_in, out attrIDs_out);
            //eventSubscriptionMgt.SetFilter(7,new int[] { 16385},0,1000,new string[] { "ASSET_1" },new string[] { "FIC10002"});
            eventSubscriptionMgt_Honey.Refresh();
            if (HRESULTS.Failed(resultCode_0))
            {
                resultCode = resultCode_0;
                base.DialogResult = DialogResult.Abort;
                base.Close();
            }
            else
            {
                // method0();
                // method1();
                // msg = "连接服务器成功";
            }
        }

        private void getOldAlarmEventInfo_SUPCON(OpcEventServer opcEventServer_0)
        {
            OnAEeventHandler onAEeventHandler0 = new OnAEeventHandler(myEventHandler);
            EventSubscriptionMgt0 oldEventSubscriptionMgt_SUPCON=new EventSubscriptionMgt0(onAEeventHandler0);
            //eventSubscriptionMgt_0 = new EventSubscriptionMgt0(onAEeventHandler0);
            int clientSubscription = 1235;
            int bufferTime = 600000;
            int maxSize = 0;
            int revisedBufferTime_old;
            int revisedMaxSize_old;
            int resultCode_0 = oldEventSubscriptionMgt_SUPCON.Create(opcEventServer_0, true, bufferTime, maxSize, clientSubscription, out revisedBufferTime_old, out revisedMaxSize_old);
            //1、3、4
            int attr_eventCategorie_in_old = 1; int[] attrIDs_in_old = new int[] { 1069 };
            int attr_code_old = oldEventSubscriptionMgt_SUPCON.SelectReturnedAttributes(attr_eventCategorie_in_old, attrIDs_in_old);

            int get_attr_eventCategorie_in_old = 1; int[] attrIDs_out_old;
            int get_attr_code_old = oldEventSubscriptionMgt_SUPCON.GetReturnedAttributes(get_attr_eventCategorie_in_old, out attrIDs_out_old);
            //eventSubscriptionMgt.SetFilter(7,new int[] { 16385},0,1000,new string[] { "ASSET_1" },new string[] { "FIC10002"});
            oldEventSubscriptionMgt_SUPCON.Refresh();
            //Thread.Sleep(5000);
            if (HRESULTS.Failed(resultCode_0))
            {
                resultCode = resultCode_0;
                base.DialogResult = DialogResult.Abort;
                base.Close();
            }
        }
        /// <summary>
        /// 霍尼实时报警读取
        /// </summary>
        /// <param name="opcEventServer_0"></param>
        /// <returns></returns>
        private void getAlarmEventInfo_SUPCON(OpcEventServer opcEventServer_0)
        {
            // 更新UI
            OnAEeventHandler onAEeventHandler0 = new OnAEeventHandler(myEventHandler);
            EventSubscriptionMgt0 eventSubscriptionMgt_SUPCON = new EventSubscriptionMgt0(onAEeventHandler0);
            //eventSubscriptionMgt_0 = new EventSubscriptionMgt0(onAEeventHandler0);
            int clientSubscription = 1235;
            int revisedBufferTime;
            int revisedMaxSize;
            int resultCode_0 = eventSubscriptionMgt_SUPCON.Create(opcEventServer_0, true, 0x3e8, 100, clientSubscription, out revisedBufferTime, out revisedMaxSize);
            //1、3、4
            int attr_eventCategorie_in_old = 1; int[] attrIDs_in_old = new int[] { 8 };
            int attr_code_old = eventSubscriptionMgt_SUPCON.SelectReturnedAttributes(attr_eventCategorie_in_old, attrIDs_in_old);

            int get_attr_eventCategorie_in_old = 1; int[] attrIDs_out_old;
            int get_attr_code_old = eventSubscriptionMgt_SUPCON.GetReturnedAttributes(get_attr_eventCategorie_in_old, out attrIDs_out_old);
            //eventSubscriptionMgt.SetFilter(7,new int[] { 16385},0,1000,new string[] { "ASSET_1" },new string[] { "FIC10002"});
            eventSubscriptionMgt_SUPCON.Refresh();
            // Thread.Sleep(5000);
            if (HRESULTS.Failed(resultCode_0))
            {
                resultCode = resultCode_0;
                base.DialogResult = DialogResult.Abort;
                base.Close();
            }
        }

        private void myEventHandler(object sender, userEventArgs e,OpcEventServer opcESvr)
        {
            base.BeginInvoke(new OnAEeventHandler(this.myEventHandlerSync), new object[] { sender, e, opcESvr });

        }
       /// <summary>
       /// 中控 保存实时报警
       /// </summary>
       /// <param name="sender"></param>
       /// <param name="e"></param>
       /// <param name="opcESvr"></param>
        private void myEventHandlerSync(object sender, userEventArgs e, OpcEventServer opcESvr)
        {
            if (e.Events != null)
            {
                int length = e.Events.Length;
                int num2 = 0x19;
                for (int i = 0; i < length; i++)
                {
                    if (e.Events[i] != null)
                    {
                        ALARMEVENTINFO alarmEventInfo = new ALARMEVENTINFO();
                        alarmEventInfo.AESource = e.Events[i].Source;
                        string source = e.Events[i].Source;
                        string[] aeSourceArray = alarmEventInfo.AESource.Split('/');
                        int aeSourceLen = aeSourceArray.Length;
                        if (aeSourceLen > 1)
                        {
                            alarmEventInfo.AETagName = aeSourceArray[aeSourceLen - 1];
                            alarmEventInfo.AELocationTag = aeSourceArray[aeSourceLen - 2];
                            alarmEventInfo.AELocation = alarmEventInfo.AESource.Substring(0, alarmEventInfo.AESource.LastIndexOf('/'));
                        }
                        else if (aeSourceLen == 1)
                        {
                            alarmEventInfo.AELocation = e.Events[i].Source;
                        }
                        else
                        {
                            alarmEventInfo.AELocation = "others";
                        }
                        DateTime time = DateTime.FromFileTime(e.Events[i].Time);
                        alarmEventInfo.AETime = time;
                        StringBuilder builder = new StringBuilder(20);
                        builder.AppendFormat("{0,2:d}:{1,2:d}:{2,2:d}.{3,3:d}", new object[] { time.Hour, time.Minute, time.Second, time.Millisecond });
                        for (int j = 0; j < builder.Length; j++)
                        {
                            if (builder[j] == ' ')
                            {
                                builder[j] = '0';
                            }
                        }
                        alarmEventInfo.AETimeStr = builder.ToString();
                        alarmEventInfo.AEMessage = e.Events[i].Message;
                        if (e.Events[i].EventAttributes!=null)
                        {
                            for (int j = 0, l = e.Events[i].EventAttributes.Length; j < l; j++) 
                            {
                                alarmEventInfo.EventAttributes += e.Events[i].EventAttributes[j]+",";
                                if (j==0)
                                {
                                    //是否抑制
                                    //if()
                                    byte shelved = 0;
                                    if (e.Events[i].EventAttributes[j].GetType() == typeof(Int16))
                                    {
                                        alarmEventInfo.IsShelved = (byte)Convert.ToInt32(e.Events[i].EventAttributes[j]);
                                    }
                                    else if (e.Events[i].EventAttributes[j].GetType() == typeof(bool)) 
                                    {
                                        bool isShelved = (bool)(e.Events[i].EventAttributes[j]);
                                        alarmEventInfo.IsShelved =(byte)(isShelved ? 1:0);
                                    }
                                    
                                    
                                }
                            }
                        }
                        alarmEventInfo.AECategory = e.Events[i].EventCategory.ToString();
                        alarmEventInfo.EventType = e.Events[i].EventType;
                        string text = "";
                        switch (e.Events[i].EventType)
                        {
                            case 1://1L
                                text = "simple";
                                break;

                            case 4://4L
                                text = "condition";
                                break;

                            case 2://2L
                                text = "tracking";
                                break;
                        }
                        //listViewEvents.Items[0].SubItems.Add(text);
                        alarmEventInfo.AESeverity = e.Events[i].Severity.ToString();
                        alarmEventInfo.AECondition = e.Events[i].ConditionName;
                        alarmEventInfo.AEAckRequired = e.Events[i].AckRequired;
                        if (e.Events[i].ActiveTime != 0)
                        {
                            alarmEventInfo.AEActiveTime = DateTime.FromFileTime(e.Events[i].ActiveTime);
                        }

                        alarmEventInfo.AEChangeMask = e.Events[i].ChangeMask.ToString();
                        alarmEventInfo.AENewState = e.Events[i].NewState.ToString();
                        alarmEventInfo.AEQuality = e.Events[i].Quality.ToString();
                        alarmEventInfo.AEType = "0";
                        if (alarmEventInfo.AECondition != null && alarmEventInfo.AECondition.Contains("CHANGE"))
                        {
                            alarmEventInfo.AEType = "1";
                        }
                        if (alarmEventInfo.AECategory == "4")
                        {
                            alarmEventInfo.AEType = "1";
                        }
                        //if (alarmEventInfo.AECondition != null && alarmEventInfo.AECondition == "BAD PV" || alarmEventInfo.AECondition == "BADCTL" || alarmEventInfo.AECondition == "PVHIGH" || alarmEventInfo.AECondition == "PVHIHI" || alarmEventInfo.AECondition == "PVLOWGH" || alarmEventInfo.AECondition == "PVLOWLOW")
                        //{
                        //    alarmEventInfo.AEType = "0";
                        //}
                        alarmEventInfo.AESubCond = e.Events[i].SubconditionName;
                        //
                        int[] attributeID = new int[] { 1, 2, 4 };
                        OPCConditionState opcConState;
                        if (HRESULTS.Succeeded(opcESvr.GetConditionState(opcESvr, opcServerInfoList, alarmEventInfo.AESource, alarmEventInfo.AECondition, attributeID, out opcConState))) 
                        {
                            if (opcConState!=null) 
                            {
                                alarmEventInfo.CondLastActive = opcConState.CondLastActive;
                                alarmEventInfo.CondLastInActive = opcConState.CondLastInactive;
                                alarmEventInfo.AscDescription = opcConState.ASCDescription;
                            }
                        }
                        _alarmEventInfoManage.addAlarmEventInfo(alarmEventInfo);
                        //alarmEventInfoList.Add(alarmEventInfo);
                    }
                }
            }
            //保存至数据库

        }


        private void myEventHandler_old(object sender, userEventArgs e, OpcEventServer opcESvr)
        {
            base.BeginInvoke(new OnAEeventHandler(this.myEventHandlerSync_old), new object[] { sender, e, opcESvr });

        }
        /// <summary>
        /// 保存陈旧报警
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <param name="opcESvr"></param>
        private void myEventHandlerSync_old(object sender, userEventArgs e, OpcEventServer opcESvr)
        {
            DateTime nowTime= DateTime.Now;
            if (e.Events != null)
            {
                //List<OldAlarmEventInfo> alarmEventInfoList = new List<OldAlarmEventInfo>();
                int length = e.Events.Length;
                int num2 = 0x19;
                for (int i = 0; i < length; i++)
                {
                    if (e.Events[i] != null)
                    {
                        OLDALARMEVENTINFO alarmEventInfo = new OLDALARMEVENTINFO();
                        alarmEventInfo.AESource = e.Events[i].Source;
                        string source = e.Events[i].Source;
                        string[] aeSourceArray = alarmEventInfo.AESource.Split('/');
                        int aeSourceLen = aeSourceArray.Length;
                        if (aeSourceLen > 1)
                        {
                            alarmEventInfo.AETagName = aeSourceArray[aeSourceLen - 1];
                            alarmEventInfo.AELocationTag = aeSourceArray[aeSourceLen - 2];
                            alarmEventInfo.AELocation = alarmEventInfo.AESource.Substring(0, alarmEventInfo.AESource.LastIndexOf('/'));
                        }
                        else if (aeSourceLen == 1)
                        {
                            alarmEventInfo.AELocation = e.Events[i].Source;
                        }
                        else
                        {
                            alarmEventInfo.AELocation = "others";
                        }
                        DateTime time = DateTime.FromFileTime(e.Events[i].Time);
                        alarmEventInfo.AETime = time;
                        StringBuilder builder = new StringBuilder(20);
                        builder.AppendFormat("{0,2:d}:{1,2:d}:{2,2:d}.{3,3:d}", new object[] { time.Hour, time.Minute, time.Second, time.Millisecond });
                        for (int j = 0; j < builder.Length; j++)
                        {
                            if (builder[j] == ' ')
                            {
                                builder[j] = '0';
                            }
                        }
                        alarmEventInfo.AETimeStr = builder.ToString();
                        alarmEventInfo.AEMessage = e.Events[i].Message;
                        if (e.Events[i].EventAttributes != null)
                        {
                            for (int j = 0, l = e.Events[i].EventAttributes.Length; j < l; j++)
                            {
                                alarmEventInfo.EventAttributes += e.Events[i].EventAttributes[j] + ",";
                                if (j == 0)
                                {
                                    //是否抑制
                                    alarmEventInfo.IsShelved = Convert.ToByte(e.Events[i].EventAttributes[j]);
                                }
                            }
                        }
                        alarmEventInfo.AECategory = e.Events[i].EventCategory.ToString();
                        alarmEventInfo.EventType = e.Events[i].EventType;
                        string text = "";
                        switch (e.Events[i].EventType)
                        {
                            case 1://1L
                                text = "simple";
                                break;

                            case 4://4L
                                text = "condition";
                                break;

                            case 2://2L
                                text = "tracking";
                                break;
                        }
                        //listViewEvents.Items[0].SubItems.Add(text);
                        alarmEventInfo.AESeverity = e.Events[i].Severity.ToString();
                        alarmEventInfo.AECondition = e.Events[i].ConditionName;
                        alarmEventInfo.AEAckRequired = e.Events[i].AckRequired;
                        if (e.Events[i].ActiveTime != 0)
                        {
                            alarmEventInfo.AEActiveTime = DateTime.FromFileTime(e.Events[i].ActiveTime);
                        }

                        alarmEventInfo.AEChangeMask = e.Events[i].ChangeMask.ToString();
                        alarmEventInfo.AENewState = e.Events[i].NewState.ToString();
                        alarmEventInfo.AEQuality = e.Events[i].Quality.ToString();
                        alarmEventInfo.AEType = "0";
                        if (alarmEventInfo.AECondition != null && alarmEventInfo.AECondition.Contains("CHANGE"))
                        {
                            alarmEventInfo.AEType = "1";
                        }
                        if (alarmEventInfo.AECategory == "4")
                        {
                            alarmEventInfo.AEType = "1";
                        }
                        //if (alarmEventInfo.AECondition != null && alarmEventInfo.AECondition == "BAD PV" || alarmEventInfo.AECondition == "BADCTL" || alarmEventInfo.AECondition == "PVHIGH" || alarmEventInfo.AECondition == "PVHIHI" || alarmEventInfo.AECondition == "PVLOWGH" || alarmEventInfo.AECondition == "PVLOWLOW")
                        //{
                        //    alarmEventInfo.AEType = "0";
                        //}
                        alarmEventInfo.AESubCond = e.Events[i].SubconditionName;
                        //
                        int[] attributeID = new int[] { 1, 2, 4 };
                        OPCConditionState opcConState;
                        if (HRESULTS.Succeeded(opcESvr.GetConditionState(opcESvr, opcServerInfoList, alarmEventInfo.AESource, alarmEventInfo.AECondition, attributeID, out opcConState)))
                        {
                            if (opcConState != null)
                            {
                                alarmEventInfo.CondLastActive = opcConState.CondLastActive;
                                alarmEventInfo.CondLastInActive = opcConState.CondLastInactive;
                                alarmEventInfo.AscDescription = opcConState.ASCDescription;
                            }
                        }
                        //if (alarmEventInfo.AESource != "Testing license") 
                        //{
                        //    alarmEventInfoList.Add(alarmEventInfo);
                        //}
                        if (alarmEventInfo.AEActiveTime != null && ((TimeSpan)(nowTime - alarmEventInfo.AEActiveTime)).TotalHours >= 24)
                        {
                           var oldAlarmEventListQue = _alarmEventInfoManage.getAllOldAlarmEventInfo();
                            List<OLDALARMEVENTINFO> oldAEList = oldAlarmEventListQue.ToList();

                            DateTime activeTime = (DateTime)alarmEventInfo.AEActiveTime;
                            string activeTimeStr_0 = activeTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
                            DateTime activeTime_0 = DateTime.Parse(activeTimeStr_0);
                            if (alarmEventInfo.CondLastInActive != null && alarmEventInfo.CondLastActive != null)
                            {
                                if (alarmEventInfo.CondLastInActive < alarmEventInfo.CondLastActive && ((TimeSpan)(nowTime - alarmEventInfo.AEActiveTime)).TotalHours >= 24)
                                {
                                    
                                    DateTime lastActiveTime = (DateTime)alarmEventInfo.CondLastActive;
                                    string lastActiveTimeStr_0 = lastActiveTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
                                    DateTime lastActiveTime_0=DateTime.Parse(lastActiveTimeStr_0);
                                    DateTime lastInActiveTime = (DateTime)alarmEventInfo.CondLastInActive;
                                    string lastInActiveTimeStr_0 = lastInActiveTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
                                    DateTime lastInActiveTime_0= DateTime.Parse(lastInActiveTimeStr_0);
                                    int oldCount = oldAlarmEventListQue.Where(a => a.AETagName == alarmEventInfo.AETagName && a.AEActiveTime == activeTime_0 && a.CondLastInActive == lastInActiveTime_0 && a.CondLastActive == lastActiveTime_0&&a.AECondition == alarmEventInfo.AECondition).Count();
                                    //添加陈旧报警
                                    if (oldCount > 0)
                                    {
                                        break;
                                    }
                                    else
                                    {
                                        _alarmEventInfoManage.addOldAlarmEventInfo(alarmEventInfo);
                                    }
                                    
                                }
                            }
                            else
                            {
                                //添加陈旧报警
                                int countOld = oldAlarmEventListQue.Where(a => a.AETagName == alarmEventInfo.AETagName && a.AEActiveTime == activeTime_0 && a.AECondition == alarmEventInfo.AECondition).Count();
                                if (countOld > 0)
                                {
                                    break;
                                }
                                else 
                                {
                                    _alarmEventInfoManage.addOldAlarmEventInfo(alarmEventInfo);
                                }
                            }
                        }
                    }
                }
            }

        }
        #region
        //private void button3_Click(object sender, EventArgs e)
        //{
        //    this.label3.Text = "";
        //    string host = this.textBox1.Text;
        //    string[] serversArray=null;
        //    try 
        //    {
        //        // 获取所有servers
        //        OpcAEServerBrowser opcAEServerBrowser = new OpcAEServerBrowser(host);
        //        opcAEServerBrowser.GetServerList(out serversArray);
        //    } catch (Exception exception) 
        //    { 

        //    }

        //    this.comboBox1.Items.Clear();
        //    if (serversArray == null)
        //    {
        //        this.label3.Text = "没有找到数据源";
        //    }
        //    else 
        //    {

        //        this.comboBox1.Items.AddRange(serversArray);
        //        this.comboBox1.SelectedIndex = 1;
        //        this.button1.Enabled = true;
        //    }
        //}
        #endregion

        private void Show_Click(object sender, EventArgs e)
        {
            //显示界面
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.Activate();
        }


        private void Exit_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("你确定要退出程序吗？", "确认", MessageBoxButtons.OKCancel,
                MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.OK)
            {
                notifyIcon1.Visible = false;
                this.Close();
                this.Dispose();
                Application.Exit();
            }
        }

        private void Form_Load(object sender, EventArgs e)
        {
            ShowInTaskbar = false;
            Hide();
            init();
            System.Timers.Timer timer_0 = new System.Timers.Timer();
            timer_0.Interval = 360000;
            timer_0.Elapsed += Timer_Tick;
            timer_0.Start();
        }
        private void AutoStart(bool isAuto = true, bool showinfo = true)
        {
            try
            {
                if (isAuto == true)
                {
                    RegistryKey R_local = Registry.LocalMachine;//RegistryKey R_local = Registry.CurrentUser;
                    RegistryKey R_run = R_local.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                    if (R_run == null)
                    {
                        R_local.CreateSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run");

                    }
                    R_run.SetValue("AlarmEvent", Application.ExecutablePath);
                    R_run.Close();
                    R_local.Close();
                }
                else
                {
                    RegistryKey R_local = Registry.LocalMachine;//RegistryKey R_local = Registry.CurrentUser;
                    RegistryKey R_run = R_local.CreateSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run");
                    R_run.DeleteValue("AlarmEvent", false);
                    R_run.Close();
                    R_local.Close();
                }
            }
            catch (Exception e)
            {
                if (showinfo)
                    MessageBox.Show("您需要管理员权限修改", "提示");
            }
        }

        private void AE_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;
            notifyIcon1.Visible = true;
        }

        private void AE_SizeChanged(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Normal)
            {
                //窗体正常显示
                notifyIcon1.Visible = false;//隐藏托盘图标
            }
            else if (this.WindowState == FormWindowState.Minimized)
            {
                this.Hide();
                notifyIcon1.Visible = true;
            }
        }

        private void AE_notifyMouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                contextMenuStrip1.Show();
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
       {
            //对数据以装置进行日统计
            DateTime endTime = DateTime.Now.Date;
            DateTime startTime = endTime.AddDays(-1);
            IQueryable<DAYSTATISTICSINFO> dayStatisticsInfoQue;
            IQueryable<ALARMEVENTINFO> alarmEventInfoQue=null;
            IQueryable<TENMININFO> tenMinInfoListIQue = null;
            IQueryable<ONETENMININFO> oneTenMinInfoListIQue = null;
            IQueryable<OLDALARMEVENTINFO> oldAlarmEventListIQue = null;
            IQueryable<AESTATISTICSINFO> statisticsListIQue = null;
            //首先判断当天是否已经有数据
            //根据时间查询
            //先判断当前时间是否大于零点20分
            if (DateTime.Now > DateTime.Now.Date.AddMinutes(30))
            {
                dayStatisticsInfoQue = _dayStatisticsInfoManage.getDayStatisticsInfoQueByTime(startTime);
                if (dayStatisticsInfoQue != null)
                {
                    if (dayStatisticsInfoQue.Count() > 0)
                    {
                        //有统计信息，不需要再处理
                        
                    }
                    else
                    {
                        //没有统计信息 进行统计
                        ////首先获取一天的数据
                        alarmEventInfoQue = _alarmEventInfoManage.getAlarmEventInfoList(startTime, endTime, "");
                        //当天10分钟切片数据
                        tenMinInfoListIQue = _minInfoManage.getTenMinInfoList(startTime, endTime, 0, "");
                        //泛滥
                        oneTenMinInfoListIQue = _minInfoManage.getOneTenMinInfoList(startTime, endTime, 0, "");
                        //陈旧报警
                        oldAlarmEventListIQue = _alarmEventInfoManage.getOldAlarmEventInfoList(0, "");
                        //以装置分组
                        var alarmEventInfoListByLocation = alarmEventInfoQue.GroupBy(a => a.AELocation).ToList();

                        statisticsListIQue = _statisticsInfoManage.getAEStatisticsInfoList("", "", startTime, endTime, 0);

                        foreach (var item in alarmEventInfoListByLocation)
                        {
                            DAYSTATISTICSINFO dayStatisticInfo = new DAYSTATISTICSINFO();
                            dayStatisticInfo.AELocation = item.Key;
                            dayStatisticInfo.StartTime = startTime;
                            //报警总数
                            dayStatisticInfo.AlarmTotalCount = item.Where(a => a.AEType == "0").Count();
                            dayStatisticInfo.EventTotalCount = item.Where(a => a.AEType == "1").Count();
                            //小时平均
                            dayStatisticInfo.AlarmHourAvg = dayStatisticInfo.AlarmTotalCount / 24.0;
                            dayStatisticInfo.EventHourAvg = dayStatisticInfo.EventTotalCount / 24.0;
                            //确认率
                            dayStatisticInfo.AckRate = 0;
                            if (dayStatisticInfo.AlarmTotalCount != 0)
                            {
                                dayStatisticInfo.AckRate = Convert.ToDouble(statisticsListIQue.Where(a => a.AELocation == item.Key).Sum(a => a.AckCount)) / dayStatisticInfo.AlarmTotalCount * 100;
                            }
                            #region//峰值\峰值率
                            int alarmPeakCount = 0;
                            int eventPeakCount = 0;
                            var tenMinInfoListByTime = tenMinInfoListIQue.Where(a => a.AELocation == item.Key).GroupBy(a => a.StartTime);
                            foreach (var tenMinItem in tenMinInfoListByTime)
                            {
                                alarmPeakCount = alarmPeakCount + (tenMinItem.Sum(a => a.AlarmCount) >= 10 ? 1 : 0);
                                eventPeakCount = eventPeakCount + (tenMinItem.Sum(a => a.EventCount) >= 10 ? 1 : 0);
                            }
                            dayStatisticInfo.AlarmPeakCount = alarmPeakCount;
                            dayStatisticInfo.EventPeakCount = eventPeakCount;
                            dayStatisticInfo.AlarmPeakRate = alarmPeakCount / 144.0 * 100;
                            dayStatisticInfo.EventPeakRate = eventPeakCount / 144.0 * 100;
                            #endregion

                            #region//泛滥报警、泛滥报警率
                            var oneTenMinInfoListByTime = oneTenMinInfoListIQue.OrderBy(a => a.StartTime).GroupBy(a => a.StartTime).ToList();
                            int num = oneTenMinInfoListByTime.Count;
                            if (num > 0)
                            {
                                //报警
                                double minutes_Alarm = 0;
                                bool isEnd_Alarm = true;
                                DateTime? sTime_Alarm = null;
                                DateTime? eTime_Alarm = null;
                                //事件
                                double minutes_Event = 0;
                                bool isEnd_Event = true;
                                DateTime? sTime_Event = null;
                                DateTime? eTime_Event = null;
                                for (int i = 0; i < num; i++)
                                {
                                    #region//报警

                                    if (Convert.ToInt32(oneTenMinInfoListByTime[i].Sum(a => a.AlarmCount)) >= 10)
                                    {

                                        if (isEnd_Alarm)
                                        {
                                            dayStatisticInfo.AlarmFloodCount = dayStatisticInfo.AlarmFloodCount + 1;
                                            sTime_Alarm = oneTenMinInfoListByTime[i].Key;
                                            isEnd_Alarm = false;
                                        }
                                        else if (i == num - 1 && !isEnd_Alarm)
                                        {
                                            eTime_Alarm = oneTenMinInfoListByTime[i].Key;
                                            minutes_Alarm = minutes_Alarm + (Convert.ToDateTime(eTime_Alarm) - Convert.ToDateTime(sTime_Alarm)).TotalMinutes + 1;
                                            isEnd_Alarm = true;
                                        }
                                    }
                                    else if (Convert.ToInt32(oneTenMinInfoListByTime[i].Sum(a => a.AlarmCount)) < 5 && !isEnd_Alarm && sTime_Alarm != null)
                                    {
                                        eTime_Alarm = oneTenMinInfoListByTime[i].Key;
                                        minutes_Alarm = minutes_Alarm + (Convert.ToDateTime(eTime_Alarm) - Convert.ToDateTime(sTime_Alarm)).TotalMinutes + 1;
                                        isEnd_Alarm = true;
                                    }
                                    #endregion
                                    #region//事件
                                    if (Convert.ToInt32(oneTenMinInfoListByTime[i].Sum(a => a.EventCount)) >= 10)
                                    {

                                        if (isEnd_Event)
                                        {
                                            dayStatisticInfo.EventFloodCount = dayStatisticInfo.EventFloodCount + 1;
                                            sTime_Event = oneTenMinInfoListByTime[i].Key;
                                            isEnd_Event = false;
                                        }
                                        else
                                        if (i == num - 1 && !isEnd_Event)
                                        {
                                            eTime_Event = oneTenMinInfoListByTime[i].Key;
                                            minutes_Event = minutes_Event + (Convert.ToDateTime(eTime_Event) - Convert.ToDateTime(sTime_Event)).TotalMinutes + 1;
                                        }
                                    }
                                    else if (Convert.ToInt32(oneTenMinInfoListByTime[i].Sum(a => a.EventCount)) < 5 && !isEnd_Event && sTime_Event != null)
                                    {
                                        eTime_Event = oneTenMinInfoListByTime[i].Key;
                                        minutes_Event = minutes_Event + (Convert.ToDateTime(eTime_Event) - Convert.ToDateTime(sTime_Event)).TotalMinutes + 1;
                                        isEnd_Event = true;
                                    }
                                    #endregion
                                }
                                dayStatisticInfo.AlarmFloodRate = minutes_Alarm / 1440 * 100;
                                dayStatisticInfo.EventFloodRate = minutes_Event / 1440 * 100;
                            }
                            #endregion
                            //陈旧报警
                            dayStatisticInfo.OldAlarmCount = oldAlarmEventListIQue.Where(a => a.AELocation == item.Key).Count();
                            //抑制报警
                            dayStatisticInfo.ShelvedAlarmCount = alarmEventInfoQue.Where(a => a.AELocation == item.Key && a.IsShelved == 1).GroupBy(a => a.AETagName).Count();

                            //抖动报警  短时间内频繁在激活状态和非激活状态之间切换 1分钟内 频繁切换（大于三次）
                            var alarmEventListByTagName = alarmEventInfoQue.Where(a => a.AEType == "0" && a.AEActiveTime != null).OrderBy(a => a.AEActiveTime).GroupBy(a => a.AETagName).ToList();
                            foreach (var item_0 in alarmEventListByTagName)
                            {
                                //抖动报警
                                var itemList = item_0.ToList();
                                int count_item = itemList.Count - 3;
                                for (int i = 0; i < count_item; i++)
                                {
                                    TimeSpan timeSpan = (TimeSpan)(itemList[i + 3].AEActiveTime - itemList[i].AEActiveTime);
                                    if (timeSpan.TotalSeconds < 60 && timeSpan.TotalSeconds > 0)
                                    {
                                        //一分钟出现三次 则表示抖动
                                        dayStatisticInfo.ChatteringAlarmCount = dayStatisticInfo.ChatteringAlarmCount + 1;
                                        break;
                                    }
                                }
                            }
                            //瞬时报警  恢复时间《30秒的位号
                            var alarmEventListByTagName_1 = alarmEventInfoQue.Where(a => a.AEType == "0" && a.CondLastActive != null && a.CondLastInActive != null).OrderBy(a => a.AEActiveTime).GroupBy(a => a.AETagName).ToList();
                            foreach (var item_0 in alarmEventListByTagName_1)
                            {
                                var itemList = item_0.ToList();
                                int count_item = itemList.Count;
                                for (int i = 0; i < count_item; i++)
                                {
                                    TimeSpan timeSpan = (TimeSpan)(itemList[i].CondLastInActive - itemList[i].CondLastActive);
                                    if (timeSpan.TotalSeconds < 30 && timeSpan.TotalSeconds > 0)
                                    {
                                        //一分钟出现三次 则表示抖动
                                        dayStatisticInfo.FleetingAlarmCount = dayStatisticInfo.FleetingAlarmCount + 1;
                                        break;
                                    }
                                }
                            }
                            if (dayStatisticInfo != null)
                            {
                                _dayStatisticsInfoManage.addDayStatisticsInfo(dayStatisticInfo);
                            }
                        }
                    }
                }
            }
        }
    }
}
