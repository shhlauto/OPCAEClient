using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace HLCommon
{
    public class PhysicalInfo
    {
        public Boolean IsEnable(string filePath,string encryptPath)
        {
            //WriteLog("IsEnable()：");
            Boolean flag = false;
            string strCpuID = "";
            try
            {
                strCpuID = GetProcessorId();
                if (!Directory.Exists("D:\\CAMP\\PEWinService\\Files\\")) Directory.CreateDirectory("D:\\CAMP\\PEWinService\\Files\\");
                System.IO.File.WriteAllText(filePath, strCpuID, Encoding.UTF8);

                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] palindata = Encoding.Default.GetBytes(strCpuID);//将要加密的字符串转换为字节数组
                byte[] encryptdata = md5.ComputeHash(palindata);//将字符串加密后也转换为字符数组
                string original1 = Convert.ToBase64String(encryptdata);
                string original2 = original1 + "HL123456";

                SHA512Managed sha512 = new SHA512Managed();
                byte[] inputBytes = Encoding.UTF8.GetBytes(original2);
                byte[] outputBytes = sha512.ComputeHash(inputBytes);
                string ciphertext = Convert.ToBase64String(outputBytes);
                //如果日志文件不存在，创建一个
                if (!File.Exists(encryptPath))
                {
                    using (FileStream fs = File.Create(encryptPath));
                    flag = false;
                }
                else
                {
                    ///读取加密信息
                    StreamReader sr = new StreamReader(encryptPath, System.Text.Encoding.GetEncoding("gb2312"));
                    String line;
                    string original = "";
                    while ((line = sr.ReadLine()) != null)
                    {
                        original = line.ToString();
                    }
                    if (original == ciphertext)
                    {
                        flag = true;
                    }
                    sr.Close();
                    sr.Dispose();
                }
            }
            catch (Exception e)
            {
                //WriteLog("IsEnable():e=" + e.ToString());
            }
            return flag;
        }
        private string GetProcessorId()
        {
            using (var searcher = new ManagementObjectSearcher("SELECT ProcessorId FROM Win32_Processor"))
            {
                foreach (var obj in searcher.Get())
                {
                    return obj["ProcessorId"].ToString();
                }
            }
            return "Unable to retrieve ProcessorId";
        }
    }
}
