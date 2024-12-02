using System;
namespace OPCAE.OPCAE
{
    public class OPCConditionState
    {
        public short State;
        public string ActiveSubCondition;
        public string ASCDefinition;
        public int ASCSeverity;
        public string ASCDescription;
        public short Quality;
        public DateTime LastAckTime;
        public DateTime SubCondLastActive;
        public DateTime CondLastActive;
        public DateTime CondLastInactive;
        public string AcknowledgerID;
        public string Comment;
        public int NumSCs;
        public string[] SCNames;
        public string[] SCDefinitions;
        public int[] SCSeverities;
        public string[] SCDescriptions;
        public int NumEventAttrs;
        public object[] EventAttributes;
        public int[] Errors;
    }
}

